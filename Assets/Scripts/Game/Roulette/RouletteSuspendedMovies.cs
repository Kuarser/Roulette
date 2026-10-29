using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Renderer PauseAllMovies remembers actual instances. An object that
        // starts after Pause, or replaces a stopped object, is not in this set.
        private readonly HashSet<object> pcPausedMovies = new HashSet<object>();
        private PcMoviePlayback pcTableMarkerMovie, pcTopperAttractMovie;
        private PcMoviePlayback pcIDeckBackgroundMovie, pcIDeckForegroundMovie, pcIDeckTouchMovie;
        private long pcMovieNextIdentity, pcMoviePauseAppliedCount, pcMovieResumeAppliedCount;
        private OriginalFooterMovieClock pcObservedFooterMovie;
        private long pcFooterMovieIdentity, pcIDeckAttractStartedTimestamp;
        private long pcIDeckForegroundGroup = -1;

        // PC movies whose existing drawing path uses analytic frame selection
        // retain that frame rule. This only accounts for their own Pause/Play
        // lifetime; it never changes a game deadline, fader, or Feature timer.
        private sealed class PcMoviePlayback
        {
            public readonly string Name;
            public readonly long Identity, SourceEpoch, Frequency;
            public readonly object SourceAsset;
            public readonly int FrameCount, RateNumerator, RateDenominator;
            public readonly bool Repeat;
            public bool Playing { get; private set; }
            public bool Paused { get; private set; }
            public int Frame { get; private set; }
            public long FrameOrdinal { get; private set; }
            private long excludedPauseTicks, pausedAt;

            public PcMoviePlayback(string name, long identity, object asset, int frameCount,
                long epoch, long frequency, int numerator, int denominator, bool repeat)
            {
                if (frameCount < 1 || frequency < 1 || numerator < 1 || denominator < 1 || epoch < 0)
                    throw new ArgumentOutOfRangeException("movie clock");
                Name = name; Identity = identity; SourceAsset = asset; FrameCount = frameCount;
                SourceEpoch = epoch; Frequency = frequency;
                RateNumerator = numerator; RateDenominator = denominator; Repeat = repeat;
                Playing = true;
            }

            public int SampleAt(long now)
            {
                if (!Playing) return Frame;
                long elapsedTicks = Math.Max(0, now - SourceEpoch - excludedPauseTicks);
                long milliseconds = Decimal.ToInt64(Decimal.Floor((decimal)elapsedTicks * 1000m / Frequency));
                FrameOrdinal = Decimal.ToInt64(Decimal.Floor((decimal)milliseconds * RateNumerator / RateDenominator));
                if (!Repeat && FrameOrdinal >= FrameCount)
                { Frame = FrameCount - 1; Playing = false; }
                else Frame = (int)(FrameOrdinal % FrameCount);
                return Frame;
            }

            public long TimestampForFrame(long ordinal)
            {
                long ticks = Decimal.ToInt64(Decimal.Ceiling(
                    (decimal)ordinal * RateDenominator * Frequency / (1000m * RateNumerator)));
                return checked(SourceEpoch + excludedPauseTicks + ticks);
            }

            public bool PauseAt(long now)
            {
                if (!Playing) return false;
                // No SampleAt here: retain the last frame actually selected
                // for drawing, including an unserviced interval before Pause.
                pausedAt = now;
                Playing = false;
                Paused = true;
                return true;
            }

            public bool ResumeAt(long now)
            {
                if (!Paused) return false;
                if (now < pausedAt) throw new ArgumentOutOfRangeException("now");
                excludedPauseTicks = checked(excludedPauseTicks + (now - pausedAt));
                Playing = true;
                Paused = false;
                return true;
            }

            public void Stop() { Playing = false; Paused = false; }

            public string Snapshot()
            {
                return Name + "#" + Identity + ":frame=" + Frame + ",ordinal=" + FrameOrdinal +
                    ",playing=" + (Playing ? 1 : 0) + ",paused=" + (Paused ? 1 : 0);
            }
        }

        private PcMoviePlayback NewPcMovie(string name, Texture2D[] frames, long epoch,
            int numerator, int denominator, bool repeat)
        {
            return frames == null || frames.Length == 0 ? null : new PcMoviePlayback(name,
                checked(++pcMovieNextIdentity), frames, frames.Length, epoch,
                Stopwatch.Frequency, numerator, denominator, repeat);
        }

        private static void StopPcMovie(ref PcMoviePlayback movie)
        {
            if (movie != null) movie.Stop();
            movie = null;
        }

        private void StartPcTableMarkerMovie(Texture2D[] frames)
        {
            StopPcMovie(ref pcTableMarkerMovie);
            pcTableMarkerMovie = NewPcMovie("table", frames, tableMarkStartedTimestamp, 24, 1000, true);
        }

        private int PcTableMarkerFrameAt(Texture2D[] frames, long now)
        {
            if (pcTableMarkerMovie == null || pcTableMarkerMovie.SourceEpoch != tableMarkStartedTimestamp ||
                !ReferenceEquals(pcTableMarkerMovie.SourceAsset, frames)) StartPcTableMarkerMovie(frames);
            return pcTableMarkerMovie == null ? 0 : pcTableMarkerMovie.SampleAt(now);
        }

        private void StartPcTopperAttractMovie()
        {
            StopPcMovie(ref pcTopperAttractMovie);
            pcTopperAttractMovie = NewPcMovie("topper", topperFrames,
                topperAttractStartedTimestamp, 1, 42, true);
        }

        private int PcTopperAttractFrameAt(long now)
        {
            if (pcTopperAttractMovie == null || pcTopperAttractMovie.SourceEpoch != topperAttractStartedTimestamp ||
                !ReferenceEquals(pcTopperAttractMovie.SourceAsset, topperFrames)) StartPcTopperAttractMovie();
            return pcTopperAttractMovie == null ? 0 : pcTopperAttractMovie.SampleAt(now);
        }

        private void StartPcIDeckAttractMovies(long now)
        {
            StopPcIDeckAttractMovies();
            pcIDeckAttractStartedTimestamp = now;
            pcIDeckBackgroundMovie = NewPcMovie("ideck-background", ideckAttractBackgroundFrames,
                now, 1, 66, true);
            pcIDeckForegroundMovie = NewPcMovie("ideck-foreground", ideckAttractForegroundFrames,
                now, 1, 66, false);
            pcIDeckForegroundGroup = 0;
        }

        private void StopPcIDeckAttractMovies()
        {
            StopPcMovie(ref pcIDeckBackgroundMovie);
            StopPcMovie(ref pcIDeckForegroundMovie);
            pcIDeckForegroundGroup = -1;
        }

        private void PcIDeckAttractFramesAt(long now, out int background, out int foreground)
        {
            if (pcIDeckBackgroundMovie == null)
                StartPcIDeckAttractMovies(pcIDeckAttractStartedTimestamp == 0 ? now : pcIDeckAttractStartedTimestamp);
            background = pcIDeckBackgroundMovie == null ? 0 : pcIDeckBackgroundMovie.SampleAt(now);
            foreground = -1;
            if (pcIDeckBackgroundMovie == null) return;
            long group = pcIDeckBackgroundMovie.FrameOrdinal / pcIDeckBackgroundMovie.FrameCount;
            if ((group & 1) != 0)
            {
                StopPcMovie(ref pcIDeckForegroundMovie);
                pcIDeckForegroundGroup = -1;
                return;
            }
            if (pcIDeckForegroundGroup != group || pcIDeckForegroundMovie == null)
            {
                StopPcMovie(ref pcIDeckForegroundMovie);
                long began = pcIDeckBackgroundMovie.TimestampForFrame(group * pcIDeckBackgroundMovie.FrameCount);
                pcIDeckForegroundMovie = NewPcMovie("ideck-foreground", ideckAttractForegroundFrames,
                    began, 1, 66, false);
                pcIDeckForegroundGroup = group;
            }
            // The completed foreground keeps its last image for this group,
            // but is no longer Playing and must not be resumed by Menu.
            if (pcIDeckForegroundMovie != null) foreground = pcIDeckForegroundMovie.SampleAt(now);
        }

        private void StartPcIDeckTouchMovie()
        {
            StopPcMovie(ref pcIDeckTouchMovie);
            pcIDeckTouchMovie = NewPcMovie("ideck-touch", ideckTouchFrames,
                ideckTouchStartedTimestamp, 1, (int)IDeckTouchFrameMilliseconds, false);
        }

        private void SyncPcMovieLifetimes()
        {
            if (session == null || currentTableResultMarkerCleared || spinning || session.LastResult == null ||
                session.LastResult.Pocket != tableMarkedPocket || tableMarkStartedTimestamp == 0)
                StopPcMovie(ref pcTableMarkerMovie);
            if (!topperAttractPreview) StopPcMovie(ref pcTopperAttractMovie);
            if (!ideckAttractPreview) StopPcIDeckAttractMovies();
            if (ideckTouchChipIndex < 0 || ideckTouchStartedTimestamp == 0)
                StopPcMovie(ref pcIDeckTouchMovie);
            TrackPcFooterMovieInstance();
        }

        private void TrackPcFooterMovieInstance()
        {
            if (!ReferenceEquals(pcObservedFooterMovie, footerMovieClock))
            {
                pcObservedFooterMovie = footerMovieClock;
                pcFooterMovieIdentity = footerMovieClock == null ? 0 : checked(++pcMovieNextIdentity);
            }
        }

        private bool PcOwnsMovie(PcMoviePlayback movie)
        {
            return ReferenceEquals(movie, pcTableMarkerMovie) || ReferenceEquals(movie, pcTopperAttractMovie) ||
                ReferenceEquals(movie, pcIDeckBackgroundMovie) || ReferenceEquals(movie, pcIDeckForegroundMovie) ||
                ReferenceEquals(movie, pcIDeckTouchMovie);
        }

        private void PausePcGameMovies()
        {
            SyncPcMovieLifetimes();
            long now = Stopwatch.GetTimestamp();
            if (footerMovieClock != null && footerMovieClock.PauseAt(now, Stopwatch.Frequency))
            { pcPausedMovies.Add(footerMovieClock); ++pcMoviePauseAppliedCount; }
            foreach (PcMoviePlayback movie in new[] { pcTableMarkerMovie, pcTopperAttractMovie,
                pcIDeckBackgroundMovie, pcIDeckForegroundMovie, pcIDeckTouchMovie })
                if (movie != null && movie.PauseAt(now))
                { pcPausedMovies.Add(movie); ++pcMoviePauseAppliedCount; }
        }

        private void ResumePcGameMovies()
        {
            SyncPcMovieLifetimes();
            long now = Stopwatch.GetTimestamp();
            foreach (object entry in pcPausedMovies)
            {
                var footer = entry as OriginalFooterMovieClock;
                if (footer != null)
                {
                    if (ReferenceEquals(footer, footerMovieClock) && footer.ResumeAt(now, Stopwatch.Frequency))
                        ++pcMovieResumeAppliedCount;
                    continue;
                }
                var movie = (PcMoviePlayback)entry;
                if (PcOwnsMovie(movie) && movie.ResumeAt(now)) ++pcMovieResumeAppliedCount;
            }
            pcPausedMovies.Clear();
        }

        // Read-only Player evidence: no frame sampling, timer progress, or
        // lifecycle synchronization occurs merely by reading these properties.
        public long PcMoviePauseCount { get { return pcMoviePauseAppliedCount; } }
        public long PcMovieResumeCount { get { return pcMovieResumeAppliedCount; } }
        public int PcPausedMovieCount
        {
            get
            {
                int count = 0;
                foreach (object entry in pcPausedMovies)
                {
                    var footer = entry as OriginalFooterMovieClock;
                    if (footer != null)
                    { if (ReferenceEquals(footer, footerMovieClock) && footer.Paused) ++count; }
                    else
                    { var movie = (PcMoviePlayback)entry; if (PcOwnsMovie(movie) && movie.Paused) ++count; }
                }
                return count;
            }
        }

        public string PcMoviePlaybackSnapshot
        {
            get
            {
                var states = new List<string>();
                if (footerMovieClock != null)
                    states.Add("footer#" + pcFooterMovieIdentity + ":phase=" + footerMoviePhase +
                        ",frame=" + footerMovieClock.Frame + ",playing=" + (footerMovieClock.Playing ? 1 : 0) +
                        ",paused=" + (footerMovieClock.Paused ? 1 : 0));
                foreach (PcMoviePlayback movie in new[] { pcTableMarkerMovie, pcTopperAttractMovie,
                    pcIDeckBackgroundMovie, pcIDeckForegroundMovie, pcIDeckTouchMovie })
                    if (movie != null) states.Add(movie.Snapshot());
                return string.Join("|", states.ToArray());
            }
        }
    }
}
