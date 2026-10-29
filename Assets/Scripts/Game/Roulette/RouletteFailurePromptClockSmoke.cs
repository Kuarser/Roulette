using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private struct FailurePromptBoundary
        {
            public int Milliseconds, Frame;
            public bool Outro, Visible, Finished;
            public FailurePromptBoundary(int milliseconds, int frame,
                bool outro = false, bool visible = true, bool finished = false)
            {
                Milliseconds = milliseconds; Frame = frame; Outro = outro;
                Visible = visible; Finished = finished;
            }
        }

        private IEnumerator FailurePromptClockSmoke()
        {
            yield return null;
            IRouletteSession saved = session;
            long savedStartupReadyAt = promptReadyTimestamp;
            long savedDelayedPlay = promptDelayedPlayTimestamp;
            string originalFingerprint = PaytableTestLiveFingerprint(saved);
            // Literal native boundaries: 17 Begin frames at 42 ms, 3000 ms
            // hold, 15 End frames, 3000 ms blank, then PlayNext.
            var boundaries = new List<FailurePromptBoundary>();
            for (int frame = 0; frame < 17; ++frame)
            {
                boundaries.Add(new FailurePromptBoundary(frame*42,frame));
                boundaries.Add(new FailurePromptBoundary(frame*42+41,frame));
            }
            boundaries.Add(new FailurePromptBoundary(714,16));
            boundaries.Add(new FailurePromptBoundary(3713,16));
            for (int frame = 0; frame < 15; ++frame)
            {
                boundaries.Add(new FailurePromptBoundary(3714+frame*42,frame,true));
                boundaries.Add(new FailurePromptBoundary(3714+frame*42+41,frame,true));
            }
            boundaries.Add(new FailurePromptBoundary(4344,0,visible:false));
            boundaries.Add(new FailurePromptBoundary(7343,0,visible:false));
            boundaries.Add(new FailurePromptBoundary(7344,0,finished:true));
            int checkedBoundaries = 0;
            try
            {
                foreach (BetRejectionReason reason in new[] {
                    BetRejectionReason.TotalLimit, BetRejectionReason.SpotLimit })
                    foreach (long epoch in new[] {0L,180L*24*60*60*1000})
                    {
                        RouletteProfile profile = RouletteProfile.Load("97");
                        if (reason == BetRejectionReason.TotalLimit) profile.MaximumTotalBet = 1;
                        else profile.MaximumSpotBet = 1;
                        session = new RouletteSession(profile,10,new PaytableTestForbiddenOutcome());
                        session.PlaceBet(100,1);
                        ResetBetPromptAfterSuccess();
                        // This fixture enters an already ready betting screen;
                        // startup's separate one-second delay is not elapsed
                        // by advancing the controlled failure clock.
                        promptReadyTimestamp = 0;
                        promptDelayedPlayTimestamp = 0;
                        string before = PaytableTestLiveFingerprint(session);
                        Bet(reason == BetRejectionReason.SpotLimit ? 100 : 101);
                        PromptKind expected = reason == BetRejectionReason.TotalLimit ?
                            PromptKind.BetLimitReached : PromptKind.BetSpotLimitReached;
                        if (failurePrompt != expected || PaytableTestLiveFingerprint(session) != before)
                            throw new InvalidOperationException("Rejected bet changed wager/state or chose wrong prompt");
                        Texture2D[] intro = PromptFrames(expected,false), outro = PromptFrames(expected,true);
                        if (intro.Length != 17 || outro.Length != 15)
                            throw new InvalidOperationException("Failure prompt frame counts differ from source");
                        // A controlled 1000-Hz clock makes every source ms exact.
                        failurePromptStartedTimestamp = epoch;
                        foreach (FailurePromptBoundary boundary in boundaries)
                        {
                            RefreshPromptUiAt(epoch+boundary.Milliseconds,1000);
                            if (boundary.Finished)
                            {
                                if (failurePrompt != PromptKind.None || shownPrompt != PromptKind.PressSpin ||
                                    !promptRoot.gameObject.activeSelf || !uiPromptButton.interactable ||
                                    uiPrompt.texture != pressSpinFrames[0])
                                    throw new InvalidOperationException("Failure animation did not hand off to source next message at 7344 ms");
                            }
                            else if (promptRoot.gameObject.activeSelf != boundary.Visible || uiPromptButton.interactable ||
                                failurePrompt != expected || boundary.Visible && uiPrompt.texture !=
                                    (boundary.Outro ? outro : intro)[boundary.Frame])
                                throw new InvalidOperationException("Failure prompt boundary differs: "+reason+" epoch="+epoch+
                                    " ms="+boundary.Milliseconds);
                            if (PaytableTestLiveFingerprint(session) != before)
                                throw new InvalidOperationException("Failure playback changed funds/RNG/bets");
                            ++checkedBoundaries;
                        }
                        // Negative elapsed time clamps to source frame zero.
                        QueueBetFailurePrompt(reason); failurePromptStartedTimestamp = epoch+1;
                        RefreshPromptUiAt(epoch,1000);
                        if (uiPrompt.texture != intro[0] || !promptRoot.gameObject.activeSelf)
                            throw new InvalidOperationException("Pre-start failure prompt did not clamp to frame zero");
                        // A repeated rejection restarts at frame zero immediately.
                        QueueBetFailurePrompt(reason); RefreshPromptUi();
                        if (uiPrompt.texture != intro[0])
                            throw new InvalidOperationException("Repeated failure did not restart source animation");
                        ResetBetPromptAfterSuccess();
                        if (failurePrompt != PromptKind.None)
                            throw new InvalidOperationException("Successful-input reset retained rejection playback");
                    }
                // The withdrawn float path cannot represent the first 42 ms
                // at this uptime. Keep evidence that this fixture catches it.
                float oldEpoch = (float)(180d*24*60*60);
                float oldNow = (float)(180d*24*60*60+.042d);
                if (oldNow-oldEpoch != 0f)
                    throw new InvalidOperationException("Long-uptime float defect fixture no longer distinguishes the old code");
            }
            finally
            {
                session = saved; promptReadyTimestamp = savedStartupReadyAt;
                promptDelayedPlayTimestamp = savedDelayedPlay;
                ResetBetPromptAfterSuccess(); RefreshPortraitUi();
            }
            if (PaytableTestLiveFingerprint(saved) != originalFingerprint)
                throw new InvalidOperationException("Failure clock diagnostic altered formal session");
            Debug.Log("ROULETTE_FAILURE_PROMPT_CLOCK_PASSED: "+checkedBoundaries+
                " live UI boundaries; both limits; 180-day epoch; Begin/Hold/End/Blank/Next; restart/reset; funds/RNG isolation");
            Application.Quit();
        }
    }
}
