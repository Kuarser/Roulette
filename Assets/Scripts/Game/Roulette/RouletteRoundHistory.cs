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
        private sealed class RecordedBet
        {
            public int id;
            public long amount;
            public long[] chips;
        }

        [Serializable]
        private sealed class RecordedPay
        {
            public int id;
            public long amount;
        }

        [Serializable]
        private sealed class RecordedHistoryPayOverride
        {
            public int payIndex;
            public long amount;
            public int progressiveLevel;
            public int kind;
        }

        [Serializable]
        private sealed class RecordedDoubleStep
        {
            // Source choice codes: club=0, spade=1, diamond=2,
            // heart=3, black=4, red=5. Dealer is 0..3.
            public int choice;
            public int dealer;
            public long wager;
            public long won;
        }

        [Serializable]
        private sealed class RecordedDouble
        {
            public long startingStake;
            public bool suitMode;
            public RecordedDoubleStep[] steps;
            public bool tookWin;
            public bool complete;
            public long finalWin;
        }

        [Serializable]
        private sealed class RecordedRound
        {
            public bool present;
            public int pocket;
            public long wager;
            public long returned;
            public int category;
            public long denomination;
            public long creditAfter;
            public long moneyRemainderCents;
            public bool totalsRecorded;
            public long totalWagered;
            public long totalReturned;
            public long totalRounds;
            public int totalWins;
            public float wheelAngle;
            public float ballX;
            public float ballY;
            public bool sourceWheelStopRecorded;
            public int sourceWheelStopIndex;
            public string profile;
            public long selectedChip;
            public RecordedBet[] bets;
            public RecordedPay[] winningPays;
            public RecordedHistoryPayOverride[] historyPayOverrides;
            public int[] evaluationOrder;
            public string sourceTableBlock;
            public string spinJournalId;
            public string roundArchiveId;
            public bool hasDoubleUp;
            public RecordedDouble doubleUp;
            public bool awaitingFinalPayout;
        }

        [Serializable]
        private sealed class RecordedRoundArchive
        {
            public RecordedRound[] rounds;
        }

        private readonly List<RecordedRound> recordedRoundPages =
            new List<RecordedRound>();

        private void SelectInitialProfileFromRecordedRound()
        {
            if(sasStartupArchiveRaw!=null)
            {
                for(int index=0;index<profiles.Length;index++)if(sasStartupProfile=="RouletteDeluxeSZ"+profiles[index]){profileIndex=index;return;}
                throw new PcRoundLifecycleDataException("Staged SAS profile unavailable");
            }
            if (temporaryHistory || recordedRoundPages.Count == 0) return;
            RecordedRound latest = recordedRoundPages[0];
            if (latest == null || !latest.present) return;
            for (int index = 0; index < profiles.Length; ++index)
            {
                if (latest.profile != "RouletteDeluxeSZ" + profiles[index]) continue;
                profileIndex = index;
                Debug.Log("ROULETTE_PROFILE_FROM_ARCHIVE: " + profiles[index]);
                return;
            }
        }

        private void LoadRecordedRoundPages()
        {
            recordedRoundPages.Clear();
            latestRoundLifecycle = null;
            pcDoubleFeatureNv = null;
            pcGamePayment = null;
            pcHistoryOverflow = null;
            pcRoundArchiveSchemaVersion = 1;
            sasStartupArchiveRaw=null;sasStartupProfile=null;
            string encoded = PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1", "");
            if (!temporaryHistory && !string.IsNullOrEmpty(encoded))
            {
                try
                {
                    if(StageSasStartupArchiveV4(encoded))return;
                    RecordedRound[] validatedRounds;
                    PcRoundLifecycleRecord validatedLifecycle;
                    PcDoubleFeatureNvV1 validatedFeature;
                    PcGamePaymentRecordV1 validatedPayment;
                    RecordedHistoryOverflowV1 validatedOverflow;
                    int validatedSchema;
                    if (TryDecodeLifecycleArchiveWithPayment(encoded, recordedSnapshots,
                        out validatedRounds, out validatedLifecycle, out validatedFeature,
                        out validatedPayment, out validatedOverflow, out validatedSchema))
                    {
                        RequireLifecycle(!recordedRawHistoryInvalid && recordedPockets.Count > 0,
                            "Versioned round archive has corrupt raw pocket history");
                        byte[] latestSnapshot = recordedSnapshots[0];
                        for (int i = 0; i < latestSnapshot.Length; ++i)
                            RequireLifecycle(latestSnapshot[i] == (i < recordedPockets.Count ?
                                (byte)recordedPockets[i] : (byte)0xff),
                                "Versioned round archive pocket list disagrees with its snapshot");
                        recordedRoundPages.AddRange(validatedRounds);
                        latestRoundLifecycle = validatedLifecycle;
                        pcDoubleFeatureNv = validatedFeature;
                        pcGamePayment = validatedPayment;
                        pcHistoryOverflow = validatedOverflow;
                        pcRoundArchiveSchemaVersion = validatedSchema;
                        return;
                    }
                    RecordedRoundArchive archive =
                        JsonUtility.FromJson<RecordedRoundArchive>(encoded);
                    if (archive != null && archive.rounds != null &&
                        archive.rounds.Length == recordedSnapshots.Count)
                    {
                        for (int i = 0; i < archive.rounds.Length; ++i)
                        {
                            RecordedRound round = archive.rounds[i];
                            recordedRoundPages.Add(IsValidRecordedRound(round,
                                recordedSnapshots[i]) ? round : new RecordedRound());
                        }
                    }
                }
                catch (ArgumentException) { }
            }
            while (recordedRoundPages.Count < recordedSnapshots.Count)
                recordedRoundPages.Add(new RecordedRound());
        }

        private static bool IsValidRecordedRound(RecordedRound round, byte[] snapshot)
        {
            return IsValidRecordedRoundWithFeature(round, snapshot, false);
        }

        private static bool IsValidRecordedRoundWithFeature(RecordedRound round, byte[] snapshot,
            bool allowDeferredTerminal)
        {
            if (round == null || !round.present) return false;
            // JsonUtility materializes a null nested class as a zero-valued
            // object. Older normal-round archives have no explicit marker.
            if (!round.hasDoubleUp && IsEmptyRecordedDouble(round.doubleUp))
                round.doubleUp = null;
            else if (round.doubleUp != null)
                round.hasDoubleUp = true;
            if (round.hasDoubleUp && round.doubleUp == null) return false;
            if (snapshot == null || snapshot.Length != HistogramCapacity ||
                round.pocket != snapshot[0] || round.pocket < 0 || round.pocket > 36 ||
                round.wager < 1 || round.returned < 0 ||
                round.denomination < 1 || round.creditAfter < 0 ||
                round.moneyRemainderCents < 0 ||
                round.moneyRemainderCents >= round.denomination ||
                float.IsNaN(round.wheelAngle) || float.IsInfinity(round.wheelAngle) ||
                float.IsNaN(round.ballX) || float.IsInfinity(round.ballX) ||
                float.IsNaN(round.ballY) || float.IsInfinity(round.ballY) ||
                (round.sourceWheelStopRecorded &&
                    (round.sourceWheelStopIndex < 0 || round.sourceWheelStopIndex >= 37)) ||
                round.ballX < 0 || round.ballX > 1080 ||
                round.ballY < 0 || round.ballY > 1920 ||
                round.selectedChip < 0 ||
                string.IsNullOrEmpty(round.profile) || round.bets == null ||
                round.bets.Length < 1 || round.bets.Length > 157)
                return false;
            long total = 0;
            var used = new HashSet<int>();
            foreach (RecordedBet bet in round.bets)
            {
                if (bet == null || bet.id < 100 || bet.id > 256 || bet.amount < 1 ||
                    bet.chips == null || bet.chips.Length == 0 ||
                    !used.Add(bet.id) || bet.amount > long.MaxValue - total)
                    return false;
                total += bet.amount;
                long chipsTotal = 0;
                foreach (long chip in bet.chips)
                {
                    if (chip < 1 || chip > long.MaxValue - chipsTotal)
                        return false;
                    chipsTotal += chip;
                }
                if (chipsTotal != bet.amount) return false;
            }
            if (total != round.wager) return false;
            if (round.winningPays != null && round.winningPays.Length > 0)
            {
                if (round.winningPays.Length > round.bets.Length) return false;
                RouletteProfile savedProfile;
                try
                {
                    const string prefix = "RouletteDeluxeSZ";
                    if (!round.profile.StartsWith(prefix, StringComparison.Ordinal))
                        return false;
                    savedProfile = RouletteProfile.Load(
                        round.profile.Substring(prefix.Length));
                }
                catch (Exception) { return false; }
                var winningIds = new HashSet<int>();
                foreach (RecordedPay pay in round.winningPays)
                {
                    WinRecord record;
                    if (pay == null || pay.amount < 0 ||
                        !used.Contains(pay.id) || !winningIds.Add(pay.id) ||
                        !savedProfile.Records.TryGetValue(pay.id, out record) ||
                        !record.Covers(round.pocket)) return false;
                }
            }
            if (round.historyPayOverrides != null &&
                round.historyPayOverrides.Length > 0)
            {
                if (round.historyPayOverrides.Length > 11) return false;
                RouletteProfile savedProfile;
                try
                {
                    const string prefix = "RouletteDeluxeSZ";
                    if (!round.profile.StartsWith(prefix, StringComparison.Ordinal))
                        return false;
                    savedProfile = RouletteProfile.Load(
                        round.profile.Substring(prefix.Length));
                }
                catch (Exception) { return false; }
                int payCount = round.bets.Count(bet =>
                    (bet.amount & 0xff) != 0 &&
                    savedProfile.Records.ContainsKey(bet.id) &&
                    savedProfile.Records[bet.id].Covers(round.pocket));
                foreach (RecordedHistoryPayOverride entry in round.historyPayOverrides)
                    if (entry == null || entry.payIndex < 0 ||
                        entry.payIndex >= payCount || entry.amount < 0 ||
                        entry.progressiveLevel < 0 || entry.progressiveLevel > 255 ||
                        entry.kind < 0 || entry.kind > 255) return false;
            }
            if (!IsValidRecordedDoubleWithFeature(round.doubleUp, allowDeferredTerminal) ||
                (round.doubleUp != null && !round.doubleUp.complete &&
                    round.doubleUp.startingStake != round.returned)) return false;
            if (round.doubleUp != null && !round.doubleUp.complete)
                round.awaitingFinalPayout = true;
            if (round.doubleUp != null && round.doubleUp.complete &&
                round.returned != round.doubleUp.finalWin)
            {
                // Earlier PC archives saved the base roulette result before
                // the feature. Upgrade only that exact previous encoding.
                if (round.returned != round.doubleUp.startingStake ||
                    round.creditAfter < round.doubleUp.startingStake ||
                    (round.totalsRecorded &&
                     round.totalReturned < round.doubleUp.startingStake)) return false;
                try
                {
                    round.creditAfter = checked(round.creditAfter -
                        round.doubleUp.startingStake + round.doubleUp.finalWin);
                    if (round.totalsRecorded)
                        round.totalReturned = checked(round.totalReturned -
                            round.doubleUp.startingStake + round.doubleUp.finalWin);
                }
                catch (OverflowException) { return false; }
                round.returned = round.doubleUp.finalWin;
            }
            if (round.totalsRecorded &&
                (round.totalWagered < round.wager ||
                 round.totalReturned < round.returned ||
                 round.totalRounds < 1 || round.totalWins < 0 ||
                 round.totalWins > round.totalRounds ||
                 ((round.returned > 0 || round.doubleUp != null) &&
                  round.totalWins < 1))) return false;
            if (string.IsNullOrEmpty(round.sourceTableBlock)) return true;
            try
            {
                byte[] saved = Convert.FromBase64String(round.sourceTableBlock);
                byte[] expected = EncodeSourceTableBlock(round.bets);
                if (BlocksEqual(saved, expected)) return true;
                // The first PC archive revision indexed this array with the
                // paytable ID instead of the source table's zero-based ID.
                // Recognize only that exact old encoding before migrating it.
                if (!BlocksEqual(saved, EncodeLegacyTableBlock(round.bets)))
                    return false;
                round.sourceTableBlock = Convert.ToBase64String(expected);
                return true;
            }
            catch (FormatException) { return false; }
        }

        private static bool IsValidRecordedDouble(RecordedDouble feature)
        {
            return IsValidRecordedDoubleWithFeature(feature, false);
        }

        private static bool IsValidRecordedDoubleWithFeature(RecordedDouble feature, bool allowDeferredTerminal)
        {
            if (feature == null) return true;
            if (feature.startingStake < 1 || feature.steps == null ||
                feature.steps.Length > 16 || feature.finalWin < 0)
                return false;
            long stake = feature.startingStake;
            bool lost = false;
            foreach (RecordedDoubleStep step in feature.steps)
            {
                if (lost || step == null || step.choice < 0 || step.choice > 5 ||
                    (!feature.suitMode && step.choice < 4) ||
                    step.dealer < 0 || step.dealer > 3 || step.wager != stake)
                    return false;
                bool won = step.choice == 5 ? step.dealer >= 2 :
                    step.choice == 4 ? step.dealer < 2 : step.choice == step.dealer;
                long award;
                try { award = won ? checked(stake * (step.choice < 4 ? 4 : 2)) : 0; }
                catch (OverflowException) { return false; }
                if (step.won != award) return false;
                lost = !won;
                stake = award;
            }
            if (feature.tookWin && (lost || feature.finalWin != stake)) return false;
            if (lost && feature.finalWin != 0) return false;
            if (!feature.complete && (lost && !allowDeferredTerminal || feature.tookWin || feature.finalWin != 0))
                return false;
            if (feature.complete && !lost && feature.finalWin != stake) return false;
            if (feature.complete && !lost && !feature.tookWin &&
                feature.steps.Length == 0) return false;
            return true;
        }

        private static bool IsEmptyRecordedDouble(RecordedDouble feature)
        {
            return feature == null || feature.startingStake == 0 &&
                (feature.steps == null || feature.steps.Length == 0) &&
                !feature.tookWin && !feature.complete && feature.finalWin == 0;
        }

        private void SaveRecordedRoundPages()
        {
            ++recordedHistoryVersion;
            if (temporaryHistory) return;
            try
            {
                StageRecordedRoundArchive();
                PlayerPrefs.Save();
            }
            catch (Exception error)
            {
                if (latestRoundLifecycle != null) FailPcRoundLifecycle("saved", error);
                throw;
            }
        }

        private static double RestoredWheelPhase(RecordedRound round)
        {
            // RouletteWheel::Restore reads WheelData byte +1 as the wheel
            // stop index. The original history page calls Restore on a new
            // wheel; it does not reuse the last displayed animation frame.
            return round.sourceWheelStopRecorded ?
                OriginalWheelArithmetic.RestorePhase(round.sourceWheelStopIndex) :
                round.wheelAngle;
        }

        private Vector2 RestoredBallCenter(RecordedRound round)
        {
            double phase = RestoredWheelPhase(round);
            if (round.sourceWheelStopRecorded)
                return OriginalWheelArithmetic.BallCenter(SourceStopIndex(round.pocket), phase, true);
            return round.ballX > 0 && round.ballY > 0 ?
                new Vector2(round.ballX, round.ballY) :
                BallCenterForPocket(round.pocket, phase);
        }

        private void FinalizePendingHistoryRound()
        {
            // Tracked V3/V4 publication belongs exclusively to the admitted
            // payment event. The legacy path must not mutate a V4 page cache.
            if (PcPaymentTracked || pcRoundArchiveSchemaVersion==4) return;
            if (recordedRoundPages.Count == 0 || session == null ||
                session.LastResult == null || session.SpinPending ||
                session.InDoubleUp || session.DoubleAvailable ||
                IsDoubleReveal || IsDoubleExit) return;
            RecordedRound round = recordedRoundPages[0];
            if (round == null || !round.awaitingFinalPayout ||
                round.pocket != session.LastResult.Pocket) return;
            round.returned = session.LastResult.Returned;
            round.creditAfter = session.Credit;
            round.totalReturned = session.Returned;
            round.totalWins = session.Wins;
            round.awaitingFinalPayout = false;
            SaveRecordedRoundPages();
        }

        private bool TryRestoreArchivedRound()
        {
            if (pcLifecycleForceFinancialRestoreFalse) return false;
            if (temporaryHistory || recordedRoundPages.Count == 0 ||
                !(session is RouletteSession)) return false;
            RecordedRound round = recordedRoundPages[0];
            if (round == null || !round.present ||
                round.profile != session.Profile.Id ||
                (round.doubleUp != null &&
                    round.doubleUp.suitMode != session.DoubleSuitMode)) return false;
            try
            {
                var stacks = new Dictionary<int, List<long>>();
                foreach (RecordedBet bet in round.bets)
                    stacks.Add(bet.id, new List<long>(bet.chips));
                RecordedDouble feature = round.doubleUp;
                var result = new RoundResult {
                    Pocket = round.pocket, Wager = round.wager,
                    Returned = round.returned, Category = round.category
                };
                long chip = round.selectedChip > 0 ? round.selectedChip : 1;
                if (PcPaymentTracked && !pcGamePayment.moneyApplied)
                {
                    var local = (RouletteSession)session;
                    bool featureStarted = pcGamePayment.owner == PcGamePaymentOwner.Double;
                    if (featureStarted) local.EnableDeferredDoubleSettlement();
                    long stake = featureStarted && feature.steps.Length > 0 ?
                        feature.steps[feature.steps.Length - 1].won : pcGamePayment.baseAward;
                    int dealer = featureStarted && feature.steps.Length > 0 ?
                        feature.steps[feature.steps.Length - 1].dealer : -1;
                    local.RestoreUnpaidRound(result, pcGamePayment.creditBefore,
                        round.denomination, chip, stacks,
                        pcGamePayment.baseOffer == PcGamePaymentOffer.Open ||
                            pcGamePayment.baseOffer == PcGamePaymentOffer.Enter,
                        featureStarted, featureStarted ? feature.steps.Length : 0,
                        stake, dealer, round.evaluationOrder, round.moneyRemainderCents);
                    if (session.InDoubleUp)
                    {
                        doubleEntranceStartedAt = float.NegativeInfinity;
                        doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
                    }
                }
                else if (!PcPaymentTracked && round.awaitingFinalPayout &&
                    (feature == null || !feature.complete))
                {
                    if (pcRoundArchiveSchemaVersion == 2 && pcDoubleFeatureNv != null &&
                        pcDoubleFeatureNv.completionStage != 2)
                        ((RouletteSession)session).EnableDeferredDoubleSettlement();
                    long stake = feature == null || feature.steps.Length == 0 ?
                        round.returned : feature.steps[feature.steps.Length - 1].won;
                    int dealer = feature == null || feature.steps.Length == 0 ? -1 :
                        feature.steps[feature.steps.Length - 1].dealer;
                    ((RouletteSession)session).RestorePendingRound(result,
                        round.creditAfter, round.denomination, chip, stacks,
                        feature != null, feature == null ? 0 : feature.steps.Length,
                        stake, dealer, round.evaluationOrder,
                        round.moneyRemainderCents);
                    if (session.InDoubleUp)
                    {
                        doubleEntranceStartedAt = float.NegativeInfinity;
                        doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
                    }
                }
                else
                {
                    ((RouletteSession)session).RestoreCompletedRound(result,
                        feature == null ? round.returned : feature.startingStake,
                        round.creditAfter, round.denomination, chip, stacks,
                        feature != null, round.evaluationOrder,
                        round.moneyRemainderCents);
                    if (!PcPaymentTracked && round.awaitingFinalPayout)
                    {
                        round.awaitingFinalPayout = false;
                        SaveRecordedRoundPages();
                    }
                }
                if (round.totalsRecorded)
                    ((RouletteSession)session).RestoreCounters(
                        round.totalWagered, round.totalReturned,
                        round.totalRounds, round.totalWins);
                wheelRenderedAngle = RestoredWheelPhase(round);
                wheelMainRenderAngle = round.sourceWheelStopRecorded ? OriginalWheelArithmetic.RestoreMainAngle(round.sourceWheelStopIndex) : OriginalWheelArithmetic.RenderAngle(wheelRenderedAngle);
                wheelSpindleRenderAngle = OriginalWheelArithmetic.RenderAngle(wheelRenderedAngle);
                wheelAngle = wheelRenderedAngle;
                wheelBallPosition = RestoredBallCenter(round);
                wheelMotionReady = true;
                wheelBallVisible = true;
                Debug.Log("ROULETTE_ARCHIVED_ROUND_RESTORED: pocket=" + round.pocket +
                    " credit=" + session.Credit + " stake=" + session.DoubleStake +
                    " feature=" + (feature != null) +
                    " pending=" + round.awaitingFinalPayout);
                return true;
            }
            catch (Exception error)
            {
                Debug.LogWarning("Saved roulette round could not be restored: " + error.Message);
                return false;
            }
        }

        private void BeginDouble()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (SourceDoubleOfferActive) { EnterSourceDoubleOffer(); return; }
            if (SourceRoundBlocksOrdinaryInput) return;
            BeginDoubleCore();
        }

        private void BeginDoubleCore()
        {
            if (!DoubleUpInputAllowed)
                throw new InvalidOperationException("Double Up unavailable during AutoPlay");
            if(pcRoundArchiveSchemaVersion==4){BeginSasDoubleEntryPresentationV4();return;}
            long stake = session.DoubleStake;
            bool nativeFeature = HasTrustedLatestLifecycle;
            if (nativeFeature) ((RouletteSession)session).EnableDeferredDoubleSettlement();
            session.StartDouble();
            doubleEntranceStartedAt = Time.realtimeSinceStartup;
            // BeginFeature waits 1000 ms before EnableButtons/ResetLamps.
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup + 1f;
            doubleIdleAnimationStartedTimestamp = OriginalRackClock.AfterMilliseconds(
                doubleEntranceStartedTimestamp, 1000,
                System.Diagnostics.Stopwatch.Frequency);
            if (recordedRoundPages.Count == 0 || !recordedRoundPages[0].present)
                return;
            recordedRoundPages[0].doubleUp = new RecordedDouble {
                startingStake = stake,
                suitMode = session.DoubleSuitMode,
                steps = new RecordedDoubleStep[0]
            };
            recordedRoundPages[0].hasDoubleUp = true;
            if (nativeFeature) BeginPcNativeDoubleFeature(configuredAutoPlay.AutoPlay != 0);
            else SaveRecordedRoundPages();
        }

        private void RecordDoubleChoice(int choice, long wager, bool won)
        {
            if (recordedRoundPages.Count == 0 ||
                recordedRoundPages[0].doubleUp == null) return;
            RecordedDouble feature = recordedRoundPages[0].doubleUp;
            int sourceChoice = choice == -1 ? 5 : choice == -2 ? 4 :
                choice == 0 ? 3 : choice == 1 ? 2 : choice == 2 ? 0 : 1;
            var steps = new List<RecordedDoubleStep>(feature.steps);
            steps.Add(new RecordedDoubleStep {
                choice = sourceChoice,
                dealer = session.LastDoubleCard,
                wager = wager,
                won = won ? session.DoubleStake : 0
            });
            feature.steps = steps.ToArray();
            var local = session as RouletteSession;
            if (local != null && local.DoubleSettlementDeferred)
            {
                RecordPcNativeDoubleChoice(won, local.DoubleChoiceTerminal);
                LifecycleCheckpoint(local.DoubleChoiceTerminal ? "DoubleTerminalDecisionSaved" : "DoubleDecisionSaved");
                return;
            }
            if (!session.InDoubleUp)
            {
                feature.finalWin = won ? session.DoubleStake : 0;
                feature.complete = true;
                recordedRoundPages[0].returned = session.LastResult.Returned;
                recordedRoundPages[0].creditAfter = session.Credit;
                recordedRoundPages[0].totalReturned = session.Returned;
                recordedRoundPages[0].totalWins = session.Wins;
            }
            SaveRecordedRoundPages();
            if (!session.InDoubleUp) LifecycleCheckpoint("DoubleTerminalSaved");
        }

        private void RecordDoubleTakeWin(long win)
        {
            if (recordedRoundPages.Count == 0 ||
                recordedRoundPages[0].doubleUp == null) return;
            RecordedDouble feature = recordedRoundPages[0].doubleUp;
            feature.tookWin = true;
            feature.complete = true;
            feature.finalWin = win;
            recordedRoundPages[0].returned = session.LastResult.Returned;
            recordedRoundPages[0].creditAfter = session.Credit;
            recordedRoundPages[0].totalReturned = session.Returned;
            recordedRoundPages[0].totalWins = session.Wins;
            SaveRecordedRoundPages();
            LifecycleCheckpoint("DoubleTakeSaved");
        }

        private static byte[] EncodeSourceTableBlock(RecordedBet[] bets)
        {
            // Original SaveTableBets reserves two 161-ushort slices. This
            // package configures one table, leaving the second slice zero.
            var bytes = new byte[0x284];
            if (bets == null) return bytes;
            foreach (RecordedBet bet in bets)
            {
                int sourceSpotId = bet.id - 100;
                if (sourceSpotId < 0 || sourceSpotId >= 157)
                    throw new InvalidOperationException("BetSpot ID exceeds source table slice");
                bytes[sourceSpotId * 2] = (byte)(bet.amount & 0xff);
            }
            return bytes;
        }

        private static byte[] EncodeLegacyTableBlock(RecordedBet[] bets)
        {
            var bytes = new byte[0x284];
            foreach (RecordedBet bet in bets)
            {
                // That revision could only save IDs below 161.
                if (bet.id < 0 || bet.id >= 161) return new byte[0];
                bytes[bet.id * 2] = (byte)(bet.amount & 0xff);
            }
            return bytes;
        }

        private static bool BlocksEqual(byte[] left, byte[] right)
        {
            if (left.Length != right.Length) return false;
            for (int i = 0; i < left.Length; ++i)
                if (left[i] != right[i]) return false;
            return true;
        }

        private void RecordRoundDetails(RoundResult result, IRouletteSession source,
            bool trackLifecycle = false)
        {
            RecordRoundDetailsWithPayment(result, source, trackLifecycle, null);
        }

        private void RecordRoundDetailsWithPayment(RoundResult result, IRouletteSession source,
            bool trackLifecycle, RecordedHistoryOverflowV1 overflow)
        {
            var page = new RecordedRound();
            if (result != null && source != null)
            {
                page.present = true;
                page.pocket = result.Pocket;
                page.wager = result.Wager;
                page.returned = result.Returned;
                page.category = result.Category;
                page.denomination = source.Denomination;
                page.creditAfter = source.Credit;
                page.moneyRemainderCents =
                    (source as RouletteSession)?.MoneyRemainderCents ?? 0;
                page.totalsRecorded = true;
                page.totalWagered = source.Wagered;
                page.totalReturned = source.Returned;
                page.totalRounds = source.Rounds;
                page.totalWins = source.Wins;
                page.awaitingFinalPayout = source.DoubleAvailable && DoubleUpInputAllowed;
                page.wheelAngle = (float)wheelRenderedAngle;
                page.ballX = wheelMotionReady ? wheelBallPosition.x : 0;
                page.ballY = wheelMotionReady ? wheelBallPosition.y : 0;
                page.sourceWheelStopRecorded = source == session &&
                    wheelMotionReady && wheelStage == WheelStage.Done &&
                    wheelOutcome == result.Pocket;
                page.sourceWheelStopIndex = page.sourceWheelStopRecorded ?
                    wheelFinalStopIndex : 0;
                page.profile = source.Profile.Id;
                page.selectedChip = source.Chip;
                page.evaluationOrder = (source as RouletteSession)?.BetEvaluationOrder;
                page.bets = source.Bets.OrderBy(pair => pair.Key).Select(pair =>
                {
                    List<long> chips;
                    return new RecordedBet {
                        id = pair.Key,
                        amount = pair.Value,
                        chips = source.BetChipStacks.TryGetValue(pair.Key, out chips) ?
                            chips.ToArray() : new long[0]
                    };
                }).ToArray();
                page.winningPays = result.WinningPays == null ? null :
                    result.WinningPays.Select(pay => new RecordedPay {
                        id = pay.BetId, amount = pay.Amount
                    }).ToArray();
                page.historyPayOverrides = result.HistoryPayOverrides == null ?
                    null : result.HistoryPayOverrides.Select(entry =>
                        new RecordedHistoryPayOverride {
                            payIndex = entry.PayIndex, amount = entry.Amount,
                            progressiveLevel = entry.ProgressiveLevel,
                            kind = entry.Kind
                        }).ToArray();
                page.sourceTableBlock = Convert.ToBase64String(
                    EncodeSourceTableBlock(page.bets));
                page.spinJournalId = pendingSpin == null ? "" : pendingSpin.id;
                page.roundArchiveId = Guid.NewGuid().ToString("N");
            }
            PcRoundLifecycleRecord nextLifecycle = trackLifecycle ?
                CreateRoundOpenLifecycle(page, pendingSpin) : latestRoundLifecycle;
            recordedRoundPages.Insert(0, page);
            if (recordedRoundPages.Count > 50)
                recordedRoundPages.RemoveAt(50);
            latestRoundLifecycle = nextLifecycle;
            ResetPcNativeDoubleFeatureForSuccessor();
            if (trackLifecycle && source is RouletteSession && ((RouletteSession)source).RoundPayoutPending)
                CreatePcGamePayment(page, source, overflow);
            StageRecordedRoundArchive();
        }

        private IEnumerator RoundHistorySaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test", StringComparison.Ordinal))
                throw new InvalidOperationException("Round history smoke needs isolated test key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            var source = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            source.PlaceBet(100, 1);
            source.SetChip(5);
            source.PlaceBet(100, 5);
            source.PlaceBet(256, 1);
            RoundResult result = source.Spin();
            RecordPocket(result.Pocket, result, source);
            RecordedRound saved = recordedRoundPages[0];
            saved.sourceWheelStopRecorded = true;
            saved.sourceWheelStopIndex = 9;
            saved.wheelAngle = 45f;
            saved.ballX = 1;
            saved.ballY = 1;
            SaveRecordedRoundPages();
            if (!saved.present || saved.pocket != result.Pocket ||
                saved.wager != 7 || saved.returned != result.Returned ||
                saved.denomination != 1 || saved.bets.Length != 2 ||
                saved.bets[0].id != 100 || saved.bets[0].amount != 6 ||
                saved.bets[0].chips.Length != 2 ||
                saved.bets[0].chips[0] != 1 || saved.bets[0].chips[1] != 5 ||
                saved.bets[1].id != 256 || saved.bets[1].amount != 1 ||
                saved.winningPays == null || saved.winningPays.Length != 2 ||
                saved.winningPays[0].id != result.WinningPays[0].BetId ||
                saved.winningPays[0].amount != result.WinningPays[0].Amount ||
                saved.winningPays[1].id != result.WinningPays[1].BetId ||
                saved.winningPays[1].amount != result.WinningPays[1].Amount ||
                Convert.FromBase64String(saved.sourceTableBlock).Length != 0x284 ||
                Convert.FromBase64String(saved.sourceTableBlock)[0] != 6 ||
                Convert.FromBase64String(saved.sourceTableBlock)[312] != 1 ||
                string.IsNullOrEmpty(PlayerPrefs.GetString(
                    recordedHistoryKey + ".RoundDetails.v1", "")))
                throw new InvalidOperationException("Round details were not captured");
            if (!saved.awaitingFinalPayout || CommittedHistoryCount() != 0)
                throw new InvalidOperationException("Double Up result entered history before feature completion");
            RecordedRound invalidSpot = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            invalidSpot.bets[1].id = 257;
            invalidSpot.sourceTableBlock = "";
            if (IsValidRecordedRound(invalidSpot, recordedSnapshots[0]))
                throw new InvalidOperationException("Unconfigured bet spot entered saved history");
            RecordedRound invalidWheel = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            invalidWheel.sourceWheelStopIndex = 37;
            if (IsValidRecordedRound(invalidWheel, recordedSnapshots[0]))
                throw new InvalidOperationException("Invalid saved wheel stop index was accepted");
            RecordedRound overridden = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            overridden.winningPays[0].amount++;
            overridden.winningPays[1].amount--;
            if (!IsValidRecordedRound(overridden, recordedSnapshots[0]))
                throw new InvalidOperationException("Saved per-bet payout override was rejected");
            overridden.winningPays[1].id = 252;
            if (IsValidRecordedRound(overridden, recordedSnapshots[0]))
                throw new InvalidOperationException("Unplaced payout entered saved history");
            RecordedRound slotOverride = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            slotOverride.historyPayOverrides =
                new[] { new RecordedHistoryPayOverride {
                    payIndex = 1, amount = result.WinningPays[1].Amount + 1,
                    kind = 2, progressiveLevel = 0
                } };
            if (!IsValidRecordedRound(slotOverride, recordedSnapshots[0]))
                throw new InvalidOperationException("Original pay-slot override was rejected");
            slotOverride.historyPayOverrides[0].payIndex = 2;
            if (IsValidRecordedRound(slotOverride, recordedSnapshots[0]))
                throw new InvalidOperationException("Out-of-range pay slot entered history");
            RecordedRound lowByte = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            lowByte.bets[0].amount = 256;
            lowByte.bets[0].chips = new long[] { 256 };
            lowByte.wager = 257;
            lowByte.totalWagered = 257;
            lowByte.sourceTableBlock = Convert.ToBase64String(
                EncodeSourceTableBlock(lowByte.bets));
            lowByte.historyPayOverrides =
                new[] { new RecordedHistoryPayOverride {
                    payIndex = 1, amount = 19, kind = 2
                } };
            if (IsValidRecordedRound(lowByte, recordedSnapshots[0]))
                throw new InvalidOperationException(
                    "Low-byte-zero history bet retained a nonexistent pay slot");
            lowByte.historyPayOverrides[0].payIndex = 0;
            if (!IsValidRecordedRound(lowByte, recordedSnapshots[0]))
                throw new InvalidOperationException(
                    "Restored low-byte history pay slot was rejected");
            bool invalidSourceSpotRejected = false;
            try { EncodeSourceTableBlock(invalidSpot.bets); }
            catch (InvalidOperationException) { invalidSourceSpotRejected = true; }
            if (!invalidSourceSpotRejected)
                throw new InvalidOperationException("Unconfigured bet spot entered source table block");
            var old = new RecordedRound {
                present = true, pocket = result.Pocket, wager = result.Wager,
                returned = result.Returned, denomination = 1,
                creditAfter = source.Credit, profile = source.Profile.Id,
                bets = new[] { saved.bets[0] },
                sourceTableBlock = Convert.ToBase64String(
                    EncodeLegacyTableBlock(new[] { saved.bets[0] }))
            };
            old.wager = 6;
            if (!IsValidRecordedRound(old, recordedSnapshots[0]) ||
                Convert.FromBase64String(old.sourceTableBlock)[0] != 6)
                throw new InvalidOperationException("Old bet ID encoding did not migrate");
            session = source;
            BeginDouble();
            RecordedRound mismatchedStake = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            mismatchedStake.doubleUp.startingStake++;
            if (IsValidRecordedRound(mismatchedStake, recordedSnapshots[0]))
                throw new InvalidOperationException("Mismatched feature stake was accepted");
            long stake = session.DoubleStake;
            bool firstWon = session.GuessDouble(-1);
            RecordDoubleChoice(-1, stake, firstWon);
            stake = session.DoubleStake;
            bool secondWon = session.GuessDouble(-2);
            RecordDoubleChoice(-2, stake, secondWon);
            RecordedDouble feature = recordedRoundPages[0].doubleUp;
            if (!firstWon || secondWon || !IsValidRecordedRound(saved, recordedSnapshots[0]) ||
                feature.steps.Length != 2 || feature.steps[0].choice != 5 ||
                feature.steps[0].dealer != 3 || feature.steps[0].won != stake ||
                feature.steps[1].choice != 4 || feature.steps[1].won != 0 ||
                feature.finalWin != 0 || !feature.complete ||
                saved.returned != 0 || saved.creditAfter != session.Credit)
                throw new InvalidOperationException("Double Up source history was not captured");
            FinalizePendingHistoryRound();
            if (saved.awaitingFinalPayout || CommittedHistoryCount() != 1)
                throw new InvalidOperationException("Completed Double Up result was not committed");
            RecordedRound olderFeature = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            olderFeature.returned = feature.startingStake;
            olderFeature.creditAfter = saved.creditAfter + feature.startingStake;
            olderFeature.totalReturned = saved.totalReturned + feature.startingStake;
            if (!IsValidRecordedRound(olderFeature, recordedSnapshots[0]) ||
                olderFeature.returned != 0 || olderFeature.creditAfter != session.Credit ||
                olderFeature.totalReturned != saved.totalReturned)
                throw new InvalidOperationException("Earlier Double Up archive did not migrate");
            RecordedRound olderWinningFeature = JsonUtility.FromJson<RecordedRound>(
                JsonUtility.ToJson(saved));
            olderWinningFeature.doubleUp.steps = new[] { feature.steps[0] };
            olderWinningFeature.doubleUp.tookWin = true;
            olderWinningFeature.doubleUp.finalWin = feature.steps[0].won;
            olderWinningFeature.returned = feature.startingStake;
            olderWinningFeature.creditAfter = saved.creditAfter + feature.startingStake;
            olderWinningFeature.totalReturned = saved.totalReturned +
                feature.startingStake;
            if (!IsValidRecordedRound(olderWinningFeature, recordedSnapshots[0]) ||
                olderWinningFeature.returned != feature.steps[0].won ||
                olderWinningFeature.creditAfter !=
                    saved.creditAfter + feature.steps[0].won ||
                olderWinningFeature.totalReturned !=
                    saved.totalReturned + feature.steps[0].won)
                throw new InvalidOperationException(
                    "Earlier winning Double Up archive lost its final return");
            Debug.Log("ROULETTE_ROUND_HISTORY_SAVE_PASSED: wager=7 spots=0,156 double=2");
        }

        private IEnumerator RoundHistoryLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test", StringComparison.Ordinal) ||
                recordedSnapshots.Count != 1 || recordedRoundPages.Count != 1 ||
                !recordedRoundPages[0].present ||
                recordedRoundPages[0].wager != 7 ||
                recordedRoundPages[0].bets == null ||
                recordedRoundPages[0].bets.Length != 2 ||
                recordedRoundPages[0].bets[0].chips == null ||
                recordedRoundPages[0].bets[0].chips.Length != 2 ||
                recordedRoundPages[0].bets[0].chips[1] != 5 ||
                recordedRoundPages[0].bets[1].id != 256 ||
                recordedRoundPages[0].winningPays == null ||
                recordedRoundPages[0].winningPays.Length != 2 ||
                recordedRoundPages[0].winningPays[0].amount <= 0 ||
                recordedRoundPages[0].winningPays[1].amount <= 0 ||
                recordedRoundPages[0].doubleUp == null ||
                recordedRoundPages[0].doubleUp.steps == null ||
                recordedRoundPages[0].doubleUp.steps.Length != 2 ||
                recordedRoundPages[0].doubleUp.steps[0].choice != 5 ||
                recordedRoundPages[0].doubleUp.steps[1].choice != 4 ||
                !recordedRoundPages[0].doubleUp.complete ||
                recordedRoundPages[0].returned != 0 ||
                string.IsNullOrEmpty(recordedRoundPages[0].sourceTableBlock) ||
                Convert.FromBase64String(recordedRoundPages[0].sourceTableBlock)[0] != 6 ||
                Convert.FromBase64String(recordedRoundPages[0].sourceTableBlock)[312] != 1 ||
                !recordedRoundPages[0].sourceWheelStopRecorded ||
                recordedRoundPages[0].sourceWheelStopIndex != 9 ||
                Math.Abs(wheelRenderedAngle - 9d * WheelStepDegrees * 2d) > .01d ||
                Vector2.Distance(wheelBallPosition,
                    BallCenterForPocket(recordedRoundPages[0].pocket,
                        9d * WheelStepDegrees * 2d)) > .01f ||
                session == null ||
                session.Credit != recordedRoundPages[0].creditAfter ||
                session.LastResult == null || session.LastResult.Returned != 0 ||
                session.LastResult.WinningPays.Count != 2 ||
                session.LastResult.WinningPays[0].BetId != 100 ||
                session.LastResult.WinningPays[0].WinLevel !=
                    session.Profile.Records[100].WinLevel ||
                session.LastResult.WinningPays[1].BetId != 256 ||
                session.LastResult.WinningPays[1].WinLevel !=
                    session.Profile.Records[256].WinLevel ||
                session.Rounds != 1 || session.DoubleAvailable || session.InDoubleUp)
                throw new InvalidOperationException("Round details did not reload");
            Debug.Log("ROULETTE_ROUND_HISTORY_LOAD_PASSED: wager=7 spots=0,156 double=2");
        }

        private IEnumerator TotalsSaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.TotalsTest", StringComparison.Ordinal))
                throw new InvalidOperationException("Totals smoke needs isolated test key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = false };
            session.PlaceBet(100, 1);
            RoundResult first = session.Spin();
            RecordPocket(first.Pocket, first, session);
            session.NextRound();
            session.Clear();
            session.PlaceBet(101, 1);
            RoundResult second = session.Spin();
            RecordPocket(second.Pocket, second, session);
            if (first.Returned != 36 || second.Returned != 0 ||
                session.Credit != 1034 || session.Rounds != 2 ||
                session.Wagered != 2 || session.Returned != 36 || session.Wins != 1 ||
                !recordedRoundPages[0].totalsRecorded ||
                recordedRoundPages[0].totalRounds != 2 ||
                recordedRoundPages[0].totalWagered != 2 ||
                recordedRoundPages[0].totalReturned != 36 ||
                recordedRoundPages[0].totalWins != 1)
                throw new InvalidOperationException("Cumulative totals were not archived");
            Debug.Log("ROULETTE_TOTALS_SAVE_PASSED: rounds=2 wagered=2 returned=36 wins=1");
        }

        private IEnumerator HistoryNextSpinSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.HistoryNextSpinTest",
                StringComparison.Ordinal))
                throw new InvalidOperationException(
                    "History next-spin smoke needs an isolated test key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome(0)) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            RoundResult first = session.Spin();
            RecordPocket(first.Pocket, first, session);
            if (!recordedRoundPages[0].awaitingFinalPayout ||
                !session.DoubleAvailable)
                throw new InvalidOperationException(
                    "Winning history fixture was already committed");
            long firstCredit = session.Credit;
            session.PlaceBet(101, 1);
            if (session.ResultPhase || session.DoubleAvailable)
                throw new InvalidOperationException(
                    "Accepted next bet did not end the previous result phase");
            // The bet and spin intentionally run without an intervening Update.
            BeginSpin();
            if (!spinning || !session.SpinPending ||
                recordedRoundPages[0].awaitingFinalPayout ||
                recordedRoundPages[0].creditAfter != firstCredit ||
                recordedRoundPages[0].returned != first.Returned)
                throw new InvalidOperationException(
                    "Same-frame next spin skipped prior history commit");
            yield return new WaitUntil(() => !spinning);
            if (recordedRoundPages.Count != 2 ||
                recordedRoundPages[1].awaitingFinalPayout ||
                recordedRoundPages[1].creditAfter != firstCredit ||
                recordedPockets.Count != 2)
                throw new InvalidOperationException(
                    "Next result displaced an unfinished prior history page");
            Debug.Log("ROULETTE_HISTORY_NEXT_SPIN_PASSED: priorCredit=" +
                      firstCredit + " pages=" + recordedRoundPages.Count);
            Application.Quit();
        }

        private IEnumerator HistoryNextSpinLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.HistoryNextSpinTest",
                StringComparison.Ordinal) || recordedRoundPages.Count != 2 ||
                recordedPockets.Count != 2 ||
                recordedRoundPages[1].awaitingFinalPayout ||
                recordedRoundPages[1].creditAfter != 1035 ||
                recordedRoundPages[1].returned != 36 ||
                CommittedHistoryCount() != 1)
                throw new InvalidOperationException(
                    "Prior round history commit did not survive Player restart");
            Debug.Log("ROULETTE_HISTORY_NEXT_SPIN_LOAD_PASSED: " +
                      "priorCredit=1035 committed=1 pages=2");
            Application.Quit();
        }

        private IEnumerator TotalsLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.TotalsTest", StringComparison.Ordinal) ||
                recordedRoundPages.Count != 2 || session == null ||
                session.LastResult == null || session.LastResult.Pocket != 0 ||
                session.LastResult.Returned != 0 || session.Credit != 1034 ||
                session.Rounds != 2 || session.Wagered != 2 ||
                session.Returned != 36 || session.Wins != 1)
                throw new InvalidOperationException("Cumulative totals did not survive restart");
            Debug.Log("ROULETTE_TOTALS_LOAD_PASSED: rounds=2 wagered=2 returned=36 wins=1");
        }

        private IEnumerator RecoverySaveSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".RecoveryTest", StringComparison.Ordinal))
                throw new InvalidOperationException("Recovery smoke needs isolated test key");
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            RoundResult result = session.Spin();
            RecordPocket(result.Pocket, result, session);
            BeginDouble();
            long risked = session.DoubleStake;
            if (!session.GuessDouble(-1))
                throw new InvalidOperationException("Recovery setup did not win");
            RecordDoubleChoice(-1, risked, true);
            if (session.Credit != 999 || session.DoubleStake != 72 ||
                !session.InDoubleUp || CommittedHistoryCount() != 0 ||
                recordedRoundPages[0].doubleUp.steps.Length != 1)
                throw new InvalidOperationException("Pending feature was not archived");
            Debug.Log("ROULETTE_RECOVERY_SAVE_PASSED: credit=999 stake=72");
        }

        private IEnumerator RecoveryLoadSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".RecoveryTest", StringComparison.Ordinal) ||
                session == null || session.Credit != 999 || !session.InDoubleUp ||
                session.DoubleStake != 72 || session.DoubleRounds != 1 ||
                session.LastDoubleCard != 3 || session.LastResult == null ||
                session.LastResult.Pocket != 0 || session.TotalBet != 1 ||
                !wheelMotionReady || CommittedHistoryCount() != 0)
                throw new InvalidOperationException("Pending feature did not restore after restart");
            long win = session.DoubleStake;
            session.CollectDouble();
            RecordDoubleTakeWin(win);
            FinalizePendingHistoryRound();
            if (session.Credit != 1071 || session.LastResult.Returned != 72 ||
                session.Returned != 72 || recordedRoundPages[0].awaitingFinalPayout ||
                recordedRoundPages[0].returned != 72 ||
                recordedRoundPages[0].creditAfter != 1071 ||
                CommittedHistoryCount() != 1)
                throw new InvalidOperationException("Restored feature paid more than once");
            Debug.Log("ROULETTE_RECOVERY_LOAD_PASSED: credit=1071 paid=72");
        }

        private IEnumerator RecoveryCompletedSmoke()
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".RecoveryTest", StringComparison.Ordinal) ||
                session == null || session.Credit != 1071 || session.InDoubleUp ||
                session.DoubleAvailable || session.LastResult == null ||
                session.LastResult.Returned != 72 || session.Returned != 72 ||
                recordedRoundPages.Count != 1 ||
                recordedRoundPages[0].awaitingFinalPayout ||
                recordedRoundPages[0].creditAfter != 1071 ||
                CommittedHistoryCount() != 1)
                throw new InvalidOperationException("Completed feature credit did not persist");
            // Simulate a power loss after the completed feature was archived
            // but before StatePostHistory made the result visible.
            recordedRoundPages[0].awaitingFinalPayout = true;
            SaveRecordedRoundPages();
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            if (!TryRestoreArchivedRound() || session.Credit != 1071 ||
                session.LastResult.Returned != 72 || session.InDoubleUp ||
                recordedRoundPages[0].awaitingFinalPayout || CommittedHistoryCount() != 1)
                throw new InvalidOperationException("Completed feature replayed its payout");
            Debug.Log("ROULETTE_RECOVERY_COMPLETED_PASSED: credit=1071 paid=72");
        }
    }
}
