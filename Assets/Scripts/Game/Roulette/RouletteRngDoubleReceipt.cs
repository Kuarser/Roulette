using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class PcDoubleDrawContext
        {
            public int version;
            public string rngPath;
            public int profileIndex;
            public string roundArchiveId, spinJournalId;
            public int featureStep, choice;
            public bool suitMode;
            public long stake, creditBefore, denomination, remainderCents;
            public long wagered, returned, rounds;
            public int wins, lastDealer;
            public long baseMaximumWin, doubleMaximumWin;
            public int doubleMaximumRounds;
            public string priorPageJson, priorPageDigest, creditEvent;
        }

        private PcRngOperationContext pcPreparedDoubleOperation;

        // Preserve the exact historical v1 field set. The v2 wrapper binds a
        // deferred financial decision to its preceding native feature record.
        [Serializable]
        private sealed class PcDoubleDrawContextV2
        {
            public int version;
            public int settlementMode;
            public string featureId, priorFeatureNvJson, priorFeatureNvDigest;
            public PcDoubleDrawContext financial;
        }

        // V1/V2 retain their paid-base meaning and exact field sets. V3
        // explicitly owns an unpaid base entitlement and binds its payment.
        [Serializable]
        private sealed class PcDoubleDrawContextV3
        {
            public int version;
            public int settlementMode;
            public bool unpaidBase;
            public string featureId, priorFeatureNvJson, priorFeatureNvDigest;
            public string priorPaymentJson, priorPaymentDigest;
            public PcDoubleDrawContext financial;
        }

        [Serializable]
        private sealed class PcDoubleContextVersion { public int version; }

        private PcDoubleDrawContextV2 ReadPcDoubleDeferredContext(PcRngDrawReceipt receipt)
        {
            var header = JsonUtility.FromJson<PcDoubleContextVersion>(receipt.context.contextJson);
            RequirePcReceipt(header != null && (header.version == 1 || header.version == 2 || header.version == 3),
                "unsupported Double context version");
            if (header.version != 2) return null;
            var value = ParsePcReceiptContext<PcDoubleDrawContextV2>(receipt.context.contextJson);
            RequirePcReceipt(value.version == 2 && value.settlementMode == 2 &&
                LifecycleGuid(value.featureId) && value.financial != null &&
                value.priorFeatureNvDigest == PcReceiptTextDigest(value.priorFeatureNvJson),
                "invalid deferred Double context");
            var saved = ParsePcReceiptContext<PcDoubleFeatureNvV1>(value.priorFeatureNvJson);
            saved.Validate();
            RequirePcReceipt(saved.featureId == value.featureId && saved.savedState == 2 &&
                saved.completionStage == 0 && saved.profile == receipt.context.profile &&
                saved.roundArchiveId == value.financial.roundArchiveId &&
                saved.spinJournalId == value.financial.spinJournalId &&
                saved.stepCount == value.financial.featureStep && saved.suitMode == value.financial.suitMode,
                "Double decision differs from its prior native feature");
            return value;
        }

        private static T ParsePcUnpaidReceiptContext<T>(string json)
        {
            T value = ParsePcReceiptContext<T>(json);
            RestoreLifecycleJsonNulls(new LifecycleJsonReader(json).Read(), value);
            return value;
        }

        private PcDoubleDrawContextV3 ReadPcDoubleUnpaidContext(PcRngDrawReceipt receipt)
        {
            var header = JsonUtility.FromJson<PcDoubleContextVersion>(receipt.context.contextJson);
            RequirePcReceipt(header != null && (header.version == 1 || header.version == 2 || header.version == 3),
                "unsupported Double context version");
            if (header.version != 3) return null;
            var value = ParsePcUnpaidReceiptContext<PcDoubleDrawContextV3>(receipt.context.contextJson);
            RequirePcReceipt(value.version == 3 && value.settlementMode == 3 && value.unpaidBase &&
                LifecycleGuid(value.featureId) && value.financial != null &&
                value.priorFeatureNvDigest == PcReceiptTextDigest(value.priorFeatureNvJson) &&
                value.priorPaymentDigest == PcReceiptTextDigest(value.priorPaymentJson),
                "invalid unpaid Double context");
            var saved = ParsePcUnpaidReceiptContext<PcDoubleFeatureNvV1>(value.priorFeatureNvJson);
            var payment = ParsePcUnpaidReceiptContext<PcGamePaymentRecordV1>(value.priorPaymentJson);
            saved.Validate();
            payment.Validate();
            RequirePcReceipt(saved.featureId == value.featureId && saved.savedState == 2 &&
                saved.completionStage == 0 && saved.profile == receipt.context.profile &&
                saved.roundArchiveId == value.financial.roundArchiveId &&
                saved.spinJournalId == value.financial.spinJournalId &&
                saved.stepCount == value.financial.featureStep && saved.suitMode == value.financial.suitMode &&
                payment.owner == PcGamePaymentOwner.Double && payment.baseOffer == PcGamePaymentOffer.Transferred &&
                payment.featureId == value.featureId && payment.roundArchiveId == saved.roundArchiveId &&
                payment.spinJournalId == saved.spinJournalId && payment.profile == saved.profile &&
                !payment.prepared && !payment.moneyApplied && !payment.historyPublished &&
                payment.creditBefore == value.financial.creditBefore &&
                payment.denomination == value.financial.denomination &&
                payment.remainderCents == value.financial.remainderCents,
                "unpaid Double decision differs from its feature or payment owner");
            return value;
        }

        private static void RequirePcDoublePaymentAnchors(PcGamePaymentRecordV1 actual,
            PcGamePaymentRecordV1 prior)
        {
            RequirePcReceipt(actual != null && prior != null, "Double payment anchor is missing");
            actual.Validate();
            prior.Validate();
            RequirePcReceipt(actual.version == prior.version && actual.roundArchiveId == prior.roundArchiveId &&
                actual.spinJournalId == prior.spinJournalId && actual.profile == prior.profile &&
                actual.operationId == prior.operationId && actual.priorHistoryCount == prior.priorHistoryCount &&
                actual.creditBefore == prior.creditBefore && actual.denomination == prior.denomination &&
                actual.remainderCents == prior.remainderCents && actual.baseAward == prior.baseAward &&
                actual.owner == prior.owner && actual.baseOffer == prior.baseOffer && actual.featureId == prior.featureId,
                "Double payment immutable anchors differ from its durable decision");
        }

        private PcDoubleDrawContext ReadPcDoubleContext(PcRngDrawReceipt receipt)
        {
            try
            {
                if(IsSasDoubleReceiptV4(receipt))
                {
                    RequirePcReceipt(pcRoundArchiveSchemaVersion==4&&session!=null,"V4 Double context requires its archive owner");
                    ValidatedSasDoubleEntry sasPrior;
                    return ReadSasDoubleContextV4(receipt,PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1",""),
                        session.BaseMaximumWin,session.DoubleMaximumWin,session.DoubleEnabled,session.DoubleSuitMode,out sasPrior).financial;
                }
                RequirePcReceipt(receipt != null && receipt.context != null &&
                    receipt.context.kind == 2 && receipt.context.scope == recordedHistoryKey,
                    "Double receipt kind or history namespace differs");
                PcRngDrawReceipt.Validate(receipt);
                PcDoubleDrawContextV3 unpaid = ReadPcDoubleUnpaidContext(receipt);
                PcDoubleDrawContextV2 deferred = ReadPcDoubleDeferredContext(receipt);
                RequirePcReceipt((unpaid != null) == (pcRoundArchiveSchemaVersion == 3) &&
                    (unpaid != null || pcGamePayment == null),
                    "Double receipt accounting version differs from its archive");
                RequirePcReceipt(unpaid != null || deferred != null || pcDoubleFeatureNv == null,
                    "historical Double receipt cannot own a versioned native feature");
                var context = unpaid != null ? unpaid.financial : deferred != null ? deferred.financial :
                    ParsePcReceiptContext<PcDoubleDrawContext>(receipt.context.contextJson);
                RecordedRound prior = ValidatePcDoubleContextCore(context, receipt.context.profile, unpaid != null);
                if (unpaid != null || deferred != null)
                {
                    PcDoubleFeatureNvV1 saved = ParsePcReceiptContext<PcDoubleFeatureNvV1>(unpaid != null ?
                        unpaid.priorFeatureNvJson : deferred.priorFeatureNvJson);
                    RequirePcReceipt(saved.resultDigest == PcDoubleFeatureResultDigest(prior),
                        "Double prior native result digest differs from its financial page");
                }
                if (unpaid != null)
                {
                    var payment = ParsePcUnpaidReceiptContext<PcGamePaymentRecordV1>(unpaid.priorPaymentJson);
                    RequirePcReceipt(payment.baseAward == prior.returned && payment.baseAward == prior.doubleUp.startingStake &&
                        payment.creditBefore == prior.creditAfter,
                        "Double prior unpaid page differs from its payment amount");
                    RequirePcDoublePaymentAnchors(pcGamePayment, payment);
                    RequirePcReceipt(recordedRoundPages.Count > 0,
                        "unpaid Double receipt has no current archived page");
                    ValidatePcDoubleFeatureNv(pcDoubleFeatureNv, recordedRoundPages[0], latestRoundLifecycle);
                    ValidatePcGamePayment(pcGamePayment, recordedRoundPages[0], latestRoundLifecycle, pcDoubleFeatureNv);
                }
                return context;
            }
            catch (PcRoundLifecycleDataException) { throw; }
            catch (Exception error)
            {
                throw new PcRoundLifecycleDataException("RNG result receipt: invalid Double context", error);
            }
        }

        private RecordedRound ValidatePcDoubleContext(PcDoubleDrawContext context, string profile)
        {
            return ValidatePcDoubleContextCore(context, profile, false);
        }

        private RecordedRound ValidatePcDoubleContextCore(PcDoubleDrawContext context, string profile, bool unpaid)
        {
            RequirePcReceipt(context != null && context.version == 1 &&
                context.profileIndex >= 0 && context.profileIndex < profiles.Length &&
                RouletteProfile.Load(profiles[context.profileIndex]).Id == profile &&
                LifecycleGuid(context.roundArchiveId) && LifecycleGuid(context.spinJournalId) &&
                context.featureStep >= 0 && context.featureStep < 16 &&
                context.choice >= -2 && context.choice <= 3 &&
                (context.choice < 0 || context.suitMode) && context.stake > 0 &&
                context.creditBefore >= 0 && context.denomination > 0 &&
                context.remainderCents >= 0 && context.remainderCents < context.denomination &&
                context.wagered > 0 && context.returned > 0 && context.rounds > 0 &&
                context.wins > 0 && context.wins <= context.rounds &&
                context.baseMaximumWin >= 0 && context.doubleMaximumWin > 0 &&
                context.doubleMaximumRounds > 0 && context.doubleMaximumRounds <= 16 &&
                context.featureStep < context.doubleMaximumRounds &&
                context.creditEvent != null && !string.IsNullOrEmpty(context.priorPageJson) &&
                context.priorPageDigest == PcReceiptTextDigest(context.priorPageJson),
                "invalid Double decision or financial context");
            RequirePcReceipt(string.Equals(context.rngPath,
                Path.GetFullPath(PcRngStateStore.PathForCurrentProcess), StringComparison.OrdinalIgnoreCase),
                "Double RNG file namespace differs");
            var prior = unpaid ? ParsePcUnpaidReceiptContext<RecordedRound>(context.priorPageJson) :
                ParsePcReceiptContext<RecordedRound>(context.priorPageJson);
            RequirePcReceipt(prior.present && prior.totalsRecorded && prior.hasDoubleUp &&
                prior.profile == profile && prior.roundArchiveId == context.roundArchiveId &&
                prior.spinJournalId == context.spinJournalId && prior.awaitingFinalPayout &&
                prior.doubleUp != null && !prior.doubleUp.complete && !prior.doubleUp.tookWin &&
                prior.doubleUp.steps != null && prior.doubleUp.steps.Length == context.featureStep &&
                prior.doubleUp.suitMode == context.suitMode &&
                prior.returned == prior.doubleUp.startingStake &&
                prior.totalWagered == context.wagered && prior.totalReturned == context.returned &&
                prior.totalRounds == context.rounds && prior.totalWins == context.wins &&
                prior.denomination == context.denomination && prior.moneyRemainderCents == context.remainderCents &&
                (unpaid ? context.creditBefore == prior.creditAfter :
                    prior.creditAfter >= prior.doubleUp.startingStake &&
                    context.creditBefore == prior.creditAfter - prior.doubleUp.startingStake) &&
                IsValidRecordedDouble(prior.doubleUp), "Double receipt does not describe its prior feature page");
            long lastStake = context.featureStep == 0 ? prior.doubleUp.startingStake :
                prior.doubleUp.steps[context.featureStep - 1].won;
            int lastDealer = context.featureStep == 0 ? -1 : prior.doubleUp.steps[context.featureStep - 1].dealer;
            RequirePcReceipt(context.stake == lastStake && context.lastDealer == lastDealer,
                "Double receipt stake or previous card differs");
            // This validates the complete table and original feature limits
            // with the same model, without touching the playable session.
            CreatePcDoubleProbeCore(context, prior, unpaid, unpaid);
            return prior;
        }

        private RouletteSession CreatePcDoubleProbe(PcDoubleDrawContext context, RecordedRound prior,
            bool deferred = false)
        {
            return CreatePcDoubleProbeCore(context, prior, deferred, false);
        }

        private RouletteSession CreatePcDoubleProbeCore(PcDoubleDrawContext context, RecordedRound prior,
            bool deferred, bool unpaid)
        {
            var probe = new RouletteSession(RouletteProfile.Load(profiles[context.profileIndex]),
                0, new ReceiptNoDrawOutcome(), context.doubleMaximumWin,
                context.doubleMaximumRounds, context.suitMode, context.baseMaximumWin) { DoubleEnabled = true };
            if (unpaid) probe.EnableDeferredBaseSettlement();
            if (deferred) probe.EnableDeferredDoubleSettlement();
            RequirePcReceipt(probe.DoubleMaximumWin == context.doubleMaximumWin &&
                probe.BaseMaximumWin == context.baseMaximumWin, "Double saved limits are inconsistent");
            var stacks = new Dictionary<int, List<long>>();
            RequirePcReceipt(prior.bets != null && prior.bets.Length > 0,
                "Double prior page has no wager");
            foreach (RecordedBet bet in prior.bets)
            {
                RequirePcReceipt(bet != null && bet.chips != null && bet.chips.Length > 0 &&
                    bet.chips.All(value => value > 0) && bet.chips.Sum() == bet.amount && !stacks.ContainsKey(bet.id),
                    "Double prior page wager stacks differ");
                stacks.Add(bet.id, bet.chips.ToList());
            }
            var result = new RoundResult { Pocket = prior.pocket, Wager = prior.wager,
                Returned = prior.returned, Category = prior.category };
            if (unpaid)
                probe.RestoreUnpaidRound(result, prior.creditAfter, prior.denomination, prior.selectedChip,
                    stacks, false, true, context.featureStep, context.stake, context.lastDealer,
                    prior.evaluationOrder, prior.moneyRemainderCents);
            else
                probe.RestorePendingRound(result, prior.creditAfter, prior.denomination,
                    prior.selectedChip, stacks, true, context.featureStep,
                    context.stake, context.lastDealer, prior.evaluationOrder, prior.moneyRemainderCents);
            probe.RestoreCounters(context.wagered, context.returned, context.rounds, context.wins);
            RequirePcReceipt(probe.InDoubleUp && probe.Credit == context.creditBefore &&
                probe.DoubleStake == context.stake && probe.DoubleRounds == context.featureStep &&
                probe.BaseSettlementDeferred == unpaid && probe.RoundPayoutPending == unpaid,
                "Double prior model does not restore its saved decision");
            return probe;
        }

        private void PreparePcDoubleDraw(int choice, long wager)
        {
            pcPreparedDoubleOperation = null;
            if(pcRoundArchiveSchemaVersion==4){PrepareSasDoubleDrawV4(choice,wager);return;}
            if (!PcReceiptEnabled) return;
            RequirePcReceipt(!pcAutoPlayStorageFaulted && session.InDoubleUp &&
                session.DoubleStake == wager && pendingSpin == null && recordedRoundPages.Count > 0 &&
                recordedRoundPages[0].doubleUp != null, "Double draw has no live archived feature");
            RecordedRound prior = recordedRoundPages[0];
            bool unpaid = pcRoundArchiveSchemaVersion == 3;
            var local = (RouletteSession)session;
            RequirePcReceipt((!unpaid || local.BaseSettlementDeferred) && local.RoundPayoutPending == unpaid,
                "Double live accounting mode differs from its archive");
            string priorJson = JsonUtility.ToJson(prior);
            var context = new PcDoubleDrawContext {
                version = 1, rngPath = Path.GetFullPath(PcRngStateStore.PathForCurrentProcess),
                profileIndex = profileIndex, roundArchiveId = prior.roundArchiveId,
                spinJournalId = prior.spinJournalId, featureStep = session.DoubleRounds,
                choice = choice, suitMode = session.DoubleSuitMode, stake = wager,
                creditBefore = session.Credit, denomination = session.Denomination,
                remainderCents = ((RouletteSession)session).MoneyRemainderCents,
                wagered = session.Wagered, returned = session.Returned,
                rounds = session.Rounds, wins = session.Wins, lastDealer = session.LastDoubleCard,
                baseMaximumWin = session.BaseMaximumWin, doubleMaximumWin = session.DoubleMaximumWin,
                doubleMaximumRounds = session.DoubleMaximumRounds,
                priorPageJson = priorJson, priorPageDigest = PcReceiptTextDigest(priorJson),
                creditEvent = PlayerPrefs.GetString(PcCreditKey, "")
            };
            ValidatePcDoubleContextCore(context, session.Profile.Id, unpaid);
            if (HasTrustedLatestLifecycle)
                RequirePcReceipt(GetLatestLifecycleForRestore().roundStage == PcRoundStage.RoundOpen,
                    "Double draw follows an already finished lifecycle");
            string contextJson = JsonUtility.ToJson(context);
            if (local.DoubleSettlementDeferred)
            {
                RequirePcReceipt((pcRoundArchiveSchemaVersion == 2 || unpaid) && pcDoubleFeatureNv != null &&
                    pcDoubleFeatureNv.completionStage == 0 && pcDoubleFeatureNv.savedState == 2 &&
                    pcDoubleFeatureNv.stepCount == context.featureStep &&
                    pcDoubleFeatureNv.roundArchiveId == prior.roundArchiveId &&
                    pcDoubleFeatureNv.spinJournalId == prior.spinJournalId,
                    "deferred Double draw lacks its matching ready feature");
                string priorFeature = JsonUtility.ToJson(pcDoubleFeatureNv);
                if (unpaid)
                {
                    ValidatePcGamePayment(pcGamePayment, prior, latestRoundLifecycle, pcDoubleFeatureNv);
                    RequirePcReceipt(pcGamePayment.owner == PcGamePaymentOwner.Double &&
                        !pcGamePayment.prepared && !pcGamePayment.moneyApplied && !pcGamePayment.historyPublished,
                        "unpaid Double draw follows payment preparation or application");
                    string priorPayment = JsonUtility.ToJson(pcGamePayment);
                    contextJson = JsonUtility.ToJson(new PcDoubleDrawContextV3 {
                        version = 3, settlementMode = 3, unpaidBase = true, featureId = pcDoubleFeatureNv.featureId,
                        priorFeatureNvJson = priorFeature, priorFeatureNvDigest = PcReceiptTextDigest(priorFeature),
                        priorPaymentJson = priorPayment, priorPaymentDigest = PcReceiptTextDigest(priorPayment),
                        financial = context
                    });
                }
                else
                    contextJson = JsonUtility.ToJson(new PcDoubleDrawContextV2 {
                        version = 2, settlementMode = 2, featureId = pcDoubleFeatureNv.featureId,
                        priorFeatureNvJson = priorFeature, priorFeatureNvDigest = PcReceiptTextDigest(priorFeature),
                        financial = context
                    });
            }
            else RequirePcReceipt(!unpaid, "unpaid Double draw requires deferred feature completion");
            var operation = new PcRngOperationContext {
                schema = 1, operationId = Guid.NewGuid().ToString("N"), scope = recordedHistoryKey,
                profile = session.Profile.Id, kind = 2, contextJson = contextJson
            };
            pcOutcome.Prepare(operation, demoOutcome.ResolveDoubleSuit);
            pcPreparedDoubleOperation = operation;
        }

        private void CompletePcDoubleDraw()
        {
            if (!PcReceiptEnabled) return;
            RequirePcReceipt(!pcAutoPlayStorageFaulted && pcPreparedDoubleOperation != null,
                "Double completion has no prepared operation");
            PcRngDrawReceipt receipt = pcOutcome.Receipt;
            RequirePcReceipt(receipt != null && receipt.context.operationId == pcPreparedDoubleOperation.operationId &&
                receipt.contextSha256 == PcRngDrawReceipt.ContextDigest(pcPreparedDoubleOperation),
                "Double draw returned without its prepared durable result");
            PcDoubleDrawContext context = ReadPcDoubleContext(receipt);
            RequireAppliedPcDoublePage(context, receipt, false);
            pcOutcome.Acknowledge(receipt.context.operationId, receipt.contextSha256);
            pcPreparedDoubleOperation = null;
            Debug.Log("ROULETTE_RNG_DOUBLE_RECEIPT_ACCEPTED: id=" + receipt.context.operationId +
                " round=" + context.roundArchiveId + " step=" + context.featureStep +
                " raw=" + receipt.rawResult + " suit=" + receipt.effectiveResult);
        }

        private void SelectPcDoubleReceiptProfile(PcRngDrawReceipt receipt)
        {
            PcDoubleDrawContext context = ReadPcDoubleContext(receipt);
            RequirePcReceipt(pendingSpin == null && recordedRoundPages.Count > 0 &&
                recordedRoundPages[0].present && recordedRoundPages[0].profile == receipt.context.profile &&
                recordedRoundPages[0].roundArchiveId == context.roundArchiveId &&
                recordedRoundPages[0].spinJournalId == context.spinJournalId,
                "Double receipt has no matching latest round");
            if (JsonUtility.ToJson(recordedRoundPages[0]) == context.priorPageJson)
            {
                PcDoubleDrawContextV3 unpaid = ReadPcDoubleUnpaidContext(receipt);
                PcDoubleDrawContextV2 deferred = ReadPcDoubleDeferredContext(receipt);
                if (unpaid != null || deferred != null)
                    RequirePcReceipt(pcRoundArchiveSchemaVersion == (unpaid != null ? 3 : 2) && pcDoubleFeatureNv != null &&
                        JsonUtility.ToJson(pcDoubleFeatureNv) == (unpaid != null ?
                            unpaid.priorFeatureNvJson : deferred.priorFeatureNvJson),
                        "Double prior feature differs before financial restore");
                if (unpaid != null)
                    RequirePcReceipt(pcGamePayment != null && JsonUtility.ToJson(pcGamePayment) == unpaid.priorPaymentJson,
                        "Double prior payment differs before financial restore");
                ValidatePcReceiptCreditBeforeRestore(context.creditEvent);
                profileIndex = context.profileIndex;
                pcReceiptOverridesStagedProfile = true;
            }
            else RequireAppliedPcDoublePage(context, receipt, true);
        }

        private static int PcRecordedDoubleChoice(int choice)
        {
            return choice == -1 ? 5 : choice == -2 ? 4 :
                choice == 0 ? 3 : choice == 1 ? 2 : choice == 2 ? 0 : 1;
        }

        private RecordedRound ExpectedPcDoublePage(PcDoubleDrawContext context,
            PcRngDrawReceipt receipt, bool followedByTake, out RouletteSession probe, out bool won,
            bool followedByCompletion = false)
        {
            PcDoubleDrawContextV3 unpaid = ReadPcDoubleUnpaidContext(receipt);
            var expected = unpaid != null ? ParsePcUnpaidReceiptContext<RecordedRound>(context.priorPageJson) :
                ParsePcReceiptContext<RecordedRound>(context.priorPageJson);
            bool deferred = unpaid != null || ReadPcDoubleDeferredContext(receipt) != null;
            probe = CreatePcDoubleProbeCore(context, expected, deferred, unpaid != null);
            won = probe.ReplayDoubleChoice(context.choice, receipt.effectiveResult);
            var steps = expected.doubleUp.steps.ToList();
            steps.Add(new RecordedDoubleStep { choice = PcRecordedDoubleChoice(context.choice),
                dealer = receipt.effectiveResult, wager = context.stake, won = won ? probe.DoubleStake : 0 });
            expected.doubleUp.steps = steps.ToArray();
            if (!probe.InDoubleUp)
            {
                expected.doubleUp.complete = true;
                expected.doubleUp.finalWin = won ? probe.DoubleStake : 0;
                ApplyPcDoubleProbeMoney(expected, probe);
            }
            if (deferred && followedByCompletion)
            {
                RequirePcReceipt(followedByTake == !probe.DoubleChoiceTerminal,
                    "deferred completion TAKE marker differs from its terminal decision");
                long taken = probe.DoubleStake;
                probe.CompleteDoubleFeature(taken);
                expected.doubleUp.tookWin = followedByTake;
                expected.doubleUp.complete = true;
                expected.doubleUp.finalWin = taken;
                expected.awaitingFinalPayout = unpaid != null && !pcGamePayment.historyPublished;
                ApplyPcDoubleProbeMoney(expected, probe);
            }
            else if (followedByTake)
            {
                RequirePcReceipt(probe.InDoubleUp, "TAKE follows an already terminal Double decision");
                long taken = probe.DoubleStake;
                probe.CollectDouble();
                expected.doubleUp.tookWin = true;
                expected.doubleUp.complete = true;
                expected.doubleUp.finalWin = taken;
                ApplyPcDoubleProbeMoney(expected, probe);
            }
            return expected;
        }

        private static void ApplyPcDoubleProbeMoney(RecordedRound page, RouletteSession probe)
        {
            page.returned = probe.LastResult.Returned;
            page.creditAfter = probe.Credit;
            page.totalReturned = probe.Returned;
            page.totalWins = probe.Wins;
        }

        private void RequireAppliedPcDoublePage(PcDoubleDrawContext context,
            PcRngDrawReceipt receipt, bool allowLaterTake)
        {
            RequirePcReceipt(recordedRoundPages.Count > 0 && recordedRoundPages[0].doubleUp != null,
                "Double result has no committed feature page");
            PcDoubleDrawContextV3 unpaid = ReadPcDoubleUnpaidContext(receipt);
            var actual = unpaid != null ? ParsePcUnpaidReceiptContext<RecordedRound>(JsonUtility.ToJson(recordedRoundPages[0])) :
                JsonUtility.FromJson<RecordedRound>(JsonUtility.ToJson(recordedRoundPages[0]));
            RequirePcReceipt(actual.doubleUp.steps != null &&
                actual.doubleUp.steps.Length == context.featureStep + 1 &&
                (allowLaterTake || !actual.doubleUp.tookWin),
                "Double committed step count or later action differs");
            RouletteSession probe; bool won;
            PcDoubleDrawContextV2 deferred = ReadPcDoubleDeferredContext(receipt);
            if (unpaid != null || deferred != null)
            {
                ValidatePcDoubleFeatureNv(pcDoubleFeatureNv, recordedRoundPages[0], latestRoundLifecycle);
                RequirePcReceipt(pcRoundArchiveSchemaVersion == (unpaid != null ? 3 : 2) && pcDoubleFeatureNv != null &&
                    pcDoubleFeatureNv.featureId == (unpaid != null ? unpaid.featureId : deferred.featureId) &&
                    pcDoubleFeatureNv.stepCount == context.featureStep + 1 &&
                    (actual.doubleUp.complete ? allowLaterTake && pcDoubleFeatureNv.completionStage == 2 :
                        pcDoubleFeatureNv.completionStage != 2),
                    "committed deferred Double feature or completion marker differs");
            }
            if (unpaid != null)
            {
                ValidatePcGamePayment(pcGamePayment, recordedRoundPages[0], latestRoundLifecycle, pcDoubleFeatureNv);
                RequirePcDoublePaymentAnchors(pcGamePayment,
                    ParsePcUnpaidReceiptContext<PcGamePaymentRecordV1>(unpaid.priorPaymentJson));
                RequirePcReceipt(actual.doubleUp.complete == pcGamePayment.moneyApplied &&
                    (allowLaterTake || JsonUtility.ToJson(pcGamePayment) == unpaid.priorPaymentJson),
                    "committed Double payment effects differ from its decision boundary");
            }
            RecordedRound expected = ExpectedPcDoublePage(context, receipt,
                allowLaterTake && actual.doubleUp.tookWin, out probe, out won,
                (unpaid != null || deferred != null) && allowLaterTake && actual.doubleUp.complete);
            // Finalize/financial restore may clear this presentation bit.
            // Every identity, earlier step and financial field stays exact.
            if (unpaid == null && deferred == null && expected.doubleUp.complete)
                expected.awaitingFinalPayout = actual.awaitingFinalPayout = false;
            RequirePcReceipt(JsonUtility.ToJson(actual) == JsonUtility.ToJson(expected),
                "Double committed page differs from its durable decision");
            if (!allowLaterTake)
                RequirePcReceipt(session != null && session.Credit == probe.Credit &&
                    session.Returned == probe.Returned && session.Wagered == probe.Wagered &&
                    session.Rounds == probe.Rounds && session.Wins == probe.Wins &&
                    session.DoubleStake == probe.DoubleStake && session.DoubleRounds == probe.DoubleRounds &&
                    session.LastDoubleCard == receipt.effectiveResult && session.InDoubleUp == probe.InDoubleUp &&
                    (unpaid == null || ((RouletteSession)session).BaseSettlementDeferred) &&
                    ((RouletteSession)session).RoundPayoutPending == probe.RoundPayoutPending,
                    "Double committed live money or feature step differs");
        }

        private void ReconcilePcDoubleDraw(PcRngDrawReceipt receipt)
        {
            try { ReconcilePcDoubleDrawCore(receipt); }
            catch (PcRoundLifecycleDataException) { throw; }
            catch (Exception error)
            {
                throw new PcRoundLifecycleDataException("RNG result receipt: Double recovery failed", error);
            }
        }

        private void ReconcilePcDoubleDrawCore(PcRngDrawReceipt receipt)
        {
            if(IsSasDoubleReceiptV4(receipt)){ApplySasDoubleReceiptV4(receipt);return;}
            PcDoubleDrawContext context = ReadPcDoubleContext(receipt);
            RequirePcReceipt(PcReceiptEnabled && pendingSpin == null && session != null &&
                recordedRoundPages.Count > 0 && recordedRoundPages[0].roundArchiveId == context.roundArchiveId &&
                recordedRoundPages[0].spinJournalId == context.spinJournalId && recordedRoundPages[0].doubleUp != null,
                "Double recovery lacks its matching financial round");
            RecordedRound current = recordedRoundPages[0];
            bool priorUnchanged = JsonUtility.ToJson(current) == context.priorPageJson;
            if (priorUnchanged)
            {
                PcDoubleDrawContextV3 unpaid = ReadPcDoubleUnpaidContext(receipt);
                PcDoubleDrawContextV2 deferred = ReadPcDoubleDeferredContext(receipt);
                RequirePcReceipt(((RouletteSession)session).DoubleSettlementDeferred == (unpaid != null || deferred != null) &&
                    (unpaid == null || ((RouletteSession)session).BaseSettlementDeferred) &&
                    ((RouletteSession)session).RoundPayoutPending == (unpaid != null),
                    "unapplied Double settlement mode differs from its context version");
                if (unpaid != null || deferred != null)
                    RequirePcReceipt(pcRoundArchiveSchemaVersion == (unpaid != null ? 3 : 2) && pcDoubleFeatureNv != null &&
                        JsonUtility.ToJson(pcDoubleFeatureNv) == (unpaid != null ?
                            unpaid.priorFeatureNvJson : deferred.priorFeatureNvJson) &&
                        ((RouletteSession)session).DoubleSettlementDeferred,
                        "unapplied Double native feature differs from its durable decision");
                if (unpaid != null)
                    RequirePcReceipt(pcGamePayment != null && JsonUtility.ToJson(pcGamePayment) == unpaid.priorPaymentJson,
                        "unapplied Double payment differs from its durable decision");
                RequirePcReceipt(session.Profile.Id == receipt.context.profile && profileIndex == context.profileIndex &&
                    session.InDoubleUp && session.DoubleSuitMode == context.suitMode &&
                    session.DoubleStake == context.stake && session.DoubleRounds == context.featureStep &&
                    session.LastDoubleCard == context.lastDealer && session.Credit == context.creditBefore &&
                    session.Denomination == context.denomination &&
                    ((RouletteSession)session).MoneyRemainderCents == context.remainderCents &&
                    session.Wagered == context.wagered && session.Returned == context.returned &&
                    session.Rounds == context.rounds && session.Wins == context.wins &&
                    session.BaseMaximumWin == context.baseMaximumWin &&
                    session.DoubleMaximumWin == context.doubleMaximumWin &&
                    session.DoubleMaximumRounds == context.doubleMaximumRounds &&
                    PlayerPrefs.GetString(PcCreditKey, "") == context.creditEvent,
                    "unapplied Double financial state or original limits differ");
                if (HasTrustedLatestLifecycle)
                    RequirePcReceipt(GetLatestLifecycleForRestore().roundStage == PcRoundStage.RoundOpen,
                        "unapplied Double decision follows End");
                bool won = ((RouletteSession)session).ReplayDoubleChoice(context.choice, receipt.effectiveResult);
                RecordDoubleChoice(context.choice, context.stake, won);
                RequirePcReceipt(!pcAutoPlayStorageFaulted, "Double recovery failed to save its feature page");
                RequireAppliedPcDoublePage(context, receipt, false);
                LifecycleCheckpoint("DoubleReceiptAppliedSaved");
                Debug.Log("ROULETTE_RNG_DOUBLE_RECEIPT_REPLAYED: id=" + receipt.context.operationId +
                    " round=" + context.roundArchiveId + " step=" + context.featureStep +
                    " suit=" + receipt.effectiveResult + " drawn=false");
            }
            else
            {
                // A successful choice may be followed by TAKE and End without
                // another RNG request. Verify that exact evolution; later PC
                // deposits/collects are validated by their own money receipt.
                RequireAppliedPcDoublePage(context, receipt, true);
                Debug.Log("ROULETTE_RNG_DOUBLE_RECEIPT_ARCHIVED: id=" + receipt.context.operationId +
                    " round=" + context.roundArchiveId + " step=" + context.featureStep);
            }
            pcOutcome.Acknowledge(receipt.context.operationId, receipt.contextSha256);
            pcPreparedDoubleOperation = null;
        }
    }
}
