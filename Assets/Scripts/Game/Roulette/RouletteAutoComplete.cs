using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RouletteDeluxe
{
    // An optional renderer must describe actual source objects/resources. A
    // MessageBox object may exist while its empty background makes Show a no-op.
    public interface IPcAutoCompletePresentation
    {
        bool HasTimerPanel { get; }
        bool HasTextTimer { get; }
        bool HasMessageBox { get; }
        void SetTimerPanelVisible(bool visible);
        void SetTextTimerVisible(bool visible);
        void ShowMessageBox(string text, uint durationSeconds);
        void HideMessageBox();
    }

    public sealed class PcAutoCompleteNotification
    {
        public readonly OriginalAutoCompleteNotification Kind;
        public readonly OriginalAutoCompleteRequest Data;
        public readonly OriginalAutoCompleteDataOrigin Origin;
        public readonly string OperationId;

        public PcAutoCompleteNotification(OriginalAutoCompleteNotification kind,
            OriginalAutoCompleteRequest data, OriginalAutoCompleteDataOrigin origin,
            string operationId)
        {
            Kind = kind; Data = data; Origin = origin; OperationId = operationId;
        }
    }

    public sealed partial class RouletteGame
    {
        private static readonly OriginalAutoCompleteProcessState autoCompleteProcess =
            new OriginalAutoCompleteProcessState();
        private OriginalAutoCompleteController pcAutoComplete;
        private IPcAutoCompletePresentation pcAutoCompletePresentation;
        private bool pcAutoCompleteTimerPanelVisible;
        private bool pcAutoCompleteTextTimerVisible;
        private readonly Queue<PcAutoCompleteControl> pcAutoCompleteControls =
            new Queue<PcAutoCompleteControl>();
        private bool pcAutoCompleteBootstrapped;
        private bool pcAutoCompleteEndTimerResumePending;
        private bool pcAutoCompleteTestHold;
        private bool pcAutoCompleteAppSuspended;
        private bool pcBonusGamesEnabled;
        private bool pcAutoCompleteGameSelected = true;
        private int? pcAutoCompleteNativeOuterState;
        private bool? pcAutoCompletePurgeDemoOverride;
        private long pcAutoCompleteNextTick;
        private long pcAutoCompleteArmedAt;
        private int pcAutoCompleteDelay;
        private readonly OriginalPeriodicIpcTimer pcAutoCompleteIpcTimer = new OriginalPeriodicIpcTimer();
        private uint pcAutoCompletePeriod;
        private string pcAutoCompleteTimerOperationId;
        private int pcAutoCompleteServiceFrame = -1;
        private bool pcAutoCompleteServicing;
        private int pcAutoCompleteOwnerThread;
        private long pcAutoCompleteDeliveries;
        private Action<string> pcAutoCompleteCheckpoint;

        // This event is a local adapter call, never a remote acknowledgement.
        // The future platform owner must define delivery/retry/ack semantics.
        public event Action<PcAutoCompleteNotification> PcAutoCompleteNotified;

        private enum PcAutoCompleteControlKind { Disable, Enable, FeatureSuspend, FeatureResume,
            GameSuspend, DirectGameSuspend, GameResume, MenuSuspend, MenuResume,
            MenuGoToGameCompleted, MenuConfigCompleted }

        private sealed class PcAutoCompleteControl
        {
            internal PcAutoCompleteControlKind Kind;
            internal OriginalAutoCompleteRequest Request;
            internal string OperationId;
            internal bool TransitionByte9;
        }

        public void RequestPcDisable(OriginalAutoCompleteRequest request, string operationId)
        {
            QueuePcAutoCompleteControl(PcAutoCompleteControlKind.Disable, request, operationId);
        }

        public void RequestPcEnable(OriginalAutoCompleteRequest request, string operationId)
        {
            QueuePcAutoCompleteControl(PcAutoCompleteControlKind.Enable, request, operationId);
        }

        // Feature-server messages have their own entry points. A metadata
        // assignment does not stand in for Menu's whole suspension protocol.
        public void RequestPcFeatureSuspend(string operationId)
        {
            QueuePcAutoCompleteControl(PcAutoCompleteControlKind.FeatureSuspend,
                OriginalAutoCompleteRequest.Zero, operationId);
        }

        public void RequestPcFeatureResume(string operationId)
        {
            QueuePcAutoCompleteControl(PcAutoCompleteControlKind.FeatureResume,
                OriginalAutoCompleteRequest.Zero, operationId);
        }

        private void QueuePcAutoCompleteControl(PcAutoCompleteControlKind kind,
            OriginalAutoCompleteRequest request, string operationId, bool transitionByte9 = false)
        {
            if (string.IsNullOrWhiteSpace(operationId))
                throw new ArgumentException("A local control operation identity is required.", "operationId");
            if (pcAutoPlayStorageFaulted)
                throw new InvalidOperationException("Automatic play storage is faulted.");
            lock (pcAutoCompleteControls)
                pcAutoCompleteControls.Enqueue(new PcAutoCompleteControl {
                    Kind = kind, Request = request, OperationId = operationId,
                    TransitionByte9 = transitionByte9
                });
        }

        // Explicit adapter inputs. Cashout11 is supplied only by an owner that
        // knows the native outer state; a PC collect-confirmation flag does not
        // establish it. Bonus defaults false for this recovered package.
        public void SetPcAutoCompletePlatformContext(bool selected, bool suspended,
            bool bonusGamesEnabled, int? nativeOuterState = null)
        {
            RequirePcAutoCompleteOwnerThread();
            pcAutoCompleteGameSelected = selected;
            pcAutoCompleteAppSuspended = suspended;
            pcBonusGamesEnabled = bonusGamesEnabled;
            pcAutoCompleteNativeOuterState = nativeOuterState;
        }

        public void SetPcAutoCompletePresentation(IPcAutoCompletePresentation presentation)
        {
            RequirePcAutoCompleteOwnerThread();
            pcAutoCompletePresentation = presentation;
        }

        private void RequirePcAutoCompleteOwnerThread()
        {
            if (pcAutoCompleteOwnerThread == 0 ||
                System.Threading.Thread.CurrentThread.ManagedThreadId != pcAutoCompleteOwnerThread)
                throw new InvalidOperationException("Synchronous PC adapter configuration requires the Unity owner thread");
        }

        private bool PcAutoCompleteTimerExists
        {
            get { return pcAutoComplete != null && pcAutoComplete.TimerExists; }
        }

        private bool PcBonusGamesEnabled { get { return pcBonusGamesEnabled; } }

        private bool PcAutoCompletePlayPlatformAllowed
        {
            get { return pcAutoCompleteGameSelected && !pcAutoCompleteAppSuspended && !PcGameOrdinaryInputBlocked &&
                pcPlayerTransactionActive; }
        }

        private void EnsurePcAutoCompleteController()
        {
            if (pcAutoComplete != null) return;
            pcAutoComplete = new OriginalAutoCompleteController(configuredAutoPlay,
                new PcAutoCompleteHost(this), autoCompleteProcess);
            pcAutoCompleteTestHold = Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteAutoCompleteTestHold") >= 0;
        }

        private void PreparePcAutoCompleteStartup()
        {
            if (pcAutoPlayStorageFaulted) return;
            EnsurePcAutoCompleteController();
            // A saved EndPrepared decision owns its original timer presence.
            // V1 completes the old decision first; V2 restores its explicit
            // presence through the End executor. Neither gets a timer callback.
            if (HasTrustedLatestLifecycle &&
                latestRoundLifecycle.roundStage == PcRoundStage.EndPrepared) return;
            pcAutoComplete.RestoreFromSavedStart();
            pcAutoCompleteBootstrapped = true;
        }

        private void RestorePcAutoCompleteTimerPresenceForEnd(bool present)
        {
            EnsurePcAutoCompleteController();
            pcAutoComplete.RestoreTimerPresenceForEndReplay(present);
            pcAutoCompleteBootstrapped = true;
            pcAutoCompleteEndTimerResumePending = present;
            pcAutoCompleteIpcTimer.Stop();
            pcAutoCompleteNextTick = 0;
        }

        private void CancelPcAutoCompleteForEnd(bool complete, string operationId,
            bool demoAvailable)
        {
            EnsurePcAutoCompleteController();
            bool? prior = pcAutoCompletePurgeDemoOverride;
            pcAutoCompletePurgeDemoOverride = demoAvailable;
            try { pcAutoComplete.CancelAutoComplete(complete, operationId); }
            finally { pcAutoCompletePurgeDemoOverride = prior; }
        }

        private bool PcAutoCompleteRecoveryOwnsDispatch
        {
            get
            {
                if (!pcLifecycleRuntimeReady || session == null || pcLifecycleIdleDispatch) return true;
                // A live App wait still receives IPC. Ordinary pending work
                // cannot block the queued resume needed to service that work.
                if (!PcGameAppSuspended &&
                    (sourceBetStateEntryPending || configuredAutoPlayCheckPending)) return true;
                if (!HasTrustedLatestLifecycle) return false;
                return latestRoundLifecycle.roundStage == PcRoundStage.EndPrepared ||
                    (!PcGameAppSuspended && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                     latestRoundLifecycle.idleEntry.state != PcIdleState.Complete);
            }
        }

        internal void ServicePcAutoComplete()
        {
            if (pcAutoPlayStorageFaulted || pcAutoCompleteServicing ||
                pcAutoCompleteServiceFrame == Time.frameCount) return;
            // EventSystem and the non-uGUI fallback may both ask to service a
            // frame. Only the first safe dispatch can own this callback.
            if (PcAutoCompleteRecoveryOwnsDispatch) return;
            pcAutoCompleteServiceFrame = Time.frameCount;
            pcAutoCompleteServicing = true;
            try
            {
                EnsurePcAutoCompleteController();
                if (!pcAutoCompleteBootstrapped)
                {
                    pcAutoComplete.RestoreFromSavedStart();
                    pcAutoCompleteBootstrapped = true;
                }
                if (pcAutoCompleteEndTimerResumePending)
                {
                    pcAutoCompleteEndTimerResumePending = false;
                    if (pcAutoComplete.TimerExists && configuredAutoPlay.SecsToAutoCompleteStart != 0)
                        pcAutoComplete.RestoreFromSavedStart();
                }
                // One queued platform request per service, before a due timer.
                // The queue is separate from (and may purge) the player FIFO.
                PcAutoCompleteControl command;
                lock (pcAutoCompleteControls)
                    command = pcAutoCompleteControls.Count == 0 ? null : pcAutoCompleteControls.Dequeue();
                if (command != null)
                {
                    switch (command.Kind)
                    {
                        case PcAutoCompleteControlKind.Enable:
                            pcAutoComplete.HandleEnable(command.Request, command.OperationId); break;
                        case PcAutoCompleteControlKind.Disable:
                            pcAutoComplete.HandleDisable(command.Request, command.OperationId); break;
                        case PcAutoCompleteControlKind.FeatureSuspend: SuspendPcAutoCompleteFeatures(); break;
                        case PcAutoCompleteControlKind.FeatureResume: ResumePcAutoCompleteFeatures(); break;
                        default: ExecutePcGameSuspension(command); break;
                    }
                    AutoCompleteCheckpoint(command.Kind + "Handled");
                }
                long now = Stopwatch.GetTimestamp();
                if (!pcAutoCompleteTestHold && pcAutoComplete.TimerRunning &&
                    pcAutoCompleteNextTick != 0 && now >= pcAutoCompleteNextTick)
                {
                    // ipc.ko retains all periodic expirations while service
                    // is delayed. One IPC poll consumes one pending message;
                    // the next original deadline is retained, even if overdue.
                    AutoCompleteCheckpoint("TimerDelivering");
                    long consumedDue;
                    if (!pcAutoCompleteIpcTimer.TryReceive(now, out consumedDue) || consumedDue != pcAutoCompleteNextTick)
                        throw new InvalidOperationException("AutoComplete IPC timer deadline differs from its pending event");
                    pcAutoCompleteNextTick = pcAutoCompleteIpcTimer.NextExpirationTimestamp;
                    AutoCompleteCheckpoint("TimerPendingAdvanced");
                    if (pcAutoComplete.OnTimerDelivery(pcAutoCompleteTimerOperationId))
                    {
                        ++pcAutoCompleteDeliveries;
                        AutoCompleteCheckpoint("TimerDelivered");
                    }
                }
                ServicePcAutoCompleteOfferContinuation();
                ServicePcAutoCompleteFeatureContinuation();
            }
            catch (Exception error) { FailPcAutoPlayStorage("auto-complete", error); }
            finally { pcAutoCompleteServicing = false; }
        }

        private void AutoCompleteCheckpoint(string point)
        {
            if (pcAutoCompleteCheckpoint != null) pcAutoCompleteCheckpoint(point);
        }

        private void ServicePcAutoCompleteOfferContinuation()
        {
            // Native StateDoubleUp states 1 and 2 both poll configured Auto.
            // The rollup must finish before that decision loop is reached.
            if (PcGameAppSuspended || configuredAutoPlay.AutoPlay == 0 || sourceRoundPhase != SourceRoundPhase.DoubleOffer ||
                !SourceDoubleOfferActive) return;
            ClosePays();
            CloseGameInfoOptions();
            TakeSourceDoubleOffer();
            AutoCompleteCheckpoint("OfferDeclined");
        }

        private void SetPcAutoCompleteActiveAuto(bool active)
        {
            autoplay = active;
            if (active && autoRoutine == null) autoRoutine = StartCoroutine(AutoLoop());
        }

        private sealed class PcAutoCompleteHost : IOriginalAutoCompleteHost
        {
            private readonly RouletteGame game;
            internal PcAutoCompleteHost(RouletteGame game) { this.game = game; }
            public int NativeOuterState { get { return game.pcAutoCompleteNativeOuterState ??
                (int)game.sourceGameplayState; } }
            public bool BonusGamesEnabled { get { return game.PcBonusGamesEnabled; } }
            public bool CreditIsOrderedZero { get { return game.session != null && !game.SourceCreditMeterNonzero; } }
            public bool AutoPlayActive { get { return game.autoplay; } }
            public bool AppSuspended { get { return game.pcAutoCompleteAppSuspended || game.PcGameAppSuspended; } }
            public bool HasTimerPanel { get { return game.pcAutoCompletePresentation == null ||
                game.pcAutoCompletePresentation.HasTimerPanel; } }
            public bool HasTextTimer { get { return game.pcAutoCompletePresentation == null ||
                game.pcAutoCompletePresentation.HasTextTimer; } }
            public bool HasMessageBox { get { return game.pcAutoCompletePresentation == null ||
                game.pcAutoCompletePresentation.HasMessageBox; } }
            public bool HasPlaySprite { get { return game.uiSpinButton != null; } }

            public void CreateTimer(uint delayMilliseconds, uint periodMilliseconds)
            {
                game.pcAutoCompletePeriod = periodMilliseconds;
                game.pcAutoCompleteArmedAt = Stopwatch.GetTimestamp();
                game.pcAutoCompleteDelay = checked((int)delayMilliseconds);
                game.pcAutoCompleteIpcTimer.Arm(game.pcAutoCompleteArmedAt, delayMilliseconds,
                    periodMilliseconds, Stopwatch.Frequency);
                game.pcAutoCompleteNextTick = game.pcAutoCompleteIpcTimer.NextExpirationTimestamp;
                game.pcAutoCompleteTimerOperationId = Guid.NewGuid().ToString("N") + ":auto-complete-expiry";
                game.AutoCompleteCheckpoint("TimerCreated");
            }
            public void StopTimer()
            {
                game.pcAutoCompleteIpcTimer.Stop(); game.pcAutoCompleteNextTick = 0;
                game.AutoCompleteCheckpoint("TimerStopped");
            }
            public void DeleteTimer()
            {
                game.pcAutoCompleteIpcTimer.Delete(); game.pcAutoCompleteNextTick = 0;
                game.AutoCompleteCheckpoint("TimerDeleted");
            }
            public void PurgeInputs() { game.PurgeSourcePlayerInputs(demoAvailable: game.pcAutoCompletePurgeDemoOverride); }
            public void SetAutoPlayActive(bool active) { game.SetPcAutoCompleteActiveAuto(active); }
            public void SetTimerPanelVisible(bool visible)
            {
                game.pcAutoCompleteTimerPanelVisible = visible;
                if (game.pcAutoCompletePresentation != null) game.pcAutoCompletePresentation.SetTimerPanelVisible(visible);
            }
            public void SetTextTimerVisible(bool visible)
            {
                game.pcAutoCompleteTextTimerVisible = visible;
                if (game.pcAutoCompletePresentation != null) game.pcAutoCompletePresentation.SetTextTimerVisible(visible);
            }
            public void ShowMessageBox(string message, uint durationSeconds)
            {
                // The recovered package constructs a MessageBox even if its
                // configured background is empty. That object's Show returns
                // without rendering; no replacement overlay is invented here.
                if (game.pcAutoCompletePresentation != null)
                    game.pcAutoCompletePresentation.ShowMessageBox(message, durationSeconds);
                game.AutoCompleteCheckpoint("MessageShow:" + durationSeconds + ":" + message);
            }
            public void HideMessageBox()
            {
                if (game.pcAutoCompletePresentation != null) game.pcAutoCompletePresentation.HideMessageBox();
                game.AutoCompleteCheckpoint("MessageHide");
            }
            public void SetPlaySpriteEnabled(bool enabled)
            {
                game.uiSpinButton.interactable = enabled;
                // The ordinary Idle/Bet renderer recomputes it on its next
                // update, including selected/suspended/transaction conditions.
                game.AutoCompleteCheckpoint("PlayEnabled:" + enabled);
            }
            public void NotifyPlatform(OriginalAutoCompleteNotification notification,
                OriginalAutoCompleteRequest data, OriginalAutoCompleteDataOrigin origin,
                string operationId)
            {
                Debug.Log("ROULETTE_AUTO_COMPLETE_LOCAL_EVENT: kind=" + notification +
                    " origin=" + origin + " operation=" + operationId + " reason=" + data.Reason +
                    " start=" + data.StartSeconds + " warning=" + data.WarningSeconds);
                Action<PcAutoCompleteNotification> notify = game.PcAutoCompleteNotified;
                if (notify != null) notify(new PcAutoCompleteNotification(notification, data, origin, operationId));
                game.AutoCompleteCheckpoint("Notify:" + notification);
            }
            public void SuspendAllFeatures() { game.SuspendPcAutoCompleteFeatures(); }
            public void ResumeAllFeatures() { game.ResumePcAutoCompleteFeatures(); }
        }
    }
}
