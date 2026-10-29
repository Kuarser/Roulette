using System;
using System.Diagnostics;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private OriginalResultDisplayWaitClock pcResultDisplayWait;
        private bool pcResultDisplayWaitServicing;
        private bool pcResultDisplayWaitPromptScheduled;
        private int pcResultDisplayWaitFrame = -1;

        private void BeginPcResultDisplayWait(long paid)
        {
            pcResultDisplayWait = new OriginalResultDisplayWaitClock(paid,
                resultSettledTimestamp, Stopwatch.Frequency, new PcResultDisplayWaitHost(this));
            // Each new native WaitInput first returns through App::Idle.
            // A fresh wait cannot consume the input snapshot that armed it.
            pcResultDisplayWaitFrame = Time.frameCount;
            pcResultDisplayWaitPromptScheduled = false;
            // This field records the nominal timer budget for diagnostics;
            // it is not an absolute deadline or the financial completion gate.
            resultDisplayDeadlineTimestamp = autoplay ? OriginalRackClock.AfterMilliseconds(
                resultSettledTimestamp, paid == 0 ? 700 : 1050, Stopwatch.Frequency) : 0;
            resultDisplayUntil = resultSettledAt + (autoplay ? paid == 0 ? .7f : 1.05f : 0f);
            promptReadyTimestamp = long.MaxValue;
            promptDelayedPlayTimestamp = 0;
            PublishPcResultDisplayWait();
            LifecycleCheckpoint("ResultDisplayWaitEntered");
        }

        private void ServicePcResultDisplayWait()
        {
            if (pcResultDisplayWait == null || pcResultDisplayWait.Completed ||
                (PcGameAppSuspended && !pcAutoCompleteGameSelected) || pcResultDisplayWaitServicing ||
                pcResultDisplayWaitFrame == Time.frameCount) return;
            pcResultDisplayWaitFrame = Time.frameCount;
            pcResultDisplayWaitServicing = true;
            try
            {
                bool changed = pcResultDisplayWait.UpdateAt(Stopwatch.GetTimestamp());
                PublishPcResultDisplayWait();
                if (changed) LifecycleCheckpoint("ResultDisplayWaitServiced");
            }
            finally { pcResultDisplayWaitServicing = false; }
        }

        private void PublishPcResultDisplayWait()
        {
            configuredResultDelayStartTimestamp = pcResultDisplayWait.HighDelayStartedAt;
            configuredResultDelayDeadlineTimestamp = pcResultDisplayWait.HighDelayDueTimestamp;
            if (!pcResultDisplayWait.Completed || pcResultDisplayWaitPromptScheduled) return;
            pcResultDisplayWaitPromptScheduled = true;
            ScheduleResultIdlePrompt(Stopwatch.Frequency);
        }

        private sealed class PcResultDisplayWaitHost : IOriginalResultDisplayWaitHost
        {
            private readonly RouletteGame game;
            internal PcResultDisplayWaitHost(RouletteGame owner) { game = owner; }
            public bool GameSuspended { get { return game.PcGameAppSuspended; } }
            public bool AutoPlayActive { get { return game.autoplay; } }
            public bool ConfiguredAutoPlayEligible { get { return game.ConfiguredAutoPlayEligible; } }
            public long CurrentAward { get { return game.PcCurrentRoundAward; } }
            public bool InputAvailable { get { return game.SourcePlayerInputs.InputAvailable(0, 0); } }
            public long Timestamp { get { return Stopwatch.GetTimestamp(); } }
            public void DispatchOneInput() { game.SourcePlayerInputs.DispatchOne(); }
        }
    }
}
