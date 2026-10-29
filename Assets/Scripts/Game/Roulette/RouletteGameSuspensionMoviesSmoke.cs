using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // This routine is selected only by the existing isolated F harness.
        // All clocks, textures, selectors and Menu commands are real owners.
        // No timestamp, movie flag, frame, wallet or persisted state is injected.
        private IEnumerator GsSmokeMovieInstances()
        {
            SetPcMenuLifecycleContext(0, false, false, true, false, false);
            SetTopperAttractPreview();
            RefreshDiagnosticUi();
            yield return GsSmokeObserve(180);
            PcMoviePlayback topper = pcTopperAttractMovie;
            RequireGsSmoke(topper != null && topper.Playing && topper.FrameOrdinal > 0 &&
                topperCanvas.enabled && topperMovie.enabled &&
                topperMovie.texture == topperFrames[topper.Frame], "real topper movie was not drawn");
            GsSmokeCheckpoint("MovieTopperCreated");

            // This PC drawing path leaves the hidden Topper owner playing
            // without selecting frames. Use that unserviced interval to test
            // Pause's retained carry; original renderer visibility is separate.
            yield return GsSmokeSuspend("movie-topper-hide", "direct");
            int hiddenFrame = topper.Frame;
            long hiddenOrdinal = topper.FrameOrdinal;
            yield return GsSmokeObserve(210);
            RequireGsSmoke(topper.Playing && !topper.Paused && topper.Frame == hiddenFrame &&
                !topperCanvas.enabled && !pcGameScreensVisible, "hidden topper unexpectedly sampled or paused");
            RequestPcMenuSuspend(true, "movie-topper-pause");
            yield return GsSmokeWait(() => GsSmokeHas("GameSuspendExecuted", "movie-topper-pause"),
                "hidden topper Menu pause", 5);
            RequireGsSmoke(!pcGameScreensVisible && ReferenceEquals(topper, pcTopperAttractMovie) &&
                topper.Paused && !topper.Playing && topper.Frame == hiddenFrame &&
                pcPausedMovies.Contains(topper), "Menu Pause updated or replaced hidden topper");
            long pausedCount = PcMoviePauseCount;
            RequestPcMenuSuspend(true, "movie-topper-pause-repeat");
            yield return GsSmokeWait(() => GsSmokeHas("GameSuspendExecuted", "movie-topper-pause-repeat"),
                "hidden topper duplicate pause", 5);
            yield return GsSmokeObserve(160);
            RequireGsSmoke(topper.Frame == hiddenFrame && topper.FrameOrdinal == hiddenOrdinal &&
                PcMoviePauseCount == pausedCount, "duplicate Menu Pause sampled topper or captured it twice");
            // Source rpc0's Hide/Show makes the paused image visible without
            // granting a renderer Resume. This gives an actual UI screenshot.
            yield return GsSmokeSuspend("movie-topper-paint-paused", "rpc0");
            RefreshDiagnosticUi();
            RequireGsSmoke(topperCanvas.enabled && topperMovie.texture == topperFrames[hiddenFrame] &&
                topper.Paused && topper.Frame == hiddenFrame, "Show changed paused topper frame");
            yield return GsSmokeCapture("topper_movie_paused");
            yield return GsSmokeResumeMovies("movie-topper-resume", new[] { topper }, null);
            yield return new WaitForEndOfFrame();
            RequireGsSmoke(ReferenceEquals(topper, pcTopperAttractMovie) && topper.FrameOrdinal >= hiddenOrdinal + 4 &&
                topperMovie.texture == topperFrames[topper.Frame], "first resumed topper draw lost unserviced elapsed carry");
            GsSmokeCheckpoint("MovieTopperFirstResumedDraw");
            yield return GsSmokeCapture("topper_movie_resumed");
            SetTopperPreview(false);
            RequireGsSmoke(pcTopperAttractMovie == null && !topper.Playing && !topper.Paused,
                "topper preview stop left a resumable old instance");

            SetIDeckAttractPreview();
            RefreshDiagnosticUi();
            yield return GsSmokeObserve(160);
            PcMoviePlayback background = pcIDeckBackgroundMovie, foreground = pcIDeckForegroundMovie;
            RequireGsSmoke(background != null && foreground != null && background.Playing && foreground.Playing &&
                background.FrameOrdinal > 0 && foreground.FrameOrdinal > 0 && ideckCanvas.enabled &&
                ideckAttractBackground.texture == ideckAttractBackgroundFrames[background.Frame] &&
                ideckAttractForeground.enabled && ideckAttractForeground.texture == ideckAttractForegroundFrames[foreground.Frame],
                "real iDeck attract movies were not drawn");
            GsSmokeCheckpoint("MovieIDeckAttractCreated");
            yield return GsSmokeSuspend("movie-ideck-pause");
            int backgroundFrame = background.Frame, foregroundFrame = foreground.Frame;
            pausedCount = PcMoviePauseCount;
            RequireGsSmoke(background.Paused && foreground.Paused && pcPausedMovies.Contains(background) &&
                pcPausedMovies.Contains(foreground), "Menu omitted real iDeck attract instances");
            yield return GsSmokeSuspend("movie-ideck-pause-repeat");
            yield return GsSmokeObserve(300);
            RequireGsSmoke(background.Frame == backgroundFrame && foreground.Frame == foregroundFrame &&
                !background.Playing && !foreground.Playing && PcMoviePauseCount == pausedCount &&
                ideckAttractBackground.texture == ideckAttractBackgroundFrames[backgroundFrame] &&
                ideckAttractForeground.texture == ideckAttractForegroundFrames[foregroundFrame],
                "paused iDeck attract image or captured instance set advanced");
            yield return GsSmokeCapture("ideck_attract_paused");
            yield return GsSmokeResumeMovies("movie-ideck-resume", new[] { background, foreground }, null);
            long resumedBackgroundOrdinal = background.FrameOrdinal;
            yield return GsSmokeObserve(150);
            RequireGsSmoke(background.FrameOrdinal > resumedBackgroundOrdinal &&
                ideckAttractBackground.texture == ideckAttractBackgroundFrames[background.Frame],
                "resumed iDeck background did not update its actual image");

            // A naturally completed finite foreground keeps its last image,
            // but Menu cannot turn it back into a playing movie.
            yield return GsSmokeWait(() => ReferenceEquals(foreground, pcIDeckForegroundMovie) &&
                !foreground.Playing && !foreground.Paused, "iDeck foreground natural completion", 10);
            RequireGsSmoke(foreground.Frame == ideckAttractForegroundFrames.Length - 1 &&
                ideckAttractForeground.enabled && ideckAttractForeground.texture == ideckAttractForegroundFrames[foreground.Frame],
                "completed iDeck foreground did not hold its last actual image");
            yield return GsSmokeSuspend("movie-ideck-completed-pause");
            RequireGsSmoke(background.Paused && !foreground.Playing && !foreground.Paused &&
                !pcPausedMovies.Contains(foreground), "completed foreground incorrectly entered the pause set");
            yield return GsSmokeObserve(150);
            GsSmokeCheckpoint("MovieIDeckCompletedForegroundPaused");
            yield return GsSmokeResumeMovies("movie-ideck-completed-resume", new[] { background }, null);
            RequireGsSmoke(!foreground.Playing && !foreground.Paused &&
                foreground.Frame == ideckAttractForegroundFrames.Length - 1, "Resume restarted completed foreground");

            // The fixture invokes the existing attract owner creation entry
            // while its previous instances are captured. Ordinary player keys
            // remain gated; this tests a new actual UI movie lifetime.
            SetIDeckAttractPreview();
            RefreshDiagnosticUi();
            yield return GsSmokeObserve(100);
            background = pcIDeckBackgroundMovie; foreground = pcIDeckForegroundMovie;
            yield return GsSmokeSuspend("movie-ideck-replace-pause");
            RequireGsSmoke(background.Paused && foreground.Paused &&
                pcPausedMovies.Contains(background) && pcPausedMovies.Contains(foreground),
                "replacement setup lacked captured old iDeck movies");
            SetIDeckAttractPreview();
            RefreshDiagnosticUi();
            PcMoviePlayback newBackground = pcIDeckBackgroundMovie, newForeground = pcIDeckForegroundMovie;
            RequireGsSmoke(newBackground != null && newForeground != null &&
                !ReferenceEquals(newBackground, background) && !ReferenceEquals(newForeground, foreground) &&
                newBackground.Playing && newForeground.Playing && !newBackground.Paused && !newForeground.Paused &&
                !background.Playing && !background.Paused && !foreground.Playing && !foreground.Paused &&
                !pcPausedMovies.Contains(newBackground) && !pcPausedMovies.Contains(newForeground),
                "new iDeck instances inherited the retired instances' pause entitlement");
            GsSmokeCheckpoint("MovieIDeckInstancesReplacedWhilePaused", background.Snapshot() + "|" + foreground.Snapshot());
            yield return GsSmokeObserve(160);
            RequireGsSmoke(pcGameAppSuspended && newBackground.FrameOrdinal > 0 && newForeground.FrameOrdinal > 0 &&
                ideckAttractBackground.texture == ideckAttractBackgroundFrames[newBackground.Frame] &&
                ideckAttractForeground.texture == ideckAttractForegroundFrames[newForeground.Frame],
                "new uncaptured iDeck movies were globally frozen during Game suspension");
            yield return GsSmokeCapture("ideck_replaced_running");
            yield return GsSmokeResumeMovies("movie-ideck-replaced-resume", null, new[] { newBackground, newForeground });
            RequireGsSmoke(!background.Playing && !background.Paused && !foreground.Playing && !foreground.Paused &&
                ReferenceEquals(newBackground, pcIDeckBackgroundMovie) && ReferenceEquals(newForeground, pcIDeckForegroundMovie),
                "old Menu Resume restarted retired or replaced new iDeck movies");
            GsSmokeCheckpoint("MovieIDeckRetiredAfterResume", background.Snapshot() + "|" + foreground.Snapshot());

            SetIDeckPreview(true);
            RefreshPortraitUi();
            yield return null;
            int chip = session.Chip == chips[1] ? 2 : 1;
            QueueStartGameButton(ideckChipButtons[chip], 591, true);
            yield return GsSmokeWait(() => session.Chip == chips[chip] && pcIDeckTouchMovie != null &&
                pcIDeckTouchMovie.Playing && SourcePlayerInputs.Count == 0, "real iDeck chip touch", 5);
            // The source selector closes preview on down. Reopen the existing
            // display so the same touch object's actual RawImage is visible.
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            PcMoviePlayback touch = pcIDeckTouchMovie;
            RequireGsSmoke(touch != null && touch.Playing && ideckTouchChipIndex == chip &&
                ideckChipTouchImages[chip].gameObject.activeSelf &&
                ideckChipTouchImages[chip].texture == ideckTouchFrames[touch.Frame], "chip input did not create its actual touch image");
            GsSmokeCheckpoint("MovieIDeckTouchCreated");
            yield return GsSmokeSuspend("movie-touch-pause");
            int touchFrame = touch.Frame;
            pausedCount = PcMoviePauseCount;
            RequireGsSmoke(ReferenceEquals(touch, pcIDeckTouchMovie) && touch.Paused && pcPausedMovies.Contains(touch),
                "Menu omitted the real finite touch instance");
            yield return GsSmokeSuspend("movie-touch-pause-repeat");
            yield return GsSmokeObserve(1200);
            RequireGsSmoke(ReferenceEquals(touch, pcIDeckTouchMovie) && touch.Paused && !touch.Playing &&
                touch.Frame == touchFrame && PcMoviePauseCount == pausedCount &&
                ideckChipTouchImages[chip].gameObject.activeSelf && ideckChipTouchImages[chip].texture == ideckTouchFrames[touchFrame],
                "paused touch advanced or ran its finite completion handler after 1008 ms");
            yield return GsSmokeCapture("ideck_touch_paused");
            yield return GsSmokeResumeMovies("movie-touch-resume", new[] { touch }, null);
            yield return GsSmokeWait(() => pcIDeckTouchMovie == null && ideckTouchChipIndex < 0,
                "resumed touch natural completion", 5);
            RequireGsSmoke(!touch.Playing && !touch.Paused && ideckChipTouchImages.All(image => !image.gameObject.activeSelf),
                "touch completion did not stop and hide its actual movie");
            GsSmokeCheckpoint("MovieIDeckTouchCompleted", touch.Snapshot());
            yield return GsSmokeCapture("ideck_touch_completed");
            SetIDeckPreview(false);
            SetTopperPreview(false);
            RefreshPortraitUi();
            RequireGsSmoke(!pcGameAppSuspended && pcPausedMovies.Count == 0 && PcPausedMovieCount == 0 &&
                pcTopperAttractMovie == null && pcIDeckBackgroundMovie == null && pcIDeckForegroundMovie == null &&
                pcIDeckTouchMovie == null && session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                gsSmokePockets == 0 && gsSmokeSuits == 0 && sourceRoundEndCount == 0,
                "movie-only path retained a paused owner or changed game accounting");
        }

        private IEnumerator GsSmokeResumeMovies(string id, PcMoviePlayback[] captured, PcMoviePlayback[] untouched)
        {
            captured = captured ?? new PcMoviePlayback[0];
            untouched = untouched ?? new PcMoviePlayback[0];
            RequireGsSmoke(captured.All(movie => PcOwnsMovie(movie) && movie.Paused && !movie.Playing &&
                pcPausedMovies.Contains(movie)) && untouched.All(movie => PcOwnsMovie(movie) && movie.Playing &&
                !movie.Paused && !pcPausedMovies.Contains(movie)), "movie resume fixture does not match ownership");
            int[] heldFrames = captured.Select(movie => movie.Frame).ToArray();
            int[] untouchedFrames = null;
            long priorResumes = PcMovieResumeCount;
            int expectedResumes = PcPausedMovieCount;
            bool observed = false;
            Action<string, string> observer = (point, operationId) => {
                if (operationId != id) return;
                if (point == "GameResumeReceived") untouchedFrames = untouched.Select(movie => movie.Frame).ToArray();
                if (point != "FeatureNotify:LocalSend:64") return;
                RequireGsSmoke(untouchedFrames != null, "movie resume missed the actual dispatch start");
                for (int i = 0; i < captured.Length; ++i)
                    RequireGsSmoke(PcOwnsMovie(captured[i]) && captured[i].Playing && !captured[i].Paused &&
                        captured[i].Frame == heldFrames[i], "actual Resume changed captured frame or owner before Update");
                for (int i = 0; i < untouched.Length; ++i)
                    RequireGsSmoke(PcOwnsMovie(untouched[i]) && untouched[i].Playing && !untouched[i].Paused &&
                        untouched[i].Frame == untouchedFrames[i], "old Resume restarted or changed an uncaptured replacement");
                RequireGsSmoke(PcMovieResumeCount == priorResumes + expectedResumes && pcPausedMovies.Count == 0 &&
                    PcPausedMovieCount == 0, "Resume count includes stale/completed/new objects or left captured state");
                observed = true;
                GsSmokeCheckpoint("MovieResumeInsideDispatch", id);
            };
            PcGameSuspensionEffect += observer;
            try { yield return GsSmokeResume(id); }
            finally { PcGameSuspensionEffect -= observer; }
            RequireGsSmoke(observed, "movie retention was not observed inside actual Menu Resume dispatch");
        }
    }
}
