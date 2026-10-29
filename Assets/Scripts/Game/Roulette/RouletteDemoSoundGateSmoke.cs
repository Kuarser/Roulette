using System;
using System.Collections;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private IEnumerator DemoSoundGateSmoke()
        {
            yield return null;
            if (demoShowMode != 1 || demoMenuRoot == null || autoplay || autoRoutine != null ||
                pcPlayerVolumeKey == DefaultPcPlayerVolumeKey || SourcePlayerInputs.Count != 0)
                throw new InvalidOperationException("Demo sound diagnostic needs idle demo mode and an isolated volume key");
            IRouletteSession saved = session;
            DemoPocketOutcomeSource savedDemo = demoOutcome;
            string originalFingerprint = PaytableTestLiveFingerprint(saved);
            bool savedMuted = pcGameSoundsMuted;
            OriginalAutoPlayData savedConfig = configuredAutoPlay.Clone();
            bool savedPending = configuredAutoPlayCheckPending;
            bool savedInputEnabled = sourceInputModule != null && sourceInputModule.enabled;
            int savedVolume = pcPlayerVolumeValue;
            long savedTimeout = pcPlayerVolumeIdleDeadlineTimestamp;
            bool savedPreferenceExists = PlayerPrefs.HasKey(pcPlayerVolumeKey);
            int savedPreference = PlayerPrefs.GetInt(pcPlayerVolumeKey,0);
            int checks = 0;
            try
            {
                configuredAutoPlay = new OriginalAutoPlayData();
                configuredAutoPlayCheckPending = false;
                if (sourceInputModule != null) sourceInputModule.enabled = false;
                demoOutcome = new DemoPocketOutcomeSource(new PaytableTestForbiddenOutcome());
                session = new RouletteSession(saved.Profile,2000,new PaytableTestForbiddenOutcome());
                string before = PaytableTestLiveFingerprint(session);
                pcPlayerVolumeValue = 0;
                PlayerPrefs.SetInt(pcPlayerVolumeKey,0);
                SetPcGameSoundsMuted(false);
                ToggleDemoMenu();
                yield return null;
                ClickThroughUiRaycast(demoOptionButtons[2]);
                if (demoMenuVisible || !configuredAutoPlay.TurboAutoPlay || autoplay || autoRoutine != null ||
                    !pcGameSoundsMuted || AudioListener.volume != 0f)
                    throw new InvalidOperationException("Raycast Turbo selection did not mute game sounds");
                ++checks;
                for (int i=0; i<3; ++i)
                {
                    int expected = i == 0 ? 5 : i == 1 ? 10 : 0;
                    AdvancePcPlayerVolume();
                    if (pcPlayerVolumeValue != expected || AudioListener.volume != 0f ||
                        PlayerPrefs.GetInt(pcPlayerVolumeKey,-1) != expected ||
                        footerVolumeButton != proViewVolumeFrames[expected/5])
                        throw new InvalidOperationException("Volume step overwrote the game sound gate");
                    ++checks;
                    LoadPcPlayerVolume();
                    if (pcPlayerVolumeValue != expected || AudioListener.volume != 0f)
                        throw new InvalidOperationException("Volume reload unmuted Turbo sounds");
                    ++checks;
                }
                pcPlayerVolumeValue = 10;
                ApplyPcPlayerVolume();
                pcPlayerVolumeIdleDeadlineTimestamp = 123456;
                TickPcPlayerVolumeIdleTimeout(123455);
                if (pcPlayerVolumeValue != 10 || AudioListener.volume != 0f)
                    throw new InvalidOperationException("Volume idle deadline expired early");
                ++checks;
                TickPcPlayerVolumeIdleTimeout(123456);
                if (pcPlayerVolumeValue != 0 || AudioListener.volume != 0f ||
                    pcPlayerVolumeIdleDeadlineTimestamp != 0)
                    throw new InvalidOperationException("Volume idle reset unmuted Turbo sounds");
                ++checks;

                foreach (int option in new[] {0,1})
                {
                    pcPlayerVolumeValue = option == 0 ? 0 : 5;
                    PlayerPrefs.SetInt(pcPlayerVolumeKey,pcPlayerVolumeValue);
                    SetPcGameSoundsMuted(true);
                    ToggleDemoMenu();
                    yield return null;
                    ClickThroughUiRaycast(demoOptionButtons[option]);
                    float expected = option == 0 ? 0f : 1f;
                    if (demoMenuVisible || configuredAutoPlay.Turbo != (option == 0) ||
                        configuredAutoPlay.AutoPlay != 1 || !configuredAutoPlay.ReplenishCredits ||
                        !configuredAutoPlay.PlayerInput || !configuredAutoPlay.DemoOnly ||
                        pcGameSoundsMuted != (option == 0) || autoplay || autoRoutine != null ||
                        Mathf.Abs(AudioListener.volume-expected) > .00001f ||
                        pcPlayerVolumeValue != (option == 0 ? 0 : 5))
                        throw new InvalidOperationException("Normal preservation or Auto sound restoration differs");
                    ++checks;
                }
                configuredAutoPlay.SetOptions(true,true,true,true,true,0);
                SetPcGameSoundsMuted(true);
                int released = 0;
                SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.TouchRelease,
                    () => { throw new InvalidOperationException("Purged release was dispatched"); },
                    () => ++released);
                SourcePlayerInputs.Enqueue(OriginalPlayerInputQueue.ButtonRelease,
                    null, () => ++released);
                if (!PurgeSourcePlayerInputs() || released != 2 || SourcePlayerInputs.Count != 0 ||
                    !configuredAutoPlay.TurboAutoPlay || !pcGameSoundsMuted || AudioListener.volume != 0f)
                    throw new InvalidOperationException("Release cleanup reset AutoData or sounds");
                ++checks;
                SourcePlayerInputs.Enqueue(0x200001,
                    () => { throw new InvalidOperationException("Purged press was dispatched"); });
                if (!PurgeSourcePlayerInputs() || SourcePlayerInputs.Count != 0 ||
                    configuredAutoPlay.AutoPlay != 0 || configuredAutoPlay.Turbo ||
                    configuredAutoPlay.ReplenishCredits || configuredAutoPlay.DemoOnly ||
                    !configuredAutoPlay.PlayerInput || pcGameSoundsMuted || autoplay ||
                    Mathf.Abs(AudioListener.volume-1f) > .00001f)
                    throw new InvalidOperationException("Queued player input did not reset AutoData and restore gain");
                ++checks;
                if (PaytableTestLiveFingerprint(session) != before || session.Rounds != 0)
                    throw new InvalidOperationException("Demo sound diagnostic changed funds/history/RNG");
            }
            finally
            {
                if (autoRoutine != null) StopCoroutine(autoRoutine);
                autoRoutine = null; autoplay = false; CloseDemoMenu();
                session = saved; configuredAutoPlay = savedConfig;
                demoOutcome = savedDemo;
                configuredAutoPlayCheckPending = savedPending;
                if (sourceInputModule != null) sourceInputModule.enabled = savedInputEnabled;
                pcPlayerVolumeValue = savedVolume;
                pcPlayerVolumeIdleDeadlineTimestamp = savedTimeout;
                if (savedPreferenceExists) PlayerPrefs.SetInt(pcPlayerVolumeKey,savedPreference);
                else PlayerPrefs.DeleteKey(pcPlayerVolumeKey);
                PlayerPrefs.Save();
                SetPcGameSoundsMuted(savedMuted); RefreshPortraitUi();
            }
            if (PaytableTestLiveFingerprint(saved) != originalFingerprint)
                throw new InvalidOperationException("Demo sound diagnostic altered formal session");
            Debug.Log("ROULETTE_DEMO_SOUND_GATE_PASSED: " + checks +
                " raycast/mute/gain/reload/idle/purge checks; Turbo volume survives 0/5/10; Normal preserves mute/config; Auto restores gain; release cleanup preserves config; queued press resets config/restores gain; funds/history/RNG isolation");
            Application.Quit();
        }
    }
}
