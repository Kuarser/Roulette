using System;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        // Save-only boundary. The coordinator installs effects after verified
        // readback; this method never changes the live wallet or payment receipt.
        private string SaveSasBasePaymentArchiveV4(PcGamePaymentRecordV2 next,RoulettePendingSettlementCandidate candidate,
            SasIntentCheckpointV1 checkpoint,string initialArchive,string initialPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            if(sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress)
                throw new PcRoundLifecycleDataException("SAS payment writer faulted or reentered");
            sasInitialArchiveWriteInProgress=true;
            try
            {
                if(pcRoundArchiveSchemaVersion!=4||pcSasGamePaymentV2==null||checkpoint.Scope!=recordedHistoryKey)
                    throw new PcRoundLifecycleDataException("SAS base payment has no installed owner");
                string key=recordedHistoryKey+".RoundDetails.v1";string before=PlayerPrefs.GetString(key,"");
                ValidateSasBasePaymentArchiveV4(before,candidate,checkpoint,initialPage,priorPages,priorHistory);
                var root=new LifecycleJsonReader(before).Read();var old=DecodeSasPaymentV2(root.Members["payment"].Text(before),candidate.Sas.Applied);
                RequireSasLiveBasePaymentBaseline(session,candidate,old);
                if(EncodeSasPaymentV2(old,candidate.Sas.Applied)!=EncodeSasPaymentV2(pcSasGamePaymentV2,candidate.Sas.Applied)||
                    session.Credit!=old.WalletAfter||session.Rounds!=candidate.RoundsAfter||session.Wagered!=candidate.WageredAfter||
                    session.Returned!=candidate.ReturnedAfter||session.Wins!=candidate.WinsAfter)
                    throw new PcRoundLifecycleDataException("SAS live payment baseline differs from durable value");
                RequireSasPaymentSuccessorV2(old,next,candidate.Sas.Applied);
                string proposed=BuildSasBasePaymentArchiveV4(initialArchive,next,root.Members["displayState"].Text(before),candidate,checkpoint,initialPage,priorPages,priorHistory);
                if(PlayerPrefs.GetString(key,"")!=before)throw new PcRoundLifecycleDataException("SAS payment predecessor changed");
                if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);
                else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
                string readback=PlayerPrefs.GetString(key,"");if(readback!=proposed)throw new PcRoundLifecycleDataException("SAS payment readback differs");
                ValidateSasBasePaymentArchiveV4(readback,candidate,checkpoint,initialPage,priorPages,priorHistory);return readback;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-base-payment-save",error);throw;}
            finally{sasInitialArchiveWriteInProgress=false;}
        }
    }
}
