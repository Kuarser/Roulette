using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class RecordedUndo
        {
            public int lastTouched;
            public int sourceSpotPlusOne;
            public bool playsRemovalSound;
            public int[] evaluationOrder;
            public int[] groupRemovalOrder;
            public RecordedBet[] bets;
        }

        [Serializable]
        private sealed class StagedBetRecord
        {
            public int version;
            public string profile;
            public int profileIndex;
            public long credit;
            public long denomination;
            public long moneyRemainderCents;
            public long selectedChip;
            public int baseHistoryCount;
            public int baseLatestPocket;
            public string baseLatestRoundId;
            public RecordedBet[] bets;
            public int[] evaluationOrder;
            public bool hasUndoState;
            public int lastTouched;
            public RecordedUndo[] undo;
            public bool hasSourceTableEditState;
            public bool sourceTableEdited;
            public int sourceGameplayState;
        }

        private StagedBetRecord stagedBets;
        private string stagedBetsJson;
        private bool restoredExplicitSourceIdleState;
        private string StagedBetKey { get { return recordedHistoryKey + ".StagedBets.v1"; } }

        private void LoadStagedBetJournal()
        {
            stagedBets = null;
            stagedBetsJson = null;
            if (temporaryHistory) return;
            string encoded = PlayerPrefs.GetString(StagedBetKey, "");
            if (string.IsNullOrEmpty(encoded)) return;
            try { stagedBets = JsonUtility.FromJson<StagedBetRecord>(encoded); }
            catch (ArgumentException error)
            {
                throw new InvalidOperationException("Saved roulette table is unreadable", error);
            }
            if (stagedBets == null || stagedBets.version != 1 ||
                stagedBets.profileIndex < 0 || stagedBets.profileIndex >= profiles.Length ||
                stagedBets.profile != "RouletteDeluxeSZ" + profiles[stagedBets.profileIndex] ||
                stagedBets.credit < 0 || stagedBets.denomination < 1 ||
                stagedBets.moneyRemainderCents < 0 ||
                stagedBets.moneyRemainderCents >= stagedBets.denomination ||
                stagedBets.bets == null ||
                stagedBets.bets.Length > 157)
                throw new InvalidOperationException("Saved roulette table is invalid");
            if (!StagedBaselineMatches(stagedBets))
            {
                Debug.LogWarning("ROULETTE_STAGED_BETS_STALE: archived round advanced");
                ClearStagedBets();
                return;
            }
            stagedBetsJson = encoded;
        }

        private bool StagedBaselineMatches(StagedBetRecord record)
        {
            int latestPocket = recordedRoundPages.Count == 0 ? -1 :
                recordedRoundPages[0].pocket;
            string latestId = recordedRoundPages.Count == 0 ? "" :
                recordedRoundPages[0].roundArchiveId ?? "";
            return record.baseHistoryCount == recordedRoundPages.Count &&
                record.baseLatestPocket == latestPocket &&
                (record.baseLatestRoundId ?? "") == latestId;
        }

        private void SaveStagedBetsIfChanged()
        {
            if (temporaryHistory || session == null || session.SpinPending ||
                session.InDoubleUp || session.ResultPhase || pendingSpin != null ||
                sourceGameplayState == SourceGameplayState.Playing ||
                !(session is RouletteSession))
                return;
            var local = (RouletteSession)session;
            var record = new StagedBetRecord {
                version = 1,
                profile = session.Profile.Id,
                profileIndex = profileIndex,
                credit = session.Credit,
                denomination = session.Denomination,
                moneyRemainderCents = local.MoneyRemainderCents,
                selectedChip = session.Chip,
                baseHistoryCount = recordedRoundPages.Count,
                baseLatestPocket = recordedRoundPages.Count == 0 ? -1 :
                    recordedRoundPages[0].pocket,
                baseLatestRoundId = recordedRoundPages.Count == 0 ? "" :
                    recordedRoundPages[0].roundArchiveId ?? "",
                bets = EncodeStagedBets(session.BetChipStacks),
                evaluationOrder = local.BetEvaluationOrder,
                hasUndoState = true,
                hasSourceTableEditState = true,
                sourceTableEdited = sourceBetsChangedSincePlay,
                sourceGameplayState = (int)this.sourceGameplayState,
                lastTouched = local.LastTouchedBet,
                undo = EncodeTableUndo(local)
            };
            string encoded = JsonUtility.ToJson(record);
            if (encoded == stagedBetsJson) return;
            PlayerPrefs.SetString(StagedBetKey, encoded);
            PlayerPrefs.Save();
            stagedBets = record;
            stagedBetsJson = encoded;
            Debug.Log("ROULETTE_STAGED_BETS_SAVED: spots=" + record.bets.Length +
                " wager=" + session.TotalBet);
        }

        private bool TryRestoreStagedBets()
        {
            if (stagedBets == null || temporaryHistory) return false;
            if (session == null || stagedBets.profile != session.Profile.Id ||
                stagedBets.profileIndex != profileIndex ||
                !StagedBaselineMatches(stagedBets))
                throw new InvalidOperationException("Saved roulette table disagrees with archive");
            var stacks = DecodeStagedBets(stagedBets.bets);
            bool moneyEvent = pcCreditOverride != null &&
                pcCreditOverride.version == 2 &&
                pcCreditOverride.anchor == PcCreditAnchor &&
                pcCreditOverride.profile == session.Profile.Id;
            if (moneyEvent && pcCreditOverride.kind == "denom" &&
                stacks.Count != 0)
                throw new InvalidOperationException(
                    "Denomination transaction conflicts with a staged wager");
            RouletteSession.SavedUndoState[] undo = stagedBets.hasUndoState ?
                DecodeTableUndo(stagedBets.undo) : new RouletteSession.SavedUndoState[0];
            var local = (RouletteSession)session;
            local.RestoreStagedBets(
                stagedBets.credit,
                moneyEvent ? session.Denomination : stagedBets.denomination,
                moneyEvent && pcCreditOverride.kind == "denom" ?
                    1 : stagedBets.selectedChip,
                stacks,
                moneyEvent && pcCreditOverride.kind == "denom" ?
                    -1 : stagedBets.hasUndoState ? stagedBets.lastTouched : -1,
                moneyEvent && pcCreditOverride.kind == "denom" ?
                    new RouletteSession.SavedUndoState[0] : undo,
                stagedBets.evaluationOrder,
                PcCreditAllowsStagedCredit(stagedBets.credit),
                moneyEvent ? local.MoneyRemainderCents :
                    stagedBets.moneyRemainderCents);
            // Preserve the loaded state evidence before money-event repair
            // can replace this record with a newly written PC Idle fallback.
            restoredExplicitSourceIdleState = stagedBets.sourceGameplayState == 1;
            sourceBetsChangedSincePlay = stagedBets.hasSourceTableEditState ?
                stagedBets.sourceTableEdited : LegacyStagedTableDiffersFromArchive(stagedBets);
            // State::Open restores a separate state value. Empty wagers can
            // still be state 2; retained completed wagers can be state 1.
            // Old PC journals did not record this distinction, so keep their
            // startup Idle fallback rather than inventing it from the wager.
            sourceGameplayState = stagedBets.sourceGameplayState == 2 ?
                SourceGameplayState.Bet : SourceGameplayState.Idle;
            sourceBetStateEntryPending = sourceGameplayState == SourceGameplayState.Bet;
            if (stagedBets.sourceGameplayState != 0 &&
                stagedBets.sourceGameplayState != 1 && stagedBets.sourceGameplayState != 2)
                Debug.LogWarning("ROULETTE_SOURCE_STATE_IGNORED: invalid staged state");
            if (moneyEvent && (stagedBets.credit != session.Credit ||
                stagedBets.denomination != session.Denomination ||
                stagedBets.moneyRemainderCents != local.MoneyRemainderCents))
                SaveStagedBetsIfChanged();
            Debug.Log("ROULETTE_STAGED_BETS_RESTORED: spots=" + stacks.Count +
                " wager=" + session.TotalBet + " credit=" + session.Credit +
                " undo=" + undo.Length);
            return true;
        }

        private static RecordedUndo[] EncodeTableUndo(RouletteSession local)
        {
            return local.ExportUndoHistory().Select(state => new RecordedUndo {
                lastTouched = state.LastTouched,
                sourceSpotPlusOne = state.SourceSpotPlusOne,
                playsRemovalSound = state.PlaysRemovalSound,
                evaluationOrder = state.Order,
                groupRemovalOrder = state.GroupRemovalOrder,
                bets = EncodeStagedBets(state.Stacks)
            }).ToArray();
        }

        private static RouletteSession.SavedUndoState[] DecodeTableUndo(RecordedUndo[] records)
        {
            if (records == null || records.Length > 10)
                throw new InvalidOperationException("Saved roulette undo queue is invalid");
            var undo = new List<RouletteSession.SavedUndoState>(records.Length);
            foreach (RecordedUndo entry in records)
            {
                if (entry == null)
                    throw new InvalidOperationException("Saved roulette undo entry is missing");
                undo.Add(new RouletteSession.SavedUndoState {
                    Stacks = DecodeStagedBets(entry.bets),
                    Order = entry.evaluationOrder,
                    GroupRemovalOrder = entry.groupRemovalOrder,
                    LastTouched = entry.lastTouched,
                    SourceSpotPlusOne = entry.sourceSpotPlusOne,
                    PlaysRemovalSound = entry.playsRemovalSound
                });
            }
            return undo.ToArray();
        }

        private bool LegacyStagedTableDiffersFromArchive(StagedBetRecord record)
        {
            // Older PC journals also stored compatibility NEXT/TAKE WIN
            // transitions. Only a different table proves a wager edit when
            // the explicit source flag is absent; ResultPhase alone cannot.
            if (recordedRoundPages.Count == 0 || !recordedRoundPages[0].present)
                return record.bets.Length != 0;
            RecordedBet[] prior = recordedRoundPages[0].bets;
            if (prior == null || prior.Length != record.bets.Length) return true;
            foreach (RecordedBet bet in record.bets)
            {
                RecordedBet old = Array.Find(prior, item => item.id == bet.id);
                if (old == null || old.amount != bet.amount || old.chips == null ||
                    !old.chips.SequenceEqual(bet.chips)) return true;
            }
            return false;
        }

        private static RecordedBet[] EncodeStagedBets(
            Dictionary<int, List<long>> stacks)
        {
            return stacks.OrderBy(pair => pair.Key).Select(pair =>
                new RecordedBet {
                    id = pair.Key,
                    amount = pair.Value.Sum(),
                    chips = pair.Value.ToArray()
                }).ToArray();
        }

        private static Dictionary<int, List<long>> DecodeStagedBets(
            RecordedBet[] bets)
        {
            if (bets == null)
                throw new InvalidOperationException("Saved roulette bet list is missing");
            var stacks = new Dictionary<int, List<long>>();
            foreach (RecordedBet bet in bets)
            {
                if (bet == null || bet.chips == null || bet.chips.Length == 0)
                    throw new InvalidOperationException("Saved roulette table contains an invalid bet");
                long amount = 0;
                foreach (long chip in bet.chips) amount = checked(amount + chip);
                if (bet.amount != amount || stacks.ContainsKey(bet.id))
                    throw new InvalidOperationException("Saved roulette table chip sum disagrees");
                stacks.Add(bet.id, new List<long>(bet.chips));
            }
            return stacks;
        }

        private void ClearStagedBets()
        {
            stagedBets = null;
            stagedBetsJson = null;
            if (temporaryHistory) return;
            PlayerPrefs.DeleteKey(StagedBetKey);
            PlayerPrefs.Save();
        }

        private IEnumerator StagedBetsSaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.StagedBetTest", StringComparison.Ordinal))
                throw new InvalidOperationException("Staged bet smoke needs isolated key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            ClearStagedBets();
            ClearPendingSpin();
            PlayerPrefs.DeleteKey(recordedHistoryKey);
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".Pages");
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".RoundDetails.v1");
            PlayerPrefs.Save();
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            if (!Try(() => session.PlaceBet(100, 1)) ||
                !Try(() => session.SetChip(2)) ||
                !Try(() => session.PlaceBet(100, 2)) ||
                !Try(() => session.Clear()) || session.CanUndo ||
                !Try(() => session.PlaceBet(101, 2)) ||
                !Try(() => session.PlaceBet(100, 1)) ||
                !Try(() => session.PlaceBet(100, 2)) ||
                stagedBets == null || stagedBets.bets.Length != 2 ||
                session.TotalBet != 5 || session.Credit != 1000 ||
                stagedBets.undo == null || stagedBets.undo.Length != 3 ||
                stagedBets.lastTouched != 100 ||
                stagedBets.evaluationOrder == null ||
                !stagedBets.evaluationOrder.SequenceEqual(new[] { 101, 100 }))
                throw new InvalidOperationException("Staged roulette table did not save");
            Debug.Log("ROULETTE_STAGED_SAVE_PASSED: credit=1000 wager=5 spots=2");
            Application.Quit();
        }

        private IEnumerator StagedBetsLoadSmoke()
        {
            yield return null;
            List<long> chips;
            if (!recordedHistoryKey.EndsWith(".Test.StagedBetTest", StringComparison.Ordinal) ||
                session == null || session.Credit != 1000 || session.TotalBet != 5 ||
                session.Chip != 2 || session.Bets.Count != 2 ||
                !session.BetChipStacks.TryGetValue(100, out chips) ||
                chips.Count != 2 || chips[0] != 1 || chips[1] != 2 ||
                session.SpinPending || session.Rounds != 0 ||
                !session.CanUndo ||
                ((RouletteSession)session).LastTouchedBet != 100 ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 100, 101 }))
                throw new InvalidOperationException("Staged roulette table did not restore");
            if (!Try(() => session.Undo()) || session.TotalBet != 3 ||
                !Try(() => session.Undo()) || session.TotalBet != 2 ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 101 }) ||
                !Try(() => session.PlaceBet(100, 1)) ||
                !Try(() => session.PlaceBet(100, 2)) || session.TotalBet != 5)
                throw new InvalidOperationException("Restored undo queue did not replay");
            string oldTable = stagedBetsJson;
            int pocket = session.BeginSpin();
            if (pocket < 0 || pocket > 36)
                throw new InvalidOperationException("Restored table could not spin");
            SavePendingSpin();
            ClearStagedBets();
            if (PlayerPrefs.HasKey(StagedBetKey) || pendingSpin == null ||
                session.Credit != 995)
                throw new InvalidOperationException("Staged table was not transferred to spin");
            RoundResult result = session.CompleteSpin();
            RecordPocket(result.Pocket, result);
            ClearPendingSpin();
            // Simulate deletion being interrupted after the new round archive
            // was written. The old table must not be applied to new credit.
            PlayerPrefs.SetString(StagedBetKey, oldTable);
            PlayerPrefs.Save();
            LoadStagedBetJournal();
            if (stagedBets != null || PlayerPrefs.HasKey(StagedBetKey))
                throw new InvalidOperationException("Stale staged table was replayed");
            Debug.Log("ROULETTE_STAGED_LOAD_PASSED: credit=1000 wager=5 spinCredit=995 stale=discarded");
            Application.Quit();
        }

        private IEnumerator GroupUndoSaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.GroupUndoOrderTest",
                StringComparison.Ordinal))
                throw new InvalidOperationException("Group undo smoke needs isolated key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            ClearStagedBets();
            ClearPendingSpin();
            PlayerPrefs.DeleteKey(recordedHistoryKey);
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".Pages");
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".RoundDetails.v1");
            PlayerPrefs.Save();
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            if (!Try(() => session.PlaceBet(200, 1)) ||
                !Try(() => session.PlaceBet(178, 1)) ||
                !Try(() => session.PlaceBet(100, 1)) ||
                !Try(() => session.PlaceSector(RouletteSector.Tiers)) ||
                stagedBets == null || stagedBets.undo == null ||
                stagedBets.undo.Length != 4 ||
                stagedBets.undo[3].sourceSpotPlusOne != 126 ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 200, 178, 100, 208, 187, 222, 228 }) ||
                stagedBets.undo[3].groupRemovalOrder == null ||
                !stagedBets.undo[3].groupRemovalOrder.SequenceEqual(
                    OriginalSectorBets.Spots(RouletteSector.Tiers)
                        .Select(spot => 100 + spot)))
                throw new InvalidOperationException("Group undo sequence was not saved");
            Debug.Log("ROULETTE_GROUP_UNDO_SAVE_PASSED: order=" +
                string.Join(",", ((RouletteSession)session).BetEvaluationOrder
                    .Select(id => id.ToString()).ToArray()));
            Application.Quit();
        }

        private IEnumerator GroupUndoLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.GroupUndoOrderTest",
                StringComparison.Ordinal) || session == null ||
                session.Credit != 1000 || session.TotalBet != 9 ||
                !session.CanUndo ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 100, 178, 187, 200, 208, 222, 228 }) ||
                !((RouletteSession)session).ExportUndoHistory().Last()
                    .GroupRemovalOrder.SequenceEqual(
                        OriginalSectorBets.Spots(RouletteSector.Tiers)
                            .Select(spot => 100 + spot)) ||
                ((RouletteSession)session).ExportUndoHistory().Last()
                    .SourceSpotPlusOne != 126 ||
                !Try(() => session.Undo()) ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 100, 200, 178 }) ||
                session.TotalBet != 3 || session.Bets.Count != 3)
                throw new InvalidOperationException("Restored group undo order changed");
            Debug.Log("ROULETTE_GROUP_UNDO_LOAD_PASSED: order=100,200,178 wager=3");
            Application.Quit();
        }
    }
}
