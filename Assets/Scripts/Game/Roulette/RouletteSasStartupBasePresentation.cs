using System;
using UnityEngine;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private string sasStartupPresentationRaw;
        private RouletteSession sasStartupPresentationSession;
        private long sasStartupPresentationWallet;
        private bool sasStartupPresentationServicing;
        private bool ServiceSasStartupBasePresentationV4()
        {
            if(!sasStartupRuntimePending || pcRoundLifecycleStorageFaulted || sasInitialArchiveWriteFaulted || !pcLifecycleRuntimeReady)return false;
            if(sasStartupPresentationServicing)return false;
            sasStartupPresentationServicing=true;
            try
            {
                var local=session as RouletteSession;
                if(local==null || pcRoundArchiveSchemaVersion!=4 || pcSasGamePaymentV2==null || pcSasGamePaymentV2.owner!=PcGamePaymentOwner.Base)
                    throw new PcRoundLifecycleDataException("SAS Base presentation has no admitted owner");
                if(pcSasGamePaymentV2.prepared)
                {sasStartupPresentationRaw=null;sasStartupPresentationSession=null;return true;}
                if(local.DoubleAvailable || local.InDoubleUp || pcSasGamePaymentV2.baseOffer==PcGamePaymentOffer.Enter)return false;
                if(pcLifecycleResumePresentation)
                {
                    if(PcGameAppSuspended || !pcAutoCompleteGameSelected)return false;
                    string raw=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");
                    RequireColdSasBasePaymentOwner(raw,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,null);
                    sasStartupPresentationRaw=raw;sasStartupPresentationSession=local;sasStartupPresentationWallet=local.Credit;
                    ServicePersistedRoundPresentation();
                    Debug.Log("ROULETTE_SAS_BASE_PRESENTATION_ENTERED: wallet="+local.Credit+" award="+pcSasGamePaymentV2.baseAward+" runtimePending=true");
                    return false;
                }
                if(sasStartupPresentationRaw!=null && (!ReferenceEquals(local,sasStartupPresentationSession) ||
                    local.Credit!=sasStartupPresentationWallet || PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=sasStartupPresentationRaw))
                    throw new PcRoundLifecycleDataException("SAS Base owner changed during presentation");
                // TickWinRacking stays on the existing Game update path. Only
                // real returned display-wait services can finish this gate.
                ServicePcResultDisplayWait();
                return sourceRoundPhase==SourceRoundPhase.ResultDisplay && !winRackActive && !ResultDisplayActive;
            }
            catch(Exception error){FailPcRoundLifecycle("sas-startup-base-presentation",error);return false;}
            finally{sasStartupPresentationServicing=false;}
        }
    }
}
