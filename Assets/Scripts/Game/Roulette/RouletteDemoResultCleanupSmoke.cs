using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private sealed class DemoCleanupOutcome : IRouletteOutcomeSource
        {
            public int Pockets, Suits;
            public int DrawPocket() { ++Pockets; return 16; }
            public int DrawDoubleSuit()
            {
                ++Suits;
                throw new InvalidOperationException("Demo cleanup reached feature RNG");
            }
        }

        [Serializable]
        private sealed class DemoCleanupReceipt
        {
            public int option;
            public string archiveId, economy, archive, rng, cleared;
        }

        private string DemoCleanupReceiptKey
        {
            get { return recordedHistoryKey + ".DemoResultCleanupFixture.v1"; }
        }

        private static void RequireDemoCleanup(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("Demo result cleanup: " + message);
        }

        // One actual manual win per process. Options 0/1/2/30 cover each
        // semantic branch after live result presentation; the existing Down
        // diagnostic separately covers dispatch of all forty option indices.
        private IEnumerator DemoResultCleanupSmoke(bool load)
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteDemoResultCleanupOption");
            int option = 0;
            RequireDemoCleanup(at < 0 || at + 1 < args.Length &&
                int.TryParse(args[at + 1], out option), "missing numeric option");
            RequireDemoCleanup(option == 0 || option == 1 || option == 2 || option == 30,
                "choose option 0, 1, 2 or 30");
            RequireDemoCleanup(demoShowMode == 1 && sourceInputModule != null &&
                demoMenuRoot != null && demoOutcome != null && session != null &&
                !temporaryHistory && recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                !spinning && !session.SpinPending && !session.InDoubleUp &&
                !autoplay && autoRoutine == null && SourcePlayerInputs.Count == 0,
                "requires isolated persistent history/volume/RNG and idle demo mode");

            bool moduleEnabled = sourceInputModule.enabled;
            int checks = 0;
            try
            {
                sourceInputModule.enabled = false;
                if (load)
                {
                    DemoCleanupReceipt proof = JsonUtility.FromJson<DemoCleanupReceipt>(
                        PlayerPrefs.GetString(DemoCleanupReceiptKey, ""));
                    RequireDemoCleanup(proof != null && proof.option == option &&
                        recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                        recordedRoundPages[0].roundArchiveId == proof.archiveId &&
                        DemoCleanupEconomy() == proof.economy &&
                        DemoCleanupArchive(false) == proof.archive &&
                        DemoCleanupRngBytes() == proof.rng &&
                        PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                        !string.IsNullOrEmpty(proof.cleared) && pendingSpin == null,
                        "reload altered funds/bets/history, rerolled RNG or lost the clear record");
                    ++checks;
                    RequireDemoCleanupClearedView(false, true);
                    Vector2 ball = uiBall.rectTransform.anchoredPosition;
                    yield return DemoCleanupReopen();
                    RequireDemoCleanupClearedView(false, true);
                    RequireDemoCleanup(uiBall.rectTransform.anchoredPosition == ball &&
                        DemoCleanupEconomy() == proof.economy &&
                        DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng,
                        "reload menu reopen restored a win or changed the settled round");
                    ++checks;
                    CaptureDemoCleanup("reload");
                    if (Array.IndexOf(args, "-rouletteDemoResultCleanupNextRound") >= 0)
                    {
                        yield return DemoCleanupSelect(30);
                        RequireDemoCleanup(demoOutcome.SelectedPocket == 27,
                            "next-round fixed pocket was not selected");
                        yield return DemoCleanupStartAndSettle();
                        RequireDemoCleanup(!currentGameResultsCleared && session.Credit == 1070 &&
                            session.Rounds == 2 && recordedRoundPages.Count == 2 &&
                            session.LastResult.Pocket == 27 && session.LastResult.Returned == 36 &&
                            session.DoubleEnabled && !session.DoubleAvailable && session.DoubleStake == 0 &&
                            !recordedRoundPages[0].awaitingFinalPayout &&
                            recordedRoundPages[1].roundArchiveId == proof.archiveId &&
                            recordedRoundPages[1].returned == 36 &&
                            JsonUtility.ToJson(recordedRoundPages[1]) == JsonUtility.ToJson(
                                JsonUtility.FromJson<RecordedRoundArchive>(proof.archive).rounds[0]) &&
                            DemoCleanupRngBytes() != proof.rng,
                            "next actual round inherited the cleared view or changed the prior archive");
                        yield return DemoCleanupWaitForVisibleWin();
                        CaptureDemoCleanup("next_win");
                        ++checks;
                    }
                    Debug.Log("ROULETTE_DEMO_RESULT_CLEANUP_LOAD_PASSED: option=" + option +
                        " checks=" + checks + " credit=" + session.Credit + " rounds=" + session.Rounds +
                        " historyPreserved=true reloadNoReroll=true nextRound=" +
                        (Array.IndexOf(args, "-rouletteDemoResultCleanupNextRound") >= 0));
                }
                else
                {
                    RequireDemoCleanup(session.Credit == 1000 && session.TotalBet == 0 &&
                        session.Rounds == 0 && recordedRoundPages.Count == 0 &&
                        recordedPockets.Count == 0 && ReadPcCreditEvent() == null &&
                        !PlayerPrefs.HasKey(DemoCleanupReceiptKey) &&
                        configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo,
                        "save fixture requires unused keys and default configuration");
                    var counted = new DemoCleanupOutcome();
                    demoOutcome = new DemoPocketOutcomeSource(counted);
                    outcome = demoOutcome;
                    session = new RouletteSession(session.Profile, 1000, demoOutcome) {
                        DoubleEnabled = true };
                    SetPcGameSoundsMuted(false);
                    yield return DemoCleanupSelect(30); // actual fixed-27 menu selection
                    session.PlaceBet(127, 1);
                    SaveStagedBetsIfChanged();
                    yield return DemoCleanupStartAndSettle();
                    RequireDemoCleanup(session.Credit == 1035 && session.TotalBet == 1 &&
                        session.Rounds == 1 && session.Wagered == 1 && session.Returned == 36 &&
                        session.LastResult.Pocket == 27 && session.LastResult.Returned == 36 &&
                        recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                        counted.Pockets == 1 && counted.Suits == 0 &&
                        !currentGameResultsCleared && pendingSpin == null &&
                        session.DoubleEnabled && !session.DoubleAvailable && session.DoubleStake == 0 &&
                        !recordedRoundPages[0].awaitingFinalPayout,
                        "actual fixed-27 manual round did not settle exactly once");
                    ++checks;
                    yield return DemoCleanupWaitForVisibleWin();
                    CaptureDemoCleanup("before");
                    string economy = DemoCleanupEconomy();
                    string archive = DemoCleanupArchive(true);
                    string rng = DemoCleanupRngBytes();
                    long cycle = resultCycleStartedTimestamp;
                    Vector2 ball = uiBall.rectTransform.anchoredPosition;
                    RequireDemoCleanup(uiTableNumberMark.gameObject.activeSelf && uiBall.gameObject.activeSelf,
                        "winning table marker/ball were absent before cleanup");

                    // Page navigation and outside dismissal do not invoke
                    // DemoOptionSelected/ClearGameResults in the original.
                    ToggleDemoMenu();
                    yield return null;
                    DemoCleanupClick(demoNextButton);
                    yield return null;
                    DemoCleanupClick(demoPreviousButton);
                    yield return null;
                    DemoCleanupClick(DemoCleanupDismissButton(), new Vector2(1065f, 100f));
                    RequireDemoCleanup(!demoMenuVisible && !currentGameResultsCleared &&
                        resultCycleStartedTimestamp == cycle && DisplayedPaid == 36 &&
                        DemoCleanupEconomy() == economy && DemoCleanupArchive(true) == archive &&
                        counted.Pockets == 1 && counted.Suits == 0,
                        "navigation/dismissal cleared results or mutated the round");
                    ++checks;

                    ToggleDemoMenu();
                    yield return DemoCleanupNavigate(option / DemoOptionsPerPage);
                    OriginalAutoPlayData beforeConfig = configuredAutoPlay.Clone();
                    bool beforeMuted = pcGameSoundsMuted;
                    PointerEventData pointer = DemoCleanupQueue(demoOptionButtons[option % DemoOptionsPerPage]);
                    RequireDemoCleanup(SourcePlayerInputs.DispatchOne() && !demoMenuVisible &&
                        currentGameResultsCleared, "choice did not clear current results on down");
                    RequireDemoCleanupClearedView();
                    RequireDemoCleanup(DemoCleanupEconomy() == economy && DemoCleanupArchive(true) == archive &&
                        uiBall.rectTransform.anchoredPosition == ball && counted.Pockets == 1 &&
                        counted.Suits == 0 && DemoCleanupRngBytes() == rng && !spinning && !autoplay,
                        "result cleanup changed money/bets/history/ball or requested another outcome");
                    if (option == 0 || option == 30)
                        RequireDemoCleanup(SameConfiguredSmokeData(configuredAutoPlay, beforeConfig) &&
                            pcGameSoundsMuted == beforeMuted, "Normal/fixed choice changed configuration or sound");
                    else
                        RequireDemoCleanup(configuredAutoPlay.AutoPlay == 1 &&
                            configuredAutoPlay.ReplenishCredits && configuredAutoPlay.PlayerInput &&
                            configuredAutoPlay.DemoOnly && configuredAutoPlay.Turbo == (option == 2) &&
                            pcGameSoundsMuted == (option == 2), "Auto/Turbo option configuration differs");
                    ++checks;
                    DemoCleanupDrainRelease(pointer);
                    RequireDemoCleanupClearedView();
                    yield return DemoCleanupReopen();
                    // Repeat real menu actions after cleanup. Mathematical
                    // result selection and the separate one-shot wheel path
                    // are not interchangeable; source-specific path checks
                    // belong to the wheel fixture.
                    yield return DemoCleanupSelect(30);
                    if (option != 30) yield return DemoCleanupSelect(option);
                    yield return new WaitForSecondsRealtime(1.6f);
                    RequireDemoCleanupClearedView();
                    RequireDemoCleanup(DemoCleanupEconomy() == economy && DemoCleanupArchive(true) == archive &&
                        counted.Pockets == 1 && counted.Suits == 0 && DemoCleanupRngBytes() == rng &&
                        !spinning && !configuredAutoPlayCheckPending && !configuredAutoPlayFinishPending,
                        "release/reopen/cycle timeout revived results or scheduled an extra round");
                    ++checks;
                    string clearRecord = PlayerPrefs.GetString(GameResultsClearedKey, "");
                    RequireDemoCleanup(!string.IsNullOrEmpty(clearRecord), "current-result clear was not persisted");
                    var proof = new DemoCleanupReceipt {
                        option = option, archiveId = recordedRoundPages[0].roundArchiveId,
                        economy = DemoCleanupEconomy(), archive = DemoCleanupArchive(false),
                        rng = rng, cleared = clearRecord
                    };
                    PlayerPrefs.SetString(DemoCleanupReceiptKey, JsonUtility.ToJson(proof));
                    PlayerPrefs.Save();
                    CaptureDemoCleanup("after");
                    checks += DemoCleanupJournalBranches();
                    RequireDemoCleanupClearedView();
                    RestartSourcePrompt(PromptKind.PressSpin);
                    RefreshPortraitUi();
                    RequireDemoCleanup(!sourcePromptStopped && promptRoot.gameObject.activeSelf &&
                        shownPrompt == PromptKind.PressSpin && DemoCleanupEconomy() == proof.economy &&
                        DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                        PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared,
                        "explicit source prompt restart did not resume without changing the settled round");
                    ++checks;
                    Debug.Log("ROULETTE_DEMO_RESULT_CLEANUP_PASSED: option=" + option +
                        " checks=" + checks + " credit=1035 wager=1 returned=36 rounds=1 draws=1 featureDraws=0 " +
                        "downCleanup=true navigationPreserved=true historyPreserved=true repeatedChoices=true " +
                        "journalBranches=3 explicitPromptRestart=true au=" + auNationalStandardsEnabled);
                }
            }
            finally
            {
                CloseDemoMenu();
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = moduleEnabled;
            }
            Application.Quit();
        }

        private IEnumerator DemoCleanupStartAndSettle()
        {
            RefreshPortraitUi();
            yield return null;
            DemoCleanupClick(uiSpinButton);
            RequireDemoCleanup(spinning && session.SpinPending, "actual PLAY did not begin a round");
            yield return StartGameWaitForActualSettlement();
            RequireDemoCleanup(!configuredAutoPlayCheckPending && !configuredAutoPlayFinishPending &&
                !spinning, "settled default-config state entry did not finish");
            RefreshPortraitUi();
        }

        private IEnumerator DemoCleanupWaitForVisibleWin()
        {
            float deadline = Time.realtimeSinceStartup + 4f;
            while (VisibleWinningPayId() < 0 && Time.realtimeSinceStartup < deadline) yield return null;
            RefreshPortraitUi();
            RequireDemoCleanup(VisibleWinningPayId() == 127 && ActiveWinningHighlights() == 1 &&
                ActiveWinningPayoutGlyphs() == 2 && uiTotalWinTitle.gameObject.activeSelf &&
                uiTotalWinAmount.Value == "(36)" && uiLinePays.Value == "Straight Up Pays 36" &&
                uiGamePays.Value == "Game Pays 36" && DisplayedPaid == 36 && uiPaid.Value == "36" &&
                uiCashPaid.Visible, "live winning display was not present before selection");
        }

        private void RequireDemoCleanupClearedView(bool expectPromptStopped = true,
            bool expectRestoredMarker = false)
        {
            RefreshPortraitUi();
            bool showZeroPaid = auNationalStandardsEnabled;
            RequireDemoCleanup(currentGameResultsCleared && !winRackActive &&
                resultCycleStartedTimestamp == 0 && VisibleWinningPayId() == -1 &&
                ActiveWinningHighlights() == 0 && ActiveWinningPayoutGlyphs() == 0 &&
                !uiTotalWinTitle.gameObject.activeSelf && uiTotalWinAmount.Value == "" &&
                uiLinePays.Value == "" && uiGamePays.Value == "" && DisplayedPaid == 0 &&
                uiPaid.Value == (showZeroPaid ? "0" : "") && uiCashPaid.Visible == showZeroPaid &&
                uiCashPaidBackground.gameObject.activeSelf == showZeroPaid &&
                uiCashPaid.Value == (showZeroPaid ? RouletteDenominationFormatter.ForFeatureCurrency(0) : "") &&
                uiTableNumberMark.gameObject.activeSelf == expectRestoredMarker &&
                (!expectRestoredMarker || tableMarkedPocket == 27) && uiBall.gameObject.activeSelf &&
                sourcePromptStopped == expectPromptStopped &&
                (!expectPromptStopped || !promptRoot.gameObject.activeSelf) &&
                session.DoubleEnabled && !session.DoubleAvailable && session.DoubleStake == 0 &&
                !session.InDoubleUp && !DoubleUpInputAllowed &&
                recordedRoundPages.Count == 1 && !recordedRoundPages[0].awaitingFinalPayout,
                "cleared cycle/labels/marker, zero paid meters, retained ball or declined feature differ");
            RequireDemoCleanup(auNationalStandardsEnabled ? CurrentFooterMovieFrame() == null :
                CurrentFooterMovieFrame() != null && (footerMoviePhase == FooterMoviePhase.Intro ||
                footerMoviePhase == FooterMoviePhase.Loop), "post-win clear violated the AU/non-AU footer gate");
        }

        private int DemoCleanupJournalBranches()
        {
            RequireDemoCleanup(recordedRoundPages.Count == 1 && !temporaryHistory &&
                currentGameResultsCleared && !session.DoubleAvailable &&
                !recordedRoundPages[0].awaitingFinalPayout,
                "journal branches need the already-cleared real round");
            RecordedRound original = recordedRoundPages[0];
            string archiveKey = recordedHistoryKey + ".RoundDetails.v1";
            string originalArchive = PlayerPrefs.GetString(archiveKey, "");
            string originalClear = PlayerPrefs.GetString(GameResultsClearedKey, "");
            string economy = DemoCleanupEconomy();
            string rng = DemoCleanupRngBytes();
            int savedVersion = recordedHistoryVersion;
            bool savedTemporary = temporaryHistory, savedCleared = currentGameResultsCleared;
            bool savedMarker = currentTableResultMarkerCleared, savedStopped = sourcePromptStopped;
            float savedCycleAt = resultCycleStartedAt;
            long savedCycleTimestamp = resultCycleStartedTimestamp;
            FooterMoviePhase savedFooterPhase = footerMoviePhase;
            OriginalFooterMovieClock savedFooterClock = footerMovieClock;
            PromptKind savedFailure = failurePrompt, savedAfterFailure = promptStartAfterFailure;
            PromptKind savedStart = promptStartNow, savedShown = shownPrompt;
            PromptPlaybackPhase savedPromptPhase = promptPhase;
            try
            {
                var stale = JsonUtility.FromJson<ClearedGameResultsRecord>(originalClear);
                RequireDemoCleanup(stale != null && !string.IsNullOrEmpty(stale.roundArchiveId),
                    "modern fixture did not save its round anchor");
                stale.roundArchiveId += "_different_round";
                string staleJson = JsonUtility.ToJson(stale);
                PlayerPrefs.SetString(GameResultsClearedKey, staleJson);
                RestoreClearedGameResults();
                RequireDemoCleanup(!currentGameResultsCleared &&
                    PlayerPrefs.GetString(GameResultsClearedKey, "") == staleJson,
                    "stale modern clear record matched or rewrote another round");

                // A real legacy record can lack both archive identity and
                // lifetime totals while retaining a pending-offer bit. The
                // settled session already declined that offer. Finalization
                // must precede the legacy digest without inventing an ID.
                RecordedRound legacy = JsonUtility.FromJson<RecordedRound>(JsonUtility.ToJson(original));
                legacy.roundArchiveId = "";
                legacy.totalsRecorded = false;
                legacy.totalRounds = 0;
                legacy.awaitingFinalPayout = true;
                recordedRoundPages[0] = legacy;
                ApplyClearedGameResultsPresentation(false);
                RequireDemoCleanup(!legacy.awaitingFinalPayout && legacy.roundArchiveId == "" &&
                    !legacy.totalsRecorded && legacy.totalRounds == 0,
                    "legacy offer finalization required missing lifetime totals or minted an ID");
                SaveClearedGameResults();
                string legacyJson = PlayerPrefs.GetString(GameResultsClearedKey, "");
                var legacyRecord = JsonUtility.FromJson<ClearedGameResultsRecord>(legacyJson);
                RequireDemoCleanup(legacyRecord != null && legacyRecord.roundArchiveId == "" &&
                    !string.IsNullOrEmpty(legacyRecord.legacyDigest), "legacy clear record lacks its digest");
                RestoreClearedGameResults();
                RequireDemoCleanup(currentGameResultsCleared && !legacy.awaitingFinalPayout &&
                    legacy.roundArchiveId == "" && PlayerPrefs.GetString(GameResultsClearedKey, "") == legacyJson,
                    "valid finalized legacy presentation did not restore unchanged");

                recordedRoundPages[0] = original;
                PlayerPrefs.SetString(GameResultsClearedKey, originalClear);
                temporaryHistory = true;
                RestoreClearedGameResults();
                RequireDemoCleanup(!currentGameResultsCleared,
                    "temporary history imported a persistent clear record");
                currentGameResultsCleared = true;
                SaveClearedGameResults();
                RequireDemoCleanup(PlayerPrefs.GetString(GameResultsClearedKey, "") == originalClear,
                    "temporary history overwrote a persistent clear record");
            }
            finally
            {
                recordedRoundPages[0] = original;
                recordedHistoryVersion = savedVersion;
                temporaryHistory = savedTemporary;
                currentGameResultsCleared = savedCleared;
                currentTableResultMarkerCleared = savedMarker;
                sourcePromptStopped = savedStopped;
                resultCycleStartedAt = savedCycleAt;
                resultCycleStartedTimestamp = savedCycleTimestamp;
                footerMoviePhase = savedFooterPhase;
                footerMovieClock = savedFooterClock;
                failurePrompt = savedFailure;
                promptStartAfterFailure = savedAfterFailure;
                promptStartNow = savedStart;
                shownPrompt = savedShown;
                promptPhase = savedPromptPhase;
                PlayerPrefs.SetString(archiveKey, originalArchive);
                PlayerPrefs.SetString(GameResultsClearedKey, originalClear);
                PlayerPrefs.Save();
            }
            RequireDemoCleanup(DemoCleanupEconomy() == economy && DemoCleanupRngBytes() == rng &&
                PlayerPrefs.GetString(archiveKey, "") == originalArchive &&
                PlayerPrefs.GetString(GameResultsClearedKey, "") == originalClear &&
                ReferenceEquals(recordedRoundPages[0], original),
                "journal boundary fixtures changed the real round or retained test metadata");
            return 3;
        }

        private IEnumerator DemoCleanupSelect(int option)
        {
            RequireDemoCleanup(!demoMenuVisible, "selection helper entered an open menu");
            ToggleDemoMenu();
            yield return DemoCleanupNavigate(option / DemoOptionsPerPage);
            DemoCleanupClick(demoOptionButtons[option % DemoOptionsPerPage]);
            RequireDemoCleanup(!demoMenuVisible, "queued choice left menu open");
        }

        private IEnumerator DemoCleanupNavigate(int page)
        {
            yield return null;
            RequireDemoCleanup(demoMenuVisible && demoMenuPage == 0, "menu did not open at page zero");
            for (int i = 0; i < page; ++i)
            {
                DemoCleanupClick(demoNextButton);
                yield return null;
                RequireDemoCleanup(demoMenuPage == i + 1, "queued navigation skipped its page");
            }
        }

        private IEnumerator DemoCleanupReopen()
        {
            ToggleDemoMenu();
            yield return null;
            RequireDemoCleanup(demoMenuVisible, "cleared result prevented menu reopening");
            DemoCleanupClick(DemoCleanupDismissButton(), new Vector2(1065f, 100f));
            RequireDemoCleanup(!demoMenuVisible, "outside release did not close menu");
        }

        private Button DemoCleanupDismissButton()
        {
            return demoMenuRoot.transform.Find("Demo menu outside dismiss").GetComponent<Button>();
        }

        private PointerEventData DemoCleanupQueue(Button target, Vector2? portraitPoint = null)
        {
            RequireDemoCleanup(SourcePlayerInputs.Count == 0 && target != null && target.IsActive(),
                "pointer fixture needs an empty queue and active button");
            Canvas.ForceUpdateCanvases();
            // Dismiss at a blank upper margin. The full-screen button's
            // center becomes a BetSpot after down closes the menu, so its
            // queued up would leave an unrelated hover-preview chip behind.
            Vector3 localPoint = portraitPoint.HasValue ?
                new Vector3(portraitPoint.Value.x, -portraitPoint.Value.y, 0f) :
                (Vector3)((RectTransform)target.transform).rect.center;
            Vector2 center = RectTransformUtility.WorldToScreenPoint(null,
                target.transform.TransformPoint(localPoint));
            if (portraitPoint.HasValue)
            {
                var probe = new PointerEventData(EventSystem.current) { position = center };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(probe, hits);
                RequireDemoCleanup(hits.Count != 0 && hits[0].gameObject == target.gameObject,
                    "neutral outside point did not hit the menu dismiss surface");
            }
            sourceInputModule.QueuePointerSample(PointerInputModule.kMouseLeftId,
                center, true, false, true, false);
            return sourceInputModule.QueuePointerSample(PointerInputModule.kMouseLeftId,
                center, false, true, false, false);
        }

        private void DemoCleanupClick(Button target, Vector2? portraitPoint = null)
        {
            PointerEventData pointer = DemoCleanupQueue(target, portraitPoint);
            RequireDemoCleanup(SourcePlayerInputs.DispatchOne(), "queued down was missing");
            DemoCleanupDrainRelease(pointer);
        }

        private void DemoCleanupDrainRelease(PointerEventData pointer)
        {
            RequireDemoCleanup(SourcePlayerInputs.Count <= 1, "choice created unexpected queued input");
            if (SourcePlayerInputs.Count != 0)
                RequireDemoCleanup(SourcePlayerInputs.DispatchOne(), "queued release disappeared");
            RequireSourcePointerReleased(pointer);
            RequireDemoCleanup(SourcePlayerInputs.Count == 0, "release left queued input");
        }

        private string DemoCleanupEconomy()
        {
            var local = (RouletteSession)session;
            return session.Profile.Id + "|" + string.Join("/", new[] {
                session.Credit, session.Denomination, local.MoneyRemainderCents, session.Chip,
                session.TotalBet, session.Wagered, session.Returned, session.Rounds, session.Wins
            }.Select(value => value.ToString()).ToArray()) + "|" +
                DemoCleanupResult(session.LastResult) + "|" +
                string.Join(";", session.History.Select(DemoCleanupResult).ToArray()) + "|" +
                string.Join(";", session.Bets.OrderBy(pair => pair.Key).Select(pair =>
                    pair.Key + "=" + pair.Value).ToArray()) + "|" +
                string.Join(";", session.BetChipStacks.OrderBy(pair => pair.Key).Select(pair =>
                    pair.Key + "=" + string.Join(",", pair.Value.Select(value => value.ToString()).ToArray())).ToArray()) + "|" +
                string.Join(",", local.BetEvaluationOrder.Select(value => value.ToString()).ToArray()) + "|" +
                string.Join(",", recordedPockets.Select(value => value.ToString()).ToArray()) + "|" +
                string.Join(";", recordedSnapshots.Select(Convert.ToBase64String).ToArray());
        }

        private static string DemoCleanupResult(RoundResult result)
        {
            if (result == null) return "null";
            return result.Pocket + "/" + result.Wager + "/" + result.Returned + "/" +
                result.Category + "/" + result.WinLimitReached + ":" +
                string.Join(",", result.WinningPays.Select(pay =>
                    pay.BetId + "=" + pay.Amount + "/" + pay.WinLevel).ToArray()) + ":" +
                string.Join(",", result.HistoryPayOverrides.Select(pay =>
                    pay.PayIndex + "=" + pay.Amount + "/" + pay.ProgressiveLevel + "/" + pay.Kind).ToArray());
        }

        private string DemoCleanupArchive(bool ignoreOfferCompletion)
        {
            var copy = JsonUtility.FromJson<RecordedRoundArchive>(JsonUtility.ToJson(
                new RecordedRoundArchive { rounds = recordedRoundPages.ToArray() }));
            if (ignoreOfferCompletion)
                foreach (RecordedRound round in copy.rounds) round.awaitingFinalPayout = false;
            return JsonUtility.ToJson(copy);
        }

        private static string DemoCleanupRngBytes()
        {
            return Convert.ToBase64String(File.ReadAllBytes(PcRngStateStore.PathForCurrentProcess));
        }

        private void CaptureDemoCleanup(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteDemoResultCleanupCapturePrefix");
            if (at < 0) return;
            RequireDemoCleanup(at + 1 < args.Length, "capture prefix is missing");
            string path = args[at + 1] + "_" + name + ".png";
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            CaptureActiveScreenOffscreen(path);
        }
    }
}
