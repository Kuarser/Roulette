using System;
using System.Linq;
using System.Text;
using System.Globalization;
using UnityEngine;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  [Serializable] private sealed class SasFrozenPayV1 {public int id;public long amount;public int winLevel;}
  [Serializable] private sealed class SasFrozenOverrideV1 {public int payIndex;public long amount;public int progressiveLevel;public string kind;}
  [Serializable] private sealed class SasFrozenInputsV1
  {
   public int frozenInputsVersion,paytableMaximumWager,tableCount,effectiveMaximumWager;
   public long staticMaxBetLimit;public string adapterPolicyId,displayPolicyId,setupPolicyId;
   public string scope,roundArchiveId,spinJournalId,profile,paytableId,paytableSha256,gameConfigSha256,pendingSpinRaw,pendingSpinSha256;
   public string priorArchiveRawSha256,priorPocketsRawSha256,priorPagesRawSha256;
   public long creditBefore,denomination,remainderCents,priorWagered,priorReturned,priorRounds,selectedChip;
   public int priorWins,priorHistoryCount;
   public bool deferred,baseDoubleAvailable;
   public RecordedBet[] bets;public int[] evaluationOrder;
   public int pocket,category;public long wager,evaluatedAward;public bool winLimitReached;
   public SasFrozenPayV1[] winningPays;public SasFrozenOverrideV1[] historyPayOverrides;
   public string retainedDrawReceiptRaw;
  }
  // Called after existing PC Pending/RNG/history restoration. Re-evaluates only.
  private RoulettePendingSettlementCandidate RestoreSasEvaluatedCandidate(SasIntentCheckpointV1 checkpoint)
  {
   if(checkpoint==null||checkpoint.Scope!=recordedHistoryKey||temporaryHistory)throw new PcRoundLifecycleDataException("SAS checkpoint restore scope differs");
   var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);
   if(frozen.frozenInputsVersion!=2||frozen.scope!=checkpoint.Scope||frozen.roundArchiveId!=checkpoint.RoundId||frozen.spinJournalId!=checkpoint.SpinId)throw new PcRoundLifecycleDataException("SAS frozen identity differs from checkpoint");
   var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("SAS restore requires PC pending session");
   var candidate=local.BuildPendingSettlementCandidate();string pendingRaw=CaptureSasIntentPendingRaw(candidate);
   if(frozen.pendingSpinRaw!=pendingRaw||frozen.retainedDrawReceiptRaw!=JsonUtility.ToJson(pcOutcome.Receipt))throw new PcRoundLifecycleDataException("SAS frozen pending or retained receipt differs from restored PC state");
   string archiveRaw=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");string pocketsRaw=PlayerPrefs.GetString(recordedHistoryKey,"");string pagesRaw=PlayerPrefs.GetString(recordedHistoryKey+".Pages","");
   RequireSasRawHistoryMatchesLoaded(pocketsRaw,pagesRaw);RequireSasArchiveMatchesLoaded(archiveRaw);
   ValidateSasEvaluatedInputs(checkpoint.FrozenInputs,candidate,pendingRaw,recordedHistoryKey,checkpoint.RoundId,checkpoint.SpinId,PcReceiptTextDigest(archiveRaw),PcReceiptTextDigest(pocketsRaw),PcReceiptTextDigest(pagesRaw),pcOutcome.Receipt);
   return candidate;
  }
  // Pure evaluated inputs. Call only after retained draw and predecessor admission.
  private static string BuildSasEvaluatedInputs(RoulettePendingSettlementCandidate candidate,string pendingSpinRaw,
   string scope,string roundId,string spinId,string priorArchiveHash,string priorPocketsHash,string priorPagesHash)
  {
   RequireSasCandidatePendingRaw(candidate,pendingSpinRaw,spinId);
   if(string.IsNullOrWhiteSpace(scope)||string.IsNullOrWhiteSpace(roundId)||roundId==spinId||
    !SasConfigEnvelopeCodecV3.IsDigest(candidate.PaytableResourceSha256)||!SasConfigEnvelopeCodecV3.IsDigest(candidate.GameConfigResourceSha256)||
    !SasConfigEnvelopeCodecV3.IsDigest(priorArchiveHash)||!SasConfigEnvelopeCodecV3.IsDigest(priorPocketsHash)||!SasConfigEnvelopeCodecV3.IsDigest(priorPagesHash))throw new PcRoundLifecycleDataException("Evaluated input identity or resource hash invalid");
   string pendingSpinHash=SasConfigSnapshotCodecV2.Digest(new UTF8Encoding(false,true).GetBytes(pendingSpinRaw));
   var text=new StringBuilder("{\"frozenInputsVersion\":2");
   Action<string,string> raw=(name,value)=>text.Append(',').Append(QuoteSasCanonical(name)).Append(':').Append(value);
   Action<string,long> number=(name,value)=>raw(name,value.ToString(CultureInfo.InvariantCulture));
   // Explicit supported PC setup; native configured table/cap adapter remains separate.
   number("paytableMaximumWager",candidate.PaytableMaximumWager);number("tableCount",1);number("staticMaxBetLimit",0);
   number("effectiveMaximumWager",SasEffectiveMaximumWager.Resolve(candidate.PaytableMaximumWager,1,0));
   raw("setupPolicyId",QuoteSasCanonical("PcSingleTableNoStaticCapV1"));raw("adapterPolicyId",QuoteSasCanonical("SingleThreadKnownDispatchV1"));raw("displayPolicyId",QuoteSasCanonical("StrictNativeType4"));
   raw("scope",QuoteSasCanonical(scope));raw("roundArchiveId",QuoteSasCanonical(roundId));raw("spinJournalId",QuoteSasCanonical(spinId));
   raw("profile",QuoteSasCanonical(candidate.ProfileId));raw("paytableId",QuoteSasCanonical(candidate.PaytableId));
   raw("paytableSha256",QuoteSasCanonical(candidate.PaytableResourceSha256));raw("gameConfigSha256",QuoteSasCanonical(candidate.GameConfigResourceSha256));
   raw("pendingSpinRaw",QuoteSasCanonical(pendingSpinRaw));raw("pendingSpinSha256",QuoteSasCanonical(pendingSpinHash));
   raw("priorArchiveRawSha256",QuoteSasCanonical(priorArchiveHash));raw("priorPocketsRawSha256",QuoteSasCanonical(priorPocketsHash));raw("priorPagesRawSha256",QuoteSasCanonical(priorPagesHash));
   number("creditBefore",candidate.CreditBefore);number("denomination",candidate.Denomination);number("remainderCents",candidate.MoneyRemainderCents);
   number("priorWagered",candidate.WageredBefore);number("priorReturned",candidate.ReturnedBefore);number("priorRounds",candidate.RoundsBefore);number("priorWins",candidate.WinsBefore);number("priorHistoryCount",candidate.PriorHistoryCount);number("selectedChip",candidate.SelectedChip);
   raw("deferred",candidate.Deferred?"true":"false");raw("baseDoubleAvailable",candidate.DoubleAvailable?"true":"false");
   var stacks=candidate.CopyChipStacks();var bets=candidate.CopyBets().OrderBy(p=>p.Key).Select(p=>
    "{\"id\":"+p.Key.ToString(CultureInfo.InvariantCulture)+",\"amount\":"+p.Value.ToString(CultureInfo.InvariantCulture)+",\"chips\":["+string.Join(",",stacks[p.Key].Select(n=>n.ToString(CultureInfo.InvariantCulture)).ToArray())+"]}");
   raw("bets","["+string.Join(",",bets.ToArray())+"]");
   raw("evaluationOrder","["+string.Join(",",candidate.CopyEvaluationOrder().Select(n=>n.ToString(CultureInfo.InvariantCulture)).ToArray())+"]");
   var basis=candidate.CopyBaseResult();number("pocket",basis.Pocket);number("wager",basis.Wager);number("evaluatedAward",basis.Returned);number("category",basis.Category);raw("winLimitReached",basis.WinLimitReached?"true":"false");
   raw("winningPays","["+string.Join(",",basis.WinningPays.Select(p=>"{\"id\":"+p.BetId.ToString(CultureInfo.InvariantCulture)+",\"amount\":"+p.Amount.ToString(CultureInfo.InvariantCulture)+",\"winLevel\":"+p.WinLevel.ToString(CultureInfo.InvariantCulture)+"}").ToArray())+"]");
   raw("historyPayOverrides","["+string.Join(",",basis.HistoryPayOverrides.Select(p=>"{\"payIndex\":"+p.PayIndex.ToString(CultureInfo.InvariantCulture)+",\"amount\":"+p.Amount.ToString(CultureInfo.InvariantCulture)+",\"progressiveLevel\":"+p.ProgressiveLevel.ToString(CultureInfo.InvariantCulture)+",\"kind\":"+QuoteSasCanonical(p.Kind.ToString())+"}").ToArray())+"]");
   return text.Append('}').ToString();
  }
  private static void ValidateSasEvaluatedInputs(string encoded,RoulettePendingSettlementCandidate candidate,string pendingRaw,
   string scope,string roundId,string spinId,string priorArchiveHash,string priorPocketsHash,string priorPagesHash,PcRngDrawReceipt retainedReceipt)
  {
   // This compares a canonical durable record against independently admitted inputs.
   // It does not reconstruct authority from the journal being checked.
   var root=new LifecycleJsonReader(encoded).Read();if(root.Kind!='{')throw new PcRoundLifecycleDataException("Frozen inputs must be an object");
   string expected=BuildSasEvaluatedInputs(candidate,pendingRaw,scope,roundId,spinId,priorArchiveHash,priorPocketsHash,priorPagesHash);
   if(retainedReceipt!=null)
   {
    string pendingFormat;var pending=ParseSasPendingSpinRaw(pendingRaw,out pendingFormat);
    RequireSasIntentDrawAdmission(retainedReceipt,false,scope,candidate.ProfileId,spinId,pending.pocket,pending.hasWheelStopIndex,pending.wheelStopIndex);
    string receiptFormat;var context=ParsePcSpinDrawContextRaw(retainedReceipt.context.contextJson,out receiptFormat);
    if(pendingFormat!=receiptFormat)throw new PcRoundLifecycleDataException("Frozen format differs from retained draw");RequireMatchingReceiptPending(pending,context,retainedReceipt);
    if(context.wagered!=candidate.WageredBefore||context.returned!=candidate.ReturnedBefore||context.rounds!=candidate.RoundsBefore||context.wins!=candidate.WinsBefore)throw new PcRoundLifecycleDataException("Retained draw accounting differs from frozen candidate");
    expected=expected.Substring(0,expected.Length-1)+",\"retainedDrawReceiptRaw\":"+QuoteSasCanonical(JsonUtility.ToJson(retainedReceipt))+"}";
   }
   if(!string.Equals(encoded,expected,StringComparison.Ordinal))throw new PcRoundLifecycleDataException("Frozen inputs differ from admitted canonical record");
  }
  private void RequireSasRawHistoryMatchesLoaded(string pocketsRaw,string pagesRaw)
  {
   var pockets=string.IsNullOrEmpty(pocketsRaw)?new string[0]:pocketsRaw.Split(',');
   if(pockets.Length!=recordedPockets.Count)throw new PcRoundLifecycleDataException("SAS raw pocket count differs from loaded history");
   for(int i=0;i<pockets.Length;i++){int n;if(!int.TryParse(pockets[i],out n)||n<0||n>36||n!=recordedPockets[i])throw new PcRoundLifecycleDataException("SAS raw pocket differs from loaded history");}
   var pages=string.IsNullOrEmpty(pagesRaw)?new string[0]:pagesRaw.Split(';');
   if(pages.Length!=recordedSnapshots.Count)throw new PcRoundLifecycleDataException("SAS raw snapshot count differs from loaded history");
   for(int i=0;i<pages.Length;i++){byte[] bytes;try{bytes=Convert.FromBase64String(pages[i]);}catch(FormatException e){throw new PcRoundLifecycleDataException("SAS raw snapshot invalid: "+e.Message);}if(!bytes.SequenceEqual(recordedSnapshots[i]))throw new PcRoundLifecycleDataException("SAS raw snapshot differs from loaded history");}
  }
  private static string SasComparableJson(object value){return value==null?"null":JsonUtility.ToJson(value);}
  private void RequireSasArchiveMatchesLoaded(string encoded)
  {
   if(string.IsNullOrEmpty(encoded))
   {
    if(HasTrustedLatestLifecycle||recordedRoundPages.Any(p=>p!=null&&p.present))throw new PcRoundLifecycleDataException("SAS durable archive missing for loaded rounds");return;
   }
   if(pcRoundArchiveSchemaVersion==4){RequireSasLifecycleOwnerV4(encoded);return;}
   RecordedRound[] rounds;PcRoundLifecycleRecord lifecycle;PcDoubleFeatureNvV1 feature;PcGamePaymentRecordV1 payment;RecordedHistoryOverflowV1 overflow;int schema;
   bool claimed=TryDecodeLifecycleArchiveWithPayment(encoded,recordedSnapshots,out rounds,out lifecycle,out feature,out payment,out overflow,out schema);
   if(claimed)
   {
    if(schema!=pcRoundArchiveSchemaVersion||SasComparableJson(lifecycle)!=SasComparableJson(latestRoundLifecycle)||SasComparableJson(feature)!=SasComparableJson(pcDoubleFeatureNv)||SasComparableJson(payment)!=SasComparableJson(pcGamePayment)||SasComparableJson(overflow)!=SasComparableJson(pcHistoryOverflow))throw new PcRoundLifecycleDataException("SAS durable archive receipts differ from loaded state");
   }
   else
   {
    var tree=new LifecycleJsonReader(encoded).Read();if(tree.Kind!='{')throw new PcRoundLifecycleDataException("SAS legacy archive must be an object");
    var legacy=JsonUtility.FromJson<RecordedRoundArchive>(encoded);if(legacy==null||legacy.rounds==null||legacy.rounds.Length!=recordedSnapshots.Count||HasTrustedLatestLifecycle)throw new PcRoundLifecycleDataException("SAS legacy archive differs from loaded state");
    rounds=legacy.rounds.Select((r,i)=>IsValidRecordedRound(r,recordedSnapshots[i])?r:new RecordedRound()).ToArray();
   }
   if(rounds.Length!=recordedRoundPages.Count)throw new PcRoundLifecycleDataException("SAS archive round count differs from loaded state");
   for(int i=0;i<rounds.Length;i++)if(SasComparableJson(rounds[i])!=SasComparableJson(recordedRoundPages[i]))throw new PcRoundLifecycleDataException("SAS archive round differs from loaded state");
  }
  private SasIntentCheckpointV1 CreateSasEvaluatedCheckpoint(string roundId)
  {
   if(temporaryHistory||recordedRawHistoryInvalid)throw new PcRoundLifecycleDataException("SAS predecessor history is not durable and valid");
   string archiveRaw=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");
   string pocketsRaw=PlayerPrefs.GetString(recordedHistoryKey,"");string pagesRaw=PlayerPrefs.GetString(recordedHistoryKey+".Pages","");
   RequireSasRawHistoryMatchesLoaded(pocketsRaw,pagesRaw);
   RequireSasArchiveMatchesLoaded(archiveRaw);
   string priorArchiveHash=PcReceiptTextDigest(archiveRaw),priorPocketsHash=PcReceiptTextDigest(pocketsRaw),priorPagesHash=PcReceiptTextDigest(pagesRaw);
   GetLatestLifecycleForRestore();
   var local=session as RouletteSession;if(local==null)throw new PcRoundLifecycleDataException("SAS evaluated checkpoint requires PC session");
   var candidate=local.BuildPendingSettlementCandidate();string pendingRaw=CaptureSasIntentPendingRaw(candidate);
   string inputs=BuildSasEvaluatedInputs(candidate,pendingRaw,recordedHistoryKey,roundId,pendingSpin.id,priorArchiveHash,priorPocketsHash,priorPagesHash);
   // Capture already validated this retained acknowledged receipt and its context.
   string receiptRaw=JsonUtility.ToJson(pcOutcome.Receipt);
   inputs=inputs.Substring(0,inputs.Length-1)+",\"retainedDrawReceiptRaw\":"+QuoteSasCanonical(receiptRaw)+"}";
   ValidateSasEvaluatedInputs(inputs,candidate,pendingRaw,recordedHistoryKey,roundId,pendingSpin.id,priorArchiveHash,priorPocketsHash,priorPagesHash,pcOutcome.Receipt);
   if(PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","")!=archiveRaw||PlayerPrefs.GetString(recordedHistoryKey,"")!=pocketsRaw||PlayerPrefs.GetString(recordedHistoryKey+".Pages","")!=pagesRaw)
    throw new PcRoundLifecycleDataException("SAS predecessor changed during checkpoint capture");
   return new SasIntentCheckpointV1(recordedHistoryKey,roundId,pendingSpin.id,inputs,0,SasRoundIntentStageV1.EvaluatedSaved,null,null,null,false,null,null,null);
  }
 }
}
