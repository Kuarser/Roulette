using System;
using System.Globalization;

namespace RouletteDeluxe
{
    public enum OriginalAutoCompleteNotification
    {
        AutoCompleteStart,
        DisableComplete,
        EnableComplete
    }

    // This is event metadata. It is never serialized into the native-shaped
    // all-zero completion payload or used as an external acknowledgement.
    public enum OriginalAutoCompleteDataOrigin
    {
        OriginalRequest,
        ZeroCompletion
    }

    // Only the fields consumed by the recovered game are represented here.
    // This DTO does not claim to reproduce native struct packing/transport.
    public struct OriginalAutoCompleteRequest
    {
        public readonly uint Reason;
        public readonly bool Requested;
        public readonly bool ShowCountdown;
        public readonly uint StartSeconds;
        public readonly uint WarningSeconds;

        public OriginalAutoCompleteRequest(uint reason, bool requested,
            bool showCountdown, uint startSeconds, uint warningSeconds)
        {
            Reason = reason;
            Requested = requested;
            ShowCountdown = showCountdown;
            StartSeconds = startSeconds;
            WarningSeconds = warningSeconds;
        }

        public static OriginalAutoCompleteRequest Zero
        {
            get { return default(OriginalAutoCompleteRequest); }
        }
    }

    // Both native counters are process globals, independent of GameAutoPlayData
    // and of the lifetime of a particular Game or Timer object. The host owns
    // one production instance for the process; a cold process starts at zero.
    public sealed class OriginalAutoCompleteProcessState
    {
        public uint ElapsedMilliseconds { get; internal set; }
        public uint FlashMilliseconds { get; internal set; }

        public OriginalAutoCompleteProcessState(uint elapsedMilliseconds = 0,
            uint flashMilliseconds = 0)
        {
            ElapsedMilliseconds = elapsedMilliseconds;
            FlashMilliseconds = flashMilliseconds;
        }
    }

    public interface IOriginalAutoCompleteHost
    {
        int NativeOuterState { get; }
        bool BonusGamesEnabled { get; }
        bool CreditIsOrderedZero { get; }
        bool AutoPlayActive { get; }
        bool AppSuspended { get; }
        bool HasTimerPanel { get; }
        bool HasTextTimer { get; }
        // Object existence, not background-image existence or visibility.
        // A native MessageBox with no configured background has no rendering.
        bool HasMessageBox { get; }
        bool HasPlaySprite { get; }

        // CreateTimer must never deliver a callback synchronously. The host
        // owns scheduling; delivery is explicit through OnTimerDelivery.
        void CreateTimer(uint delayMilliseconds, uint periodMilliseconds);
        void StopTimer();
        void DeleteTimer();
        void PurgeInputs();
        void SetAutoPlayActive(bool active);
        void SetTimerPanelVisible(bool visible);
        void SetTextTimerVisible(bool visible);
        void ShowMessageBox(string message, uint durationSeconds);
        void HideMessageBox();
        void SetPlaySpriteEnabled(bool enabled);
        void NotifyPlatform(OriginalAutoCompleteNotification notification,
            OriginalAutoCompleteRequest data, OriginalAutoCompleteDataOrigin origin,
            string operationId);
        void SuspendAllFeatures();
        void ResumeAllFeatures();
    }

    // Recovered source: Game::HandleDisableWithAutoComplete 080f9670,
    // HandleEnable 080f95b0, DispatchAutoCompleteTimer 080f9850,
    // CancelAutoComplete 080f93a0 and Game::Init's timer restoration.
    // Financial settlement and ordinary Auto/StateEnd execution belong to
    // their existing owners. All scalar setters retain their own persistence.
    public sealed class OriginalAutoCompleteController
    {
        public const uint DisableReason = 0x1000;
        public const uint InitialDelayMilliseconds = 1;
        public const uint PeriodMilliseconds = 250;
        public const string CompletionMessage = "Game will finish automatically";

        private readonly OriginalAutoPlayData data;
        private readonly IOriginalAutoCompleteHost host;
        private readonly OriginalAutoCompleteProcessState process;
        private bool operating;

        public bool TimerExists { get; private set; }
        public bool TimerRunning { get; private set; }

