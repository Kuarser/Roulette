using System;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private string SaveSasDisplayProgressV4(SasDisplayStateV1 next,RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,
            string initialArchive,string initialPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            if(sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress)throw new PcRoundLifecycleDataException("SAS display writer faulted or reentered");
            sasInitialArchiveWriteInProgress=true;
            try
            {
                if(pcRoundArchiveSchemaVersion!=4||pcSasGamePaymentV2==null||sasDisplayStateV1==null||checkpoint.Scope!=recordedHistoryKey)
                    throw new PcRoundLifecycleDataException("SAS display has no installed archive");
                string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");
                ValidateSasBasePaymentArchiveV4(before,candidate,checkpoint,initialPage,priorPages,priorHistory);
                var root=new LifecycleJsonReader(before).Read();var payment=DecodeSasPaymentV2(root.Members["payment"].Text(before),candidate.Sas.Applied);
                var display=DecodeSasCheckpointDisplay(root.Members["displayState"].Text(before),checkpoint);
                if(JsonUtility.ToJson(display)!=JsonUtility.ToJson(sasDisplayStateV1)||EncodeSasPaymentV2(payment,candidate.Sas.Applied)!=EncodeSasPaymentV2(pcSasGamePaymentV2,candidate.Sas.Applied))
                    throw new PcRoundLifecycleDataException("SAS display live predecessor differs");
                RequireSasLiveBasePaymentBaseline(session,candidate,payment);SasDisplayProgressV1.RequireSuccessor(display,next);
                string proposed=BuildSasBasePaymentArchiveV4(initialArchive,payment,JsonUtility.ToJson(next),candidate,checkpoint,initialPage,priorPages,priorHistory);
                if(PlayerPrefs.GetString(key,"")!=before)throw new PcRoundLifecycleDataException("SAS display predecessor changed");
                if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
                string readback=PlayerPrefs.GetString(key,"");if(readback!=proposed)throw new PcRoundLifecycleDataException("SAS display readback differs");
                ValidateSasBasePaymentArchiveV4(readback,candidate,checkpoint,initialPage,priorPages,priorHistory);
                sasDisplayStateV1=next.Clone();return readback;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-display-save",error);throw;}
            finally{sasInitialArchiveWriteInProgress=false;}
        }
    }
}
