using System;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private bool sasConsumedPendingWriteInProgress;
  private Action<string> sasConsumedPendingDeleteForTest;
  // Only the exact Pending frozen into the current accepted Base archive can
  // be removed. RNG acknowledgement retains the original durable receipt.
  private void ReconcileSasConsumedPendingV4(string readback)
  {
   if(temporaryHistory||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||sasConsumedPendingWriteInProgress||
    pcPreparedSpinDraw!=null||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
    throw new PcRoundLifecycleDataException("Consumed SAS Pending ownership is not ready");
   try
   {
    var root=new LifecycleJsonReader(readback).Read();string first=root.Members["pages"].Items[0].Text(readback);
    var checkpoint=DecodeSasPageCheckpointV4(first);var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);
    if(checkpoint.Scope!=recordedHistoryKey)throw new PcRoundLifecycleDataException("Consumed SAS Pending scope differs");
    if(pcOutcome!=null&&pcOutcome.Receipt!=null&&pcOutcome.Receipt.context.kind==1&&
     JsonUtility.ToJson(pcOutcome.Receipt)!=frozen.retainedDrawReceiptRaw)
    {
     var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("SAS successor has no limits");
     ValidatedSasBaseContainer proof;
     ValidateArchivedSasLifecycleContainerV4(readback,profiles[profileIndex],local.BaseMaximumWin,local.DoubleMaximumWin,
      local.DoubleEnabled,local.DoubleSuitMode,out proof);
     if(RequireSasSuccessorDrawV4(proof))return;
    }
    string raw=PlayerPrefs.GetString(PendingSpinKey,"");bool exists=PlayerPrefs.HasKey(PendingSpinKey);
    if(exists&&(raw!=frozen.pendingSpinRaw||pcOutcome==null))throw new PcRoundLifecycleDataException("Pending is not the verified consumed SAS draw");
    if(pendingSpin!=null&&(!exists||JsonUtility.ToJson(pendingSpin)!=JsonUtility.ToJson(ParseSasPendingSpinRaw(raw))))
     throw new PcRoundLifecycleDataException("Live consumed Pending differs from durable journal");
    if(!exists&&(pendingSpin!=null||resumePendingSpin))throw new PcRoundLifecycleDataException("Consumed Pending has no durable owner");
    PcRngDrawReceipt receipt=null;bool doubleReceipt=false;
    if(pcOutcome!=null)
    {
     receipt=pcOutcome.Receipt;
     if(receipt==null)throw new PcRoundLifecycleDataException("Actual RNG receipt differs from consumed SAS draw");
     PcRngDrawReceipt.Validate(receipt);
     if(JsonUtility.ToJson(receipt)!=frozen.retainedDrawReceiptRaw)
     {
      if(exists||root.Members["featureNv"].Kind=='n'||!IsSasDoubleReceiptV4(receipt))
       throw new PcRoundLifecycleDataException("Actual RNG receipt differs from consumed SAS draw");
      var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("SAS Double receipt has no limits");
      ValidatedSasDoubleEntry prior;
      ReadSasDoubleContextV4(receipt,readback,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out prior);
      doubleReceipt=true;
     }
    }
    sasConsumedPendingWriteInProgress=true;
    if(exists)
    {
     if(PlayerPrefs.GetString(PendingSpinKey,"")!=raw||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
      throw new PcRoundLifecycleDataException("Consumed Pending changed before deletion");
     if(sasConsumedPendingDeleteForTest!=null)sasConsumedPendingDeleteForTest(PendingSpinKey);
     else{PlayerPrefs.DeleteKey(PendingSpinKey);PlayerPrefs.Save();}
     if(PlayerPrefs.HasKey(PendingSpinKey))throw new PcRoundLifecycleDataException("Consumed Pending deletion readback failed");
    }
    if(pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
     throw new PcRoundLifecycleDataException("Archive changed before consumed draw acknowledgement");
    if(receipt!=null&&!doubleReceipt)pcOutcome.Acknowledge(receipt.context.operationId,receipt.contextSha256);
    pendingSpin=null;resumePendingSpin=false;pcRestoredSpinDraw=null;pcRestoredSpinReceipt=null;
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-consumed-pending",error);throw;}
   finally{sasConsumedPendingWriteInProgress=false;}
  }
 }
}
