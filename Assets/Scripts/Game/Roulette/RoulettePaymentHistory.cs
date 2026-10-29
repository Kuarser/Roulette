using System;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class RecordedHistoryOverflowV1
        {
            public RecordedRound round;
            public string snapshotBase64;
        }

        private RecordedHistoryOverflowV1 pcHistoryOverflow;

        private bool HasOwnedPcHistoryOverflow
        {
            get
            {
                return pcHistoryOverflow != null &&
                    PcCurrentResultHistoryUnpublished &&
                    recordedSnapshots.Count == 50 && recordedRoundPages.Count == 50;
            }
        }

        private bool IsCommittedHistoryRecord(int recordIndex)
        {
            if (recordIndex < 0 || recordIndex >= recordedSnapshots.Count) return false;
            if (recordIndex == 0 && PcCurrentResultHistoryUnpublished) return false;
            RecordedRound round = recordIndex < recordedRoundPages.Count ?
                recordedRoundPages[recordIndex] : null;
            // Old snapshot-only entries and old feature hiding retain their
            // existing meanings. New payment visibility is owned by v3.
            return round == null || !round.awaitingFinalPayout;
        }

        private int CommittedHistoryCount()
        {
            int count = 0;
            for (int i = 0; i < recordedSnapshots.Count; ++i)
                if (IsCommittedHistoryRecord(i)) ++count;
            return count + (HasOwnedPcHistoryOverflow ? 1 : 0);
        }

        private int HistoryRecordIndex(int page)
        {
            if (page < 0) return -1;
            int visible = 0;
            for (int i = 0; i < recordedSnapshots.Count; ++i)
                if (IsCommittedHistoryRecord(i) && visible++ == page) return i;
            if (HasOwnedPcHistoryOverflow && visible == page) return recordedSnapshots.Count;
            return -1;
        }

        // Record indices address the retained list, then the one owned
        // overflow pair. -1 is empty. Public consumers must resolve both
        // objects here instead of indexing either physical list themselves.
        private bool TryGetHistoryRecord(int recordIndex,
            out RecordedRound round, out byte[] snapshot)
        {
            round = null;
            snapshot = null;
            if (IsCommittedHistoryRecord(recordIndex))
            {
                round = recordIndex < recordedRoundPages.Count ?
                    recordedRoundPages[recordIndex] : null;
                snapshot = recordedSnapshots[recordIndex];
                return true;
            }
            if (recordIndex != recordedSnapshots.Count || !HasOwnedPcHistoryOverflow) return false;
            round = pcHistoryOverflow.round;
            snapshot = DecodePcHistoryOverflowSnapshot(pcHistoryOverflow.snapshotBase64);
            return true;
        }

        private static byte[] DecodePcHistoryOverflowSnapshot(string encoded)
        {
            RequireLifecycle(!string.IsNullOrEmpty(encoded), "History overflow snapshot is missing");
            byte[] snapshot;
            try { snapshot = Convert.FromBase64String(encoded); }
            catch (FormatException error)
            {
                throw new PcRoundLifecycleDataException("History overflow snapshot is not Base64", error);
            }
            RequireLifecycle(snapshot.Length == HistogramCapacity,
                "History overflow snapshot has an invalid length");
            foreach (byte pocket in snapshot)
                RequireLifecycle(pocket <= 36 || pocket == 0xff,
                    "History overflow snapshot contains an invalid pocket");
            return snapshot;
        }

        private static void ValidatePcHistoryOverflow(RecordedHistoryOverflowV1 overflow,
            RecordedRound[] rounds, IList<byte[]> snapshots, bool latestUnpublished)
        {
            if (overflow == null) return;
            RequireLifecycle(latestUnpublished && rounds != null && snapshots != null &&
                rounds.Length == 50 && snapshots.Count == 50 &&
                rounds[0] != null && rounds[0].present,
                "History overflow has no full unpublished current-round owner");
            RecordedRound round = overflow.round;
            RequireLifecycle(round != null && !round.awaitingFinalPayout,
                "History overflow page is missing or unpublished");
            byte[] snapshot = DecodePcHistoryOverflowSnapshot(overflow.snapshotBase64);
            if (!round.present)
            {
                RequireLifecycle(IsDefaultLifecyclePage(round),
                    "History overflow placeholder contains nondefault detail");
            }
            else
            {
                RequireLifecycle(IsEmptyRecordedDouble(round.doubleUp) || round.doubleUp.complete,
                    "History overflow contains an unfinished feature");
                // The existing legacy checker may normalize a page. Validate
                // a copy and reject any required semantic repair; observing
                // or validating overflow never mutates the archived page.
                RecordedRound copy = JsonUtility.FromJson<RecordedRound>(JsonUtility.ToJson(round));
                if (!copy.hasDoubleUp && IsEmptyRecordedDouble(copy.doubleUp)) copy.doubleUp = null;
                string before = JsonUtility.ToJson(copy);
                RequireLifecycle(IsValidRecordedRound(copy, snapshot), "History overflow page is invalid");
                RequireLifecycle(JsonUtility.ToJson(copy) == before,
                    "History overflow page requires legacy normalization");
            }
            var roundIds = new HashSet<string>(StringComparer.Ordinal);
            var spinIds = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i <= rounds.Length; ++i)
            {
                RecordedRound item = i < rounds.Length ? rounds[i] : round;
                RequireLifecycle(item != null, "History overflow validation encountered a null page");
                if (!string.IsNullOrEmpty(item.roundArchiveId))
                    RequireLifecycle(PcRoundLifecycleRecord.IsCanonicalId(item.roundArchiveId) &&
                        roundIds.Add(item.roundArchiveId), "History overflow has an invalid or duplicate round identity");
                if (!string.IsNullOrEmpty(item.spinJournalId))
                    RequireLifecycle(PcRoundLifecycleRecord.IsCanonicalId(item.spinJournalId) &&
                        spinIds.Add(item.spinJournalId), "History overflow has an invalid or duplicate spin identity");
            }
        }
    }
}
