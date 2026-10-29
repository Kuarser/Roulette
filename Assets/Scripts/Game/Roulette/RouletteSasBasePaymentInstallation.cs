using System;
using System.Linq;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private void InstallSasBasePaymentArchiveV4(string readback,RoulettePendingSettlementCandidate candidate,
            SasIntentCheckpointV1 checkpoint,string initialPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            try
            {
                if(sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress||pcSasGamePaymentV2==null||
                    pcRoundArchiveSchemaVersion!=4||checkpoint.Scope!=recordedHistoryKey||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
                    throw new PcRoundLifecycleDataException("SAS payment installation lacks verified current value");
                ValidateSasBasePaymentArchiveV4(readback,candidate,checkpoint,initialPage,priorPages,priorHistory);
                var root=new LifecycleJsonReader(readback).Read();var next=DecodeSasPaymentV2(root.Members["payment"].Text(readback),candidate.Sas.Applied);
                RequireSasPaymentSuccessorV2(pcSasGamePaymentV2,next,candidate.Sas.Applied);
                RequireSasLiveBasePaymentBaseline(session,candidate,pcSasGamePaymentV2);
                var local=session as RouletteSession;
                if(local==null||local.Credit!=pcSasGamePaymentV2.WalletAfter||local.Rounds!=candidate.RoundsAfter||
                    local.Wagered!=candidate.WageredAfter||local.Returned!=candidate.ReturnedAfter||local.Wins!=candidate.WinsAfter)
                    throw new PcRoundLifecycleDataException("SAS payment installation baseline changed");
                var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(readback);var wrappers=root.Members["pages"].Items.Select(n=>n.Text(readback)).ToArray();
                var pages=ValidateV4PageHistory(wrappers,value.historyState);var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(readback));
                RestoreLifecycleJsonNulls(root.Members["lifecycle"],lifecycle);
                recordedRoundPages.Capacity=Math.Max(recordedRoundPages.Capacity,pages.Length);sasRecordedPageWrappersV4.Capacity=Math.Max(sasRecordedPageWrappersV4.Capacity,wrappers.Length);
                if(pcSasGamePaymentV2.baseOffer!=PcGamePaymentOffer.Take&&next.baseOffer==PcGamePaymentOffer.Take&&local.DoubleAvailable)
                    local.DeclineDoubleOffer();
                if(!pcSasGamePaymentV2.moneyApplied&&next.moneyApplied)local.CompleteBaseGamePayout(next.finalAward);
                if(local.Credit!=next.WalletAfter)throw new PcRoundLifecycleDataException("SAS payment installed wallet differs");
                recordedRoundPages.Clear();recordedRoundPages.AddRange(pages);sasRecordedPageWrappersV4.Clear();sasRecordedPageWrappersV4.AddRange(wrappers);
                pcSasGamePaymentV2=next;latestRoundLifecycle=lifecycle;sasHistoryOverflowV2=root.Members["historyOverflow"].Kind=='n'?null:value.historyOverflow;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-base-payment-install",error);throw;}
        }
    }
}
