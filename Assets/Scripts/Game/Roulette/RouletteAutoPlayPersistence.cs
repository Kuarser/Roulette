using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private PcAutoPlayStateStore pcAutoPlayStateStore;
        private bool pcAutoPlayStorageFaulted;
        private string pcAutoPlayStorageFaultOperation;
        private GUIStyle pcAutoPlayFaultStyle;

        internal bool PcAutoPlayStorageFaulted
        {
            get { return pcAutoPlayStorageFaulted; }
        }

        // A game-level PC record, independent of each paytable and round.
        // The file envelope is deliberately separate from cabinet NV2.
        private string PcAutoPlayStatePath
        {
            get
            {
                string name = "roulette_autoplay_v1.bin";
                if (recordedHistoryKey != RecordedHistoryKey)
                {
                    using (SHA256 sha = SHA256.Create())
                        name = "roulette_autoplay_test_" + BitConverter.ToString(
                            sha.ComputeHash(Encoding.UTF8.GetBytes(recordedHistoryKey)))
                            .Replace("-", "").ToLowerInvariant() + ".bin";
                }
                return Path.Combine(Application.persistentDataPath, name);
            }
        }

        private bool InitializePcAutoPlayState()
        {
            if (pcAutoPlayStorageFaulted) return false;
            // Fresh-history tools and diagnostics without a named test
            // namespace must neither read nor overwrite the player's settings.
            bool unisolatedSmoke = recordedHistoryKey == RecordedHistoryKey &&
                Environment.GetCommandLineArgs().Any(arg =>
                    arg.StartsWith("-roulette", StringComparison.Ordinal) &&
                    arg.EndsWith("Smoke", StringComparison.Ordinal));
            if (temporaryHistory || unisolatedSmoke)
            {
                pcAutoPlayStateStore = null;
                configuredAutoPlay = new OriginalAutoPlayData();
                SetPcGameSoundsMuted(false);
                Debug.Log("ROULETTE_AUTO_NV_DETACHED: temporary or unisolated diagnostic");
                return true;
            }
            try
            {
                pcAutoPlayStateStore = new PcAutoPlayStateStore(PcAutoPlayStatePath);
                OriginalAutoPlayData loaded;
                bool restored = pcAutoPlayStateStore.TryLoad(out loaded);
                configuredAutoPlay = restored ? loaded : new OriginalAutoPlayData();
                // Native Open creates the initial NV block. Materialize the
                // zero configuration here as well, before any state dispatch.
                if (!restored) pcAutoPlayStateStore.Save(configuredAutoPlay);
                configuredAutoPlay.AttachPersistence(SavePcAutoPlayState);
                // Game::Init applies this independently of Demo availability.
                // Active Game+5110 remains cleared by LoadProfile.
                SetPcGameSoundsMuted(configuredAutoPlay.TurboAutoPlay);
                Debug.Log("ROULETTE_AUTO_NV_OPEN: restored=" + restored +
                    " auto=" + configuredAutoPlay.AutoPlay +
                    " turbo=" + configuredAutoPlay.Turbo +
                    " muted=" + pcGameSoundsMuted);
                return true;
            }
            catch (Exception error)
            {
                FailPcAutoPlayStorage("loaded", error);
                return false;
            }
        }

        private void SavePcAutoPlayState(OriginalAutoPlayData data)
        {
            if (pcAutoPlayStorageFaulted)
                throw new InvalidOperationException("Automatic play storage is faulted");
            // A diagnostic may enter temporary-history mode after startup.
            if (temporaryHistory || pcAutoPlayStateStore == null) return;
            try
            {
                pcAutoPlayStateStore.Save(data);
                AutoCompleteCheckpoint("AutoSaved");
            }
            catch (Exception error)
            {
                FailPcAutoPlayStorage("saved", error);
                throw;
            }
        }

        private void ArmPcAutoPlayInitialIdleEntry()
        {
            // A failed financial/pending restore must keep every startup
            // dispatcher disarmed, including the trusted lifecycle branch.
            if (pcAutoPlayStorageFaulted) return;
            if (HasTrustedLatestLifecycle)
            {
                // The trusted stage already owns unfinished work. A completed
                // source Idle may enter again; staged Bet must not do so.
                if (latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Complete &&
                    (restoredExplicitSourceIdleState || stagedBets == null) &&
                    sourceGameplayState == SourceGameplayState.Idle && !resumePendingSpin)
                    configuredAutoPlayCheckPending = true;
                return;
            }
            if (session == null ||
                sourceGameplayState != SourceGameplayState.Idle ||
                SourceRoundBlocksOrdinaryInput || resumePendingSpin ||
                pendingSpin != null || session.SpinPending || session.InDoubleUp)
                return;
            // Game::InitData converts a new State NV value 0 to state 1.
            // A validated staged record explicitly storing 1 also proves Idle.
            // A paid archive alone cannot distinguish pre-End from post-End.
            bool fresh = recordedRoundPages.Count == 0 && recordedPockets.Count == 0 &&
                stagedBets == null;
            bool stagedIdle = restoredExplicitSourceIdleState;
            if (!fresh && !stagedIdle) return;
            configuredAutoPlayCheckPending = true;
            Debug.Log("ROULETTE_AUTO_NV_INITIAL_IDLE: " + (fresh ? "fresh" : "staged"));
            // Dispatch waits until Start has supplied Demo availability and
            // the real input module services this single state-entry request.
        }

        private void FailPcAutoPlayStorage(string operation, Exception error)
        {
            if (pcAutoPlayStorageFaulted) return;
            pcAutoPlayStorageFaulted = true;
            pcAutoPlayStorageFaultOperation = operation;
            // PC fail-stop policy: preserve the last durable settings and all
            // financial journals. A partially executed native-style setter
            // must never turn into ordinary feedback followed by more play.
            StopAllCoroutines();
            CancelInvoke();
            autoplay = false;
            autoConfirmPending = false;
            configuredAutoPlayCheckPending = false;
            configuredAutoPlayFinishPending = false;
            sourceTableInputEnabled = false;
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            if (portraitCanvas != null) portraitCanvas.gameObject.SetActive(false);
            if (ideckCanvas != null) ideckCanvas.gameObject.SetActive(false);
            if (topperCanvas != null) topperCanvas.gameObject.SetActive(false);
            SetPcGameSoundsMuted(true);
            Debug.LogError((pcRoundLifecycleStorageFaulted ? "ROULETTE_LIFECYCLE_FAULT: " :
                "ROULETTE_AUTO_NV_FAULT: ") + operation + " " + error);
        }

        private void OnGUI()
        {
            if (!pcAutoPlayStorageFaulted) return;
            GUI.depth = -1000;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
            if (pcAutoPlayFaultStyle == null)
            {
                pcAutoPlayFaultStyle = new GUIStyle(GUI.skin.label);
                pcAutoPlayFaultStyle.alignment = TextAnchor.MiddleCenter;
                pcAutoPlayFaultStyle.wordWrap = true;
            }
            pcAutoPlayFaultStyle.fontSize = Mathf.Clamp(Screen.width / 28, 18, 38);
            GUI.Label(new Rect(Screen.width * .08f, Screen.height * .2f,
                Screen.width * .84f, Screen.height * .6f),
                "Game stopped\n\nThe game could not safely read or save its records." +
                "\n\nClose the game and check its saved game and credit records before restarting.",
                pcAutoPlayFaultStyle);
        }
    }
}
