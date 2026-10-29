using System;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private bool pcRoundLifecycleStorageFaulted;
        private bool pcLifecycleColdRound;
        private bool pcLifecycleResumePresentation;
        private bool pcLifecycleColdDisplayEntry;
        private bool pcLifecycleRuntimeReady;
        private bool pcLifecycleIdleDispatch;
        private Action<string> pcLifecycleCheckpoint;
        private Action<string> pcLifecycleBeforeWrite;
        private bool pcLifecycleForceFinancialRestoreFalse;

        private void LifecycleCheckpoint(string boundary)
        {
            if (pcLifecycleCheckpoint != null) pcLifecycleCheckpoint(boundary);
        }

        private void LifecycleBeforeWrite(string boundary)
        {
            if (pcLifecycleBeforeWrite != null) pcLifecycleBeforeWrite(boundary);
        }

        private static bool LifecycleGuid(string text)
        {
            Guid value;
            return text != null && text.Length == 32 &&
                Guid.TryParseExact(text, "N", out value) && value != Guid.Empty &&
                value.ToString("N") == text;
        }

        private static void RequireLifecycle(bool condition, string message)
        {
            if (!condition) throw new PcRoundLifecycleDataException(message);
        }

        private static void ValidateRoundLifecyclePayload(PcRoundLifecycleRecord record,
            RecordedRound latest)
        {
            RequireLifecycle(record != null && latest != null, "Lifecycle has no latest round");
            bool final = record.roundStage != PcRoundStage.RoundOpen;
            if (final)
                RequireLifecycle(!latest.awaitingFinalPayout &&
                    ((!latest.hasDoubleUp && IsEmptyRecordedDouble(latest.doubleUp)) ||
                     (latest.doubleUp != null && latest.doubleUp.complete)),
                    "Final lifecycle conflicts with an unfinished feature");
            bool hasEnd = record.roundStage == PcRoundStage.EndPrepared ||
                record.roundStage == PcRoundStage.Ended;
            RequireLifecycle(hasEnd == (record.endIntent != null), "Lifecycle End receipt is missing or premature");
            if (hasEnd)
            {
                PcRoundEndIntent intent = record.endIntent;
                RequireLifecycle((intent.version == 1 || intent.version == 2) && intent.autoBefore != null &&
                    intent.autoBefore.turboOption >= 0 && intent.autoBefore.turboOption <= 255 &&
                    intent.finishAuto == (intent.autoBefore.offWhenFinished || !intent.transactionActive),
                    "Lifecycle End decision is invalid");
                if (intent.version == 2) ValidateRoundEndIntentV2(intent, record.roundArchiveId);
            }
            RequireLifecycle((record.roundStage == PcRoundStage.Ended) == (record.idleEntry != null),
                "Lifecycle Idle receipt is missing or premature");
            if (record.idleEntry == null) return;
            PcIdleEntry entry = record.idleEntry;
            RequireLifecycle(entry.version == 1 && LifecycleGuid(entry.idleEntryId) &&
                LifecycleGuid(entry.depositEventId) && LifecycleGuid(entry.successorSpinId) &&
                entry.idleEntryId != entry.depositEventId && entry.idleEntryId != entry.successorSpinId &&
                entry.depositEventId != entry.successorSpinId, "Idle receipt identity is invalid");
            RequireLifecycle(string.IsNullOrEmpty(entry.consumedCollectEventId) ||
                LifecycleGuid(entry.consumedCollectEventId), "Idle collect handoff identity is invalid");
            if (entry.state == PcIdleState.Pending)
            {
                RequireLifecycle(entry.step == PcIdleStep.None && entry.autoBefore == null &&
                    !entry.eligible && !entry.demoAvailable && !entry.depositRequired &&
                    entry.creditBefore == 0 && entry.creditAfter == 0 && entry.denomination == 0 &&
                    entry.remainderCents == 0 && string.IsNullOrEmpty(entry.priorCreditEventId) &&
                    string.IsNullOrEmpty(entry.priorCreditEventDigest) &&
                    !entry.prepareRequested && !entry.prepareAccepted && !entry.tableInputEnabled,
                    "Pending Idle contains executed effects");
                return;
            }
            RequireLifecycle(entry.autoBefore != null && entry.autoBefore.turboOption >= 0 &&
                entry.autoBefore.turboOption <= 255 &&
                ((entry.state == PcIdleState.Running && entry.step >= PcIdleStep.Begin &&
                    entry.step <= PcIdleStep.DrawInFlight) ||
                 (entry.state == PcIdleState.Complete && entry.step == PcIdleStep.Complete)),
                "Idle progress is invalid");
            RequireLifecycle(entry.eligible == (entry.autoBefore.autoPlay != 0 &&
                (entry.demoAvailable || !entry.autoBefore.demoOnly)) &&
                entry.creditBefore >= 0 && entry.denomination > 0 && entry.remainderCents >= 0 &&
                entry.remainderCents < entry.denomination &&
                entry.depositRequired == (entry.eligible && entry.autoBefore.replenishCredits &&
                    entry.creditBefore < 2000) &&
                (decimal)entry.creditAfter == (decimal)entry.creditBefore +
                    (entry.depositRequired ? 2000 : 0) &&
                (decimal)entry.creditAfter * entry.denomination + entry.remainderCents <= long.MaxValue,
                "Idle deposit decision is invalid");
            bool priorEmpty = string.IsNullOrEmpty(entry.priorCreditEventId);
            RequireLifecycle(priorEmpty ? string.IsNullOrEmpty(entry.priorCreditEventDigest) :
                LifecycleGuid(entry.priorCreditEventId) && IsLifecycleDigest(entry.priorCreditEventDigest),
                "Idle prior credit receipt is invalid");
            RequireLifecycle((entry.step >= PcIdleStep.CheckPurged || !entry.prepareRequested) &&
                (entry.step >= PcIdleStep.PrepareFinished || !entry.prepareAccepted) &&
                (!entry.prepareAccepted || entry.prepareRequested) &&
                (!entry.prepareRequested || entry.eligible) &&
                (entry.step != PcIdleStep.DrawInFlight || entry.prepareAccepted),
                "Idle preparation decision is invalid");
        }

        private static bool IsLifecycleDigest(string text)
        {
            if (text == null || text.Length != 64) return false;
            foreach (char value in text)
                if (!((value >= '0' && value <= '9') || (value >= 'A' && value <= 'F'))) return false;
            return true;
        }

        private static void ValidateIdleLifecycleSuccessor(PcRoundLifecycleRecord predecessor,
            string successorSpinId)
        {
            PcIdleEntry entry = predecessor.idleEntry;
            RequireLifecycle(entry != null, "Ended round has no Idle entry");
            if (entry.state == PcIdleState.Pending || entry.state == PcIdleState.Running)
                RequireLifecycle(entry.state == PcIdleState.Running &&
                    entry.step == PcIdleStep.DrawInFlight && entry.prepareAccepted &&
                    successorSpinId == entry.successorSpinId, "Successor does not own the running Idle entry");
            else if (entry.prepareAccepted)
                RequireLifecycle(successorSpinId == entry.successorSpinId,
                    "Prepared Idle successor identity differs");
        }

        private PcRoundLifecycleRecord CopyLatestLifecycle()
        {
            return CloneRoundLifecycle(GetLatestLifecycleForRestore());
        }

        private void SaveLifecycleTransition(PcRoundLifecycleRecord candidate)
        {
            if (pcRoundArchiveSchemaVersion == 4)
            {
                SaveSasLifecycleTransitionV4(candidate);
                return;
            }
            PcRoundLifecycleRecord prior = latestRoundLifecycle;
            try
            {
                // No gameplay flag is published until the complete envelope saves.
                latestRoundLifecycle = candidate;
                LifecycleBeforeWrite(candidate.roundStage == PcRoundStage.Ended &&
                    candidate.idleEntry.state != PcIdleState.Pending ?
                    "Idle" + candidate.idleEntry.step : candidate.roundStage.ToString());
                StageRecordedRoundArchive();
                PlayerPrefs.Save();
                ++recordedHistoryVersion;
                Debug.Log("ROULETTE_LIFECYCLE_SAVED: round=" + candidate.roundArchiveId +
                    " stage=" + candidate.roundStage + " idle=" +
                    (candidate.idleEntry == null ? "none" : candidate.idleEntry.state + "/" + candidate.idleEntry.step));
            }
            catch (Exception error)
            {
                latestRoundLifecycle = prior;
                FailPcRoundLifecycle("saved", error);
                throw;
            }
            LifecycleCheckpoint(candidate.roundStage == PcRoundStage.Ended &&
                candidate.idleEntry.state != PcIdleState.Pending ?
                "Idle" + candidate.idleEntry.step + "Saved" : candidate.roundStage + "Saved");
        }

        private void FailPcRoundLifecycle(string operation, Exception error)
        {
            pcRoundLifecycleStorageFaulted = true;
            FailPcAutoPlayStorage(operation, error);
        }

        private bool LifecycleAllowsStagedTable
        {
            get { return !HasTrustedLatestLifecycle ||
                GetLatestLifecycleForRestore().roundStage == PcRoundStage.Ended; }
        }

        private bool LifecycleBlocksPlayerInput
        {
            get { return !pcLifecycleIdleDispatch && HasTrustedLatestLifecycle &&
                (latestRoundLifecycle.roundStage != PcRoundStage.Ended ||
                 latestRoundLifecycle.idleEntry.state != PcIdleState.Complete); }
        }

        private bool RestorePersistedRoundLifecycle()
        {
            if (!HasTrustedLatestLifecycle) return false;
            EnsureCollectedLifecycleIdleEntry(pcCreditOverride);
            PcRoundLifecycleRecord record = GetLatestLifecycleForRestore();
            pcLifecycleColdRound = true;
            gameStartTimestamp = 0;
            if (resumePendingSpin)
            {
                RequireLifecycle(record.roundStage == PcRoundStage.Ended && pendingSpin != null &&
                    pendingSpin.baseLatestRoundId == record.roundArchiveId,
                    "Pending successor conflicts with unfinished lifecycle");
                ValidateIdleLifecycleSuccessor(record, pendingSpin.id);
                if (record.idleEntry.state == PcIdleState.Running)
                {
                    PcRoundLifecycleRecord completed = CopyLatestLifecycle();
                    completed.idleEntry.state = PcIdleState.Complete;
                    completed.idleEntry.step = PcIdleStep.Complete;
                    SaveLifecycleTransition(completed);
                }
                ArmSourceRoundAfterCommit();
                pcLifecycleColdRound = true;
                return true;
            }
            if (record.roundStage == PcRoundStage.Ended)
            {
                sourceRoundPhase = SourceRoundPhase.None;
                if (record.idleEntry.state != PcIdleState.Complete)
                {
                    RequireLifecycle(record.idleEntry.step != PcIdleStep.DrawInFlight,
                        "Draw was started without a recoverable pending result");
                    sourceGameplayState = SourceGameplayState.Idle;
                    sourceBetStateEntryPending = false;
                    configuredAutoPlayCheckPending = true;
                }
                else RequireLifecycle(!record.idleEntry.prepareAccepted,
                    "Completed Idle handoff has no pending successor");
                return true;
            }
            sourceGameplayState = SourceGameplayState.Playing;
            sourceBetStateEntryPending = false;
            sourceTableInputEnabled = false;
            gameOverMessageLatched = record.roundStage != PcRoundStage.RoundOpen;
            pcGamePayoutEntered = false;
            pcPaymentResumeEnter = false;
            pcGamePaymentRetryDue = 0;
            pcGamePaymentEventFrame = -1;
            if (PcPaymentTracked && pcGamePayment.prepared || pcRoundArchiveSchemaVersion==4 && pcSasGamePaymentV2!=null && pcSasGamePaymentV2.prepared)
            {
                sourceRoundPhase = SourceRoundPhase.ResultDisplay;
                pcLifecycleResumePresentation = false;
                return true;
            }
            if (PcPaymentTracked && pcGamePayment.baseOffer == PcGamePaymentOffer.Enter ||
                pcRoundArchiveSchemaVersion==4&&pcSasGamePaymentV2!=null&&pcSasGamePaymentV2.owner==PcGamePaymentOwner.Base&&pcSasGamePaymentV2.baseOffer==PcGamePaymentOffer.Enter)
            {
                sourceRoundPhase = SourceRoundPhase.DoubleOffer;
                sourceBaseHasDoubleOffer = true;
                sourceDoubleOfferChoice = SourceDoubleOfferChoice.Enter;
                pcPaymentResumeEnter = true;
                return true;
            }
            if (record.roundStage == PcRoundStage.RoundOpen &&
                (session.DoubleAvailable || session.InDoubleUp))
            {
                sourceBaseHasDoubleOffer = true;
                sourceRoundPhase = session.InDoubleUp ? SourceRoundPhase.DoublePlaying : SourceRoundPhase.DoubleOffer;
                if (sourceRoundPhase == SourceRoundPhase.DoubleOffer) StartSourceOfferPrompt();
                if(pcRoundArchiveSchemaVersion==4&&pcSasGamePaymentV2!=null&&pcSasGamePaymentV2.owner==PcGamePaymentOwner.Base&&
                    pcSasGamePaymentV2.baseOffer==PcGamePaymentOffer.None)pcLifecycleResumePresentation=true;
            }
            else
            {
                sourceRoundPhase = SourceRoundPhase.ResultDisplay;
                pcLifecycleResumePresentation = record.roundStage == PcRoundStage.RoundOpen;
            }
            return true;
        }

        private bool ServicePersistedRoundPresentation()
        {
            if (!pcLifecycleResumePresentation) return false;
            if (!pcLifecycleRuntimeReady) return true;
            pcLifecycleResumePresentation = false;
            long paid = session.LastResult.Returned;
            // Native +514f and active +5110 are newly constructed on cold start.
            autoplay = false;
            // InitCycleResults reads Turbo before StateDisplayResults purges;
            // PerformRollup reads it again after the purge may reset options.
            resultCycleStartedAt = paid > 0 && !TurboAutoPlayActive ? Time.realtimeSinceStartup : -1f;
            resultCycleStartedTimestamp = paid > 0 && !TurboAutoPlayActive ?
                System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            PurgeSourcePlayerInputs();
            pcLifecycleColdDisplayEntry = true;
            sourceRoundPhase = paid > 0 ? SourceRoundPhase.BaseRollup : SourceRoundPhase.ResultDisplay;
            if (paid > 0)
            {
                if (TurboAutoPlayActive) CompleteTurboWinWithoutRack(paid);
                else StartWinRacking(paid);
            }
            else BeginSourceResultDisplay(paid, false);
            return true;
        }

        private void PersistAwaitEnd()
        {
            if (!HasTrustedLatestLifecycle || latestRoundLifecycle.roundStage != PcRoundStage.RoundOpen) return;
            RequireLifecycle(!PcPaymentTracked || pcGamePayment.historyPublished,
                "AwaitEnd precedes payment history publication");
            PcRoundLifecycleRecord record = CopyLatestLifecycle();
            record.roundStage = PcRoundStage.AwaitEnd;
            SaveLifecycleTransition(record);
        }

        private static PcIdleEntry NewLifecycleIdleEntry()
        {
            return new PcIdleEntry {
                version = 1, idleEntryId = Guid.NewGuid().ToString("N"),
                depositEventId = Guid.NewGuid().ToString("N"),
                successorSpinId = Guid.NewGuid().ToString("N"),
                consumedCollectEventId = "",
                state = PcIdleState.Pending, step = PcIdleStep.None,
                priorCreditEventId = "", priorCreditEventDigest = ""
            };
        }

        private bool EndAutoStateAllowed(PcRoundEndIntent intent)
        {
            PcLifecycleAutoSnapshot actual = PcLifecycleAutoSnapshot.Capture(configuredAutoPlay);
            if (intent.version == 2) return EndV2AutoStateAllowed(actual, intent);
            PcLifecycleAutoSnapshot expected = intent.autoBefore.Clone();
            if (actual.Same(expected)) return true;
            if (!intent.finishAuto) return false;
            expected.autoPlay = 0;
            if (actual.Same(expected)) return true;
            expected.turbo = false;
            if (actual.Same(expected)) return true;
            expected.offWhenFinished = false;
            return actual.Same(expected) || (intent.demoAvailable && expected.playerInput &&
                actual.Same(expected.Purged()));
        }

        private static void ValidateRoundEndIntentV2(PcRoundEndIntent intent, string scope)
        {
            RequireLifecycle(intent != null && intent.version == 2 && LifecycleGuid(scope) &&
                intent.autoBefore != null && intent.autoBefore.turboOption >= 0 &&
                intent.autoBefore.turboOption <= 255 &&
                intent.finishDisable == (intent.autoBefore.disableWhenFinished && !intent.bonusGamesEnabled) &&
                intent.finishAuto == (intent.autoBefore.offWhenFinished || !intent.transactionActive) &&
                intent.cancelDisableOperationId == scope + ":end-disable" &&
                intent.cancelAutoOperationId == scope + ":end-auto",
                "Lifecycle End V2 decision or operation identity is invalid");
        }

        private PcRoundEndIntent CreateSourceRoundEndIntentV2(string operationScopeId)
        {
            var intent = new PcRoundEndIntent {
                version = 2, transactionActive = pcPlayerTransactionActive,
                demoAvailable = demoShowMode > 0, bonusGamesEnabled = PcBonusGamesEnabled,
                timerPresent = PcAutoCompleteTimerExists,
                autoBefore = PcLifecycleAutoSnapshot.Capture(configuredAutoPlay),
                cancelDisableOperationId = operationScopeId + ":end-disable",
                cancelAutoOperationId = operationScopeId + ":end-auto"
            };
            intent.finishDisable = intent.autoBefore.disableWhenFinished && !intent.bonusGamesEnabled;
            intent.finishAuto = intent.autoBefore.offWhenFinished || !intent.transactionActive;
            ValidateRoundEndIntentV2(intent, operationScopeId);
            return intent;
        }

        // Only actual native setter prefixes are accepted. In particular a
        // countdown decrement is not an End effect, and an absent timer does
        // not save Start=0. Warning, ShowCountdown and all other fields retain
        // their saved values unless the six option fields are actually purged.
        private static bool EndV2AutoStateAllowed(PcLifecycleAutoSnapshot actual, PcRoundEndIntent intent)
        {
            PcLifecycleAutoSnapshot expected = intent.autoBefore.Clone();
            if (actual.Same(expected)) return true;
            var candidates = new List<PcLifecycleAutoSnapshot> { expected };
            if (intent.finishDisable)
            {
                if (intent.timerPresent)
                {
                    expected.secondsStart = 0;
                    if (actual.Same(expected)) return true;
                }
                if (intent.demoAvailable && expected.playerInput)
                {
                    PcLifecycleAutoSnapshot purged = expected.Purged();
                    if (actual.Same(purged)) return true;
                    candidates.Add(purged);
                }
                foreach (PcLifecycleAutoSnapshot candidate in candidates)
                {
                    candidate.disableWhenFinished = false;
                    if (actual.Same(candidate)) return true;
                }
            }
            if (!intent.finishAuto) return false;
            foreach (PcLifecycleAutoSnapshot candidate in candidates)
            {
                candidate.autoPlay = 0;
                if (actual.Same(candidate)) return true;
                candidate.turbo = false;
                if (actual.Same(candidate)) return true;
                candidate.offWhenFinished = false;
                if (actual.Same(candidate)) return true;
                if (intent.timerPresent && !intent.finishDisable)
                {
                    candidate.secondsStart = 0;
                    if (actual.Same(candidate)) return true;
                }
                if (intent.demoAvailable && candidate.playerInput && actual.Same(candidate.Purged())) return true;
            }
            return false;
        }

        // The same effect sequence serves live End and durable V2 replay.
        // Small effect delegates keep its exact ordering independently testable.
        private static void ApplySourceRoundEndV2(PcRoundEndIntent intent, OriginalAutoPlayData data,
            Action<bool, string, bool> cancel, Action soundsOn, Action deactivate,
            Action<string> checkpoint)
        {
            if (intent.finishDisable)
            {
                cancel(true, intent.cancelDisableOperationId, intent.demoAvailable);
                checkpoint("EndDisableCancelled");
                data.SetDisableWhenGameFinished(false);
                checkpoint("EndDisableFalse");
            }
            if (intent.finishAuto)
            {
                data.SetAutoPlay(0);
                checkpoint("EndAuto0");
                data.SetTurbo(false);
                checkpoint("EndTurboFalse");
                soundsOn();
                checkpoint("EndSoundsOn");
                data.SetAutoPlayOffWhenGameFinished(false);
                checkpoint("EndOffFalse");
                deactivate();
                cancel(true, intent.cancelAutoOperationId, intent.demoAvailable);
                checkpoint("EndPurged");
            }
        }

        private void ExecuteSourceRoundEndV2(PcRoundEndIntent intent, bool restoreTimerPresence)
        {
            string id = intent == null ? null : intent.cancelDisableOperationId;
            ValidateRoundEndIntentV2(intent, id == null || id.Length < 32 ? null : id.Substring(0, 32));
            RequireLifecycle(EndAutoStateAllowed(intent),
                "End recovery AutoData differs from its saved V2 decision");
            if (restoreTimerPresence) RestorePcAutoCompleteTimerPresenceForEnd(intent.timerPresent);
            ResetSourceSelectedDemoOption();
            ApplySourceRoundEndV2(intent, configuredAutoPlay, CancelPcAutoCompleteForEnd,
                () => SetPcGameSoundsMuted(false),
                () => { autoplay = false; autoConfirmPending = false; }, LifecycleCheckpoint);
        }

        private void CompletePersistedRoundEnd()
        {
            PcRoundLifecycleRecord record = GetLatestLifecycleForRestore();
            bool restoreTimerPresence = pcLifecycleColdRound && record.roundStage == PcRoundStage.EndPrepared;
            if (record.roundStage == PcRoundStage.AwaitEnd)
            {
                record = CopyLatestLifecycle();
                record.roundStage = PcRoundStage.EndPrepared;
                record.endIntent = CreateSourceRoundEndIntentV2(record.roundArchiveId);
                SaveLifecycleTransition(record);
            }
            RequireLifecycle(record.roundStage == PcRoundStage.EndPrepared && EndAutoStateAllowed(record.endIntent),
                "End recovery AutoData differs from its saved decision");
            if (record.endIntent.version == 2)
                ExecuteSourceRoundEndV2(record.endIntent, restoreTimerPresence);
            else ExecuteLegacyRoundEnd(record.endIntent);
            PcRoundLifecycleRecord ended = CopyLatestLifecycle();
            ended.roundStage = PcRoundStage.Ended;
            ended.idleEntry = NewLifecycleIdleEntry();
            SaveLifecycleTransition(ended);
            PublishSourceRoundEnd();
        }

        // V1 receipts are already durable decisions from the previous build.
        // They retain their original order and do not acquire V2 timer effects.
        private void ExecuteLegacyRoundEnd(PcRoundEndIntent intent)
        {
            ResetSourceSelectedDemoOption();
            if (intent.finishAuto)
            {
                configuredAutoPlay.SetAutoPlay(0);
                LifecycleCheckpoint("EndAuto0");
                configuredAutoPlay.SetTurbo(false);
                LifecycleCheckpoint("EndTurboFalse");
                configuredAutoPlay.SetAutoPlayOffWhenGameFinished(false);
                LifecycleCheckpoint("EndOffFalse");
                autoplay = false;
                autoConfirmPending = false;
                SetPcGameSoundsMuted(false);
                if (uiSpinButton != null) uiSpinButton.interactable = false;
                PurgeSourcePlayerInputs(demoAvailable: intent.demoAvailable);
                LifecycleCheckpoint("EndPurged");
            }
        }

        private void PublishSourceRoundEnd()
        {
            sourceRoundPhase = SourceRoundPhase.None;
            sourceBaseHasDoubleOffer = false;
            sourceDoubleOfferChoice = SourceDoubleOfferChoice.None;
            configuredAutoPlayFinishPending = false;
            ++sourceRoundEndCount;
            configuredAutoPlayCheckPending = true;
            Debug.Log("ROULETTE_SOURCE_STATE_END: count=" + sourceRoundEndCount +
                " rounds=" + session.Rounds + " credit=" + session.Credit);
        }
    }
}
