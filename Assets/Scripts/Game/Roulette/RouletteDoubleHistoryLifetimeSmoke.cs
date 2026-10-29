using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private sealed class DoubleLifetimeOutcome : IRouletteOutcomeSource
        {
            private int draws;
            public int DrawPocket()
            {
                if (++draws > 2)
                    throw new InvalidOperationException("Unexpected lifetime fixture pocket draw");
                return draws == 1 ? 0 : 1;
            }
            public int DrawDoubleSuit() { return 3; }
        }

        private IEnumerator DoubleHistoryLifetimeSmoke(bool load)
        {
            yield return null;
            if (!recordedHistoryKey.EndsWith(".Test.DoubleLifetimeTest", StringComparison.Ordinal))
                throw new InvalidOperationException("Double history lifetime needs its isolated test key");
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-rouletteDoubleHistoryLifetimeCapture");
            string directory = option >= 0 && option + 1 < args.Length ? args[option + 1] : null;
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            if (!load)
            {
                temporaryHistory = false;
                recordedPockets.Clear(); recordedSnapshots.Clear(); recordedRoundPages.Clear();
                ++recordedHistoryVersion;
                outcome = new DoubleLifetimeOutcome();
                session = new RouletteSession(RouletteProfile.Load("97"), 1000, outcome)
                    { DoubleEnabled = true };
                session.PlaceBet(100, 1);
                RefreshPortraitUi();
                yield return null;
                ClickThroughUiRaycast(uiSpinButton);
                if (!spinning || !session.SpinPending)
                    throw new InvalidOperationException("Lifetime first PLAY raycast did not start a spin");
                float deadline = Time.realtimeSinceStartup + 40f;
                while ((spinning || ResultDisplayActive) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                if (spinning || ResultDisplayActive || !session.DoubleAvailable ||
                    session.LastResult == null || session.LastResult.Returned != 36 ||
                    recordedRoundPages.Count != 1)
                    throw new InvalidOperationException("Lifetime first round did not reach its Double offer");

                // BeginDouble is the same entry used by the PC D key/iDeck.
                BeginDouble();
                deadline = Time.realtimeSinceStartup + 8f;
                while (!DoubleDecisionReady && Time.realtimeSinceStartup < deadline)
                    yield return null;
                RefreshPortraitUi();
                yield return null;
                if (!DoubleDecisionReady)
                    throw new InvalidOperationException("Lifetime feature entrance did not finish");
                ClickThroughUiRaycast(uiDoubleRedButton);
                if (!doubleRevealWon || session.DoubleStake != 72)
                    throw new InvalidOperationException("Lifetime red touch did not win 72");
                deadline = Time.realtimeSinceStartup + 8f;
                while (IsDoubleReveal && Time.realtimeSinceStartup < deadline) yield return null;
                RefreshPortraitUi();
                yield return null;
                if (!DoubleDecisionReady || IsDoubleReveal)
                    throw new InvalidOperationException("Lifetime red reveal did not finish");
                ClickThroughUiRaycast(uiDoubleBlackButton);
                if (doubleRevealWon || session.InDoubleUp ||
                    recordedRoundPages[0].doubleUp == null ||
                    !recordedRoundPages[0].doubleUp.complete)
                    throw new InvalidOperationException("Lifetime black touch did not finish a feature loss");
                deadline = Time.realtimeSinceStartup + 8f;
                while ((IsDoubleReveal || IsDoubleExit) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                RefreshPortraitUi();
                yield return null;
                FinalizePendingHistoryRound();
                if (IsDoubleReveal || IsDoubleExit || session.Credit != 999 ||
                    recordedRoundPages[0].awaitingFinalPayout ||
                    recordedRoundPages[0].doubleUp.steps.Length != 2)
                    throw new InvalidOperationException("Lifetime feature round did not commit");

                // Preserve the same session and wager. This ordinary losing
                // round must not inherit the preceding round's feature flag.
                ClickThroughUiRaycast(uiSpinButton);
                if (!spinning || !session.SpinPending || session.Credit != 998)
                    throw new InvalidOperationException("Lifetime next PLAY did not commit exactly one credit");
                deadline = Time.realtimeSinceStartup + 40f;
                while ((spinning || ResultDisplayActive) && Time.realtimeSinceStartup < deadline)
                    yield return null;
                FinalizePendingHistoryRound();
                if (spinning || ResultDisplayActive)
                    throw new InvalidOperationException("Lifetime next ordinary spin did not finish");
            }

            if (recordedRoundPages.Count != 2 || recordedPockets.Count != 2 ||
                recordedRoundPages[0].pocket != 1 || recordedRoundPages[0].returned != 0 ||
                recordedRoundPages[0].doubleUp != null || recordedRoundPages[0].hasDoubleUp ||
                recordedRoundPages[0].awaitingFinalPayout ||
                recordedRoundPages[1].pocket != 0 || recordedRoundPages[1].returned != 0 ||
                recordedRoundPages[1].doubleUp == null || !recordedRoundPages[1].hasDoubleUp ||
                !recordedRoundPages[1].doubleUp.complete ||
                recordedRoundPages[1].doubleUp.steps.Length != 2 ||
                recordedRoundPages[1].awaitingFinalPayout || session.Credit != 998)
                throw new InvalidOperationException("Feature eligibility leaked or was lost across round history");

            SetHistoryView(true);
            RefreshPortraitUi();
            yield return null;
            RefreshHistoryPageUi();
            if (historyPage != 0 || historyDoubleButton.gameObject.activeSelf)
                throw new InvalidOperationException("Ordinary next-round history exposed Double Up recall");
            CaptureDoubleLifetime(directory, load ? "loaded_latest" : "latest");
            ClickThroughUiRaycast(historyPrevious);
            yield return null;
            RefreshHistoryPageUi();
            if (historyPage != 1 || !historyDoubleButton.gameObject.activeSelf)
                throw new InvalidOperationException("Prior completed feature lost its history button");
            CaptureDoubleLifetime(directory, load ? "loaded_feature" : "feature");
            ClickThroughUiRaycast(historyDoubleButton);
            yield return null;
            if (!doubleHistoryView || !doubleHistoryRoot.gameObject.activeSelf)
                throw new InvalidOperationException("Prior feature did not reopen after an ordinary round");
            CaptureDoubleLifetime(directory, load ? "loaded_recall" : "recall");
            ClickThroughUiRaycast(doubleHistoryExitButton);
            yield return null;
            ClickThroughUiRaycast(historyNext);
            yield return null;
            RefreshHistoryPageUi();
            if (historyPage != 0 || historyDoubleButton.gameObject.activeSelf)
                throw new InvalidOperationException("Returning to the ordinary page retained the feature button");
            ClickThroughUiRaycast(historyGame);
            if (historyView || doubleHistoryView)
                throw new InvalidOperationException("Lifetime history Return did not exit");
            Debug.Log(load ? "ROULETTE_DOUBLE_HISTORY_LIFETIME_LOAD_PASSED: two persisted rounds, only older feature has recall, credit=998" :
                "ROULETTE_DOUBLE_HISTORY_LIFETIME_PASSED: actual PLAY/RED/BLACK/PLAY, feature then ordinary loss, prior recall retained, credit=998");
            Application.Quit();
        }

        private void CaptureDoubleLifetime(string directory, string name)
        {
            if (!string.IsNullOrEmpty(directory))
                CaptureActiveScreenOffscreen(Path.Combine(directory, name + ".png"));
        }
    }
}
