using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // App, Menu shared metadata and Feature suspension are separate owners.
        // These fields are process-local; recovery continues to use the existing
        // round/receipt journals and does not restore an old timer epoch.
        private bool pcGameAppSuspended;
        private bool pcGameScreensVisible = true;
        private bool pcGameResumePurgingInputs;
        private bool pcGameFirstResumeNotification = true;
        private int pcGameSuspendChanges, pcGameResumeChanges;
        private long pcWheelTimerEpoch;
        private long pcWheelTimerGeneration;
        private int pcWheelTimerConsumed, pcWheelDeliveredFrames;
        private bool pcWheelTimerPaused;
        private OriginalMenuSuspendController pcMenuSuspend;
        private int pcMenuState;
        private bool pcMenuWaitConfig, pcMenuTransitioning, pcMenuDiagnostics;
        private bool pcMenuOwnerReady = true, pcMenuSuppressFeatures;
        private bool pcMenuDiagnosticsReturn, pcMenuSharedSuspended;
        private readonly HashSet<AudioSource> pcPausedSounds = new HashSet<AudioSource>();

        private bool PcGameAppSuspended { get { return pcGameAppSuspended; } }
        private bool PcGameScreensVisible { get { return pcGameScreensVisible; } }
        private bool PcGameOrdinaryInputBlocked
        { get { return pcGameAppSuspended || pcGameResumePurgingInputs || sasStartupRuntimePending&&!PcSasBaseOfferInputAllowedV4&&!sasIdleRuntimeDispatchV4; } }
        private bool PcServiceKeepAliveReady
        { get { return pcGameAppSuspended && pcAutoCompleteGameSelected &&
            !pcGameResumePurgingInputs && !pcAutoPlayStorageFaulted && ideckServiceButton != null; } }

        // Local effects are observable, but never assert an external ACK.
        public event Action<string, string> PcGameSuspensionEffect;

        public void RequestPcGameSuspend(bool transitionByte9, string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.GameSuspend, transitionByte9, operationId); }
        public void RequestPcDirectGameSuspend(string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.DirectGameSuspend, false, operationId); }
        public void RequestPcGameResume(string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.GameResume, false, operationId); }
        public void RequestPcMenuSuspend(bool transitionByte9, string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.MenuSuspend, transitionByte9, operationId); }
        public void RequestPcMenuResume(string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.MenuResume, false, operationId); }
        public void RequestPcMenuGoToGameCompleted(string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.MenuGoToGameCompleted, false, operationId); }
        public void RequestPcMenuConfigCompleted(string operationId)
        { QueuePcGameSuspension(PcAutoCompleteControlKind.MenuConfigCompleted, false, operationId); }

        public void SetPcMenuLifecycleContext(int state, bool waitConfig, bool transitioning,
            bool ready, bool diagnostics, bool suppressFeatures)
        {
            RequirePcAutoCompleteOwnerThread();
            pcMenuState = state; pcMenuWaitConfig = waitConfig;
            pcMenuTransitioning = transitioning; pcMenuOwnerReady = ready;
            pcMenuDiagnostics = diagnostics; pcMenuSuppressFeatures = suppressFeatures;
        }

        private void QueuePcGameSuspension(PcAutoCompleteControlKind kind, bool byte9, string operationId)
        { QueuePcAutoCompleteControl(kind, OriginalAutoCompleteRequest.Zero, operationId, byte9); }

        private void EnsurePcMenuSuspendController()
        {
            if (pcMenuSuspend == null)
                pcMenuSuspend = new OriginalMenuSuspendController(new PcMenuSuspendHost(this));
        }

        private void ExecutePcGameSuspension(PcAutoCompleteControl command)
        {
            EnsurePcMenuSuspendController();
            switch (command.Kind)
            {
                case PcAutoCompleteControlKind.GameSuspend:
                    ReceivePcGameSuspend(true, command.TransitionByte9, false, command.OperationId); break;
                case PcAutoCompleteControlKind.DirectGameSuspend:
                    ReceivePcGameSuspend(true, false, true, command.OperationId); break;
                case PcAutoCompleteControlKind.GameResume:
                    ReceivePcGameSuspend(false, false, false, command.OperationId); break;
                case PcAutoCompleteControlKind.MenuSuspend:
                    // The original transition sender explicitly writes byte9=1.
                    pcMenuSuspend.HandleSuspendGameRequest(pcMenuTransitioning ||
                        command.TransitionByte9, command.OperationId); break;
                case PcAutoCompleteControlKind.MenuResume:
                    var disposition = pcMenuSuspend.HandleResumeGameRequest(command.OperationId);
                    GameSuspendEffect("MenuResume:" + disposition, command.OperationId); break;
                case PcAutoCompleteControlKind.MenuGoToGameCompleted:
                    pcMenuSuspend.HandleGoToGameTransitionCompleted(command.OperationId); break;
                case PcAutoCompleteControlKind.MenuConfigCompleted:
                    pcMenuSuspend.HandleGameConfigChangingCompletedTail(command.OperationId); break;
                default: throw new InvalidOperationException("Unknown local suspension command");
            }
        }

        private void GameSuspendEffect(string effect, string operationId)
        {
            Debug.Log("ROULETTE_GAME_SUSPEND_LOCAL: effect=" + effect + " operation=" + operationId +
                " app=" + pcGameAppSuspended + " screens=" + pcGameScreensVisible +
                " activeAuto=" + autoplay + " configuredAuto=" + configuredAutoPlay.AutoPlay +
                " wheelFrames=" + pcWheelDeliveredFrames + " pending=" +
                (pcMenuSuspend != null && pcMenuSuspend.PendingResume));
            var observer = PcGameSuspensionEffect;
            if (observer != null) observer(effect, operationId);
        }

        private void SetPcGameScreensVisible(bool visible, string operationId)
        {
            pcGameScreensVisible = visible;
            // ShowGameScreens can restart the footer through the source's
            // selected/history/spin/award gates. Keep the movie's repeat guard.
            if (visible && pcAutoCompleteGameSelected && !historyView && !spinning &&
                session != null && !session.InDoubleUp &&
                (currentGameResultsCleared || session.LastResult == null || session.LastResult.Returned == 0))
                StartFooterMovie();
            GameSuspendEffect(visible ? "ShowScreens" : "HideScreens", operationId);
        }

        private void ReceivePcGameSuspend(bool suspended, bool byte9, bool direct, string operationId)
        {
            RequirePcAutoCompleteOwnerThread();
            GameSuspendEffect(suspended ? "GameSuspendReceived" : "GameResumeReceived", operationId);
            if (suspended)
            {
                if (direct)
                { SetPcGameScreensVisible(true, operationId); SetPcGameScreensVisible(false, operationId); }
                else if (!byte9)
                { SetPcGameScreensVisible(false, operationId); SetPcGameScreensVisible(true, operationId); }
                pcWheelTimerPaused = true;
                pcWheelTimerConsumed = 0; // SetPeriod(0,0) discards old pending.
                ++pcWheelTimerGeneration;
                sourceTableInputEnabled = false;
                sourcePromptStopped = true;
                promptPhase = PromptPlaybackPhase.None;
                shownPrompt = PromptKind.None;
                SuspendPcPaysFlash(true);
                PausePcGameSounds();
                pcMenuSharedSuspended = true;
                SetPcGameAppSuspended(true, operationId);
                // This is active Auto only; no configured-Auto setter here.
                autoplay = false;
            }
            else
            {
                // Local PC WaitToResume has no physical/hybrid/reel devices.
                // It does not imply ShowGameScreens, or restore active Auto.
                if (!gameOverMessageLatched && spinning)
                {
                    pcWheelTimerEpoch = Stopwatch.GetTimestamp();
                    pcWheelTimerConsumed = 0;
                    pcWheelTimerPaused = false;
                    ++pcWheelTimerGeneration;
                }
                sourceTableInputEnabled = gameOverMessageLatched && SourceCreditMeterNonzero;
                ResumePcGamePrompt();
                SuspendPcPaysFlash(false);
                ResumePcGameSounds();
                pcMenuSharedSuspended = false;
                if (pcGameFirstResumeNotification)
                {
                    GameSuspendEffect("MenuSuspendChanged:false:LocalSend", operationId);
                    pcGameFirstResumeNotification = false;
                }
                SetPcGameAppSuspended(false, operationId);
            }
            RefreshPcSuspendedInputPresentation();
            GameSuspendEffect(suspended ? "GameSuspendExecuted" : "GameResumeExecuted", operationId);
        }

        private void SetPcGameAppSuspended(bool suspended, string operationId)
        {
            if (pcGameAppSuspended == suspended) return;
            pcGameAppSuspended = suspended;
            if (suspended) ++pcGameSuspendChanges;
            else ++pcGameResumeChanges;
            SuspendPcResultCycle(suspended);
            if (!suspended)
            {
                pcGameResumePurgingInputs = true;
                try { PurgeSourcePlayerInputs(); }
                finally { pcGameResumePurgingInputs = false; }
            }
            GameSuspendEffect(suspended ? "AppSuspending:true" : "AppSuspending:false", operationId);
        }

        private void ResumePcGamePrompt()
        {
            // Play(null) restarts the first eligible Begin, retaining the
            // enabled list and the separately armed delayed-play receipt.
            long now = Stopwatch.GetTimestamp();
            sourcePromptStopped = false;
            promptPhase = PromptPlaybackPhase.None;
            shownPrompt = PromptKind.None;
            promptStartNow = PromptKind.None;
            promptStartAfterFailure = PromptKind.None;
            promptReadyTimestamp = now;
            if (failurePrompt != PromptKind.None) failurePromptStartedTimestamp = now;
        }

        private void RefreshPcSuspendedInputPresentation()
        {
            SetPcCanvasInput(portraitCanvas);
            SetPcCanvasInput(ideckCanvas);
            SetPcCanvasInput(topperCanvas);
            // The configured keep-alive deck retains btnService. Raw events
            // are still only dispatched by an actual pending WaitInput; this
            // does not service the Video queue from a suspended RunLoop.
            if (ideckServiceButton != null)
            {
                CanvasGroup service = ideckServiceButton.GetComponent<CanvasGroup>();
                if (service == null) service = ideckServiceButton.gameObject.AddComponent<CanvasGroup>();
                service.ignoreParentGroups = PcServiceKeepAliveReady;
                service.interactable = !PcGameOrdinaryInputBlocked || PcServiceKeepAliveReady;
                service.blocksRaycasts = true;
            }
        }

        private void SetPcCanvasInput(Canvas canvas)
        {
            if (canvas == null) return;
            CanvasGroup group = canvas.GetComponent<CanvasGroup>();
            if (group == null) group = canvas.gameObject.AddComponent<CanvasGroup>();
            group.interactable = !PcGameOrdinaryInputBlocked;
            group.blocksRaycasts = !PcGameOrdinaryInputBlocked;
        }

        private void PausePcGameSounds()
        {
            foreach (AudioSource sound in GetComponents<AudioSource>())
                if (sound != null && sound.isPlaying)
                { sound.Pause(); pcPausedSounds.Add(sound); }
        }

        private void ResumePcGameSounds()
        {
            // UnPause resumes a paused instance, and cannot restart one that
            // another owner stopped or replaced while this set was retained.
            foreach (AudioSource sound in pcPausedSounds)
                if (sound != null) sound.UnPause();
            pcPausedSounds.Clear();
        }

        private sealed class PcMenuSuspendHost : IOriginalMenuSuspendHost
        {
            private readonly RouletteGame g;
            internal PcMenuSuspendHost(RouletteGame game) { g = game; }
            public bool HasOrdinarySelectedGameClient { get { return g.pcAutoCompleteGameSelected; } }
            public bool ForceMenuResumeTarget { get { return false; } }
            public bool DiagnosticsRequested { get { return g.pcMenuDiagnostics; } }
            public int MenuState { get { return g.pcMenuState; } }
            public bool WaitForConfigGameResponse { get { return g.pcMenuWaitConfig; } }
            public bool TransitioningToSelectedGame { get { return g.pcMenuTransitioning; } }
            public bool OwnerReady { get { return g.pcMenuOwnerReady; } }
            public bool SuppressSuspendAllFeatures { get { return g.pcMenuSuppressFeatures; } }
            public void SetMenuState(int state) { g.pcMenuState = state; }
            public void SetDiagnosticsReturnFlag(bool value) { g.pcMenuDiagnosticsReturn = value; }
            public void SetTransitioningToSelectedGame(bool value) { g.pcMenuTransitioning = value; }
            public void SendGameSuspendRequest(bool value, bool byte9, string id)
            {
                g.GameSuspendEffect(value ? "GameSuspend:LocalSend" : "GameResume:LocalSend", id);
                g.ReceivePcGameSuspend(value, byte9, false, id);
            }
            public void SendFeatureNotify(uint value, string id)
            { g.GameSuspendEffect("FeatureNotify:LocalSend:" + value, id); }
            public void SuspendAllFeatures(string id)
            { g.SuspendPcAutoCompleteFeatures(); g.GameSuspendEffect("FeaturesSuspended:LocalExecute", id); }
            public void ResumeAllFeatures(string id)
            { g.ResumePcAutoCompleteFeatures(); g.GameSuspendEffect("FeaturesResumed:LocalExecute", id); }
            public void PauseAllSounds() { g.PausePcGameSounds(); }
            public void ResumeAllSounds() { g.ResumePcGameSounds(); }
            public void PauseAllMovies() { g.PausePcGameMovies(); }
            public void ResumeAllMovies() { g.ResumePcGameMovies(); }
            public void SetSharedGameSuspended(bool value) { g.pcMenuSharedSuspended = value; }
            public void SuspendMultiGameMenu()
            { g.ConfirmAutoPlay(false); g.ConfirmPcService(false); g.ConfirmPcCollect(false); }
            public void ResumeMultiGameMenu() { }
        }
    }
}
