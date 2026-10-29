using System;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private string SaveColdSasDisplayProgressV4(SasDisplayStateV1 next,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,RoundResult[] priorHistory)
  {
   if(next==null||temporaryHistory||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||sasInitialArchiveWriteInProgress||sasDisplayStateV1==null)
    throw new PcRoundLifecycleDataException("Cold display writer missing or faulted");
   sasInitialArchiveWriteInProgress=true;
   try
   {
    string key=recordedHistoryKey+".RoundDetails.v1",before=PlayerPrefs.GetString(key,"");
    RequireColdSasBasePaymentOwner(before,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory);
    var root=new LifecycleJsonReader(before).Read();var point=root.Members["displayState"];
    var checkpoint=DecodeSasPageCheckpointV4(root.Members["pages"].Items[0].Text(before));var previous=DecodeSasCheckpointDisplay(point.Text(before),checkpoint);
    SasDisplayProgressV1.RequireSuccessor(previous,next);var copy=next.Clone();
    string proposed=before.Substring(0,point.Start)+JsonUtility.ToJson(copy)+before.Substring(point.End);
    ValidateArchivedSasBaseContainerV4(proposed,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
    if(PlayerPrefs.GetString(key,"")!=before)throw new PcRoundLifecycleDataException("Cold display predecessor changed");
    if(sasInitialArchiveWriteForTest!=null)sasInitialArchiveWriteForTest(key,proposed);else{PlayerPrefs.SetString(key,proposed);PlayerPrefs.Save();}
    if(pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||PlayerPrefs.GetString(key,"")!=proposed)throw new PcRoundLifecycleDataException("Cold display readback differs");
    ValidateArchivedSasBaseContainerV4(proposed,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
    RequireColdSasBasePaymentOwner(before,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory);
    sasDisplayStateV1=copy;return proposed;
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-cold-display",error);throw;}
   finally{sasInitialArchiveWriteInProgress=false;}
  }
  private int ServiceColdSasDisplayHideV4(SasDisplayClockV1 clock,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,RoundResult[] priorHistory,long? controlledTicks=null)
  {
   if(sasDisplayStateV1==null||sasDisplayStateV1.hideCompleted)return 0;
   if(clock==null)throw new PcRoundLifecycleDataException("Cold SAS Hide lacks process clock");
   try
   {
    var captured=controlledTicks.HasValue?clock.Capture(controlledTicks.Value):clock.CaptureNow();int delay=SasDisplayProgressV1.ServicedDelayMilliseconds(captured);
    if(delay==0)captured=SasDisplayProgressV1.Hide(captured);
    if(captured.hideCompleted||captured.accruedVisibleMilliseconds-sasDisplayStateV1.accruedVisibleMilliseconds>=100)
    {SaveColdSasDisplayProgressV4(captured,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,priorHistory);clock.AcceptSaved(captured);}
    return delay;
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-cold-hide",error);throw;}
  }
 }
}
