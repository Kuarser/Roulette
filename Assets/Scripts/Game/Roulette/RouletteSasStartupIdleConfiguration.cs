using System;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private int sasStartupIdleConfigFrame=-1;
        private string sasIdleConfigOwnerRaw;
        private RouletteSession sasIdleConfigOwnerSession;
        private string sasIdleConfigCreditRaw,sasIdleConfigTableRaw,sasIdleConfigPresentationRaw,sasIdleConfigUndoRaw;
        private Action<long> sasHostConfigSavedForTest;

        private void RequireSasIdleConfigOwner()
        {
            if(sasIdleConfigOwnerRaw==null)return;
            if(!ReferenceEquals(session,sasIdleConfigOwnerSession)||
                PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=sasIdleConfigOwnerRaw||
                PlayerPrefs.GetString(PcCreditKey,"")!=sasIdleConfigCreditRaw||PlayerPrefs.GetString(StagedBetKey,"")!=sasIdleConfigTableRaw||
                PlayerPrefs.GetString(GameResultsClearedKey,"")!=sasIdleConfigPresentationRaw||
                PlayerPrefs.GetString(SettledTableUndoKey,"")!=sasIdleConfigUndoRaw)
                throw new PcRoundLifecycleDataException("SAS Idle archive owner changed during configuration commit");
            RequireSasLifecycleOwnerV4(sasIdleConfigOwnerRaw);
            RequireSasPaidIdleSessionV4(sasIdleConfigOwnerRaw,sasIdleConfigOwnerSession);
        }
        private bool ServiceSasStartupIdleConfigurationV4()
        {
            if(!sasStartupRuntimePending || pcRoundLifecycleStorageFaulted || sasInitialArchiveWriteFaulted ||
                !pcLifecycleRuntimeReady || !pcAutoCompleteGameSelected || PcGameAppSuspended)return false;
            if(sasStartupIdleConfigFrame==Time.frameCount)return true;
            if(latestRoundLifecycle==null || latestRoundLifecycle.roundStage!=PcRoundStage.Ended)return false;
            sasStartupIdleConfigFrame=Time.frameCount;
            try
            {
                if(temporaryHistory || sasHostConfigCoordinator==null || sasHostConfigStore==null ||
                    sourceRoundPhase!=SourceRoundPhase.None || pendingSpin!=null || resumePendingSpin ||
                    spinning || session==null || session.SpinPending || session.InDoubleUp || pcCollectPending)
                    throw new PcRoundLifecycleDataException("SAS startup Idle configuration has no native Idle owner");
                var local=session as RouletteSession;
                string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");
                var proof=RequireSasLifecycleOwnerV4(before);
                if(local==null || !proof.Payment.historyPublished || proof.Lifecycle.roundStage!=PcRoundStage.Ended)
                    throw new PcRoundLifecycleDataException("SAS startup Idle configuration precedes paid End");
                RequireSasPaidIdleSessionV4(before,local);
                if(!ServicePcSasHostMessagesV1())return false;
                // Validate the disk snapshot before beginning any Idle effect.
                RequirePcSasHostConfigurationReadback();
                if(proof.Lifecycle.idleEntry.state==PcIdleState.Pending)BeginLifecycleIdleEntry();
                var entry=CopyIdleEntry();
                if(entry.state!=PcIdleState.Running || entry.step==PcIdleStep.DrawInFlight)
                    throw new PcRoundLifecycleDataException("SAS startup Idle configuration has no running entry");
                sourceGameplayState=SourceGameplayState.Idle;sourceBetStateEntryPending=false;
                sasIdleConfigOwnerRaw=PlayerPrefs.GetString(key,"");sasIdleConfigOwnerSession=local;
                sasIdleConfigCreditRaw=PlayerPrefs.GetString(PcCreditKey,"");sasIdleConfigTableRaw=PlayerPrefs.GetString(StagedBetKey,"");
                sasIdleConfigPresentationRaw=PlayerPrefs.GetString(GameResultsClearedKey,"");
                sasIdleConfigUndoRaw=PlayerPrefs.GetString(SettledTableUndoKey,"");
                RequireSasIdleConfigOwner();
                int writes=sasHostConfigCoordinator.ServiceIdleEntry(entry.idleEntryId,pcAutoCompleteGameSelected,
                    PcGameAppSuspended,pcLifecycleRuntimeReady,sourceGameplayState==SourceGameplayState.Idle,entry.step>PcIdleStep.Begin);
                RequireSasIdleConfigOwner();
                var saved=sasHostConfigStore.Load();
                if(saved==null || saved.IdleReceipt==null || saved.IdleReceipt.Scope!=recordedHistoryKey ||
                    saved.IdleReceipt.IdleEntryId!=entry.idleEntryId)
                    throw new PcRoundLifecycleDataException("SAS startup Idle lacks its durable configuration receipt");
                RequirePcSasHostConfigurationReadback();
                Debug.Log("ROULETTE_SAS_IDLE_CONFIG_READY: idle="+entry.idleEntryId+" generation="+saved.Config.Generation+
                    " writes="+writes+" lifecycleStep="+entry.step+" runtimePending=true");
                // Deposit, AutoPlay and the V4 successor writer remain separate
                // owners. Ordinary input is not released by configuration NV.
                return true;
            }
            catch(Exception error){FailPcRoundLifecycle("sas-startup-idle-config",error);return false;}
            finally{sasIdleConfigOwnerRaw=null;sasIdleConfigOwnerSession=null;}
        }
    }
}
