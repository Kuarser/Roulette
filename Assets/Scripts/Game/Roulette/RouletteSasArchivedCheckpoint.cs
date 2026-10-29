using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static SasConfigObservationReceiptV1 ArchivedSasObservation(PcSasObservationV1 payload,SasObservationPoint point)
        {
            if(payload==null)return null;
            return new SasConfigObservationReceiptV1(PcGamePaymentRecordV2.DecodeObservation(payload,point),payload.eventSource,payload.eventId,payload.eventSequence);
        }
        // Structural decision reconstruction. Resource/paytable evaluation and
        // ReadyCommit digest must still be verified against an admitted candidate.
        // The caller supplies the admitted resource suffix and resolved native win
        // limit. Neither is selected from the archive being checked.
        private static RoundResult ValidateArchivedSasBaseEvaluationV4(string encoded,
            string admittedProfileSuffix, long admittedMaximumWin)
        {
            SasFrozenInputsV1 frozen;
            DecodeSasPageCheckpointAndInputsV4(encoded,out frozen);
            return EvaluateDecodedSasBaseV4(frozen,admittedProfileSuffix,admittedMaximumWin);
        }
        private static RoundResult EvaluateDecodedSasBaseV4(SasFrozenInputsV1 frozen,string admittedProfileSuffix,long admittedMaximumWin)
        {
            var profile=RouletteProfile.Load(admittedProfileSuffix);
            if(frozen.profile!=profile.Id||frozen.paytableId!=profile.PaytableId||
                frozen.paytableSha256!=profile.PaytableResourceSha256||frozen.gameConfigSha256!=profile.GameConfigResourceSha256||
                frozen.paytableMaximumWager!=profile.MaximumTotalBet||frozen.bets==null||frozen.winningPays==null||frozen.historyPayOverrides==null||
                frozen.denomination<1||frozen.denomination>int.MaxValue||!profile.Denominations.Contains((int)frozen.denomination))
                throw new PcRoundLifecycleDataException("Archived base resource or inputs differ");
            var bets=new Dictionary<int,long>();
            foreach(var bet in frozen.bets)
            {
                WinRecord rule;
                if(bet==null||!profile.Records.TryGetValue(bet.id,out rule)||bets.ContainsKey(bet.id)||bet.chips==null||bet.chips.Length==0||
                    bet.chips.Any(chip=>chip<1)||bet.chips.Sum()!=bet.amount||bet.amount<rule.MinimumBet||
                    bet.amount>profile.MaximumSpotBet||bet.amount%rule.BetMultiple!=0)
                    throw new PcRoundLifecycleDataException("Archived base bet differs from resources");
                bets.Add(bet.id,bet.amount);
            }
            if(bets.Values.Sum()>profile.MaximumTotalBet)throw new PcRoundLifecycleDataException("Archived base wager exceeds resource maximum");
            var result=RouletteSession.EvaluateBaseSpin(profile,frozen.pocket,bets,frozen.evaluationOrder,admittedMaximumWin);
            if(result.Wager!=frozen.wager||result.Returned!=frozen.evaluatedAward||result.Category!=frozen.category||
                result.WinLimitReached!=frozen.winLimitReached||result.WinningPays.Count!=frozen.winningPays.Length||
                !result.WinningPays.Zip(frozen.winningPays,(pay,saved)=>saved!=null&&pay.BetId==saved.id&&pay.Amount==saved.amount&&pay.WinLevel==saved.winLevel).All(equal=>equal)||
                result.HistoryPayOverrides.Count!=frozen.historyPayOverrides.Length)
                throw new PcRoundLifecycleDataException("Archived base result differs from resource evaluation");
            return result;
        }
        private static SasIntentCheckpointV1 DecodeSasPageCheckpointV4(string encoded)
        {
            SasFrozenInputsV1 frozen;return DecodeSasPageCheckpointAndInputsV4(encoded,out frozen);
        }
        private static SasIntentCheckpointV1 DecodeSasPageCheckpointAndInputsV4(string encoded,out SasFrozenInputsV1 decodedInputs,bool catalogueAlreadyValidated=false)
        {
            decodedInputs=null;
            var pageRoot=new LifecycleJsonReader(encoded).Read();
            if(pageRoot.Kind!='{')throw new PcRoundLifecycleDataException("Archived SAS page must be object");
            // Catalogue validation includes the complete page/sidecar shape and round identity checks.
            if(!catalogueAlreadyValidated)ValidateV4PageCatalogue(new[]{encoded});
            var page=JsonUtility.FromJson<RecordedPageV4>(encoded);
            if(page.originSchemaVersion!=4||pageRoot.Members["sas"].Kind!='{')throw new PcRoundLifecycleDataException("Archived SAS decision requires origin4");
            string raw=pageRoot.Members["sas"].Text(encoded);
            var record=JsonUtility.FromJson<SasRoundRecordV1>(raw);var recordRoot=new LifecycleJsonReader(raw).Read();RestoreLifecycleJsonNulls(recordRoot,record);
            var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(record.evaluationIdentity);
            if(record.version!=1||frozen.frozenInputsVersion!=2||record.roundArchiveId!=frozen.roundArchiveId||record.spinJournalId!=frozen.spinJournalId||
                record.roundArchiveId!=page.round.roundArchiveId||record.spinJournalId!=page.round.spinJournalId||record.adapterPolicyId!=frozen.adapterPolicyId||record.displayPolicyId!=frozen.displayPolicyId)
                throw new PcRoundLifecycleDataException("Archived SAS identity differs from evaluation");
            var qualification=ArchivedSasObservation(record.qualification,SasObservationPoint.Qualification);
            if(qualification==null)throw new PcRoundLifecycleDataException("Archived qualification missing");
            var q=new SasQualification(qualification.Observation,frozen.evaluatedAward,frozen.wager,frozen.effectiveMaximumWager);
            var display=ArchivedSasObservation(record.displayObservation,SasObservationPoint.Display);var recorded=ArchivedSasObservation(record.recordedObservation,SasObservationPoint.Recorded);
            var arithmetic=ArchivedSasObservation(record.arithmeticObservation,SasObservationPoint.Arithmetic);
            var decision=new SasDecision(q,display==null?null:display.Observation,recorded==null?null:recorded.Observation,arithmetic==null?null:arithmetic.Observation);
            if(record.evaluatedAward!=frozen.evaluatedAward||record.eligible!=q.Eligible||record.ineligibleReason!=q.Failure.ToString()||
                record.applied!=decision.Applied||record.postMultiplierAward!=decision.PostMultiplierAward||record.recordedMultiplier!=decision.RecordedMultiplier)
                throw new PcRoundLifecycleDataException("Archived SAS decision amounts differ");
            var checkpoint=new SasIntentCheckpointV1(frozen.scope,record.roundArchiveId,record.spinJournalId,record.evaluationIdentity,q.Eligible?6:2,
                SasRoundIntentStageV1.ReadyCommit,q,qualification,display,q.Eligible,recorded,arithmetic,record.readyCommitDigest);
            decodedInputs=frozen;return checkpoint;
        }
    }
}
