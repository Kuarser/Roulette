using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class PcSpinDrawContext
        {
            public int version;
            public string rngPath;
            public PendingSpinRecord pending;
            public string baseLatestPage;
            public string creditEvent;
            public long wagered, returned, rounds;
            public int wins;
            public int demoOption, armedVisualPath;
        }

        private PcOutcomeSource pcOutcome;
        private PcSpinDrawContext pcPreparedSpinDraw;
        private PcSpinDrawContext pcRestoredSpinDraw;
        private PcRngDrawReceipt pcRestoredSpinReceipt;
        private bool pcReceiptPredecessorMatches;
        private bool pcReceiptOverridesStagedProfile;

        private bool PcReceiptEnabled
        {
            get
            {
                var local = session as RouletteSession;
                return !temporaryHistory && pcOutcome != null && demoOutcome != null &&
                    ReferenceEquals(outcome, demoOutcome) && local != null &&
                    ReferenceEquals(local.OutcomeSource, outcome);
            }
        }

        private static void RequirePcReceipt(bool accepted, string message)
        {
            if (!accepted) throw new PcRoundLifecycleDataException("RNG result receipt: " + message);
        }

        private static string PcReceiptTextDigest(string text)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text ?? "")))
                    .Replace("-", "");
        }

        private static T ParsePcReceiptContext<T>(string json)
        {
            RequirePcReceipt(!string.IsNullOrEmpty(json), "missing operation context");
            var tree = new LifecycleJsonReader(json).Read();
            ValidateLifecycleJsonShape(tree, typeof(T), json);
            T value = JsonUtility.FromJson<T>(json);
            RequirePcReceipt(value != null, "invalid operation context");
            return value;
        }

        private string PcReceiptLatestPage
        {
            get { return recordedRoundPages.Count == 0 ? "" : JsonUtility.ToJson(recordedRoundPages[0]); }
        }

        private void ValidatePcReceiptCreditBeforeRestore(string expected)
        {
            RequirePcReceipt(PlayerPrefs.GetString(PcCreditKey, "") == expected,
                "uncommitted draw credit receipt differs");
            PcCreditEvent credit = ReadPcCreditEvent();
            RequirePcReceipt(credit == null || credit.anchor != PcCreditAnchor || credit.phase == 3,
                "uncommitted draw precedes credit completion");
        }

        // Allocate every identity and capture the uncommitted financial state
        // before BeginSpin debits the stake and invokes the real RNG source.
        private void PreparePcSpinDraw()
        {
            pcPreparedSpinDraw = null;
            pcPreparedSpinContextRaw = null;
            pcSpinRoundFormatContextRaw = null;
            bool v4 = SelectPcSpinV4Format();
            if (!PcReceiptEnabled) return;
            var local = (RouletteSession)session;
            var context = new PcSpinDrawContext {
                version = 1,
                rngPath = Path.GetFullPath(PcRngStateStore.PathForCurrentProcess),
                pending = CreatePendingSpinRecord(LifecycleSuccessorSpinId(), session.Credit, -1),
                baseLatestPage = PcReceiptLatestPage,
                creditEvent = PlayerPrefs.GetString(PcCreditKey, ""),
                wagered = session.Wagered, returned = session.Returned,
                rounds = session.Rounds, wins = session.Wins,
                demoOption = demoOutcome.SelectedOption,
                armedVisualPath = demoOutcome.ArmedVisualPath
            };
            ValidatePcSpinDrawContext(context, null);
            string json = JsonUtility.ToJson(context);
            if (v4) { json = EncodePcSpinFormatContextV2(json); pcSpinRoundFormatContextRaw = json; }
            pcOutcome.Prepare(new PcRngOperationContext {
                schema = 1, operationId = context.pending.id,
                scope = recordedHistoryKey, profile = session.Profile.Id,
                kind = 1, contextJson = json
            }, demoOutcome.ResolvePocket);
            pcPreparedSpinDraw = context;
            pcPreparedSpinContextRaw = json;
        }

        private void ValidatePcSpinDrawContext(PcSpinDrawContext context, PcRngDrawReceipt receipt)
        {
            RequirePcReceipt(context != null && context.version == 1 && context.pending != null,
                "unsupported wheel context");
            PendingSpinRecord p = context.pending;
            Guid operation;
            RequirePcReceipt(Guid.TryParseExact(p.id, "N", out operation) && operation != Guid.Empty &&
                p.id == operation.ToString("N") && p.version == 1 && p.pocket == -1 &&
                !p.hasWheelStopIndex && p.wheelStopIndex == 0 &&
                p.profileIndex >= 0 && p.profileIndex < profiles.Length &&
                p.profile == RouletteProfile.Load(profiles[p.profileIndex]).Id &&
                p.creditBefore > 0 && p.wager > 0 && p.wager <= p.creditBefore &&
                p.denomination > 0 && p.moneyRemainderCents >= 0 &&
                p.moneyRemainderCents < p.denomination && p.selectedChip > 0 &&
                p.baseHistoryCount >= 0 && p.baseHistoryCount <= 50 &&
                p.baseLatestPocket >= -1 && p.baseLatestPocket <= 36 &&
                p.hasTableUndo && p.tableUndo != null &&
                context.wagered >= 0 && context.returned >= 0 && context.rounds >= 0 &&
                context.wins >= 0 && context.wins <= context.rounds &&
                context.demoOption >= 0 && context.demoOption < 40 &&
                context.armedVisualPath >= -1 && context.armedVisualPath <= 36 &&
                context.baseLatestPage != null && context.creditEvent != null,
                "invalid wheel identity or financial context");
            RequirePcReceipt(string.Equals(context.rngPath,
                Path.GetFullPath(PcRngStateStore.PathForCurrentProcess), StringComparison.OrdinalIgnoreCase),
                "RNG file namespace differs");
            RequirePcReceipt(p.baseHistoryCount == 0 ?
                p.baseLatestPocket == -1 && p.baseLatestRoundId == "" && context.baseLatestPage == "" :
                p.baseLatestPocket >= 0 && !string.IsNullOrEmpty(context.baseLatestPage),
                "invalid predecessor context");
            var profile = RouletteProfile.Load(profiles[p.profileIndex]);
            var probe = new RouletteSession(profile, p.creditBefore, new ReceiptNoDrawOutcome());
            var stacks = DecodeStagedBets(p.bets);
            RequirePcReceipt(stacks.Values.Sum(chips => chips.Sum()) == p.wager,
                "wager differs from complete chip stacks");
            // Validate the same wager, profile limits, ordering and complete
            // Undo queue without mutating the playable session or drawing.
            probe.RestoreStagedBets(p.creditBefore, p.denomination, p.selectedChip,
                stacks, p.tableLastTouched, DecodeTableUndo(p.tableUndo),
                p.evaluationOrder, true, p.moneyRemainderCents);
            probe.RestorePendingSpin(0, p.creditBefore, p.denomination, p.selectedChip,
                stacks, p.evaluationOrder, p.moneyRemainderCents);
            if (receipt == null) return;
            RequirePcReceipt(receipt.context.kind == 1 && receipt.context.operationId == p.id &&
                receipt.context.scope == recordedHistoryKey && receipt.context.profile == p.profile &&
                receipt.rawResult >= 0 && receipt.rawResult <= 36 &&
                receipt.effectiveResult >= 0 && receipt.effectiveResult <= 36,
                "wheel receipt identity or range differs");
            // An explicitly selected fixed Demo result is independent of raw.
            // Other pure mappings are confined to isolated smoke decorators.
            if (context.demoOption >= 3)
                RequirePcReceipt(receipt.effectiveResult == context.demoOption - 3,
                    "fixed Demo result differs from saved choice");
        }

        private sealed class ReceiptNoDrawOutcome : IRouletteOutcomeSource
        {
            public int DrawPocket() { throw new InvalidOperationException("Receipt validation attempted a draw"); }
            public int DrawDoubleSuit() { throw new InvalidOperationException("Receipt validation attempted a suit draw"); }
        }

        // Read-only candidate selection. No missing pending record is written
        // until the original archive and PC credit receipt have been restored.
        private void SelectPcReceiptProfile()
        {
            pcRestoredSpinDraw = null;
            pcRestoredSpinReceipt = null;
            pcReceiptPredecessorMatches = false;
            pcReceiptOverridesStagedProfile = false;
            if (temporaryHistory || pcOutcome == null) return;
            PcRngDrawReceipt receipt = pcOutcome.Receipt;
            if (receipt == null) return;
            RequirePcReceipt(receipt.context.scope == recordedHistoryKey,
                "receipt belongs to another history namespace");
            if (receipt.context.kind == 2)
            {
                SelectPcDoubleReceiptProfile(receipt);
                return;
            }
            RequirePcReceipt(receipt.context.kind == 1, "unsupported draw kind");
            string formatRaw;
            var context = DecodePcSpinDrawContext(receipt.context.contextJson, receipt, out formatRaw);
            if (pendingSpin != null)
                RequirePcReceipt(pcSpinRoundFormatContextRaw == formatRaw,
                    "Pending format conflicts with retained wheel receipt");
            else pcSpinRoundFormatContextRaw = formatRaw;
            pcRestoredSpinDraw = context;
            pcRestoredSpinReceipt = receipt;
            pcReceiptPredecessorMatches = context.baseLatestPage == PcReceiptLatestPage;
            bool archived = recordedRoundPages.Count > 0 &&
                recordedRoundPages[0].spinJournalId == receipt.context.operationId;
            // A later credit event may legitimately follow this draw. Prove
            // the archive consumer before credit recovery can advance it.
            if (archived) RequireArchivedPcSpinReceipt(context, receipt);
            else ValidatePcReceiptCreditBeforeRestore(context.creditEvent);
            if (!archived && pendingSpin == null)
            {
                pcReceiptOverridesStagedProfile = true;
                profileIndex = context.pending.profileIndex;
            }
        }

        private void RequireArchivedPcSpinReceipt(PcSpinDrawContext context,
            PcRngDrawReceipt receipt)
        {
            PendingSpinRecord p = context.pending;
            RequirePcReceipt(recordedRoundPages.Count > 0 &&
                recordedRoundPages[0].spinJournalId == p.id,
                "committed wheel archive differs");
            RecordedRound page = recordedRoundPages[0];
            RequirePcReceipt(page.present && page.totalsRecorded &&
                page.profile == p.profile && page.pocket == receipt.effectiveResult &&
                page.wager == p.wager && page.denomination == p.denomination &&
                page.moneyRemainderCents == p.moneyRemainderCents &&
                page.totalRounds == context.rounds + 1 &&
                page.totalWagered == checked(context.wagered + p.wager) &&
                SameReceiptBets(page.bets, p.bets), "committed wheel archive differs");
            if (pcRoundArchiveSchemaVersion == 3)
            {
                ValidatePcGamePayment(pcGamePayment, page, latestRoundLifecycle, pcDoubleFeatureNv);
                RequirePcReceipt(pcGamePayment.spinJournalId == p.id &&
                    pcGamePayment.roundArchiveId == page.roundArchiveId && pcGamePayment.profile == p.profile &&
                    pcGamePayment.creditBefore == checked(p.creditBefore - p.wager) &&
                    pcGamePayment.priorHistoryCount == p.baseHistoryCount &&
                    pcGamePayment.denomination == p.denomination &&
                    pcGamePayment.remainderCents == p.moneyRemainderCents,
                    "unpaid wheel payment anchors differ from its committed RNG decision");
            }
            if (pendingSpin != null)
                RequireMatchingReceiptPending(pendingSpin, context, receipt);
        }

        private void ReconcilePcRngReceipt()
        {
            if (temporaryHistory || pcOutcome == null) return;
            PcRngDrawReceipt receipt = pcOutcome.Receipt;
            if (receipt == null) return;
            if (receipt.context.kind == 2) { ReconcilePcDoubleDraw(receipt); return; }
            var context = pcRestoredSpinDraw;
            RequirePcReceipt(context != null && pcRestoredSpinReceipt != null,
                "wheel receipt was not validated before profile restore");
            PendingSpinRecord p = context.pending;
            if (recordedRoundPages.Count > 0 && recordedRoundPages[0].spinJournalId == p.id)
            {
                RequireArchivedPcSpinReceipt(context, receipt);
                pcOutcome.Acknowledge(receipt.context.operationId, receipt.contextSha256);
                pcRestoredSpinReceipt = null;
                pcRestoredSpinDraw = null;
                Debug.Log("ROULETTE_RNG_RECEIPT_ARCHIVED: id=" + p.id);
                return;
            }
            RequirePcReceipt(session != null && session.Profile.Id == p.profile &&
                profileIndex == p.profileIndex && pcReceiptPredecessorMatches &&
                recordedRoundPages.Count == p.baseHistoryCount &&
                (recordedRoundPages.Count == 0 ? -1 : recordedRoundPages[0].pocket) == p.baseLatestPocket &&
                session.Credit == p.creditBefore && session.Denomination == p.denomination &&
                ((RouletteSession)session).MoneyRemainderCents == p.moneyRemainderCents &&
                session.Wagered == context.wagered && session.Returned == context.returned &&
                session.Rounds == context.rounds && session.Wins == context.wins &&
                PlayerPrefs.GetString(PcCreditKey, "") == context.creditEvent,
                "uncommitted wheel financial baseline differs");
            if (HasTrustedLatestLifecycle)
            {
                PcRoundLifecycleRecord prior = GetLatestLifecycleForRestore();
                RequirePcReceipt(prior.roundStage == PcRoundStage.Ended &&
                    prior.roundArchiveId == p.baseLatestRoundId, "wheel lacks its strong predecessor");
                ValidateIdleLifecycleSuccessor(prior, p.id);
            }
            if (pendingSpin == null)
            {
                PendingSpinRecord restored = JsonUtility.FromJson<PendingSpinRecord>(JsonUtility.ToJson(p));
                restored.pocket = receipt.effectiveResult;
                LifecycleBeforeWrite("RngPendingRebuild");
                SavePcPendingSpinValue(restored, pcSpinRoundFormatContextRaw);
                pendingSpin = restored;
                Debug.Log("ROULETTE_RNG_RECEIPT_PENDING_REBUILT: id=" + restored.id);
                LifecycleCheckpoint("RngPendingRebuilt");
            }
            else RequireMatchingReceiptPending(pendingSpin, context, receipt);
            if (!pendingSpin.hasWheelStopIndex)
                demoOutcome.RestoreReceiptVisualPath(context.armedVisualPath);
        }

        private static bool SameReceiptBets(RecordedBet[] left, RecordedBet[] right)
        {
            if (left == null || right == null || left.Length != right.Length) return false;
            RecordedBet[] a = left.OrderBy(b => b.id).ToArray();
            RecordedBet[] b = right.OrderBy(item => item.id).ToArray();
            for (int i = 0; i < a.Length; ++i)
                if (a[i] == null || b[i] == null || a[i].id != b[i].id || a[i].amount != b[i].amount ||
                    a[i].chips == null || b[i].chips == null || !a[i].chips.SequenceEqual(b[i].chips)) return false;
            return true;
        }

        private static void RequireMatchingReceiptPending(PendingSpinRecord pending,
            PcSpinDrawContext context, PcRngDrawReceipt receipt)
        {
            RequirePcReceipt(pending != null && pending.pocket == receipt.effectiveResult,
                "pending effective result differs");
            // Wheel stop is added after the result journal and can legitimately
            // differ; every other stored field must remain the frozen context.
            var normalized = pending.CopyForReceiptComparison();
            RequirePcReceipt(JsonUtility.ToJson(normalized) == JsonUtility.ToJson(context.pending),
                "pending context differs from committed RNG result");
        }

        private PendingSpinRecord CreatePcReceiptPendingSpin()
        {
            if (pcPreparedSpinDraw == null) return null;
            PcRngDrawReceipt receipt = pcOutcome.Receipt;
            RequirePcReceipt(receipt != null && receipt.context.operationId == pcPreparedSpinDraw.pending.id &&
                receipt.context.contextJson == pcPreparedSpinContextRaw &&
                session.SpinPending && receipt.effectiveResult == session.PendingPocket,
                "draw returned without the prepared durable result");
            var record = CreatePendingSpinRecord(pcPreparedSpinDraw.pending.id,
                checked(session.Credit + session.TotalBet), session.PendingPocket);
            RequireMatchingReceiptPending(record, pcPreparedSpinDraw, receipt);
            return record;
        }

        private void AcknowledgePcSpinDraw()
        {
            if (pcOutcome == null) return;
            PcRngDrawReceipt receipt = pcOutcome.Receipt;
            PcSpinDrawContext context = pcPreparedSpinDraw ?? pcRestoredSpinDraw;
            if (receipt == null || receipt.context.kind != 1 || context == null) return;
            RequireMatchingReceiptPending(pendingSpin, context, receipt);
            string formatRaw;
            DecodePcSpinDrawContext(receipt.context.contextJson, receipt, out formatRaw);
            RequirePendingSpinFormatMatchesReceipt(receipt, formatRaw);
            RequirePcReceipt(session.SpinPending && session.PendingPocket == receipt.effectiveResult,
                "pending result did not restore to the live session");
            pcOutcome.Acknowledge(receipt.context.operationId, receipt.contextSha256);
            pcPreparedSpinDraw = null;
            pcPreparedSpinContextRaw = null;
            pcRestoredSpinDraw = null;
            pcRestoredSpinReceipt = null;
            Debug.Log("ROULETTE_RNG_RECEIPT_PENDING_ACCEPTED: id=" + receipt.context.operationId);
            LifecycleCheckpoint("RngPendingAccepted");
        }
    }
}
