using System;
using UnityEngine;

namespace RouletteDeluxe
{
    // The source wheel advances on a 42 ms platform timer. Count elapsed
    // periods from a monotonic integer timestamp so long PC sessions do not
    // lose timer resolution through Unity's float uptime clock.
    public static class OriginalWheelClock
    {
        public const int TickMilliseconds = 42;

        public static int DueFrames(long startTimestamp, long nowTimestamp,
            long timestampFrequency)
        {
            if (timestampFrequency <= 0)
                throw new ArgumentOutOfRangeException("timestampFrequency");
            long elapsed = nowTimestamp - startTimestamp;
            if (elapsed <= 0) return 0;
            double due = Math.Floor((double)elapsed * 1000d /
                ((double)timestampFrequency * TickMilliseconds));
            return due >= int.MaxValue ? int.MaxValue : (int)due;
        }
    }

    // glibc's default TYPE_3 rand/srand stream. The cabinet's RouletteWheel
    // seeds the process C RNG with time(NULL) when it creates the wheel.
    public sealed class OriginalWheelPathRandom
    {
        private readonly uint[] state = new uint[31];
        private int front = 3, rear;

        public OriginalWheelPathRandom(uint seed)
        {
            if (seed == 0) seed = 1;
            state[0] = seed;
            long word = unchecked((int)seed);
            for (int i = 1; i < state.Length; ++i)
            {
                long hi = word / 127773;
                long lo = word % 127773;
                word = 16807 * lo - 2836 * hi;
                if (word < 0) word += 2147483647;
                state[i] = unchecked((uint)word);
            }
            for (int i = 0; i < 310; ++i) NextRaw();
        }

        public int NextRaw()
        {
            uint sum = unchecked(state[front] + state[rear]);
            state[front] = sum;
            if (++front == state.Length) front = 0;
            if (++rear == state.Length) rear = 0;
            return (int)(sum >> 1);
        }

        public int NextPath(int count)
        {
            if (count < 2) throw new ArgumentOutOfRangeException("count");
            // SetStop truncates (count-1) * rand() / 2^31. With 37 paths,
            // the normal branch therefore yields 0..35; 36 is gaff-only.
            return (int)((long)NextRaw() * (count - 1) / 2147483648L);
        }
    }

    // The timer and state transitions follow RouletteWheel::SetStop/Play/Update
    // in the original 32-bit game executable. Coordinates come from filedefs.xml.
    public sealed partial class RouletteGame
    {
        private const float WheelFrameSeconds = OriginalWheelClock.TickMilliseconds / 1000f;
        // StateDisplayResults calls VideoApp::WaitInput(0x23, true) only in AutoPlay.
        private const float ResultWaitSeconds = .035f;
        private const int WinningResultDisplayTicks = 30;
        private const int LosingResultDisplayTicks = 20;
        // Independently measured from newmv.mp4: TOTAL WIN is visible for 1.18–1.28 s.
        private const float WinningResultVisualSeconds = 1.23f;
        // StateDisplayResults calls App::Delay(500, true) for AutoPlay returns > 500.
        private const float HighWinAutoDelaySeconds = .5f;
        private static readonly double WheelStepDegrees = OriginalWheelArithmetic.Step;
        private static readonly int[] SourceWheelStops = {
            9,31,14,20,1,33,16,24,5,10,23,8,30,11,36,13,27,6,34,17,25,2,
            21,4,19,15,32,0,26,3,35,12,28,7,29,18,22
        };

        private enum WheelStage { Delay, Loop, Descent, Land, EaseOut, Done }
        private WheelStage wheelStage;
        private int wheelVariant, wheelOutcome, wheelDelayFrames, wheelPathFrame;
        private int wheelFinalStopIndex;
        private OriginalWheelPathRandom wheelVisualRandom;
        private float wheelEaseVelocity, wheelEaseRemaining;
        private double wheelRenderedAngle;
        private float wheelMainRenderAngle = 36000f, wheelSpindleRenderAngle = 36000f;
        private bool wheelMotionReady, wheelBallVisible;
        private bool wheelSkillStopRequested;
        private bool wheelTurboSnapRequested;
        private Vector2 wheelBallPosition;

        private static int SourceStopIndex(int pocket)
        {
            int index = Array.IndexOf(SourceWheelStops, pocket);
            if (index < 0) throw new InvalidOperationException("Invalid roulette pocket " + pocket);
            return index + 1; // The executable builds a one-based inverse table.
        }

