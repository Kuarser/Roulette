using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Native ClearGameResults saves its empty current result and paid
        // meter independently of archived rounds. The PC keeps that view
        // state separate from the accounting RoundResult shared by History.
        private bool currentGameResultsCleared;
        // Native Roulette +0x91ab is independent of completed-game +0x919d.
        // Rejected table events clear the view but do not set this edit flag.
        private bool sourceBetsChangedSincePlay;

        // Roulette::HandleRouletteTableEvent clears the current result for
        // every event except chip selection (12), before updating its wager
        // or showing a rejection. NeighborsGadget validation failures do not
        // emit that event; their callers opt out of rejection cleanup.
        private bool TrySourceTableEvent(Action action, AudioClip actionSound = null,
            AudioClip failureSound = null, bool clearRejectedResult = true,
            bool requestsBetState = false, bool undoEvent = false)
        {
            if (PcGameOrdinaryInputBlocked || SourceRoundBlocksOrdinaryInput) return false;
            // The callback can change the PC ResultPhase before returning.
            // ClearPaid's gameplay Intro depends on the preceding native
            // state (Idle=1), not on the newly accepted wager's state.
            bool wasIdle = sourceGameplayState == SourceGameplayState.Idle;
            return Try(() => {
                try { action(); }
                catch (BetRejectedException)
                {
                    if (clearRejectedResult) ClearSourceTableEventResult(wasIdle);
                    throw;
                }
                sourceBetsChangedSincePlay = true;
                ClearSourceTableEventResult(wasIdle);
                if (requestsBetState || undoEvent && session.LastUndoRequestsBetState)
                    RequestSourceBetState();
            }, actionSound, failureSound);
        }

        private void ClearSourceTableEventResult(bool wasIdle)
        {
            ApplyClearedGameResultsPresentation(false);
            if (wasIdle && !auNationalStandardsEnabled) StartFooterMovie();
            // Apply also finalizes the old feature-offer metadata. Save the
            // view only after that archive write, including legacy digests.
            SaveClearedGameResults();
        }

        private string GameResultsClearedKey
        {
            get { return recordedHistoryKey + ".CurrentResultPresentation.v1"; }
        }

        [Serializable]
        private sealed class ClearedGameResultsRecord
        {
            public int version;
            public string profile;
            public string roundArchiveId;
            public string legacyDigest;
            public bool cleared;
        }

        private bool TryGetCurrentPresentationArchive(out RecordedRound latest)
        {
            latest = null;
            if (session == null || session.LastResult == null ||
                session.SpinPending || resumePendingSpin || recordedRoundPages.Count == 0)
                return false;
            RecordedRound candidate = recordedRoundPages[0];
            // Loaded entries have already passed IsValidRecordedRound;
            // newly recorded entries were made from the settled session.
            if (candidate == null || !candidate.present ||
                candidate.profile != session.Profile.Id ||
                candidate.pocket != session.LastResult.Pocket)
                return false;
            latest = candidate;
            return true;
        }

        private static string LegacyPresentationDigest(RecordedRound latest)
        {
            // Do not assign a new ID to a legacy archive: its existing ID is
            // also part of the credit and staged-bet journal anchors. Hash
            // the validated, canonical record without changing any field.
            string canonical = JsonUtility.ToJson(latest);
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(canonical))).Replace("-", "");
        }

        private void SaveClearedGameResults()
        {
            if (temporaryHistory || !currentGameResultsCleared) return;
            RecordedRound latest;
            // Before the first result the ordinary startup view is already
            // empty, so there is no archived result to suppress on restart.
            if (!TryGetCurrentPresentationArchive(out latest)) return;
            bool legacy = string.IsNullOrEmpty(latest.roundArchiveId);
            var record = new ClearedGameResultsRecord {
                version = 1,
                profile = latest.profile,
                roundArchiveId = legacy ? "" : latest.roundArchiveId,
                legacyDigest = legacy ? LegacyPresentationDigest(latest) : "",
                cleared = true
            };
            try
            {
                PlayerPrefs.SetString(GameResultsClearedKey, JsonUtility.ToJson(record));
                PlayerPrefs.Save();
                Debug.Log("ROULETTE_CURRENT_RESULT_PRESENTATION_SAVED: legacy=" + legacy);
            }
            catch (Exception error)
            {
                // This record never controls funds or archive recovery.
                Debug.LogWarning("ROULETTE_CURRENT_RESULT_PRESENTATION_SAVE_FAILED: " + error.Message);
            }
        }

        private void RestoreClearedGameResults()
        {
            currentGameResultsCleared = false;
            if (temporaryHistory || session == null || session.SpinPending || resumePendingSpin)
                return;

            ClearedGameResultsRecord record;
            try
            {
                string raw = PlayerPrefs.GetString(GameResultsClearedKey, "");
                if (string.IsNullOrEmpty(raw)) return;
                record = JsonUtility.FromJson<ClearedGameResultsRecord>(raw);
            }
            catch (Exception error)
            {
                Debug.LogWarning("ROULETTE_CURRENT_RESULT_PRESENTATION_IGNORED: unreadable " + error.Message);
                return;
            }
            if (record == null || record.version != 1 || !record.cleared ||
                string.IsNullOrEmpty(record.profile) ||
                (string.IsNullOrEmpty(record.roundArchiveId) == string.IsNullOrEmpty(record.legacyDigest)))
            {
                Debug.LogWarning("ROULETTE_CURRENT_RESULT_PRESENTATION_IGNORED: invalid record");
                return;
            }

            RecordedRound latest;
            if (!TryGetCurrentPresentationArchive(out latest) || record.profile != latest.profile)
            {
                Debug.LogWarning("ROULETTE_CURRENT_RESULT_PRESENTATION_IGNORED: archive/profile changed");
                return;
            }
            bool legacy = string.IsNullOrEmpty(latest.roundArchiveId);
            bool matches = legacy ?
                string.IsNullOrEmpty(record.roundArchiveId) &&
                    string.Equals(record.legacyDigest, LegacyPresentationDigest(latest), StringComparison.Ordinal) :
                string.IsNullOrEmpty(record.legacyDigest) &&
                    string.Equals(record.roundArchiveId, latest.roundArchiveId, StringComparison.Ordinal);
            if (!matches)
            {
                Debug.LogWarning("ROULETTE_CURRENT_RESULT_PRESENTATION_IGNORED: archived round changed");
                return;
            }

            currentGameResultsCleared = true;
            // LoadProfile calls this after staged-bet restoration. The source
            // presentation helper owns the state-1/AU footer replay condition.
            ApplyClearedGameResultsPresentation(true, true);
            Debug.Log("ROULETTE_CURRENT_RESULT_PRESENTATION_RESTORED: legacy=" + legacy);
        }

        private void RestoreEditedTableResultPresentation()
        {
            if (!sourceBetsChangedSincePlay || currentGameResultsCleared ||
                session == null || session.LastResult == null || session.SpinPending ||
                resumePendingSpin || session.InDoubleUp) return;
            // A durable accepted table edit independently proves that the
            // native callback cleared its live result. This also handles a
            // missing view record without reviving the old award or marker.
            ApplyClearedGameResultsPresentation(true, true);
            SaveClearedGameResults();
            Debug.Log("ROULETTE_EDITED_TABLE_RESULT_PRESENTATION_RESTORED");
        }

        private void ResetClearedGameResultsForNewRound()
        {
            currentGameResultsCleared = false;
            if (temporaryHistory) return;
            // The caller has durably recorded the new result. Even if this
            // cleanup fails, its new archive ID cannot match the old record.
            try
            {
                if (!PlayerPrefs.HasKey(GameResultsClearedKey)) return;
                PlayerPrefs.DeleteKey(GameResultsClearedKey);
                PlayerPrefs.Save();
            }
            catch (Exception error)
            {
                Debug.LogWarning("ROULETTE_CURRENT_RESULT_PRESENTATION_RESET_FAILED: " + error.Message);
            }
        }
    }
}
