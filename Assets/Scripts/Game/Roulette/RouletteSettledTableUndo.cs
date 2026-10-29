using System;
using System.Linq;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Native RouletteData saves the current table's ten Undo entries
        // independently of settlement history. Keep optional PC metadata out
        // of RecordedRound so older financial records and their presentation
        // digests retain their existing serialized shape.
        [Serializable]
        private sealed class SettledTableUndoRecord
        {
            public int version;
            public string profile;
            public string roundArchiveId;
            public int tableLastTouched;
            public RecordedUndo[] tableUndo;
        }

        private string SettledTableUndoKey
        {
            get { return recordedHistoryKey + ".SettledTableUndo.v1"; }
        }

        private bool SaveSettledTableUndo(IRouletteSession source)
        {
            if (temporaryHistory) return false;
            var local = source as RouletteSession;
            if (local == null || local.SpinPending || local.LastResult == null ||
                recordedRoundPages.Count == 0) return false;
            RecordedRound latest = recordedRoundPages[0];
            Guid archiveId;
            if (latest == null || !latest.present ||
                latest.profile != local.Profile.Id ||
                latest.pocket != local.LastResult.Pocket ||
                !Guid.TryParseExact(latest.roundArchiveId, "N", out archiveId)) return false;
            try
            {
                var record = new SettledTableUndoRecord {
                    version = 1,
                    profile = latest.profile,
                    roundArchiveId = latest.roundArchiveId,
                    tableLastTouched = local.LastTouchedBet,
                    tableUndo = EncodeTableUndo(local)
                };
                // RecordPocket has already committed the financial archive.
                // If this separate write fails, an older record's GUID cannot
                // apply its queue to the newly settled table.
                LifecycleBeforeWrite("SettledUndo");
                PlayerPrefs.SetString(SettledTableUndoKey, JsonUtility.ToJson(record));
                PlayerPrefs.Save();
                Debug.Log("ROULETTE_SETTLED_TABLE_UNDO_SAVED: count=" + record.tableUndo.Length);
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("ROULETTE_SETTLED_TABLE_UNDO_SAVE_FAILED: " + error.Message);
                return false;
            }
        }

        private bool RestoreSettledTableUndo()
        {
            var local = session as RouletteSession;
            if (local == null || temporaryHistory) return false;
            local.RestoreTableUndoState(-1, new RouletteSession.SavedUndoState[0]);
            try
            {
                string raw = PlayerPrefs.GetString(SettledTableUndoKey, "");
                if (string.IsNullOrEmpty(raw)) return false;
                SettledTableUndoRecord record = JsonUtility.FromJson<SettledTableUndoRecord>(raw);
                RecordedRound latest = recordedRoundPages.Count == 0 ? null : recordedRoundPages[0];
                Guid archiveId;
                if (record == null || record.version != 1 || latest == null ||
                    !latest.present || local.LastResult == null ||
                    latest.pocket != local.LastResult.Pocket ||
                    record.profile != local.Profile.Id || record.profile != latest.profile ||
                    !Guid.TryParseExact(record.roundArchiveId, "N", out archiveId) ||
                    !string.Equals(record.roundArchiveId, latest.roundArchiveId,
                        StringComparison.Ordinal))
                    throw new InvalidOperationException("table Undo archive anchor differs");
                return RestoreOptionalTableUndo(local, true, record.tableLastTouched,
                    record.tableUndo, "SETTLED");
            }
            catch (Exception error)
            {
                // Preserve the valid settled money/result and the raw optional
                // record for inspection. No invented Undo for legacy archives.
                Debug.LogWarning("ROULETTE_SETTLED_TABLE_UNDO_IGNORED: " + error.Message);
                return false;
            }
        }

        private bool TryRecoverSettledTableUndoFromPending(PendingSpinRecord record,
            out bool saved)
        {
            saved = false;
            var local = session as RouletteSession;
            RecordedRound latest = recordedRoundPages.Count == 0 ? null : recordedRoundPages[0];
            if (record == null || !record.hasTableUndo || local == null ||
                latest == null || !latest.present) return false;
            try
            {
                Guid archiveId;
                if (local.LastResult == null || local.SpinPending ||
                    !Guid.TryParseExact(latest.roundArchiveId, "N", out archiveId) ||
                    latest.spinJournalId != record.id ||
                    record.profile != latest.profile || record.profile != local.Profile.Id ||
                    record.pocket != latest.pocket || record.pocket != local.LastResult.Pocket ||
                    record.wager != latest.wager || local.TotalBet != latest.wager ||
                    record.denomination != latest.denomination)
                    throw new InvalidOperationException("settled pending Undo anchor differs");
                var pendingStacks = DecodeStagedBets(record.bets);
                var currentStacks = local.BetChipStacks;
                if (pendingStacks.Count != currentStacks.Count || pendingStacks.Any(pair =>
                    !currentStacks.ContainsKey(pair.Key) ||
                    !pair.Value.SequenceEqual(currentStacks[pair.Key])))
                    throw new InvalidOperationException("settled pending Undo table differs");
                if (!RestoreOptionalTableUndo(local, true, record.tableLastTouched,
                    record.tableUndo, "SETTLED_PENDING")) return false;
                // The matching spin is already paid. Only hand off its Undo
                // queue, and commit that copy before deleting the old journal.
                saved = SaveSettledTableUndo(local);
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("ROULETTE_SETTLED_PENDING_TABLE_UNDO_IGNORED: " + error.Message);
                return false;
            }
        }

        private static bool RestoreOptionalTableUndo(RouletteSession local,
            bool hasTableUndo, int touched, RecordedUndo[] records, string origin)
        {
            // Pending recovery replaces the settled table. Even an older
            // pending journal without this optional field must not inherit
            // the previous archive's unrelated queue.
            local.RestoreTableUndoState(-1, new RouletteSession.SavedUndoState[0]);
            if (!hasTableUndo) return false;
            try
            {
                local.RestoreTableUndoState(touched, DecodeTableUndo(records));
                Debug.Log("ROULETTE_" + origin + "_TABLE_UNDO_RESTORED: count=" + records.Length);
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("ROULETTE_" + origin + "_TABLE_UNDO_IGNORED: " + error.Message);
                return false;
            }
        }
    }
}
