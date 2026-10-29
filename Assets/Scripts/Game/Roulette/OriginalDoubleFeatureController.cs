using System;

namespace RouletteDeluxe
{
    public enum OriginalDoubleFeatureView
    {
        Entrance, Restore, Ready, RevealWin, RevealLoss, RevealMaximum,
        Take, AutoTake, Exit, Inactive
    }

    public interface IOriginalDoubleFeatureHost
    {
        long CurrentAward { get; }
        bool MaximumReached { get; }
        bool AuNationalStandards { get; }
        int ExitDelayMilliseconds { get; }
        void SaveNativeState(PcDoubleFeatureNvV1 state, string boundary);
        void SetTimer(int delayMilliseconds, int opcode, long generation);
        void Present(OriginalDoubleFeatureView view, int durationMilliseconds);
        // Called only by the ordinary Game payout owner, after native Feature
        // completion. Applies money and completionStage=2 in one archive write.
        // A replay uses the stable featureId and returns the committed record.
        PcDoubleFeatureNvV1 CommitFeatureCompletion(PcDoubleFeatureNvV1 prepared);
    }

    // Source: SuitGamble, RedBlackDoubleUp ELF
    // EEC5482C0FE9B4C7235A4FAC6A51A459924A1D141F6E13496B395207DC3B4670.
    // No clock, random draw, Session mutation or Unity callback is hidden here.
    public sealed class OriginalDoubleFeatureController
    {
        private readonly IOriginalDoubleFeatureHost host;
        private PcDoubleFeatureNvV1 nv;
        private int liveState;
        private bool localAuto;
        private bool active;
        private bool suspended;
        private bool faulted;
        private bool timerRunning;
        private int timerOpcode;
        private int timerDelay;
        private long timerGeneration;

        public OriginalDoubleFeatureController(PcDoubleFeatureNvV1 saved,
            IOriginalDoubleFeatureHost host)
        {
            if (saved == null) throw new ArgumentNullException("saved");
            if (host == null) throw new ArgumentNullException("host");
            saved.Validate();
            nv = saved.Clone();
            this.host = host;
            liveState = saved.savedState;
            active = saved.completionStage != 2 && saved.savedState != 0;
        }

        public PcDoubleFeatureNvV1 Saved { get { return nv.Clone(); } }
        public int LiveState { get { return liveState; } }
        public bool LocalAuto { get { return localAuto; } }
        public bool Active { get { return active; } }
        public bool GamePayoutPending { get { return nv.completionStage == 1; } }
        public bool Suspended { get { return suspended; } }
        public bool Faulted { get { return faulted; } }
        public bool TimerRunning { get { return timerRunning; } }
        public int TimerOpcode { get { return timerOpcode; } }
        public int TimerDelayMilliseconds { get { return timerDelay; } }
        public long TimerGeneration { get { return timerGeneration; } }
        public bool DecisionReady { get { return active && !suspended && !faulted &&
            liveState == 2 && !timerRunning && nv.completionStage == 0; } }

        private void Execute(Action action)
        {
            if (faulted) throw new InvalidOperationException("Double Feature is faulted");
            try { action(); }
            catch
            {
                faulted = true;
                timerRunning = false;
                // The caller also latches its storage fault. Invalidate the
                // old dispatch even if a host timer callback itself failed.
                unchecked { ++timerGeneration; }
                throw;
            }
        }

        private void Save(string boundary)
        {
            nv.Validate();
            host.SaveNativeState(nv.Clone(), boundary);
        }

        private void Schedule(int milliseconds, int opcode)
        {
            if (milliseconds <= 0) throw new ArgumentOutOfRangeException("milliseconds");
            timerDelay = milliseconds;
            timerOpcode = opcode;
            timerRunning = true;
            timerGeneration = checked(timerGeneration + 1);
            host.SetTimer(milliseconds, opcode, timerGeneration);
        }

