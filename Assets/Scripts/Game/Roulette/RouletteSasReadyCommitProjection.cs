using System;
using System.Linq;
using System.Text;
using System.Globalization;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
 public sealed partial class RouletteGame
 {
  // Pure canonical ReadyCommit projection. Stage/display progress/digest are
  // excluded. Actual durable intent must supply validated predecessor hashes.
  private static string BuildSasReadyCommitProjection(RoulettePendingSettlementCandidate candidate,
   string decisionRaw,string scope,string roundId,string spinId,string resourceHash,
   int paytableMaximum,int tableCount,long staticCap,string adapterPolicy,string displayPolicy,
   SasConfigObservationReceiptV1[] observations,string pendingSpinRaw,string pendingSpinHash,
   string priorArchiveHash,string priorPocketsHash,string priorPagesHash)
  {
   if(pendingSpinRaw==null || SasConfigSnapshotCodecV2.Digest(Encoding.UTF8.GetBytes(pendingSpinRaw))!=pendingSpinHash ||
    !SasConfigEnvelopeCodecV3.IsDigest(priorArchiveHash)||!SasConfigEnvelopeCodecV3.IsDigest(priorPocketsHash)||!SasConfigEnvelopeCodecV3.IsDigest(priorPagesHash))
    throw new PcRoundLifecycleDataException("ReadyCommit predecessor or pending bytes invalid");
   ValidateCandidateSasDecisionEvidence(candidate,decisionRaw,scope,roundId,spinId,resourceHash,paytableMaximum,tableCount,staticCap,adapterPolicy,displayPolicy);
   ValidateSasDecisionProvenance(decisionRaw,observations);
   string decision=CanonicalizeSasDecisionEvidence(decisionRaw,scope,roundId,spinId);
   var text=new StringBuilder("{\"projectionVersion\":1");
   Action<string,string> raw=(name,value)=>text.Append(',').Append(QuoteSasCanonical(name)).Append(':').Append(value);
   Action<string,long> number=(name,value)=>raw(name,value.ToString(CultureInfo.InvariantCulture));
   raw("pendingSpinRaw",QuoteSasCanonical(pendingSpinRaw));raw("pendingSpinSha256",QuoteSasCanonical(pendingSpinHash));
   raw("priorArchiveRawSha256",QuoteSasCanonical(priorArchiveHash));raw("priorPocketsRawSha256",QuoteSasCanonical(priorPocketsHash));raw("priorPagesRawSha256",QuoteSasCanonical(priorPagesHash));
   number("creditBefore",candidate.CreditBefore);number("denomination",candidate.Denomination);number("remainderCents",candidate.MoneyRemainderCents);
   number("priorWagered",candidate.WageredBefore);number("priorReturned",candidate.ReturnedBefore);number("priorRounds",candidate.RoundsBefore);number("priorWins",candidate.WinsBefore);number("priorHistoryCount",candidate.PriorHistoryCount);number("selectedChip",candidate.SelectedChip);
   raw("deferred",candidate.Deferred?"true":"false");raw("doubleAvailable",candidate.DoubleAvailable?"true":"false");
   var stacks=candidate.CopyChipStacks();var bets=candidate.CopyBets().OrderBy(p=>p.Key).Select(p=>
    "{\"id\":"+p.Key.ToString(CultureInfo.InvariantCulture)+",\"amount\":"+p.Value.ToString(CultureInfo.InvariantCulture)+",\"chips\":["+string.Join(",",stacks[p.Key].Select(n=>n.ToString(CultureInfo.InvariantCulture)).ToArray())+"]}");
   raw("bets","["+string.Join(",",bets.ToArray())+"]");
   raw("evaluationOrder","["+string.Join(",",candidate.CopyEvaluationOrder().Select(n=>n.ToString(CultureInfo.InvariantCulture)).ToArray())+"]");
   var basis=candidate.CopyBaseResult();number("pocket",basis.Pocket);number("wager",basis.Wager);number("evaluatedAward",basis.Returned);number("category",basis.Category);raw("winLimitReached",basis.WinLimitReached?"true":"false");
   raw("winningPays","["+string.Join(",",basis.WinningPays.Select(p=>"{\"id\":"+p.BetId.ToString(CultureInfo.InvariantCulture)+",\"amount\":"+p.Amount.ToString(CultureInfo.InvariantCulture)+",\"winLevel\":"+p.WinLevel.ToString(CultureInfo.InvariantCulture)+"}").ToArray())+"]");
   raw("decision",decision);return text.Append('}').ToString();
  }
  private static string SasReadyCommitDigest(string projection)
  {if(projection==null)throw new ArgumentNullException("projection");return SasConfigSnapshotCodecV2.Digest(new UTF8Encoding(false,true).GetBytes(projection));}
 }
}
