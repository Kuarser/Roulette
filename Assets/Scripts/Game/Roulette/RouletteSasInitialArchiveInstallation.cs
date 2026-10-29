using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private readonly List<string> sasRecordedPageWrappersV4=new List<string>();
        private PcGamePaymentRecordV2 pcSasGamePaymentV2;
        private SasHistoryOverflowV2 sasHistoryOverflowV2;
        private SasDisplayStateV1 sasDisplayStateV1;
        private string consumedSasIntentDigest;

        private RoundResult InstallSasInitialArchiveV4(string readback,RoulettePendingSettlementCandidate candidate,
            SasIntentCheckpointV1 checkpoint,string newPage,string[] priorPages,SasHistoryStateV1 priorHistory)
        {
            try
            {
                if(sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress||checkpoint.Scope!=recordedHistoryKey||
                    PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
                    throw new PcRoundLifecycleDataException("SAS installation lacks current verified durable value");
                ValidateSasInitialArchiveV4(readback,candidate,checkpoint,newPage,priorPages,priorHistory);
                var root=new LifecycleJsonReader(readback).Read();var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(readback);
                string[] wrappers=root.Members["pages"].Items.Select(n=>n.Text(readback)).ToArray();
                var pages=ValidateV4PageHistory(wrappers,value.historyState);
                var snapshots=value.historyState.snapshotsBase64.Select(Convert.FromBase64String).ToArray();
                var payment=DecodeSasPaymentV2(root.Members["payment"].Text(readback),candidate.Sas.Applied);
                var display=DecodeSasCheckpointDisplay(root.Members["displayState"].Text(readback),checkpoint);
                var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(readback));
                RestoreLifecycleJsonNulls(root.Members["lifecycle"],lifecycle);
                var overflow=root.Members["historyOverflow"].Kind=='n'?null:value.historyOverflow;
                var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("SAS install session missing");
                candidate.RequireCurrentPendingBaseline(local);
                // Complete collection allocation before publishing financial state.
                recordedRoundPages.Capacity=Math.Max(recordedRoundPages.Capacity,pages.Length);
                recordedSnapshots.Capacity=Math.Max(recordedSnapshots.Capacity,snapshots.Length);
                recordedPockets.Capacity=Math.Max(recordedPockets.Capacity,value.historyState.pockets.Length);
                sasRecordedPageWrappersV4.Capacity=Math.Max(sasRecordedPageWrappersV4.Capacity,wrappers.Length);
                var result=local.InstallSasSettlementCandidate(candidate);
                recordedRoundPages.Clear();recordedRoundPages.AddRange(pages);
                recordedSnapshots.Clear();recordedSnapshots.AddRange(snapshots);
                recordedPockets.Clear();recordedPockets.AddRange(value.historyState.pockets);
                sasRecordedPageWrappersV4.Clear();sasRecordedPageWrappersV4.AddRange(wrappers);
                latestRoundLifecycle=lifecycle;pcRoundArchiveSchemaVersion=4;pcDoubleFeatureNv=null;pcGamePayment=null;pcHistoryOverflow=null;
                pcSasGamePaymentV2=payment;sasHistoryOverflowV2=overflow;sasDisplayStateV1=display;consumedSasIntentDigest=checkpoint.ReadyDigest;
                return result;
            }
            catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-initial-install",error);throw;}
        }
    }
}