        private void CancelTimer()
        {
            timerRunning = false;
            timerGeneration = checked(timerGeneration + 1);
            host.SetTimer(0, timerOpcode, timerGeneration);
        }

        public void Begin(bool auto)
        {
            Execute(() => {
                if (nv.stepCount != 0 || nv.completionStage != 0 || nv.savedState != 2)
                    throw new InvalidOperationException("Double Feature Begin requires a new saved state2");
                localAuto = auto;
                active = true;
                suspended = false;
                liveState = 1;
                Schedule(1000, 0x0a);
                host.Present(OriginalDoubleFeatureView.Entrance, 1000);
                nv.autoRestoreMarker = false;
                nv.savedState = 2;
                Save("BeginSaved2");
            });
        }

        public void Suspend()
        {
            Execute(() => {
                if (!active) return;
                suspended = true;
                CancelTimer();
            });
        }

        public void Resume(bool auto)
        {
            Execute(() => {
                CancelTimer();
                suspended = false;
                localAuto = auto;
                liveState = nv.savedState;
                if (nv.completionStage == 2)
                {
                    FinishNativeCompletion();
                    return;
                }
                active = true;
                if (nv.completionStage == 1)
                {
                    // Old Prepared receipts can retain saved2/3/5/7. Native
                    // completion has no new draw or timer, and Game payout
                    // belongs to a later ordinary Game dispatch.
                    FinishNativeCompletion();
                    return;
                }
                if (liveState == 7)
                {
                    Complete();
                    return;
                }
                host.Present(OriginalDoubleFeatureView.Restore, 0);
                if (host.CurrentAward <= 0)
                {
                    if (nv.stepCount == 0)
                        throw new PcRoundLifecycleDataException("Zero award has no saved Double result");
                    ReplayResult(false, true);
                }
                else if (localAuto)
                {
                    // 080a1489 precedes native state3/state5 special cases.
                    // Deliberately do not change liveState to 2 here.
                    nv.autoRestoreMarker = true;
                    Save("AutoRestoreMarked");
                    TakeCore(false);
                }
                else if (liveState == 3)
                    ReplayResult(true, host.MaximumReached);
                else if (liveState == 5)
                {
                    liveState = 2; // 080a17e8, non-Auto branch only.
                    TakeCore(true);
                }
                else if (host.MaximumReached)
                    TakeCore(false);
                else
                    DeckFlash();
            });
        }

        // The host has already committed one result through the existing RNG
        // receipt and populated its candidate archive step. SaveNativeState
        // atomically writes that candidate together with this native phase.
        public void ChoiceCommitted(bool won, bool terminal, int stepCount, string resultDigest)
        {
            Execute(() => {
                if (!DecisionReady || stepCount != nv.stepCount + 1 || stepCount > 16 ||
                    !PcDoubleFeatureNvV1.IsDigest(resultDigest) || (!won && !terminal) ||
                    (won ? host.CurrentAward <= 0 : host.CurrentAward != 0))
                    throw new InvalidOperationException("Double Feature result does not follow its ready decision");
                nv.stepCount = stepCount;
                nv.resultDigest = resultDigest;
                // Source stores dealer/pick before writing live/saved state3.
                Save("ChoiceResultSaved");
                ReplayResult(won, terminal);
            });
        }

        private void ReplayResult(bool won, bool terminal)
        {
            liveState = 3;
            nv.savedState = 3;
            Save("RevealSaved3");
            int delay = won && terminal && host.AuNationalStandards ? 3000 : 1250;
            host.Present(!won ? OriginalDoubleFeatureView.RevealLoss : terminal ?
                OriginalDoubleFeatureView.RevealMaximum : OriginalDoubleFeatureView.RevealWin, delay);
            Schedule(delay, !won ? 0x17 : terminal ? 0x18 : 0x16);
        }

        public void Take(bool force3000)
        {
            Execute(() => {
                if (!DecisionReady)
                    throw new InvalidOperationException("Double Feature take is not ready");
                TakeCore(force3000);
            });
        }

