using System;
using UnityEngine;
using Stopwatch=System.Diagnostics.Stopwatch;
namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private int sasStartupEndServiceFrame=-1;
        private bool sasStartupEndServicing;
        private bool ServiceSasStartupRoundEndV4()
        {
            if(!sasStartupRuntimePending || pcRoundLifecycleStorageFaulted || sasInitialArchiveWriteFaulted ||
                !pcLifecycleRuntimeReady || !pcAutoCompleteGameSelected || PcGameAppSuspended ||
                PaytableTestActive || OperatorFallbackActive)return false;
            if(latestRoundLifecycle==null ||
                latestRoundLifecycle.roundStage!=PcRoundStage.AwaitEnd&&latestRoundLifecycle.roundStage!=PcRoundStage.EndPrepared)return false;
            if(sasStartupPaymentServiceFrame==Time.frameCount || sasStartupEndServiceFrame==Time.frameCount)return false;
            if(sasStartupEndServicing)throw new PcRoundLifecycleDataException("SAS startup End reentered");
            // A RoundOpen restored presentation still belongs to its native
            // display controller. Financial publication cannot skip that work.
            if(pcLifecycleResumePresentation || sourceRoundPhase!=SourceRoundPhase.ResultDisplay ||
                spinning || session==null || session.SpinPending || winRackActive || ResultDisplayActive ||
                session.DoubleAvailable || session.InDoubleUp || doubleCollectPending || IsDoubleReveal || IsDoubleExit)return false;
            sasStartupEndServiceFrame=Time.frameCount;sasStartupEndServicing=true;
            try
            {
                string before=PlayerPrefs.GetString(recordedHistoryKey+".RoundDetails.v1","");
                var proof=RequireSasLifecycleOwnerV4(before);
                if(!proof.Payment.historyPublished || !proof.Payment.IsResolved ||
                    proof.Lifecycle.roundStage!=PcRoundStage.AwaitEnd&&proof.Lifecycle.roundStage!=PcRoundStage.EndPrepared)
                    throw new PcRoundLifecycleDataException("SAS End precedes resolved paid history");
                long now=Stopwatch.GetTimestamp();
                if(!pcLifecycleColdRound && !OriginalRackClock.GameDelayExpired(gameStartTimestamp,
                    now,gameDelayMilliseconds,Stopwatch.Frequency))
                {ServiceSourceEndingDelayPrompt(now);return false;}
                if(sourceEndingDelayPromptDeadline!=0){ClearSourceEndingDelayPrompt();proViewFlashStartedTimestamp=0;}
                gameOverMessageLatched=true;
                // Original routine freezes End V2 once, resumes saved effects
                // after cold load, and writes Ended before publishing Idle.
                CompletePersistedRoundEnd();
                Debug.Log("ROULETTE_SAS_STARTUP_END_READY: round="+latestRoundLifecycle.roundArchiveId+
                    " stage="+latestRoundLifecycle.roundStage+" wallet="+session.Credit+" runtimePending=true");
                return true;
            }
            catch(Exception error){FailPcRoundLifecycle("sas-startup-end-runtime",error);return false;}
            finally{sasStartupEndServicing=false;}
        }
    }
}
