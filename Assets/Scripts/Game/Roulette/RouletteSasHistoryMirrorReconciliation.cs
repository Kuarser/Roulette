using System;
using System.Linq;
using System.Globalization;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private bool sasHistoryMirrorWriteInProgress;
  private Action<string,string,string> sasHistoryMirrorWriteForTest;
  // V4 owns history; legacy keys are derived mirrors. An interrupted mirror
  // write is repaired again from the unchanged authoritative archive.
  private void ReconcileSasHistoryMirrorsV4(string readback,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   ReconcileSasArchiveHistoryMirrorsV4(readback,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,false);
  }
  private void ReconcileSasLifecycleHistoryMirrorsV4(string readback,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode)
  {
   ReconcileSasArchiveHistoryMirrorsV4(readback,maximumWin,doubleMaximumWin,doubleEnabled,suitMode,true);
  }
  private void ReconcileSasArchiveHistoryMirrorsV4(string readback,long maximumWin,long doubleMaximumWin,bool doubleEnabled,bool suitMode,bool lifecycleContinuation)
  {
   if(temporaryHistory||pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||sasHistoryMirrorWriteInProgress||
    profileIndex<0||profileIndex>=profiles.Length||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)
    throw new PcRoundLifecycleDataException("SAS mirror reconciliation ownership is not ready");
   try
   {
    if(lifecycleContinuation){ValidatedSasBaseContainer proof;ValidateArchivedSasLifecycleContainerV4(readback,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode,out proof);}
    else
    {
     var ownerRoot=new LifecycleJsonReader(readback).Read();
     if(ownerRoot.Members["featureNv"].Kind=='n')ValidateArchivedSasBaseContainerV4(readback,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
     else ValidateArchivedSasDoubleActiveV4(readback,profiles[profileIndex],maximumWin,doubleMaximumWin,doubleEnabled,suitMode);
    }
    var root=new LifecycleJsonReader(readback).Read();string first=root.Members["pages"].Items[0].Text(readback);
    if(DecodeSasPageCheckpointV4(first).Scope!=recordedHistoryKey)throw new PcRoundLifecycleDataException("SAS mirror scope differs");
    var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(readback);
    string pockets=string.Join(",",value.historyState.pockets.Select(p=>p.ToString(CultureInfo.InvariantCulture)).ToArray());
    string pages=string.Join(";",value.historyState.snapshotsBase64);
    if(PlayerPrefs.GetString(recordedHistoryKey,"")==pockets&&PlayerPrefs.GetString(recordedHistoryKey+".Pages","")==pages)return;
    sasHistoryMirrorWriteInProgress=true;
    if(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback)throw new PcRoundLifecycleDataException("SAS archive changed before mirror save");
    if(sasHistoryMirrorWriteForTest!=null)sasHistoryMirrorWriteForTest(recordedHistoryKey,pockets,pages);
    else{PlayerPrefs.SetString(recordedHistoryKey,pockets);PlayerPrefs.SetString(recordedHistoryKey+".Pages",pages);PlayerPrefs.Save();}
    if(pcRoundLifecycleStorageFaulted||sasInitialArchiveWriteFaulted||PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=readback||
     PlayerPrefs.GetString(recordedHistoryKey,"")!=pockets||PlayerPrefs.GetString(recordedHistoryKey+".Pages","")!=pages)
     throw new PcRoundLifecycleDataException("SAS mirror save readback differs");
   }
   catch(Exception error){sasInitialArchiveWriteFaulted=true;FailPcRoundLifecycle("sas-history-mirrors",error);throw;}
   finally{sasHistoryMirrorWriteInProgress=false;}
  }
 }
}
