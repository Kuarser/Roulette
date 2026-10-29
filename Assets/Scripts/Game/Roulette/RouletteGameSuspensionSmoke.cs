using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class GameSuspensionSmokeEvent
        {
            public string point, operationId, featureView, rng, collectId, consumedCollectId, moviePlayback;
            public long timestamp, wheelEpoch, wheelGeneration, cycleDue, rackStart, rackDue;
            public long featureDue, featureArmedAt, featureGeneration, autoDue, autoArmedAt;
            public long autoGeneration, autoOrdinal, credit, returned, stake, rounds, wagered, bet;
            public int frame, wheelFrames, wheelConsumed, wheelPath, wheelStage;
            public double wheelAngle;
            public Vector2 wheelBall;
            public int suspendChanges, resumeChanges, menuState, inputs, auto, live, saved, completion;
            public int featureOpcode, featureDelay, autoDelay, sourceState, sourcePhase;
            public int cycleIndex, cycleVisibleIndex, movieFrame, pausedMovies, pausedSounds, pays;
            public int footerPhase, paidFadeCallbacks;
            public float paidAlpha;
            public bool paidFadeRunning;
            public long cycleCallbacks, rackPaid, rackNextPaid, rackIncrement;
            public long moviePauseCount, movieResumeCount;
            public long paysTimeoutDue, paysFlashDue, paysFlashArmedAt, paysFlashGeneration, paysFlashCallbacks;
            public int paysFlashDelay, paysFlashState;
            public bool paysExists, paysReturning;
            public bool payoutPending, nativeFeatureActive;
            public long displayedCredit, displayedPaid, resultDisplayNominalEnd, configuredResultDue;
            public int resultWaitPhase, resultWaitInitial, resultWaitRemaining, resultWaitTimeouts, resultWaitInputs, resultWaitSkipped;
            public long resultWaitStarted, resultWaitDue, resultWaitHighStarted, resultWaitCompletedAt;
            public bool resultWaitComplete;
            public int resultInputProbeDispatches;
            public int retainedInputProbeDispatches;
            public bool serviceRequested, serviceConfirm;
            public int ownedPausedMovies;
            public int rackDelay, rackRemaining, rackPreviousInterval, ends, pockets, suits, collectPhase, idleState, idleStep;
            public uint start, warning, elapsed, flash;
            public bool app, screens, wheelPaused, pending, ready, transitioning, waitConfig, diagnostics, shared;
            public bool firstResume, active, disable, off, timer, timerRunning, featureSuspended, featureTimer;
            public bool cycleActive, cyclePaused, cycleVisible, rackActive, moviePlaying, moviePaused;
            public bool promptStopped, tableInput, inDouble, storageFault, collectPending;
        }

        [Serializable]
        private sealed class GameSuspensionSmokeNotification
        {
            public string kind, origin, operationId;
            public uint reason, start, warning;
            public bool requested, showCountdown;
            public long timestamp;
        }

        [Serializable]
        private sealed class GameSuspensionSmokeCapture
        {
            public string label, path, sha256;
            public GameSuspensionSmokeEvent state;
        }

        [Serializable]
        private sealed class GameSuspensionSmokeProof
        {
            public int schema = 5;
            public string testCase, phase, checkpoint, exitMode, historyKey, rngPath, autoPath;
            public string previousProofSha256, rawJson, rawSha256, rngCandidate, featureJson;
            public RoundLifecycleSmokeRaw raw;
            public long frequency;
            public int checks, pockets, suits, ends;
            public GameSuspensionSmokeEvent finalState;
            public GameSuspensionSmokeEvent[] events;
            public GameSuspensionSmokeNotification[] notifications;
            public GameSuspensionSmokeCapture[] stageCaptures;
        }

        private string gsSmokeCase, gsSmokePhase, gsSmokePath, gsSmokeCapturePath;
        private string gsSmokePreviousHash = "";
        private GameSuspensionSmokeProof gsSmokePrevious;
        private bool gsSmokeConfigured, gsSmokeStarted, gsSmokeExiting;
        private int gsSmokeChecks, gsSmokePockets, gsSmokeSuits;
        private int gsSmokeResultInputProbeDispatches;
        private int gsSmokeRetainedInputProbeDispatches;
        private readonly List<GameSuspensionSmokeEvent> gsSmokeEvents = new List<GameSuspensionSmokeEvent>();
        private readonly List<GameSuspensionSmokeNotification> gsSmokeNotifications = new List<GameSuspensionSmokeNotification>();
        private readonly List<GameSuspensionSmokeCapture> gsSmokeCaptures = new List<GameSuspensionSmokeCapture>();

        private void RequireGsSmoke(bool accepted, string message)
        {
            if (!accepted)
            {
                // A failed assertion must retain its actual operands. This
                // diagnostic is separate from a successful acceptance proof.
                try
                {
                    if (!string.IsNullOrEmpty(gsSmokePath))
                    {
                        string failurePath = gsSmokePath + ".failure-state.json";
                        if (!File.Exists(failurePath))
                            File.WriteAllText(failurePath,
                                JsonUtility.ToJson(GsSmokeSnapshot("Failure:" + message)),
                                new System.Text.UTF8Encoding(false));
                        Debug.Log("ROULETTE_GAME_SUSPENSION_FAILURE_STATE: path=" + failurePath);
                    }
                }
                catch (Exception diagnosticError)
                {
                    Debug.Log("ROULETTE_GAME_SUSPENSION_FAILURE_STATE_UNAVAILABLE: " + diagnosticError.Message);
                }
                throw new InvalidOperationException("Game suspension smoke " +
                    gsSmokeCase + "/" + gsSmokePhase + ": " + message);
            }
            ++gsSmokeChecks;
        }

        private bool GsSmokeCutCase { get { return gsSmokeCase.StartsWith("cut_", StringComparison.Ordinal); } }
        private bool GsSmokeNoRoundCase { get { return gsSmokeCase == "menu_gates" ||
            gsSmokeCase == "idle_autocomplete" || gsSmokeCase == "demo_purge" || gsSmokeCase == "movie_instances" || gsSmokeCase == "pays_timeout"; } }

        private void ConfigureGameSuspensionSmoke()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteGameSuspensionSmoke") < 0) return;
            gsSmokeConfigured = true;
            gsSmokeCase = RoundSmokeArgument("-rouletteGameSuspensionSmoke");
            gsSmokePhase = RoundSmokeArgument("-rouletteGameSuspensionPhase");
            RequireGsSmoke(new[] { "menu_gates", "movie_instances", "bet_inputs", "wheel_rpc0", "wheel_rpc1", "wheel_direct",
                "rollup_menu", "feature_ready_expiry", "feature_reveal", "feature_autotake_saved5",
                "wheel_autocomplete", "idle_autocomplete", "demo_purge", "cut_wheel_paused",
                "cut_feature_paused", "game_feature_independent", "pays_timeout", "game_payout_wait",
                "cut_game_feature_received", "game_payout_auto", "game_payout_inputs", "game_payout_service" }.Contains(gsSmokeCase), "unknown case");
            RequireGsSmoke(new[] { "run", "restore", "reload" }.Contains(gsSmokePhase) &&
                (gsSmokePhase != "restore" || GsSmokeCutCase), "unknown phase");
            RequireGsSmoke(!temporaryHistory && recordedHistoryKey.Contains(".Test.") &&
                RoundSmokeArgument("-rouletteVolumeTestKey").StartsWith("RouletteDeluxeSZ.Volume.Test.", StringComparison.Ordinal) &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires isolated durable history, RNG and volume");
            gsSmokePath = RoundSmokeArgument("-rouletteGameSuspensionProof");
            gsSmokeCapturePath = RoundSmokeArgument("-rouletteGameSuspensionCapture");
            RequireGsSmoke(Path.IsPathRooted(gsSmokePath) && !File.Exists(gsSmokePath) &&
                Path.IsPathRooted(gsSmokeCapturePath) && !File.Exists(gsSmokeCapturePath), "proof/capture must be new absolute paths");
            string cut = RoundSmokeArgument("-rouletteGameSuspensionCut", false) ?? "";
            RequireGsSmoke(cut == "" || GsSmokeCutCase && gsSmokePhase == "run" && cut == "Paused", "invalid cut point");
            string previous = RoundSmokeArgument("-rouletteGameSuspensionPreviousProof", false);
            RequireGsSmoke((gsSmokePhase == "run") == string.IsNullOrEmpty(previous), "wrong predecessor");
            RoundLifecycleSmokeRaw boot = ReadRoundSmokeRaw();
            if (!string.IsNullOrEmpty(previous))
            {
                string json = File.ReadAllText(previous, Encoding.UTF8);
                gsSmokePreviousHash = RoundSmokeSha(json);
                gsSmokePrevious = JsonUtility.FromJson<GameSuspensionSmokeProof>(json);
                RequireGsSmoke(gsSmokePrevious != null && gsSmokePrevious.schema == 5,
                    "closed predecessor proof must use schema 5");
                RequireGsSmoke(gsSmokePrevious.testCase == gsSmokeCase && gsSmokePrevious.historyKey == recordedHistoryKey &&
                    gsSmokePrevious.rawJson == JsonUtility.ToJson(gsSmokePrevious.raw) &&
                    gsSmokePrevious.rawSha256 == RoundSmokeSha(gsSmokePrevious.rawJson) &&
                    JsonUtility.ToJson(boot) == gsSmokePrevious.rawJson && RngSmokeCandidate() == gsSmokePrevious.rngCandidate,
                    "cold bytes differ from the closed predecessor proof");
                RequireGsSmoke(gsSmokePhase == "restore" ? gsSmokePrevious.phase == "run" &&
                    gsSmokePrevious.checkpoint == "Paused" && gsSmokePrevious.exitMode == "checkpoint" :
                    gsSmokePrevious.phase == (GsSmokeCutCase ? "restore" : "run") &&
                    gsSmokePrevious.exitMode == "natural", "predecessor phase/exit differs");
            }
            // This is observed before the existing recovery owner executes.
            // No pause state or timer epoch is installed by the fixture.
            RequireGsSmoke(!pcGameAppSuspended && pcGameScreensVisible && pcGameFirstResumeNotification &&
                pcGameSuspendChanges == 0 && pcGameResumeChanges == 0 && pcWheelTimerEpoch == 0 &&
                !pcWheelTimerPaused && pcPausedMovies.Count == 0 && pcPausedSounds.Count == 0 &&
                (pcMenuSuspend == null || !pcMenuSuspend.PendingResume), "process inherited transient suspension state");
            PcGameSuspensionEffect += GsSmokeEffect;
            pcAutoCompleteCheckpoint = point => GsSmokeCheckpoint("Auto:" + point);
            pcLifecycleCheckpoint = point => GsSmokeCheckpoint("Lifecycle:" + point);
            PcAutoCompleteNotified += notification => {
                gsSmokeNotifications.Add(new GameSuspensionSmokeNotification {
                    kind = notification.Kind.ToString(), origin = notification.Origin.ToString(),
                    operationId = notification.OperationId, reason = notification.Data.Reason,
                    start = notification.Data.StartSeconds, warning = notification.Data.WarningSeconds,
                    requested = notification.Data.Requested, showCountdown = notification.Data.ShowCountdown,
                    timestamp = Stopwatch.GetTimestamp()
                });
                GsSmokeCheckpoint("Notification:" + notification.Kind, notification.OperationId);
            };
            GsSmokeCheckpoint("FreshProcess");
            Debug.Log("ROULETTE_GAME_SUSPENSION_BOOT: case=" + gsSmokeCase + " phase=" + gsSmokePhase +
                " raw=" + RoundSmokeSha(JsonUtility.ToJson(boot)) + " previous=" +
                (gsSmokePreviousHash == "" ? "none" : gsSmokePreviousHash));
        }

        private PcOutcomeSource CreateGameSuspensionSmokeOutcome()
        {
            if (!gsSmokeConfigured) return CreateAutoCompleteSmokeOutcome();
            uint[] snapshot = gsSmokePrevious == null ? new OriginalPlatformRngCore(5489).ExportSnapshot() : null;
            return PcOutcomeSource.CreateReceiptDiagnostic(snapshot, PcRngStateStore.PathForCurrentProcess);
        }

        private void AttachGameSuspensionSmokeOutcome()
        {
            if (!gsSmokeConfigured) return;
            pcOutcome.ConfigureReceiptDiagnostics((point, receipt) => {
                if (point != "saved") return;
                if (receipt.context.kind == 1) ++gsSmokePockets; else ++gsSmokeSuits;
                GsSmokeCheckpoint(receipt.context.kind == 1 ? "RngPocketSaved" : "RngDoubleSaved");
            });
        }

        private GameSuspensionSmokeEvent GsSmokeSnapshot(string point, string operationId = "")
        {
            PcCreditEvent creditEvent = ReadPcCreditEvent();
            return new GameSuspensionSmokeEvent {
                point = point, operationId = operationId, timestamp = Stopwatch.GetTimestamp(), frame = Time.frameCount,
                app = pcGameAppSuspended, screens = pcGameScreensVisible, suspendChanges = pcGameSuspendChanges,
                resumeChanges = pcGameResumeChanges, firstResume = pcGameFirstResumeNotification,
                wheelEpoch = pcWheelTimerEpoch, wheelGeneration = pcWheelTimerGeneration,
                wheelFrames = pcWheelDeliveredFrames, wheelConsumed = pcWheelTimerConsumed,
                wheelPaused = pcWheelTimerPaused, wheelPath = wheelPathFrame, wheelStage = (int)wheelStage,
                wheelAngle = wheelRenderedAngle, wheelBall = wheelBallPosition,
                pending = pcMenuSuspend != null && pcMenuSuspend.PendingResume, menuState = pcMenuState,
                ready = pcMenuOwnerReady, transitioning = pcMenuTransitioning, waitConfig = pcMenuWaitConfig,
                diagnostics = pcMenuDiagnostics, shared = pcMenuSharedSuspended, inputs = SourcePlayerInputs.Count,
                active = autoplay, auto = configuredAutoPlay.AutoPlay, start = configuredAutoPlay.SecsToAutoCompleteStart,
                warning = configuredAutoPlay.SecsToAutoCompleteWarning, elapsed = autoCompleteProcess.ElapsedMilliseconds,
                flash = autoCompleteProcess.FlashMilliseconds, disable = configuredAutoPlay.DisableWhenGameFinished,
                off = configuredAutoPlay.AutoPlayOffWhenGameFinished, timer = PcAutoCompleteTimerExists,
                timerRunning = pcAutoComplete != null && pcAutoComplete.TimerRunning,
                autoDue = pcAutoCompleteNextTick, autoArmedAt = pcAutoCompleteArmedAt, autoDelay = pcAutoCompleteDelay,
                autoGeneration = checked((long)pcAutoCompleteIpcTimer.Generation),
                autoOrdinal = checked((long)pcAutoCompleteIpcTimer.NextExpirationOrdinal),
                live = pcDoubleFeature == null ? -1 : pcDoubleFeature.LiveState,
                saved = pcDoubleFeatureNv == null ? -1 : pcDoubleFeatureNv.savedState,
                completion = pcDoubleFeatureNv == null ? -1 : pcDoubleFeatureNv.completionStage,
                featureSuspended = pcDoubleFeature != null && pcDoubleFeature.Suspended,
                featureTimer = pcDoubleFeature != null && pcDoubleFeature.TimerRunning,
                featureDue = pcDoubleFeatureDue, featureArmedAt = pcDoubleFeatureArmedAt,
                featureGeneration = pcDoubleFeatureDueGeneration, featureOpcode = pcDoubleFeatureDueOpcode,
                featureDelay = pcDoubleFeatureDelay, featureView = pcDoubleFeatureView.ToString(),
                cycleActive = pcResultCycleClock != null && pcResultCycleClock.Active,
                cyclePaused = pcResultCycleClock != null && pcResultCycleClock.Paused,
                cycleVisible = pcResultCycleClock != null && pcResultCycleClock.Visible,
                cycleIndex = pcResultCycleClock == null ? -1 : pcResultCycleClock.Index,
                cycleVisibleIndex = pcResultCycleClock == null ? -1 : pcResultCycleClock.VisibleIndex,
                cycleCallbacks = pcResultCycleClock == null ? 0 : checked((long)pcResultCycleClock.CallbackCount),
                cycleDue = pcResultCycleClock == null ? 0 : pcResultCycleClock.NextExpirationTimestamp,
                rackActive = winRackActive, rackPaid = winRackPaid, rackNextPaid = winRackNextPaid,
                rackStart = winRackStartTimestamp, rackDue = winRackNextServiceTimestamp,
                rackIncrement = winRackIncrement, rackDelay = winRackDelayMilliseconds,
                rackRemaining = winRackPreviousRemainingMilliseconds, rackPreviousInterval = winRackPreviousIntervalMilliseconds,
                movieFrame = footerMovieClock == null ? -1 : footerMovieClock.Frame,
                footerPhase = (int)footerMoviePhase, paidAlpha = PaidOpacity,
                paidFadeRunning = paidFadeClock.Running, paidFadeCallbacks = paidFadeClock.CallbackCount,
                moviePlaying = footerMovieClock != null && footerMovieClock.Playing,
                moviePaused = footerMovieClock != null && footerMovieClock.Paused,
                pausedMovies = pcPausedMovies.Count, pausedSounds = pcPausedSounds.Count,
                moviePlayback = PcMoviePlaybackSnapshot, moviePauseCount = PcMoviePauseCount,
                movieResumeCount = PcMovieResumeCount, ownedPausedMovies = PcPausedMovieCount,
                promptStopped = sourcePromptStopped, tableInput = sourceTableInputEnabled, pays = paysPage,
                paysTimeoutDue = paysTimeoutDeadlineTimestamp,
                paysFlashDue = pcPaysFlash == null ? 0 : pcPaysFlash.NextExpirationTimestamp,
                paysFlashArmedAt = pcPaysFlash == null ? 0 : pcPaysFlash.ArmedAtTimestamp,
                paysFlashDelay = pcPaysFlash == null ? 0 : checked((int)pcPaysFlash.ArmedDelayMilliseconds),
                paysFlashGeneration = pcPaysFlash == null ? 0 : checked((long)pcPaysFlash.Generation),
                paysFlashCallbacks = pcPaysFlash == null ? 0 : checked((long)pcPaysFlash.CallbackCount),
                paysFlashState = pcPaysFlash == null ? 0 : pcPaysFlash.State,
                paysExists = pcPaysFlash != null && pcPaysFlash.Exists,
                paysReturning = pcPaysFlash != null && pcPaysFlash.Returning,
                payoutPending = PcDoubleGamePayoutPending,
                nativeFeatureActive = pcDoubleFeature != null && pcDoubleFeature.Active,
                displayedCredit = session == null ? -1 : DisplayedCredit,
                displayedPaid = session == null ? -1 : DisplayedPaid,
                resultDisplayNominalEnd = resultDisplayDeadlineTimestamp,
                configuredResultDue = configuredResultDelayDeadlineTimestamp,
                resultWaitPhase = pcResultDisplayWait == null ? -1 : (int)pcResultDisplayWait.Phase,
                resultWaitInitial = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.InitialWaits,
                resultWaitRemaining = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.RemainingWaits,
                resultWaitTimeouts = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.TimeoutCount,
                resultWaitInputs = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.InputCount,
                resultWaitSkipped = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.SkippedWaits,
                resultWaitStarted = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.WaitStartedAt,
                resultWaitDue = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.WaitDueTimestamp,
                resultWaitHighStarted = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.HighDelayStartedAt,
                resultWaitCompletedAt = pcResultDisplayWait == null ? 0 : pcResultDisplayWait.CompletedAt,
                resultWaitComplete = pcResultDisplayWait == null || pcResultDisplayWait.Completed,
                resultInputProbeDispatches = gsSmokeResultInputProbeDispatches,
                retainedInputProbeDispatches = gsSmokeRetainedInputProbeDispatches,
                serviceRequested = pcServiceRequested,
                serviceConfirm = serviceConfirmPending,
                sourceState = (int)sourceGameplayState, sourcePhase = (int)sourceRoundPhase,
                credit = session == null ? -1 : session.Credit, returned = session == null ? -1 : session.Returned,
                stake = session == null ? -1 : session.DoubleStake, rounds = session == null ? -1 : session.Rounds,
                wagered = session == null ? -1 : session.Wagered, bet = session == null ? -1 : session.TotalBet,
                inDouble = session != null && session.InDoubleUp, ends = sourceRoundEndCount,
                pockets = gsSmokePockets, suits = gsSmokeSuits, storageFault = pcAutoPlayStorageFaulted,
                collectPending = pcCollectPending, collectId = creditEvent == null ? "" : creditEvent.id,
                collectPhase = creditEvent == null ? 0 : creditEvent.phase,
                idleState = latestRoundLifecycle == null || latestRoundLifecycle.idleEntry == null ? -1 : (int)latestRoundLifecycle.idleEntry.state,
                idleStep = latestRoundLifecycle == null || latestRoundLifecycle.idleEntry == null ? -1 : (int)latestRoundLifecycle.idleEntry.step,
                consumedCollectId = latestRoundLifecycle == null || latestRoundLifecycle.idleEntry == null ? "" :
                    latestRoundLifecycle.idleEntry.consumedCollectEventId,
                rng = ReadRoundSmokeRaw().rng
            };
        }

        private void GsSmokeEffect(string point, string operationId) { GsSmokeCheckpoint(point, operationId); }
        private void GsSmokeCheckpoint(string point, string operationId = "")
        {
            if (gsSmokeConfigured && !gsSmokeExiting) gsSmokeEvents.Add(GsSmokeSnapshot(point, operationId));
        }
        private bool GsSmokeHas(string point, string id)
        { return gsSmokeEvents.Any(e => e.point == point && e.operationId == id); }
        private GameSuspensionSmokeEvent GsSmokeEvent(string point, string id)
        { return gsSmokeEvents.Last(e => e.point == point && e.operationId == id); }

        private void StartGameSuspensionSmoke()
        {
            if (!gsSmokeConfigured) return;
            RequireGsSmoke(!gsSmokeStarted, "observer started twice");
            gsSmokeStarted = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            new GameObject("Game suspension acceptance observer").AddComponent<RouletteRoundLifecycleSmokeHost>()
                .StartCoroutine(GameSuspensionSmoke());
        }

        private IEnumerator GsSmokeWait(Func<bool> condition, string label, float seconds = 90f)
        {
            sourceInputModule.enabled = true;
            float until = Time.realtimeSinceStartup + seconds;
            while (!condition() && !pcAutoPlayStorageFaulted && Time.realtimeSinceStartup < until) yield return null;
            sourceInputModule.enabled = false;
            RequireGsSmoke(condition() && !pcAutoPlayStorageFaulted, "wait failed: " + label);
        }

        private IEnumerator GsSmokeObserve(int milliseconds)
        {
            sourceInputModule.enabled = true;
            long until = OriginalRackClock.AfterMilliseconds(Stopwatch.GetTimestamp(), milliseconds, Stopwatch.Frequency);
            while (Stopwatch.GetTimestamp() < until && !pcAutoPlayStorageFaulted) yield return null;
            sourceInputModule.enabled = false;
            RequireGsSmoke(!pcAutoPlayStorageFaulted, "storage fault during observation");
        }

        private IEnumerator GsSmokeSuspend(string id, string route = "menu")
        {
            if (route == "direct") RequestPcDirectGameSuspend(id);
            else if (route == "rpc0" || route == "rpc1") RequestPcGameSuspend(route == "rpc1", id);
            else RequestPcMenuSuspend(false, id);
            yield return GsSmokeWait(() => GsSmokeHas("GameSuspendExecuted", id), id, 10);
            RequireGsSmoke(pcGameAppSuspended && !autoplay && !sourceTableInputEnabled && sourcePromptStopped,
                "full suspend did not block App/table/prompt or clear active Auto");
            string[] effects = gsSmokeEvents.Where(e => e.operationId == id).Select(e => e.point).ToArray();
            int hide = Array.IndexOf(effects, "HideScreens"), show = Array.IndexOf(effects, "ShowScreens");
            RequireGsSmoke(route == "rpc1" ? hide < 0 && show < 0 : route == "direct" ?
                show >= 0 && hide > show && !pcGameScreensVisible : hide >= 0 && show > hide && pcGameScreensVisible,
                "source-specific Hide/Show order differs");
            if (route == "menu")
                RequireGsSmoke(Array.IndexOf(effects, "GameSuspend:LocalSend") < Array.IndexOf(effects, "GameSuspendReceived") &&
                    Array.IndexOf(effects, "FeatureNotify:LocalSend:128") > Array.IndexOf(effects, "GameSuspendExecuted") &&
                    Array.IndexOf(effects, "FeaturesSuspended:LocalExecute") > Array.IndexOf(effects, "FeatureNotify:LocalSend:128"),
                    "Menu outgoing and local execution order differs");
            GsSmokeCheckpoint("Paused:" + id);
        }

        private IEnumerator GsSmokeResume(string id, bool menu = true)
        {
            if (menu) RequestPcMenuResume(id); else RequestPcGameResume(id);
            yield return GsSmokeWait(() => GsSmokeHas("GameResumeExecuted", id), id, 10);
            RequireGsSmoke(!pcGameAppSuspended && !pcMenuSharedSuspended && !pcGameFirstResumeNotification,
                "resume did not change App/shared/first notification");
            if (menu)
            {
                string[] effects = gsSmokeEvents.Where(e => e.operationId == id).Select(e => e.point).ToArray();
                RequireGsSmoke(Array.IndexOf(effects, "GameResume:LocalSend") < Array.IndexOf(effects, "GameResumeReceived") &&
                    Array.IndexOf(effects, "FeaturesResumed:LocalExecute") > Array.IndexOf(effects, "GameResumeExecuted") &&
                    Array.IndexOf(effects, "FeatureNotify:LocalSend:64") > Array.IndexOf(effects, "FeaturesResumed:LocalExecute"),
                    "Menu resume send/execute/Feature notification order differs");
            }
            GsSmokeCheckpoint("Resumed:" + id);
        }

        private void GsSmokeQueueKey(KeyCode key)
        {
            SourcePlayerInputs.Enqueue(0x200010, () => DispatchSourceButton(key));
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease);
        }

        private IEnumerator GsSmokeSeedWager()
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiChipSelections[0].GetComponent<UnityEngine.UI.Button>());
            RefreshPortraitUi();
            QueueStartGameButton(uiBetButtons[27], 510, true);
            yield return GsSmokeWait(() => session.TotalBet == 1 && sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "real straight wager");
            yield return StartGameChooseDemo(30);
            RefreshPortraitUi();
        }

        private IEnumerator GsSmokeSeedWheel()
        {
            yield return GsSmokeSeedWager();
            QueueStartGameButton(uiSpinButton, 511, true);
            yield return GsSmokeWait(() => spinning && sourceRoundPhase == SourceRoundPhase.Wheel && gsSmokePockets == 1 &&
                pcWheelDeliveredFrames > 1, "real wheel");
            RequireGsSmoke(pcOutcome.Receipt.rawResult == 30 && pcOutcome.Receipt.effectiveResult == 27 &&
                session.Credit == 999 && sourceRoundEndCount == 0, "golden real pocket receipt differs");
        }

        private IEnumerator GsSmokeSeedFeature()
        {
            yield return GsSmokeSeedWheel();
            yield return GsSmokeWait(() => SourceDoubleOfferActive && !winRackActive && SourcePlayerInputs.Count == 0, "real Double offer");
            RequireGsSmoke(session.Credit == (PcPaymentTracked ? 999 : 1035) &&
                session.Returned == 36 && session.Rounds == 1, "base evaluation differs");
            if (PcPaymentTracked)
                RequireGsSmoke(pcGamePayment.owner == PcGamePaymentOwner.Base &&
                    pcGamePayment.baseOffer == PcGamePaymentOffer.Open && !pcGamePayment.prepared &&
                    !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished &&
                    ((RouletteSession)session).RoundPayoutPending && CommittedHistoryCount() == 0 &&
                    DisplayedCredit == 1035, "v3 unpaid offer lost its wallet, projection or history boundary");
            GsSmokeQueueKey(KeyCode.D);
            yield return GsSmokeWait(() => session.InDoubleUp && DoubleDecisionReady && SourcePlayerInputs.Count == 0, "real Feature Ready");
            RequireGsSmoke(((RouletteSession)session).DoubleSettlementDeferred && pcDoubleFeatureNv.savedState == 2 &&
                session.Credit == 999 && session.DoubleStake == 36, "deferred Feature Ready differs");
        }

        private IEnumerator GsSmokeGuess()
        {
            RefreshPortraitUi();
            QueueStartGameButton(session.DoubleSuitMode ? uiSuitChoiceButtons[2] : uiDoubleBlackButton, 512, true);
            yield return GsSmokeWait(() => session.DoubleRounds == 1, "real winning suit");
            RequireGsSmoke(pcOutcome.Receipt.rawResult == 0 && pcOutcome.Receipt.effectiveResult == 0 &&
                (PcPaymentTracked ? ReadPcDoubleUnpaidContext(pcOutcome.Receipt) != null :
                    ReadPcDoubleDeferredContext(pcOutcome.Receipt) != null) && gsSmokeSuits == 1 &&
                pcDoubleFeatureNv.savedState == 3 && session.Credit == 999 && session.Returned == 36 &&
                !recordedRoundPages[0].doubleUp.complete, "Reveal receipt or deferred money differs");
        }

        private IEnumerator GsSmokeTakeWin(int pointerId)
        {
            yield return GsSmokeWait(() => DoubleDecisionReady && SourcePlayerInputs.Count == 0,
                "Feature ready for real TAKE", 10);
            int suitsBefore = gsSmokeSuits;
            int pocketsBefore = gsSmokePockets;
            int roundsBefore = session.DoubleRounds;
            int stepBefore = pcDoubleFeatureNv.stepCount;
            string rngBefore = ReadRoundSmokeRaw().rng;
            string receiptBefore = JsonUtility.ToJson(pcOutcome.Receipt);
            if (session.DoubleSuitMode)
            {
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
            }
            RefreshPortraitUi();
            // Wait for CanvasScaler/layout before sampling the real visible button.
            yield return null;
            RefreshPortraitUi();
            RefreshDiagnosticUi();
            Canvas.ForceUpdateCanvases();
            UnityEngine.UI.Button target = session.DoubleSuitMode ? ideckSuitTakeWin : uiDoubleTakeWinButton;
            Canvas expectedCanvas = session.DoubleSuitMode ? ideckCanvas : portraitCanvas;
            RequireGsSmoke(PcGameScreensVisible && expectedCanvas != null && expectedCanvas.enabled &&
                expectedCanvas.gameObject.activeInHierarchy && target != null && target.IsActive() &&
                target.IsInteractable() && DoubleDecisionReady && SourcePlayerInputs.Count == 0,
                "real TAKE target or Canvas unavailable");
            Vector2 point = StartGameButtonPoint(target);
            var pointer = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) {
                position = point, button = UnityEngine.EventSystems.PointerEventData.InputButton.Left
            };
            var hits = new List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointer, hits);
            RequireGsSmoke(hits.Count > 0 && hits[0].gameObject == target.gameObject,
                "real TAKE coordinates hit another UI target");
            GsSmokeCheckpoint("TakeQueued");
            QueueStartGameButton(target, pointerId, true);
            // Live Update dispatches the queued down/release; the fixture does not
            // invoke the callback, dispatch input itself, or shortcut native state.
            yield return GsSmokeWait(() => pcDoubleFeatureNv.savedState == 5 &&
                pcDoubleFeature.TimerRunning && !DoubleDecisionReady && SourcePlayerInputs.Count == 0,
                "real TAKE accepted into saved5", 10);
            RequireGsSmoke(gsSmokeSuits == suitsBefore && gsSmokePockets == pocketsBefore &&
                session.DoubleRounds == roundsBefore && pcDoubleFeatureNv.stepCount == stepBefore &&
                ReadRoundSmokeRaw().rng == rngBefore && JsonUtility.ToJson(pcOutcome.Receipt) == receiptBefore,
                "real TAKE consumed a new outcome or changed Feature step");
            GsSmokeCheckpoint("TakeAccepted");
        }

        private void GsSmokeDisable(string id)
        { RequestPcDisable(new OriginalAutoCompleteRequest(0x1000, true, true, 1, 1), id); }

        private IEnumerator GameSuspensionSmoke()
        {
            RequireGsSmoke(session != null && sourceInputModule != null && pcOutcome != null &&
                !ideckSecondDisplay && !topperDedicatedDisplay && !pcAutoPlayStorageFaulted, "missing real runtime");
            if (gsSmokePrevious != null)
            {
                RequireGsSmoke(gsSmokePockets == 0 && gsSmokeSuits == 0 && gameStartTimestamp == 0 &&
                    ReadRoundSmokeRaw().rng == gsSmokePrevious.raw.rng && !pcGameAppSuspended &&
                    (pcMenuSuspend == null || !pcMenuSuspend.PendingResume), "cold continuation drew or inherited suspension");
                GsSmokeCheckpoint("ColdContinuation");
                if (gsSmokeCase == "cut_game_feature_received" && gsSmokePhase == "restore")
                {
                    RequireGsSmoke(PcDoubleGamePayoutPending && !pcDoubleFeature.Active &&
                        pcDoubleFeatureNv.savedState == 0 && pcDoubleFeatureNv.completionStage == 1 &&
                        session.Credit == 999 && sourceRoundEndCount == 0,
                        "cold received-result restore paid before ordinary Game dispatch");
                    GsSmokeCheckpoint("ColdGameResultReceived");
                }
            }
            else
            {
                yield return GsSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                    !configuredAutoPlayCheckPending && !sourceBetStateEntryPending, "initial Idle");
                RequireGsSmoke(session.Credit == 1000 && session.Rounds == 0 && recordedRoundPages.Count == 0 &&
                    autoCompleteProcess.ElapsedMilliseconds == 0 && autoCompleteProcess.FlashMilliseconds == 0,
                    "fresh financial/process baseline differs");
                if (gsSmokeCase == "menu_gates") yield return GsSmokeMenuGates();
                else if (gsSmokeCase == "movie_instances") yield return GsSmokeMovieInstances();
                else if (gsSmokeCase == "idle_autocomplete") yield return GsSmokeIdleExpiry();
                else if (gsSmokeCase == "demo_purge") yield return GsSmokeDemoPurge();
                else if (gsSmokeCase == "bet_inputs") yield return GsSmokeBetInputs();
                else if (gsSmokeCase == "rollup_menu") yield return GsSmokeRollup();
                else if (gsSmokeCase == "game_feature_independent" || gsSmokeCase == "game_payout_wait" ||
                    gsSmokeCase == "game_payout_auto" || gsSmokeCase == "game_payout_inputs" ||
                    gsSmokeCase == "game_payout_service" ||
                    gsSmokeCase == "cut_game_feature_received") yield return GsSmokeIndependentFeature();
                else if (gsSmokeCase == "pays_timeout") yield return GsSmokePaysTimeout();
                else if (gsSmokeCase.StartsWith("feature_", StringComparison.Ordinal) || gsSmokeCase == "cut_feature_paused")
                    yield return GsSmokeFeature();
                else yield return GsSmokeWheel();
                if (gsSmokeExiting) yield break;
            }
            if (GsSmokeNoRoundCase)
            {
                yield return GsSmokeObserve(300);
                RequireGsSmoke(!pcGameAppSuspended && session.Credit == 1000 && session.Rounds == 0 &&
                    gsSmokePockets == 0 && gsSmokeSuits == 0 && !autoplay && sourceRoundEndCount == 0,
                    "Idle control changed money, drew, or retained suspension");
                yield return GsSmokeCapture("final", true);
                GsSmokeExit(gsSmokePhase == "reload" ? "StableReload" : "StableIdle", "natural");
                yield break;
            }
            if (gsSmokeCase == "feature_autotake_saved5")
            {
                yield return GsSmokeWait(() => pcDoubleFeature.Active && !pcDoubleFeature.TimerRunning &&
                    !pcDoubleFeature.DecisionReady && configuredAutoPlay.AutoPlay == 1, "native unconsumed saved5 event", 15);
                string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
                RequireGsSmoke(session.Credit == 999 && session.Returned == 36 && session.InDoubleUp &&
                    pcDoubleFeatureNv.savedState == 5 && pcDoubleFeatureNv.completionStage == 0 &&
                    !recordedRoundPages[0].doubleUp.complete && sourceRoundEndCount == 0, "saved5 event paid or ended");
                yield return GsSmokeObserve(1000);
                RequireGsSmoke(raw == JsonUtility.ToJson(ReadRoundSmokeRaw()) && session.Credit == 999 &&
                    sourceRoundEndCount == 0, "native unconsumed event advanced later");
                yield return GsSmokeCapture("final", true);
                GsSmokeExit("NativeUnconsumed", "natural");
                yield break;
            }
            if ((gsSmokeCase == "feature_reveal" || gsSmokeCase == "cut_feature_paused") && session.InDoubleUp)
            {
                yield return GsSmokeTakeWin(513);
            }
            if (gsSmokeCase != "wheel_autocomplete" && gsSmokeCase != "feature_ready_expiry" &&
                gsSmokeCase != "feature_reveal" && gsSmokeCase != "cut_feature_paused" &&
                gsSmokeCase != "game_feature_independent" && gsSmokeCase != "game_payout_wait" &&
                gsSmokeCase != "game_payout_auto" && gsSmokeCase != "game_payout_inputs" &&
                gsSmokeCase != "game_payout_service" &&
                gsSmokeCase != "cut_game_feature_received" && gsSmokePhase != "reload")
            {
                yield return GsSmokeWait(() => SourceDoubleOfferActive && !winRackActive, "offer after resumed wheel/rack");
                GsSmokeQueueKey(KeyCode.Space);
            }
            yield return GsSmokeWait(() => latestRoundLifecycle != null && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete && sourceRoundPhase == SourceRoundPhase.None &&
                !configuredAutoPlayCheckPending && !session.InDoubleUp && !ResultDisplayActive, "one legal End and stable Idle");
            long expectedReturn = gsSmokeCase == "feature_reveal" || gsSmokeCase == "cut_feature_paused" ?
                (session.DoubleSuitMode ? 144 : 72) : 36;
            long expected = gsSmokeCase == "bet_inputs" && gsSmokePhase == "reload" ? 0 : 999 + expectedReturn;
            RequireGsSmoke(session.Credit == expected && session.Returned == expectedReturn && session.Wagered == 1 &&
                session.Rounds == 1 && session.Wins == 1 && !autoplay && configuredAutoPlay.AutoPlay == 0 &&
                sourceRoundEndCount == (gsSmokePhase == "reload" ? 0 : 1), "final money/Auto/End count differs");
            RequireGsSmoke(gsSmokePockets == (gsSmokePhase == "run" ? 1 : 0) &&
                gsSmokeSuits == (gsSmokePhase == "run" && (gsSmokeCase == "feature_reveal" ||
                gsSmokeCase == "cut_feature_paused") ? 1 : 0), "extra outcome draw");
            if (gsSmokeCase == "feature_ready_expiry" || gsSmokeCase == "feature_reveal" || gsSmokeCase == "cut_feature_paused" ||
                gsSmokeCase == "game_feature_independent" || gsSmokeCase == "game_payout_wait" ||
                gsSmokeCase == "game_payout_auto" || gsSmokeCase == "game_payout_inputs" ||
                gsSmokeCase == "game_payout_service" ||
                gsSmokeCase == "cut_game_feature_received")
                RequireGsSmoke(pcDoubleFeatureNv.completionStage == 2 && pcDoubleFeatureNv.savedState == 0 &&
                    recordedRoundPages[0].doubleUp.complete, "Feature completion lacks Applied state0");
            if (gsSmokeCase == "bet_inputs")
            {
                if (gsSmokePhase == "run") yield return GsSmokeCollectResume();
                else
                {
                    PcCreditEvent collect = ReadPcCreditEvent();
                    RequireGsSmoke(collect != null && collect.kind == "collect" && collect.phase == 3 &&
                        collect.collected == 1035 && latestRoundLifecycle.idleEntry.consumedCollectEventId == collect.id &&
                        !pcCollectPending && session.Credit == 0, "cold collect replay duplicated or lost its completed Idle");
                }
            }
            yield return GsSmokeCapture("final", true);
            GsSmokeExit(gsSmokePhase == "reload" ? "StableReload" : "Complete", "natural");
        }

        private IEnumerator GsSmokeMenuGates()
        {
            yield return GsSmokeWait(() => footerMovieClock != null && footerMovieClock.Playing, "real Idle footer movie", 10);
            yield return GsSmokeSuspend("idle-pause");
            int movie = footerMovieClock.Frame;
            RequireGsSmoke(footerMovieClock.Paused && pcPausedMovies.Contains(footerMovieClock), "Menu omitted its playing movie");
            yield return GsSmokeSuspend("idle-pause-repeat");
            string moviesPaused = PcMoviePlaybackSnapshot;
            long moviesPausedCount = PcMoviePauseCount;
            yield return GsSmokeObserve(350);
            RequireGsSmoke(pcGameSuspendChanges == 1 && footerMovieClock.Frame == movie && !footerMovieClock.Playing,
                "duplicate suspend changed App twice or advanced paused movie");
            RequireGsSmoke(moviesPaused == PcMoviePlaybackSnapshot && moviesPausedCount == PcMoviePauseCount &&
                PcPausedMovieCount > 0, "Menu movie pause set changed or its real frames advanced");
            yield return GsSmokeCapture("idle_paused");
            yield return GsSmokeResume("idle-resume");
            yield return GsSmokeResume("idle-resume-repeat");
            RequireGsSmoke(pcGameResumeChanges == 1 && pcPausedMovies.Count == 0 && pcPausedSounds.Count == 0 &&
                PcPausedMovieCount == 0 && PcMovieResumeCount == moviesPausedCount &&
                gsSmokeEvents.Count(e => e.point == "MenuSuspendChanged:false:LocalSend") == 1,
                "duplicate resume changed App or first notification twice");
            yield return GsSmokeCapture("idle_resumed");

            yield return GsSmokeSuspend("config-pause");
            SetPcMenuLifecycleContext(10, false, false, true, false, false);
            RequestPcMenuResume("config-pending");
            yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "config-pending"), "state10 pending", 5);
            RequireGsSmoke(pcMenuSuspend.PendingResume && pcGameAppSuspended, "state10 false wait bypassed pending");
            SetPcMenuLifecycleContext(10, false, false, true, true, false);
            RequestPcMenuResume("diagnostics");
            yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Diagnostics", "diagnostics"), "diagnostics gate", 5);
            RequireGsSmoke(pcMenuState == 9 && pcMenuDiagnosticsReturn && pcMenuSuspend.PendingResume && pcGameAppSuspended,
                "diagnostics branch lost pending or resumed");
            SetPcMenuLifecycleContext(10, true, false, true, false, false);
            yield return GsSmokeResume("config-wait-true");
            RequireGsSmoke(!GsSmokeEvent("GameResume:LocalSend", "config-wait-true").pending,
                "ordinary resume cleared pending after dispatch");

            SetPcMenuLifecycleContext(0, false, false, true, false, false);
            yield return GsSmokeSuspend("transition-pause");
            SetPcMenuLifecycleContext(0, false, true, true, false, false);
            RequestPcMenuResume("transition-pending");
            yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "transition-pending"), "transition pending", 5);
            RequestPcMenuGoToGameCompleted("goto-tail");
            yield return GsSmokeWait(() => GsSmokeHas("FeatureNotify:LocalSend:64", "goto-tail"), "GoToGame tail", 5);
            RequireGsSmoke(!pcMenuTransitioning && !pcMenuSuspend.PendingResume && !pcGameAppSuspended &&
                GsSmokeEvent("GameResume:LocalSend", "goto-tail").pending &&
                !GsSmokeEvent("GameResume:LocalSend", "goto-tail").transitioning,
                "GoToGame did not clear transition before and pending after dispatch");
            int effects = gsSmokeEvents.Count(e => e.point == "GameResume:LocalSend");
            RequestPcMenuGoToGameCompleted("goto-tail-repeat");
            yield return GsSmokeObserve(50);
            RequireGsSmoke(gsSmokeEvents.Count(e => e.point == "GameResume:LocalSend") == effects, "GoToGame consumed pending twice");

            yield return GsSmokeSuspend("ready-pause");
            SetPcMenuLifecycleContext(0, false, false, false, false, false);
            RequestPcMenuResume("ready-pending");
            yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "ready-pending"), "unready pending", 5);
            RequireGsSmoke(pcMenuSuspend.PendingResume && pcGameAppSuspended, "unready owner resumed");
            SetPcMenuLifecycleContext(0, false, false, true, false, false);
            yield return GsSmokeSuspend("ready-clear-pending");
            RequireGsSmoke(!pcMenuSuspend.PendingResume, "new Suspend did not clear pending");
            yield return GsSmokeResume("ready-resume");

            yield return GsSmokeSuspend("config-tail-pause");
            SetPcMenuLifecycleContext(10, false, false, true, false, false);
            RequestPcMenuResume("config-tail-pending");
            yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "config-tail-pending"), "config pending", 5);
            RequestPcMenuConfigCompleted("config-tail");
            yield return GsSmokeWait(() => GsSmokeHas("FeatureNotify:LocalSend:64", "config-tail"), "config completion tail", 5);
            RequireGsSmoke(pcMenuState == 0 && !pcMenuSuspend.PendingResume && !pcGameAppSuspended &&
                GsSmokeEvent("GameResume:LocalSend", "config-tail").pending &&
                GsSmokeEvent("GameResume:LocalSend", "config-tail").menuState == 0,
                "config tail did not clear state before and pending after dispatch");
            effects = gsSmokeEvents.Count(e => e.point == "GameResume:LocalSend");
            RequestPcMenuConfigCompleted("config-tail-repeat");
            yield return GsSmokeObserve(50);
            RequireGsSmoke(gsSmokeEvents.Count(e => e.point == "GameResume:LocalSend") == effects,
                "config tail consumed pending twice");
        }

        private IEnumerator GsSmokeBetInputs()
        {
            yield return GsSmokeSeedWager();
            yield return GsSmokeSuspend("bet-pause");
            int dispatched = 0, released = 0;
            SourcePlayerInputs.Enqueue(0x200010, () => { ++dispatched; DispatchSourceButton(KeyCode.Space); });
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease, () => ++dispatched, () => ++released);
            yield return GsSmokeObserve(350);
            RefreshPortraitUi();
            RequireGsSmoke(SourcePlayerInputs.Count == 2 && dispatched == 0 && session.TotalBet == 1 &&
                session.Credit == 1000 && gsSmokePockets == 0 && !uiSpinButton.interactable &&
                !uiBetButtons[27].interactable, "paused Bet accepted ordinary input");
            yield return GsSmokeCapture("bet_paused");
            yield return GsSmokeResume("bet-resume");
            RequireGsSmoke(SourcePlayerInputs.Count == 0 && dispatched == 0 && released == 1 && session.TotalBet == 1 &&
                gsSmokePockets == 0, "resume did not purge pending press/release");
            yield return GsSmokeCapture("bet_resumed");
            RefreshPortraitUi();
            QueueStartGameButton(uiSpinButton, 514, true);
        }

        private IEnumerator GsSmokeWheel()
        {
            yield return GsSmokeSeedWheel();
            string route = gsSmokeCase == "wheel_rpc1" ? "rpc1" : gsSmokeCase == "wheel_direct" ? "direct" : "rpc0";
            if (gsSmokeCase == "wheel_autocomplete" || GsSmokeCutCase) route = "menu";
            yield return GsSmokeSuspend("wheel-pause", route);
            GameSuspensionSmokeEvent before = GsSmokeSnapshot("WheelHeldStart");
            gsSmokeEvents.Add(before);
            if (gsSmokeCase == "wheel_autocomplete")
            {
                GsSmokeDisable("wheel-disable");
                yield return GsSmokeWait(() => gsSmokeNotifications.Any(n => n.kind == "AutoCompleteStart"), "paused wheel AutoComplete", 10);
                RequireGsSmoke(configuredAutoPlay.AutoPlay == 1 && autoplay && pcAutoCompleteDeliveries == 4 &&
                    pcGameAppSuspended && sourceRoundPhase == SourceRoundPhase.Wheel && sourceRoundEndCount == 0,
                    "paused wheel expiry did not service timer or crossed App gate");
            }
            else yield return GsSmokeObserve(350);
            GameSuspensionSmokeEvent after = GsSmokeSnapshot("WheelHeldEnd");
            gsSmokeEvents.Add(after);
            RequireGsSmoke(before.wheelFrames == after.wheelFrames && before.wheelPath == after.wheelPath &&
                before.wheelStage == after.wheelStage && before.wheelAngle == after.wheelAngle &&
                before.wheelBall == after.wheelBall && before.rng == after.rng && after.wheelPaused &&
                after.wheelConsumed == 0 && before.credit == after.credit && before.rounds == after.rounds,
                "paused wheel advanced trajectory, RNG or money");
            yield return GsSmokeCapture("wheel_paused");
            if (GsSmokeCutCase)
            {
                // Also prove Menu pending is process-local, using its actual gate.
                SetPcMenuLifecycleContext(0, false, true, true, false, false);
                RequestPcMenuResume("cut-pending");
                yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "cut-pending"), "cut pending", 5);
                RequireGsSmoke(pcMenuSuspend.PendingResume && pcGameAppSuspended, "cut lost stable pause");
                yield return GsSmokeCapture("final", true);
                GsSmokeExit("Paused", "checkpoint");
                yield break;
            }
            yield return GsSmokeResume("wheel-resume", route == "menu");
            GameSuspensionSmokeEvent resume = GsSmokeEvent("GameResumeExecuted", "wheel-resume");
            RequireGsSmoke(resume.wheelEpoch > before.wheelEpoch && resume.wheelGeneration > before.wheelGeneration &&
                resume.wheelFrames == before.wheelFrames &&
                resume.wheelConsumed == 0 && resume.wheelPath == before.wheelPath && !resume.wheelPaused &&
                resume.screens == before.screens, "wheel resume reset trajectory, displayed screens, or reused timer epoch");
            long due = OriginalRackClock.AfterMilliseconds(resume.wheelEpoch, 42, Stopwatch.Frequency);
            sourceInputModule.enabled = true;
            while (Stopwatch.GetTimestamp() < due)
            {
                RequireGsSmoke(pcWheelDeliveredFrames == before.wheelFrames, "wheel resumed before its fresh full period");
                yield return null;
            }
            sourceInputModule.enabled = false;
            yield return GsSmokeWait(() => pcWheelDeliveredFrames > before.wheelFrames, "first resumed wheel frame", 5);
            GsSmokeCheckpoint("FirstResumedWheelFrame");
            yield return GsSmokeCapture("wheel_resumed");
            if (route == "direct")
            {
                RequireGsSmoke(!pcGameScreensVisible, "direct Resume unexpectedly called ShowScreens");
                // A separate real 0x106 request owns Show; Resume itself does not.
                yield return GsSmokeSuspend("direct-display-reopen", "rpc0");
                yield return GsSmokeResume("direct-display-resume", false);
                RequireGsSmoke(pcGameScreensVisible, "subsequent rpc0 did not restore source display path");
                yield return GsSmokeCapture("direct_visible_again");
            }
        }

        private IEnumerator GsSmokeRollup()
        {
            yield return GsSmokeSeedWheel();
            yield return GsSmokeWait(() => winRackActive && winRackPaid > 0 &&
                winRackPreviousRemainingMilliseconds != int.MaxValue && pcResultCycleClock != null && pcResultCycleClock.Visible &&
                footerMoviePhase == FooterMoviePhase.Outro && footerMovieClock != null && footerMovieClock.Playing &&
                paidFadeClock.Running && PaidOpacity < 1f,
                "real serviced rollup with visible winning pay and active Outro/fade");
            yield return GsSmokeSuspend("rollup-pause");
            GameSuspensionSmokeEvent before = GsSmokeSnapshot("RackHeldStart");
            gsSmokeEvents.Add(before);
            RequireGsSmoke(before.cyclePaused && before.cycleDue == 0 && before.pausedSounds > 0,
                "rollup did not stop cycle receipt and pause real rack sound");
            RequireGsSmoke(before.footerPhase == (int)FooterMoviePhase.Outro && before.moviePaused && !before.moviePlaying &&
                before.paidFadeRunning && before.paidAlpha >= 0f && before.paidAlpha < 1f &&
                before.paidFadeCallbacks >= 0 && before.paidFadeCallbacks < 30,
                "rollup pause did not retain the real Outro and independent in-flight Paid fade");
            yield return GsSmokeObserve(1700);
            // Menu pauses renderer movies. ProView's separate 33 ms label
            // trigger continues, so an early Outro frame can coexist with
            // fully opaque Paid digits. Do not hide either source object.
            // Observe completion, rather than infer it from elapsed wall time.
            yield return GsSmokeWait(() => !paidFadeClock.Running && PaidOpacity == 1f,
                "independent Paid fade completion while the Movie remains paused", 10);
            GameSuspensionSmokeEvent after = GsSmokeSnapshot("RackHeldEnd");
            gsSmokeEvents.Add(after);
            RequireGsSmoke(after.rackActive && after.rackPaid == before.rackPaid && after.rackStart == before.rackStart &&
                after.rackDue == before.rackDue && after.cycleCallbacks == before.cycleCallbacks &&
                after.cycleIndex == before.cycleIndex && after.cycleVisible == before.cycleVisible &&
                after.rng == before.rng && after.ends == 0, "paused rack/cycle ordinary service advanced or shifted its anchor");
            RequireGsSmoke(after.app && after.footerPhase == before.footerPhase && after.moviePaused && !after.moviePlaying &&
                after.movieFrame == before.movieFrame && !after.paidFadeRunning && after.paidAlpha == 1f &&
                after.paidFadeCallbacks == 30 && after.paidFadeCallbacks > before.paidFadeCallbacks &&
                uiPaid.Opacity == 1f && uiCashPaid.Opacity == 1f,
                "Movie pause must preserve its frame while the independent Paid/CashPaid fade completes");
            yield return GsSmokeCapture("rack_paused");
            yield return GsSmokeResume("rollup-resume");
            GameSuspensionSmokeEvent resumed = GsSmokeEvent("GameResumeExecuted", "rollup-resume");
            RequireGsSmoke(resumed.rackStart == before.rackStart && resumed.rackDue == before.rackDue &&
                resumed.cycleCallbacks == before.cycleCallbacks && resumed.cycleIndex == before.cycleIndex &&
                resumed.cycleVisible == before.cycleVisible && !resumed.cyclePaused &&
                resumed.cycleDue > resumed.timestamp - Stopwatch.Frequency / 100 &&
                resumed.cycleDue <= resumed.timestamp + Stopwatch.Frequency / 500,
                "resume shifted rack early or failed fresh 1ms cycle receipt");
            yield return GsSmokeWait(() => pcResultCycleClock.CallbackCount > (ulong)before.cycleCallbacks, "next retained cycle action", 5);
            RequireGsSmoke(pcResultCycleClock.CallbackCount == (ulong)before.cycleCallbacks + 1 &&
                !pcResultCycleClock.Visible && pcResultCycleClock.Index == (before.cycleIndex + 1) % pcResultCycleClock.RecordCount,
                "resume replayed initial pay or skipped cycle phases");
            RequireGsSmoke((footerMovieClock == null || !footerMovieClock.Paused) &&
                !paidFadeClock.Running && PaidOpacity == 1f && paidFadeClock.CallbackCount == 30 &&
                uiPaid.Opacity == 1f && uiCashPaid.Opacity == 1f,
                "Movie resume restarted or hid the completed independent Paid fade");
            GsSmokeCheckpoint("RackServiceResumed");
            yield return GsSmokeCapture("rack_resumed");
            yield return GsSmokeWait(() => SourceDoubleOfferActive && !winRackActive, "rack to offer");
            ToggleGameInfoOptions(); OpenPaysFromGameInfo(); RefreshPortraitUi();
            RequireGsSmoke(paysPage >= 0, "real Pays view did not open");
            yield return GsSmokeSuspend("pays-pause");
            int page = paysPage;
            yield return GsSmokeObserve(350);
            RequireGsSmoke(paysPage == page && SourceDoubleOfferActive && sourceRoundEndCount == 0,
                "paused Pays changed offer or page");
            yield return GsSmokeCapture("pays_paused");
            yield return GsSmokeResume("pays-resume");
            yield return GsSmokeCapture("pays_resumed");
            ClosePays(); CloseGameInfoOptions();
        }

        private IEnumerator GsSmokeFeature()
        {
            yield return GsSmokeSeedFeature();
            if (gsSmokeCase == "feature_reveal" || gsSmokeCase == "cut_feature_paused") yield return GsSmokeGuess();
            else if (gsSmokeCase == "feature_autotake_saved5")
            {
                GsSmokeDisable("ready-take-disable");
                yield return GsSmokeWait(() => PcNativeDoubleFeatureAutoTake && pcDoubleFeature.LiveState == 2 &&
                    pcDoubleFeatureNv.savedState == 5, "live2/saved5 automatic TAKE", 10);
            }
            long generation = pcDoubleFeature.TimerGeneration;
            int saved = pcDoubleFeatureNv.savedState;
            yield return GsSmokeSuspend("feature-pause");
            RequireGsSmoke(pcDoubleFeature.Suspended && !pcDoubleFeature.TimerRunning && !PcNativeDoubleFeatureVisible,
                "full Menu suspend did not stop and hide Feature");
            long oldDue = pcDoubleFeatureDue;
            if (gsSmokeCase == "feature_ready_expiry")
            {
                GsSmokeDisable("ready-paused-disable");
                yield return GsSmokeWait(() => configuredAutoPlay.AutoPlay == 1 && !pcAutoComplete.TimerRunning,
                    "AutoComplete during full Ready pause", 10);
                RequireGsSmoke(pcAutoCompleteDeliveries == 4 && autoplay && pcDoubleFeature.Suspended &&
                    !pcDoubleFeature.LocalAuto && !pcDoubleFeature.TimerRunning && sourceRoundEndCount == 0,
                    "expiry failed to service IPC or resumed Feature under App gate");
            }
            yield return GsSmokeObserve(gsSmokeCase == "feature_autotake_saved5" ? 3300 : 1400);
            RequireGsSmoke(session.Credit == 999 && session.Returned == 36 && session.InDoubleUp &&
                pcDoubleFeatureNv.savedState == saved && pcDoubleFeatureNv.completionStage == 0 &&
                !pcDoubleFeature.TimerRunning && pcDoubleFeatureDue == oldDue && sourceRoundEndCount == 0,
                "paused Feature's cancelled receipt completed or paid");
            yield return GsSmokeCapture("feature_paused");
            if (GsSmokeCutCase)
            {
                SetPcMenuLifecycleContext(10, false, false, true, false, false);
                RequestPcMenuResume("cut-feature-pending");
                yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "cut-feature-pending"), "Feature cut pending", 5);
                RequireGsSmoke(pcMenuSuspend.PendingResume && pcDoubleFeature.Suspended, "Feature cut lost pending pause");
                yield return GsSmokeCapture("final", true);
                GsSmokeExit("Paused", "checkpoint");
                yield break;
            }
            yield return GsSmokeResume("feature-resume");
            GameSuspensionSmokeEvent resume = GsSmokeEvent("FeaturesResumed:LocalExecute", "feature-resume");
            RequireGsSmoke(!pcDoubleFeature.Suspended && pcDoubleFeature.TimerRunning &&
                pcDoubleFeature.TimerGeneration > generation && resume.credit == 999 && resume.returned == 36,
                "Feature resume reused timer or paid synchronously");
            if (gsSmokeCase == "feature_reveal")
            {
                RequireGsSmoke(resume.saved == 3 && resume.featureDelay == 1250,
                    "saved Reveal did not rearm a new full reveal delay");
                yield return GsSmokeObserve(300);
                RequireGsSmoke(!DoubleDecisionReady && pcDoubleFeatureNv.savedState == 3 && session.Credit == 999,
                    "Reveal consumed its old pre-pause deadline");
            }
            else if (gsSmokeCase == "feature_autotake_saved5")
                RequireGsSmoke(resume.live == 5 && resume.saved == 5 && resume.featureOpcode == 0x1a && resume.featureDelay == 3000,
                    "saved5 resume did not preserve native ignored event branch");
            else
                RequireGsSmoke(resume.auto == 1 && resume.featureDelay == 3000 && resume.featureOpcode == 0x1a &&
                    resume.live == 2 && resume.saved == 5, "Feature resume did not read current Auto and begin TAKE");
            yield return GsSmokeCapture("feature_resumed");
        }

        private IEnumerator GsSmokeIdleExpiry()
        {
            // Bonus is an explicit platform fixture for the native deferred
            // Idle Disable branch. App suspension comes only from Menu.
            SetPcAutoCompletePlatformContext(true, false, true);
            yield return GsSmokeSuspend("idle-auto-pause");
            GsSmokeDisable("idle-auto-disable");
            yield return GsSmokeWait(() => GsSmokeHas("Auto:DisableHandled", "") && PcAutoCompleteTimerExists,
                "deferred Idle countdown", 5);
            SetPcAutoCompletePlatformContext(true, false, false);
            yield return GsSmokeWait(() => !pcAutoComplete.TimerRunning, "paused Idle timer expiry", 10);
            RequireGsSmoke(pcGameAppSuspended && pcAutoCompleteDeliveries == 4 && configuredAutoPlay.SecsToAutoCompleteStart == 0 &&
                configuredAutoPlay.SecsToAutoCompleteWarning == 0 && configuredAutoPlay.AutoPlay == 0 && !autoplay &&
                gsSmokeNotifications.Count == 1 && gsSmokeNotifications[0].kind == "DisableComplete" &&
                gsSmokeNotifications[0].origin == "ZeroCompletion" && session.Credit == 1000 && session.Rounds == 0,
                "paused Idle countdown or zero completion differs");
            yield return GsSmokeCapture("idle_auto_paused");
            yield return GsSmokeResume("idle-auto-resume");
            yield return GsSmokeCapture("idle_auto_resumed");
        }

        private IEnumerator GsSmokeCollectResume()
        {
            RequireGsSmoke(latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete && session.Credit == 1035,
                "collect fixture requires naturally completed End/Idle");
            SetPcAutoCompletePlatformContext(true, false, true);
            GsSmokeDisable("collect-idle-disable");
            yield return GsSmokeWait(() => PcAutoCompleteTimerExists && pcAutoComplete.TimerRunning,
                "collect Idle countdown", 5);
            SetPcAutoCompletePlatformContext(true, false, false);
            long deliveries = pcAutoCompleteDeliveries;
            string rng = ReadRoundSmokeRaw().rng;
            RequestPcCollect();
            PcCreditEvent accepted = ReadPcCreditEvent();
            RequireGsSmoke(pcCollectPending && accepted != null && accepted.kind == "collect" && accepted.phase == 1 &&
                accepted.collected == 1035, "real PC collect request did not enter its first yield");
            GsSmokeCheckpoint("CollectAccepted");
            // Queued on the same owner frame as the collect's first yield.
            // The collect journal continues independently of ordinary App work.
            yield return GsSmokeSuspend("collect-pause");
            yield return GsSmokeWait(() => !pcCollectPending && ReadPcCreditEvent().phase == 3 &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Pending, "collect phase3 with paused Idle Pending", 5);
            RequireGsSmoke(pcGameAppSuspended && session.Credit == 0 && sourceRoundEndCount == 1 &&
                latestRoundLifecycle.idleEntry.consumedCollectEventId == accepted.id,
                "collect under suspension bypassed its pending Idle or duplicated End");
            GsSmokeCheckpoint("CollectIdlePendingWhilePaused");
            yield return GsSmokeWait(() => !pcAutoComplete.TimerRunning, "collect-paused IPC countdown", 10);
            RequireGsSmoke(pcAutoCompleteDeliveries > deliveries && pcGameAppSuspended &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Pending && session.Credit == 0,
                "collect Pending blocked IPC or ran ordinary Idle while paused");
            yield return GsSmokeCapture("collect_pending_paused");
            yield return GsSmokeResume("collect-resume");
            yield return GsSmokeWait(() => latestRoundLifecycle.idleEntry.state == PcIdleState.Complete &&
                !configuredAutoPlayCheckPending, "collect resume consumes ordinary Idle once", 10);
            PcCreditEvent completed = ReadPcCreditEvent();
            RequireGsSmoke(completed.id == accepted.id && completed.phase == 3 && completed.collected == 1035 &&
                latestRoundLifecycle.idleEntry.consumedCollectEventId == accepted.id && session.Credit == 0 &&
                sourceRoundEndCount == 1 && gsSmokePockets == 1 && gsSmokeSuits == 0 && ReadRoundSmokeRaw().rng == rng,
                "collect resume repeated its transaction/End/draw");
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
            yield return GsSmokeObserve(300);
            RequireGsSmoke(raw == JsonUtility.ToJson(ReadRoundSmokeRaw()), "completed collect continued writing");
            yield return GsSmokeCapture("collect_resumed");
        }

        private IEnumerator GsSmokeDemoPurge()
        {
            // Use actual Demo selection and the live persistent setters; no
            // substitute wallet, outcome source or queue is installed.
            yield return StartGameChooseDemo(2);
            RequireGsSmoke(configuredAutoPlay.TurboAutoPlay && pcGameSoundsMuted && !autoplay, "real Demo Turbo fixture differs");
            yield return GsSmokeSuspend("release-pause");
            string autoRaw = ReadRoundSmokeRaw().auto;
            int released = 0, dispatched = 0;
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease, () => ++dispatched, () => ++released);
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease, () => ++dispatched, () => ++released);
            yield return GsSmokeResume("release-resume");
            RequireGsSmoke(released == 2 && dispatched == 0 && SourcePlayerInputs.Count == 0 &&
                ReadRoundSmokeRaw().auto == autoRaw && configuredAutoPlay.TurboAutoPlay && pcGameSoundsMuted,
                "special-only resume purge wrote Auto or unmuted sounds");

            yield return GsSmokeSuspend("ordinary-pause");
            SourcePlayerInputs.Enqueue(0x200001, () => ++dispatched);
            SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease, null, () => ++released);
            yield return GsSmokeObserve(100);
            RequireGsSmoke(SourcePlayerInputs.Count == 2 && dispatched == 0, "ordinary input escaped App gate");
            yield return GsSmokeCapture("demo_input_paused");
            yield return GsSmokeResume("ordinary-resume");
            RequireGsSmoke(released == 3 && dispatched == 0 && SourcePlayerInputs.Count == 0 &&
                ReadRoundSmokeRaw().auto != autoRaw && configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo &&
                !configuredAutoPlay.ReplenishCredits && !configuredAutoPlay.DemoOnly && configuredAutoPlay.PlayerInput &&
                !pcGameSoundsMuted && !autoplay && demoOutcome.SelectedOption == 2,
                "ordinary resume purge lost configured setter/sound or reset unrelated Demo enum");

            // Current Deluxe UI index 7 is fixed pocket 4, not native Demo
            // enum 7. Preserve that mapping and its armed visual path.
            yield return StartGameChooseDemo(7);
            configuredAutoPlay.SetOptions(true, false, false, false, true, 0);
            yield return GsSmokeSuspend("no-player-input-pause");
            autoRaw = ReadRoundSmokeRaw().auto;
            SourcePlayerInputs.Enqueue(0x200010, () => ++dispatched);
            yield return GsSmokeResume("no-player-input-resume");
            RequireGsSmoke(ReadRoundSmokeRaw().auto == autoRaw && configuredAutoPlay.AutoPlay == 1 &&
                !configuredAutoPlay.PlayerInput && dispatched == 0 && demoOutcome.SelectedOption == 7 &&
                demoOutcome.SelectedPocket == 4 && demoOutcome.ArmedVisualPath == 4,
                "GetPlayerInput=false purge changed options or confused UI index with native enum");
            // Finish using the same real reset setter through resume purge,
            // leaving this no-round chain cold-reloadable without Auto entry.
            configuredAutoPlay.SetPlayerInput(true);
            yield return GsSmokeSuspend("demo-cleanup-pause");
            SourcePlayerInputs.Enqueue(0x200010, () => ++dispatched);
            yield return GsSmokeResume("demo-cleanup-resume");
            RequireGsSmoke(configuredAutoPlay.AutoPlay == 0 && demoOutcome.SelectedOption == 7 &&
                demoOutcome.ArmedVisualPath == 4 && dispatched == 0, "normal input reset changed fixed Demo choice");
            yield return GsSmokeCapture("demo_input_resumed");
        }

        private IEnumerator GsSmokeCapture(string label, bool final = false)
        {
            string path = final ? gsSmokeCapturePath : Path.Combine(Path.GetDirectoryName(gsSmokeCapturePath),
                Path.GetFileNameWithoutExtension(gsSmokeCapturePath) + "_" + label + ".png");
            RequireGsSmoke(!File.Exists(path) && gsSmokeCaptures.All(c => c.label != label), "capture already exists");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            yield return null;
            var capture = new GameSuspensionSmokeCapture { label = label, path = path, state = GsSmokeSnapshot("Capture:" + label) };
            ScreenCapture.CaptureScreenshot(path, 2);
            float until = Time.realtimeSinceStartup + 10;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < until) yield return null;
            RequireGsSmoke(File.Exists(path) && new FileInfo(path).Length > 0, "capture did not finish");
            capture.sha256 = RngSmokeBytesSha(File.ReadAllBytes(path)); gsSmokeCaptures.Add(capture);
            Debug.Log("ROULETTE_GAME_SUSPENSION_CAPTURE: label=" + label + " path=" + path + " sha256=" + capture.sha256);
        }

        private void GsSmokeExit(string point, string mode)
        {
            RequireGsSmoke(!gsSmokeExiting && !pcAutoPlayStorageFaulted, "duplicate/faulted exit");
            gsSmokeExiting = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var proof = new GameSuspensionSmokeProof {
                testCase = gsSmokeCase, phase = gsSmokePhase, checkpoint = point, exitMode = mode,
                historyKey = recordedHistoryKey, rngPath = PcRngStateStore.PathForCurrentProcess, autoPath = PcAutoPlayStatePath,
                previousProofSha256 = gsSmokePreviousHash, raw = ReadRoundSmokeRaw(), rngCandidate = RngSmokeCandidate(),
                featureJson = pcDoubleFeatureNv == null ? "null" : JsonUtility.ToJson(pcDoubleFeatureNv),
                frequency = Stopwatch.Frequency, checks = gsSmokeChecks, pockets = gsSmokePockets, suits = gsSmokeSuits,
                ends = sourceRoundEndCount, finalState = GsSmokeSnapshot(point), events = gsSmokeEvents.ToArray(),
                notifications = gsSmokeNotifications.ToArray(), stageCaptures = gsSmokeCaptures.ToArray()
            };
            proof.rawJson = JsonUtility.ToJson(proof.raw); proof.rawSha256 = RoundSmokeSha(proof.rawJson);
            string encoded = JsonUtility.ToJson(proof, true) + "\n";
            Directory.CreateDirectory(Path.GetDirectoryName(gsSmokePath));
            using (var stream = new FileStream(gsSmokePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(encoded);
                stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
            }
            Debug.Log("ROULETTE_GAME_SUSPENSION_PASSED: case=" + gsSmokeCase + " phase=" + gsSmokePhase + " point=" + point +
                " mode=" + mode + " checks=" + gsSmokeChecks + " credit=" + proof.finalState.credit + " rounds=" + proof.finalState.rounds +
                " ends=" + proof.ends + " pockets=" + proof.pockets + " suits=" + proof.suits +
                " fault=false proof=" + RoundSmokeSha(encoded));
            Debug.Log("ROULETTE_GAME_SUSPENSION_EXIT: api=" + (mode == "checkpoint" ? "TerminateProcess" : "Application.Quit") +
                " code=0 mode=" + mode);
            if (mode == "checkpoint")
            {
                RequireGsSmoke(recordedHistoryKey.Contains(".Test."), "cut requires an isolated namespace");
                TerminateProcess(GetCurrentProcess(), 0);
                throw new InvalidOperationException("Game suspension current-process cut returned");
            }
            Application.Quit(0);
        }
    }
}