        private void TakeCore(bool force3000)
        {
            bool auto = localAuto;
            int delay = auto || force3000 ? 3000 : 100;
            Schedule(delay, 0x1a);
            nv.savedState = 7;
            Save("TakeSaved7");
            if (auto) localAuto = false;
            host.Present(auto ? OriginalDoubleFeatureView.AutoTake : OriginalDoubleFeatureView.Take, delay);
            nv.savedState = 5;
            Save("TakeSaved5");
        }

        private void DeckFlash()
        {
            if (liveState == 2)
            {
                if (localAuto) TakeCore(false);
                else host.Present(OriginalDoubleFeatureView.Ready, 0);
                // The source writes liveState after Take returns. At entrance
                // or win->ready this overwrites Take's saved5 with saved2.
                nv.savedState = liveState;
                Save("DeckFlashSavedLive");
            }
        }

        public bool DeliverTimer(int opcode, long generation)
        {
            bool consumed = false;
            Execute(() => {
                if (!active || suspended || !timerRunning || generation != timerGeneration ||
                    opcode != timerOpcode) return;
                timerRunning = false;
                host.SetTimer(0, timerOpcode, timerGeneration);
                if (liveState == 1 && opcode == 0x0a || liveState == 3 && opcode == 0x16)
                {
                    liveState = 2;
                    DeckFlash();
                    consumed = true;
                }
                else if (liveState == 2 && opcode == 0x1a ||
                    liveState == 3 && (opcode == 0x17 || opcode == 0x18))
                {
                    liveState = 6;
                    int delay = host.ExitDelayMilliseconds;
                    if (delay < 10) throw new InvalidOperationException("Double exit duration is invalid");
                    host.Present(OriginalDoubleFeatureView.Exit, delay);
                    Schedule(delay, 0x19);
                    consumed = true;
                }
                else if (liveState == 6 && opcode == 0x19)
                {
                    liveState = 7;
                    Complete();
                    consumed = true;
                }
                // In particular (live3,1a), (live5,1a), (live6,1a) stop
                // without consuming the event. This is the source table.
            });
            return consumed;
        }

        private void Complete()
        {
            Save("WrapUpSaved");
            nv.completionStage = 1;
            nv.completionAward = host.CurrentAward;
            Save("FeatureCompletionPrepared");
            FinishNativeCompletion();
        }

        public void CommitPendingGamePayout()
        {
            Execute(() => {
                if (nv.completionStage == 2) return;
                if (nv.completionStage != 1 || nv.savedState != 0 || liveState != 0 ||
                    active || timerRunning)
                    throw new InvalidOperationException("Game payout requires a completed native Feature");
                CommitPreparedCompletion();
            });
        }

        private void CommitPreparedCompletion()
        {
            PcDoubleFeatureNvV1 committed = host.CommitFeatureCompletion(nv.Clone());
            if (committed == null) throw new PcRoundLifecycleDataException("Double completion returned no record");
            committed.Validate();
            if (committed.completionStage != 2 || committed.featureId != nv.featureId ||
                committed.roundArchiveId != nv.roundArchiveId || committed.spinJournalId != nv.spinJournalId ||
                committed.profile != nv.profile || committed.stepCount != nv.stepCount ||
                committed.suitMode != nv.suitMode || committed.savedState != nv.savedState ||
                committed.autoRestoreMarker != nv.autoRestoreMarker ||
                committed.completionAward != nv.completionAward)
                throw new PcRoundLifecycleDataException("Double completion changed its prepared identity or amount");
            nv = committed.Clone();
        }

        private void FinishNativeCompletion()
        {
            // FtrComplete sends its completion and saves native0 before the
            // Game later reaches StatePayout. This is not an Applied receipt.
            active = false;
            timerRunning = false;
            liveState = 0;
            host.Present(OriginalDoubleFeatureView.Inactive, 0);
            if (nv.savedState != 0)
            {
                nv.savedState = 0;
                Save("FeatureCompletedSaved0");
            }
        }
    }
}
