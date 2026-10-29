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
        private sealed class PendingSpinRecord
        {
            public int version;
            public string id;
            public string profile;
            public int profileIndex;
            public int pocket;
            public bool hasWheelStopIndex;
            public int wheelStopIndex;
            public long creditBefore;
            public long wager;
            public long denomination;
            public long moneyRemainderCents;
            public long selectedChip;
            public int baseHistoryCount;
            public int baseLatestPocket;
            public string baseLatestRoundId;
            public RecordedBet[] bets;
            public int[] evaluationOrder;
            public bool hasTableUndo;
            public int tableLastTouched;
            public RecordedUndo[] tableUndo;
            public PendingSpinRecord CopyForReceiptComparison()
            {
                // Only these scalars are normalized. Shared arrays remain read-only.
                var copy=(PendingSpinRecord)MemberwiseClone();
                copy.pocket=-1;copy.hasWheelStopIndex=false;copy.wheelStopIndex=0;return copy;
            }
        }

        private PendingSpinRecord pendingSpin;
        private bool resumePendingSpin;

        private string PendingSpinKey { get { return recordedHistoryKey + ".PendingSpin.v1"; } }

        private void LoadPendingSpinJournal()
        {
            pendingSpin = null;
            pcSpinRoundFormatContextRaw = null;
            if (temporaryHistory) return;
            string encoded = PlayerPrefs.GetString(PendingSpinKey, "");
            if (string.IsNullOrEmpty(encoded)) return;
            try { pendingSpin = DecodePendingSpinEnvelope(encoded, out pcSpinRoundFormatContextRaw); }
            catch (ArgumentException error)
            {
                throw new InvalidOperationException("Pending roulette spin is unreadable", error);
            }
            if (pendingSpin == null || pendingSpin.version != 1 ||
                string.IsNullOrEmpty(pendingSpin.id) || pendingSpin.id.Length != 32 ||
                pendingSpin.profileIndex < 0 || pendingSpin.profileIndex >= profiles.Length ||
                pendingSpin.pocket < 0 || pendingSpin.pocket > 36 ||
                (pendingSpin.hasWheelStopIndex &&
                 (pendingSpin.wheelStopIndex < 0 || pendingSpin.wheelStopIndex >= 37)) ||
                pendingSpin.creditBefore < 1 || pendingSpin.wager < 1 ||
                pendingSpin.denomination < 1 ||
                pendingSpin.moneyRemainderCents < 0 ||
                pendingSpin.moneyRemainderCents >= pendingSpin.denomination ||
                pendingSpin.bets == null || pendingSpin.bets.Length == 0)
                throw new InvalidOperationException("Pending roulette spin is invalid");
        }

        private void SavePendingSpin()
        {
            if (temporaryHistory) return;
            if (pendingSpin != null || session == null || !session.SpinPending)
                throw new InvalidOperationException("Pending roulette spin state is invalid");
            PendingSpinRecord record = CreatePcReceiptPendingSpin() ?? CreatePendingSpinRecord(
                LifecycleSuccessorSpinId(), checked(session.Credit + session.TotalBet), session.PendingPocket);
            SavePcPendingSpinValue(record, pcSpinRoundFormatContextRaw);
            pendingSpin = record;
            LifecycleCheckpoint("PendingSaved");
            Debug.Log("ROULETTE_PENDING_SPIN_SAVED: id=" + record.id +
                " pocket=" + record.pocket + " wager=" + record.wager);
        }

        private PendingSpinRecord CreatePendingSpinRecord(string id, long creditBefore, int pocket)
        {
            var local = session as RouletteSession;
            var record = new PendingSpinRecord {
                version = 1,
                id = id,
                profile = session.Profile.Id,
                profileIndex = profileIndex,
                pocket = pocket,
                creditBefore = creditBefore,
                wager = session.TotalBet,
                denomination = session.Denomination,
                moneyRemainderCents =
                    (session as RouletteSession)?.MoneyRemainderCents ?? 0,
                selectedChip = session.Chip,
                baseHistoryCount = recordedRoundPages.Count,
                baseLatestPocket = recordedRoundPages.Count == 0 ? -1 :
                    recordedRoundPages[0].pocket,
                baseLatestRoundId = recordedRoundPages.Count == 0 ? "" :
                    recordedRoundPages[0].roundArchiveId ?? "",
                evaluationOrder = (session as RouletteSession)?.BetEvaluationOrder,
                hasTableUndo = local != null,
                tableLastTouched = local == null ? -1 : local.LastTouchedBet,
                tableUndo = local == null ? null : EncodeTableUndo(local),
                bets = session.Bets.OrderBy(pair => pair.Key).Select(pair => {
                    List<long> chips;
                    return new RecordedBet {
                        id = pair.Key,
                        amount = pair.Value,
                        chips = session.BetChipStacks.TryGetValue(pair.Key, out chips) ?
                            chips.ToArray() : new long[0]
                    };
                }).ToArray()
            };
            return record;
        }

        private void SavePendingWheelStopIndex(int stopIndex)
        {
            if (temporaryHistory) return;
            if (pendingSpin == null || stopIndex < 0 || stopIndex >= 37)
                throw new InvalidOperationException("Pending wheel stop is invalid");
            var candidate = JsonUtility.FromJson<PendingSpinRecord>(JsonUtility.ToJson(pendingSpin));
            candidate.wheelStopIndex = stopIndex;
            candidate.hasWheelStopIndex = true;
            SavePcPendingSpinValue(candidate, pcSpinRoundFormatContextRaw);
            pendingSpin = candidate;
            Debug.Log("ROULETTE_PENDING_WHEEL_STOP_SAVED: index=" + stopIndex);
        }

        private bool TryRestorePendingSpin(bool archivedRoundRestored,
            bool settledTableUndoRestored)
        {
            if (pendingSpin == null || temporaryHistory) return false;
            // A crash after history commit but before journal deletion must
            // not pay or increment the same result a second time.
            if (recordedRoundPages.Count > 0 &&
                recordedRoundPages[0].spinJournalId == pendingSpin.id)
            {
                if (session == null || session.LastResult == null)
                    throw new InvalidOperationException("Settled spin history did not restore");
                bool undoSaved = false;
                bool undoRecovered = !settledTableUndoRestored && archivedRoundRestored &&
                    TryRecoverSettledTableUndoFromPending(pendingSpin, out undoSaved);
                if (undoRecovered && !undoSaved)
                {
                    // Keep the sole durable Undo copy for a later restart.
                    // Financial recovery is complete; release the in-memory
                    // journal so staged table recovery can still take priority.
                    string id = pendingSpin.id;
                    pendingSpin = null;
                    pcSpinRoundFormatContextRaw = null;
                    resumePendingSpin = false;
                    Debug.LogWarning("ROULETTE_PENDING_UNDO_RETRY_RETAINED: id=" + id);
                }
                else ClearPendingSpin();
                Debug.Log("ROULETTE_PENDING_SPIN_ALREADY_SETTLED");
                return false;
            }
            if (session == null || pendingSpin.profile != session.Profile.Id ||
                pendingSpin.profileIndex != profileIndex ||
                pendingSpin.baseHistoryCount != recordedRoundPages.Count ||
                pendingSpin.baseLatestPocket !=
                    (recordedRoundPages.Count == 0 ? -1 : recordedRoundPages[0].pocket))
                throw new InvalidOperationException("Pending spin disagrees with saved session");
            if (HasTrustedLatestLifecycle)
            {
                PcRoundLifecycleRecord predecessor = GetLatestLifecycleForRestore();
                RequireLifecycle(predecessor.roundStage == PcRoundStage.Ended &&
                    pendingSpin.baseLatestRoundId == predecessor.roundArchiveId,
                    "Pending spin has no strong predecessor identity");
                ValidateIdleLifecycleSuccessor(predecessor, pendingSpin.id);
            }
            if (pendingSpin.creditBefore != session.Credit ||
                pendingSpin.denomination != session.Denomination ||
                pendingSpin.moneyRemainderCents !=
                    ((RouletteSession)session).MoneyRemainderCents)
            {
                // The PC testbed has no credit-insertion command between
                // archived rounds. A different balance with the same archive
                // baseline is therefore an orphaned local journal. Preserve
                // its full bytes before reopening the archived round.
                if (!archivedRoundRestored || recordedRoundPages.Count == 0)
                    throw new InvalidOperationException(
                        "Pending spin credit disagrees with saved session");
                QuarantinePendingSpin("credit baseline differs from archived round");
                return false;
            }
            var stacks = new Dictionary<int, List<long>>();
            long wager = 0;
            foreach (RecordedBet bet in pendingSpin.bets)
            {
                if (bet == null || bet.chips == null || bet.chips.Length == 0)
                    throw new InvalidOperationException("Pending spin contains an invalid bet");
                long amount = 0;
                foreach (long chip in bet.chips) amount = checked(amount + chip);
                if (amount != bet.amount)
                    throw new InvalidOperationException("Pending spin wager disagrees with chips");
                stacks.Add(bet.id, new List<long>(bet.chips));
                wager = checked(wager + amount);
            }
            if (wager != pendingSpin.wager)
                throw new InvalidOperationException("Pending spin wager disagrees with archive");
            ((RouletteSession)session).RestorePendingSpin(
                pendingSpin.pocket, pendingSpin.creditBefore,
                pendingSpin.denomination, pendingSpin.selectedChip, stacks,
                pendingSpin.evaluationOrder, pendingSpin.moneyRemainderCents);
            // Optional table metadata is validated after the financial
            // recovery and cannot reject a valid committed wager.
            RestoreOptionalTableUndo((RouletteSession)session,
                pendingSpin.hasTableUndo, pendingSpin.tableLastTouched,
                pendingSpin.tableUndo, "PENDING");
            if (pendingSpin.hasWheelStopIndex)
            {
                wheelAngle = OriginalWheelArithmetic.RestorePhase(pendingSpin.wheelStopIndex);
                wheelRenderedAngle = wheelAngle;
                Debug.Log("ROULETTE_PENDING_WHEEL_STOP_RESTORED: index=" +
                    pendingSpin.wheelStopIndex);
            }
            Debug.Log("ROULETTE_PENDING_SPIN_RESTORED: id=" + pendingSpin.id +
                " pocket=" + session.PendingPocket + " credit=" + session.Credit);
            return true;
        }

        private void ClearPendingSpin()
        {
            pendingSpin = null;
            pcSpinRoundFormatContextRaw = null;
            resumePendingSpin = false;
            if (temporaryHistory) return;
            PlayerPrefs.DeleteKey(PendingSpinKey);
            PlayerPrefs.Save();
        }

        private void ReleaseSettledPendingSpin()
        {
            if (retainSettledPendingForUndo && pendingSpin != null)
            {
                // Finance is already archived. Keep the sole durable Undo
                // copy just as the cold already-settled recovery path does.
                Debug.LogWarning("ROULETTE_PENDING_UNDO_RETRY_RETAINED: id=" + pendingSpin.id);
                pendingSpin = null;
                pcSpinRoundFormatContextRaw = null;
                resumePendingSpin = false;
                retainSettledPendingForUndo = false;
                return;
            }
            retainSettledPendingForUndo = false;
            ClearPendingSpin();
        }

        private string PendingSpinQuarantineKey(string id)
        {
            return PendingSpinKey + ".Quarantine." + id;
        }

        private void QuarantinePendingSpin(string reason)
        {
            RequirePcReceipt(pcRestoredSpinReceipt == null,
                "durable result cannot be quarantined for a different financial baseline");
            string encoded = PlayerPrefs.GetString(PendingSpinKey, "");
            if (pendingSpin == null || string.IsNullOrEmpty(encoded))
                throw new InvalidOperationException("Pending spin journal cannot be preserved");
            string quarantineKey = PendingSpinQuarantineKey(pendingSpin.id);
            string existing = PlayerPrefs.GetString(quarantineKey, "");
            if (!string.IsNullOrEmpty(existing) && existing != encoded)
                throw new InvalidOperationException("Pending spin quarantine ID conflicts");
            PlayerPrefs.SetString(quarantineKey, encoded);
            PlayerPrefs.Save();
            if (PlayerPrefs.GetString(quarantineKey, "") != encoded)
                throw new InvalidOperationException("Pending spin quarantine was not saved");
            PlayerPrefs.DeleteKey(PendingSpinKey);
            PlayerPrefs.Save();
            Debug.LogWarning("ROULETTE_PENDING_SPIN_QUARANTINED: id=" + pendingSpin.id +
                " reason=" + reason);
            pendingSpin = null;
            resumePendingSpin = false;
            pcSpinRoundFormatContextRaw = null;
        }

        private IEnumerator PendingSpinSaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.PendingSpinTest", StringComparison.Ordinal))
                throw new InvalidOperationException("Pending spin smoke needs isolated key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            ClearPendingSpin();
            PlayerPrefs.DeleteKey(recordedHistoryKey);
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".Pages");
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".RoundDetails.v1");
            PlayerPrefs.Save();
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.PlaceBet(101, 1);
            session.PlaceBet(100, 1);
            if (session.BeginSpin() != 0) throw new InvalidOperationException("Unexpected test pocket");
            SavePendingSpin();
            SavePendingWheelStopIndex(11);
            if (!session.SpinPending || session.Credit != 997 ||
                pendingSpin == null || pendingSpin.wager != 3 ||
                !pendingSpin.hasWheelStopIndex || pendingSpin.wheelStopIndex != 11 ||
                !pendingSpin.hasTableUndo || pendingSpin.tableLastTouched != 100 ||
                pendingSpin.tableUndo == null || pendingSpin.tableUndo.Length != 3 ||
                !pendingSpin.tableUndo.Select(entry => entry.sourceSpotPlusOne)
                    .SequenceEqual(new[] { 1, 2, 1 }) ||
                pendingSpin.evaluationOrder == null ||
                !pendingSpin.evaluationOrder.SequenceEqual(new[] { 101, 100 }))
                throw new InvalidOperationException("Pending spin was not saved");
            Debug.Log("ROULETTE_PENDING_SAVE_PASSED: credit=997 pocket=0 order=101,100 undo=3 sourceSpot=0");
            Application.Quit();
        }

        private IEnumerator PendingSpinLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.PendingSpinTest", StringComparison.Ordinal) ||
                !session.SpinPending || session.PendingPocket != 0 ||
                session.Credit != 997 || pendingSpin == null ||
                !pendingSpin.hasWheelStopIndex || pendingSpin.wheelStopIndex != 11 ||
                !PendingSpinSmokeHasOriginalUndo() ||
                Math.Abs(wheelAngle - 11d * WheelStepDegrees * 2d) > .00001d ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 100, 101 }))
                throw new InvalidOperationException("Pending spin did not restore");
            RoundResult result = session.CompleteSpin();
            RecordPocket(result.Pocket, result);
            // Deliberately leave the journal, as if power failed after the
            // history write and before its normal deletion.
            if (session.Credit != 1069 || session.Rounds != 1 ||
                recordedRoundPages.Count != 1 ||
                recordedRoundPages[0].spinJournalId != pendingSpin.id ||
                !PendingSpinSmokeHasOriginalUndo() ||
                recordedRoundPages[0].evaluationOrder == null ||
                !recordedRoundPages[0].evaluationOrder.SequenceEqual(new[] { 100, 101 }))
                throw new InvalidOperationException("Pending spin did not settle once");
            // Simulate the narrower crash window after the financial archive
            // commits but before its optional table Undo copy becomes durable.
            string pendingRaw = PlayerPrefs.GetString(PendingSpinKey, "");
            if (string.IsNullOrEmpty(pendingRaw) || !PlayerPrefs.HasKey(SettledTableUndoKey))
                throw new InvalidOperationException("Pending Undo fallback setup lacks its journals");
            PlayerPrefs.SetString(recordedHistoryKey + ".ExpectedUndoFallbackPending", pendingRaw);
            PlayerPrefs.DeleteKey(SettledTableUndoKey);
            PlayerPrefs.Save();
            Debug.Log("ROULETTE_PENDING_LOAD_PASSED: credit=1069 rounds=1 order=100,101 undo=3 sourceSpot=0 sidecar=missing pendingRetained=true");
            Application.Quit();
        }

        private IEnumerator PendingSpinAlreadySettledSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.PendingSpinTest", StringComparison.Ordinal) ||
                pendingSpin != null || session.SpinPending || session.Credit != 1069 ||
                session.Rounds != 1 || recordedRoundPages.Count != 1 ||
                !PendingSpinSmokeHasOriginalUndo() ||
                PlayerPrefs.HasKey(PendingSpinKey) || !PlayerPrefs.HasKey(SettledTableUndoKey) ||
                !((RouletteSession)session).BetEvaluationOrder.SequenceEqual(
                    new[] { 100, 101 }))
                throw new InvalidOperationException("Committed spin was replayed");
            string pendingRaw = PlayerPrefs.GetString(
                recordedHistoryKey + ".ExpectedUndoFallbackPending", "");
            string fullSidecar = PlayerPrefs.GetString(SettledTableUndoKey, "");
            string archive = JsonUtility.ToJson(new RecordedRoundArchive {
                rounds = recordedRoundPages.ToArray()
            });
            if (string.IsNullOrEmpty(pendingRaw) || string.IsNullOrEmpty(fullSidecar))
                throw new InvalidOperationException("Settled Undo fallback did not persist its replacement");
            IRouletteOutcomeSource originalOutcome = outcome;
            var forbidden = new PaytableTestForbiddenOutcome();
            outcome = forbidden;
            try
            {
                // An explicitly saved empty queue is valid metadata. It must
                // win over the older nonempty queue still in the spin journal.
                var empty = JsonUtility.FromJson<SettledTableUndoRecord>(fullSidecar);
                empty.tableLastTouched = -1;
                empty.tableUndo = new RecordedUndo[0];
                PlayerPrefs.SetString(SettledTableUndoKey, JsonUtility.ToJson(empty));
                PlayerPrefs.SetString(PendingSpinKey, pendingRaw);
                PlayerPrefs.Save();
                LoadProfile();
                if (!PendingSpinSmokeSettledFundsIntact(archive) || session.CanUndo ||
                    ((RouletteSession)session).LastTouchedBet != -1 ||
                    ((RouletteSession)session).ExportUndoHistory().Length != 0 ||
                    PlayerPrefs.HasKey(PendingSpinKey) || forbidden.Requests != 0)
                    throw new InvalidOperationException("Valid empty sidecar lost priority over pending Undo");

                // A matching sidecar whose optional queue is invalid is not a
                // successful restore; the matching pending copy repairs it.
                var invalid = JsonUtility.FromJson<SettledTableUndoRecord>(fullSidecar);
                invalid.tableUndo[2].sourceSpotPlusOne = session.Profile.Records.Count + 1;
                PlayerPrefs.SetString(SettledTableUndoKey, JsonUtility.ToJson(invalid));
                PlayerPrefs.SetString(PendingSpinKey, pendingRaw);
                PlayerPrefs.Save();
                LoadProfile();
                if (!PendingSpinSmokeSettledFundsIntact(archive) ||
                    !PendingSpinSmokeHasOriginalUndo() || PlayerPrefs.HasKey(PendingSpinKey) ||
                    !PlayerPrefs.HasKey(SettledTableUndoKey) || forbidden.Requests != 0)
                    throw new InvalidOperationException("Invalid sidecar prevented settled pending Undo recovery");
            }
            finally { outcome = originalOutcome; }
            Debug.Log("ROULETTE_PENDING_ALREADY_SETTLED_PASSED: credit=1069 rounds=1 order=100,101 undo=3 sourceSpot=0 missingFallback=true invalidFallback=true emptySidecarPriority=true recoveryDraws=0");
            Application.Quit();
        }

        private bool PendingSpinSmokeSettledFundsIntact(string archive)
        {
            return session != null && !session.SpinPending && !session.InDoubleUp &&
                pendingSpin == null && session.Credit == 1069 && session.Denomination == 1 &&
                ((RouletteSession)session).MoneyRemainderCents == 0 &&
                session.TotalBet == 3 && session.Wagered == 3 && session.Returned == 72 &&
                session.Rounds == 1 && session.Wins == 1 && session.LastResult != null &&
                session.LastResult.Pocket == 0 && session.LastResult.Wager == 3 &&
                session.LastResult.Returned == 72 &&
                JsonUtility.ToJson(new RecordedRoundArchive {
                    rounds = recordedRoundPages.ToArray()
                }) == archive;
        }

        private bool PendingSpinSmokeHasOriginalUndo()
        {
            var local = session as RouletteSession;
            if (local == null || !local.CanUndo || local.LastTouchedBet != 100 ||
                local.LastUndoSourceSpot != 0) return false;
            RouletteSession.SavedUndoState[] undo = local.ExportUndoHistory();
            return undo.Length == 3 &&
                undo.Select(entry => entry.SourceSpotPlusOne).SequenceEqual(new[] { 1, 2, 1 }) &&
                undo.Select(entry => entry.LastTouched).SequenceEqual(new[] { -1, 100, 101 }) &&
                undo[0].Stacks.Count == 0 && undo[1].Stacks.Count == 1 &&
                undo[1].Stacks.ContainsKey(100) && undo[1].Stacks[100].SequenceEqual(new long[] { 1 }) &&
                undo[2].Stacks.Count == 2 && undo[2].Stacks.ContainsKey(100) &&
                undo[2].Stacks.ContainsKey(101) && undo[2].Stacks[100].SequenceEqual(new long[] { 1 }) &&
                undo[2].Stacks[101].SequenceEqual(new long[] { 1 });
        }

        private IEnumerator PendingSpinStaleSaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.PendingStaleTest",
                StringComparison.Ordinal))
                throw new InvalidOperationException("Stale pending smoke needs isolated key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            ClearPendingSpin();
            PlayerPrefs.DeleteKey(recordedHistoryKey);
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".Pages");
            PlayerPrefs.DeleteKey(recordedHistoryKey + ".RoundDetails.v1");
            PlayerPrefs.Save();

            var completed = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(6));
            completed.PlaceBet(100, 1);
            RoundResult result = completed.Spin();
            RecordPocket(result.Pocket, result, completed);
            if (result.Pocket != 6 || completed.Credit != 999 ||
                recordedRoundPages.Count != 1)
                throw new InvalidOperationException("Stale pending baseline was not saved");

            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(35));
            session.PlaceBet(100, 1);
            session.BeginSpin();
            SavePendingSpin();
            string encoded = PlayerPrefs.GetString(PendingSpinKey, "");
            PlayerPrefs.SetString(recordedHistoryKey + ".ExpectedPendingRaw", encoded);
            PlayerPrefs.SetString(recordedHistoryKey + ".ExpectedPendingId", pendingSpin.id);
            PlayerPrefs.Save();
            if (pendingSpin.creditBefore != 1000 ||
                recordedRoundPages[0].creditAfter != 999)
                throw new InvalidOperationException("Stale pending mismatch not established");
            Debug.Log("ROULETTE_PENDING_STALE_SAVE_PASSED: archive=999 journal=1000");
            Application.Quit();
        }

        private IEnumerator PendingSpinStaleLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.PendingStaleTest",
                StringComparison.Ordinal))
                throw new InvalidOperationException("Stale pending smoke needs isolated key");
            string expected = PlayerPrefs.GetString(
                recordedHistoryKey + ".ExpectedPendingRaw", "");
            string id = PlayerPrefs.GetString(recordedHistoryKey + ".ExpectedPendingId", "");
            if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(id) ||
                session == null || session.Credit != 999 || session.LastResult == null ||
                session.LastResult.Pocket != 6 || session.Rounds != 1 ||
                session.SpinPending || pendingSpin != null ||
                !string.IsNullOrEmpty(PlayerPrefs.GetString(PendingSpinKey, "")) ||
                PlayerPrefs.GetString(PendingSpinQuarantineKey(id), "") != expected)
                throw new InvalidOperationException("Stale journal was not safely quarantined");
            Debug.Log("ROULETTE_PENDING_STALE_LOAD_PASSED: credit=999 rounds=1 preserved=" + id);
            Application.Quit();
        }
    }
}
