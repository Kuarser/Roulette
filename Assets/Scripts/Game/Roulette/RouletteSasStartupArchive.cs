using System;
using System.Linq;
using UnityEngine;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  private string sasStartupArchiveRaw,sasStartupProfile;
  // Admission is now part of startup. Runtime dispatch remains a separate
  // owner until the V4 controller, Double and successor writer are connected.
  private bool sasStartupRuntimePending;
  private bool StageSasStartupArchiveV4(string encoded)
  {
   var root=new LifecycleJsonReader(encoded).Read();LifecycleJsonNode version;
   if(root.Kind!='{'||!root.Members.TryGetValue("lifecycleSchemaVersion",out version)||version.Text(encoded)!="4")return false;
   ValidateLifecycleJsonShape(root,typeof(RecordedRoundArchiveV4),encoded);
   var value=JsonUtility.FromJson<RecordedRoundArchiveV4>(encoded);
   if(value.lifecycleSchemaVersion!=4||value.pages==null||value.pages.Length==0)
    throw new PcRoundLifecycleDataException("Startup V4 envelope invalid");
   string[] wrappers=root.Members["pages"].Items.Select(n=>n.Text(encoded)).ToArray();
   var pages=ValidateV4PageHistory(wrappers,value.historyState);
   SasFrozenInputsV1 frozen;var checkpoint=DecodeSasPageCheckpointAndInputsV4(wrappers[0],out frozen);
   if(checkpoint.Scope!=recordedHistoryKey||!pages[0].present||!profiles.Any(p=>"RouletteDeluxeSZ"+p==frozen.profile))
    throw new PcRoundLifecycleDataException("Startup V4 namespace or profile invalid");
   // These are untrusted staging hints. No lifecycle, money or payment cache
   // is published before actual configured limits and RNG ownership validate.
   sasStartupArchiveRaw=encoded;sasStartupProfile=frozen.profile;return true;
  }
  private void RestoreSasStartupProfileV4()
  {
   sasStartupRuntimePending=true;
   try
   {
    if(temporaryHistory||outcome==null||pcOutcome==null||sasStartupArchiveRaw==null)
     throw new PcRoundLifecycleDataException("Startup V4 requires the PC outcome owner");
    var args=Environment.GetCommandLineArgs();bool suit=Array.IndexOf(args,"-rouletteSuitMode")>=0;
    int at=Array.IndexOf(args,"-rouletteBaseWinLimitCredits");long maximum=0;
    if(at>=0&&(at+1>=args.Length||!long.TryParse(args[at+1],out maximum)||maximum<0))throw new ArgumentException("Invalid PC base win limit in credits");
    var local=new RouletteSession(RouletteProfile.Load(profiles[profileIndex]),1000,outcome,doubleSuitMode:suit,baseMaximumWin:maximum);
    local.DoubleEnabled=true;local.EnableDeferredBaseSettlement();
    var retired=PlanSasPredecessorJournalsV4(sasStartupArchiveRaw,local);
    RequireSasPaidIdleJournalsV4(sasStartupArchiveRaw,local,retired);
    RetireSasPredecessorJournalsV4(sasStartupArchiveRaw,retired);session=local;
    var root=new LifecycleJsonReader(sasStartupArchiveRaw).Read();
    var lifecycle=ParsePcReceiptContext<PcRoundLifecycleRecord>(root.Members["lifecycle"].Text(sasStartupArchiveRaw));
    bool continuation=lifecycle.roundStage==PcRoundStage.EndPrepared||lifecycle.roundStage==PcRoundStage.Ended;
    RestoreSasArchiveV4(sasStartupArchiveRaw,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,outcome,null,continuation);
    if(pcOutcome.Receipt!=null&&pcOutcome.Receipt.context.kind==2)ApplySasDoubleReceiptV4(pcOutcome.Receipt);
    if(PlayerPrefs.HasKey(SettledTableUndoKey)&&!RestoreSettledTableUndo())
     throw new PcRoundLifecycleDataException("Admitted V4 settled Undo did not restore");
    RecordedRound round=recordedRoundPages[0];
    wheelRenderedAngle=RestoredWheelPhase(round);
    wheelMainRenderAngle=round.sourceWheelStopRecorded?OriginalWheelArithmetic.RestoreMainAngle(round.sourceWheelStopIndex):OriginalWheelArithmetic.RenderAngle(wheelRenderedAngle);
    wheelSpindleRenderAngle=OriginalWheelArithmetic.RenderAngle(wheelRenderedAngle);wheelAngle=wheelRenderedAngle;
    wheelBallPosition=RestoredBallCenter(round);wheelMotionReady=true;wheelBallVisible=true;
    if(!((RouletteSession)session).RoundPayoutPending){footerMoviePhase=FooterMoviePhase.Hidden;footerMovieClock=null;}
    RestorePcCreditEvent();
    bool successor=pcOutcome.Receipt!=null&&pcOutcome.Receipt.context.kind==1&&
     pcOutcome.Receipt.context.operationId!=latestRoundLifecycle.spinJournalId;
    if(successor){LoadPendingSpinJournal();SelectPcReceiptProfile();ReconcilePcRngReceipt();
     resumePendingSpin=TryRestorePendingSpin(true,false);if(!resumePendingSpin)throw new PcRoundLifecycleDataException("Admitted SAS successor did not restore");
     AcknowledgePcSpinDraw();ValidatePendingSasIntentAtStartupV4();gameOverMessageLatched=false;sasStartupRuntimePending=false;}
    LoadStagedBetJournal();
    if(stagedBets!=null&&!resumePendingSpin)TryRestoreStagedBets();
    EnsureCollectedLifecycleIdleEntry(pcCreditOverride);
    RestoreSourceRoundLifecycle();RestoreSourceTableInput();
    if(!resumePendingSpin){RestoreClearedGameResults();RestoreEditedTableResultPresentation();}
    status="Recovered session is waiting for game services.";
    Debug.Log("ROULETTE_SAS_STARTUP_FINANCIAL_RESTORED: wallet="+session.Credit+" history="+session.History.Count+" stage="+latestRoundLifecycle.roundStage+" runtimePending="+sasStartupRuntimePending);
   }
   catch(Exception error){FailPcRoundLifecycle("sas-startup",error);}
  }
 }
}
