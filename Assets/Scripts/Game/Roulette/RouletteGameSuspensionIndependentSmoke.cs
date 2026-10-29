using System.Collections;
using System.Linq;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private IEnumerator GsSmokeIndependentFeature()
        {
            bool payoutPause = gsSmokeCase == "game_payout_wait";
            bool payoutInputs = gsSmokeCase == "game_payout_inputs";
            bool payoutService = gsSmokeCase == "game_payout_service";
            bool payoutNatural = gsSmokeCase == "game_payout_auto" || payoutInputs || payoutService;
            bool payoutWait = payoutPause || payoutNatural;
            yield return GsSmokeSeedFeature();
            yield return GsSmokeTakeWin(541);
            yield return GsSmokeSuspend("independent-feature-pause", "rpc0");
            GameSuspensionSmokeEvent paused = GsSmokeSnapshot("GameOnlyFeatureWaiting");
            gsSmokeEvents.Add(paused);
            RequireGsSmoke(!paused.featureSuspended && paused.featureTimer && paused.saved == 5 &&
                paused.credit == 999 && paused.completion == 0 && paused.featureDue > 0 &&
                paused.featureDue == OriginalRackClock.AfterMilliseconds(paused.featureArmedAt,
                    paused.featureDelay, Stopwatch.Frequency) &&
                paused.pausedMovies == 0 && !gsSmokeEvents.Any(e =>
                    e.operationId == "independent-feature-pause" && e.point.StartsWith("FeaturesSuspended")),
                "Game-only pause stopped an independent Feature or paused Menu movies");
            if (!GsSmokeCutCase) yield return GsSmokeCapture("game_feature_waiting");
            if (payoutWait)
            {
                GsSmokeDisable("independent-feature-auto");
                yield return GsSmokeWait(() => gsSmokeNotifications.Any(n => n.kind == "AutoCompleteStart"),
                    "independent AutoComplete before main Game payout", 10);
                RequireGsSmoke(pcGameAppSuspended && autoplay && configuredAutoPlay.AutoPlay == 1 &&
                    pcAutoCompleteDeliveries == 4, "independent countdown lost active Auto under Game pause");
            }
            yield return GsSmokeWait(() => PcDoubleGamePayoutPending && !pcDoubleFeature.Active &&
                pcDoubleFeatureNv.savedState == 0 && sourceRoundPhase == SourceRoundPhase.ResultDisplay &&
                pcResultDisplayWait != null, "selected Game received and presented Feature result while suspended", 10);
            GameSuspensionSmokeEvent received = GsSmokeSnapshot("GameOnlyFeatureReceived");
            gsSmokeEvents.Add(received);
            RequireGsSmoke(received.app && !received.featureSuspended && received.credit == 999 && received.returned == 36 &&
                received.payoutPending && !received.nativeFeatureActive && received.completion == 1 && received.inDouble &&
                received.displayedCredit == 1035 && received.displayedPaid == 36 && received.cycleActive && !received.cyclePaused &&
                received.ends == 0 && received.rng == paused.rng && !recordedRoundPages[0].doubleUp.complete &&
                latestRoundLifecycle.roundStage == PcRoundStage.RoundOpen,
                "independent Feature result paid money, remained active, drew RNG, or crossed Game's gate");
            yield return GsSmokeObserve(350);
            RequireGsSmoke(pcGameAppSuspended && sourceRoundEndCount == 0 && session.Credit == 999 &&
                PcDoubleGamePayoutPending && DisplayedCredit == 1035 &&
                pcResultCycleClock != null && pcResultCycleClock.CallbackCount > 0,
                "paused Game paid money or failed to service its newly created result owner");
            gsSmokeEvents.Add(GsSmokeSnapshot("GameOnlyResultWhileSuspended"));
            yield return GsSmokeCapture("game_feature_received");
            if (payoutService)
            {
                SetIDeckPreview(true);
                RefreshPcSuspendedInputPresentation();
                RefreshDiagnosticUi();
                yield return null;
                RequireGsSmoke(ideckServiceButton.IsInteractable() && !pcServiceRequested && !serviceConfirmPending &&
                    SourcePlayerInputs.Count == 0 && pcResultDisplayWait.InputCount == 0,
                    "paused service keep-alive fixture was not ready");
                QueueStartGameButton(ideckServiceButton, 751, true);
                yield return GsSmokeWait(() => pcServiceRequested && SourcePlayerInputs.Count == 0,
                    "paused WaitInput dispatched real Service press and release", 5);
                RequireGsSmoke(pcGameAppSuspended && !serviceConfirmPending && autoplay &&
                    session.Credit == 999 && PcDoubleGamePayoutPending && pcResultDisplayWait.InputCount == 2 &&
                    pcResultDisplayWait.TimeoutCount == 0,
                    "paused live Service opened confirmation, changed Auto/money, or bypassed its two receipts");
                gsSmokeEvents.Add(GsSmokeSnapshot("GamePayoutServiceRequested"));
                yield return GsSmokeCapture("game_service_requested");
                QueueStartGameButton(ideckServiceButton, 752, true);
                yield return GsSmokeWait(() => !pcServiceRequested && SourcePlayerInputs.Count == 0,
                    "second paused Service release cleared local request", 5);
                RequireGsSmoke(pcGameAppSuspended && !serviceConfirmPending && autoplay &&
                    session.Credit == 999 && PcDoubleGamePayoutPending && pcResultDisplayWait.InputCount == 4 &&
                    pcResultDisplayWait.TimeoutCount == 0,
                    "second Service release changed Game payout or active Auto");
                gsSmokeEvents.Add(GsSmokeSnapshot("GamePayoutServiceCleared"));
                yield return GsSmokeCapture("game_service_cleared");
                SetIDeckPreview(false);
            }
            if (GsSmokeCutCase)
            {
                SetPcMenuLifecycleContext(10, false, false, true, false, false);
                RequestPcMenuResume("cut-result-pending");
                yield return GsSmokeWait(() => GsSmokeHas("MenuResume:Pending", "cut-result-pending"),
                    "received result cut retained Menu pending", 5);
                RequireGsSmoke(pcMenuSuspend.PendingResume && pcGameAppSuspended && PcDoubleGamePayoutPending,
                    "received result cut lost its pending Game payout");
                yield return GsSmokeCapture("final", true);
                GsSmokeExit("Paused", "checkpoint");
                yield break;
            }
            yield return GsSmokeResume("independent-feature-resume", false);
            if (payoutWait)
            {
                yield return GsSmokeWait(() => sourceRoundPhase == SourceRoundPhase.ResultDisplay &&
                    PcDoubleGamePayoutPending && ResultDisplayActive, "real automatic result-display wait", 5);
                GameSuspensionSmokeEvent displaying = GsSmokeSnapshot("GamePayoutDisplayWait");
                gsSmokeEvents.Add(displaying);
                RequireGsSmoke(displaying.credit == 999 && displaying.displayedCredit == 1035 &&
                    displaying.displayedPaid == 36 && displaying.active && displaying.payoutPending &&
                    !displaying.resultWaitComplete && displaying.resultWaitPhase == 0 &&
                    displaying.resultWaitInitial == 30 && displaying.resultWaitRemaining > 0 && displaying.ends == 0,
                    "result display changed wallet before its independent main Game payout");
            }
            yield return GsSmokeCapture("game_feature_resumed");
            if (payoutPause)
            {
                yield return GsSmokeSuspend("payout-wait-pause", "rpc0");
                yield return GsSmokeObserve(1500);
                GameSuspensionSmokeEvent held = GsSmokeSnapshot("GamePayoutWaitHeld");
                gsSmokeEvents.Add(held);
                RequireGsSmoke(held.credit == 999 && held.displayedCredit == 1035 && held.payoutPending &&
                    !held.active && held.app && held.ends == 0 && held.saved == 0 && held.completion == 1 &&
                    held.timestamp >= held.resultWaitDue && !held.resultWaitComplete,
                    "expired result wait paid while App was suspended");
                yield return GsSmokeCapture("game_payout_paused");
                yield return GsSmokeResume("payout-wait-resume", false);
                yield return GsSmokeWait(() => !PcDoubleGamePayoutPending && pcDoubleFeatureNv.completionStage == 2,
                    "ordinary Game payout after App resume", 5);
                yield return GsSmokeCapture("game_payout_resumed");
            }
            if (payoutNatural)
            {
                OriginalResultDisplayWaitClock waiting = pcResultDisplayWait;
                int queuedProbeCount = 0;
                RequireGsSmoke(waiting != null && !waiting.Completed && waiting.InitialWaits == 30 &&
                    PcDoubleGamePayoutPending && session.Credit == 999,
                    "natural result wait fixture lost its unpaid thirty-iteration owner");
                if (payoutInputs)
                {
                    int count = waiting.RemainingWaits;
                    queuedProbeCount = count;
                    for (int i = 0; i < count; ++i)
                        SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease, () => {
                            if (pcResultDisplayWaitServicing)
                            {
                                ++gsSmokeResultInputProbeDispatches;
                                RequireGsSmoke(!waiting.Completed && PcDoubleGamePayoutPending && session.Credit == 999,
                                    "WaitInput callback escaped its unpaid result owner");
                                gsSmokeEvents.Add(GsSmokeSnapshot("GamePayoutInputDispatched"));
                            }
                            else
                            {
                                ++gsSmokeRetainedInputProbeDispatches;
                                RequireGsSmoke(waiting.Completed &&
                                    (PcDoubleGamePayoutPending ? session.Credit == 999 : session.Credit == 1035),
                                    "retained input bypassed a pending wait or changed its payment boundary");
                                gsSmokeEvents.Add(GsSmokeSnapshot("GamePayoutRetainedInputDispatched"));
                            }
                            RequireGsSmoke(session.Rounds == 1 && gsSmokePockets == 1 && gsSmokeSuits == 0,
                                "queued result input consumed an additional outcome");
                        });
                    gsSmokeEvents.Add(GsSmokeSnapshot("GamePayoutInputsQueued"));
                }
                yield return GsSmokeWait(() => waiting.Completed && !PcDoubleGamePayoutPending &&
                    pcDoubleFeatureNv.completionStage == 2, "thirty independently returned result waits", 10);
                if (payoutInputs)
                    yield return GsSmokeWait(() => SourcePlayerInputs.Count == 0,
                        "ordinary dispatch of inputs retained by result timeouts", 10);
                // Preserve every operand before the compound acceptance check.
                // A returned Idle may legitimately prefer an expired timeout to
                // queued input; diagnostics must distinguish that from a lost wait.
                UnityEngine.Debug.Log("ROULETTE_GAME_PAYOUT_WAIT_DIAGNOSTIC: timeouts=" + waiting.TimeoutCount +
                    " inputs=" + waiting.InputCount + " skipped=" + waiting.SkippedWaits +
                    " highDelay=" + waiting.HighDelayDueTimestamp + " credit=" + session.Credit +
                    " waitCallbacks=" + gsSmokeResultInputProbeDispatches +
                    " retainedCallbacks=" + gsSmokeRetainedInputProbeDispatches);
                RequireGsSmoke(waiting.TimeoutCount + waiting.InputCount == 30 && waiting.SkippedWaits == 0 &&
                    waiting.HighDelayDueTimestamp == 0 && session.Credit == 1035 &&
                    (payoutInputs ? waiting.InputCount > 0 && waiting.InputCount == gsSmokeResultInputProbeDispatches &&
                        gsSmokeResultInputProbeDispatches + gsSmokeRetainedInputProbeDispatches == queuedProbeCount :
                        payoutService ? waiting.InputCount == 4 && waiting.TimeoutCount == 26 :
                        waiting.InputCount == 0 && waiting.TimeoutCount == 30),
                    "natural result waits skipped iterations, ignored queued input, or added an unrelated delay");
                gsSmokeEvents.Add(GsSmokeSnapshot("GamePayoutNaturalFinished"));
                yield return GsSmokeCapture("game_payout_finished");
            }
        }

        private IEnumerator GsSmokePaysTimeout()
        {
            GameSuspensionSmokeEvent opening = GsSmokeSnapshot("PaysNaturalTimeoutOpening");
            gsSmokeEvents.Add(opening);
            long openedBefore = opening.timestamp;
            ToggleGameInfoOptions(); OpenPaysFromGameInfo(); RefreshPortraitUi();
            GameSuspensionSmokeEvent opened = GsSmokeSnapshot("PaysNaturalTimeoutOpened");
            gsSmokeEvents.Add(opened);
            RequireGsSmoke(opened.pays == 0 && opened.paysExists &&
                opened.paysTimeoutDue >= openedBefore + 60 * Stopwatch.Frequency &&
                opened.paysTimeoutDue <= opened.timestamp + 60 * Stopwatch.Frequency,
                "Pays did not naturally arm its configured sixty-second timeout");
            yield return GsSmokeWait(() => pcPaysFlash != null && pcPaysFlash.CallbackCount > 0,
                "first real Pays flash receipt", 5);
            yield return GsSmokeSuspend("pays-timeout-pause");
            GameSuspensionSmokeEvent held = GsSmokeSnapshot("PaysNaturalTimeoutHeldStart");
            gsSmokeEvents.Add(held);
            RequireGsSmoke(held.paysFlashDue == 0 && held.paysTimeoutDue == opened.paysTimeoutDue &&
                held.paysExists && !held.paysReturning, "suspend shifted the independent Pays timeout");
            yield return GsSmokeObserve(350);
            GameSuspensionSmokeEvent after = GsSmokeSnapshot("PaysNaturalTimeoutHeldEnd");
            gsSmokeEvents.Add(after);
            RequireGsSmoke(after.paysFlashCallbacks == held.paysFlashCallbacks &&
                after.paysFlashState == held.paysFlashState && after.paysFlashDue == 0 &&
                after.paysTimeoutDue == held.paysTimeoutDue, "suspended Pays flash advanced");
            yield return GsSmokeCapture("pays_timeout_waiting");
            // Wait for the actual timer: no deadline, clock, or state injection.
            yield return GsSmokeWait(() => PcPaysReturnPending,
                "natural sixty-second Pays timeout while App suspended", 75);
            GameSuspensionSmokeEvent returning = GsSmokeSnapshot("PaysNaturalTimeoutReturning");
            gsSmokeEvents.Add(returning);
            RequireGsSmoke(returning.app && returning.timestamp >= opened.paysTimeoutDue &&
                returning.paysTimeoutDue == 0 && returning.paysFlashDue > 0 &&
                returning.paysFlashGeneration > held.paysFlashGeneration &&
                returning.paysFlashCallbacks == held.paysFlashCallbacks && returning.paysReturning,
                "timeout failed to rearm the stopped Pays Trigger while App was suspended");
            yield return GsSmokeWait(() => paysPage < 0 && pcPaysFlash == null,
                "timeout Trigger destroyed Pays during suspension", 5);
            GameSuspensionSmokeEvent closed = GsSmokeSnapshot("PaysNaturalTimeoutClosed");
            gsSmokeEvents.Add(closed);
            RequireGsSmoke(closed.app && closed.timestamp >= returning.paysFlashDue && !closed.paysExists &&
                closed.credit == 1000 && closed.rounds == 0 && closed.rng == held.rng,
                "Pays timeout changed money/RNG or resumed App");
            yield return GsSmokeCapture("pays_timeout_closed");
            yield return GsSmokeResume("pays-timeout-resume");
            RequireGsSmoke(paysPage == -1 && pcPaysFlash == null,
                "Resume recreated the destroyed Pays Trigger");
            yield return GsSmokeCapture("pays_timeout_resumed");
        }
    }
}
