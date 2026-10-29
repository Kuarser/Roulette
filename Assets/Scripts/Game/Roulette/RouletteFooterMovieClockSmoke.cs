using System;
using System.Collections;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private int footerClockUiChecks;

        private void AssertFooterClockAt(long now, long frequency,
            FooterMoviePhase expectedPhase, int frame, string unchanged, string description)
        {
            RefreshFooterMovieUiAt(now, frequency);
            Texture2D[] frames = expectedPhase == FooterMoviePhase.Intro ?
                footerGamePlayIntroFrames : expectedPhase == FooterMoviePhase.Loop ?
                footerGamePlayFrames : footerGamePlayOutroFrames;
            bool hidden = expectedPhase == FooterMoviePhase.Hidden;
            if (footerMoviePhase != expectedPhase ||
                uiFooterMovie.gameObject.activeSelf == hidden ||
                uiFooterMovie.texture != (hidden ? null : frames[frame]) ||
                !hidden && (frames[frame].width != 258 || frames[frame].height != 59))
                throw new InvalidOperationException("Footer UI clock differs: " + description);
            if (PaytableTestLiveFingerprint(session) != unchanged)
                throw new InvalidOperationException("Footer clock changed funds/history/RNG: " + description);
            ++footerClockUiChecks;
        }

        private IEnumerator FooterMovieClockSmoke()
        {
            yield return null;
            OriginalFooterMovieClockReference.Validate();
            if (footerGamePlayIntroFrames.Length != 36 || footerGamePlayFrames.Length != 36 ||
                footerGamePlayOutroFrames.Length != 12)
                throw new InvalidOperationException("Footer original Bink frame sets differ");
            OriginalFooterMovieClock savedClock = footerMovieClock;
            FooterMoviePhase savedPhase = footerMoviePhase;
            OriginalPaidFadeClock savedPaidFade = paidFadeClock;
            paidFadeClock = new OriginalPaidFadeClock();
            string before = PaytableTestLiveFingerprint(session);
            const long period = 41999L;
            const long introDone = 1511964L;
            const long outroDone = 503988L;
            try
            {
                foreach (int days in new[] {0,180,730})
                    foreach (long frequency in new[] {1000000L,10000000L})
                    {
                        long epoch = days * 86400L * frequency;
                        long ticksPerMicrosecond = frequency / 1000000L;
                        string label = days + " days " + frequency + " Hz";
                        footerMoviePhase = FooterMoviePhase.Hidden; footerMovieClock = null;
                        paidFadeClock = new OriginalPaidFadeClock();
                        StartFooterMovieAt(epoch,frequency);
                        for (int frame=0; frame<36; ++frame)
                        {
                            long first = epoch + frame*period*ticksPerMicrosecond;
                            long last = epoch + (frame+1)*period*ticksPerMicrosecond - 1;
                            AssertFooterClockAt(first,frequency,FooterMoviePhase.Intro,
                                frame,before,"Intro first tick " + frame + " " + label);
                            if (frame == 10)
                            {
                                OriginalFooterMovieClock runningIntro = footerMovieClock;
                                StartFooterMovieAt(first,frequency);
                                if (footerMovieClock != runningIntro)
                                    throw new InvalidOperationException("Already visible Intro replayed");
                            }
                            AssertFooterClockAt(last,frequency,FooterMoviePhase.Intro,
                                frame,before,"Intro last tick " + frame + " " + label);
                        }
                        long loopStart = epoch + introDone*ticksPerMicrosecond;
                        AssertFooterClockAt(loopStart,frequency,FooterMoviePhase.Loop,0,
                            before,"Intro done starts separate Loop " + label);
                        AssertFooterClockAt(loopStart + period*ticksPerMicrosecond-1,
                            frequency,FooterMoviePhase.Loop,0,before,"Loop frame zero last tick " + label);
                        AssertFooterClockAt(loopStart + period*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Loop,1,before,"Loop frame one first tick " + label);
                        AssertFooterClockAt(loopStart + (36*period-1)*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Loop,35,before,"Loop last frame " + label);
                        AssertFooterClockAt(loopStart + 36*period*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Loop,0,before,"Loop wrap " + label);
                        AssertFooterClockAt(loopStart + 1553998L*ticksPerMicrosecond-1,
                            frequency,FooterMoviePhase.Loop,0,before,"Separate loop span last tick " + label);
                        AssertFooterClockAt(loopStart + 1553998L*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Loop,1,before,"Separate loop span first tick " + label);

                        long outroStart = loopStart + 2000000L*ticksPerMicrosecond;
                        StopFooterMovieAt(outroStart,frequency);
                        OriginalFooterMovieClock runningOutro = footerMovieClock;
                        StopFooterMovieAt(outroStart + 1,frequency);
                        if (footerMovieClock != runningOutro)
                            throw new InvalidOperationException("Stop replayed an active Outro");
                        for (int frame=0; frame<12; ++frame)
                        {
                            AssertFooterClockAt(outroStart + frame*period*ticksPerMicrosecond,
                                frequency,FooterMoviePhase.Outro,frame,before,"Outro first tick " + frame + " " + label);
                            AssertFooterClockAt(outroStart + (frame+1)*period*ticksPerMicrosecond-1,
                                frequency,FooterMoviePhase.Outro,frame,before,"Outro last tick " + frame + " " + label);
                        }
                        long outroEnd = outroStart + outroDone*ticksPerMicrosecond;
                        AssertFooterClockAt(outroEnd,frequency,FooterMoviePhase.Hidden,0,
                            before,"Outro done hides " + label);
                        StopFooterMovieAt(outroEnd,frequency);
                        AssertFooterClockAt(outroEnd+1,frequency,FooterMoviePhase.Hidden,0,
                            before,"Hidden stop stays hidden " + label);
                        StartFooterMovieAt(outroEnd+2,frequency);
                        AssertFooterClockAt(outroEnd+2,frequency,FooterMoviePhase.Intro,0,
                            before,"Next play starts Intro " + label);

                        // A delayed Intro completion starts the new movie
                        // at the serviced callback clock, without old lateness.
                        footerMoviePhase = FooterMoviePhase.Hidden; footerMovieClock = null;
                        paidFadeClock = new OriginalPaidFadeClock();
                        StartFooterMovieAt(epoch,frequency);
                        long delayedService = epoch + 3000000L*ticksPerMicrosecond;
                        AssertFooterClockAt(delayedService,frequency,FooterMoviePhase.Loop,0,
                            before,"Late Intro callback starts Loop now " + label);
                        AssertFooterClockAt(delayedService + period*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Loop,1,before,"Fresh Loop after late callback " + label);
                        StartFooterMovieAt(delayedService + 2*period*ticksPerMicrosecond,frequency);
                        AssertFooterClockAt(delayedService + 2*period*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Intro,0,before,"Play stops Loop and starts Intro " + label);
                        StopFooterMovieAt(delayedService + 3*period*ticksPerMicrosecond,frequency);
                        AssertFooterClockAt(delayedService + 3*period*ticksPerMicrosecond,
                            frequency,FooterMoviePhase.Outro,0,before,"Stop interrupts Intro with Outro " + label);
                    }

                // 1 kHz sampling observes the same native microsecond rule
                // at its available whole-millisecond ticks.
                const long coarseFrequency = 1000L;
                long coarseEpoch = 730L * 86400L * coarseFrequency;
                footerMoviePhase = FooterMoviePhase.Hidden; footerMovieClock = null;
                paidFadeClock = new OriginalPaidFadeClock();
                StartFooterMovieAt(coarseEpoch,coarseFrequency);
                AssertFooterClockAt(coarseEpoch+41,coarseFrequency,FooterMoviePhase.Intro,0,before,"1 kHz at 41 ms");
                AssertFooterClockAt(coarseEpoch+42,coarseFrequency,FooterMoviePhase.Intro,1,before,"1 kHz at 42 ms");
                AssertFooterClockAt(coarseEpoch+1511,coarseFrequency,FooterMoviePhase.Intro,35,before,"1 kHz at 1511 ms");
                AssertFooterClockAt(coarseEpoch+1512,coarseFrequency,FooterMoviePhase.Loop,0,before,"1 kHz at 1512 ms");

                uint floatBefore = BitConverter.ToUInt32(BitConverter.GetBytes((float)(180d*86400)),0);
                uint floatFrame1 = BitConverter.ToUInt32(BitConverter.GetBytes((float)(180d*86400+.042d)),0);
                if (floatBefore != floatFrame1)
                    throw new InvalidOperationException("Old long-running float frame fixture differs");
                Debug.Log("ROULETTE_FOOTER_OLD_FLOAT_STORES: 180-day start/42ms=" + floatBefore.ToString("X8"));
            }
            finally
            {
                footerMovieClock = savedClock; footerMoviePhase = savedPhase;
                paidFadeClock = savedPaidFade;
                ApplyPaidOpacity(PaidOpacity);
                RefreshFooterMovieUiAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency);
            }
            if (PaytableTestLiveFingerprint(session) != before)
                throw new InvalidOperationException("Footer clock diagnostic altered formal session");
            Debug.Log("ROULETTE_FOOTER_CLOCK_UI_PASSED: " + footerClockUiChecks +
                " actual texture/activity boundaries; Intro/Loop/Outro; duplicate Play/Stop; late callback; 0/180/730 days; 1kHz/1MHz/10MHz; funds/history/RNG isolation");
            Application.Quit();
        }
    }
}
