using System;
using System.Threading;
namespace RouletteDeluxe.SasDraftV1
{
 public sealed class SasIntentCheckpointV1
 {
  public readonly string Scope,RoundId,SpinId,FrozenInputs;
  public readonly long Revision;
  public readonly SasRoundIntentStageV1 Stage;
  public readonly SasQualification Qualification;
  public readonly SasConfigObservationReceiptV1 QualificationReceipt;
  public readonly SasConfigObservationReceiptV1 Display,Recorded,Arithmetic;
  public readonly bool ShownSuccessfully;
  public readonly string ReadyDigest;
  public SasIntentCheckpointV1(string scope,string roundId,string spinId,string frozenInputs,long revision,
   SasRoundIntentStageV1 stage,SasQualification qualification,SasConfigObservationReceiptV1 qualificationReceipt,SasConfigObservationReceiptV1 display,
   bool shown,SasConfigObservationReceiptV1 recorded,SasConfigObservationReceiptV1 arithmetic,string readyDigest)
  {
   if(string.IsNullOrWhiteSpace(scope)||string.IsNullOrWhiteSpace(roundId)||string.IsNullOrWhiteSpace(spinId)||roundId==spinId||
    string.IsNullOrWhiteSpace(frozenInputs)||revision<0)throw new ArgumentException("Invalid checkpoint identity");
   SasRoundIntentStageRulesV1.ValidateStagePayload(stage,qualification,display==null?null:display.Observation,shown,
    recorded==null?null:recorded.Observation,arithmetic==null?null:arithmetic.Observation,readyDigest);
   if((qualification==null)!=(qualificationReceipt==null) || (qualification!=null && !SasIntentCheckpointCommitterV1.SameObservation(qualification.Observation,qualificationReceipt.Observation)))throw new ArgumentException("Qualification receipt differs");
   long expectedRevision=stage==SasRoundIntentStageV1.ReadyCommit&&qualification!=null&&!qualification.Eligible?2:(long)stage-1;
   if(revision!=expectedRevision)throw new ArgumentException("Checkpoint revision differs from durable stage path");
   QualificationReceipt=qualificationReceipt;
   Scope=scope;RoundId=roundId;SpinId=spinId;FrozenInputs=frozenInputs;Revision=revision;Stage=stage;
   Qualification=qualification;Display=display;ShownSuccessfully=shown;Recorded=recorded;Arithmetic=arithmetic;ReadyDigest=readyDigest;
  }
 }
 // Typed transition/publish coordinator only. The persistence callback must
 // encode, validate and durably save the full journal before returning readback.
 public sealed class SasIntentCheckpointCommitterV1
 {
  private readonly int owner=Thread.CurrentThread.ManagedThreadId;
  private readonly Func<long,SasIntentCheckpointV1,SasIntentCheckpointV1> save;
  private bool committing;
  public SasIntentCheckpointV1 Current {get;private set;}
  public bool Faulted {get;private set;}
  public SasIntentCheckpointCommitterV1(SasIntentCheckpointV1 validatedSaved,Func<long,SasIntentCheckpointV1,SasIntentCheckpointV1> persistence)
  {if(validatedSaved==null||persistence==null)throw new ArgumentNullException();Current=validatedSaved;save=persistence;}
  public void Commit(SasIntentCheckpointV1 candidate)
  {
   if(Thread.CurrentThread.ManagedThreadId!=owner || committing)throw new InvalidOperationException("Checkpoint owner or recursion violation");
   if(Faulted)throw new InvalidOperationException("Checkpoint committer faulted");
   committing=true;
   try
   {
    RequireSuccessor(Current,candidate);
    var readback=save(Current.Revision,candidate);
    if(readback==null || !Equivalent(candidate,readback))throw new InvalidOperationException("Checkpoint readback differs");
    Current=readback;
   }
   catch{Faulted=true;throw;}
   finally{committing=false;}
  }
  internal static void RequireSuccessor(SasIntentCheckpointV1 current,SasIntentCheckpointV1 candidate)
  {
    if(candidate==null || candidate.Revision!=checked(current.Revision+1)||!SameIdentity(current,candidate))throw new InvalidOperationException("Checkpoint frozen input or revision changed");
    SasRoundIntentStageRulesV1.RequireTransition(current.Stage,candidate.Stage,candidate.Qualification!=null&&candidate.Qualification.Eligible);
    if(current.Qualification!=null&&(!SameQualification(current.Qualification,candidate.Qualification)||!SameReceipt(current.QualificationReceipt,candidate.QualificationReceipt)))throw new InvalidOperationException("Saved qualification changed");
    if((current.Display!=null&&!SameReceipt(current.Display,candidate.Display)) || (current.Recorded!=null&&!SameReceipt(current.Recorded,candidate.Recorded)) || (current.Arithmetic!=null&&!SameReceipt(current.Arithmetic,candidate.Arithmetic)))throw new InvalidOperationException("Saved observation changed");
  }
  private static bool SameIdentity(SasIntentCheckpointV1 a,SasIntentCheckpointV1 b)
  {return a.Scope==b.Scope&&a.RoundId==b.RoundId&&a.SpinId==b.SpinId&&a.FrozenInputs==b.FrozenInputs;}
  internal static bool SameObservation(SasObservation a,SasObservation b)
  {
   if(a==null||b==null)return a==b;
   if(a.Point!=b.Point||a.Generation!=b.Generation||a.RuntimePendingDisableObserved!=b.RuntimePendingDisableObserved)return false;
   byte[] x=a.Payload.CopyBytes(),y=b.Payload.CopyBytes();for(int i=0;i<x.Length;i++)if(x[i]!=y[i])return false;return true;
  }
  private static bool SameQualification(SasQualification a,SasQualification b)
  {if(a==null||b==null)return a==b;return a.EvaluatedAward==b.EvaluatedAward&&a.Wager==b.Wager&&a.EffectiveMaximumWager==b.EffectiveMaximumWager&&a.Failure==b.Failure&&SameObservation(a.Observation,b.Observation);}
  private static bool SameReceipt(SasConfigObservationReceiptV1 a,SasConfigObservationReceiptV1 b)
  {if(a==null||b==null)return a==b;return a.EventSource==b.EventSource&&a.EventId==b.EventId&&a.EventSequence==b.EventSequence&&SameObservation(a.Observation,b.Observation);}
  private static bool Equivalent(SasIntentCheckpointV1 a,SasIntentCheckpointV1 b)
  {return SameIdentity(a,b)&&a.Revision==b.Revision&&a.Stage==b.Stage&&SameQualification(a.Qualification,b.Qualification)&&SameReceipt(a.QualificationReceipt,b.QualificationReceipt)&&SameReceipt(a.Display,b.Display)&&SameReceipt(a.Recorded,b.Recorded)&&SameReceipt(a.Arithmetic,b.Arithmetic)&&a.ShownSuccessfully==b.ShownSuccessfully&&a.ReadyDigest==b.ReadyDigest;}
 }
}
