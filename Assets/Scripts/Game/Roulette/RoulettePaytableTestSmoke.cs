using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private sealed class PaytableTestForbiddenOutcome : IRouletteOutcomeSource
        {
            public int Requests;
            public int DrawPocket() { ++Requests; throw new InvalidOperationException("Test UI reached live pocket RNG"); }
            public int DrawDoubleSuit() { ++Requests; throw new InvalidOperationException("Test UI reached live feature RNG"); }
        }

        private static void RequirePaytableTest(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("Paytable Test UI: " + message);
        }

        private string PaytableTestLiveFingerprint(IRouletteSession state)
        {
            var local = state as RouletteSession;
            RequirePaytableTest(local != null, "PC fixture requires a local session");
            string funds = string.Join("/", new[] {state.Credit, state.Denomination,
                local.MoneyRemainderCents, state.Chip, state.TotalBet, state.Wagered,
                state.Returned, state.Rounds, state.Wins, state.History.Count,
                state.DoubleStake, state.DoubleRounds}.Select(n => n.ToString()).ToArray());
            string wagers = string.Join(";", state.Bets.OrderBy(b => b.Key).Select(b => b.Key + "=" + b.Value).ToArray());
            string stacks = string.Join(";", state.BetChipStacks.OrderBy(b => b.Key).Select(b =>
                b.Key + "=" + string.Join(",", b.Value.Select(n => n.ToString()).ToArray())).ToArray());
            string history = string.Join(";", state.History.Select(r => JsonUtility.ToJson(r)).ToArray());
            string recorded = JsonUtility.ToJson(new RecordedRoundArchive { rounds = recordedRoundPages.ToArray() });
            string prefs = string.Join(";", new[] {recordedHistoryKey, recordedHistoryKey + ".Pages",
                recordedHistoryKey + ".RoundDetails.v1", PendingSpinKey, StagedBetKey, PcCreditKey,
                NeighborSelectionKey}.Select(k => k + "=" + PlayerPrefs.GetString(k, "<absent>")).ToArray());
            return funds + "|" + wagers + "|" + stacks + "|" + history + "|" + recorded + "|" + prefs +
                "|" + state.SpinPending + "/" + state.ResultPhase + "/" + state.InDoubleUp + "/" + state.CanUndo;
        }

        private IEnumerator PaytableTestSmoke()
        {
            yield return null;
            IRouletteSession savedSession = session;
            string savedState = PaytableTestLiveFingerprint(savedSession);
            var noDraw = new PaytableTestForbiddenOutcome();
            session = new RouletteSession(RouletteProfile.Load("97"), 1000, noDraw) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            string liveBefore = PaytableTestLiveFingerprint(session);
            string[] args = Environment.GetCommandLineArgs();
            int captureOption = Array.IndexOf(args, "-roulettePaytableTestCaptureDirectory");
            string directory = captureOption < 0 || captureOption+1 >= args.Length ? null : args[captureOption+1];
            try
            {
                RequirePaytableTest(RequestPcPaytableTest("89", 100, 20, 0, new Rect(0, 0, 1, 1)), "Entry rejected idle game");
                yield return null;
                RequirePaytableTest(paytableTest.Pocket == 31 && paytableTest.TotalBet == 0 &&
                    paytableTestMessages[0].Value == "" && paytableTestMessages[2].Value == "" &&
                    paytableTestBall.rectTransform.anchoredPosition == new Vector2(664, -354), "Initial wheel/empty labels/ball coordinate");
                RequirePaytableTest(paytableTestButtons[0].interactable && paytableTestButtons[1].interactable &&
                    paytableTestButtons[5].interactable && !paytableTestButtons[2].interactable, "0x23 button mask");
                RequirePaytableTest(Mathf.Abs(Mathf.DeltaAngle(paytableTestWheel.rectTransform.localEulerAngles.z,
                    -paytableTest.MainRenderAngle)) < .0001f &&
                    paytableTestWheel.rectTransform.anchoredPosition == new Vector2(472, -383),
                    "Source y-down rotation and integer wheel pivot");
                if (directory != null) { Directory.CreateDirectory(directory); CaptureActiveScreenOffscreen(Path.Combine(directory, "initial.png")); }

                // The actual source table route applies the release target.
                var target = paytableTestScreen.GetComponentsInChildren<RouletteTableRelease>()
                    .First(t => t.SpotIndex == 31).GetComponent<UnityEngine.UI.Button>();
                PointerEventData betPress = PressThroughUiRaycast(target);
                Rect betBounds = originalLayout.bet_spots[31].ToRect();
                RequirePaytableTest(paytableTestMarker.gameObject.activeSelf &&
                    paytableTestMarker.texture == originalMainChipButtonFrames[0][0] &&
                    paytableTestMarker.rectTransform.anchoredPosition == new Vector2(
                        betBounds.x+(betBounds.width-90)*.5f, -betBounds.y-(betBounds.height-90)*.5f),
                    "Selected chip marker/down/source center");
                ReleaseUiPointer(target, betPress);
                RequirePaytableTest(!paytableTestMarker.gameObject.activeSelf, "Release must hide test marker");
                RequirePaytableTest(paytableTest.TotalBet == 1 && paytableTest.Bets[131] == 1 &&
                    paytableTestMessages[0].Value == "Total Bet = 1", "Test table release wager");
                var evaluate = paytableTestButtons[1];
                Vector2 neutral = paytableTestButtonLabels[1].Root.anchoredPosition;
                PointerEventData held = PressThroughUiRaycast(evaluate);
                RequirePaytableTest(paytableTest.Result != null && paytableTest.Result.Returned == 32,
                    "Evaluate/down payout=" + (paytableTest.Result == null ? "null" : paytableTest.Result.Returned.ToString()));
                RequirePaytableTest(paytableTestMessages[2].Value == "Game Pays = 32",
                    "Evaluate message=" + paytableTestMessages[2].Value);
                RequirePaytableTest(paytableTestButtonLabels[1].Root.anchoredPosition == neutral + new Vector2(1, -1),
                    "Evaluate label=" + paytableTestButtonLabels[1].Root.anchoredPosition + " neutral=" + neutral);
                RoundResult firstResult = paytableTest.Result;
                ReleaseUiPointer(evaluate, held);
                RequirePaytableTest(ReferenceEquals(firstResult, paytableTest.Result) &&
                    paytableTestButtonLabels[1].Root.anchoredPosition == neutral, "Release ran a second evaluation or kept label depressed");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "evaluated.png"));

                held = PressThroughUiRaycast(paytableTestButtons[5]);
                RequirePaytableTest(paytableTest.Pocket == 14 && paytableTest.Result == null &&
                    paytableTestMessages[2].Value == "", "NUDGE/down did not clear evaluation");
                ExecuteEvents.Execute(paytableTestButtons[5].gameObject, held, ExecuteEvents.initializePotentialDrag);
                RequirePaytableTest(!held.useDragThreshold, "Source capture should not use drag threshold");
                ExecuteEvents.Execute(paytableTestButtons[5].gameObject, held, ExecuteEvents.dragHandler);
                RequirePaytableTest(paytableTest.Pocket == 20, "Captured NUDGE movement");
                ReleaseUiPointer(paytableTestButtons[5], held);
                RequirePaytableTest(paytableTest.Pocket == 20, "NUDGE release executed");

                int navigationSounds = paysClickCount;
                ClickThroughUiRaycast(paytableTestButtons[0]);
                RequirePaytableTest(paytableTestPaysPage == 0 && paytableTestButtons[2].interactable &&
                    paytableTestButtons[3].interactable && paytableTestButtons[4].interactable &&
                    !paytableTestButtons[0].interactable && !paytableTestButtons[5].interactable, "0x1c Pays mask");
                RequirePaytableTest(paytableTestButtons[2].GetComponent<UnityEngine.UI.RawImage>().texture ==
                    Resources.Load<Texture2D>("Original/PaytableTest/Button_1") &&
                    !paytableTestPaysLines.gameObject.activeSelf,
                    "Shown Return needs its enabled source frame immediately; page 1 has no pay lines");
                ClickThroughUiRaycast(paytableTestButtons[3]);
                RequirePaytableTest(paytableTestPaysPage == 0 && paysClickCount == navigationSounds,
                    "Previous first-page bound must not dispatch a navigation sound");
                held = PressThroughUiRaycast(paytableTestButtons[4]);
                RequirePaytableTest(paytableTestPaysPage == 1, "Pays Next/down");
                ExecuteEvents.Execute(paytableTestButtons[4].gameObject, held, ExecuteEvents.dragHandler);
                RequirePaytableTest(paytableTestPaysPage == 2, "Pays Next/captured move");
                ReleaseUiPointer(paytableTestButtons[4], held);
                RequirePaytableTest(paytableTestPaysPage == 2 && paysClickCount == navigationSounds+2,
                    "Pays Next/release must not repeat sound");
                ClickThroughUiRaycast(paytableTestButtons[4]);
                ClickThroughUiRaycast(paytableTestButtons[4]);
                RequirePaytableTest(paytableTestPaysPage == 3 && paysClickCount == navigationSounds+3,
                    "Next last-page bound must not dispatch a navigation sound");
                RequirePaytableTest(paytableTestPaysLines.gameObject.activeSelf &&
                    paytableTestPaysLines.texture == paysLinePages[0] &&
                    paytableTestPays.rectTransform.sizeDelta == new Vector2(1080, 1054),
                    "Final Pays page must show the selected test profile's pay lines at native size");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "pays.png"));
                ClickThroughUiRaycast(paytableTestButtons[2]);
                RequirePaytableTest(paytableTestPaysPage == -1 && paytableTestButtons[5].interactable &&
                    paysClickCount == navigationSounds+4 &&
                    paytableTestButtonLabels[0].Root.anchoredPosition == Vector2.zero &&
                    paytableTestButtonLabels[2].Root.anchoredPosition == Vector2.zero,
                    "Return to test and hidden captured-button release restoration");
                ClickThroughUiRaycast(paytableTestButtons[0]);
                paytableTestPaysDeadlineTimestamp = System.Diagnostics.Stopwatch.GetTimestamp()-1;
                RefreshPaytableTestUi();
                RequirePaytableTest(PaytableTestActive && paytableTestPaysPage == -1 &&
                    paysClickCount == navigationSounds+5 && paytableTestButtons[5].interactable,
                    "Pays timer must return to the test table once with the source sound");
                ClickThroughUiRaycast(paytableTestClear);
                RequirePaytableTest(paytableTest.TotalBet == 0 && paytableTest.Result == null &&
                    !paytableTestClear.interactable && paytableTestMessages[0].Value == "Total Bet = 0" &&
                    paytableTestMessages[2].Value == "", "Clear source labels and disabled state");
                Bet(131); BeginSpin();
                RequirePaytableTest(noDraw.Requests == 0 && PaytableTestLiveFingerprint(session) == liveBefore,
                    "Test actions changed live funds, bets, counters, history, persistence or requested RNG");
                RequirePaytableTest(!RequestPcPaytableTest("97", 0, 0, 0, new Rect(0, 0, 1, 1)), "Nested test entry");
                ClosePcPaytableTest();
                RequirePaytableTest(PaytableTestLiveFingerprint(session) == liveBefore, "Exit changed live state");

                session = new RouletteSession(RouletteProfile.Load("97"), 0, noDraw);
                string emptyBefore = PaytableTestLiveFingerprint(session);
                RequirePaytableTest(RequestPcPaytableTest("97", 0, 0, 1, new Rect(.05f, .05f, .9f, .9f)), "Diagnostics incorrectly require live funds");
                yield return null;
                target = paytableTestScreen.GetComponentsInChildren<RouletteTableRelease>()
                    .First(t => t.SpotIndex == 31).GetComponent<UnityEngine.UI.Button>();
                ClickThroughUiRaycast(target);
                ClickThroughUiRaycast(paytableTestButtons[1]);
                RequirePaytableTest(paytableTest.Result.Returned == 1 && paytableTest.Result.WinningPays[0].Amount == 36 &&
                    PaytableTestLiveFingerprint(session) == emptyBefore && noDraw.Requests == 0,
                    "Zero-credit diagnostics/capped total/full individual pay/isolation");
                if (directory != null) CaptureActiveScreenOffscreen(Path.Combine(directory, "scaled_capped.png"));
                ClosePcPaytableTest();
                RequirePaytableTest(PaytableTestLiveFingerprint(savedSession) == savedState, "Saved live session changed");
                Debug.Log("ROULETTE_PAYTABLE_TEST_UI_PASSED: down/captured-move/no-release; 0x23/0x1c; marker/table-release; native Pays/pay lines/sound/timeout; clear; zero-credit; cap; rectangle; live/persistence/RNG isolation");
            }
            finally
            {
                ClosePcPaytableTest();
                session = savedSession;
                RefreshPortraitUi();
            }
            yield return null;
            Application.Quit();
        }
    }
}
