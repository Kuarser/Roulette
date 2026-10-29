using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class PaidFadeReceipt
        {
            public StartGameStateBetReceipt state;
            public string config, autoBytes;
            public bool muted;
            public int alphaBits, callbacks;
        }

        private string paidFadeSmokeCase, paidFadeSmokePhase;
        private int paidFadeSmokeChecks;
        private string PaidFadeReceiptKey { get { return recordedHistoryKey + ".PaidFadeFixture.v1"; } }

        private void RequirePaidFade(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("Paid fade: " + message);
            ++paidFadeSmokeChecks;
        }

        private static int PaidFadeBits(float value)
        { return BitConverter.ToInt32(BitConverter.GetBytes(value), 0); }

        private static string PaidFadeArgument(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, flag);
            if (at < 0 || at + 1 >= args.Length) throw new InvalidOperationException("Paid fade missing " + flag);
            return args[at + 1];
        }

        private IEnumerator PaidFadeSmoke()
        {
            paidFadeSmokeCase = PaidFadeArgument("-roulettePaidFadeCase");
            paidFadeSmokePhase = PaidFadeArgument("-roulettePaidFadePhase");
            bool coldLoad = paidFadeSmokeCase == "cold" && paidFadeSmokePhase == "load";
            RequirePaidFade(paidFadeSmokeCase == "clock_ui" || paidFadeSmokeCase == "au_zero" ||
                paidFadeSmokeCase == "normal" || paidFadeSmokeCase == "turbo" || paidFadeSmokeCase == "cold",
                "unknown case");
            RequirePaidFade(paidFadeSmokePhase == (paidFadeSmokeCase == "cold" ?
                (coldLoad ? "load" : "save") : "run"), "unknown phase");
            RequirePaidFade(demoShowMode == 1 && session != null && sourceInputModule != null &&
                !temporaryHistory && recordedHistoryKey.Contains(".Test.") && pcPlayerVolumeKey.Contains(".Test.") &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal) &&
                Path.GetFileName(PcAutoPlayStatePath).StartsWith("roulette_autoplay_test_", StringComparison.Ordinal) &&
                !pcAutoPlayStorageFaulted && !autoplay && autoRoutine == null && sourceRoundEndCount == 0 &&
                SourcePlayerInputs.Count == 0 && !ideckSecondDisplay && !topperDedicatedDisplay,
                "requires new isolated single-display process with inactive Auto");
            RequirePaidFade(auNationalStandardsEnabled == (paidFadeSmokeCase == "au_zero"), "wrong AU mode");
            RequirePaidFade(PaidOpacity == 1f && !paidFadeClock.Running, "cold constructor must start fully opaque without a timer");
            yield return null;
            sourceInputModule.enabled = false;
            try
            {
                if (coldLoad) yield return PaidFadeColdLoad();
                else
                {
                    RequirePaidFade(session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                        recordedRoundPages.Count == 0 && recordedPockets.Count == 0 && !PlayerPrefs.HasKey(PaidFadeReceiptKey) &&
                        configuredAutoPlay.AutoPlay == 0 && !spinning && !session.SpinPending && pendingSpin == null,
                        "case requires unused fixture keys");
                    yield return StartGameLiveFrames(3);
                    RequirePaidFade(!configuredAutoPlayCheckPending && sourceGameplayState == SourceGameplayState.Idle,
                        "initial actual Idle entry did not finish");
                    if (paidFadeSmokeCase == "clock_ui") PaidFadeClockUi();
                    else if (paidFadeSmokeCase == "au_zero") yield return PaidFadeAuZero();
                    else yield return PaidFadeRealRound(paidFadeSmokeCase != "normal", paidFadeSmokeCase == "cold");
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = false;
            }
            if (paidFadeSmokeCase == "cold" && paidFadeSmokePhase == "save")
                RequirePaidFade(paidFadeClock.Running && PaidOpacity > 0f && PaidOpacity < 1f &&
                    paidFadeClock.CallbackCount < 30, "cold save reached its exit boundary after the fade ended");
            Debug.Log("ROULETTE_PAID_FADE_PASSED: case=" + paidFadeSmokeCase + " phase=" + paidFadeSmokePhase +
                " checks=" + paidFadeSmokeChecks + " credit=" + session.Credit + " rounds=" + session.Rounds +
                " ends=" + sourceRoundEndCount + " alpha=" + PaidOpacity.ToString("R", CultureInfo.InvariantCulture) +
                " callbacks=" + paidFadeClock.CallbackCount + " running=" + paidFadeClock.Running.ToString().ToLowerInvariant());
            Application.Quit();
        }

        // This fingerprint deliberately extends the older paytable UI proof:
        // it includes actual PC RNG bytes, all journals, all eleven Auto
        // fields and their file, sound, active Auto and real End provenance.
        private string PaidFadeProtectedFingerprint(bool includePhase = true)
        {
            return StartGameDurableFingerprint() + "\n" + AutoNvConfig(configuredAutoPlay) + "\n" +
                AutoNvBytes() + "\n" + pcGameSoundsMuted + "|" + autoplay + "|" + (autoRoutine != null) +
                "|" + autoConfirmPending + "|" + sourceRoundEndCount + "|" + (includePhase ? sourceRoundPhase.ToString() : "") +
                "|" + SourcePlayerInputs.Count;
        }

        private void RequirePaidFadeLabel(PictureLabel label, float alpha, bool glyphs, string name)
        {
            RequirePaidFade(label != null && PaidFadeBits(label.Opacity) == PaidFadeBits(alpha), name + " opacity differs");
            CanvasGroup group = label.Root.GetComponent<CanvasGroup>();
            RequirePaidFade(group == null ? alpha == 1f : PaidFadeBits(group.alpha) == PaidFadeBits(alpha),
                name + " CanvasGroup opacity differs");
            int activeGlyphs = 0;
            for (int i = 0; i < label.Root.childCount; ++i)
            {
                Transform child = label.Root.GetChild(i);
                if (!child.gameObject.activeSelf) continue; // Set destroys prior glyphs at frame end.
                RawImage glyph = child.GetComponent<RawImage>();
                if (glyph == null) continue;
                ++activeGlyphs;
                RequirePaidFade(glyph.color == Color.white && PaidFadeBits(glyph.color.a * (group == null ? 1f : group.alpha)) ==
                    PaidFadeBits(alpha), name + " rebuilt glyph changed tint or effective alpha");
            }
            RequirePaidFade(!glyphs || activeGlyphs > 0, name + " has no active numeric glyphs");
        }

        private void RequirePaidFadeMain(float alpha, bool glyphs)
        {
            RequirePaidFade(PaidFadeBits(PaidOpacity) == PaidFadeBits(alpha), "owner opacity differs");
            RequirePaidFadeLabel(uiPaid, alpha, glyphs, "Paid");
            RequirePaidFadeLabel(uiCashPaid, alpha, glyphs, "CashPaid");
            RequirePaidFade(uiCashPaidBackground.color.a == 1f, "cash Paid background was faded");
        }

        private void PaidFadeClockUi()
        {
            Debug.Log("ROULETTE_PAID_FADE_CALL_LEVEL: syntheticClock=true numericLabelsOnly=true financialMutation=false");
            string before = PaidFadeProtectedFingerprint();
            OriginalPaidFadeClock savedFade = paidFadeClock;
            OriginalFooterMovieClock savedMovie = footerMovieClock;
            FooterMoviePhase savedPhase = footerMoviePhase;
            Texture2D[] savedOutro = footerGamePlayOutroFrames;
            bool savedHistory = historyView, paidVisible = uiPaid.Visible, cashVisible = uiCashPaid.Visible;
            string paidValue = uiPaid.Value, cashValue = uiCashPaid.Value;
            const long frequency = 1000000L, epoch = 1000000L;
            try
            {
                paidFadeClock = new OriginalPaidFadeClock();
                ApplyPaidOpacity(1f);
                footerMoviePhase = FooterMoviePhase.Hidden; footerMovieClock = null;
                StopFooterMovieAt(epoch, frequency);
                RequirePaidFade(PaidOpacity == 1f && !paidFadeClock.Running && paidFadeClock.CallbackCount == 0,
                    "hidden Stop started a fade");

                footerGamePlayOutroFrames = new Texture2D[0];
                StartFooterMovieAt(epoch, frequency); StopFooterMovieAt(epoch, frequency);
                RequirePaidFade(footerMoviePhase == FooterMoviePhase.Hidden && PaidOpacity == 1f && !paidFadeClock.Running,
                    "missing Outro started a numeric fade");
                footerGamePlayOutroFrames = savedOutro;
                StartFooterMovieAt(epoch, frequency); StopFooterMovieAt(epoch, frequency);
                uiPaid.Set("36"); uiCashPaid.Set("RM0.36"); uiPaid.SetVisible(true); uiCashPaid.SetVisible(true);
                RequirePaidFade(footerMoviePhase == FooterMoviePhase.Outro && paidFadeClock.Running &&
                    paidFadeClock.CallbackCount == 0 && paidFadeClock.DelayMilliseconds == 33,
                    "first real Stop did not arm the independent 33 ms timer");
                RequirePaidFadeMain(0f, true);
                ServicePaidFadeAt(epoch + 32999L, frequency);
                RequirePaidFadeMain(0f, true);
                RequirePaidFade(paidFadeClock.CallbackCount == 0, "callback arrived before its whole millisecond deadline");
                ServicePaidFadeAt(epoch + 33000L, frequency);
                RequirePaidFade(PaidFadeBits(PaidOpacity) == 0x3d088889 && paidFadeClock.CallbackCount == 1,
                    "first callback did not use the source float step");
                long arm = paidFadeClock.LastArmTimestamp;
                StopFooterMovieAt(epoch + 33000L, frequency);
                RequirePaidFade(paidFadeClock.LastArmTimestamp == arm && paidFadeClock.CallbackCount == 1 &&
                    PaidFadeBits(PaidOpacity) == 0x3d088889, "duplicate visible-Outro Stop restarted fade");
                uiPaid.Set("108"); uiCashPaid.Set("RM1.08");
                RequirePaidFadeMain(PaidOpacity, true);
                uiPaid.Set(""); uiCashPaid.SetVisible(false);
                ServicePaidFadeAt(epoch + 66000L, frequency);
                RequirePaidFade(PaidFadeBits(PaidOpacity) == 0x3d888889 && paidFadeClock.CallbackCount == 2 &&
                    !uiCashPaid.Visible, "hidden or empty numeric label stopped its timer");
                uiPaid.Set("36"); uiCashPaid.Set("RM0.36"); uiCashPaid.SetVisible(true);
                RequirePaidFadeMain(PaidOpacity, true);
                historyView = true; RefreshHistoryPageUi();
                RequirePaidFadeLabel(historyPaid, 1f, true, "independent history Paid");
                RequirePaidFadeLabel(historyCashPaid, 1f, true, "independent history CashPaid");
                RequirePaidFade(historyPaid.Value == "0" && historyCashPaidBackground.color.a == 1f,
                    "history copied main alpha or changed its current zero meter");
                historyView = false; RefreshHistoryPageUi();

                StartFooterMovieAt(epoch + 67000L, frequency);
                RequirePaidFade(footerMoviePhase == FooterMoviePhase.Intro && paidFadeClock.Running &&
                    paidFadeClock.CallbackCount == 2 && paidFadeClock.LastArmTimestamp == epoch + 66000L,
                    "Play cancelled or rearmed an existing Paid timer");
                ServicePaidFadeAt(epoch + 99000L, frequency);
                float afterPlay = PaidOpacity;
                StopFooterMovieAt(epoch + 100000L, frequency);
                RequirePaidFade(PaidOpacity == afterPlay && paidFadeClock.CallbackCount == 3,
                    "new Outro reset an already incomplete fade");
                for (int step = 4; step <= 20; ++step) ServicePaidFadeAt(epoch + step * 33000L, frequency);
                RefreshFooterMovieUiAt(epoch + 660000L, frequency);
                RequirePaidFade(footerMoviePhase == FooterMoviePhase.Hidden && paidFadeClock.Running &&
                    paidFadeClock.CallbackCount == 20 && PaidOpacity > .6f && PaidOpacity < .7f,
                    "fade stopped when the independent Outro became Hidden");
                RequirePaidFadeMain(PaidOpacity, true);
                for (int step = 21; step <= 30; ++step) ServicePaidFadeAt(epoch + step * 33000L, frequency);
                RequirePaidFade(PaidOpacity == 1f && !paidFadeClock.Running && paidFadeClock.CallbackCount == 30 &&
                    paidFadeClock.DelayMilliseconds == 0, "30 callbacks did not clamp to one and cancel");
                RequirePaidFadeMain(1f, true);
                StopFooterMovieAt(epoch + 1000000L, frequency);
                RequirePaidFade(PaidOpacity == 1f && !paidFadeClock.Running, "Hidden Stop restarted a completed fade");

                StartFooterMovieAt(epoch + 1010000L, frequency); StopFooterMovieAt(epoch + 1010000L, frequency);
                ServicePaidFadeAt(epoch + 1060000L, frequency);
                RequirePaidFade(paidFadeClock.CallbackCount == 1 && PaidFadeBits(PaidOpacity) == 0x3d088889 &&
                    paidFadeClock.DelayMilliseconds == 16, "17 ms lateness did not rearm once for 16 ms");
                ServicePaidFadeAt(epoch + 1060000L, frequency);
                RequirePaidFade(paidFadeClock.CallbackCount == 1, "same service time dispatched a duplicate callback");
                ServicePaidFadeAt(epoch + 1076000L, frequency);
                RequirePaidFade(paidFadeClock.CallbackCount == 2 && paidFadeClock.DelayMilliseconds == 33,
                    "on-time compensated callback did not return to 33 ms");
                ServicePaidFadeAt(epoch + 6060000L, frequency);
                RequirePaidFade(paidFadeClock.CallbackCount == 3 && paidFadeClock.DelayMilliseconds == 1,
                    "late service caught up multiple steps instead of rearming once for 1 ms");
                ServicePaidFadeAt(epoch + 6060999L, frequency);
                RequirePaidFade(paidFadeClock.CallbackCount == 3, "1 ms recovery callback arrived early");
                ServicePaidFadeAt(epoch + 6061000L, frequency);
                RequirePaidFade(paidFadeClock.CallbackCount == 4 && paidFadeClock.DelayMilliseconds == 33,
                    "1 ms recovery did not return to the repeating interval");
                RequirePaidFadeMain(PaidOpacity, true);
                RequirePaidFade(PaidFadeProtectedFingerprint() == before, "synchronous UI/clock contract changed durable state");
            }
            finally
            {
                paidFadeClock = savedFade; footerMovieClock = savedMovie; footerMoviePhase = savedPhase;
                footerGamePlayOutroFrames = savedOutro; historyView = savedHistory;
                uiPaid.Set(paidValue); uiCashPaid.Set(cashValue); uiPaid.SetVisible(paidVisible); uiCashPaid.SetVisible(cashVisible);
                ApplyPaidOpacity(savedFade.Alpha); RefreshPortraitUi();
            }
            RequirePaidFade(PaidFadeProtectedFingerprint() == before && PaidOpacity == 1f,
                "UI/clock fixture did not restore its owner or protected state");
        }

        private IEnumerator PaidFadeAuZero()
        {
            RefreshPortraitUi();
            string before = PaidFadeProtectedFingerprint();
            StartFooterMovie(); StopFooterMovie();
            RequirePaidFade(footerMoviePhase == FooterMoviePhase.Hidden && !paidFadeClock.Running &&
                PaidOpacity == 1f && uiPaid.Value == "0" && uiPaid.Visible,
                "AU initial zero or Play-without-movie changed fade state");
            RequirePaidFadeMain(1f, true);
            sourceInputModule.enabled = true;
            for (int i = 0; i < 4; ++i)
            {
                yield return null;
                RequirePaidFade(PaidFadeProtectedFingerprint() == before && PaidOpacity == 1f && !paidFadeClock.Running,
                    "AU zero live frames changed money, Auto, RNG, End or opacity");
            }
            sourceInputModule.enabled = false;
            CapturePaidFade("zero");
        }

        private IEnumerator PaidFadeRealRound(bool turbo, bool saveMid)
        {
            RefreshPortraitUi(); DemoCleanupClick(uiBetButtons[27]);
            RequirePaidFade(session.TotalBet == 1 && sourceBetStateEntryPending, "real wager did not enter StateBet");
            yield return StartGameWaitForBetEntry();
            if (turbo) yield return StartGameChooseDemo(2);
            yield return StartGameChooseDemo(30); // Real PC RNG still advances; only the returned pocket is fixed.
            if (turbo) configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            string rng = DemoCleanupRngBytes(), undo = StateBetCleanupUndoFingerprint();
            RefreshPortraitUi(); DemoCleanupClick(uiSpinButton);
            RequirePaidFade(spinning && session.SpinPending && session.PendingPocket == 27 && session.Credit == 999 &&
                session.Rounds == 0 && pendingSpin != null && DemoCleanupRngBytes() != rng && SourcePlayerInputs.Count == 0,
                "real PLAY did not commit exactly one funded PC draw");
            string committedRng = DemoCleanupRngBytes();
            sourceInputModule.enabled = true;
            float deadline = Time.realtimeSinceStartup + 30f;
            while (session.Rounds == 0 && Time.realtimeSinceStartup < deadline) yield return null;
            RequirePaidFade(session.Rounds == 1 && session.Credit == (PcPaymentTracked ? 999 : 1035) &&
                session.Wagered == 1 && session.Returned == 36 &&
                session.Wins == 1 && session.LastResult.Pocket == 27 && !session.SpinPending && pendingSpin == null &&
                DemoCleanupRngBytes() == committedRng && StateBetCleanupUndoFingerprint() == undo &&
                paidFadeClock.Running && PaidOpacity < .8f,
                "actual positive result did not begin one live numeric fade");
            if (PcPaymentTracked)
                RequirePaidFade(!pcGamePayment.prepared && !pcGamePayment.moneyApplied &&
                    !pcGamePayment.historyPublished && pcGamePayment.baseAward == 36 &&
                    ((RouletteSession)session).RoundPayoutPending && CommittedHistoryCount() == 0,
                    "positive result fade applied money or published history before StatePayout");
            if (turbo)
            {
                yield return StateEndWaitForIdle(false);
                RequirePaidFade(sourceRoundEndCount == 1 && configuredAutoPlay.AutoPlay == 0 &&
                    !configuredAutoPlay.Turbo && !configuredAutoPlay.AutoPlayOffWhenGameFinished && !pcGameSoundsMuted &&
                    paidFadeClock.Running && PaidOpacity < .8f,
                    "Turbo did not consume its one End before the independent fade finished");
            }
            else RequirePaidFade(winRackActive && sourceRoundEndCount == 0 && sourceBaseHasDoubleOffer &&
                session.DoubleAvailable, "manual payout skipped its real rack or expected Double offer");
            sourceInputModule.enabled = true;
            // A slow renderer may let the normal rack finish while this
            // independent timer is still running. The real offer may advance
            // BaseRollup -> DoubleOffer, but cannot pay, draw or End until our
            // later real PLAY declines it. Turbo has already completed End.
            string protectedState = PaidFadeProtectedFingerprint(turbo);
            SourceRoundPhase priorPhase = sourceRoundPhase;
            bool captured = false, hiddenWhileRunning = false;
            string lastPaid = "";
            int valueChanges = 0;
            deadline = Time.realtimeSinceStartup + 30f;
            do
            {
                RefreshPortraitUi();
                RequirePaidFade(PaidFadeProtectedFingerprint(turbo) == protectedState,
                    "live fade altered protected financial, Auto, RNG or End state");
                if (!turbo)
                {
                    RequirePaidFade(sourceRoundEndCount == 0 && SourceDoubleOfferActive &&
                        (sourceRoundPhase == priorPhase || priorPhase == SourceRoundPhase.BaseRollup &&
                            sourceRoundPhase == SourceRoundPhase.DoubleOffer),
                        "normal fade regressed its offer phase or completed an unrequested End");
                    priorPhase = sourceRoundPhase;
                }
                RequirePaidFadeMain(PaidOpacity, true);
                RequirePaidFade(uiPaid.Value == DisplayedPaid.ToString() && uiCashPaid.Value ==
                    RouletteDenominationFormatter.ForFeatureCurrency(DisplayedPaid * session.Denomination),
                    "live meter rebuild diverged from the actual displayed paid amount");
                if (lastPaid != uiPaid.Value) { ++valueChanges; lastPaid = uiPaid.Value; }
                if (footerMoviePhase == FooterMoviePhase.Hidden && paidFadeClock.Running) hiddenWhileRunning = true;
                if (!captured && PaidOpacity >= .4f && PaidOpacity <= .7f && DisplayedPaid > 0)
                {
                    if (!saveMid) CapturePaidFade("mid");
                    captured = true;
                    if (saveMid)
                    {
                        RequirePaidFade(turbo && sourceRoundEndCount == 1 && PaidOpacity < 1f,
                            "cold receipt is not an actual completed round with an unfinished fade");
                        SavePaidFadeReceipt();
                        sourceInputModule.enabled = false;
                        yield break;
                    }
                }
                if (!paidFadeClock.Running) break;
                yield return null;
            }
            while (Time.realtimeSinceStartup < deadline);
            sourceInputModule.enabled = false;
            RequirePaidFade(captured && hiddenWhileRunning && PaidOpacity == 1f && !paidFadeClock.Running &&
                paidFadeClock.CallbackCount == 30 && footerMoviePhase == FooterMoviePhase.Hidden &&
                !uiFooterMovie.gameObject.activeSelf && (turbo || valueChanges >= 2),
                "live fade missed its middle, independent post-Outro ticks, numeric rebuilds or 30-step completion");
            if (!turbo) yield return StateEndWaitForIdle(true);
            RequirePaidFade(sourceRoundEndCount == 1 && session.Rounds == 1 && session.Credit == 1035 &&
                session.Wagered == 1 && session.Returned == 36 && session.TotalBet == 1 &&
                DemoCleanupRngBytes() == committedRng && StateBetCleanupUndoFingerprint() == undo &&
                ReadPcCreditEvent() == null && !autoplay && autoRoutine == null,
                "final live round repeated payout/RNG/End or made a deposit");
            string stable = PaidFadeProtectedFingerprint();
            yield return StartGameLiveFrames(4);
            RequirePaidFade(PaidFadeProtectedFingerprint() == stable && PaidOpacity == 1f && !paidFadeClock.Running,
                "later actual frames changed the completed result");
            CapturePaidFade("end");
        }

        private void SavePaidFadeReceipt()
        {
            var state = new StartGameStateBetReceipt {
                testCase = "paid_fade", phase = "settled", ordinary = false,
                economy = StartGameRestoredEconomy(), archive = DemoCleanupArchive(false),
                rng = DemoCleanupRngBytes(), undo = StateBetCleanupUndoFingerprint(),
                pending = PlayerPrefs.GetString(PendingSpinKey, ""), staged = PlayerPrefs.GetString(StagedBetKey, ""),
                cleared = PlayerPrefs.GetString(GameResultsClearedKey, ""), settledUndo = PlayerPrefs.GetString(SettledTableUndoKey, ""),
                creditEvent = StartGameCreditEventFingerprint(), sourceState = (int)sourceGameplayState,
                neighbor = selectedNeighbor, neighborNv = PlayerPrefs.GetInt(NeighborSelectionKey, -2),
                credit = session.Credit, wagered = session.Wagered, returned = session.Returned, rounds = session.Rounds
            };
            var proof = new PaidFadeReceipt { state = state, config = AutoNvConfig(configuredAutoPlay), autoBytes = AutoNvBytes(),
                muted = pcGameSoundsMuted, alphaBits = PaidFadeBits(PaidOpacity), callbacks = paidFadeClock.CallbackCount };
            RequirePaidFade(state.pending == "" && sourceGameplayState == SourceGameplayState.Idle &&
                proof.callbacks >= 12 && proof.callbacks <= 21 && paidFadeClock.Running,
                "save receipt did not capture the real intermediate fade after End");
            PlayerPrefs.SetString(PaidFadeReceiptKey, JsonUtility.ToJson(proof)); PlayerPrefs.Save();
            Debug.Log("ROULETTE_PAID_FADE_RECEIPT: savedMid=true alphaBits=" + proof.alphaBits.ToString("X8") +
                " callbacks=" + proof.callbacks + " rounds=" + state.rounds + " ends=" + sourceRoundEndCount);
        }

        private IEnumerator PaidFadeColdLoad()
        {
            var proof = JsonUtility.FromJson<PaidFadeReceipt>(PlayerPrefs.GetString(PaidFadeReceiptKey, ""));
            RequirePaidFade(proof != null && proof.state != null && proof.state.testCase == "paid_fade" &&
                proof.state.phase == "settled" && proof.callbacks >= 12 && proof.callbacks <= 21 &&
                proof.alphaBits != PaidFadeBits(1f), "missing an intermediate-fade receipt");
            RequireStartGameReceipt(proof.state);
            RequirePaidFade(AutoNvConfig(configuredAutoPlay) == proof.config && AutoNvBytes() == proof.autoBytes &&
                pcGameSoundsMuted == proof.muted && session.Credit == 1035 && session.Rounds == 1 &&
                sourceRoundEndCount == 0 && sourceRoundPhase == SourceRoundPhase.None &&
                !spinning && !session.SpinPending && pendingSpin == null && PaidOpacity == 1f && !paidFadeClock.Running,
                "cold restore resumed a presentation timer, End, draw or different configuration");
            string before = PaidFadeProtectedFingerprint();
            sourceInputModule.enabled = true;
            for (int i = 0; i < 4; ++i)
            {
                yield return null;
                RequirePaidFade(PaidFadeProtectedFingerprint() == before && PaidOpacity == 1f && !paidFadeClock.Running,
                    "cold restored live frames changed protected state or restarted fade");
            }
            sourceInputModule.enabled = false;
            RefreshPortraitUi(); RequirePaidFadeMain(1f, true);
            RequirePaidFade(uiPaid.Value == "36" && !uiFooterMovie.gameObject.activeSelf, "cold stable result is obscured");
            CapturePaidFade("reload");
        }

        private void CapturePaidFade(string shot)
        {
            RefreshPortraitUi();
            string path = PaidFadeArgument("-roulettePaidFadeCapturePrefix") + "_" + shot + ".png";
            RequirePaidFade(!File.Exists(path), "refusing to overwrite a capture");
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            RequirePaidFade(uiPaid.Visible && !string.IsNullOrEmpty(uiPaid.Value) &&
                PaidOpacity > 0f && (shot != "mid" || PaidOpacity >= .4f && PaidOpacity <= .7f),
                "capture has no visible numeric amount or missed its actual middle alpha");
            float alpha = PaidOpacity;
            int callbacks = paidFadeClock.CallbackCount, ends = sourceRoundEndCount;
            string paid = uiPaid.Value, cash = uiCashPaid.Value;
            CaptureActiveScreenOffscreen(path);
            Debug.Log("ROULETTE_PAID_FADE_CAPTURE: case=" + paidFadeSmokeCase + " phase=" + paidFadeSmokePhase +
                " shot=" + shot + " alpha=" + alpha.ToString("R", CultureInfo.InvariantCulture) +
                " alphaBits=" + PaidFadeBits(alpha).ToString("X8") + " callbacks=" + callbacks +
                " paid=" + paid + " cash=" + cash + " ends=" + ends);
        }
    }
}
