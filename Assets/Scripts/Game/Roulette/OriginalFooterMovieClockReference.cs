using System;
using System.Globalization;
using UnityEngine;

namespace RouletteDeluxe
{
    public static class OriginalFooterMovieClockReference
    {
        public static int Validate()
        {
            TextAsset asset = Resources.Load<TextAsset>("Original/footer_movie_clock_vectors");
            if (asset == null) throw new InvalidOperationException("Native footer oracle missing");
            string[] lines = asset.text.Split(new[] {'\r','\n'}, StringSplitOptions.RemoveEmptyEntries);
            if (lines[0] != "RATE,41BE79E8,41999,1511999,503999")
                throw new InvalidOperationException("Native FPS/truncation oracle differs");
            int checks = 1;
            foreach (int days in new[] {0,180,730})
                foreach (long frequency in new[] {1000000L,10000000L})
                {
                    long epoch = days * 86400L * frequency;
                    long epochMicroseconds = days * 86400L * 1000000L;
                    OriginalFooterMovieClock clock = null;
                    int sequence = 0;
                    foreach (string line in lines)
                    {
                        string[] fields = line.Split(',');
                        if (fields[0] == "RATE") continue;
                        if (fields[0] == "RESET")
                        {
                            sequence = int.Parse(fields[1], CultureInfo.InvariantCulture);
                            clock = new OriginalFooterMovieClock(
                                int.Parse(fields[2], CultureInfo.InvariantCulture),
                                fields[3] == "1", epoch, frequency);
                            continue;
                        }
                        long microseconds = long.Parse(fields[2], CultureInfo.InvariantCulture);
                        long timestamp = epoch + microseconds * (frequency / 1000000L);
                        int frame = clock.UpdateAt(timestamp, frequency);
                        if (int.Parse(fields[1], CultureInfo.InvariantCulture) != sequence ||
                            frame != int.Parse(fields[3], CultureInfo.InvariantCulture) ||
                            clock.Playing != (fields[4] == "1") ||
                            clock.AnchorMicroseconds != epochMicroseconds +
                                long.Parse(fields[5], CultureInfo.InvariantCulture) ||
                            clock.CarryMicroseconds != long.Parse(fields[6], CultureInfo.InvariantCulture))
                            throw new InvalidOperationException("Native footer playback differs: " +
                                days + " days, " + frequency + " Hz, " + line);
                        ++checks;
                    }
                }
            checks += ValidatePauseResume();
            Debug.Log("ROULETTE_FOOTER_NATIVE_CLOCK_PASSED: " + checks +
                " oracle and pause/resume checks; x87 FPS/truncation; finite/repeat; dense/sparse/irregular service; 0/180/730 days; 1MHz/10MHz");
            return checks;
        }

        private static void Require(bool condition, string message, ref int checks)
        {
            if (!condition) throw new InvalidOperationException("Footer pause: " + message);
            ++checks;
        }

        public static int ValidatePauseResume()
        {
            int checks = 0;
            foreach (int days in new[] { 0, 180, 730 })
                foreach (long frequency in new[] { 1000000L, 10000000L })
                {
                    long epoch = days * 86400L * frequency;
                    long epochUs = days * 86400L * 1000000L;
                    long scale = frequency / 1000000L;
                    var clock = new OriginalFooterMovieClock(36, true, epoch, frequency);
                    Require(clock.PauseAt(epoch + 50000 * scale, frequency) &&
                        clock.Paused && !clock.Playing && clock.Frame == -1 &&
                        clock.CarryMicroseconds == 50000 && clock.AnchorMicroseconds == epochUs,
                        "Pause accumulates unserviced time without rendering a frame", ref checks);
                    Require(!clock.PauseAt(epoch + 100000 * scale, frequency) &&
                        clock.UpdateAt(epoch + 200000 * scale, frequency) == -1 &&
                        clock.CarryMicroseconds == 50000,
                        "duplicate Pause and paused Update retain carry/frame", ref checks);
                    Require(clock.ResumeAt(epoch + 300000 * scale, frequency) &&
                        clock.Playing && !clock.Paused && clock.Frame == -1 &&
                        clock.AnchorMicroseconds == epochUs + 300000 &&
                        clock.CarryMicroseconds == 50000,
                        "Resume keeps carry/frame and resets anchor", ref checks);
                    Require(!clock.ResumeAt(epoch + 320000 * scale, frequency) &&
                        clock.AnchorMicroseconds == epochUs + 300000,
                        "duplicate Resume cannot shift the live anchor", ref checks);
                    Require(clock.UpdateAt(epoch + 300000 * scale, frequency) == 1 &&
                        clock.UpdateAt(epoch + 333997 * scale, frequency) == 1 &&
                        clock.UpdateAt(epoch + 333998 * scale, frequency) == 2,
                        "first resumed Update consumes retained elapsed time with native frame divisor", ref checks);

                    var baseline = new OriginalFooterMovieClock(36, true, epoch, frequency);
                    var suspended = new OriginalFooterMovieClock(36, true, epoch, frequency);
                    baseline.UpdateAt(epoch + 50000 * scale, frequency);
                    suspended.UpdateAt(epoch + 50000 * scale, frequency);
                    suspended.PauseAt(epoch + 50000 * scale, frequency);
                    suspended.ResumeAt(epoch + 550000 * scale, frequency);
                    foreach (long us in new[] { 50000L, 83998L, 1512000L, 1600000L, 4536000L, 6000000L })
                    {
                        int expected = baseline.UpdateAt(epoch + us * scale, frequency);
                        int actual = suspended.UpdateAt(epoch + (us + 500000) * scale, frequency);
                        Require(actual == expected && suspended.Playing == baseline.Playing &&
                            suspended.CarryMicroseconds == baseline.CarryMicroseconds &&
                            suspended.AnchorMicroseconds == baseline.AnchorMicroseconds + 500000,
                            "repeat playback excludes pause duration at " + us + " us", ref checks);
                    }

                    var finite = new OriginalFooterMovieClock(12, false, epoch, frequency);
                    finite.UpdateAt(epoch + 419990 * scale, frequency);
                    Require(finite.PauseAt(epoch + 503988 * scale, frequency) &&
                        finite.Frame == 10 && finite.Paused && !finite.Playing,
                        "finite Pause does not render or falsely signal completion", ref checks);
                    finite.ResumeAt(epoch + 1500000 * scale, frequency);
                    Require(finite.UpdateAt(epoch + 1500000 * scale, frequency) == 11 &&
                        !finite.Playing && !finite.Paused,
                        "completion occurs when resumed Update reaches end", ref checks);
                    Require(!finite.PauseAt(epoch + 1600000 * scale, frequency) &&
                        !finite.ResumeAt(epoch + 1700000 * scale, frequency) && !finite.Playing,
                        "completed finite movie never receives resume entitlement", ref checks);
                }
            return checks;
        }
    }
}
