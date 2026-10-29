using System;

namespace RouletteDeluxe
{
    // SpriteMovie2D::Play stores float(1000/42). The original renderer
    // divides 1,000,000 by that stored FPS and uses FISTP with RC=11.
    // Its frame divisor is 41,999 us; a whole loop is truncated separately.
    public sealed class OriginalFooterMovieClock
    {
        public const long FrameMicroseconds = 41999;
        private static readonly float SourceFps = BitConverter.ToSingle(
            BitConverter.GetBytes(0x41BE79E8u), 0);

        public readonly int FrameCount;
        public readonly bool Repeat;
        public readonly long LoopMicroseconds;
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }
        public int Frame { get; private set; }
        public long AnchorMicroseconds { get; private set; }
        public long CarryMicroseconds { get; private set; }
        private long pausedAtMicroseconds;

        public OriginalFooterMovieClock(int frameCount, bool repeat,
            long timestamp, long frequency)
        {
            if (frameCount <= 0) throw new ArgumentOutOfRangeException("frameCount");
            FrameCount = frameCount;
            Repeat = repeat;
            LoopMicroseconds = (long)(frameCount * 1000000d / SourceFps);
            AnchorMicroseconds = MicrosecondsAt(timestamp, frequency);
            Frame = -1;
            Playing = true;
        }

        public static long MicrosecondsAt(long timestamp, long frequency)
        {
            if (timestamp < 0 || frequency <= 0)
                throw new ArgumentOutOfRangeException("timestamp");
            // Keep absolute microsecond quantization, without converting a
            // years-long process timestamp to float or double.
            return checked(timestamp / frequency * 1000000L +
                           timestamp % frequency * 1000000L / frequency);
        }

        public int UpdateAt(long timestamp, long frequency)
        {
            if (!Playing) return Frame;
            long now = MicrosecondsAt(timestamp, frequency);
            if (now < AnchorMicroseconds)
                throw new ArgumentOutOfRangeException("timestamp");
            long elapsed = now - AnchorMicroseconds;
            long candidate = (elapsed + CarryMicroseconds) / FrameMicroseconds;
            if (candidate >= FrameCount)
            {
                if (Repeat)
                {
                    long loops = candidate / FrameCount;
                    candidate -= loops * FrameCount;
                    CarryMicroseconds -= checked(loops * LoopMicroseconds);
                }
                else
                {
                    candidate = FrameCount - 1;
                    Playing = false;
                }
            }
            // UpdateForward commits elapsed time only when the visible
            // frame changes, including a loop wrap. Equal frames keep the
            // original anchor and the newly adjusted accumulator.
            if (candidate != Frame)
            {
                CarryMicroseconds += elapsed;
                AnchorMicroseconds = now;
            }
            Frame = (int)candidate;
            return Frame;
        }

        public bool PauseAt(long timestamp, long frequency)
        {
            if (!Playing) return false;
            long now = MicrosecondsAt(timestamp, frequency);
            if (now < AnchorMicroseconds)
                throw new ArgumentOutOfRangeException("timestamp");
            // SSprite2DMovie::Pause commits elapsed microseconds without
            // UpdateForward: the displayed frame is not advanced here.
            CarryMicroseconds = checked(CarryMicroseconds + (now - AnchorMicroseconds));
            pausedAtMicroseconds = now;
            Playing = false;
            Paused = true;
            return true;
        }

        public bool ResumeAt(long timestamp, long frequency)
        {
            if (!Paused) return false;
            long now = MicrosecondsAt(timestamp, frequency);
            if (now < pausedAtMicroseconds)
                throw new ArgumentOutOfRangeException("timestamp");
            // PlayNow resumes an existing paused sprite with its frame and
            // accumulator intact. Only its live-time anchor is replaced.
            AnchorMicroseconds = now;
            Playing = true;
            Paused = false;
            return true;
        }
    }
}
