using System;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Runtime entry is distinct from durable request intent. A cold process
        // must cross RunLoop's App gate again before servicing the saved intent.
        private bool pcGamePayoutEntered;
        private bool pcPaymentResumeEnter;
        private long pcGamePaymentRetryDue;
        private int pcGamePaymentEventFrame = -1;
        private Func<string, bool> pcGamePaymentEventAllowed;
        private Func<string, long, int> pcGamePaymentReply;

        private bool PcPaymentTracked { get { return pcRoundArchiveSchemaVersion == 3 && pcGamePayment != null; } }
        private bool PcCurrentResultHistoryUnpublished { get { return PcPaymentTracked ? !pcGamePayment.historyPublished :
            pcRoundArchiveSchemaVersion==4&&pcSasGamePaymentV2!=null&&!pcSasGamePaymentV2.historyPublished; } }
        private bool PcUnpaidBaseProjection { get { return (PcPaymentTracked ? !pcGamePayment.moneyApplied&&pcGamePayment.owner==PcGamePaymentOwner.Base :
            pcRoundArchiveSchemaVersion==4&&pcSasGamePaymentV2!=null&&!pcSasGamePaymentV2.moneyApplied&&pcSasGamePaymentV2.owner==PcGamePaymentOwner.Base) && session is RouletteSession &&
            ((RouletteSession)session).RoundPayoutPending && session.ResultPhase; } }
        private long PcUnpaidBaseProjectionAward { get { return pcRoundArchiveSchemaVersion==4?pcSasGamePaymentV2.baseAward:pcGamePayment.baseAward; } }

        private static void ValidatePcGamePayment(PcGamePaymentRecordV1 payment, RecordedRound round,
            PcRoundLifecycleRecord lifecycle, PcDoubleFeatureNvV1 feature)
        {
            RequireLifecycle(payment != null && round != null && lifecycle != null,
                "Payment has no current round");
            payment.Validate();
            RequireLifecycle(payment.roundArchiveId == round.roundArchiveId &&
                payment.spinJournalId == round.spinJournalId && payment.profile == round.profile &&
                payment.roundArchiveId == lifecycle.roundArchiveId &&
                payment.spinJournalId == lifecycle.spinJournalId && payment.profile == lifecycle.profile &&
                payment.denomination == round.denomination && payment.remainderCents == round.moneyRemainderCents,
                "Payment identity or denomination differs from its round");
            RequireLifecycle((decimal)round.creditAfter == (decimal)payment.creditBefore +
                (payment.moneyApplied ? payment.finalAward : 0) &&
                round.awaitingFinalPayout == !payment.historyPublished &&
                (lifecycle.roundStage == PcRoundStage.RoundOpen || payment.historyPublished),
                "Payment wallet, history or lifecycle boundary differs");
            if (payment.owner == PcGamePaymentOwner.Base)
            {
                RequireLifecycle(feature == null && !round.hasDoubleUp && IsEmptyRecordedDouble(round.doubleUp) &&
                    round.returned == payment.baseAward,
                    "Ordinary payment conflicts with a feature or base award");
            }
            else
            {
                RequireLifecycle(feature != null && round.hasDoubleUp && round.doubleUp != null &&
                    payment.featureId == feature.featureId && payment.baseAward == round.doubleUp.startingStake &&
                    payment.moneyApplied == (feature.completionStage == 2) &&
                    (!payment.prepared || feature.completionStage >= 1 && feature.savedState == 0 &&
                        payment.finalAward == feature.completionAward) &&
                    round.returned == (payment.moneyApplied ? payment.finalAward : payment.baseAward),
                    "Feature payment owner, final award or application marker differs");
            }
        }

        private RecordedHistoryOverflowV1 CapturePcPaymentHistoryOverflow(bool trackLifecycle)
        {
            var local = session as RouletteSession;
            if (!trackLifecycle || local == null || !local.RoundPayoutPending) return null;
            RequireLifecycle(recordedRoundPages.Count == recordedSnapshots.Count &&
                recordedRoundPages.Count <= 50, "Prepayment history pairs differ");
            if (recordedRoundPages.Count < 50) return null;
            return new RecordedHistoryOverflowV1 { round = recordedRoundPages[49],
                snapshotBase64 = Convert.ToBase64String(recordedSnapshots[49]) };
        }

        private void CreatePcGamePayment(RecordedRound round, IRouletteSession source,
            RecordedHistoryOverflowV1 overflow)
        {
            var local = source as RouletteSession;
            RequireLifecycle(local != null && local.RoundPayoutPending && pendingSpin != null,
                "New payment requires an unpaid model and settled spin");
            pcGamePayment = new PcGamePaymentRecordV1 {
                version = 1, roundArchiveId = round.roundArchiveId, spinJournalId = round.spinJournalId,
                profile = round.profile, operationId = round.roundArchiveId + ":game-payout",
                priorHistoryCount = pendingSpin.baseHistoryCount, creditBefore = source.Credit,
                denomination = source.Denomination, remainderCents = local.MoneyRemainderCents,
                baseAward = round.returned, owner = PcGamePaymentOwner.Base, featureId = "",
                baseOffer = source.DoubleAvailable && DoubleUpInputAllowed ?
                    PcGamePaymentOffer.Open : PcGamePaymentOffer.None
            };
            RequireLifecycle((decimal)pcGamePayment.creditBefore == (decimal)pendingSpin.creditBefore - round.wager,
                "Unpaid wallet differs from the settled spin");
            pcHistoryOverflow = overflow;
            pcRoundArchiveSchemaVersion = 3;
            round.awaitingFinalPayout = true;
            pcGamePayoutEntered = false;
            pcPaymentResumeEnter = false;
            pcGamePaymentRetryDue = 0;
            pcGamePaymentEventFrame = -1;
        }

        private void SavePcGamePayment(PcGamePaymentRecordV1 candidate, string boundary)
        {
            PcGamePaymentRecordV1 prior = pcGamePayment;
            try
            {
                candidate.Validate();
                pcGamePayment = candidate;
                LifecycleBeforeWrite("GamePayment:" + boundary);
                SaveRecordedRoundPages();
            }
            catch (Exception error)
            {
                pcGamePayment = prior;
                FailPcRoundLifecycle("game-payment:" + boundary, error);
                throw;
            }
            LifecycleCheckpoint("GamePayment" + boundary);
        }

        private void PersistPcGameOffer(bool enter)
        {
            if(pcRoundArchiveSchemaVersion==4){PersistSasBaseOfferChoiceV4(enter);return;}
            if (!PcPaymentTracked) return;
            SavePcGamePayment(enter ? PcGamePaymentTransitions.OfferEntered(pcGamePayment) :
                PcGamePaymentTransitions.OfferTaken(pcGamePayment), enter ? "OfferEnterSaved" : "OfferTakeSaved");
        }

        private void ResolvePcGameOfferEligibility()
        {
            if (!PcPaymentTracked || pcGamePayment.owner != PcGamePaymentOwner.Base ||
                pcGamePayment.baseOffer != PcGamePaymentOffer.Open || session.DoubleAvailable) return;
            PersistPcGameOffer(false);
        }

        private bool ServicePcGamePayment()
        {
            RequireLifecycle(PcPaymentTracked, "Game payment service has no payment");
            if (pcGamePayment.historyPublished) return true;
            if (!pcGamePayoutEntered)
            {
                if (PcGameAppSuspended) return false;
                pcGamePayoutEntered = true;
                LifecycleCheckpoint("GamePaymentEntered");
            }
            // CoreUpdate can reach the service more than once in a frame.
            // Publication is a separate adapter event too.
            if (pcGamePaymentEventFrame == Time.frameCount) return false;
            if (pcGamePayment.IsResolved)
            {
                if (PcGameAppSuspended) return false;
                pcGamePaymentEventFrame = Time.frameCount;
                PublishPcGamePaymentHistory();
                return true;
            }
            // The local adapter delivers one observable event per Unity frame.
            // Only a rejected request adds the native serviced 10 ms retry.
            string next = !pcGamePayment.prepared ? "Prepare" :
                pcGamePayment.finalAward == 0 ? (!pcGamePayment.moneyApplied ? "Apply" : "Zero") :
                pcGamePayment.replyValue == 0 ? "Reply" :
                !pcGamePayment.moneyApplied ? "Apply" : !pcGamePayment.creditNotified ? "Credit" : "Complete";
            if (pcGamePaymentEventAllowed != null && !pcGamePaymentEventAllowed(next)) return false;
            if (next == "Reply" && Stopwatch.GetTimestamp() < pcGamePaymentRetryDue) return false;
            pcGamePaymentEventFrame = Time.frameCount;
            if (next == "Prepare")
                SavePcGamePayment(PcGamePaymentTransitions.Prepare(pcGamePayment,
                    pcGamePayment.owner == PcGamePaymentOwner.Base ? pcGamePayment.baseAward :
                        pcDoubleFeatureNv.completionAward), "PreparedSaved");
            else if (next == "Reply")
            {
                int reply = pcGamePaymentReply == null ? 1 :
                    pcGamePaymentReply(pcGamePayment.operationId, checked(pcGamePayment.finalAward * pcGamePayment.denomination));
                SavePcGamePayment(PcGamePaymentTransitions.Reply(pcGamePayment, reply), "ReplySaved");
                pcGamePaymentRetryDue = reply == 0 ? OriginalRackClock.AfterMilliseconds(
                    Stopwatch.GetTimestamp(), 10, Stopwatch.Frequency) : 0;
            }
            else if (next == "Apply") ApplyPcGamePaymentMoney();
            else if (next == "Credit")
                SavePcGamePayment(PcGamePaymentTransitions.CreditNotified(pcGamePayment), "CreditNotifiedSaved");
            else if (next == "Complete")
                SavePcGamePayment(PcGamePaymentTransitions.Completed(pcGamePayment), "CompletionSaved");
            else SavePcGamePayment(PcGamePaymentTransitions.ResolveZero(pcGamePayment), "ZeroResolvedSaved");
            return false;
        }

        private void ApplyPcGamePaymentMoney()
        {
            RequireLifecycle(pcGamePayoutEntered && PcPaymentTracked && pcGamePayment.prepared &&
                !pcGamePayment.moneyApplied, "Game money application has no prepared owner");
            if (pcGamePayment.owner == PcGamePaymentOwner.Double)
            {
                ApplyPcDoubleGamePayout();
                return;
            }
            try
            {
                ((RouletteSession)session).CompleteBaseGamePayout(pcGamePayment.finalAward);
                recordedRoundPages[0].creditAfter = session.Credit;
                SavePcGamePayment(PcGamePaymentTransitions.Applied(pcGamePayment), "MoneyAppliedSaved");
            }
            catch (Exception error) { FailPcRoundLifecycle("game-payment:apply", error); throw; }
        }

        private void PublishPcGamePaymentHistory()
        {
            RequireLifecycle(!PcGameAppSuspended && pcGamePayment.IsResolved &&
                !pcGamePayment.historyPublished, "History publication precedes Game payout completion");
            RecordedRound round = recordedRoundPages[0];
            RecordedHistoryOverflowV1 priorOverflow = pcHistoryOverflow;
            try
            {
                round.awaitingFinalPayout = false;
                pcHistoryOverflow = null;
                SavePcGamePayment(PcGamePaymentTransitions.Published(pcGamePayment), "HistoryPublishedSaved");
            }
            catch
            {
                round.awaitingFinalPayout = true;
                pcHistoryOverflow = priorOverflow;
                throw;
            }
        }
    }
}
