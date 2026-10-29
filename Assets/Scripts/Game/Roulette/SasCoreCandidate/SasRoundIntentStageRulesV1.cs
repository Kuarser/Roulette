using System;
namespace RouletteDeluxe.SasDraftV1
{
 public enum SasRoundIntentStageV1
 {
  EvaluatedSaved=1,QualificationSaved=2,ShowPlanned=3,ShownSaved=4,
  RecordedSaved=5,ArithmeticSaved=6,ReadyCommit=7
 }
 public static class SasRoundIntentStageRulesV1
 {
  public static void RequireTransition(SasRoundIntentStageV1 before,SasRoundIntentStageV1 after,bool qualified)
  {
   if(!Enum.IsDefined(typeof(SasRoundIntentStageV1),before)||!Enum.IsDefined(typeof(SasRoundIntentStageV1),after))
    throw new ArgumentException("Unsupported SAS intent stage");
   bool allowed=before==SasRoundIntentStageV1.EvaluatedSaved && after==SasRoundIntentStageV1.QualificationSaved;
   if(qualified)allowed|=(before>=SasRoundIntentStageV1.QualificationSaved && before<SasRoundIntentStageV1.ReadyCommit && (int)after==(int)before+1);
   else allowed|=before==SasRoundIntentStageV1.QualificationSaved && after==SasRoundIntentStageV1.ReadyCommit;
   if(!allowed)throw new InvalidOperationException("SAS intent cannot skip, repeat or reverse its saved stage");
  }
  public static void ValidateStagePayload(SasRoundIntentStageV1 stage,SasQualification qualification,
   SasObservation display,bool shownSuccessfully,SasObservation recorded,SasObservation arithmetic,string readyDigest)
  {
   if(!Enum.IsDefined(typeof(SasRoundIntentStageV1),stage))throw new ArgumentException("Unsupported SAS intent stage");
   if(stage==SasRoundIntentStageV1.EvaluatedSaved)
   {
    if(qualification!=null || display!=null || shownSuccessfully || recorded!=null || arithmetic!=null || readyDigest!=null)
     throw new ArgumentException("Evaluated stage contains future work");
    return;
   }
   if(qualification==null)throw new ArgumentException("Saved qualification required");
   if(!qualification.Eligible)
   {
    if((stage!=SasRoundIntentStageV1.QualificationSaved && stage!=SasRoundIntentStageV1.ReadyCommit)||
     display!=null || shownSuccessfully || recorded!=null || arithmetic!=null)
     throw new ArgumentException("Unqualified intent cannot carry application work");
   }
   else
   {
    bool displayRequired=stage>=SasRoundIntentStageV1.ShowPlanned;
    bool shownRequired=stage>=SasRoundIntentStageV1.ShownSaved;
    bool recordedRequired=stage>=SasRoundIntentStageV1.RecordedSaved;
    bool arithmeticRequired=stage>=SasRoundIntentStageV1.ArithmeticSaved;
    if((display!=null)!=displayRequired || shownSuccessfully!=shownRequired ||
     (recorded!=null)!=recordedRequired || (arithmetic!=null)!=arithmeticRequired)
     throw new ArgumentException("SAS saved stage evidence incomplete or premature");
    if(display!=null)SasObservation.Require(display,SasObservationPoint.Display);
    if(recorded!=null)SasObservation.Require(recorded,SasObservationPoint.Recorded);
    if(arithmetic!=null)SasObservation.Require(arithmetic,SasObservationPoint.Arithmetic);
   }
   if(stage==SasRoundIntentStageV1.ReadyCommit)
   {if(!SasConfigEnvelopeCodecV3.IsDigest(readyDigest))throw new ArgumentException("ReadyCommit digest required");}
   else if(readyDigest!=null)throw new ArgumentException("Ready digest precedes completed arithmetic");
  }
 }
}
