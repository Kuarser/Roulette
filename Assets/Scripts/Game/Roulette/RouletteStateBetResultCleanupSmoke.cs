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
        [Serializable]
        private sealed class StateBetCleanupReceipt
        {
            public string testCase, archiveId, economy, restoredEconomy, archive, rng, cleared, staged, undo;
            public bool resultPhase, canUndo, afterReloadUndo;
            public Vector2 ball;
        }

        [Serializable]
        private sealed class StateBetCleanupUndoSnapshot
        {
            public int lastTouched;
            public RecordedUndo[] entries;
        }

        [Serializable]
        private sealed class StateBetPendingUndoReceipt
        {
            public bool settled;
            public string undo, pending, rng, economy, archive, cleared, staged;
        }

        [Serializable]
        private sealed class StateBetLowCreditUndoReceipt
        {
            public int remainingBet;
            public string undo, rng, economy, archive, cleared, staged, settledUndo;
        }

        private string StateBetCleanupReceiptKey
        {
            get { return recordedHistoryKey + ".StateBetCleanupFixture.v1"; }
        }

        private string StateBetPendingUndoReceiptKey
        {
            get { return recordedHistoryKey + ".StateBetPendingUndoFixture.v1"; }
        }

        private string StateBetLowCreditUndoReceiptKey
        {
            get { return recordedHistoryKey + ".StateBetLowCreditUndoFixture.v1"; }
        }

        private static void RequireStateBetCleanup(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("StateBet result cleanup: " + message);
        }

        // Each case starts with a real wheel/rack win. The action goes through
        // captured device samples, dispatch-time raycasts and the existing
        // table/control handlers. A second process verifies durable recovery.
        private IEnumerator StateBetResultCleanupSmoke(bool load)
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteStateBetCleanupCase");
            RequireStateBetCleanup(at >= 0 && at + 1 < args.Length, "missing case");
            string testCase = args[at + 1];
            RequireStateBetCleanup(new[] { "direct", "neighbor", "sector", "reject_spot",
                "reject_total", "undo", "clear", "low_credit_undo" }.Contains(testCase), "unknown case");
            if (testCase == "low_credit_undo")
            {
                yield return StateBetLowCreditUndoSmoke(load);
                yield break;
            }
            bool rejected = testCase.StartsWith("reject_", StringComparison.Ordinal);
            RequireStateBetCleanup(demoShowMode == 1 && sourceInputModule != null &&
                demoOutcome != null && session != null && !temporaryHistory &&
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                !spinning && !session.SpinPending && !session.InDoubleUp &&
                !autoplay && autoRoutine == null && SourcePlayerInputs.Count == 0,
                "requires isolated history/volume/RNG and idle demo mode");

            bool moduleEnabled = sourceInputModule.enabled;
            int checks = 0;
            try
            {
                sourceInputModule.enabled = false;
                if (load)
                {
                    var proof = JsonUtility.FromJson<StateBetCleanupReceipt>(
                        PlayerPrefs.GetString(StateBetCleanupReceiptKey, ""));
                    bool restoredMarker = rejected && proof != null && !proof.afterReloadUndo;
                    RequireStateBetCleanup(proof != null && proof.testCase == testCase &&
                        recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                        recordedRoundPages[0].roundArchiveId == proof.archiveId &&
                        DemoCleanupEconomy() == proof.restoredEconomy && DemoCleanupArchive(false) == proof.archive &&
                        DemoCleanupRngBytes() == proof.rng && session.ResultPhase == proof.resultPhase &&
                        sourceBetsChangedSincePlay == (!rejected || proof.afterReloadUndo) &&
                        session.CanUndo == proof.canUndo && StateBetCleanupUndoFingerprint() == proof.undo &&
                        PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                        PlayerPrefs.GetString(StagedBetKey, "") == proof.staged &&
                        pendingSpin == null && ReadPcCreditEvent() == null,
                        "reload changed funds/table/undo/archive/RNG or presentation journals");
                    RequireStateBetCleanupCleared(restoredMarker);
                    // Native Restore quantizes the wheel from its saved stop
                    // index; it does not preserve the animation's last frame.
                    Vector2 restoredBall = RestoredBallCenter(recordedRoundPages[0]);
                    RequireStateBetCleanup(uiBall.rectTransform.anchoredPosition ==
                        new Vector2(restoredBall.x - 12f, -(restoredBall.y - 12f)),
                        "reload ball differs from saved source wheel-stop geometry");
                    ++checks;
                    StateBetCleanupParkPointer();
                    yield return new WaitForSecondsRealtime(1.6f);
                    RequireStateBetCleanupCleared(restoredMarker);
                    RequireStateBetCleanup(DemoCleanupEconomy() == proof.restoredEconomy &&
                        DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                        StateBetCleanupUndoFingerprint() == proof.undo,
                        "reload update/cycle timeout revived or changed the completed round");
                    CaptureStateBetCleanup(proof.afterReloadUndo ? "undo_reload" : "reload");
                    ++checks;
                    if (testCase == "direct")
                    {
                        checks += StateBetCleanupRecoveryBranches(proof);
                        yield return DemoCleanupSelect(30);
                        RequireStateBetCleanup(demoOutcome.SelectedPocket == 27 && session.TotalBet == 2,
                            "next actual round lost its selected pocket or two-chip wager");
                        yield return DemoCleanupStartAndSettle();
                        RequireStateBetCleanup(!currentGameResultsCleared && !sourceBetsChangedSincePlay &&
                            session.Credit == 1105 &&
                            session.Rounds == 2 && session.Wagered == 3 && session.Returned == 108 &&
                            session.LastResult.Pocket == 27 && session.LastResult.Returned == 72 &&
                            session.DoubleEnabled && !session.DoubleAvailable && session.DoubleStake == 0 &&
                            recordedRoundPages.Count == 2 && !recordedRoundPages[0].awaitingFinalPayout &&
                            recordedRoundPages[1].roundArchiveId == proof.archiveId &&
                            JsonUtility.ToJson(recordedRoundPages[1]) == JsonUtility.ToJson(
                                JsonUtility.FromJson<RecordedRoundArchive>(proof.archive).rounds[0]) &&
                            DemoCleanupRngBytes() != proof.rng &&
                            PlayerPrefs.GetString(GameResultsClearedKey, "") == "",
                            "next real win inherited cleanup, lost its offer or altered the previous archive");
                        yield return StateBetCleanupWaitForWin(72);
                        StateBetCleanupParkPointer();
                        CaptureStateBetCleanup("next_win");
                        ++checks;
                    }
                    else if (!proof.afterReloadUndo && (rejected || testCase == "undo"))
                    {
                        if (testCase == "reject_spot") checks += StateBetCleanupOptionalUndoIsolation(proof);
                        if (testCase == "undo") RequireStateBetCleanupUndoState(testCase);
                        StateBetCleanupReplayRestoredUndo(proof);
                        ++checks;
                    }
                    Debug.Log("ROULETTE_STATE_BET_RESULT_CLEANUP_LOAD_PASSED: case=" + testCase +
                        " checks=" + checks + " credit=" + session.Credit + " rounds=" + session.Rounds +
                        " markerRestored=" + restoredMarker + " reloadNoReroll=true nextActualWin=" +
                        (testCase == "direct") + " exactUndoRestored=true afterReloadUndo=" +
                        proof.afterReloadUndo + " au=" + auNationalStandardsEnabled);
                }
                else
                {
                    RequireStateBetCleanup(session.Credit == 1000 && session.TotalBet == 0 &&
                        session.Rounds == 0 && recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                        ReadPcCreditEvent() == null && !PlayerPrefs.HasKey(StateBetCleanupReceiptKey) &&
                        configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo,
                        "save fixture requires unused keys and default configuration");
                    var counted = new DemoCleanupOutcome();
                    demoOutcome = new DemoPocketOutcomeSource(counted);
                    outcome = demoOutcome;
                    session = new RouletteSession(session.Profile, 1000, demoOutcome) { DoubleEnabled = true };
                    long initialWager = testCase == "undo" ? 2 : 1;
                    long initialCredit = 1000 - initialWager + 36;
                    SetPcGameSoundsMuted(false);
                    yield return DemoCleanupSelect(30);
                    session.PlaceBet(127, 1);
                    if (testCase == "undo") session.PlaceBet(101, 1);
                    SaveStagedBetsIfChanged();
                    yield return DemoCleanupStartAndSettle();
                    yield return StateBetCleanupWaitForWin(36);
                    RequireStateBetCleanup(session.Credit == initialCredit && session.TotalBet == initialWager &&
                        session.Rounds == 1 && !session.DoubleAvailable && counted.Pockets == 1 &&
                        counted.Suits == 0 && recordedRoundPages.Count == 1 &&
                        !recordedRoundPages[0].awaitingFinalPayout, "initial actual completed win differs");
                    ++checks;
                    string before = DemoCleanupEconomy();
                    string archive = DemoCleanupArchive(true);
                    string rng = DemoCleanupRngBytes();
                    string result = DemoCleanupResult(session.LastResult);
                    long cycle = resultCycleStartedTimestamp;
                    Vector2 ball = uiBall.rectTransform.anchoredPosition;

                    // Chip event 12 and gadget validation do not dispatch a
                    // roulette table mutation/rejection event in the source.
                    DemoCleanupClick(uiChipSelections[1].GetComponent<Button>());
                    RequireStateBetCleanup(session.Chip == 2, "real chip selection did not run");
                    DemoCleanupClick(uiChipSelections[0].GetComponent<Button>());
                    StateBetCleanupPreview(27, true);
                    int savedLimit = session.Profile.MaximumTotalBet;
                    try
                    {
                        session.Profile.MaximumTotalBet = (int)initialWager;
                        StateBetCleanupPreview(0, false);
                        DemoCleanupClick(uiVoisinsButton);
                    }
                    finally { session.Profile.MaximumTotalBet = savedLimit; }
                    StateBetCleanupParkPointer();
                    RefreshPortraitUi();
                    RequireStateBetCleanup(!currentGameResultsCleared && session.ResultPhase &&
                        !session.DoubleAvailable && session.DoubleStake == 0 &&
                        resultCycleStartedTimestamp == cycle && DisplayedPaid == 36 &&
                        uiTableNumberMark.gameObject.activeSelf && DemoCleanupEconomy() == before &&
                        DemoCleanupArchive(true) == archive && DemoCleanupRngBytes() == rng &&
                        !recordedRoundPages[0].awaitingFinalPayout,
                        "chip/valid preview/gadget rejection cleared or changed the live result");
                    ++checks;
                    yield return StateBetCleanupWaitForWin(36);
                    CaptureStateBetCleanup("before");

                    int savedSpotLimit = session.Profile.MaximumSpotBet;
                    savedLimit = session.Profile.MaximumTotalBet;
                    try
                    {
                        if (testCase == "reject_spot") session.Profile.MaximumSpotBet = 1;
                        if (testCase == "reject_total") session.Profile.MaximumTotalBet = 1;
                        Button target = uiBetButtons[27];
                        if (testCase == "neighbor")
                        {
                            // The same captured mouse previews a number and
                            // then reaches the separate neighbors control.
                            // Do not bypass its exit/selection behavior.
                            StateBetCleanupPreview(27, true);
                            RefreshPortraitUi();
                            RequireStateBetCleanup(selectedNeighbor == 27 && uiNeighborButton.interactable,
                                "single-pointer preview release lost the neighbors selection");
                            target = uiNeighborButton;
                        }
                        else if (testCase == "sector") target = uiVoisinsButton;
                        else if (testCase == "undo") target = uiUndoButton;
                        else if (testCase == "clear") target = uiClearButton;
                        RefreshPortraitUi();
                        RequireStateBetCleanup(target.interactable, "case control was disabled");
                        PointerEventData pointer = DemoCleanupQueue(target);
                        RequireStateBetCleanup(SourcePlayerInputs.DispatchOne(), "action down was missing");
                        if (testCase != "neighbor" && testCase != "sector")
                            RequireStateBetCleanup(!currentGameResultsCleared && session.ResultPhase &&
                                session.TotalBet == initialWager && !session.DoubleAvailable,
                                "release action cleared results before its actual release");
                        DemoCleanupDrainRelease(pointer);
                    }
                    finally
                    {
                        session.Profile.MaximumSpotBet = savedSpotLimit;
                        session.Profile.MaximumTotalBet = savedLimit;
                    }

                    RequireStateBetCleanup(session.ResultPhase == rejected &&
                        sourceBetsChangedSincePlay == !rejected && session.Credit == initialCredit &&
                        session.Wagered == initialWager && session.Returned == 36 && session.Rounds == 1 &&
                        session.Wins == 1 && DemoCleanupResult(session.LastResult) == result &&
                        DemoCleanupArchive(true) == archive && !recordedRoundPages[0].awaitingFinalPayout &&
                        counted.Pockets == 1 && counted.Suits == 0 && DemoCleanupRngBytes() == rng &&
                        pendingSpin == null && !spinning && !autoplay &&
                        !configuredAutoPlayCheckPending && !configuredAutoPlayFinishPending,
                        "table event changed settlement/RNG or failed immediate offer finalization");
                    RequireStateBetCleanupBets(testCase);
                    RequireStateBetCleanupUndoState(testCase);
                    RequireStateBetCleanupCleared(false);
                    PromptKind expected = testCase == "reject_spot" ? PromptKind.BetSpotLimitReached :
                        testCase == "reject_total" ? PromptKind.BetLimitReached :
                        testCase == "undo" || testCase == "clear" ? PromptKind.PlaceBets : PromptKind.PressSpin;
                    if (rejected)
                    {
                        RequireStateBetCleanup(!sourcePromptStopped && failurePrompt == expected &&
                            failurePromptStartedTimestamp > 0,
                            "rejected table event did not start its source failure prompt");
                        // Failure playback owns its own clock and returns
                        // before the ordinary shownPrompt list is updated.
                        RefreshPromptUiAt(failurePromptStartedTimestamp,
                            System.Diagnostics.Stopwatch.Frequency);
                        RequireStateBetCleanup(promptRoot.gameObject.activeSelf &&
                            uiPrompt.texture == PromptFrames(expected, false)[0],
                            "source rejection did not display its first failure frame at the start clock");
                    }
                    else
                        RequireStateBetCleanup(!sourcePromptStopped && shownPrompt == expected &&
                            promptRoot.gameObject.activeSelf, "source action did not immediately show its named prompt");
                    RequireStateBetCleanup(uiBall.rectTransform.anchoredPosition == ball,
                        "table event moved the retained ball");
                    ++checks;
                    string clearRecord = PlayerPrefs.GetString(GameResultsClearedKey, "");
                    string staged = PlayerPrefs.GetString(StagedBetKey, "");
                    RequireStateBetCleanup(!string.IsNullOrEmpty(clearRecord) &&
                        (rejected ? string.IsNullOrEmpty(staged) : !string.IsNullOrEmpty(staged)),
                        "table event did not persist the matching clear/staged journals");
                    var proof = new StateBetCleanupReceipt {
                        testCase = testCase, archiveId = recordedRoundPages[0].roundArchiveId,
                        economy = DemoCleanupEconomy(), restoredEconomy = StateBetCleanupRestoredEconomy(),
                        archive = DemoCleanupArchive(false), rng = rng,
                        cleared = clearRecord, staged = staged, resultPhase = session.ResultPhase,
                        canUndo = session.CanUndo, undo = StateBetCleanupUndoFingerprint(), ball = ball
                    };
                    StateBetCleanupParkPointer();
                    yield return new WaitForSecondsRealtime(1.6f);
                    RequireStateBetCleanupCleared(false);
                    RequireStateBetCleanup(DemoCleanupEconomy() == proof.economy &&
                        DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                        PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                        PlayerPrefs.GetString(StagedBetKey, "") == proof.staged &&
                        StateBetCleanupUndoFingerprint() == proof.undo &&
                        counted.Pockets == 1 && counted.Suits == 0,
                        "release/update/cycle timeout restored a result or changed persisted state");
                    PlayerPrefs.SetString(StateBetCleanupReceiptKey, JsonUtility.ToJson(proof));
                    PlayerPrefs.Save();
                    CaptureStateBetCleanup("after");
                    ++checks;
                    Debug.Log("ROULETTE_STATE_BET_RESULT_CLEANUP_PASSED: case=" + testCase +
                        " checks=" + checks + " credit=" + initialCredit + " wager=" + session.TotalBet +
                        " returned=36 rounds=1 draws=1 featureDraws=0 actualPointerQueue=true " +
                        "chipAndGadgetPreserved=true archivePreserved=true immediateOfferFinalized=true exactUndoSaved=true au=" +
                        auNationalStandardsEnabled);
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = moduleEnabled;
            }
            Application.Quit();
        }

        // A completed losing wager can exceed the remaining credit. Its
        // table and Undo metadata remain recoverable while edits reduce it.
        private IEnumerator StateBetLowCreditUndoSmoke(bool load)
        {
            RequireStateBetCleanup(demoShowMode == 1 && sourceInputModule != null &&
                demoOutcome != null && session != null && !temporaryHistory &&
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                !spinning && !session.SpinPending && !session.InDoubleUp &&
                !autoplay && autoRoutine == null && SourcePlayerInputs.Count == 0,
                "low-credit Undo fixture requires isolated persistent state and manual idle mode");
            bool moduleEnabled = sourceInputModule.enabled;
            try
            {
                sourceInputModule.enabled = false;
                StateBetLowCreditUndoReceipt proof;
                if (!load)
                {
                    RequireStateBetCleanup(session.Credit == 1000 && session.TotalBet == 0 &&
                        session.Rounds == 0 && recordedRoundPages.Count == 0 &&
                        recordedPockets.Count == 0 && ReadPcCreditEvent() == null &&
                        !PlayerPrefs.HasKey(StateBetLowCreditUndoReceiptKey),
                        "low-credit save requires unused fixture keys");
                    session = new RouletteSession(session.Profile, 4, demoOutcome) { DoubleEnabled = true };
                    yield return DemoCleanupSelect(19); // fixed mathematical pocket 16
                    for (int i = 0; i < 3; ++i)
                    {
                        RefreshPortraitUi();
                        DemoCleanupClick(uiBetButtons[27]);
                    }
                    RequireStateBetCleanup(session.Credit == 4 && session.TotalBet == 3 &&
                        ((RouletteSession)session).ExportUndoHistory().Length == 3,
                        "actual low-credit input did not create three one-chip operations");
                    string rngBefore = DemoCleanupRngBytes();
                    yield return DemoCleanupStartAndSettle();
                    RequireStateBetCleanup(session.Credit == 1 && session.TotalBet == 3 &&
                        session.ResultPhase && session.Rounds == 1 && session.Wagered == 3 &&
                        session.Returned == 0 && session.Wins == 0 &&
                        session.LastResult.Pocket == 16 && session.LastResult.Returned == 0 &&
                        recordedRoundPages.Count == 1 && !recordedRoundPages[0].awaitingFinalPayout &&
                        DemoCleanupRngBytes() != rngBefore && !sourceBetsChangedSincePlay,
                        "actual low-credit loss did not settle its wager once");
                    proof = new StateBetLowCreditUndoReceipt {
                        remainingBet = 3, archive = DemoCleanupArchive(false),
                        rng = DemoCleanupRngBytes(), settledUndo = PlayerPrefs.GetString(SettledTableUndoKey, "")
                    };
                    StateBetLowCreditUndoOnce(proof);
                }
                else
                {
                    proof = JsonUtility.FromJson<StateBetLowCreditUndoReceipt>(
                        PlayerPrefs.GetString(StateBetLowCreditUndoReceiptKey, ""));
                    RequireStateBetCleanup(proof != null && proof.remainingBet >= 0 && proof.remainingBet <= 2 &&
                        DemoCleanupEconomy() == proof.economy && StateBetCleanupUndoFingerprint() == proof.undo &&
                        DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                        PlayerPrefs.GetString(StagedBetKey, "") == proof.staged &&
                        PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                        PlayerPrefs.GetString(SettledTableUndoKey, "") == proof.settledUndo,
                        "low-credit reload changed its table/Undo/funds/archive/RNG or journals");
                    RequireStateBetLowCreditUndo(proof.remainingBet);
                    RequireStateBetLowCreditArchiveUndo(proof);
                    StateBetCleanupParkPointer();
                    CaptureStateBetCleanup("low_credit_reload_bet" + proof.remainingBet);
                    if (proof.remainingBet > 0) StateBetLowCreditUndoOnce(proof);
                }
                RequireStateBetLowCreditUndo(proof.remainingBet);
                RequireStateBetLowCreditArchiveUndo(proof);
                StateBetCleanupParkPointer();
                yield return new WaitForSecondsRealtime(1.6f);
                RequireStateBetLowCreditUndo(proof.remainingBet);
                RequireStateBetCleanup(DemoCleanupEconomy() == proof.economy &&
                    StateBetCleanupUndoFingerprint() == proof.undo && DemoCleanupArchive(false) == proof.archive &&
                    DemoCleanupRngBytes() == proof.rng &&
                    PlayerPrefs.GetString(StagedBetKey, "") == proof.staged &&
                    PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                    PlayerPrefs.GetString(SettledTableUndoKey, "") == proof.settledUndo,
                    "low-credit update changed restored table metadata or settlement");
                CaptureStateBetCleanup("low_credit_" + (load ? "after" : "save") + "_bet" + proof.remainingBet);
                Debug.Log((load ? "ROULETTE_STATE_BET_LOW_CREDIT_UNDO_LOAD_PASSED" :
                    "ROULETTE_STATE_BET_LOW_CREDIT_UNDO_SAVE_PASSED") +
                    ": credit=1 bet=" + proof.remainingBet + " rounds=1 wagered=3 returned=0 " +
                    "actualLoss=true exactUndoRestored=true archivedHigherStakePreserved=true noReroll=true");
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = moduleEnabled;
            }
            Application.Quit();
        }

        private void StateBetLowCreditUndoOnce(StateBetLowCreditUndoReceipt proof)
        {
            RefreshPortraitUi();
            RequireStateBetCleanup(proof.remainingBet > 0 && uiUndoButton.interactable &&
                session.TotalBet == proof.remainingBet && session.LastUndoSourceSpot == 27,
                "low-credit table did not expose its next source Undo operation");
            string beforeUndo = StateBetCleanupUndoFingerprint();
            Vector2 ball = uiBall.rectTransform.anchoredPosition;
            PointerEventData pointer = DemoCleanupQueue(uiUndoButton);
            RequireStateBetCleanup(SourcePlayerInputs.DispatchOne() &&
                session.TotalBet == proof.remainingBet && StateBetCleanupUndoFingerprint() == beforeUndo,
                "low-credit Undo acted before release");
            DemoCleanupDrainRelease(pointer);
            --proof.remainingBet;
            RequireStateBetLowCreditUndo(proof.remainingBet);
            RequireStateBetCleanup(uiBall.rectTransform.anchoredPosition == ball &&
                DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                PlayerPrefs.GetString(SettledTableUndoKey, "") == proof.settledUndo,
                "low-credit Undo changed the retained ball, archived round or RNG");
            proof.economy = StateBetCleanupRestoredEconomy();
            proof.undo = StateBetCleanupUndoFingerprint();
            proof.staged = PlayerPrefs.GetString(StagedBetKey, "");
            proof.cleared = PlayerPrefs.GetString(GameResultsClearedKey, "");
            var staged = JsonUtility.FromJson<StagedBetRecord>(proof.staged);
            RequireStateBetCleanup(staged != null && staged.hasSourceTableEditState && staged.sourceTableEdited &&
                staged.hasUndoState && staged.undo.Length == proof.remainingBet &&
                !string.IsNullOrEmpty(proof.cleared), "low-credit edit and Undo queue were not persisted");
            PlayerPrefs.SetString(StateBetLowCreditUndoReceiptKey, JsonUtility.ToJson(proof));
            PlayerPrefs.Save();
        }

        private void RequireStateBetLowCreditUndo(int remaining)
        {
            RefreshPortraitUi();
            var local = (RouletteSession)session;
            RouletteSession.SavedUndoState[] queue = local.ExportUndoHistory();
            RequireStateBetCleanup(session.Credit == 1 && session.Wagered == 3 && session.Returned == 0 &&
                session.Rounds == 1 && session.Wins == 0 && session.TotalBet == remaining &&
                session.Bets.Count == (remaining == 0 ? 0 : 1) &&
                (remaining == 0 || session.Bets[127] == remaining &&
                    session.BetChipStacks[127].SequenceEqual(Enumerable.Repeat(1L, remaining))) &&
                queue.Length == remaining && session.CanUndo == (remaining != 0) &&
                session.LastUndoSourceSpot == (remaining == 0 ? -1 : 27) &&
                local.LastTouchedBet == (remaining == 0 ? -1 : 127) &&
                queue.All(entry => entry.SourceSpotPlusOne == 28 && entry.PlaysRemovalSound) &&
                sourceBetsChangedSincePlay && currentGameResultsCleared && currentTableResultMarkerCleared &&
                !session.ResultPhase && !session.SpinPending && !session.InDoubleUp &&
                !session.DoubleAvailable && session.DoubleStake == 0 && pendingSpin == null && !spinning &&
                session.LastResult != null && session.LastResult.Pocket == 16 &&
                session.LastResult.Wager == 3 && session.LastResult.Returned == 0 &&
                recordedRoundPages.Count == 1 && !recordedRoundPages[0].awaitingFinalPayout &&
                resultCycleStartedTimestamp == 0 && VisibleWinningPayId() == -1 &&
                DisplayedPaid == 0 && !uiTableNumberMark.gameObject.activeSelf && uiBall.gameObject.activeSelf &&
                uiCredit.Value == "1" && uiBet.Value == remaining.ToString(),
                "low-credit Undo/reload changed financial state, active history or source result presentation");
            if (auNationalStandardsEnabled)
                RequireStateBetCleanup(CurrentFooterMovieFrame() == null && !uiFooterMovie.gameObject.activeSelf,
                    "AU low-credit Undo/reload kept the footer movie visible beside WIN 0");
        }

        private void RequireStateBetLowCreditArchiveUndo(StateBetLowCreditUndoReceipt proof)
        {
            var archived = JsonUtility.FromJson<SettledTableUndoRecord>(proof.settledUndo);
            RequireStateBetCleanup(archived != null && archived.tableUndo != null && archived.tableUndo.Length == 3 &&
                archived.tableUndo.Any(entry => entry.bets.Sum(bet => bet.amount) > session.Credit) &&
                archived.roundArchiveId == recordedRoundPages[0].roundArchiveId,
                "settled sidecar lost the original higher-stake history (separate from the shrinking active queue)");
        }

        // Three real processes: interrupt an accepted PLAY, resume its wheel
        // and Undo the restored last operation, then reload the edited table.
        // The production startup path owns pending-spin visual resumption.
        private IEnumerator StateBetPendingUndoSmoke(bool load)
        {
            yield return null;
            RequireStateBetCleanup(demoShowMode == 1 && sourceInputModule != null &&
                demoOutcome != null && session != null && !temporaryHistory &&
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                !autoplay && autoRoutine == null && SourcePlayerInputs.Count == 0,
                "pending Undo fixture requires isolated history/volume/RNG and manual demo mode");
            bool moduleEnabled = sourceInputModule.enabled;
            try
            {
                sourceInputModule.enabled = false;
                if (!load)
                {
                    RequireStateBetCleanup(!spinning && !session.SpinPending && pendingSpin == null &&
                        session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                        recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                        ReadPcCreditEvent() == null && !PlayerPrefs.HasKey(StateBetPendingUndoReceiptKey),
                        "pending Undo save requires unused keys");
                    yield return DemoCleanupSelect(30);
                    RefreshPortraitUi();
                    DemoCleanupClick(uiBetButtons[27]);
                    RefreshPortraitUi();
                    DemoCleanupClick(uiBetButtons[1]);
                    RefreshPortraitUi();
                    RequireStateBetCleanup(session.TotalBet == 2 && session.Bets.Count == 2 &&
                        session.Bets[127] == 1 && session.Bets[101] == 1 &&
                        session.LastUndoSourceSpot == 1 && ((RouletteSession)session).LastTouchedBet == 101 &&
                        ((RouletteSession)session).ExportUndoHistory().Length == 2,
                        "actual table input did not stage two distinct Undo operations");
                    string undo = StateBetCleanupUndoFingerprint();
                    DemoCleanupClick(uiSpinButton);
                    RequireStateBetCleanup(spinning && session.SpinPending && session.PendingPocket == 27 &&
                        session.Credit == 998 && session.Rounds == 0 && !sourceBetsChangedSincePlay &&
                        pendingSpin != null && pendingSpin.hasTableUndo && pendingSpin.tableLastTouched == 101 &&
                        pendingSpin.tableUndo != null && pendingSpin.tableUndo.Length == 2 &&
                        pendingSpin.tableUndo[0].sourceSpotPlusOne == 28 &&
                        pendingSpin.tableUndo[1].sourceSpotPlusOne == 2 &&
                        StateBetCleanupUndoFingerprint() == undo,
                        "accepted PLAY failed to commit both pending Undo operations");
                    var proof = new StateBetPendingUndoReceipt {
                        undo = undo, pending = PlayerPrefs.GetString(PendingSpinKey, ""),
                        rng = DemoCleanupRngBytes()
                    };
                    RequireStateBetCleanup(!string.IsNullOrEmpty(proof.pending), "pending journal was not durable");
                    PlayerPrefs.SetString(StateBetPendingUndoReceiptKey, JsonUtility.ToJson(proof));
                    PlayerPrefs.Save();
                    CaptureStateBetCleanup("pending_saved");
                    Debug.Log("ROULETTE_STATE_BET_PENDING_UNDO_SAVE_PASSED: credit=998 pending=27 wager=2 " +
                        "undo=2 sourceSpots=27,1 actualPlay=true");
                }
                else
                {
                    var proof = JsonUtility.FromJson<StateBetPendingUndoReceipt>(
                        PlayerPrefs.GetString(StateBetPendingUndoReceiptKey, ""));
                    RequireStateBetCleanup(proof != null && StateBetCleanupUndoFingerprint() == proof.undo &&
                        DemoCleanupRngBytes() == proof.rng, "pending Undo reload lost its queue or redrew RNG");
                    if (!proof.settled)
                    {
                        RequireStateBetCleanup(spinning && session.SpinPending && session.PendingPocket == 27 &&
                            session.Credit == 998 && session.Rounds == 0 && pendingSpin != null &&
                            StateBetPendingCommitFingerprint(PlayerPrefs.GetString(PendingSpinKey, "")) ==
                                StateBetPendingCommitFingerprint(proof.pending) &&
                            session.LastUndoSourceSpot == 1 && ((RouletteSession)session).LastTouchedBet == 101,
                            "startup did not resume the committed pending wager and its source operation");
                        yield return StartGameWaitForActualSettlement();
                        yield return StateBetCleanupWaitForWin(36);
                        RequireStateBetCleanup(session.Credit == 1034 && session.Wagered == 2 &&
                            session.Returned == 36 && session.Rounds == 1 && session.Wins == 1 &&
                            session.TotalBet == 2 && recordedRoundPages.Count == 1 &&
                            pendingSpin == null && StateBetCleanupUndoFingerprint() == proof.undo &&
                            DemoCleanupRngBytes() == proof.rng && !sourceBetsChangedSincePlay,
                            "pending round settled more than once, changed Undo, or redrew RNG");
                        string archive = DemoCleanupArchive(true);
                        CaptureStateBetCleanup("pending_resumed_win");
                        RefreshPortraitUi();
                        RequireStateBetCleanup(uiUndoButton.interactable, "resumed win did not enable Undo");
                        PointerEventData pointer = DemoCleanupQueue(uiUndoButton);
                        RequireStateBetCleanup(SourcePlayerInputs.DispatchOne() && session.TotalBet == 2 &&
                            session.ResultPhase && !session.DoubleAvailable,
                            "resumed Undo acted before release");
                        DemoCleanupDrainRelease(pointer);
                        RequireStateBetCleanupCleared(false);
                        RequireStateBetCleanupUndoState("undo");
                        RequireStateBetCleanup(sourceBetsChangedSincePlay && !session.ResultPhase &&
                            session.TotalBet == 1 && session.Bets.Count == 1 && session.Bets[127] == 1 &&
                            session.Credit == 1034 && session.Wagered == 2 && session.Returned == 36 &&
                            session.Rounds == 1 && session.Wins == 1 &&
                            DemoCleanupArchive(true) == archive && DemoCleanupRngBytes() == proof.rng,
                            "Undo of the recovered pending operation changed settlement or removed the wrong bet");
                        proof.settled = true;
                        proof.undo = StateBetCleanupUndoFingerprint();
                        proof.economy = StateBetCleanupRestoredEconomy();
                        proof.archive = DemoCleanupArchive(false);
                        proof.cleared = PlayerPrefs.GetString(GameResultsClearedKey, "");
                        proof.staged = PlayerPrefs.GetString(StagedBetKey, "");
                        RequireStateBetCleanup(!string.IsNullOrEmpty(proof.cleared) &&
                            !string.IsNullOrEmpty(proof.staged), "pending-resume Undo was not durably saved");
                        PlayerPrefs.SetString(StateBetPendingUndoReceiptKey, JsonUtility.ToJson(proof));
                        PlayerPrefs.Save();
                        StateBetCleanupParkPointer();
                        CaptureStateBetCleanup("pending_resumed_undo");
                        Debug.Log("ROULETTE_STATE_BET_PENDING_UNDO_LOAD_PASSED: credit=1034 rounds=1 " +
                            "actualResume=true undo=1 sourceSpot=27 noReroll=true settledReload=false");
                    }
                    else
                    {
                        RequireStateBetCleanup(!spinning && !session.SpinPending && pendingSpin == null &&
                            session.Credit == 1034 && session.TotalBet == 1 && session.Rounds == 1 &&
                            sourceBetsChangedSincePlay && !session.ResultPhase &&
                            DemoCleanupEconomy() == proof.economy && DemoCleanupArchive(false) == proof.archive &&
                            PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                            PlayerPrefs.GetString(StagedBetKey, "") == proof.staged,
                            "post-pending edited-table reload lost source state, funds, journals or the prior archive");
                        RequireStateBetCleanupCleared(false);
                        RequireStateBetCleanupUndoState("undo");
                        StateBetCleanupParkPointer();
                        yield return new WaitForSecondsRealtime(1.6f);
                        RequireStateBetCleanupCleared(false);
                        RequireStateBetCleanup(StateBetCleanupUndoFingerprint() == proof.undo &&
                            DemoCleanupEconomy() == proof.economy && DemoCleanupArchive(false) == proof.archive &&
                            DemoCleanupRngBytes() == proof.rng,
                            "post-pending reload update revived or changed the settled round");
                        CaptureStateBetCleanup("pending_settled_reload");
                        Debug.Log("ROULETTE_STATE_BET_PENDING_UNDO_LOAD_PASSED: credit=1034 rounds=1 " +
                            "actualResume=true undo=1 sourceSpot=27 noReroll=true settledReload=true");
                    }
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                sourceInputModule.enabled = moduleEnabled;
            }
            Application.Quit();
        }

        private string StateBetPendingCommitFingerprint(string raw)
        {
            var record = JsonUtility.FromJson<PendingSpinRecord>(raw);
            RequireStateBetCleanup(record != null, "pending commitment is unreadable");
            // Native SetStop predicts and saves a new wheel phase when a
            // pending animation resumes. The pocket, stake, IDs and all
            // financial/table metadata must retain their committed values.
            record.hasWheelStopIndex = false;
            record.wheelStopIndex = 0;
            return JsonUtility.ToJson(record);
        }

        private IEnumerator StateBetCleanupWaitForWin(long returned)
        {
            float deadline = Time.realtimeSinceStartup + 4f;
            while (VisibleWinningPayId() < 0 && Time.realtimeSinceStartup < deadline) yield return null;
            RefreshPortraitUi();
            RequireStateBetCleanup(VisibleWinningPayId() == 127 && ActiveWinningHighlights() == 1 &&
                ActiveWinningPayoutGlyphs() == 2 && uiTotalWinTitle.gameObject.activeSelf &&
                uiTotalWinAmount.Value == "(" + returned + ")" &&
                uiLinePays.Value == "Straight Up Pays " + returned &&
                uiGamePays.Value == "Game Pays " + returned && DisplayedPaid == returned &&
                uiPaid.Value == returned.ToString() && uiCashPaid.Visible &&
                uiTableNumberMark.gameObject.activeSelf && uiBall.gameObject.activeSelf,
                "actual win was not visible");
        }

        private int StateBetCleanupRecoveryBranches(StateBetCleanupReceipt proof)
        {
            try
            {
                PlayerPrefs.DeleteKey(GameResultsClearedKey);
                PlayerPrefs.Save();
                LoadProfile();
                RequireStateBetCleanupCleared(false);
                RequireStateBetCleanup(sourceBetsChangedSincePlay &&
                    DemoCleanupEconomy() == proof.restoredEconomy && DemoCleanupArchive(false) == proof.archive &&
                    DemoCleanupRngBytes() == proof.rng && StateBetCleanupUndoFingerprint() == proof.undo &&
                    PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared,
                    "durable changed-table state did not reconstruct a missing presentation record");

                var legacy = JsonUtility.FromJson<StagedBetRecord>(proof.staged);
                RequireStateBetCleanup(legacy != null && legacy.hasSourceTableEditState && legacy.sourceTableEdited,
                    "direct fixture lacks a durable source table edit");
                legacy.hasSourceTableEditState = false;
                legacy.sourceTableEdited = false;
                PlayerPrefs.SetString(StagedBetKey, JsonUtility.ToJson(legacy));
                PlayerPrefs.DeleteKey(GameResultsClearedKey);
                PlayerPrefs.Save();
                LoadProfile();
                RequireStateBetCleanupCleared(false);
                RequireStateBetCleanup(sourceBetsChangedSincePlay &&
                    DemoCleanupEconomy() == proof.restoredEconomy && DemoCleanupArchive(false) == proof.archive &&
                    DemoCleanupRngBytes() == proof.rng && StateBetCleanupUndoFingerprint() == proof.undo &&
                    PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared,
                    "legacy changed chip stacks did not reconstruct suppressed result presentation");
            }
            finally
            {
                PlayerPrefs.SetString(StagedBetKey, proof.staged);
                PlayerPrefs.SetString(GameResultsClearedKey, proof.cleared);
                PlayerPrefs.Save();
                LoadProfile();
            }
            RequireStateBetCleanup(sourceBetsChangedSincePlay && DemoCleanupEconomy() == proof.restoredEconomy &&
                DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                StateBetCleanupUndoFingerprint() == proof.undo &&
                PlayerPrefs.GetString(StagedBetKey, "") == proof.staged &&
                PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared,
                "recovery branches changed the saved table or retained legacy test metadata");
            return 2;
        }

        private int StateBetCleanupOptionalUndoIsolation(StateBetCleanupReceipt proof)
        {
            string original = PlayerPrefs.GetString(SettledTableUndoKey, "");
            string pending = PlayerPrefs.GetString(PendingSpinKey, "");
            var invalid = JsonUtility.FromJson<SettledTableUndoRecord>(original);
            RequireStateBetCleanup(invalid != null && invalid.tableUndo != null && invalid.tableUndo.Length == 1,
                "optional Undo isolation needs the settled one-operation sidecar");
            invalid.tableUndo[0].sourceSpotPlusOne = session.Profile.Records.Count + 1;
            string corrupt = JsonUtility.ToJson(invalid);
            try
            {
                PlayerPrefs.SetString(SettledTableUndoKey, corrupt);
                PlayerPrefs.Save();
                LoadProfile();
                RequireStateBetCleanupCleared(true);
                RequireStateBetCleanup(!session.CanUndo && session.LastUndoSourceSpot == -1 &&
                    ((RouletteSession)session).LastTouchedBet == -1 &&
                    ((RouletteSession)session).ExportUndoHistory().Length == 0 &&
                    !sourceBetsChangedSincePlay && session.ResultPhase &&
                    DemoCleanupEconomy() == proof.restoredEconomy && DemoCleanupArchive(false) == proof.archive &&
                    DemoCleanupRngBytes() == proof.rng &&
                    PlayerPrefs.GetString(GameResultsClearedKey, "") == proof.cleared &&
                    PlayerPrefs.GetString(StagedBetKey, "") == proof.staged &&
                    PlayerPrefs.GetString(PendingSpinKey, "") == pending &&
                    PlayerPrefs.GetString(SettledTableUndoKey, "") == corrupt,
                    "invalid optional Undo altered valid money/result/journals/RNG or was not isolated");
            }
            finally
            {
                PlayerPrefs.SetString(SettledTableUndoKey, original);
                PlayerPrefs.Save();
                LoadProfile();
            }
            RequireStateBetCleanup(session.CanUndo == proof.canUndo &&
                StateBetCleanupUndoFingerprint() == proof.undo && DemoCleanupEconomy() == proof.restoredEconomy &&
                DemoCleanupArchive(false) == proof.archive && DemoCleanupRngBytes() == proof.rng &&
                PlayerPrefs.GetString(SettledTableUndoKey, "") == original &&
                PlayerPrefs.GetString(PendingSpinKey, "") == pending,
                "optional Undo isolation did not restore the original valid metadata");
            return 1;
        }

        private string StateBetCleanupRestoredEconomy()
        {
            // Native NV restoration rebuilds the active bet list in ID order;
            // all other economy/history fields must remain byte-identical.
            string[] fields = DemoCleanupEconomy().Split('|');
            RequireStateBetCleanup(fields.Length == 9, "shared economy fingerprint shape changed");
            fields[6] = string.Join(",", session.Bets.Keys.OrderBy(id => id)
                .Select(id => id.ToString()).ToArray());
            return string.Join("|", fields);
        }

        private string StateBetCleanupUndoFingerprint()
        {
            var local = (RouletteSession)session;
            return JsonUtility.ToJson(new StateBetCleanupUndoSnapshot {
                lastTouched = local.LastTouchedBet,
                entries = local.ExportUndoHistory().Select(saved => new RecordedUndo {
                    lastTouched = saved.LastTouched,
                    sourceSpotPlusOne = saved.SourceSpotPlusOne,
                    playsRemovalSound = saved.PlaysRemovalSound,
                    evaluationOrder = saved.Order,
                    groupRemovalOrder = saved.GroupRemovalOrder,
                    bets = EncodeStagedBets(saved.Stacks)
                }).ToArray()
            });
        }

        private void RequireStateBetCleanupUndoState(string testCase)
        {
            var local = (RouletteSession)session;
            RouletteSession.SavedUndoState[] queue = local.ExportUndoHistory();
            int count = testCase == "clear" ? 0 : testCase == "direct" ||
                testCase == "neighbor" || testCase == "sector" ? 2 : 1;
            int lastSource = testCase == "sector" ? 133 : count == 0 ? -1 : 27;
            RequireStateBetCleanup(queue.Length == count && session.CanUndo == (count != 0) &&
                session.LastUndoSourceSpot == lastSource &&
                (count == 0 ? local.LastTouchedBet == -1 : queue[0].SourceSpotPlusOne == 28 &&
                    queue[0].LastTouched == -1 && queue[0].Stacks.Count == 0 &&
                    queue[0].PlaysRemovalSound), "action lost source spot or prior undo entries");
            if (testCase == "undo")
                RequireStateBetCleanup(local.LastTouchedBet == 127 && queue[0].Order.Length == 0 &&
                    selectedNeighbor == 27, "Undo did not retain the first operation and its neighbor source");
        }

        private void StateBetCleanupReplayRestoredUndo(StateBetCleanupReceipt proof)
        {
            long credit = session.Credit, wagered = session.Wagered, returned = session.Returned;
            long rounds = session.Rounds;
            int wins = session.Wins;
            string result = DemoCleanupResult(session.LastResult);
            RefreshPortraitUi();
            RequireStateBetCleanup(uiUndoButton.interactable && session.CanUndo &&
                session.LastUndoSourceSpot == 27 && ((RouletteSession)session).ExportUndoHistory().Length == 1,
                "restored table did not expose its remaining source Undo operation");
            PointerEventData pointer = DemoCleanupQueue(uiUndoButton);
            RequireStateBetCleanup(SourcePlayerInputs.DispatchOne() &&
                DemoCleanupEconomy() == proof.restoredEconomy && StateBetCleanupUndoFingerprint() == proof.undo,
                "restored Undo changed the table before release");
            DemoCleanupDrainRelease(pointer);
            RequireStateBetCleanupCleared(false);
            RequireStateBetCleanup(sourceBetsChangedSincePlay && !session.ResultPhase &&
                session.TotalBet == 0 && session.Bets.Count == 0 && !session.CanUndo &&
                ((RouletteSession)session).ExportUndoHistory().Length == 0 &&
                ((RouletteSession)session).LastTouchedBet == -1 && session.LastUndoSourceSpot == -1 &&
                selectedNeighbor == -1 && session.Credit == credit && session.Wagered == wagered &&
                session.Returned == returned && session.Rounds == rounds && session.Wins == wins &&
                DemoCleanupResult(session.LastResult) == result && DemoCleanupArchive(false) == proof.archive &&
                DemoCleanupRngBytes() == proof.rng && !spinning && pendingSpin == null,
                "actual restored Undo changed settlement/archive/RNG or failed source edit state");
            proof.afterReloadUndo = true;
            proof.economy = DemoCleanupEconomy();
            proof.restoredEconomy = StateBetCleanupRestoredEconomy();
            proof.undo = StateBetCleanupUndoFingerprint();
            proof.staged = PlayerPrefs.GetString(StagedBetKey, "");
            proof.cleared = PlayerPrefs.GetString(GameResultsClearedKey, "");
            proof.resultPhase = false;
            proof.canUndo = false;
            var staged = JsonUtility.FromJson<StagedBetRecord>(proof.staged);
            RequireStateBetCleanup(staged != null && staged.hasSourceTableEditState && staged.sourceTableEdited &&
                staged.hasUndoState && staged.undo.Length == 0 && staged.lastTouched == -1 &&
                staged.bets.Length == 0 && !string.IsNullOrEmpty(proof.cleared),
                "restored Undo did not durably save its empty table and exhausted queue");
            PlayerPrefs.SetString(StateBetCleanupReceiptKey, JsonUtility.ToJson(proof));
            PlayerPrefs.Save();
            StateBetCleanupParkPointer();
            CaptureStateBetCleanup("after_reload_undo");
        }

        private void RequireStateBetCleanupCleared(bool restoredMarker)
        {
            RefreshPortraitUi();
            bool showZero = auNationalStandardsEnabled;
            RequireStateBetCleanup(currentGameResultsCleared && !winRackActive &&
                resultCycleStartedTimestamp == 0 && VisibleWinningPayId() == -1 &&
                ActiveWinningHighlights() == 0 && ActiveWinningPayoutGlyphs() == 0 &&
                !uiTotalWinTitle.gameObject.activeSelf && uiTotalWinAmount.Value == "" &&
                uiLinePays.Value == "" && uiGamePays.Value == "" && DisplayedPaid == 0 &&
                uiPaid.Value == (showZero ? "0" : "") && uiCashPaid.Visible == showZero &&
                uiCashPaidBackground.gameObject.activeSelf == showZero &&
                uiCashPaid.Value == (showZero ? RouletteDenominationFormatter.ForFeatureCurrency(0) : "") &&
                uiTableNumberMark.gameObject.activeSelf == restoredMarker &&
                (!restoredMarker || tableMarkedPocket == 27) && uiBall.gameObject.activeSelf &&
                session.DoubleEnabled && !session.DoubleAvailable && session.DoubleStake == 0 &&
                !session.InDoubleUp && !DoubleUpInputAllowed &&
                recordedRoundPages.Count == 1 && !recordedRoundPages[0].awaitingFinalPayout,
                "cleared cycle/paid/marker/offer presentation differs");
            if (auNationalStandardsEnabled)
                RequireStateBetCleanup(CurrentFooterMovieFrame() == null && !uiFooterMovie.gameObject.activeSelf,
                    "AU cleared result kept the footer movie visible beside WIN 0");
        }

        private void RequireStateBetCleanupBets(string testCase)
        {
            var expected = new Dictionary<int, long>();
            if (testCase != "clear") expected.Add(127, 1);
            if (testCase == "direct") expected[127] = 2;
            if (testCase == "neighbor")
                foreach (int pocket in new[] { 34, 6, 13, 36 }) expected.Add(100 + pocket, 1);
            if (testCase == "sector")
                foreach (int spot in OriginalSectorBets.Spots(RouletteSector.Voisins))
                {
                    int id = 100 + spot;
                    expected[id] = expected.ContainsKey(id) ? expected[id] + 1 : 1;
                }
            RequireStateBetCleanup(session.Bets.Count == expected.Count &&
                session.TotalBet == expected.Values.Sum() && expected.All(pair =>
                    session.Bets.ContainsKey(pair.Key) && session.Bets[pair.Key] == pair.Value) &&
                session.BetChipStacks.All(pair => pair.Value.All(chip => chip == 1)),
                "case changed the wrong bet spots or chip stacks");
        }

        private Vector2 StateBetCleanupNeutralPoint()
        {
            Rect bounds = portraitRoot.rect;
            return RectTransformUtility.WorldToScreenPoint(null,
                portraitRoot.TransformPoint(new Vector3(bounds.xMin + 1065f,
                    bounds.yMax - 100f, 0f)));
        }

        private PointerEventData StateBetCleanupHoldPreview(int spot, int pointerId, bool touch)
        {
            Canvas.ForceUpdateCanvases();
            Button target = uiBetButtons[spot];
            Vector2 center = RectTransformUtility.WorldToScreenPoint(null,
                target.transform.TransformPoint(((RectTransform)target.transform).rect.center));
            PointerEventData pointer = sourceInputModule.QueuePointerSample(pointerId,
                center, true, false, true, touch);
            RequireStateBetCleanup(SourcePlayerInputs.DispatchOne(), "preview down was missing");
            return pointer;
        }

        private void StateBetCleanupReleasePreview(int pointerId, bool touch)
        {
            PointerEventData pointer = sourceInputModule.QueuePointerSample(pointerId,
                StateBetCleanupNeutralPoint(), false, true, false, touch);
            RequireStateBetCleanup(SourcePlayerInputs.DispatchOne(), "preview release was missing");
            RequireSourcePointerReleased(pointer);
            RequireStateBetCleanup(SourcePlayerInputs.Count == 0, "preview left queued input");
        }

        private void StateBetCleanupPreview(int spot, bool valid)
        {
            long before = session.TotalBet;
            StateBetCleanupHoldPreview(spot, PointerInputModule.kMouseLeftId, false);
            RequireStateBetCleanup((selectedNeighbor == spot) == valid &&
                !currentGameResultsCleared && !session.DoubleAvailable && session.TotalBet == before,
                "preview validation changed the live win or selected the wrong neighbor");
            StateBetCleanupReleasePreview(PointerInputModule.kMouseLeftId, false);
        }

        private void StateBetCleanupParkPointer()
        {
            Vector2 neutral = StateBetCleanupNeutralPoint();
            var probe = new PointerEventData(EventSystem.current) { position = neutral };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(probe, hits);
            RequireStateBetCleanup(hits.Count == 0 || hits.All(hit =>
                hit.gameObject.GetComponentInParent<Button>() == null &&
                hit.gameObject.GetComponentInParent<RouletteTableRelease>() == null),
                "neutral pointer position overlaps a control");
            sourceInputModule.QueuePointerSample(PointerInputModule.kMouseLeftId,
                neutral, true, false, true, false);
            PointerEventData pointer = sourceInputModule.QueuePointerSample(PointerInputModule.kMouseLeftId,
                neutral, false, true, false, false);
            RequireStateBetCleanup(SourcePlayerInputs.DispatchOne(), "neutral down was missing");
            DemoCleanupDrainRelease(pointer);
            RequireStateBetCleanup(!uiBetMarker.gameObject.activeSelf,
                "neutral release retained a hover-preview chip");
        }

        private void CaptureStateBetCleanup(string suffix)
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-rouletteStateBetCleanupCapturePrefix");
            if (at < 0) return;
            RequireStateBetCleanup(at + 1 < args.Length, "capture prefix missing");
            string path = args[at + 1] + "_" + suffix + ".png";
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            CaptureActiveScreenOffscreen(path);
        }
    }
}