        public OriginalAutoCompleteController(OriginalAutoPlayData data,
            IOriginalAutoCompleteHost host, OriginalAutoCompleteProcessState process)
        {
            if (data == null) throw new ArgumentNullException("data");
            if (host == null) throw new ArgumentNullException("host");
            if (process == null) throw new ArgumentNullException("process");
            this.data = data;
            this.host = host;
            this.process = process;
        }

        public void RestoreFromSavedStart()
        {
            BeginOperation();
            try
            {
                if (data.SecsToAutoCompleteStart != 0) ReplaceTimer();
            }
            finally { operating = false; }
        }

        // EndPrepared cold replay reconstructs the saved presence decision.
        // No host call, callback, counter reset or scalar write occurs here.
        // A later RestoreFromSavedStart can replace this stopped presence;
        // Start==0 leaves it stopped, as the original Init guard requires.
        public void RestoreTimerPresenceForEndReplay(bool present)
        {
            BeginOperation();
            try
            {
                if (TimerRunning || TimerExists)
                    throw new InvalidOperationException("End replay timer presence requires a fresh controller.");
                TimerExists = present;
                TimerRunning = false;
            }
            finally { operating = false; }
        }

        public void HandleDisable(OriginalAutoCompleteRequest request, string operationId)
        {
            BeginOperation();
            try
            {
                if (request.Reason != DisableReason) return;
                host.PurgeInputs();
                if ((!host.BonusGamesEnabled || host.CreditIsOrderedZero) &&
                    CanCompleteEnableOrDisable(host.NativeOuterState))
                {
                    host.NotifyPlatform(OriginalAutoCompleteNotification.DisableComplete,
                        request, OriginalAutoCompleteDataOrigin.OriginalRequest, operationId);
                    DisablePlaySprite();
                    return;
                }

                data.SetDisableWhenGameFinished(true);
                if (data.AutoPlay != 0 || host.AutoPlayActive)
                    data.SetAutoPlayOffWhenGameFinished(true);
                if (request.Requested && data.SecsToAutoCompleteStart == 0)
                {
                    data.SetSecsToAutoCompleteStart(request.StartSeconds);
                    data.SetSecsToAutoCompleteWarning(request.WarningSeconds);
                    data.SetAutoCompleteShowCountdown(request.ShowCountdown);
                    ReplaceTimer();
                }
            }
            finally { operating = false; }
        }

        public void HandleEnable(OriginalAutoCompleteRequest request, string operationId)
        {
            BeginOperation();
            try
            {
                if (request.Reason != DisableReason) return;
                CancelCore(false, operationId);
                data.SetDisableWhenGameFinished(false);
                if (CanCompleteEnableOrDisable(host.NativeOuterState))
                    host.NotifyPlatform(OriginalAutoCompleteNotification.EnableComplete,
                        request, OriginalAutoCompleteDataOrigin.OriginalRequest, operationId);
            }
            finally { operating = false; }
        }

        public bool OnTimerDelivery(string operationId)
        {
            BeginOperation();
            try
            {
                if (!TimerExists || !TimerRunning) return false;
                process.ElapsedMilliseconds = unchecked(process.ElapsedMilliseconds + PeriodMilliseconds);
                if (process.ElapsedMilliseconds > 999)
                {
                    if (data.SecsToAutoCompleteStart != 0)
                        data.SetSecsToAutoCompleteStart(data.SecsToAutoCompleteStart - 1);
                    if (data.SecsToAutoCompleteWarning != 0)
                        data.SetSecsToAutoCompleteWarning(data.SecsToAutoCompleteWarning - 1);
                    process.ElapsedMilliseconds = unchecked(process.ElapsedMilliseconds - 1000);
                }

                if (data.SecsToAutoCompleteStart == 0)
                    Expire(operationId);
                else
                    PresentCountdownAndWarning();
                return true;
            }
            finally { operating = false; }
        }

        public void CancelAutoComplete(bool complete, string operationId)
        {
            BeginOperation();
            try { CancelCore(complete, operationId); }
            finally { operating = false; }
        }

