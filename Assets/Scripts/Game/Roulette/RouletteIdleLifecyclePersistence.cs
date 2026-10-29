using System;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private void ValidateLifecycleBeginIdle(PcRoundLifecycleRecord predecessor)
        {
            PcIdleEntry entry = predecessor.idleEntry;
            RequireLifecycle(entry != null && (pcLifecycleIdleDispatch ?
                entry.state == PcIdleState.Running &&
                    (entry.step == PcIdleStep.CheckPurged || entry.step == PcIdleStep.IdleEffectsDone) :
                entry.state == PcIdleState.Complete && !entry.prepareAccepted),
                "A new spin does not own a completed or prepared Idle entry");
        }

        private void PersistIdleStep(PcIdleEntry entry, PcIdleStep step)
        {
            PcRoundLifecycleRecord record = CopyLatestLifecycle();
            entry.step = step;
            // The caller continues advancing its local candidate. Never
            // publish that mutable object as the last committed receipt.
            record.idleEntry = JsonUtility.FromJson<PcIdleEntry>(JsonUtility.ToJson(entry));
            SaveLifecycleTransition(record);
        }

        private PcIdleEntry CopyIdleEntry()
        {
            return CopyLatestLifecycle().idleEntry;
        }

        private void BeginLifecycleIdleEntry()
        {
            PcRoundLifecycleRecord record = GetLatestLifecycleForRestore();
            RequireLifecycle(record.roundStage == PcRoundStage.Ended, "Idle precedes End");
            if (record.idleEntry.state == PcIdleState.Complete)
            {
                RequireLifecycle(!record.idleEntry.prepareAccepted,
                    "Prepared Idle has no successor pending record");
                record = CopyLatestLifecycle();
                string consumedCollect = record.idleEntry.consumedCollectEventId;
                record.idleEntry = NewLifecycleIdleEntry();
                record.idleEntry.consumedCollectEventId = consumedCollect;
                SaveLifecycleTransition(record);
            }
            if (record.idleEntry.state != PcIdleState.Pending) return;
            PcCreditEvent prior = ReadPcCreditEvent();
            PcIdleEntry entry = CopyIdleEntry();
            entry.state = PcIdleState.Running;
            entry.autoBefore = PcLifecycleAutoSnapshot.Capture(configuredAutoPlay);
            entry.demoAvailable = demoShowMode > 0;
            entry.eligible = ConfiguredAutoPlayEligible;
            entry.depositRequired = entry.eligible && configuredAutoPlay.ReplenishCredits && session.Credit < 2000;
            entry.creditBefore = session.Credit;
            entry.creditAfter = checked(session.Credit + (entry.depositRequired ? 2000 : 0));
            entry.denomination = session.Denomination;
            entry.remainderCents = ((RouletteSession)session).MoneyRemainderCents;
            entry.priorCreditEventId = prior == null ? "" : prior.id;
            entry.priorCreditEventDigest = prior == null ? "" : prior.digest;
            entry.tableInputEnabled = sourceTableInputEnabled;
            PersistIdleStep(entry, PcIdleStep.Begin);
        }

        private void ValidateIdleAutoState(PcIdleEntry entry)
        {
            PcLifecycleAutoSnapshot actual = PcLifecycleAutoSnapshot.Capture(configuredAutoPlay);
            RequireLifecycle(actual.Same(entry.autoBefore) ||
                (entry.demoAvailable && entry.autoBefore.playerInput && actual.Same(entry.autoBefore.Purged())),
                "Running Idle AutoData differs from its saved options or input purge");
        }

        private void EnsureLifecycleDeposit(PcIdleEntry entry, bool allowCreate = true)
        {
            PcCreditEvent current = ReadPcCreditEvent();
            if (entry.depositRequired && current != null && current.id == entry.depositEventId)
            {
                RequireLifecycle(current.version == 2 && current.kind == "deposit" && current.phase == 3 &&
                    current.anchor == PcCreditAnchor && current.profile == session.Profile.Id &&
                    current.before == entry.creditBefore && current.after == entry.creditAfter &&
                    current.beforeDenomination == entry.denomination && current.afterDenomination == entry.denomination &&
                    current.beforeRemainderCents == entry.remainderCents &&
                    current.afterRemainderCents == entry.remainderCents && session.Credit == entry.creditAfter,
                    "Idle deposit receipt disagrees with its saved operation");
            }
            else
            {
                RequireLifecycle(session.Credit == entry.creditBefore &&
                    (current == null ? string.IsNullOrEmpty(entry.priorCreditEventId) :
                     current.id == entry.priorCreditEventId && current.digest == entry.priorCreditEventDigest),
                    "Idle deposit prior credit receipt changed");
                if (entry.depositRequired)
                {
                    RequireLifecycle(allowCreate, "Saved Idle deposit receipt disappeared");
                    AddPcVirtualCredits(2000, true, entry.depositEventId);
                }
            }
            RequireLifecycle(session.Credit == entry.creditAfter && session.Denomination == entry.denomination &&
                ((RouletteSession)session).MoneyRemainderCents == entry.remainderCents,
                "Idle deposit did not produce its saved money amount");
        }

        private void ServicePersistedIdleEntry()
        {
            if(pcRoundArchiveSchemaVersion==4){ServiceSasIdleRuntimeV4(true);return;}
            ServicePersistedIdleEntryCore();
        }
        private void ServicePersistedIdleEntryCore()
        {
            if (!pcLifecycleRuntimeReady) return;
            pcLifecycleIdleDispatch = true;
            try
            {
                BeginLifecycleIdleEntry();
                configuredAutoPlayCheckPending = false;
                sourceGameplayState = SourceGameplayState.Idle;
                sourceBetStateEntryPending = false;
                PcIdleEntry entry = CopyIdleEntry();
                RequireLifecycle(entry.state == PcIdleState.Running && entry.step != PcIdleStep.DrawInFlight,
                    "Idle cannot repeat an in-flight draw");
                ValidateIdleAutoState(entry);
                if (entry.step > PcIdleStep.Begin) EnsureLifecycleDeposit(entry, false);
                if (entry.step == PcIdleStep.Begin)
                {
                    EnsureLifecycleDeposit(entry);
                    SaveStagedBetsIfChanged();
                    PersistIdleStep(entry, PcIdleStep.DepositDone);
                }
                if (entry.step == PcIdleStep.DepositDone)
                {
                    if (entry.eligible) PurgeSourcePlayerInputs(demoAvailable: entry.demoAvailable);
                    entry.prepareRequested = entry.eligible &&
                        (configuredAutoPlay.AutoPlay != 0 || configuredAutoPlay.Turbo);
                    PersistIdleStep(entry, PcIdleStep.CheckPurged);
                }
                if (entry.step == PcIdleStep.CheckPurged)
                {
                    entry.prepareAccepted = entry.prepareRequested && PrepareSourceSpin(true);
                    if (configuredAutoPlay.AutoPlay == 0)
                        sourceTableInputEnabled = SourceCreditMeterNonzero;
                    entry.tableInputEnabled = sourceTableInputEnabled;
                    PersistIdleStep(entry, PcIdleStep.PrepareFinished);
                }
                if (entry.step == PcIdleStep.PrepareFinished)
                {
                    sourceTableInputEnabled = entry.tableInputEnabled;
                    PurgeSourcePlayerInputs(demoAvailable: entry.demoAvailable);
                    EnableSourceDemoMode();
                    PersistIdleStep(entry, PcIdleStep.IdleEffectsDone);
                }
                if (entry.step == PcIdleStep.IdleEffectsDone)
                {
                    sourceTableInputEnabled = entry.tableInputEnabled;
                    // Acceptance is preserved even if the second purge cleared Auto.
                    if (entry.prepareAccepted)
                    {
                        CommitSourceSpin();
                        return;
                    }
                    entry.state = PcIdleState.Complete;
                    PersistIdleStep(entry, PcIdleStep.Complete);
                    SaveStagedBetsIfChanged();
                }
            }
            catch (Exception error) { FailPcRoundLifecycle("resumed", error); }
            finally { pcLifecycleIdleDispatch = false; }
        }

        private void PersistIdleDrawInFlight()
        {
            if (!HasTrustedLatestLifecycle || !pcLifecycleIdleDispatch) return;
            PcIdleEntry entry = CopyIdleEntry();
            RequireLifecycle(entry.state == PcIdleState.Running && entry.step == PcIdleStep.IdleEffectsDone &&
                entry.prepareAccepted, "Idle has no accepted draw to commit");
            PersistIdleStep(entry, PcIdleStep.DrawInFlight);
        }

        private void CompleteIdlePendingHandoff()
        {
            if (!HasTrustedLatestLifecycle || !pcLifecycleIdleDispatch) return;
            PcRoundLifecycleRecord record = GetLatestLifecycleForRestore();
            RequireLifecycle(pendingSpin != null && pendingSpin.baseLatestRoundId == record.roundArchiveId,
                "Idle pending handoff lacks the preceding round identity");
            ValidateIdleLifecycleSuccessor(record, pendingSpin.id);
            PcIdleEntry entry = CopyIdleEntry();
            entry.state = PcIdleState.Complete;
            PersistIdleStep(entry, PcIdleStep.Complete);
        }

        private string LifecycleSuccessorSpinId()
        {
            if (HasTrustedLatestLifecycle && latestRoundLifecycle.idleEntry != null &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Running &&
                latestRoundLifecycle.idleEntry.step == PcIdleStep.DrawInFlight)
                return latestRoundLifecycle.idleEntry.successorSpinId;
            return Guid.NewGuid().ToString("N");
        }

        private void EnsureCollectedLifecycleIdleEntry(PcCreditEvent item)
        {
            if (!HasTrustedLatestLifecycle || item == null || item.kind != "collect" ||
                item.phase != 3 || item.anchor != PcCreditAnchor || item.profile != session.Profile.Id) return;
            PcRoundLifecycleRecord record = GetLatestLifecycleForRestore();
            RequireLifecycle(record.roundStage == PcRoundStage.Ended, "Collect precedes the saved round End");
            if (record.idleEntry.consumedCollectEventId == item.id) return;
            RequireLifecycle(record.idleEntry.state == PcIdleState.Complete && !record.idleEntry.prepareAccepted,
                "Collect conflicts with unfinished Idle work");
            record = CopyLatestLifecycle();
            record.idleEntry = NewLifecycleIdleEntry();
            record.idleEntry.consumedCollectEventId = item.id;
            SaveLifecycleTransition(record);
        }

        private bool PersistExplicitLifecycleIdleEntry()
        {
            if (!HasTrustedLatestLifecycle) return true;
            try
            {
                PcRoundLifecycleRecord record = GetLatestLifecycleForRestore();
                RequireLifecycle(record.roundStage == PcRoundStage.Ended &&
                    record.idleEntry.state == PcIdleState.Complete && !record.idleEntry.prepareAccepted,
                    "A new Idle entry conflicts with unfinished work");
                record = CopyLatestLifecycle();
                string consumedCollect = record.idleEntry.consumedCollectEventId;
                record.idleEntry = NewLifecycleIdleEntry();
                record.idleEntry.consumedCollectEventId = consumedCollect;
                SaveLifecycleTransition(record);
                return true;
            }
            catch (Exception error)
            {
                FailPcRoundLifecycle("entered", error);
                return false;
            }
        }
    }
}
