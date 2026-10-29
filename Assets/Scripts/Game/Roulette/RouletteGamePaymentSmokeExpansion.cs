using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private bool gpSmokePauseQueued;
        private string gpSmokePauseOperation = "";

        private void ConfigureGamePaymentSmokeExpansion()
        {
            bool pause = gpSmokeCase.StartsWith("pause_", StringComparison.Ordinal);
            if (pause || gpSmokeCase == "staged")
                RequireGpSmoke(gpSmokePhase == "live", "App stage cases currently require a live process");
            if (GpSmokeIsSuitCase)
                RequireGpSmoke(Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteSuitMode") >= 0,
                    "Suit case requires its actual configured Suit branch");
            if (gpSmokeCase == "pause_after_reply") gpSmokeBlockedEvent = "Apply";
            else if (gpSmokeCase == "pause_after_apply") gpSmokeBlockedEvent = "Credit";
            else if (gpSmokeCase == "staged") gpSmokeBlockedEvent = "Reply";
        }

        private void CheckGamePaymentSmokeExpansion(string point)
        {
            // The real TAKE down queues a separate up. Wait until that up has
            // drained before suspending ordinary input, while saved5's native
            // timer is still pending. The driver records this verified boundary.
            if (gpSmokeCase == "pause_before" && point == "FeatureTakeAccepted" && !gpSmokePauseQueued)
            {
                RequireGpSmoke(!pcGamePayoutEntered && pcGamePayment != null && !pcGamePayment.prepared &&
                    pcGamePayment.owner == PcGamePaymentOwner.Double && pcDoubleFeatureNv != null &&
                    pcDoubleFeatureNv.savedState == 5 && pcDoubleFeatureNv.completionStage == 0,
                    "before-payment pause did not start at the real Feature TAKE state");
                GpSmokeRequestPause();
            }
        }

        private void GpSmokeRequestPause()
        {
            RequireGpSmoke(!gpSmokePauseQueued && !PcGameAppSuspended && pcAutoCompleteGameSelected,
                "App pause is duplicate or has no selected Game");
            gpSmokePauseQueued = true;
            gpSmokePauseOperation = "game-payment-smoke:" + gpSmokeCase + ":suspend";
            RequestPcGameSuspend(false, gpSmokePauseOperation);
            GamePaymentSmokeEvent queued = GpSmokeSnapshot("AppSuspendQueued");
            queued.controlOperationId = gpSmokePauseOperation;
            gpSmokeEvents.Add(queued);
        }

        private IEnumerator GpSmokeWaitForPause()
        {
            yield return GpSmokeWait(() => PcGameAppSuspended, "actual queued App suspension");
            RequireGpSmoke(pcAutoCompleteGameSelected && gpSmokeEvents.Any(item =>
                item.point == "Suspension:GameSuspendExecuted" && item.controlOperationId == gpSmokePauseOperation &&
                item.app && item.gameSelected), "App suspension has no executed selected-Game event");
        }

        private IEnumerator GpSmokeResumeApp()
        {
            RequireGpSmoke(PcGameAppSuspended && gpSmokePauseQueued && pcAutoCompleteGameSelected,
                "App resume has no observed suspension");
            string operation = "game-payment-smoke:" + gpSmokeCase + ":resume";
            RequestPcGameResume(operation);
            yield return GpSmokeWait(() => !PcGameAppSuspended, "actual queued App resume");
            RequireGpSmoke(gpSmokeEvents.Any(item => item.point == "Suspension:GameResumeExecuted" &&
                item.controlOperationId == operation && !item.app && item.gameSelected),
                "App resume has no executed selected-Game event");
        }

        private IEnumerator GpSmokeRunExpandedPaymentStages()
        {
            if (gpSmokeCase == "pause_before")
            {
                RequireGpSmoke(gpSmokePauseQueued, "before-payment case did not queue suspend at the saved Feature TAKE");
                yield return GpSmokeWaitForPause();
                RequireGpSmoke(!pcGamePayoutEntered && !pcGamePayment.prepared,
                    "App suspension arrived after the first StatePayout entry");
                yield return GpSmokeWait(() => pcDoubleFeatureNv != null && pcDoubleFeatureNv.savedState == 0 &&
                    pcDoubleFeatureNv.completionStage == 1 && sourceRoundPhase == SourceRoundPhase.ResultDisplay &&
                    !ResultDisplayActive, "selected suspended Game received native Feature completion");
                RequireGpSmoke(!pcGamePayoutEntered && !pcGamePayment.prepared &&
                    pcDoubleFeatureNv.completionAward == 72, "Feature completion bypassed the first StatePayout App gate");
                yield return GpSmokeHoldStage("app_before_payment", () => PcGameAppSuspended &&
                    pcAutoCompleteGameSelected && !pcGamePayoutEntered && !pcGamePayment.prepared &&
                    pcDoubleFeatureNv.savedState == 0 && pcDoubleFeatureNv.completionStage == 1 &&
                    !ResultDisplayActive && session.Credit == GpSmokeCreditBefore && !pcGamePayment.historyPublished && sourceRoundEndCount == 0);
                yield return GpSmokeResumeApp();
                gpSmokeBlockedEvent = "";
                yield break;
            }
            if (gpSmokeCase == "pause_after_reply" || gpSmokeCase == "pause_after_apply")
            {
                bool applied = gpSmokeCase == "pause_after_apply";
                yield return GpSmokeWait(() => pcGamePayment.prepared && pcGamePayment.replyValue != 0 &&
                    pcGamePayment.moneyApplied == applied && !pcGamePayment.creditNotified &&
                    !pcGamePayment.completionReceived, applied ? "applied stage before App pause" : "accepted reply before App pause");
                RequireGpSmoke(pcGamePayoutEntered, "continuation case never entered StatePayout");
                GpSmokeRequestPause();
                yield return GpSmokeWaitForPause();
                gpSmokeBlockedEvent = "";
                yield return GpSmokeWait(() => pcGamePayment.IsResolved && !pcGamePayment.historyPublished,
                    "payment continuation while selected Game is suspended");
                yield return GpSmokeHoldStage("app_completed_unpublished", () => PcGameAppSuspended &&
                    pcAutoCompleteGameSelected && pcGamePayoutEntered && pcGamePayment.IsResolved &&
                    !pcGamePayment.historyPublished && session.Credit == GpSmokeExpectedCredit &&
                    DisplayedCredit == session.Credit && sourceRoundEndCount == 0);
                yield return GpSmokeResumeApp();
                yield break;
            }
            if (gpSmokeCase != "staged") yield break;

            yield return GpSmokeWait(() => pcGamePayment.prepared && pcGamePayment.replyValue == 0 &&
                !pcGamePayment.moneyApplied, "durable intent before request reply");
            yield return GpSmokeHoldStage("reply_pending", () => pcGamePayoutEntered && pcGamePayment.prepared &&
                pcGamePayment.replyValue == 0 && !pcGamePayment.moneyApplied && session.Credit == GpSmokeCreditBefore &&
                !pcGamePayment.historyPublished && sourceRoundEndCount == 0);

            gpSmokeBlockedEvent = "Apply";
            yield return GpSmokeWait(() => pcGamePayment.replyValue != 0 && !pcGamePayment.moneyApplied,
                "accepted reply with unpaid wallet");
            yield return GpSmokeHoldStage("accepted_reply", () => pcGamePayment.replyValue != 0 &&
                !pcGamePayment.moneyApplied && !pcGamePayment.creditNotified && !pcGamePayment.completionReceived &&
                session.Credit == GpSmokeCreditBefore && !pcGamePayment.historyPublished && sourceRoundEndCount == 0);

            gpSmokeBlockedEvent = "Credit";
            yield return GpSmokeWait(() => pcGamePayment.moneyApplied && !pcGamePayment.creditNotified,
                "applied wallet before credit delivery");
            yield return GpSmokeHoldStage("applied_credit_pending", () => pcGamePayment.moneyApplied &&
                !pcGamePayment.creditNotified && !pcGamePayment.completionReceived &&
                session.Credit == GpSmokeExpectedCredit && DisplayedCredit == session.Credit &&
                !pcGamePayment.historyPublished && sourceRoundEndCount == 0);

            gpSmokeBlockedEvent = "Complete";
            yield return GpSmokeWait(() => pcGamePayment.creditNotified && !pcGamePayment.completionReceived,
                "credit delivery before completion callback");
            yield return GpSmokeHoldStage("credit_complete_pending", () => pcGamePayment.moneyApplied &&
                pcGamePayment.creditNotified && !pcGamePayment.completionReceived &&
                session.Credit == GpSmokeExpectedCredit && DisplayedCredit == session.Credit &&
                !pcGamePayment.historyPublished && sourceRoundEndCount == 0);

            GpSmokeRequestPause();
            yield return GpSmokeWaitForPause();
            gpSmokeBlockedEvent = "";
            yield return GpSmokeWait(() => pcGamePayment.IsResolved && !pcGamePayment.historyPublished,
                "resolved payment before history publication");
            yield return GpSmokeHoldStage("completed_unpublished", () => PcGameAppSuspended && pcAutoCompleteGameSelected &&
                pcGamePayoutEntered && pcGamePayment.IsResolved && !pcGamePayment.historyPublished &&
                session.Credit == GpSmokeExpectedCredit && DisplayedCredit == session.Credit && sourceRoundEndCount == 0);
            yield return GpSmokeResumeApp();
        }

        private void GpSmokeRecordFunding(string point)
        {
            PcCreditEvent receipt = ReadPcCreditEvent();
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
            RequireGpSmoke(receipt != null, "funding observation has no authoritative credit receipt");
            gpSmokeFunding.Add(new GamePaymentSmokeFunding { point = point,
                receiptJson = JsonUtility.ToJson(receipt), rawJson = raw, rawSha256 = RoundSmokeSha(raw),
                at = Stopwatch.GetTimestamp(), frame = Time.frameCount, credit = session.Credit });
            GpSmokeCheckpoint("Funding:" + point);
        }

        private IEnumerator GpSmokeFundLowWallet()
        {
            RequireGpSmoke(GpSmokeIsLowWalletCase && gpSmokePrevious == null && session.Credit == 1000 &&
                session.Rounds == 0 && session.TotalBet == 0 && !session.SpinPending && !session.InDoubleUp &&
                recordedRoundPages.Count == 0 && !configuredAutoPlayCheckPending && !pcCollectPending,
                "low-wallet setup is not an unused Idle money state");
            IRouletteSession originalSession = session;
            sourceInputModule.enabled = true;
            RequestPcCollect();
            PcCreditEvent collect = ReadPcCreditEvent();
            RequireGpSmoke(pcCollectPending && collect != null && collect.kind == "collect" && collect.phase == 1 &&
                collect.before == 1000 && collect.after == 0 && collect.collected == 1000 && collect.collectedCents == 1000 &&
                session.Credit == 1000, "real collect request did not preserve its phase1 wallet");
            string collectId = collect.id;
            GpSmokeRecordFunding("collect_requested");
            int seenPhase = 1;
            float deadline = Time.realtimeSinceStartup + 20;
            while (pcCollectPending && Time.realtimeSinceStartup < deadline)
            {
                // End-of-frame observes each existing collect coroutine step;
                // the setup does not call its internal apply/completion code.
                yield return new WaitForEndOfFrame();
                collect = ReadPcCreditEvent();
                RequireGpSmoke(collect != null && collect.id == collectId && collect.kind == "collect" &&
                    collect.before == 1000 && collect.after == 0 && collect.collected == 1000 &&
                    ReferenceEquals(originalSession, session), "collect replaced its money operation or session");
                if (collect.phase != seenPhase)
                {
                    RequireGpSmoke(collect.phase == seenPhase + 1 && session.Credit == 0,
                        "collect observation skipped a phase or lost its wallet change");
                    seenPhase = collect.phase;
                    GpSmokeRecordFunding(seenPhase == 2 ? "collect_credit_changed" : "collect_completed");
                }
            }
            sourceInputModule.enabled = false;
            RequireGpSmoke(!pcCollectPending && seenPhase == 3 && session.Credit == 0,
                "real collect did not complete all three observed phases");
            yield return GpSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                !configuredAutoPlayCheckPending && !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0,
                "real Idle after collect");
            AddPcVirtualCredits(1);
            PcCreditEvent deposit = ReadPcCreditEvent();
            RequireGpSmoke(deposit != null && deposit.kind == "deposit" && deposit.phase == 3 && deposit.id != collectId &&
                deposit.before == 0 && deposit.after == 1 && deposit.collected == 0 && session.Credit == 1 &&
                session.Denomination == 1 && ((RouletteSession)session).MoneyRemainderCents == 0 &&
                session.Rounds == 0 && session.Wagered == 0 && session.Returned == 0 &&
                ReferenceEquals(originalSession, session) && gpSmokePockets == 0 && gpSmokeSuits == 0 &&
                sourceRoundEndCount == 0, "real deposit1 did not create exactly the intended new funding");
            GpSmokeRecordFunding("deposit_completed");
        }
    }
}
