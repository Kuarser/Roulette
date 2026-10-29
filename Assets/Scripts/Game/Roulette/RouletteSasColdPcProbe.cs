using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed class RouletteSasColdPcProbe : MonoBehaviour
 {
  [Serializable] private sealed class Header {public Page[] pages;public SasHistoryStateV1 historyState;public PcGamePaymentRecordV2 payment;}
  [Serializable] private sealed class Page {public SasRoundRecordV1 sas;}
  [Serializable] private sealed class Scope {public string scope;}
  private sealed class NoDraw : IRouletteOutcomeSource {public int calls;public int DrawPocket(){calls++;throw new Exception("Frame service drew pocket");}public int DrawDoubleSuit(){calls++;throw new Exception("Frame service drew suit");}}
  private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
  private static bool finished;
  private static string raw,unhidden,evidenceRaw,scope;private static Header header;private static GameObject go;
  private static RouletteGame game;private static NoDraw source;private static MethodInfo service;
  private static int stage,constructed,checks,lastFrame=-1;private static double started;private static bool ownsKeys;
  private static PcGamePaymentRecordV2 evidence;
  private static double eventMilliseconds,eventWorstMilliseconds;private static int timedEvents;
  private static void Set(string field,object value){typeof(RouletteGame).GetField(field,Flags).SetValue(game,value);}
  private static PcGamePaymentRecordV2 Payment(){return ((PcGamePaymentRecordV2)typeof(RouletteGame).GetField("pcSasGamePaymentV2",Flags).GetValue(game)).Clone();}
  private static RouletteSession Session(){return (RouletteSession)typeof(RouletteGame).GetField("session",Flags).GetValue(game);}
  private static void Check(bool pass,string message){if(!pass)throw new Exception(message);checks++;}
  private static string Arg(string name){string[] args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);if(i<0||i+1>=args.Length)throw new Exception("Missing "+name);return args[i+1];}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  private static void Bootstrap()
  {
   if(Application.isEditor||Array.IndexOf(Environment.GetCommandLineArgs(),"-sasColdPcProbe")<0)return;
   new GameObject("SasColdPcProbeDriver").AddComponent<RouletteSasColdPcProbe>();
   try{Run();}catch(Exception error){Debug.LogException(error);Finish(1);}
  }
  private void Update(){if(!finished)Tick();}
  private static void Run()
  {
   raw=File.ReadAllText(Arg("-sasArchivedContainerTestPath"));unhidden=File.ReadAllText(Arg("-sasUnhiddenArchivePath"));evidenceRaw=File.ReadAllText(Arg("-sasPaymentEvidencePath"));
   header=JsonUtility.FromJson<Header>(raw);scope=JsonUtility.FromJson<Scope>(header.pages[0].sas.evaluationIdentity).scope;evidence=JsonUtility.FromJson<Header>(evidenceRaw).payment;
   if(!scope.StartsWith("Roulette.Test.SasFactory.",StringComparison.Ordinal))throw new Exception("Frame test requires isolated scope");
   foreach(string key in new[]{scope,scope+".Pages",scope+".RoundDetails.v1"})if(PlayerPrefs.HasKey(key))throw new Exception("Frame fixture namespace occupied");
   started=Time.realtimeSinceStartup;
   Debug.Log("SAS_COLD_PC_PROBE_STARTED unity="+Application.unityVersion+" platform="+Application.platform+" scope="+scope);
  }
  private static void Restore(string archive)
  {
   PlayerPrefs.SetString(scope,string.Join(",",header.historyState.pockets));PlayerPrefs.SetString(scope+".Pages",string.Join(";",header.historyState.snapshotsBase64));PlayerPrefs.SetString(scope+".RoundDetails.v1",archive);PlayerPrefs.Save();ownsKeys=true;
   go=new GameObject("SasColdFrameIsolated");go.SetActive(false);game=go.AddComponent<RouletteGame>();source=new NoDraw();Set("session",new RouletteSession(RouletteProfile.Load("97"),1000,source));Set("profileIndex",2);Set("recordedHistoryKey",scope);
   typeof(RouletteGame).GetMethod("RestoreArchivedSasBaseV4",Flags).Invoke(game,new object[]{archive,0L,2000000000L,false,false,source,new RoundResult[0]});
   service=typeof(RouletteGame).GetMethod("ServiceColdSasBasePaymentEventV4",Flags);
  }
  private static bool Event(bool publish,Func<PcGamePaymentRecordV2,PcGamePaymentRecordV2> transition)
  {
   long start=System.Diagnostics.Stopwatch.GetTimestamp();bool handled=(bool)service.Invoke(game,new object[]{publish,new Func<PcGamePaymentRecordV2,PcGamePaymentRecordV2>(p=>{constructed++;return transition(p);}),0L,2000000000L,false,false,new RoundResult[0]});
   if(handled){double milliseconds=(System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency;eventMilliseconds+=milliseconds;eventWorstMilliseconds=Math.Max(eventWorstMilliseconds,milliseconds);timedEvents++;}return handled;
  }
  private static void Tick()
  {
   try
   {
    if(Time.realtimeSinceStartup-started>120)throw new Exception("Frame service timed out");
    if(Time.frameCount==lastFrame)return;lastFrame=Time.frameCount;
    if(stage==0)
    {
     Restore(unhidden);Check(!Event(false,p=>PcGamePaymentTransitionsV2.Prepare(p,true,p.baseAward,evidence.routeObservation))&&constructed==0,"Hide gate constructed event");UnityEngine.Object.DestroyImmediate(go);go=null;
     Restore(raw);Set("pcGameAppSuspended",true);Check(!Event(false,p=>PcGamePaymentTransitionsV2.Prepare(p,true,p.baseAward,evidence.routeObservation))&&constructed==0,"App entry constructed event");
    }
    else if(stage==1)
    {
     Set("pcGameAppSuspended",false);Check(Event(false,p=>PcGamePaymentTransitionsV2.Prepare(p,true,p.baseAward,evidence.routeObservation)),"Prepare blocked");int count=constructed;
     Check(!Event(false,p=>PcGamePaymentTransitionsV2.PrepareQuery(p,true))&&constructed==count,"Same frame constructed second event");
    }
    else if(stage==2){Set("pcGameAppSuspended",true);Check(Event(false,p=>PcGamePaymentTransitionsV2.PrepareQuery(p,true)),"Entered service did not process query while suspended");}
    else if(stage==3)Check(Event(false,p=>PcGamePaymentTransitionsV2.SaveQueryResponse(p,true,evidence.queryResponseValue)),"Query response blocked");
    else if(stage==4)Check(Event(false,p=>PcGamePaymentTransitionsV2.PreparePacket(p,true,evidence.packetObservation)),"Packet blocked");
    else if(stage==5)Check(Event(false,p=>PcGamePaymentTransitionsV2.SaveDispatchReturn(p,true,evidence.dispatchReturnValue)),"Dispatch response blocked");
    else if(stage==6)Check(Event(false,p=>PcGamePaymentTransitionsV2.Apply(p,true))&&Session().Credit==1071,"Money apply differs");
    else if(stage==7)Check(Event(false,p=>PcGamePaymentTransitionsV2.NotifyCredit(p,true))&&Session().Credit==1071,"Credit notification paid twice");
    else if(stage==8)Check(Event(false,p=>PcGamePaymentTransitionsV2.Complete(p,true)),"Completion blocked");
    else if(stage==9){int count=constructed;Check(!Event(true,p=>PcGamePaymentTransitionsV2.Publish(p,true))&&constructed==count&&Payment().IsResolved&&!Payment().historyPublished,"Suspended publication constructed event");}
    else if(stage==10)
    {
     Set("pcGameAppSuspended",false);Check(Event(true,p=>PcGamePaymentTransitionsV2.Publish(p,true))&&Payment().historyPublished&&Session().Credit==1071,"Publication after resume differs");int count=constructed;
     Check(!Event(true,p=>PcGamePaymentTransitionsV2.Publish(p,true))&&constructed==count,"Same-frame duplicate publish constructed");
     var nextRound=Payment();nextRound.roundArchiveId=Guid.NewGuid().ToString("N");Set("pcSasGamePaymentV2",nextRound);Set("pcGameAppSuspended",true);
     Check(!Event(false,p=>p)&&constructed==count,"Next round reused entered App gate");Check(source.calls==0&&constructed==9,"Frame service redrew or event count differs");
     Debug.Log("ROULETTE_SAS_COLD_PC_PROBE_OK checks="+checks+" events="+constructed+" lastActualFrame="+Time.frameCount+" eventMeanMs="+(eventMilliseconds/timedEvents).ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+" eventWorstMs="+eventWorstMilliseconds.ToString("F3",System.Globalization.CultureInfo.InvariantCulture));Finish(0);return;
    }
    stage++;
   }
   catch(Exception error){Debug.LogException(error);Finish(1);}
  }
  private static void Finish(int code)
  {
   if(finished)return;finished=true;
   if(ownsKeys){PlayerPrefs.DeleteKey(scope);PlayerPrefs.DeleteKey(scope+".Pages");PlayerPrefs.DeleteKey(scope+".RoundDetails.v1");PlayerPrefs.Save();}
   if(go!=null)UnityEngine.Object.DestroyImmediate(go);
   Debug.Log("SAS_COLD_PC_PROBE_EXIT code="+code);Application.Quit(code);
  }
 }
}
