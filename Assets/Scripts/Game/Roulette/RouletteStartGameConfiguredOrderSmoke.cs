using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private sealed class ConfiguredOrderOutcome : IRouletteOutcomeSource
        {
            public int Pockets, Suits;
            public Action ObservePocket;
            public int DrawPocket()
            {
                ++Pockets;
                if (ObservePocket != null) ObservePocket();
                return 16;
            }
            public int DrawDoubleSuit()
            {
                ++Suits;
                throw new InvalidOperationException("Configured order fixture reached feature RNG");
            }
        }

        private static void RequireConfiguredOrder(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException("StartGame configured order: " + message);
        }

        // Deliberately a call-level state-entry fixture: configuration and its
        // pending Idle entry are installed directly, and PlaceBet prepares its
        // one wager. Draw and settlement use production code. Pays/Clear use
        // actual uGUI raycasts; monetary transitions use the real PC journals.
        private IEnumerator StartGameConfiguredOrderSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            RequireConfiguredOrder(demoShowMode == 1 && demoMenuState == 3 &&
                sourceInputModule != null && demoOutcome != null && session != null &&
                !temporaryHistory && !autoplay && autoRoutine == null && !spinning &&
                !session.SpinPending && !session.InDoubleUp && !session.ResultPhase &&
                !ResultDisplayActive && !pcCollectPending && pendingSpin == null &&
                session.Credit == 1000 && session.Denomination == 1 && session.TotalBet == 0 &&
                session.Rounds == 0 && recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                ReadPcCreditEvent() == null && SourcePlayerInputs.Count == 0 &&
                sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending &&
                !configuredAutoPlayCheckPending && configuredAutoPlay.AutoPlay == 0 &&
                recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) >= 0 &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith(
                    "roulette_platform_rng_test_", StringComparison.Ordinal) &&
                Array.IndexOf(args, "-rouletteVisualVariant") < 0,
                "requires unused isolated history/volume/RNG, Demo1 and fresh Idle session");

            bool moduleEnabled = sourceInputModule.enabled;
            IRouletteOutcomeSource originalOutcome = outcome;
            DemoPocketOutcomeSource originalDemo = demoOutcome;
            OriginalAutoPlayData originalConfiguration = configuredAutoPlay;
            var counted = new ConfiguredOrderOutcome();
            string liveRngBefore = DemoCleanupRngBytes();
            bool drawObserved = false;
            try
            {
                sourceInputModule.enabled = false;
                demoOutcome = new DemoPocketOutcomeSource(counted);
                outcome = demoOutcome;
                session = new RouletteSession(session.Profile, 1000, demoOutcome) {
                    DoubleEnabled = false };
                configuredAutoPlay = new OriginalAutoPlayData();
                configuredAutoPlayFinishPending = false;
                pcPlayerTransactionActive = true;
                Debug.Log("ROULETTE_START_GAME_CONFIGURED_ORDER_FIXTURE: " +
                    "entry=call-level wagerSetup=model playerReachabilityClaim=false " +
                    "funding=false activeAuto=false");

                // PlayerInput=false retains configured Auto through the
                // deliberately queued non-release input in this entry fixture.
                configuredAutoPlay.SetOptions(true, false, false, false, true, 0);
                string emptyEconomy = DemoCleanupEconomy();
                string emptyArchive = DemoCleanupArchive(false);
                configuredAutoPlayCheckPending = true;
                ServiceConfiguredAutoPlay();
                RequireConfiguredOrder(!configuredAutoPlayCheckPending &&
                    configuredAutoPlay.AutoPlay == 1 && !configuredAutoPlay.ReplenishCredits &&
                    sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending &&
                    demoMenuState == 3 && !demoMenuVisible && !sourceTableInputEnabled &&
                    currentGameResultsCleared && !spinning && !session.SpinPending &&
                    pendingSpin == null && counted.Pockets == 0 && counted.Suits == 0 &&
                    DemoCleanupEconomy() == emptyEconomy && DemoCleanupArchive(false) == emptyArchive &&
                    ReadPcCreditEvent() == null && DemoCleanupRngBytes() == liveRngBefore,
                    "configured zero-wager refusal must finish Idle Enable without funding/draw/state change");

                // Preparing a model wager does not pretend a locked table
                // accepted a real click, nor does it force an extra Bet entry.
                session.PlaceBet(100, 1); // zero straight; fixed16 loses
                SaveStagedBetsIfChanged();
                RequireConfiguredOrder(session.Credit == 1000 && session.TotalBet == 1 &&
                    sourceGameplayState == SourceGameplayState.Idle && counted.Pockets == 0,
                    "pre-service wager fixture debited or drew early");
                int cleanup = 0, dispatch = 0;
                SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease,
                    () => ++dispatch, () => ++cleanup);
                SourcePlayerInputs.Enqueue(0x200001, () => ++dispatch);
                counted.ObservePocket = () => {
                    RequireConfiguredOrder(counted.Pockets == 1 && counted.Suits == 0 &&
                        demoMenuState == 3 && !demoMenuVisible &&
                        sourceGameplayState == SourceGameplayState.Playing &&
                        !sourceBetStateEntryPending && !gameOverMessageLatched &&
                        !sourceBetsChangedSincePlay && !sourceTableInputEnabled &&
                        SourcePlayerInputs.Count == 0 && cleanup == 1 && dispatch == 0 &&
                        !autoplay && autoRoutine == null && configuredAutoPlay.AutoPlay == 1 &&
                        !configuredAutoPlay.ReplenishCredits && session.Credit == 999 &&
                        session.TotalBet == 1 && session.Rounds == 0 && !session.SpinPending &&
                        pendingSpin == null && ReadPcCreditEvent() == null,
                        "DrawPocket ran before Idle Enable/StatePlay, before debit, or with queued input");
                    // Model commits the stake before DrawPocket, matching the
                    // source StateStartGame -> StateDrawStops ledger order.
                    drawObserved = true;
                    Debug.Log("ROULETTE_START_GAME_CONFIGURED_DRAW_OBSERVED: " +
                        "demoState=3 sourceState=3 completed=false queue=0 credit=999");
                };
                configuredAutoPlayCheckPending = true;
                ServiceConfiguredAutoPlay();
                RequireConfiguredOrder(drawObserved && counted.Pockets == 1 &&
                    spinning && session.SpinPending && pendingSpin != null &&
                    !autoplay && !configuredAutoPlayCheckPending &&
                    !PlayerPrefs.HasKey(StagedBetKey),
                    "accepted configured entry did not commit exactly one pending spin");

                long stopAt = System.Diagnostics.Stopwatch.GetTimestamp() +
                    40L * System.Diagnostics.Stopwatch.Frequency;
                while (spinning && System.Diagnostics.Stopwatch.GetTimestamp() < stopAt)
                    yield return null;
                RequireConfiguredOrder(!spinning && !session.SpinPending && pendingSpin == null &&
                    session.Rounds == 1 && session.Wagered == 1 && session.Returned == 0 &&
                    session.Credit == 999 && session.LastResult != null && session.LastResult.Pocket == 16 &&
                    recordedRoundPages.Count == 1 && recordedPockets.Count == 1 &&
                    counted.Pockets == 1 && counted.Suits == 0 && configuredAutoPlayCheckPending &&
                    !autoplay && gameOverMessageLatched && !ResultDisplayActive,
                    "configured fixture did not naturally settle its single losing spin");

                // Consume the actual completion entry with Auto disabled before
                // continuing recovery checks; this prevents a second fixture spin.
                configuredAutoPlay.SetAutoPlay(0);
                ServiceConfiguredAutoPlay();
                RequireConfiguredOrder(sourceGameplayState == SourceGameplayState.Idle &&
                    !configuredAutoPlayCheckPending && sourceTableInputEnabled && demoMenuState == 3,
                    "actual completion entry did not restore ordinary Idle inputs");
                yield return ConfiguredOrderUiClick(uiClearButton);
                RequireConfiguredOrder(session.TotalBet == 0 && !session.ResultPhase &&
                    sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending,
                    "actual Clear did not leave an empty Idle table");
                string settledArchive = DemoCleanupArchive(false);
                string clearedEconomy = DemoCleanupEconomy();

                // Named defensive call: disabled empty PLAY cannot reach this
                // through normal user input. Pays is then a real recovery action.
                BeginSpin();
                RequireConfiguredOrder(!sourceTableInputEnabled && demoMenuState == 2 &&
                    gameOverMessageLatched && !spinning && !configuredAutoPlayCheckPending,
                    "manual call-level empty refusal did not retain table/Demo lock");
                yield return ConfiguredOrderPaysReturn(true);
                RequireConfiguredOrder(DemoCleanupEconomy() == clearedEconomy &&
                    DemoCleanupArchive(false) == settledArchive && ReadPcCreditEvent() == null,
                    "Pays recovery changed settled economics or generated a money event");

                // Produce a genuine zero meter through the production collect
                // transaction, without manufacturing archive or credit records.
                BeginSpin();
                RequestPcCollect();
                RequireConfiguredOrder(pcCollectPending, "collect fixture was not accepted");
                for (int frame = 0; frame < 8 && pcCollectPending; ++frame) yield return null;
                var local = (RouletteSession)session;
                PcCreditEvent collected = ReadPcCreditEvent();
                RequireConfiguredOrder(!pcCollectPending && local.MoneyCents == 0 &&
                    collected != null && collected.kind == "collect" && collected.phase == 3 &&
                    collected.collectedCents == 999 && !sourceTableInputEnabled && demoMenuState == 2 &&
                    configuredAutoPlayCheckPending && sourceGameplayState == SourceGameplayState.Idle &&
                    !sourceBetStateEntryPending,
                    "production collect did not establish the locked zero-meter fixture");
                // Cashout returns through a genuine Idle entry. Let the live
                // adapter consume that entry rather than emulating its effects.
                sourceInputModule.enabled = true;
                for (int frame = 0; frame < 4 && configuredAutoPlayCheckPending; ++frame)
                    yield return null;
                sourceInputModule.enabled = false;
                RequireConfiguredOrder(!configuredAutoPlayCheckPending && demoMenuState == 3 &&
                    !sourceTableInputEnabled && !TableTouchInputEnabled() && local.MoneyCents == 0 &&
                    sourceGameplayState == SourceGameplayState.Idle && counted.Pockets == 1 &&
                    counted.Suits == 0 && ReadPcCreditEvent().id == collected.id,
                    "live cashout Idle entry did not enable Demo while closing the zero-credit table");
                // Establish a separate, explicit call-level refusal after the
                // cashout entry to test the zero-crossing writer independently.
                BeginSpin();
                RequireConfiguredOrder(demoMenuState == 2 && !sourceTableInputEnabled &&
                    !configuredAutoPlayCheckPending && local.MoneyCents == 0,
                    "post-cashout call-level refusal did not establish disabled Demo");
                AddPcVirtualCredits(1);
                PcCreditEvent deposit = ReadPcCreditEvent();
                RequireConfiguredOrder(local.MoneyCents == 1 && session.Credit == 1 &&
                    sourceTableInputEnabled && TableTouchInputEnabled() && demoMenuState == 2 &&
                    deposit != null && deposit.kind == "deposit" && deposit.before == 0 &&
                    deposit.after == 1 && deposit.phase == 3,
                    "zero-to-positive deposit must restore the table without enabling Demo");

                // The production denomination transaction creates a real
                // sub-credit remainder. Integer Credit==0 is not zero money.
                RequestPcDenominationChange();
                RequireConfiguredOrder(session.Denomination > 1 && session.Credit == 0 &&
                    local.MoneyCents == 1 && local.MoneyRemainderCents == 1 &&
                    SourceCreditMeterNonzero && TableTouchInputEnabled(),
                    "denomination transaction lost the fraction-only nonzero meter");
                BeginSpin();
                RequireConfiguredOrder(!sourceTableInputEnabled && demoMenuState == 2,
                    "fraction-only call-level rejection did not lock the table");
                yield return ConfiguredOrderPaysReturn();
                RequireConfiguredOrder(session.Credit == 0 && local.MoneyCents == 1 &&
                    TableTouchInputEnabled(), "Pays did not reopen a fractional nonzero credit meter");
                BeginSpin();
                long denomination = session.Denomination;
                AddPcVirtualCredits(1);
                deposit = ReadPcCreditEvent();
                RequireConfiguredOrder(local.MoneyCents == denomination + 1 && session.Credit == 1 &&
                    local.MoneyRemainderCents == 1 && !sourceTableInputEnabled &&
                    !TableTouchInputEnabled() && demoMenuState == 2 && deposit != null &&
                    deposit.kind == "deposit" && deposit.before == 0 && deposit.after == 1 &&
                    deposit.beforeRemainderCents == 1 && deposit.afterRemainderCents == 1 &&
                    deposit.phase == 3,
                    "deposit into an existing fractional meter incorrectly acted as a zero crossing");
                yield return ConfiguredOrderPaysReturn();
                RequireConfiguredOrder(DemoCleanupArchive(false) == settledArchive &&
                    DemoCleanupRngBytes() == liveRngBefore && counted.Pockets == 1 && counted.Suits == 0 &&
                    session.Rounds == 1 && session.Wagered == 1 && session.Returned == 0 &&
                    session.TotalBet == 0 && !session.SpinPending && pendingSpin == null &&
                    !spinning && !autoplay && !configuredAutoPlayCheckPending &&
                    SourcePlayerInputs.Count == 0 && sourceGameplayState == SourceGameplayState.Idle &&
                    !sourceBetStateEntryPending && demoMenuState == 2,
                    "recovery actions changed the immutable result/RNG or started another round");
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                configuredAutoPlayCheckPending = false;
                configuredAutoPlayFinishPending = false;
                configuredAutoPlay = originalConfiguration;
                outcome = originalOutcome;
                demoOutcome = originalDemo;
                sourceInputModule.enabled = moduleEnabled;
            }
            // Emitted after cleanup; the strict runner requires natural exit 0.
            Debug.Log("ROULETTE_START_GAME_CONFIGURED_ORDER_PASSED: " +
                "entry=call-level zeroRefusal=IdleDemo3 draw=afterIdleEnable " +
                "draws=1 settledRounds=1 pays=actual-uGUI paysPurge=release-and-backlog demoRetained=2 " +
                "cashoutIdle=live-adapter zeroDeposit=reopened fractionalDeposit=keptLocked " +
                "archive=preserved liveRng=untouched");
            Application.Quit();
        }

        private IEnumerator ConfiguredOrderUiClick(Button button)
        {
            RefreshPortraitUi();
            yield return null;
            RequireConfiguredOrder(button != null && button.gameObject.activeInHierarchy &&
                button.interactable, "expected active UI action is unavailable");
            DemoCleanupClick(button);
            RefreshPortraitUi();
            yield return null;
        }

        private IEnumerator ConfiguredOrderPaysReturn(bool verifyPurge = false)
        {
            RequireConfiguredOrder(!sourceTableInputEnabled && demoMenuState == 2 &&
                gameOverMessageLatched && SourceCreditMeterNonzero && paysPage < 0,
                "Pays recovery lacks its completed, locked, nonzero fixture");
            yield return ConfiguredOrderUiClick(uiInfoButton);
            RequireConfiguredOrder(gameInfoOptionsOpen, "actual INFO did not open options");
            if (verifyPurge)
            {
                // Queue the real raw-pointer down/up, then an observable
                // adapter-level down callback. Opening Pays must purge its
                // own captured release and discard that later callback.
                int laterDownDispatches = 0;
                var held = DemoCleanupQueue(uiGameInfoSeePays);
                SourcePlayerInputs.Enqueue(0x200001, () => ++laterDownDispatches);
                RequireConfiguredOrder(SourcePlayerInputs.Count == 3 &&
                    SourcePlayerInputs.DispatchOne() && SourcePlayerInputs.Count == 0 &&
                    laterDownDispatches == 0 && paysPage == 0,
                    "actual SEE PAYS did not purge its queued release and later down callback");
                RequireSourcePointerReleased(held);
                RefreshPortraitUi();
                yield return null;
                RequireConfiguredOrder(SourcePlayerInputs.Count == 0 && laterDownDispatches == 0,
                    "purged Pays backlog executed after its opening frame");
            }
            else yield return ConfiguredOrderUiClick(uiGameInfoSeePays);
            RequireConfiguredOrder(paysPage == 0 && !sourceTableInputEnabled && demoMenuState == 2,
                "actual SEE PAYS changed Demo availability or failed to close table inputs");
            yield return ConfiguredOrderUiClick(uiPaysExit);
            RequireConfiguredOrder(paysPage < 0 && sourceTableInputEnabled &&
                TableTouchInputEnabled() && demoMenuState == 2 && !demoMenuVisible &&
                sourceGameplayState == SourceGameplayState.Idle && !sourceBetStateEntryPending,
                "actual Pays return must restore table only, preserving Demo2 and gameplay state");
        }
    }
}
