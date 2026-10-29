using System;
using System.Linq;
using RouletteDeluxe.SasDraftV1;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private static void RequireSasPredecessorRawEvidence(SasIntentCheckpointV1 checkpoint,string[] pages,SasHistoryStateV1 history,
            string archiveRaw,string pocketsRaw,string snapshotsRaw)
        {
            var frozen=ParsePcReceiptContext<SasFrozenInputsV1>(checkpoint.FrozenInputs);
            if(frozen.frozenInputsVersion!=2||frozen.scope!=checkpoint.Scope||frozen.roundArchiveId!=checkpoint.RoundId||frozen.spinJournalId!=checkpoint.SpinId)
                throw new PcRoundLifecycleDataException("V4 predecessor evidence checkpoint identity differs");
            if(archiveRaw==null||pocketsRaw==null||snapshotsRaw==null||PcReceiptTextDigest(archiveRaw)!=frozen.priorArchiveRawSha256||
                PcReceiptTextDigest(pocketsRaw)!=frozen.priorPocketsRawSha256||PcReceiptTextDigest(snapshotsRaw)!=frozen.priorPagesRawSha256)
                throw new PcRoundLifecycleDataException("V4 predecessor raw hashes differ from intent");
            ValidateV4PageHistory(pages,history);
            string expectedPockets=string.Join(",",history.pockets.Select(n=>n.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray());
            if(pocketsRaw!=expectedPockets||snapshotsRaw!=string.Join(";",history.snapshotsBase64))
                throw new PcRoundLifecycleDataException("V4 predecessor history differs from admitted raw mirrors");
        }
        private static void RequireSasInitialCandidatePredecessorEvidence(RoulettePendingSettlementCandidate candidate,
            SasIntentCheckpointV1 checkpoint,string[] pages,SasHistoryStateV1 history,string archiveRaw,string pocketsRaw,string snapshotsRaw)
        {
            if(string.IsNullOrEmpty(archiveRaw)){RequireSasInitialPredecessorEvidence(checkpoint,pages,history,archiveRaw,pocketsRaw,snapshotsRaw);return;}
            var root=new LifecycleJsonReader(archiveRaw).Read();LifecycleJsonNode schema;
            if(root.Kind!='{'||!root.Members.TryGetValue("lifecycleSchemaVersion",out schema)||schema.Text(archiveRaw)!="4")
            {RequireSasInitialPredecessorEvidence(checkpoint,pages,history,archiveRaw,pocketsRaw,snapshotsRaw);return;}
            RequireSasPredecessorRawEvidence(checkpoint,pages,history,archiveRaw,pocketsRaw,snapshotsRaw);
            var local=candidate.Owner;candidate.RequireCurrentPendingBaseline(local);
            var suffix=local.Profile.Id.Substring("RouletteDeluxeSZ".Length);ValidatedSasBaseContainer proof;
            ValidateArchivedSasLifecycleContainerV4(archiveRaw,suffix,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode,out proof);
            if(proof.Checkpoint.Scope!=checkpoint.Scope||proof.Lifecycle.roundStage!=PcRoundStage.Ended||
                !pages.SequenceEqual(proof.PageWrappers)||UnityEngine.JsonUtility.ToJson(history)!=UnityEngine.JsonUtility.ToJson(proof.Archive.historyState)||
                candidate.PriorHistoryCount!=pages.Length||candidate.WageredBefore!=proof.Rounds[0].totalWagered||
                candidate.ReturnedBefore!=proof.Rounds[0].totalReturned||candidate.RoundsBefore!=proof.Rounds[0].totalRounds||candidate.WinsBefore!=proof.Rounds[0].totalWins)
                throw new PcRoundLifecycleDataException("V4 successor predecessor or accounting differs");
            ValidateIdleLifecycleSuccessor(proof.Lifecycle,checkpoint.SpinId);
            var admitted=candidate.CopyPriorHistory();
            for(int i=0;i<pages.Length;i++)
            {
                var result=ValidateCompletedSasPriorBasePageV4(pages[i],checkpoint.Scope,suffix,local.BaseMaximumWin,local.DoubleMaximumWin,local.DoubleEnabled,local.DoubleSuitMode);
                if(!RoulettePendingSettlementCandidate.SameResult(result,admitted[i]))
                    throw new PcRoundLifecycleDataException("V4 successor model history differs from resource-backed predecessor");
            }
        }
        private static void RequireSasInitialPredecessorEvidence(SasIntentCheckpointV1 checkpoint,string[] pages,SasHistoryStateV1 history,
            string archiveRaw,string pocketsRaw,string snapshotsRaw)
        {
            RequireSasPredecessorRawEvidence(checkpoint,pages,history,archiveRaw,pocketsRaw,snapshotsRaw);
            if(archiveRaw.Length==0)
            {
                if(pages.Length!=0)throw new PcRoundLifecycleDataException("V4 predecessor archive missing");return;
            }
            // Initial migration uses the frozen legacy validators. Continuing an
            // existing V4 archive requires its standalone financial decoder.
            var snapshots=history.snapshotsBase64.Select(Convert.FromBase64String).ToArray();
            var imported=ImportLegacyPagesV4(archiveRaw,snapshots);
            if(!pages.SequenceEqual(imported))throw new PcRoundLifecycleDataException("V4 imported predecessor pages differ from source archive");
        }
    }
}