        private void BeginOperation()
        {
            if (operating)
                throw new InvalidOperationException("AutoComplete host callbacks must not reenter the controller.");
            operating = true;
        }

        private static bool CanCompleteEnableOrDisable(int state)
        {
            uint value = unchecked((uint)state);
            return value <= 11 && (0x806u & (1u << (int)value)) != 0;
        }

        private void ReplaceTimer()
        {
            // Init and a new request delete the old object without Cancel's
            // stop/reset/save sequence; both process counters are retained.
            if (TimerExists)
            {
                host.DeleteTimer();
                TimerExists = false;
                TimerRunning = false;
            }
            host.CreateTimer(InitialDelayMilliseconds, PeriodMilliseconds);
            TimerExists = true;
            TimerRunning = true;
        }

        private void CancelCore(bool complete, string operationId)
        {
            if (TimerExists)
            {
                host.StopTimer();
                TimerRunning = false;
                host.DeleteTimer();
                TimerExists = false;
                process.ElapsedMilliseconds = 0;
                data.SetSecsToAutoCompleteStart(0);
            }
            HideTimerPanel();
            if (host.HasMessageBox) host.HideMessageBox();
            if (!complete) return;
            host.PurgeInputs();
            DisablePlaySprite();
            host.NotifyPlatform(OriginalAutoCompleteNotification.DisableComplete,
                OriginalAutoCompleteRequest.Zero, OriginalAutoCompleteDataOrigin.ZeroCompletion,
                operationId);
        }

        private void Expire(string operationId)
        {
            HideTimerPanel();
            host.StopTimer();
            TimerRunning = false;
            // The stopped timer object remains present until Cancel or the
            // next Init/new request deletes it.
            if (host.NativeOuterState == 1)
            {
                if (host.HasMessageBox) host.HideMessageBox();
                data.SetDisableWhenGameFinished(false);
                data.SetAutoPlayOffWhenGameFinished(false);
                host.NotifyPlatform(OriginalAutoCompleteNotification.DisableComplete,
                    OriginalAutoCompleteRequest.Zero, OriginalAutoCompleteDataOrigin.ZeroCompletion,
                    operationId);
                DisablePlaySprite();
            }
            else
            {
                data.SetAutoPlay(1);
                data.SetAutoPlayOffWhenGameFinished(true);
                host.SetAutoPlayActive(true);
                if (host.HasMessageBox) host.ShowMessageBox(CompletionMessage, 5);
                host.NotifyPlatform(OriginalAutoCompleteNotification.AutoCompleteStart,
                    OriginalAutoCompleteRequest.Zero, OriginalAutoCompleteDataOrigin.ZeroCompletion,
                    operationId);
                if (!host.AppSuspended)
                {
                    host.SuspendAllFeatures();
                    host.ResumeAllFeatures();
                }
            }
        }

        private void PresentCountdownAndWarning()
        {
            if (data.AutoCompleteShowCountdown && host.HasTimerPanel)
            {
                // Native sprintf computes mm:ss but never supplies it to a
                // label. Only the static TextTimer image changes visibility.
                host.SetTimerPanelVisible(true);
                process.FlashMilliseconds = unchecked(process.FlashMilliseconds + PeriodMilliseconds);
                int signedFlash = unchecked((int)process.FlashMilliseconds);
                bool visible = signedFlash < 1250 || signedFlash >= 1500;
                if (host.HasTextTimer) host.SetTextTimerVisible(visible);
                if (signedFlash >= 1500) process.FlashMilliseconds = 0;
            }
            if (data.SecsToAutoCompleteWarning == 0 && host.HasMessageBox)
            {
                // The source %ld consumes a signed 32-bit argument even
                // though the persisted seconds field is uint32.
                string seconds = unchecked((int)data.SecsToAutoCompleteStart)
                    .ToString(CultureInfo.InvariantCulture);
                host.ShowMessageBox("Play must be completed in " + seconds + " seconds!", 0);
            }
        }

        private void HideTimerPanel()
        {
            if (host.HasTimerPanel) host.SetTimerPanelVisible(false);
        }

        private void DisablePlaySprite()
        {
            if (host.HasPlaySprite) host.SetPlaySpriteEnabled(false);
        }
    }
}