        private Vector2 BallCenterForPocket(int pocket, double phase)
        {
            return OriginalWheelArithmetic.BallCenter(SourceStopIndex(pocket), phase, false);
        }

        private void InitializeWheelVisualRandom()
        {
            uint seed = (uint)(DateTime.UtcNow -
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            string[] launch = Environment.GetCommandLineArgs();
            int seedOption = Array.IndexOf(launch, "-rouletteVisualSeed");
            uint supplied;
            if (seedOption >= 0 && seedOption + 1 < launch.Length &&
                uint.TryParse(launch[seedOption + 1], out supplied)) seed = supplied;
            wheelVisualRandom = new OriginalWheelPathRandom(seed);
            Debug.Log("ROULETTE_WHEEL_VISUAL_SEED: " + seed);
        }

        private bool BeginWheelPlayback(int outcome)
        {
            if (originalBallLoop == null || originalBallLoop.points == null ||
                originalBallMotion == null || originalBallMotion.descent == null ||
                originalBallMotion.land == null ||
                originalBallMotion.descent.Length != 37 ||
                originalBallMotion.land.Length != 37)
                return false;

            wheelOutcome = outcome;
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-rouletteVisualVariant");
            int selected;
            int armedPath = -1;
            bool hasArmedPath = demoOutcome != null &&
                demoOutcome.TryConsumeVisualPath(37, out armedPath);
            // Explicit diagnostic override wins over the source arm, but both
            // paths skip libc rand. Ordinary gameplay consumes the arm once.
            if (option >= 0 && option + 1 < args.Length &&
                int.TryParse(args[option + 1], out selected) && selected >= 0 && selected < 37)
                wheelVariant = selected;
            else if (hasArmedPath)
                wheelVariant = armedPath;
            else
                wheelVariant = wheelVisualRandom.NextPath(37);

            int loopCount = originalBallLoop.points.Length;
            int descentCount = originalBallMotion.descent[wheelVariant].points.Length;
            int landCount = originalBallMotion.land[wheelVariant].points.Length;
            int totalPathFrames = loopCount + descentCount + landCount;
            int currentStop = OriginalWheelArithmetic.CurrentStop(wheelAngle);
            wheelAngle = OriginalWheelArithmetic.RestorePhase(currentStop);

            // Literal integer arithmetic from RouletteWheel::SetStop. The
            // initial delay lets the chosen visual landing meet the outcome.
            int delay = (((totalPathFrames / 74) * 37 - totalPathFrames / 2 + 37 -
                SourceStopIndex(outcome) + SourceStopIndex(wheelVariant) - currentStop) * 2) + 1;
            while (delay < 0) delay += 74;
            wheelDelayFrames = delay;
            wheelFinalStopIndex = (currentStop + 3 + (totalPathFrames + delay) / 2) % 37;
            wheelPathFrame = 0;
            wheelStage = WheelStage.Delay;
            wheelEaseVelocity = (float)WheelStepDegrees;
            wheelEaseRemaining = 2f;
            wheelBallVisible = false;
            wheelMotionReady = true;
            wheelSkillStopRequested = false;
            wheelTurboSnapRequested = false;
            // The final Delay tick also consumes Loop point 0. The last
            // Land tick skips the fixed-speed draw; ease-out finishes on
            // its 24th 42 ms callback without another rendered rotation.
            // Used by the diagnostic capture scheduler, not game settlement.
            spinDuration = (delay + totalPathFrames - 1 + 24) * WheelFrameSeconds;
            Debug.Log("ROULETTE_WHEEL_TRACE: outcome=" + outcome +
                " variant=" + wheelVariant + " delayFrames=" + delay +
                " pathFrames=" + totalPathFrames + " finalStop=" + wheelFinalStopIndex);
            return true;
        }

        private bool StepWheelPlayback()
        {
            if (!wheelMotionReady) return true;
            bool landedThisFrame = false;
            OriginalPoint[] points;
            switch (wheelStage)
            {
                case WheelStage.Delay:
                    --wheelDelayFrames;
                    if (wheelDelayFrames > 0) break;
                    wheelStage = WheelStage.Loop;
                    wheelBallVisible = true;
                    AudioClip clip = wheelSpinSounds[wheelVariant];
                    if (clip != null)
                    {
                        wheelAudioSource.clip = clip;
                        wheelAudioSource.volume = 1f;
                        wheelAudioSource.Play();
                    }
                    goto case WheelStage.Loop;
                case WheelStage.Loop:
                    points = originalBallLoop.points;
                    wheelBallPosition = points[wheelPathFrame++].ToVector();
                    if (wheelPathFrame >= points.Length)
                    {
                        wheelPathFrame = 0;
                        wheelStage = WheelStage.Descent;
                    }
                    break;
                case WheelStage.Descent:
                    points = originalBallMotion.descent[wheelVariant].points;
                    wheelBallPosition = points[wheelPathFrame++].ToVector();
                    if (wheelPathFrame >= points.Length)
                    {
                        wheelPathFrame = 0;
                        wheelStage = WheelStage.Land;
                    }
                    break;
                case WheelStage.Land:
                    points = originalBallMotion.land[wheelVariant].points;
                    wheelBallPosition = points[wheelPathFrame++].ToVector();
                    if (wheelPathFrame >= points.Length)
                    {
                        wheelPathFrame = 0;
                        wheelStage = WheelStage.EaseOut;
                        landedThisFrame = true;
                    }
                    break;
                case WheelStage.EaseOut:
                    // Initialize(step, 0, framesPerStop=2): acceleration is
                    // step / (2^2 * 0.25), so the velocity reaches zero in 1 s.
                    if (OriginalWheelArithmetic.Ease(ref wheelEaseRemaining, ref wheelEaseVelocity))
                    {
                        wheelAudioSource.Stop();
                        wheelAngle = OriginalWheelArithmetic.Align(wheelAngle);
                        // Original Update() stores the aligned phase for the
                        // next spin without moving the ball or rotating either
                        // sprite on this terminal timer tick.
                        wheelStage = WheelStage.Done;
                        Debug.Log("ROULETTE_WHEEL_SETTLED: pocket=" + wheelOutcome +
                            " storedAngle=" + wheelAngle +
                            " displayAngle=" + wheelRenderedAngle +
                            " ball=" + wheelBallPosition +
                            " elapsedMs=" +
                            ((Time.realtimeSinceStartup - spinStartedAt) * 1000f));
                        return true;
                    }
                    OriginalWheelArithmetic.Advance(ref wheelAngle, (int)wheelEaseVelocity, out wheelMainRenderAngle);
                    wheelSpindleRenderAngle = wheelMainRenderAngle;
                    wheelRenderedAngle = wheelAngle;
                    wheelBallPosition = BallCenterForPocket(wheelOutcome, wheelAngle);
                    landedThisFrame = true;
                    break;
                default:
                    return true;
            }
            if (!landedThisFrame)
            {
                OriginalWheelArithmetic.Advance(ref wheelAngle, WheelStepDegrees, out wheelMainRenderAngle);
                wheelSpindleRenderAngle = wheelMainRenderAngle;
                wheelRenderedAngle = wheelAngle;
            }
            return false;
        }

        private void RequestWheelSkillStop()
        {
            if (PcGameOrdinaryInputBlocked) return;
            // The packaged non-Australian PC scenario maps the cabinet's
            // physical START button to STOP while the wheel is spinning.
            if (auNationalStandardsEnabled || !spinning || !wheelMotionReady ||
                wheelStage == WheelStage.EaseOut || wheelStage == WheelStage.Done)
                return;
            wheelStage = WheelStage.EaseOut;
            wheelBallVisible = true;
            wheelSkillStopRequested = true;
            wheelAudioSource.Stop();
            if (autoplay) ToggleAuto();
            Debug.Log("ROULETTE_SKILL_STOP: pocket=" + wheelOutcome +
                      " elapsed=" + (Time.realtimeSinceStartup - spinStartedAt));
        }

        private void SnapWheelForTurboAutoPlay()
        {
            // StateSpin invokes DoStop immediately after Play when both
            // AutoPlay and Turbo are set. DoStop calls SnapToFinalOutcome:
            // the timer enters its normal ease-out stage without consuming
            // the delay, loop, descent or landing points.
            if (!spinning || !wheelMotionReady || wheelStage == WheelStage.Done)
                return;
            wheelStage = WheelStage.EaseOut;
            wheelBallVisible = true;
            wheelTurboSnapRequested = true;
            wheelAudioSource.Stop();
            Debug.Log("ROULETTE_TURBO_WHEEL_SNAP: pocket=" + wheelOutcome +
                      " variant=" + wheelVariant);
        }
    }
}
