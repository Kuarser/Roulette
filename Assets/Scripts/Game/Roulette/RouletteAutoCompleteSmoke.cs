using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class AutoCompleteSmokeEvent
        {
            public string point;
            public long timestamp;
            public long featureDue, featureGeneration, featureArmedAt, autoDue, autoArmedAt;
            public long autoGeneration, autoOrdinal;
            public int featureOpcode, featureDelay, autoDelay;
            public int frame, auto, live, saved, completion;
            public int sourceState, sourcePhase;
            public bool rackActive, baseOffer;
            public uint start, warning, elapsed, flash;
            public bool timer, running, disable, off, active;
            public bool bonus, suspended;
            public long credit, returned, stake;
        }
        [Serializable]
        private sealed class AutoCompleteSmokeNotification
        {
            public string kind, origin, operationId;
            public uint reason, start, warning;
            public bool requested, showCountdown;
            public long timestamp;
        }
        [Serializable]
        private sealed class AutoCompleteSmokeStageCapture
        {
            public string label, path, sha256, view;
            public long timestamp, featureDue;
            public int frame, live, saved, featureOpcode;
        }
        [Serializable]
        private sealed class AutoCompleteSmokeProof
        {
            public int schema = 1;
            public string testCase, phase, checkpoint, exitMode, historyKey, rngPath, autoPath;
            public string previousProofSha256, rawJson, rawSha256, rngCandidate, featureJson, faultOperation;
            public RoundLifecycleSmokeRaw raw;
            public long credit, wagered, returned, rounds, stake, deliveries, frequency;
            public long observedAt;
            public int observedFrame;
            public int checks, ends, pockets, suits, auto, archiveSchema, live, saved, completion;
            public uint start, warning, elapsed, flash;
            public bool fault, inDouble, timer, timerRunning, disable, off, active, terminal;
            public bool suitMode, au;
            public AutoCompleteSmokeEvent[] events;
            public AutoCompleteSmokeNotification[] notifications;
            public AutoCompleteSmokeStageCapture[] stageCaptures;
        }

        private string acSmokeCase, acSmokePhase, acSmokePath, acSmokeCut, acSmokeFaultBoundary;
        private string acSmokePreviousHash = "";
        private AutoCompleteSmokeProof acSmokePrevious;
        private RoundLifecycleSmokeRaw acSmokeBoot;
        private readonly List<AutoCompleteSmokeEvent> acSmokeEvents = new List<AutoCompleteSmokeEvent>();
        private readonly List<PcAutoCompleteNotification> acSmokeNotifications = new List<PcAutoCompleteNotification>();
        private readonly List<AutoCompleteSmokeNotification> acSmokeNotificationProof = new List<AutoCompleteSmokeNotification>();
        private readonly List<AutoCompleteSmokeStageCapture> acSmokeStageCaptures = new List<AutoCompleteSmokeStageCapture>();
        private bool acSmokeConfigured, acSmokeStarted, acSmokeExiting, acSmokeFaultInjected;
        private int acSmokeChecks, acSmokePockets, acSmokeSuits;

        private void RequireAcSmoke(bool accepted, string message)
        {
            if (!accepted) throw new InvalidOperationException("AutoComplete smoke " + acSmokeCase + "/" + acSmokePhase + ": " + message);
            ++acSmokeChecks;
        }

        private void ConfigureAutoCompleteSmoke()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteAutoCompleteSmoke") < 0) return;
            acSmokeConfigured = true;
            acSmokeCase = RoundSmokeArgument("-rouletteAutoCompleteSmoke");
            acSmokePhase = RoundSmokeArgument("-rouletteAutoCompletePhase");
            RequireAcSmoke(new[] { "idle", "idle_late", "enable", "immediate", "ignored", "offer", "offer_info", "double_ready", "double_suspended",
                "double_win", "double_loss", "double_cap", "double_take", "double_manual",
                "receipt_win", "receipt_loss", "receipt_cap", "double_entrance_auto", "double_resuspend",
                "wheel_expiry", "bet_expiry", "rollup_expiry" }.Contains(acSmokeCase), "unknown case");
            RequireAcSmoke(new[] { "run", "restore", "reload" }.Contains(acSmokePhase), "unknown phase");
            RequireAcSmoke(!temporaryHistory && recordedHistoryKey.Contains(".Test.") &&
                RoundSmokeArgument("-rouletteVolumeTestKey").StartsWith("RouletteDeluxeSZ.Volume.Test.", StringComparison.Ordinal) &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires isolated durable history, RNG and volume");
            acSmokePath = RoundSmokeArgument("-rouletteAutoCompleteProof");
            RequireAcSmoke(Path.IsPathRooted(acSmokePath) && !File.Exists(acSmokePath), "proof must be a new absolute path");
            acSmokeCut = RoundSmokeArgument("-rouletteAutoCompleteCut", false) ?? "";
            acSmokeFaultBoundary = RoundSmokeArgument("-rouletteAutoCompleteFault", false) ?? "";
            string previous = RoundSmokeArgument("-rouletteAutoCompletePreviousProof", false);
            RequireAcSmoke((acSmokePhase == "run") == string.IsNullOrEmpty(previous), "wrong predecessor");
            acSmokeBoot = ReadRoundSmokeRaw();
            if (!string.IsNullOrEmpty(previous))
            {
                string json = File.ReadAllText(previous, Encoding.UTF8);
                acSmokePreviousHash = RoundSmokeSha(json);
                acSmokePrevious = JsonUtility.FromJson<AutoCompleteSmokeProof>(json);
                RequireAcSmoke(acSmokePrevious != null && acSmokePrevious.schema == 1 && acSmokePrevious.testCase == acSmokeCase &&
                    acSmokePrevious.historyKey == recordedHistoryKey && acSmokePrevious.rawJson == JsonUtility.ToJson(acSmokePrevious.raw) &&
                    acSmokePrevious.rawSha256 == RoundSmokeSha(acSmokePrevious.rawJson) &&
                    JsonUtility.ToJson(acSmokeBoot) == acSmokePrevious.rawJson && RngSmokeCandidate() == acSmokePrevious.rngCandidate,
                    "cold bytes differ from the closed predecessor proof");
            }
            pcAutoCompleteCheckpoint = AcSmokeCheckpoint;
            pcLifecycleCheckpoint = AcSmokeCheckpoint;
            pcLifecycleBeforeWrite = AcSmokeBeforeWrite;
            PcAutoCompleteNotified += notification => {
                acSmokeNotifications.Add(notification);
                acSmokeNotificationProof.Add(new AutoCompleteSmokeNotification {
                    kind = notification.Kind.ToString(), origin = notification.Origin.ToString(), operationId = notification.OperationId,
                    reason = notification.Data.Reason, start = notification.Data.StartSeconds, warning = notification.Data.WarningSeconds,
                    requested = notification.Data.Requested, showCountdown = notification.Data.ShowCountdown,
                    timestamp = System.Diagnostics.Stopwatch.GetTimestamp()
                });
                AcSmokeCheckpoint("Notification:" + notification.Kind);
            };
            Debug.Log("ROULETTE_AUTO_COMPLETE_BOOT: case=" + acSmokeCase + " phase=" + acSmokePhase +
                " raw=" + RoundSmokeSha(JsonUtility.ToJson(acSmokeBoot)) + " previous=" +
                (acSmokePreviousHash == "" ? "none" : acSmokePreviousHash));
        }

        private PcOutcomeSource CreateAutoCompleteSmokeOutcome()
        {
            if (!acSmokeConfigured) return CreateRngReceiptSmokeOutcome();
            uint[] snapshot = acSmokePrevious == null ? new OriginalPlatformRngCore(5489).ExportSnapshot() : null;
            return PcOutcomeSource.CreateReceiptDiagnostic(snapshot, PcRngStateStore.PathForCurrentProcess);
        }

        private void AttachAutoCompleteSmokeOutcome()
        {
            if (!acSmokeConfigured) return;
            pcOutcome.ConfigureReceiptDiagnostics((point, receipt) => {
                if (point != "saved") return;
                if (receipt.context.kind == 1) ++acSmokePockets; else ++acSmokeSuits;
                AcSmokeCheckpoint(receipt.context.kind == 1 ? "RngPocketSaved" : "RngDoubleSaved");
            });
        }

        private void AcSmokeBeforeWrite(string point)
        {
            if (acSmokeFaultInjected || acSmokeFaultBoundary == "" || point != acSmokeFaultBoundary) return;
            acSmokeFaultInjected = true;
            AcSmokeCheckpoint("Injected:" + point);
            throw new IOException("AutoComplete smoke injected before " + point);
        }

        private void AcSmokeCheckpoint(string point)
        {
            if (!acSmokeConfigured || acSmokeExiting) return;
            acSmokeEvents.Add(new AutoCompleteSmokeEvent {
                point = point, timestamp = System.Diagnostics.Stopwatch.GetTimestamp(), frame = Time.frameCount,
                featureDue = pcDoubleFeatureDue, featureGeneration = pcDoubleFeatureDueGeneration,
                featureArmedAt = pcDoubleFeatureArmedAt, featureOpcode = pcDoubleFeatureDueOpcode,
                featureDelay = pcDoubleFeatureDelay, autoDue = pcAutoCompleteNextTick,
                autoArmedAt = pcAutoCompleteArmedAt, autoDelay = pcAutoCompleteDelay,
                autoGeneration = checked((long)pcAutoCompleteIpcTimer.Generation),
                autoOrdinal = checked((long)pcAutoCompleteIpcTimer.NextExpirationOrdinal),
                sourceState = (int)sourceGameplayState, sourcePhase = (int)sourceRoundPhase,
                rackActive = winRackActive, baseOffer = sourceBaseHasDoubleOffer,
                auto = configuredAutoPlay.AutoPlay, start = configuredAutoPlay.SecsToAutoCompleteStart,
                warning = configuredAutoPlay.SecsToAutoCompleteWarning,
                elapsed = autoCompleteProcess.ElapsedMilliseconds, flash = autoCompleteProcess.FlashMilliseconds,
                timer = PcAutoCompleteTimerExists, running = pcAutoComplete != null && pcAutoComplete.TimerRunning,
                disable = configuredAutoPlay.DisableWhenGameFinished, off = configuredAutoPlay.AutoPlayOffWhenGameFinished,
                active = autoplay, live = pcDoubleFeature == null ? -1 : pcDoubleFeature.LiveState,
                bonus = pcBonusGamesEnabled, suspended = pcAutoCompleteAppSuspended,
                saved = pcDoubleFeatureNv == null ? -1 : pcDoubleFeatureNv.savedState,
                completion = pcDoubleFeatureNv == null ? -1 : pcDoubleFeatureNv.completionStage,
                credit = session == null ? -1 : session.Credit, returned = session == null ? -1 : session.Returned,
                stake = session == null ? -1 : session.DoubleStake
            });
            if (acSmokeCut != "" && point == acSmokeCut) AcSmokeExit(point, "checkpoint");
        }

        private void StartAutoCompleteSmoke()
        {
            if (!acSmokeConfigured) return;
            RequireAcSmoke(!acSmokeStarted, "observer started twice");
            acSmokeStarted = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            new GameObject("AutoComplete acceptance observer").AddComponent<RouletteRoundLifecycleSmokeHost>()
                .StartCoroutine(AutoCompleteSmoke());
        }

        private IEnumerator AcSmokeWait(Func<bool> condition, string label, float seconds = 90f, bool allowFault = false)
        {
            sourceInputModule.enabled = true;
            float until = Time.realtimeSinceStartup + seconds;
            while (!condition() && !pcAutoPlayStorageFaulted && Time.realtimeSinceStartup < until) yield return null;
            sourceInputModule.enabled = false;
            if (pcAutoPlayStorageFaulted && acSmokeFaultInjected && acSmokeFaultBoundary != "")
            {
                yield return AcSmokeObserveFault();
                // Application.Quit completes after this frame. Keep the
                // parent fixture from performing another action meanwhile.
                while (true) yield return null;
            }
            RequireAcSmoke(condition() && !pcAutoPlayStorageFaulted || allowFault && pcAutoPlayStorageFaulted, "wait failed: " + label);
        }

        private IEnumerator AcSmokeObserveMilliseconds(int milliseconds)
        {
            sourceInputModule.enabled = true;
            long until = OriginalRackClock.AfterMilliseconds(System.Diagnostics.Stopwatch.GetTimestamp(), milliseconds,
                System.Diagnostics.Stopwatch.Frequency);
            while (System.Diagnostics.Stopwatch.GetTimestamp() < until && !pcAutoPlayStorageFaulted) yield return null;
            sourceInputModule.enabled = false;
        }

        private void AcSmokeDisable(uint reason = 0x1000)
        {
            RequestPcDisable(new OriginalAutoCompleteRequest(reason, true, true, 1, 1),
                Guid.NewGuid().ToString("N") + ":disable");
        }

        private bool AcSmokeStalls { get { return acSmokeCase == "double_win" || acSmokeCase == "double_cap" || acSmokeCase == "double_take" ||
            acSmokeCase == "double_resuspend" ||
            acSmokePrevious != null && acSmokePrevious.saved == 5 && acSmokePrevious.auto == 1 && acSmokePrevious.completion == 0; } }

        private bool AcSmokeEarlyExpiry { get { return acSmokeCase == "wheel_expiry" ||
            acSmokeCase == "bet_expiry" || acSmokeCase == "rollup_expiry"; } }

        private bool AcSmokeIdleCase { get { return acSmokeCase == "idle" || acSmokeCase == "idle_late" || acSmokeCase == "enable" ||
            acSmokeCase == "ignored" || acSmokeCase == "immediate"; } }

        private IEnumerator AutoCompleteSmoke()
        {
            if (pcAutoPlayStorageFaulted) { yield return AcSmokeObserveFault(); yield break; }
            RequireAcSmoke(session != null && sourceInputModule != null && pcOutcome != null &&
                !temporaryHistory && !ideckSecondDisplay && !topperDedicatedDisplay, "missing real runtime");
            if (acSmokePrevious == null)
            {
                yield return AcSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                    !configuredAutoPlayCheckPending && !sourceBetStateEntryPending, "initial Idle");
                RequireAcSmoke(session.Credit == 1000 && session.Rounds == 0 && recordedRoundPages.Count == 0 &&
                    autoCompleteProcess.ElapsedMilliseconds == 0 && autoCompleteProcess.FlashMilliseconds == 0,
                    "fresh financial/process baseline differs");
                if (AcSmokeIdleCase)
                {
                    yield return AcSmokeIdleCases();
                    yield break;
                }
                if (AcSmokeEarlyExpiry) yield return AcSmokeSeedEarlyExpiry();
                else
                {
                yield return AcSmokeSeedOffer();
                if (acSmokeCase == "offer_info") { ToggleGameInfoOptions(); OpenPaysFromGameInfo(); RequireAcSmoke(paysPage >= 0, "information page did not open"); }
                if (acSmokeCase == "offer" || acSmokeCase == "offer_info") AcSmokeDisable();
                else
                {
                    if (acSmokeCase == "double_entrance_auto")
                    {
                        RequestPcDisable(new OriginalAutoCompleteRequest(0x1000, true, true, 29, 7), "entrance-disable");
                        yield return AcSmokeWait(() => acSmokeEvents.Any(e => e.point == "DisableHandled"), "entrance timer");
                    }
                    SourcePlayerInputs.Enqueue(0x200010, () => {
                        // Explicit configuration branch fixture. The real D
                        // callback begins the feature with configured Auto;
                        // no live/saved feature state is assigned here.
                        if (acSmokeCase == "double_entrance_auto")
                        {
                            configuredAutoPlay.SetAutoPlay(1);
                            configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
                        }
                        DispatchSourceButton(KeyCode.D);
                    });
                    SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
                    yield return AcSmokeWait(() => session.InDoubleUp && SourcePlayerInputs.Count == 0 &&
                        (acSmokeCase == "double_entrance_auto" ? pcDoubleFeature.LiveState == 1 : DoubleDecisionReady), "feature entry");
                    RequireAcSmoke(((RouletteSession)session).DoubleSettlementDeferred && pcDoubleFeatureNv != null &&
                        pcDoubleFeatureNv.savedState == 2 && session.Credit == 999 && session.DoubleStake == 36,
                        "native feature entry differs");
                    if (acSmokeCase == "double_entrance_auto")
                    {
                        RequireAcSmoke(pcDoubleFeature.LocalAuto && !autoplay && !configuredAutoPlay.Turbo,
                            "entrance fixture changed active Auto or Turbo");
                        yield return AcSmokeWait(() => acSmokeEvents.Any(e => e.point == "Feature:DeckFlashSavedLive"), "entrance auto DeckFlash");
                        RequireAcSmoke(pcDoubleFeature.LiveState == 2 && pcDoubleFeatureNv.savedState == 2 &&
                            PcNativeDoubleFeatureAutoTake && !pcDoubleFeature.LocalAuto && pcDoubleFeature.TimerOpcode == 0x1a &&
                            session.Credit == 999 && session.Returned == 36 && acSmokeSuits == 0,
                            "entrance Auto did not overwrite Take saved5 with live2");
                        yield return AcSmokeCaptureStage("auto_take");
                    }
                    else if (acSmokeCase == "double_resuspend")
                    {
                        AcSmokeDisable();
                        yield return AcSmokeWait(() => PcNativeDoubleFeatureAutoTake && pcDoubleFeature.LiveState == 2 &&
                            pcDoubleFeatureNv.savedState == 5, "ready automatic take");
                        long generation = pcDoubleFeature.TimerGeneration;
                        RequestPcFeatureSuspend("take-suspend");
                        yield return AcSmokeWait(() => pcDoubleFeature.Suspended, "take Feature suspend");
                        RequireAcSmoke(!PcNativeDoubleFeatureVisible && !pcDoubleFeature.TimerRunning, "take suspend retained timer/view");
                        yield return AcSmokeObserveMilliseconds(3300);
                        RequireAcSmoke(pcDoubleFeature.Suspended && session.Credit == 999 && session.Returned == 36 &&
                            sourceRoundEndCount == 0, "cancelled take timer completed while suspended");
                        RequestPcFeatureResume("take-resume");
                        yield return AcSmokeWait(() => !pcDoubleFeature.Suspended && pcDoubleFeature.TimerRunning, "take Feature resume");
                        RequireAcSmoke(pcDoubleFeature.LiveState == 5 && pcDoubleFeatureNv.savedState == 5 &&
                            pcDoubleFeature.TimerGeneration > generation && pcDoubleFeature.TimerOpcode == 0x1a,
                            "take resume did not reload saved5 with a new timer");
                    }
                    else if (acSmokeCase == "double_suspended")
                    {
                        SetPcAutoCompletePlatformContext(true, true, false);
                        RequestPcFeatureSuspend("feature-suspend");
                        yield return AcSmokeWait(() => pcDoubleFeature.Suspended, "platform Feature Suspend", 5);
                        RequireAcSmoke(!PcNativeDoubleFeatureVisible && !pcDoubleFeature.TimerRunning,
                            "suspended Feature retained presentation or a timer");
                        AcSmokeDisable();
                        yield return AcSmokeWait(() => configuredAutoPlay.AutoPlay == 1 && !pcAutoComplete.TimerRunning,
                            "auto expiry during platform suspension", 10);
                        RequireAcSmoke(pcDoubleFeature.Suspended && !pcDoubleFeature.LocalAuto &&
                            !pcDoubleFeature.TimerRunning && session.Credit == 999 && session.DoubleStake == 36,
                            "expiry resumed a suspended Feature");
                        yield return AcSmokeObserveMilliseconds(1000);
                        RequireAcSmoke(pcDoubleFeature.Suspended && session.Credit == 999 && sourceRoundEndCount == 0,
                            "suspended Feature advanced later");
                        SetPcAutoCompletePlatformContext(true, false, false);
                        RequestPcFeatureResume("feature-resume");
                    }
                    else if (acSmokeCase == "double_ready") AcSmokeDisable();
                    else if (acSmokeCase == "double_manual" || acSmokeCase == "double_take")
                    {
                        BeginCollectDouble();
                        if (acSmokeCase == "double_take") AcSmokeDisable();
                    }
                    else
                    {
                        RefreshPortraitUi();
                        bool loss = acSmokeCase.EndsWith("_loss", StringComparison.Ordinal);
                        QueueStartGameButton(session.DoubleSuitMode ? uiSuitChoiceButtons[loss ? 0 : 2] :
                            loss ? uiDoubleRedButton : uiDoubleBlackButton, loss ? 406 : 407, true);
                        yield return AcSmokeWait(() => session.DoubleRounds == 1, "committed suit", 10, acSmokeFaultBoundary != "");
                        if (pcAutoPlayStorageFaulted) { yield return AcSmokeObserveFault(); yield break; }
                        RequireAcSmoke(session.Credit == 999 && session.Returned == 36 && session.InDoubleUp &&
                            !recordedRoundPages[0].doubleUp.complete && pcDoubleFeatureNv.savedState == 3,
                            "suit decision paid before FeatureComplete");
                        RequireAcSmoke(pcOutcome.Receipt.rawResult == 0 && pcOutcome.Receipt.effectiveResult == 0 &&
                            (PcPaymentTracked ? ReadPcDoubleUnpaidContext(pcOutcome.Receipt) != null :
                                ReadPcDoubleDeferredContext(pcOutcome.Receipt) != null),
                            "native decision lost its versioned receipt or golden suit");
                        if (acSmokeCase == "receipt_cap") yield return AcSmokeCaptureStage("reveal_cap");
                        if (!acSmokeCase.StartsWith("receipt_", StringComparison.Ordinal)) AcSmokeDisable();
                    }
                }
                }
            }
            else
            {
                RequireAcSmoke(acSmokePockets == 0 && acSmokeSuits == 0 && gameStartTimestamp == 0 &&
                    ReadRoundSmokeRaw().rng == acSmokePrevious.raw.rng, "cold continuation drew or retained an old wall clock");
                if (AcSmokeIdleCase)
                {
                    if (configuredAutoPlay.SecsToAutoCompleteStart > 0)
                        yield return AcSmokeWait(() => !pcAutoComplete.TimerRunning, "cold countdown", 10);
                    RequireAcSmoke(!autoplay && session.Credit == 1000 && session.Rounds == 0, "cold idle timer changed money");
                    yield return AcSmokeCapture("idle");
                    AcSmokeExit("StableIdle", "natural");
                    yield break;
                }
            }
            if (acSmokeFaultBoundary != "")
            {
                yield return AcSmokeWait(() => pcAutoPlayStorageFaulted, "expected storage failure", 30, true);
                yield return AcSmokeObserveFault();
                yield break;
            }
            if (acSmokeCase == "receipt_win" && session.InDoubleUp && pcDoubleFeatureNv.completionStage == 0 &&
                (pcDoubleFeatureNv.savedState == 2 || pcDoubleFeatureNv.savedState == 3))
            {
                yield return AcSmokeWait(() => DoubleDecisionReady, "receipt win ready for real TAKE", 10);
                BeginCollectDouble();
            }
            if (AcSmokeStalls && (acSmokePrevious == null || acSmokePrevious.completion != 2))
            {
                yield return AcSmokeWait(() => pcDoubleFeature != null && !pcDoubleFeature.TimerRunning &&
                    pcDoubleFeature.Active && !pcDoubleFeature.DecisionReady && configuredAutoPlay.AutoPlay == 1,
                    "native unconsumed Auto take event", 15);
                string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
                RequireAcSmoke(session.Credit == 999 && session.Returned == 36 && session.InDoubleUp &&
                    !recordedRoundPages[0].doubleUp.complete && sourceRoundEndCount == 0 &&
                    pcDoubleFeatureNv.savedState == 5 && pcDoubleFeatureNv.completionStage == 0,
                    "native blocked state changed money or ended");
                yield return AcSmokeObserveMilliseconds(1000);
                RequireAcSmoke(!pcAutoPlayStorageFaulted && raw == JsonUtility.ToJson(ReadRoundSmokeRaw()) &&
                    session.Credit == 999 && sourceRoundEndCount == 0, "unconsumed event advanced later");
                yield return AcSmokeCapture("stalled");
                AcSmokeExit("NativeUnconsumed", "natural");
                yield break;
            }
            yield return AcSmokeWait(() => latestRoundLifecycle != null && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete && sourceRoundPhase == SourceRoundPhase.None &&
                !configuredAutoPlayCheckPending && !session.InDoubleUp && !ResultDisplayActive,
                "round completed through native feature and End", 90);
            long expected = acSmokeCase.EndsWith("_loss", StringComparison.Ordinal) ? 999 :
                acSmokeCase == "receipt_win" || acSmokeCase == "receipt_cap" ?
                    (session.DoubleSuitMode ? 1143 : 1071) : 1035;
            RequireAcSmoke(session.Credit == expected && session.Returned == expected - 999 && session.Wagered == 1 &&
                session.Rounds == 1 && session.Wins == 1 && !autoplay && configuredAutoPlay.AutoPlay == 0 &&
                !configuredAutoPlay.DisableWhenGameFinished && !configuredAutoPlay.AutoPlayOffWhenGameFinished,
                "completed financial/Auto state differs");
            if (acSmokeCase.StartsWith("double_", StringComparison.Ordinal) || acSmokeCase.StartsWith("receipt_", StringComparison.Ordinal))
                RequireAcSmoke(pcDoubleFeatureNv != null && pcDoubleFeatureNv.completionStage == 2 &&
                    pcDoubleFeatureNv.savedState == 0 && recordedRoundPages[0].doubleUp.complete,
                    "feature has no final Applied state0");
            if (acSmokePrevious == null && acSmokeCase != "double_manual" && !acSmokeCase.StartsWith("receipt_", StringComparison.Ordinal))
                RequireAcSmoke(acSmokeNotifications.Count(n => n.Kind == OriginalAutoCompleteNotification.AutoCompleteStart) ==
                    (acSmokeCase == "double_entrance_auto" ? 0 : 1) &&
                    acSmokeNotifications.Count(n => n.Kind == OriginalAutoCompleteNotification.DisableComplete) == 2 &&
                    acSmokeNotifications.Where(n => n.Kind == OriginalAutoCompleteNotification.DisableComplete)
                        .Select(n => n.OperationId).Distinct().Count() == 2,
                    "End did not emit the two distinct original cancellation notifications");
            if (AcSmokeEarlyExpiry)
                RequireAcSmoke(pcDoubleFeatureNv == null && acSmokeSuits == 0 &&
                    (acSmokeCase == "rollup_expiry") == (acSmokeEvents.Any(e => e.point == "OfferDeclined") ||
                        acSmokePrevious != null && acSmokePrevious.events.Any(p => p.point == "OfferDeclined")),
                    "early expiry followed the wrong Double qualification path");
            yield return AcSmokeCapture("complete");
            AcSmokeExit(acSmokePhase == "reload" ? "StableReload" : "Complete", "natural");
        }

        private IEnumerator AcSmokeIdleCases()
        {
            // Explicit platform fixture: native Idle with Bonus enabled and
            // nonzero credit must take the deferred Disable branch.
            if (acSmokeCase == "idle" || acSmokeCase == "idle_late" || acSmokeCase == "enable")
                SetPcAutoCompletePlatformContext(true, false, true);
            if (acSmokeCase == "idle_late") pcAutoCompleteTestHold = true;
            AcSmokeDisable(acSmokeCase == "ignored" ? 0x2000u : 0x1000u);
            yield return AcSmokeWait(() => acSmokeEvents.Any(e => e.point == "DisableHandled"), "disable dispatch", 10);
            SetPcAutoCompletePlatformContext(true, false, false);
            if (acSmokeCase == "ignored")
            {
                RequireAcSmoke(!PcAutoCompleteTimerExists && acSmokeNotifications.Count == 0 &&
                    configuredAutoPlay.SecsToAutoCompleteStart == 0, "unrecognized reason had effects");
            }
            else if (acSmokeCase == "immediate")
            {
                RequireAcSmoke(!PcAutoCompleteTimerExists && acSmokeNotifications.Count == 1 &&
                    acSmokeNotifications[0].Kind == OriginalAutoCompleteNotification.DisableComplete &&
                    acSmokeNotifications[0].Origin == OriginalAutoCompleteDataOrigin.OriginalRequest &&
                    configuredAutoPlay.SecsToAutoCompleteStart == 0,
                    "ordinary Idle did not complete Disable immediately");
            }
            else if (acSmokeCase == "enable")
            {
                RequireAcSmoke(PcAutoCompleteTimerExists && pcAutoComplete.TimerRunning, "enable fixture lacks its real timer");
                RequestPcEnable(new OriginalAutoCompleteRequest(0x1000, false, false, 0, 0), "enable-original");
                yield return AcSmokeWait(() => acSmokeEvents.Any(e => e.point == "EnableHandled"), "enable dispatch", 10);
                RequireAcSmoke(!PcAutoCompleteTimerExists && configuredAutoPlay.SecsToAutoCompleteStart == 0 &&
                    acSmokeNotifications.Count == 1 && acSmokeNotifications[0].Kind == OriginalAutoCompleteNotification.EnableComplete &&
                    acSmokeNotifications[0].Origin == OriginalAutoCompleteDataOrigin.OriginalRequest,
                    "enable cancel/notification differs");
            }
            else
            {
                long release = 0;
                if (acSmokeCase == "idle_late")
                {
                    yield return AcSmokeObserveMilliseconds(1300);
                    RequireAcSmoke(pcAutoCompleteDeliveries == 0 && configuredAutoPlay.SecsToAutoCompleteStart == 1,
                        "held service consumed a timer callback");
                    AcSmokeCheckpoint("TimerBacklogRelease");
                    release = acSmokeEvents[acSmokeEvents.Count - 1].timestamp;
                    pcAutoCompleteTestHold = false;
                }
                yield return AcSmokeWait(() => PcAutoCompleteTimerExists && !pcAutoComplete.TimerRunning, "idle expiry", 10);
                RequireAcSmoke(pcAutoCompleteDeliveries == 4 && configuredAutoPlay.SecsToAutoCompleteStart == 0 &&
                    configuredAutoPlay.SecsToAutoCompleteWarning == 0 && acSmokeNotifications.Count == 1 &&
                    acSmokeNotifications[0].Origin == OriginalAutoCompleteDataOrigin.ZeroCompletion &&
                    !pcAutoCompleteTimerPanelVisible && pcAutoCompleteTextTimerVisible,
                    "idle countdown/save/zero completion differs");
                if (acSmokeCase == "idle_late")
                {
                    AutoCompleteSmokeEvent[] delivered = acSmokeEvents.Where(e => e.point == "TimerDelivering").ToArray();
                    RequireAcSmoke(delivered.Length == 4 && delivered.All(e => e.autoDue <= release) &&
                        delivered.Select(e => e.frame).Distinct().Count() == 4,
                        "late service dropped queued expirations or delivered several in one frame");
                }
            }
            RequireAcSmoke(session.Credit == 1000 && session.Rounds == 0 && !autoplay && acSmokePockets == 0 && acSmokeSuits == 0,
                "idle control changed money or drew");
            yield return AcSmokeCapture("idle");
            AcSmokeExit("StableIdle", "natural");
        }

        private IEnumerator AcSmokeSeedOffer()
        {
            yield return AcSmokeSeedWager();
            QueueStartGameButton(uiSpinButton, 411, true);
            yield return AcSmokeWait(() => SourceDoubleOfferActive && !winRackActive && SourcePlayerInputs.Count == 0, "real Double offer");
            RequireAcSmoke(acSmokePockets == 1 && pcOutcome.Receipt.rawResult == 30 && pcOutcome.Receipt.effectiveResult == 27 &&
                session.Credit == (PcPaymentTracked ? 999 : 1035) && session.Returned == 36 &&
                session.Rounds == 1 && sourceRoundEndCount == 0, "seeded raw draw/Demo/base evaluation differs");
            if (PcPaymentTracked)
                RequireAcSmoke(pcGamePayment.owner == PcGamePaymentOwner.Base &&
                    pcGamePayment.baseOffer == PcGamePaymentOffer.Open && !pcGamePayment.prepared &&
                    !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                    ((RouletteSession)session).RoundPayoutPending && CommittedHistoryCount() == 0 &&
                    DisplayedCredit == 1035, "v3 unpaid offer lost its wallet, projection or history boundary");
        }

        private IEnumerator AcSmokeSeedWager()
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiChipSelections[0].GetComponent<UnityEngine.UI.Button>());
            RefreshPortraitUi();
            QueueStartGameButton(uiBetButtons[27], 410, true);
            yield return AcSmokeWait(() => session.TotalBet == 1 && sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "real straight wager");
            yield return StartGameChooseDemo(30);
            RefreshPortraitUi();
        }

        private IEnumerator AcSmokeSeedEarlyExpiry()
        {
            yield return AcSmokeSeedWager();
            if (acSmokeCase == "bet_expiry")
            {
                SetPcAutoCompletePlatformContext(true, false, true);
                AcSmokeDisable();
                yield return AcSmokeWait(() => acSmokeEvents.Any(e => e.point == "DisableHandled"), "Bet deferred disable");
                RequireAcSmoke(acSmokePockets == 0 && session.Credit == 1000 && session.TotalBet == 1,
                    "Bet expiry fixture already committed a spin");
                SetPcAutoCompletePlatformContext(true, false, false);
            }
            else
            {
                QueueStartGameButton(uiSpinButton, 411, true);
                if (acSmokeCase == "wheel_expiry")
                    yield return AcSmokeWait(() => spinning && sourceRoundPhase == SourceRoundPhase.Wheel && acSmokePockets == 1,
                        "committed wheel before expiry");
                else
                    yield return AcSmokeWait(() => sourceRoundPhase == SourceRoundPhase.BaseRollup && winRackActive && sourceBaseHasDoubleOffer,
                        "qualified base rollup before expiry");
                AcSmokeDisable();
            }
            yield return AcSmokeWait(() => acSmokeEvents.Any(e => e.point == "Notification:AutoCompleteStart"), "early expiry notification");
            AutoCompleteSmokeEvent atExpiry = acSmokeEvents.First(e => e.point == "Notification:AutoCompleteStart");
            RequireAcSmoke(acSmokeCase == "bet_expiry" ? atExpiry.sourceState == (int)SourceGameplayState.Bet && atExpiry.credit == 1000 :
                acSmokeCase == "wheel_expiry" ? atExpiry.sourcePhase == (int)SourceRoundPhase.Wheel :
                atExpiry.sourcePhase == (int)SourceRoundPhase.BaseRollup && atExpiry.rackActive && atExpiry.baseOffer,
                "timer expired in the wrong base-game phase");
        }

        private IEnumerator AcSmokeObserveFault()
        {
            RequireAcSmoke(acSmokeFaultBoundary != "" && acSmokeFaultInjected && pcAutoPlayStorageFaulted,
                "unexpected storage failure");
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
            long credit = session == null ? -1 : session.Credit;
            int draws = acSmokePockets + acSmokeSuits;
            for (int i = 0; i < 8; ++i) yield return null;
            RequireAcSmoke(raw == JsonUtility.ToJson(ReadRoundSmokeRaw()) &&
                (session == null ? -1 : session.Credit) == credit && draws == acSmokePockets + acSmokeSuits,
                "faulted runtime kept changing state");
            yield return AcSmokeCapture("fault");
            AcSmokeExit("ExpectedFault", "expected_fault");
        }

        private IEnumerator AcSmokeCapture(string label)
        {
            string path = RoundSmokeArgument("-rouletteAutoCompleteCapture", false);
            if (string.IsNullOrEmpty(path)) yield break;
            RequireAcSmoke(Path.IsPathRooted(path) && !File.Exists(path), "capture must be new and absolute");
            yield return null;
            ScreenCapture.CaptureScreenshot(path, 2);
            float until = Time.realtimeSinceStartup + 10;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < until) yield return null;
            RequireAcSmoke(File.Exists(path) && new FileInfo(path).Length > 0, "capture did not finish");
            Debug.Log("ROULETTE_AUTO_COMPLETE_CAPTURE: label=" + label + " path=" + path +
                " sha256=" + RngSmokeBytesSha(File.ReadAllBytes(path)));
        }

        private IEnumerator AcSmokeCaptureStage(string label)
        {
            string finalPath = RoundSmokeArgument("-rouletteAutoCompleteCapture", false);
            if (string.IsNullOrEmpty(finalPath)) yield break;
            string path = Path.Combine(Path.GetDirectoryName(finalPath), Path.GetFileNameWithoutExtension(finalPath) + "_" + label + ".png");
            RequireAcSmoke(Path.IsPathRooted(path) && !File.Exists(path) &&
                acSmokeStageCaptures.All(c => c.label != label), "stage capture must be new");
            yield return null;
            var proof = new AutoCompleteSmokeStageCapture {
                label = label, path = path, timestamp = System.Diagnostics.Stopwatch.GetTimestamp(), frame = Time.frameCount,
                view = pcDoubleFeatureView.ToString(), live = pcDoubleFeature.LiveState, saved = pcDoubleFeatureNv.savedState,
                featureOpcode = pcDoubleFeatureDueOpcode, featureDue = pcDoubleFeatureDue
            };
            ScreenCapture.CaptureScreenshot(path, 2);
            float until = Time.realtimeSinceStartup + 10;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < until) yield return null;
            RequireAcSmoke(File.Exists(path) && new FileInfo(path).Length > 0, "stage capture did not finish");
            proof.sha256 = RngSmokeBytesSha(File.ReadAllBytes(path));
            acSmokeStageCaptures.Add(proof);
            Debug.Log("ROULETTE_AUTO_COMPLETE_STAGE_CAPTURE: label=" + label + " path=" + path + " sha256=" + proof.sha256);
        }

        private void AcSmokeExit(string point, string mode)
        {
            RequireAcSmoke(!acSmokeExiting, "duplicate exit");
            acSmokeExiting = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var local = session as RouletteSession;
            var proof = new AutoCompleteSmokeProof {
                testCase = acSmokeCase, phase = acSmokePhase, checkpoint = point, exitMode = mode,
                historyKey = recordedHistoryKey, rngPath = PcRngStateStore.PathForCurrentProcess, autoPath = PcAutoPlayStatePath,
                previousProofSha256 = acSmokePreviousHash, raw = ReadRoundSmokeRaw(), rngCandidate = RngSmokeCandidate(),
                featureJson = pcDoubleFeatureNv == null ? "null" : JsonUtility.ToJson(pcDoubleFeatureNv),
                faultOperation = pcAutoPlayStorageFaultOperation ?? "", checks = acSmokeChecks, ends = sourceRoundEndCount,
                pockets = acSmokePockets, suits = acSmokeSuits, deliveries = pcAutoCompleteDeliveries,
                frequency = System.Diagnostics.Stopwatch.Frequency, archiveSchema = pcRoundArchiveSchemaVersion,
                observedAt = System.Diagnostics.Stopwatch.GetTimestamp(), observedFrame = Time.frameCount,
                auto = configuredAutoPlay.AutoPlay, start = configuredAutoPlay.SecsToAutoCompleteStart,
                warning = configuredAutoPlay.SecsToAutoCompleteWarning, elapsed = autoCompleteProcess.ElapsedMilliseconds,
                flash = autoCompleteProcess.FlashMilliseconds, disable = configuredAutoPlay.DisableWhenGameFinished,
                off = configuredAutoPlay.AutoPlayOffWhenGameFinished, active = autoplay, fault = pcAutoPlayStorageFaulted,
                timer = PcAutoCompleteTimerExists, timerRunning = pcAutoComplete != null && pcAutoComplete.TimerRunning,
                live = pcDoubleFeature == null ? -1 : pcDoubleFeature.LiveState,
                saved = pcDoubleFeatureNv == null ? -1 : pcDoubleFeatureNv.savedState,
                completion = pcDoubleFeatureNv == null ? -1 : pcDoubleFeatureNv.completionStage,
                credit = session == null ? -1 : session.Credit, wagered = session == null ? -1 : session.Wagered,
                returned = session == null ? -1 : session.Returned, rounds = session == null ? -1 : session.Rounds,
                stake = session == null ? -1 : session.DoubleStake, inDouble = session != null && session.InDoubleUp,
                terminal = local != null && local.DoubleChoiceTerminal, events = acSmokeEvents.ToArray(),
                stageCaptures = acSmokeStageCaptures.ToArray(),
                suitMode = session != null && session.DoubleSuitMode, au = auNationalStandardsEnabled,
                notifications = acSmokeNotificationProof.ToArray()
            };
            proof.rawJson = JsonUtility.ToJson(proof.raw);
            proof.rawSha256 = RoundSmokeSha(proof.rawJson);
            string encoded = JsonUtility.ToJson(proof, true) + "\n";
            Directory.CreateDirectory(Path.GetDirectoryName(acSmokePath));
            using (var stream = new FileStream(acSmokePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(encoded);
                stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
            }
            Debug.Log("ROULETTE_AUTO_COMPLETE_PASSED: case=" + acSmokeCase + " phase=" + acSmokePhase + " point=" + point +
                " mode=" + mode + " checks=" + acSmokeChecks + " credit=" + proof.credit + " rounds=" + proof.rounds +
                " ends=" + proof.ends + " pockets=" + proof.pockets + " suits=" + proof.suits +
                " fault=" + proof.fault.ToString().ToLowerInvariant() + " proof=" + RoundSmokeSha(encoded));
            Debug.Log("ROULETTE_AUTO_COMPLETE_EXIT: api=" + (mode == "checkpoint" ? "TerminateProcess" : "Application.Quit") +
                " code=0 mode=" + mode);
            if (mode == "checkpoint")
            {
                RequireAcSmoke(recordedHistoryKey.Contains(".Test."), "cut requires an isolated namespace");
                TerminateProcess(GetCurrentProcess(), 0);
                throw new InvalidOperationException("AutoComplete current-process cut returned");
            }
            Application.Quit(0);
        }
    }
}
