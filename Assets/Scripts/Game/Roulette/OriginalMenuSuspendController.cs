using System;

namespace RouletteDeluxe
{
    public enum OriginalMenuResumeDisposition
    {
        Pending,
        Diagnostics,
        Dispatched
    }

    // These calls describe Menu-side requests and local effects. Sending a
    // Game or Feature request does not acknowledge execution by that owner.
    // The adapter must serialize this controller on its existing owner thread.
    public interface IOriginalMenuSuspendHost
    {
        // Connected ordinary Game candidate; readiness is checked separately
        // so a request may become pending before native GetSelectedGameName
        // stops returning "Menu".
        bool HasOrdinarySelectedGameClient { get; }
        bool ForceMenuResumeTarget { get; }       // Menu +475c.
        bool DiagnosticsRequested { get; }       // *(Menu +378) +11.
        int MenuState { get; }
        bool WaitForConfigGameResponse { get; }
        bool TransitioningToSelectedGame { get; } // Menu +492e.
        bool OwnerReady { get; }                 // Menu +4741.
        bool SuppressSuspendAllFeatures { get; } // Menu +4674.

        void SetMenuState(int state);
        void SetDiagnosticsReturnFlag(bool value); // Menu +331.
        void SetTransitioningToSelectedGame(bool value);
        void SendGameSuspendRequest(bool suspended, bool transitionByte9, string operationId);
        void SendFeatureNotify(uint notification, string operationId);
        void SuspendAllFeatures(string operationId);
        // Read current Auto at actual feature dispatch. Never cache it on
        // Suspend, or describe this outgoing request as an external ACK.
        void ResumeAllFeatures(string operationId);
        void PauseAllSounds();  // SoundPauseAll(true, true).
        void ResumeAllSounds(); // SoundResumeAll(true).
        void PauseAllMovies();
        void ResumeAllMovies();
        void SetSharedGameSuspended(bool suspended);
        // The host owns MultiGameMenu's existing confirmation/input behavior.
        void SuspendMultiGameMenu();
        void ResumeMultiGameMenu();
    }

    // Recovered from Menu HandleSuspend 080a4f40, HandleResume 080b67a0,
    // CompleteResume 080b6350, and the pending-consumption tails at 080b5fb8
    // and 080b8add. This class owns only the process-local +492f pending bit.
    // It does not model Menu-self display/denomination selection, device
    // initialization, the complete GoToGame/config workflow, or persistence.
    public sealed class OriginalMenuSuspendController
    {
        private readonly IOriginalMenuSuspendHost host;
        private bool operating;

        public bool PendingResume { get; private set; }

        public bool SupportedContext
        {
            get { return host.HasOrdinarySelectedGameClient && !host.ForceMenuResumeTarget; }
        }

        public OriginalMenuSuspendController(IOriginalMenuSuspendHost host)
        {
            if (host == null) throw new ArgumentNullException("host");
            this.host = host;
            // Native constructor initializes +492f to zero. A new process
            // does not inherit a previous process's pending resume request.
            PendingResume = false;
        }

        public void HandleSuspendGameRequest(bool transitionByte9, string operationId)
        {
            Begin(operationId, false, true);
            try
            {
                PendingResume = false;
                // PC callers supply a complete bool. Native nontransition
                // byte9 has an uninitialized-sender boundary; no random stack
                // byte or inferred zero is reproduced here.
                host.SendGameSuspendRequest(true, transitionByte9, operationId);
                host.SendFeatureNotify(0x80, operationId);
                if (!host.SuppressSuspendAllFeatures)
                    host.SuspendAllFeatures(operationId);
                host.PauseAllSounds();
                host.PauseAllMovies();
                host.SetSharedGameSuspended(true);
                host.SuspendMultiGameMenu();
            }
            finally { operating = false; }
        }

        public OriginalMenuResumeDisposition HandleResumeGameRequest(string operationId)
        {
            Begin(operationId, false);
            try
            {
                // The diagnostics branch preserves the old pending bit.
                if (host.DiagnosticsRequested)
                {
                    host.SetMenuState(9);
                    host.SetDiagnosticsReturnFlag(true);
                    return OriginalMenuResumeDisposition.Diagnostics;
                }
                if ((host.MenuState == 10 && !host.WaitForConfigGameResponse) ||
                    host.TransitioningToSelectedGame || !host.OwnerReady)
                {
                    PendingResume = true;
                    return OriginalMenuResumeDisposition.Pending;
                }
                // Ordinary request clears pending before any resume effect.
                PendingResume = false;
                CompleteResumeCore(operationId);
                return OriginalMenuResumeDisposition.Dispatched;
            }
            finally { operating = false; }
        }

        // This is the original completion entry, not the gated request entry.
        // It does not itself clear pending. The native caller controls when
        // that assignment happens.
        public void CompleteHandleResumeGameRequest(string operationId)
        {
            Begin(operationId, true, true);
            try { CompleteResumeCore(operationId); }
            finally { operating = false; }
        }

        // Call only after the owner reaches the real GoToGame transition tail.
        // The larger selection, rendering and ShowGame sequence remains the
        // owner's responsibility; this hook does not pretend to perform it.
        public bool HandleGoToGameTransitionCompleted(string operationId)
        {
            Begin(operationId, false, PendingResume);
            try
            {
                host.SetTransitioningToSelectedGame(false);
                if (!PendingResume) return false;
                CompleteResumeCore(operationId);
                // Both native tails clear AFTER the whole resume dispatch.
                PendingResume = false;
                return true;
            }
            finally { operating = false; }
        }

        // Call only at the configuration-completed tail, after the owner has
        // performed its existing configuration work and before its following
        // EnableAttract/RestartMenuAttractTimer effects.
        public bool HandleGameConfigChangingCompletedTail(string operationId)
        {
            // A pending completion while transitioning would enter native
            // Menu-self display. Reject that unsupported context before the
            // state-zero effect instead of half-dispatching a Game resume.
            Begin(operationId, PendingResume, PendingResume);
            try
            {
                host.SetMenuState(0);
                if (!PendingResume) return false;
                CompleteResumeCore(operationId);
                PendingResume = false;
                return true;
            }
            finally { operating = false; }
        }

        private void CompleteResumeCore(string operationId)
        {
            host.SendGameSuspendRequest(false, false, operationId);
            host.ResumeAllFeatures(operationId);
            host.ResumeAllSounds();
            host.ResumeAllMovies();
            host.SetSharedGameSuspended(false);
            host.ResumeMultiGameMenu();
            // Resolve the selected feature client at dispatch time, as the
            // native completion does after MultiGameMenu resume.
            host.SendFeatureNotify(0x40, operationId);
        }

        private void Begin(string operationId, bool completingWithoutTransitionClear, bool requireReady = false)
        {
            if (operating) throw new InvalidOperationException("Menu suspend dispatch cannot re-enter itself.");
            if (string.IsNullOrEmpty(operationId)) throw new ArgumentException("A local operation ID is required.", "operationId");
            // All unsupported-path validation precedes pending assignments,
            // outgoing messages, media calls, and state changes.
            // GetSelectedGameName 080a4350 returns "Menu" while +4741 is
            // false. A gated request may wait for readiness; an actual Game
            // dispatch must reject that unsupported effective target first.
            if (!SupportedContext || (requireReady && !host.OwnerReady) ||
                (completingWithoutTransitionClear && host.TransitioningToSelectedGame))
                throw new NotSupportedException("This controller requires the ordinary selected Game path; Menu-self resume is not connected.");
            operating = true;
        }
    }
}
