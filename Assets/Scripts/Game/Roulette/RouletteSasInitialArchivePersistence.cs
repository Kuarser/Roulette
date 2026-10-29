using System;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private bool sasInitialArchiveWriteFaulted;
        private bool sasInitialArchiveWriteInProgress;
        // Isolated validation seam; normal gameplay leaves this null.
        private Action<string,string> sasInitialArchiveWriteForTest;
        // Saves only the authoritative value. Live wallet/history installation
        // belongs to the coordinator after this returns verified durable text.
        private string SaveSasInitialArchiveV4(RoulettePendingSettlementCandidate candidate,SasIntentCheckpointV1 checkpoint,
            string newPage,string[] priorPages,SasHistoryStateV1 priorHistory,string displayRaw)
        {
            if(sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress)
                throw new PcRoundLifecycleDataException("SAS archive writer faulted or reentered");
            sasInitialArchiveWriteInProgress=true;
            try
            {
                var local=session as RouletteSession;
                if(local==null||checkpoint==null||checkpoint.Scope!=recordedHistoryKey)
                    throw new PcRoundLifecycleDataException("SAS archive write scope or session invalid");
                candidate.RequireCurrentPendingBaseline(local);
                string key=recordedHistoryKey+".RoundDetails.v1";
                string before=PlayerPrefs.GetString(key,"");string pockets=PlayerPrefs.GetString(recordedHistoryKey,"");
                string snapshots=PlayerPrefs.GetString(recordedHistoryKey+".Pages","");
                RequireSasRawHistoryMatchesLoaded(pockets,snapshots);RequireSasArchiveMatchesLoaded(before);
                string proposed=BuildSasInitialArchiveV4(candidate,checkpoint,newPage,priorPages,priorHistory,displayRaw,before,pockets,snapshots);
                if(PlayerPrefs.GetString(key,"")!=before||PlayerPrefs.GetString(recordedHistoryKey,"")!=pockets||
                    PlayerPrefs.GetString(recordedHistoryKey+".Pages","")!=snapshots)
                    throw new PcRoundLifecycleDataException("SAS predecessor changed before write");
                candidate.RequireCurrentPendingBaseline(local);
                if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);
                else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
                string readback=PlayerPrefs.GetString(key,"");
                if(readback!=proposed)throw new PcRoundLifecycleDataException("SAS archive readback differs");
                ValidateSasInitialArchiveV4(readback,candidate,checkpoint,newPage,priorPages,priorHistory);return readback;
            }
            catch(Exception error)
            {
                sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-initial-archive",error);throw;
            }
            finally{sasInitialArchiveWriteInProgress=false;}
        }
    }
}
