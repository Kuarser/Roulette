using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    // Kept separate from RouletteGame so a production fail-stop can be observed.
    public sealed class RouletteGamePaymentSmokeHost : MonoBehaviour { }

    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class GamePaymentSmokeEvent
        {
            public string point, paymentJson, featureJson, receiptJson, controlOperationId;
            public long at, credit, returned, wagered, rounds, displayedCredit, displayedPaid;
            public int frame, visibleHistory, physicalHistory, pockets, suits, replyAttempts, ends;
            public int phase, state;
            public bool overflow, fault, app, rack, resultDisplay, modelPending;
            public bool gameSelected, payoutEntered, screens, offerPlayAllowed;
        }

        [Serializable]
        private sealed partial class GamePaymentSmokeProof
        {
            public int schema = 2;
            public string testCase, phase, checkpoint, exitMode, historyKey, rngPath, autoPath;
            public string previousProofSha256, rawSha256, rawJson, bootRawSha256, bootRawJson, paymentJson, featureJson, lifecycleJson;
            public string rngReceiptJson, faultOperation;
            public RoundLifecycleSmokeRaw raw;
            public long credit, returned, wagered, rounds, wins, denomination, remainder, frequency;
            public long expectedInitialCredit, expectedBaseAward, expectedFinalAward;
            public long[] replyTimes;
            public int checks, pockets, suits, replyAttempts, completeSpins, recordPockets, ends;
            public int physicalHistory, visibleHistory, archiveSchema;
            public bool fault, overflow, actualOfferPlay, actualFeatureEnter, actualFeatureTake;
            public bool gameSelected, payoutEntered, screens;
            public GamePaymentSmokeEvent[] events;
            public GamePaymentSmokeCapture[] stageCaptures;
            public GamePaymentSmokeHold[] holds;
            public GamePaymentSmokeFunding[] fundingEvents;
        }

        private string gpSmokeCase, gpSmokePhase, gpSmokeOutput, gpSmokeCut, gpSmokeFailBefore;
        private string gpSmokePreviousHash = "";
        private GamePaymentSmokeProof gpSmokePrevious;
        private RoundLifecycleSmokeRaw gpSmokeBoot, gpSmokeFaultRaw;
        private readonly List<GamePaymentSmokeEvent> gpSmokeEvents = new List<GamePaymentSmokeEvent>();
        private readonly List<long> gpSmokeReplyTimes = new List<long>();
        private bool gpSmokeConfigured, gpSmokeStarted, gpSmokeExiting, gpSmokeHoldCompletion, gpSmokeInjected;
        private bool gpSmokeActualOfferPlay, gpSmokeActualFeatureEnter, gpSmokeActualFeatureTake;
        private int gpSmokeChecks, gpSmokePockets, gpSmokeSuits, gpSmokeReplyAttempts;
        private int gpSmokeCompleteSpins, gpSmokeRecordPockets;

        private void RequireGpSmoke(bool accepted, string message)
        {
            if (!accepted) throw new InvalidOperationException("Game payment smoke " + gpSmokeCase + "/" +
                gpSmokePhase + ": " + message);
            ++gpSmokeChecks;
        }

        private void ConfigureGamePaymentSmokeHooks()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteGamePaymentSmoke") < 0) return;
            RequireGpSmoke(!gpSmokeConfigured, "hooks configured twice");
            gpSmokeConfigured = true;
            gpSmokeCase = RoundSmokeArgument("-rouletteGamePaymentCase");
            gpSmokePhase = RoundSmokeArgument("-rouletteGamePaymentPhase");
            bool extensionRecognized = false;
            ConfigureGamePaymentSmokeExtension(ref extensionRecognized);
            RequireGpSmoke(extensionRecognized || new[] { "take", "double_take", "double_loss", "zero", "rejected",
                "delayed_complete", "write_fail", "write_fail_double", "write_fail_publish", "write_fail_double_publish",
                "low_wallet_take", "low_wallet_double_take", "suit_take", "suit_loss",
                "pause_before", "pause_after_reply", "pause_after_apply", "staged" }
                .Contains(gpSmokeCase), "unsupported case");
            RequireGpSmoke(new[] { "live", "seed", "resume", "stable" }.Contains(gpSmokePhase), "unsupported phase");
            RequireGpSmoke(!temporaryHistory && recordedHistoryKey.Contains(".Test.") &&
                RoundSmokeArgument("-rouletteVolumeTestKey").StartsWith("RouletteDeluxeSZ.Volume.Test.", StringComparison.Ordinal) &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal),
                "requires isolated durable history, volume and receipt-backed RNG");
            string requested = RoundSmokeArgument("-rouletteGamePaymentOutput");
            RequireGpSmoke(Path.IsPathRooted(requested), "proof path must be absolute");
            gpSmokeOutput = Path.GetFullPath(requested);
            RequireGpSmoke(!File.Exists(gpSmokeOutput), "proof already exists");
            string requestedCapture = RoundSmokeArgument("-rouletteGamePaymentCapture", false);
            gpSmokeCapturePath = requestedCapture == "" ? Path.ChangeExtension(gpSmokeOutput, ".png") : requestedCapture;
            RequireGpSmoke(Path.IsPathRooted(gpSmokeCapturePath), "capture path must be absolute");
            gpSmokeCapturePath = Path.GetFullPath(gpSmokeCapturePath);
            RequireGpSmoke(!File.Exists(gpSmokeCapturePath), "final capture already exists");
            gpSmokeCut = RoundSmokeArgument("-rouletteGamePaymentCut", false);
            gpSmokeFailBefore = RoundSmokeArgument("-rouletteGamePaymentFailBefore", false);
            if (GpSmokeIsFaultCase && gpSmokePhase != "resume" && gpSmokePhase != "stable" && gpSmokeFailBefore == "")
                gpSmokeFailBefore = gpSmokeCase.EndsWith("_publish", StringComparison.Ordinal) ?
                    "GamePayment:HistoryPublishedSaved" : "GamePayment:MoneyAppliedSaved";
            if (gpSmokePhase == "seed") RequireGpSmoke(gpSmokeCut != "", "seed requires an exact checkpoint");
            string previousPath = RoundSmokeArgument("-rouletteGamePaymentPrevious", false);
            if (gpSmokePhase == "resume" || gpSmokePhase == "stable")
                RequireGpSmoke(previousPath != "", "cold phase requires previous proof");
            if (previousPath != "")
            {
                string text = File.ReadAllText(previousPath, Encoding.UTF8);
                gpSmokePreviousHash = RoundSmokeSha(text);
                gpSmokePrevious = JsonUtility.FromJson<GamePaymentSmokeProof>(text);
                RequireGpSmoke(gpSmokePrevious != null && gpSmokePrevious.schema == 2 &&
                    gpSmokePrevious.testCase == gpSmokeCase && gpSmokePrevious.historyKey == recordedHistoryKey &&
                    gpSmokePrevious.raw != null && gpSmokePrevious.rawSha256 ==
                        RoundSmokeSha(JsonUtility.ToJson(gpSmokePrevious.raw)), "previous proof identity or digest differs");
            }
            gpSmokeBoot = ReadRoundSmokeRaw();
            if (gpSmokePrevious != null)
                RequireGpSmoke(JsonUtility.ToJson(gpSmokeBoot) == JsonUtility.ToJson(gpSmokePrevious.raw),
                    "cold bytes differ from the previous closed proof");
            gpSmokeHoldCompletion = gpSmokeCase == "delayed_complete" && gpSmokePhase != "resume" && gpSmokePhase != "stable";
            ConfigureGamePaymentSmokeExpansion();
            pcLifecycleCheckpoint = GpSmokeCheckpoint;
            pcLifecycleBeforeWrite = GpSmokeBeforeWrite;
            pcAutoCompleteCheckpoint = point => GpSmokeCheckpoint("Auto:" + point);
            PcGameSuspensionEffect += GpSmokeSuspensionEvent;
            pcGamePaymentEventAllowed = GpSmokePaymentEventAllowed;
            pcGamePaymentReply = (operation, money) => {
                RequireGpSmoke(pcGamePayment != null && operation == pcGamePayment.operationId &&
                    money == checked(pcGamePayment.finalAward * pcGamePayment.denomination),
                    "request identity or money differs");
                ++gpSmokeReplyAttempts;
                gpSmokeReplyTimes.Add(Stopwatch.GetTimestamp());
                GpSmokeCheckpoint("ReplyAttempt");
                return gpSmokeCase == "rejected" && gpSmokePhase != "resume" && gpSmokePhase != "stable" &&
                    gpSmokeReplyAttempts <= 2 ? 0 : 1;
            };
            Debug.Log("ROULETTE_GAME_PAYMENT_BOOT: case=" + gpSmokeCase + " phase=" + gpSmokePhase +
                " raw=" + RoundSmokeSha(JsonUtility.ToJson(gpSmokeBoot)) + " previous=" +
                (gpSmokePreviousHash == "" ? "none" : gpSmokePreviousHash));
        }

        private PcOutcomeSource CreateGamePaymentSmokeOutcome()
        {
            if (!gpSmokeConfigured) return CreateGameSuspensionSmokeOutcome();
            uint[] snapshot = gpSmokePrevious == null ? new OriginalPlatformRngCore(5489).ExportSnapshot() : null;
            return PcOutcomeSource.CreateReceiptDiagnostic(snapshot, PcRngStateStore.PathForCurrentProcess);
        }

        private void AttachGamePaymentSmokeOutcome()
        {
            if (!gpSmokeConfigured) return;
            pcOutcome.ConfigureReceiptDiagnostics((point, receipt) => {
                if (point != "saved") return;
                if (receipt.context.kind == 1) ++gpSmokePockets; else ++gpSmokeSuits;
                GpSmokeCheckpoint(receipt.context.kind == 1 ? "RngPocketSaved" : "RngDoubleSaved");
            });
        }

        private void GpSmokeBeforeWrite(string point)
        {
            if (gpSmokeInjected || gpSmokeFailBefore == "" || point != gpSmokeFailBefore) return;
            gpSmokeInjected = true;
            gpSmokeFaultRaw = ReadRoundSmokeRaw();
            GpSmokeCheckpoint("Injected:" + point);
            throw new IOException("Game payment smoke injected before " + point);
        }

        private void GpSmokeCheckpoint(string point)
        {
            if (!gpSmokeConfigured || gpSmokeExiting) return;
            if (point == "CompleteSpin") ++gpSmokeCompleteSpins;
            if (point == "RecordPocket") ++gpSmokeRecordPockets;
            gpSmokeEvents.Add(GpSmokeSnapshot(point));
            if (point.StartsWith("GamePayment", StringComparison.Ordinal) && pcGamePayment != null &&
                !point.StartsWith("GamePaymentEntered", StringComparison.Ordinal))
            {
                pcGamePayment.Validate();
                RequireGpSmoke(session.Credit == pcGamePayment.creditBefore +
                    (pcGamePayment.moneyApplied ? pcGamePayment.finalAward : 0), "payment checkpoint has a second or missing wallet effect");
                RequireGpSmoke(CommittedHistoryCount() == Math.Min(50,
                    pcGamePayment.priorHistoryCount + (pcGamePayment.historyPublished ? 1 : 0)),
                    "payment checkpoint exposed or lost an unpublished history page");
                RequireGpSmoke(recordedRoundPages[0].awaitingFinalPayout == !pcGamePayment.historyPublished,
                    "page visibility disagrees with payment receipt");
                if (!pcGamePayment.historyPublished)
                    RequireGpSmoke(sourceRoundEndCount == 0, "End preceded history publication");
            }
            CheckGamePaymentSmokeExpansion(point);
            CheckGamePaymentSmokeExtension(point);
            if (gpSmokeCut != "" && point == gpSmokeCut) GpSmokeExit(point, "checkpoint");
        }

        private GamePaymentSmokeEvent GpSmokeSnapshot(string point)
        {
            return new GamePaymentSmokeEvent {
                point = point, at = Stopwatch.GetTimestamp(), frame = Time.frameCount,
                paymentJson = pcGamePayment == null ? "" : JsonUtility.ToJson(pcGamePayment),
                featureJson = pcDoubleFeatureNv == null ? "" : JsonUtility.ToJson(pcDoubleFeatureNv),
                receiptJson = pcOutcome == null || pcOutcome.Receipt == null ? "" : JsonUtility.ToJson(pcOutcome.Receipt),
                credit = session == null ? -1 : session.Credit, returned = session == null ? -1 : session.Returned,
                wagered = session == null ? -1 : session.Wagered, rounds = session == null ? -1 : session.Rounds,
                visibleHistory = CommittedHistoryCount(), physicalHistory = recordedRoundPages.Count,
                overflow = pcHistoryOverflow != null, pockets = gpSmokePockets, suits = gpSmokeSuits,
                replyAttempts = gpSmokeReplyAttempts, ends = sourceRoundEndCount,
                phase = (int)sourceRoundPhase, state = (int)sourceGameplayState,
                fault = pcRoundLifecycleStorageFaulted, app = PcGameAppSuspended,
                rack = winRackActive, resultDisplay = ResultDisplayActive,
                displayedCredit = session == null ? -1 : DisplayedCredit,
                displayedPaid = session == null ? -1 : DisplayedPaid,
                modelPending = session is RouletteSession && ((RouletteSession)session).RoundPayoutPending,
                gameSelected = pcAutoCompleteGameSelected, payoutEntered = pcGamePayoutEntered,
                screens = PcGameScreensVisible, offerPlayAllowed = session != null && SourceOfferPlayAllowed,
                controlOperationId = ""
            };
        }

        private void StartGamePaymentSmoke()
        {
            if (!gpSmokeConfigured) return;
            RequireGpSmoke(!gpSmokeStarted, "observer started twice");
            gpSmokeStarted = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var host = new GameObject("Game payment acceptance observer").AddComponent<RouletteGamePaymentSmokeHost>();
            host.StartCoroutine(GamePaymentSmoke());
        }

        private IEnumerator GamePaymentSmoke()
        {
            // Extensions own their normal and loader-fault entry contracts.
            if (gpSmokeExternalCase != null)
            {
                yield return gpSmokeExternalCase();
                yield break;
            }
            if (pcRoundLifecycleStorageFaulted)
            {
                RequireGpSmoke(GpSmokeIsFaultCase, "unexpected startup fault");
                yield return GpSmokeObserveFault();
                yield break;
            }
            RequireGpSmoke(session != null && sourceInputModule != null && pcOutcome != null &&
                !pcAutoPlayStorageFaulted && !ideckSecondDisplay && !topperDedicatedDisplay && !autoplay &&
                pcAutoCompleteGameSelected,
                "missing isolated inactive single-display runtime");
            if (gpSmokePrevious == null)
            {
                RequireGpSmoke(gpSmokeBoot.envelope == "" && gpSmokeBoot.pending == "" &&
                    session.Credit == 1000 && session.Rounds == 0 && recordedRoundPages.Count == 0,
                    "fresh smoke requires unused money and archive keys");
                yield return GpSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                    !configuredAutoPlayCheckPending && !sourceBetStateEntryPending, "initial Idle");
                if (GpSmokeIsLowWalletCase) yield return GpSmokeFundLowWallet();
                yield return GpSmokeSeedWheel();
            }
            else
            {
                RequireGpSmoke(gpSmokePockets == 0 && gpSmokeSuits == 0 && gameStartTimestamp == 0 &&
                    ReadRoundSmokeRaw().rng == gpSmokePrevious.raw.rng && pcGamePayment != null,
                    "cold continuation redrew or lost the payment");
                var prior = JsonUtility.FromJson<PcGamePaymentRecordV1>(gpSmokePrevious.paymentJson);
                RequireGpSmoke(prior != null && pcGamePayment.roundArchiveId == prior.roundArchiveId &&
                    pcGamePayment.spinJournalId == prior.spinJournalId && pcGamePayment.operationId == prior.operationId &&
                    pcGamePayment.creditBefore == prior.creditBefore && pcGamePayment.baseAward == prior.baseAward,
                    "cold continuation changed immutable payment identity");
                GpSmokeCheckpoint("ColdContinuation");
            }

            if (gpSmokePhase == "stable")
            {
                yield return GpSmokeStableReload();
                yield break;
            }
            gpSmokeExpectedSuits = GpSmokeIsDoubleCase && (pcDoubleFeatureNv == null ||
                pcDoubleFeatureNv.stepCount == 0 && pcDoubleFeatureNv.completionStage == 0) ? 1 : 0;

            if (pcGamePayment == null || !pcGamePayment.prepared)
            {
                if (GpSmokeIsDoubleCase)
                    yield return GpSmokeDriveFeature();
                else if (gpSmokeCase != "zero") yield return GpSmokeTakeBaseOffer();
            }
            yield return GpSmokeRunExpandedPaymentStages();
            if (GpSmokeIsFaultCase && gpSmokePhase != "resume")
            {
                yield return GpSmokeObserveFault();
                yield break;
            }
            if (gpSmokeHoldCompletion)
            {
                yield return GpSmokeWait(() => pcGamePayment != null && pcGamePayment.creditNotified &&
                    !pcGamePayment.completionReceived, "held completion after credit delivery");
                RequireGpSmoke(pcGamePayment.moneyApplied && session.Credit == GpSmokeExpectedCredit &&
                    !pcGamePayment.historyPublished && CommittedHistoryCount() == 0 && sourceRoundEndCount == 0,
                    "callback pending was treated as resolved payout");
                GpSmokeCheckpoint("CompletionHoldStarted");
                yield return GpSmokeHoldStage("credit_complete_pending", () =>
                    session.Credit == GpSmokeExpectedCredit && pcGamePayment.moneyApplied &&
                    pcGamePayment.creditNotified && !pcGamePayment.completionReceived &&
                    !pcGamePayment.historyPublished && sourceRoundEndCount == 0);
                GpSmokeCheckpoint("CompletionHeld");
                gpSmokeHoldCompletion = false;
            }
            yield return GpSmokeWait(() => pcGamePayment != null && pcGamePayment.historyPublished &&
                latestRoundLifecycle != null && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                latestRoundLifecycle.idleEntry != null && latestRoundLifecycle.idleEntry.state == PcIdleState.Complete &&
                sourceGameplayState == SourceGameplayState.Idle && !configuredAutoPlayCheckPending &&
                !ResultDisplayActive && !winRackActive && SourcePlayerInputs.Count == 0, "resolved payment and real End/Idle");
            RequireGpSmoke(pcGamePayment.IsResolved && session.Credit == pcGamePayment.creditBefore + pcGamePayment.finalAward &&
                CommittedHistoryCount() == 1 && recordedRoundPages.Count == 1 && !session.InDoubleUp,
                "terminal payment has wrong credit, owner or history");
            RequireGpSmoke(pcGamePayment.finalAward == GpSmokeExpectedAward && session.Credit == GpSmokeExpectedCredit &&
                pcGamePayment.owner == (GpSmokeIsDoubleCase ?
                    PcGamePaymentOwner.Double : PcGamePaymentOwner.Base), "terminal case award or owner differs");
            if (gpSmokePrevious == null)
                RequireGpSmoke(gpSmokePockets == 1 && gpSmokeCompleteSpins == 1 && gpSmokeRecordPockets == 1 &&
                    sourceRoundEndCount == 1 && gpSmokeSuits == (GpSmokeIsDoubleCase ? 1 : 0),
                    "live path redrew, settled, recorded or ended more than once");
            else RequireGpSmoke(gpSmokePockets == 0 && gpSmokeCompleteSpins == 0 && gpSmokeRecordPockets == 0 &&
                gpSmokeSuits == gpSmokeExpectedSuits && sourceRoundEndCount == 1,
                "cold path redrew a saved result, settled twice or lost End");
            if (gpSmokeCase == "rejected" && gpSmokePrevious == null)
            {
                RequireGpSmoke(gpSmokeReplyAttempts == 3 && gpSmokeReplyTimes.Count == 3, "reply retry count differs");
                for (int i = 1; i < gpSmokeReplyTimes.Count; ++i)
                    RequireGpSmoke(gpSmokeReplyTimes[i] >= OriginalRackClock.AfterMilliseconds(
                        gpSmokeReplyTimes[i - 1], 10, Stopwatch.Frequency), "reply zero retry skipped native 10ms service delay");
            }
            yield return GpSmokeHoldStage("published_idle", () =>
                session.Credit == GpSmokeExpectedCredit && pcGamePayment.IsResolved &&
                pcGamePayment.historyPublished && CommittedHistoryCount() == 1 &&
                sourceRoundEndCount == 1);
            yield return GpSmokeCapture("final", true);
            GpSmokeExit("PaymentStable", "natural");
        }

        private IEnumerator GpSmokeSeedWheel()
        {
            RefreshPortraitUi();
            DemoCleanupClick(uiChipSelections[0].GetComponent<Button>());
            yield return GpSmokeQueue(uiBetButtons[27], portraitCanvas, 610, "wager");
            yield return GpSmokeWait(() => session.TotalBet == 1 && sourceGameplayState == SourceGameplayState.Bet &&
                !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "real straight wager");
            if (gpSmokeCase != "zero") yield return StartGameChooseDemo(30);
            yield return GpSmokeQueue(uiSpinButton, portraitCanvas, 611, "PLAY commit");
            yield return GpSmokeWait(() => session.SpinPending && pendingSpin != null && gpSmokePockets == 1,
                "receipt-backed wheel commit");
            RequireGpSmoke(pcOutcome.Receipt != null && pcOutcome.Receipt.context.kind == 1 &&
                pcOutcome.Receipt.rawResult == 30 && pcOutcome.Receipt.effectiveResult == (gpSmokeCase == "zero" ? 30 : 27) &&
                session.Credit == GpSmokeCreditBefore && session.Rounds == 0, "golden real pocket receipt or committed wager differs");
            yield return GpSmokeWait(() => pcGamePayment != null && session.Rounds == 1, "unpaid round record");
            RequireGpSmoke(pcGamePayment.creditBefore == GpSmokeCreditBefore && !pcGamePayment.moneyApplied &&
                session.Credit == GpSmokeCreditBefore && CommittedHistoryCount() == 0 &&
                pcGamePayment.baseAward == (gpSmokeCase == "zero" ? 0 : 36),
                "CompleteSpin paid or published before StatePayout");
            GpSmokeCheckpoint("UnpaidRoundObserved");
        }

        private IEnumerator GpSmokeTakeBaseOffer()
        {
            if (pcGamePayment != null && pcGamePayment.baseOffer == PcGamePaymentOffer.Take) yield break;
            yield return GpSmokeWait(() => SourceDoubleOfferActive && !winRackActive && SourcePlayerInputs.Count == 0,
                "unpaid base offer");
            RequireGpSmoke(pcGamePayment.owner == PcGamePaymentOwner.Base && pcGamePayment.baseOffer == PcGamePaymentOffer.Open &&
                session.Credit == pcGamePayment.creditBefore && !pcGamePayment.moneyApplied && CommittedHistoryCount() == 0,
                "offer already paid or published");
            yield return GpSmokeHoldStage("unpaid_offer", () => SourceDoubleOfferActive &&
                !winRackActive && session.Credit == pcGamePayment.creditBefore && !pcGamePayment.moneyApplied &&
                CommittedHistoryCount() == 0 && SourceOfferPlayAllowed);
            int pockets = gpSmokePockets, suits = gpSmokeSuits;
            string rng = ReadRoundSmokeRaw().rng;
            yield return GpSmokeQueue(uiSpinButton, portraitCanvas, 612, "offer PLAY takes award");
            yield return GpSmokeWait(() => pcGamePayment.baseOffer == PcGamePaymentOffer.Take &&
                !session.DoubleAvailable && SourcePlayerInputs.Count == 0, "actual queued offer PLAY decision");
            gpSmokeActualOfferPlay = true;
            RequireGpSmoke(gpSmokePockets == pockets && gpSmokeSuits == suits && ReadRoundSmokeRaw().rng == rng,
                "offer PLAY started a second spin");
            GpSmokeCheckpoint("OfferPlayAccepted");
        }

        private IEnumerator GpSmokeDriveFeature()
        {
            if (pcGamePayment != null && pcGamePayment.owner == PcGamePaymentOwner.Base &&
                pcGamePayment.baseOffer == PcGamePaymentOffer.Enter)
            {
                yield return GpSmokeWait(() => pcGamePayment.owner == PcGamePaymentOwner.Double &&
                    session.InDoubleUp && DoubleDecisionReady, "cold selected Feature entry");
                GpSmokeCheckpoint("ColdFeatureEntryContinued");
            }
            if (pcGamePayment != null && pcGamePayment.owner == PcGamePaymentOwner.Base)
            {
                yield return GpSmokeWait(() => SourceDoubleOfferActive && !winRackActive && SourcePlayerInputs.Count == 0,
                    "unpaid Double offer");
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
                yield return GpSmokeQueue(ideckOfferDoubleUpButton, ideckCanvas, 613, "Double entry");
                yield return GpSmokeWait(() => session.InDoubleUp && DoubleDecisionReady && SourcePlayerInputs.Count == 0,
                    "real Feature Ready");
                gpSmokeActualFeatureEnter = true;
                RequireGpSmoke(pcGamePayment.owner == PcGamePaymentOwner.Double &&
                    pcGamePayment.baseOffer == PcGamePaymentOffer.Transferred && session.Credit == GpSmokeCreditBefore &&
                    !pcGamePayment.moneyApplied && pcDoubleFeatureNv.savedState == 2 && session.DoubleStake == 36,
                    "Double entry paid or debited the unpaid base stake");
                GpSmokeCheckpoint("FeatureEnteredUnpaid");
            }
            if (pcDoubleFeatureNv.completionStage > 0) yield break;
            if (pcDoubleFeatureNv.stepCount == 0)
            {
                yield return GpSmokeWait(() => DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0,
                    "real feature guess gate");
                if (GpSmokeIsLowWalletCase || GpSmokeIsSuitCase)
                    yield return GpSmokeHoldStage("feature_ready_unpaid", () => DoubleDecisionReady &&
                        !IsDoubleReveal && session.InDoubleUp && session.DoubleStake == 36 &&
                        session.Credit == GpSmokeCreditBefore && !pcGamePayment.moneyApplied && CommittedHistoryCount() == 0);
                bool lose = GpSmokeIsLossCase;
                Button guess = session.DoubleSuitMode ? uiSuitChoiceButtons[lose ? 0 : 2] :
                    lose ? uiDoubleRedButton : uiDoubleBlackButton;
                yield return GpSmokeQueue(guess, portraitCanvas, 614, "feature guess");
                yield return GpSmokeWait(() => session.DoubleRounds == 1 && gpSmokeSuits == 1, "one real suit receipt");
                RequireGpSmoke(pcOutcome.Receipt.rawResult == 0 && pcOutcome.Receipt.effectiveResult == 0 &&
                    ReadPcDoubleUnpaidContext(pcOutcome.Receipt) != null && session.Credit == GpSmokeCreditBefore &&
                    pcDoubleFeatureNv.savedState == 3 && !pcGamePayment.moneyApplied &&
                    session.DoubleStake == (lose ? 0 : GpSmokeExpectedAward), "deferred Double reveal changed wallet or its receipt");
                GpSmokeCheckpoint("FeatureRevealUnpaid");
                if (GpSmokeIsLowWalletCase || GpSmokeIsSuitCase)
                    yield return GpSmokeCapture("feature_reveal_unpaid");
            }
            if (GpSmokeIsLossCase) yield break;
            yield return GpSmokeWait(() => DoubleDecisionReady && !IsDoubleReveal && SourcePlayerInputs.Count == 0,
                "feature TAKE ready");
            string rngBefore = ReadRoundSmokeRaw().rng;
            int suitsBefore = gpSmokeSuits;
            if (session.DoubleSuitMode) SetIDeckPreview(true);
            yield return GpSmokeQueue(session.DoubleSuitMode ? ideckSuitTakeWin : uiDoubleTakeWinButton,
                session.DoubleSuitMode ? ideckCanvas : portraitCanvas, 615, "feature TAKE");
            yield return GpSmokeWait(() => pcDoubleFeatureNv.savedState == 5 && pcDoubleFeature.TimerRunning &&
                !DoubleDecisionReady && SourcePlayerInputs.Count == 0, "actual TAKE saved5");
            gpSmokeActualFeatureTake = true;
            RequireGpSmoke(gpSmokeSuits == suitsBefore && ReadRoundSmokeRaw().rng == rngBefore &&
                session.Credit == GpSmokeCreditBefore && !pcGamePayment.moneyApplied, "TAKE redrew or settled before Game payout");
            GpSmokeCheckpoint("FeatureTakeAccepted");
        }

        private IEnumerator GpSmokeQueue(Button target, Canvas expectedCanvas, int pointerId, string action)
        {
            RefreshPortraitUi(); RefreshDiagnosticUi();
            yield return null;
            RefreshPortraitUi(); RefreshDiagnosticUi(); Canvas.ForceUpdateCanvases();
            RequireGpSmoke(PcGameScreensVisible && expectedCanvas != null && expectedCanvas.enabled &&
                expectedCanvas.gameObject.activeInHierarchy && target != null && target.IsActive() && target.IsInteractable() &&
                SourcePlayerInputs.Count == 0, action + " target or Canvas unavailable");
            Vector2 point = StartGameButtonPoint(target);
            var pointer = new PointerEventData(EventSystem.current) {
                position = point, button = PointerEventData.InputButton.Left
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            RequireGpSmoke(hits.Count > 0 && hits[0].gameObject == target.gameObject,
                action + " raycast hits another target");
            GpSmokeCheckpoint("Queued:" + action);
            QueueStartGameButton(target, pointerId, true);
        }

        private IEnumerator GpSmokeWait(Func<bool> ready, string description, float timeout = 90f)
        {
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + timeout;
            while (!ready() && !pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline) yield return null;
            sourceInputModule.enabled = false;
            RequireGpSmoke(ready() && !pcRoundLifecycleStorageFaulted, "live wait failed: " + description);
        }

        private IEnumerator GpSmokeFrames(int count)
        {
            sourceInputModule.enabled = true;
            for (int i = 0; i < count; ++i) yield return null;
            sourceInputModule.enabled = false;
        }

        private IEnumerator GpSmokeObserveFault()
        {
            if (!pcRoundLifecycleStorageFaulted)
            {
                sourceInputModule.enabled = true;
                float deadline = Time.realtimeSinceStartup + 90f;
                while (!pcRoundLifecycleStorageFaulted && Time.realtimeSinceStartup < deadline) yield return null;
            }
            RequireGpSmoke(pcRoundLifecycleStorageFaulted && pcAutoPlayStorageFaulted && !autoplay &&
                !sourceTableInputEnabled && !configuredAutoPlayCheckPending &&
                (sourceInputModule == null || !sourceInputModule.enabled), "write failure did not stop ordinary dispatch");
            RequireGpSmoke(gpSmokeFaultRaw != null && JsonUtility.ToJson(ReadRoundSmokeRaw()) == JsonUtility.ToJson(gpSmokeFaultRaw),
                "failed write changed authoritative persistent bytes");
            RequireGpSmoke(session != null && DisplayedCredit == session.Credit,
                "faulted display projected the applied award a second time");
            GpSmokeCheckpoint("FaultStopped");
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
            int pockets = gpSmokePockets, suits = gpSmokeSuits, ends = sourceRoundEndCount;
            long holdBegin = Stopwatch.GetTimestamp();
            int beginFrame = Time.frameCount;
            while (OriginalRackClock.ElapsedMillisecondsLong(holdBegin, Stopwatch.GetTimestamp(), Stopwatch.Frequency) < 1200 ||
                Time.frameCount - beginFrame < 2)
            {
                yield return null;
                RequireGpSmoke(JsonUtility.ToJson(ReadRoundSmokeRaw()) == raw && gpSmokePockets == pockets &&
                    gpSmokeSuits == suits && sourceRoundEndCount == ends &&
                    DisplayedCredit == session.Credit &&
                    (portraitCanvas == null || !portraitCanvas.gameObject.activeSelf), "faulted frame continued a business effect");
            }
            gpSmokeHolds.Add(new GamePaymentSmokeHold { stage = "fault_stopped", beginAt = holdBegin,
                endAt = Stopwatch.GetTimestamp(), beginFrame = beginFrame, endFrame = Time.frameCount,
                rawBeginSha256 = RoundSmokeSha(raw), rawEndSha256 = RoundSmokeSha(JsonUtility.ToJson(ReadRoundSmokeRaw())),
                rawStableRequired = true });
            yield return GpSmokeCapture("fault_stopped", true);
            GpSmokeExit("ExpectedWriteFault", "expected_fault");
        }

        private void GpSmokeExit(string checkpoint, string mode)
        {
            RequireGpSmoke(!gpSmokeExiting, "duplicate proof exit");
            gpSmokeExiting = true;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var proof = new GamePaymentSmokeProof {
                testCase = gpSmokeCase, phase = gpSmokePhase, checkpoint = checkpoint, exitMode = mode,
                historyKey = recordedHistoryKey, rngPath = PcRngStateStore.PathForCurrentProcess,
                autoPath = PcAutoPlayStatePath, previousProofSha256 = gpSmokePreviousHash,
                raw = ReadRoundSmokeRaw(), paymentJson = pcGamePayment == null ? "" : JsonUtility.ToJson(pcGamePayment),
                featureJson = pcDoubleFeatureNv == null ? "" : JsonUtility.ToJson(pcDoubleFeatureNv),
                lifecycleJson = latestRoundLifecycle == null ? "" : JsonUtility.ToJson(latestRoundLifecycle),
                rngReceiptJson = pcOutcome == null || pcOutcome.Receipt == null ? "" : JsonUtility.ToJson(pcOutcome.Receipt),
                credit = session == null ? -1 : session.Credit, returned = session == null ? -1 : session.Returned,
                wagered = session == null ? -1 : session.Wagered, rounds = session == null ? -1 : session.Rounds,
                wins = session == null ? -1 : session.Wins, denomination = session == null ? -1 : session.Denomination,
                remainder = session is RouletteSession ? ((RouletteSession)session).MoneyRemainderCents : -1,
                expectedInitialCredit = GpSmokeInitialCredit, expectedBaseAward = gpSmokeCase == "zero" ? 0 : 36,
                expectedFinalAward = GpSmokeExpectedAward,
                checks = gpSmokeChecks, pockets = gpSmokePockets, suits = gpSmokeSuits, replyAttempts = gpSmokeReplyAttempts,
                completeSpins = gpSmokeCompleteSpins, recordPockets = gpSmokeRecordPockets, ends = sourceRoundEndCount,
                physicalHistory = recordedRoundPages.Count, visibleHistory = CommittedHistoryCount(), archiveSchema = pcRoundArchiveSchemaVersion,
                fault = pcRoundLifecycleStorageFaulted, faultOperation = pcAutoPlayStorageFaultOperation ?? "",
                overflow = pcHistoryOverflow != null, actualOfferPlay = gpSmokeActualOfferPlay,
                actualFeatureEnter = gpSmokeActualFeatureEnter, actualFeatureTake = gpSmokeActualFeatureTake,
                gameSelected = pcAutoCompleteGameSelected, payoutEntered = pcGamePayoutEntered,
                screens = PcGameScreensVisible, stageCaptures = gpSmokeCaptures.ToArray(),
                holds = gpSmokeHolds.ToArray(), fundingEvents = gpSmokeFunding.ToArray(),
                events = gpSmokeEvents.ToArray(), frequency = Stopwatch.Frequency, replyTimes = gpSmokeReplyTimes.ToArray(),
                bootRawJson = JsonUtility.ToJson(gpSmokeBoot)
            };
            proof.rawJson = JsonUtility.ToJson(proof.raw);
            proof.rawSha256 = RoundSmokeSha(proof.rawJson);
            proof.bootRawSha256 = RoundSmokeSha(proof.bootRawJson);
            FillGamePaymentSmokeExtensionProof(proof);
            string parent = Path.GetDirectoryName(gpSmokeOutput);
            if (!string.IsNullOrEmpty(parent)) Directory.CreateDirectory(parent);
            string encoded = JsonUtility.ToJson(proof, true) + "\n";
            using (var stream = new FileStream(gpSmokeOutput, FileMode.CreateNew, FileAccess.Write,
                FileShare.Read, 4096, FileOptions.WriteThrough))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(encoded);
                stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
            }
            Debug.Log("ROULETTE_GAME_PAYMENT_PASSED: case=" + gpSmokeCase + " phase=" + gpSmokePhase +
                " checkpoint=" + checkpoint + " mode=" + mode + " checks=" + gpSmokeChecks +
                " credit=" + proof.credit + " pockets=" + gpSmokePockets + " suits=" + gpSmokeSuits +
                " receipt=" + RoundSmokeSha(encoded));
            Debug.Log("ROULETTE_GAME_PAYMENT_EXIT: api=" + (mode == "checkpoint" ? "TerminateProcess" : "Application.Quit") +
                " code=0 mode=" + mode);
            if (mode == "checkpoint")
            {
                RequireGpSmoke(gpSmokeConfigured && recordedHistoryKey.Contains(".Test.") &&
                    Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteGamePaymentSmoke") >= 0,
                    "current-process cut lacks explicit isolated smoke authorization");
                TerminateProcess(GetCurrentProcess(), 0);
                throw new InvalidOperationException("Game payment current-process cut returned");
            }
            Application.Quit(0);
        }
    }
}
