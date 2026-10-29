using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RouletteDeluxe
{
    // The storage fault stops RouletteGame coroutines. Keep only this explicit
    // diagnostic observer alive so it can verify the resulting frozen state.
    public sealed class RouletteAutoPlayNvSmokeHost : MonoBehaviour { }

    public sealed partial class RouletteGame
    {
        private sealed class AutoNvOutcome : IRouletteOutcomeSource, IRouletteOutcomeMapping, IDisposable
        {
            private readonly IRouletteOutcomeSource inner;
            internal int Pockets, Suits, Pocket = 27;
            internal AutoNvOutcome(IRouletteOutcomeSource source) { inner = source; }
            public int ResolvePocket(int raw) { return Pocket; }
            public int ResolveDoubleSuit(int raw)
            {
                var mapping = inner as IRouletteOutcomeMapping;
                return mapping == null ? raw : mapping.ResolveDoubleSuit(raw);
            }
            public int DrawPocket()
            {
                int actual = inner.DrawPocket();
                ++Pockets;
                Debug.Log("ROULETTE_AUTO_NV_DRAW: count=" + Pockets + " actual=" + actual + " fixture=" + Pocket);
                return Pocket;
            }
            public int DrawDoubleSuit() { ++Suits; return inner.DrawDoubleSuit(); }
            public void Dispose() { var disposable = inner as IDisposable; if (disposable != null) disposable.Dispose(); }
        }

        [Serializable]
        private sealed class AutoNvReceipt
        {
            public string testCase, phase, config, bytes, economy, archive, rng, undo,
                pending, staged, settledUndo, creditEvent, cleared, raw;
            public int volume;
            public long credit, rounds, wagered, returned;
        }

        private AutoNvOutcome autoNvOutcome;
        private string AutoNvReceiptKey { get { return recordedHistoryKey + ".AutoNvFixture.v1"; } }

        private IRouletteOutcomeSource CreateAutoPlayNvOutcome(IRouletteOutcomeSource platform)
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteAutoPlayNvSmoke") < 0) return platform;
            autoNvOutcome = new AutoNvOutcome(platform);
            return autoNvOutcome;
        }

        private void StartAutoPlayNvSmoke()
        {
            // Observe boot before the first possible input-module service.
            // Do not consume or clear the pending initial Idle check.
            if (sourceInputModule != null) sourceInputModule.enabled = false;
            var host = new GameObject("Auto NV acceptance observer").AddComponent<RouletteAutoPlayNvSmokeHost>();
            host.StartCoroutine(AutoPlayNvSmoke());
        }

        private static void RequireAutoNv(bool valid, string message)
        { if (!valid) throw new InvalidOperationException("Auto NV: " + message); }

        private static string AutoNvArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, name);
            RequireAutoNv(at >= 0 && at + 1 < args.Length, "missing " + name);
            return args[at + 1];
        }

        private IEnumerator AutoPlayNvSmoke()
        {
            string testCase = AutoNvArgument("-rouletteAutoPlayNvCase");
            string phase = AutoNvArgument("-rouletteAutoPlayNvPhase");
            RequireAutoNv(new[] { "menu_auto", "menu_turbo", "pending", "purge", "legacy", "fresh", "corrupt", "write_failure" }
                .Contains(testCase), "unknown case");
            RequireAutoNv(recordedHistoryKey.Contains(".Test.") && pcPlayerVolumeKey.Contains(".Test.") &&
                Path.GetFileName(PcRngStateStore.PathForCurrentProcess).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal) &&
                Path.GetFileName(PcAutoPlayStatePath).StartsWith("roulette_autoplay_test_", StringComparison.Ordinal) &&
                sourceRoundEndCount == 0, "requires isolated History/RNG/Volume/Auto records and a new process");
            RequireAutoNv(temporaryHistory == (testCase == "fresh"), "only fresh case permits temporary history");
            try
            {
                if (testCase == "corrupt" && phase == "load")
                {
                    AutoNvReceipt proof = ReadAutoNvReceipt("corrupt", "corrupt");
                    RequireAutoNv(pcAutoPlayStorageFaulted && session == null && autoNvOutcome == null &&
                        pcAutoPlayStorageFaultOperation == "loaded" && AutoNvBytes() == proof.bytes &&
                        AutoNvRawRecords() == proof.raw && DemoCleanupRngBytes() == proof.rng,
                        "corrupt startup altered settings, journal, RNG or constructed a playable session");
                    for (int i = 0; i < 4; ++i) yield return null;
                    RequireAutoNv(AutoNvBytes() == proof.bytes && AutoNvRawRecords() == proof.raw &&
                        DemoCleanupRngBytes() == proof.rng && !autoplay && sourceRoundEndCount == 0,
                        "faulted startup continued dispatch");
                    yield return CaptureAutoNvFault();
                }
                else
                {
                    RequireAutoNv(!pcAutoPlayStorageFaulted && session != null && autoNvOutcome != null &&
                        sourceInputModule != null && SourcePlayerInputs.Count == 0 && !autoplay,
                        "playable startup or active-state reset differs");
                    if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteSingleDisplay") >= 0)
                        RequireAutoNv(!ideckSecondDisplay && !topperDedicatedDisplay,
                            "single-display diagnostic activated an additional output");
                    if (testCase == "fresh")
                    {
                        RequireAutoNv(phase == "run", "bad fresh phase");
                        yield return AutoNvFresh();
                    }
                    else if (testCase == "menu_auto" || testCase == "menu_turbo")
                        yield return AutoNvMenu(testCase, phase);
                    else if (testCase == "pending") yield return AutoNvPending(phase);
                    else if (testCase == "purge") yield return AutoNvPurge(phase);
                    else if (testCase == "legacy") yield return AutoNvLegacy(phase);
                    else if (testCase == "corrupt")
                    {
                        RequireAutoNv(phase == "save", "bad corrupt phase");
                        yield return AutoNvPreparePending();
                        byte[] bytes = File.ReadAllBytes(pcAutoPlayStateStore.StatePath);
                        RequireAutoNv(bytes.Length == 172, "unexpected Auto record size");
                        bytes[bytes.Length - 1] ^= 0x80;
                        File.WriteAllBytes(pcAutoPlayStateStore.StatePath, bytes);
                        SaveAutoNvReceipt("corrupt", "corrupt");
                    }
                    else
                    {
                        RequireAutoNv(phase == "run", "bad write-failure phase");
                        yield return AutoNvWriteFailure();
                    }
                }
            }
            finally
            {
                SourcePlayerInputs.Purge(0, 0);
                if (sourceInputModule != null) sourceInputModule.enabled = false;
            }
            Debug.Log("ROULETTE_AUTO_NV_PASSED: case=" + testCase + " phase=" + phase +
                " credit=" + (session == null ? -1 : session.Credit) + " ends=" + sourceRoundEndCount +
                " pockets=" + (autoNvOutcome == null ? 0 : autoNvOutcome.Pockets) +
                " fault=" + pcAutoPlayStorageFaulted);
            Application.Quit();
        }

        private IEnumerator AutoNvInitialIdle()
        {
            RequireAutoNv(session.Credit == 1000 && session.TotalBet == 0 && session.Rounds == 0 &&
                recordedRoundPages.Count == 0 && configuredAutoPlay.AutoPlay == 0 &&
                !PlayerPrefs.HasKey(AutoNvReceiptKey), "save requires unused records");
            yield return StartGameLiveFrames(3);
            RequireAutoNv(!configuredAutoPlayCheckPending && sourceRoundPhase == SourceRoundPhase.None &&
                sourceGameplayState == SourceGameplayState.Idle && autoNvOutcome.Pockets == 0,
                "initial default Idle did not finish without a draw");
            pcPlayerVolumeValue = 5;
            PlayerPrefs.SetInt(pcPlayerVolumeKey, 5);
            ApplyPcPlayerVolume();
            PlayerPrefs.Save();
        }

        private void AutoNvSentinels()
        {
            configuredAutoPlay.SetTurboOption(9);
            configuredAutoPlay.SetAutoPlayOffWhenGameFinished(true);
            configuredAutoPlay.SetSecsToAutoCompleteWarning(11);
            configuredAutoPlay.SetSecsToAutoCompleteStart(7);
            configuredAutoPlay.SetAutoCompleteShowCountdown(true);
            configuredAutoPlay.SetDisableWhenGameFinished(true);
            Debug.Log("ROULETTE_AUTO_NV_CALL_LEVEL_FIELDS: option=9 off=true warning=11 start=7 countdown=true disable=true");
        }

        private IEnumerator AutoNvMenu(string testCase, string phase)
        {
            bool turbo = testCase == "menu_turbo";
            if (phase == "save")
            {
                RequireAutoNv(demoShowMode == 1, "menu save needs Demo mode");
                yield return AutoNvInitialIdle();
                yield return StartGameChooseDemo(turbo ? 2 : 1);
                AutoNvSentinels();
                RequireAutoNv(configuredAutoPlay.AutoPlay == 1 && configuredAutoPlay.Turbo == turbo &&
                    pcGameSoundsMuted == turbo && !autoplay && session.Credit == 1000 && session.TotalBet == 0,
                    "real menu did not preserve configured versus active distinction");
                string before = AutoNvBytes(), config = AutoNvConfig(configuredAutoPlay);
                OriginalAutoPlayData clone = configuredAutoPlay.Clone();
                clone.SetAutoPlay(3); clone.SetTurbo(!turbo); clone.SetSecsToAutoCompleteStart(uint.MaxValue);
                RequireAutoNv(AutoNvBytes() == before && AutoNvConfig(configuredAutoPlay) == config,
                    "detached diagnostic clone changed live settings or record");
                OriginalAutoPlayData old = configuredAutoPlay;
                RequireAutoNv(InitializePcAutoPlayState() && !ReferenceEquals(old, configuredAutoPlay) &&
                    AutoNvConfig(configuredAutoPlay) == config && AutoNvBytes() == before && pcGameSoundsMuted == turbo,
                    "explicit same-process storage reopen did not read a new bound instance");
                RequireAutoNvStored();
                SaveAutoNvReceipt(testCase, "menu");
            }
            else
            {
                RequireAutoNv(phase == "load" || (!turbo && (phase == "idle" || phase == "idle_reload")), "bad menu phase");
                AutoNvReceipt proof = ReadAutoNvReceipt(testCase, phase == "idle_reload" ? "idle" : "menu");
                RequireAutoNvReceipt(proof, true);
                RequireAutoNv(configuredAutoPlay.AutoPlay == 1 && configuredAutoPlay.Turbo == turbo &&
                    pcGameSoundsMuted == turbo && !autoplay && pcPlayerVolumeValue == 5,
                    "cold configuration, mute or active reset differs");
                if (phase == "idle")
                {
                    RequireAutoNv(demoShowMode == 1 && configuredAutoPlayCheckPending && session.TotalBet == 0,
                        "proven Idle did not arm its real startup check");
                    yield return StateEndUntil(() => !configuredAutoPlayCheckPending, "Auto NV initial configured Idle");
                    PcCreditEvent deposit = ReadPcCreditEvent();
                    RequireAutoNv(session.Credit == 3000 && session.TotalBet == 0 && session.Rounds == 0 &&
                        autoNvOutcome.Pockets == 0 && autoNvOutcome.Suits == 0 && sourceRoundEndCount == 0 &&
                        !autoplay && deposit != null && deposit.kind == "deposit" && deposit.before == 1000 &&
                        deposit.after == 3000 && deposit.phase == 3 && AutoNvBytes() == proof.bytes &&
                        AutoNvConfig(configuredAutoPlay) == proof.config,
                        "startup Check did not fund exactly once without a round");
                    yield return AutoNvStable(0);
                    SaveAutoNvReceipt(testCase, "idle");
                }
                else
                {
                    RequireAutoNv(demoShowMode == 0, "ordinary reload must test Demo-independent reading");
                    yield return AutoNvStable(0);
                    RequireAutoNvReceipt(proof, true);
                    if (turbo) CaptureAutoNv("restart");
                }
            }
        }

        private IEnumerator AutoNvPreparePending()
        {
            RequireAutoNv(demoShowMode == 1, "pending save needs Demo mode");
            yield return AutoNvInitialIdle();
            yield return StateEndPlaceBet(20);
            yield return StartGameChooseDemo(2);
            AutoNvSentinels();
            // This persistence scope does not implement the native completion
            // timer or platform DisableData. Keep those live branches off;
            // the menu and purge cases independently round-trip their fields.
            configuredAutoPlay.SetSecsToAutoCompleteStart(0);
            configuredAutoPlay.SetDisableWhenGameFinished(false);
            RequireAutoNvStored();
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            QueueStartGameButton(ideckAutoPlayButton, 241, true);
            yield return StateEndUntil(() => autoConfirmPending, "actual Auto confirmation");
            RefreshDiagnosticUi();
            QueueStartGameButton(ideckAutoYesButton, 242, true);
            yield return StateEndUntil(() => spinning && session.SpinPending, "actual active Auto pending");
            RequireAutoNv(autoplay && pendingSpin != null && session.PendingPocket == 27 && session.Credit == 980 &&
                session.Rounds == 0 && autoNvOutcome.Pockets == 1 && autoNvOutcome.Suits == 0 &&
                sourceRoundPhase == SourceRoundPhase.Wheel && sourceRoundEndCount == 0 &&
                configuredAutoPlay.TurboAutoPlay && pcGameSoundsMuted &&
                configuredAutoPlay.AutoPlayOffWhenGameFinished && configuredAutoPlay.SecsToAutoCompleteStart == 0 &&
                SourcePlayerInputs.Count == 0 && ReadPcCreditEvent() == null,
                "actual active Auto failed to commit one configured pending win");
        }

        private IEnumerator AutoNvPending(string phase)
        {
            if (phase == "save")
            {
                yield return AutoNvPreparePending();
                SaveAutoNvReceipt("pending", "pending");
            }
            else
            {
                RequireAutoNv(phase == "complete" || phase == "reload", "bad pending phase");
                AutoNvReceipt proof = ReadAutoNvReceipt("pending", phase == "complete" ? "pending" : "completed");
                RequireAutoNvReceipt(proof, true);
                if (phase == "complete")
                {
                    RequireAutoNv(spinning && session.SpinPending && session.PendingPocket == 27 && !autoplay &&
                        sourceRoundPhase == SourceRoundPhase.Wheel && configuredAutoPlay.TurboAutoPlay &&
                        configuredAutoPlay.AutoPlayOffWhenGameFinished && configuredAutoPlay.SecsToAutoCompleteStart == 0,
                        "pending restart lost its ticket/configuration or restored active Auto");
                    // Off+zero Start skips Double; the actual input module
                    // owns the high-win wait and ensuing End.
                    yield return StateEndWaitForIdle(false);
                    RequireStateEndTotals(1700, 20, 720, 1, 1, 1);
                    RequireAutoNv(configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo &&
                        !configuredAutoPlay.AutoPlayOffWhenGameFinished && configuredAutoPlay.ReplenishCredits &&
                        configuredAutoPlay.DemoOnly && configuredAutoPlay.PlayerInput && configuredAutoPlay.TurboOption == 9 &&
                        configuredAutoPlay.SecsToAutoCompleteStart == 0 && configuredAutoPlay.SecsToAutoCompleteWarning == 11 &&
                        configuredAutoPlay.AutoCompleteShowCountdown && !configuredAutoPlay.DisableWhenGameFinished &&
                        !pcGameSoundsMuted && !autoplay && autoNvOutcome.Pockets == 0 && autoNvOutcome.Suits == 0 &&
                        DemoCleanupRngBytes() == proof.rng && StateBetCleanupUndoFingerprint() == proof.undo &&
                        ReadPcCreditEvent() == null,
                        "real End did not persist only its three fields or replayed RNG/funds");
                    RequireAutoNvStored();
                    yield return AutoNvStable(1);
                    SaveAutoNvReceipt("pending", "completed");
                    CaptureAutoNv("completed");
                }
                else
                {
                    RequireAutoNv(!spinning && pendingSpin == null && sourceRoundPhase == SourceRoundPhase.None,
                        "completed reload armed the old round");
                    yield return AutoNvStable(0);
                    RequireAutoNvReceipt(proof, true);
                }
            }
        }

        private IEnumerator AutoNvPurge(string phase)
        {
            if (phase == "load")
            {
                AutoNvReceipt proof = ReadAutoNvReceipt("purge", "reset");
                RequireAutoNvReceipt(proof, true);
                RequireAutoNv(sourceGameplayState == SourceGameplayState.Bet && !configuredAutoPlayCheckPending &&
                    session.TotalBet == 1 && configuredAutoPlay.AutoPlay == 0 && !configuredAutoPlay.Turbo &&
                    configuredAutoPlay.AutoPlayOffWhenGameFinished && configuredAutoPlay.SecsToAutoCompleteStart == 7 &&
                    !pcGameSoundsMuted && !autoplay, "reset load restarted Auto or consumed an unended stop request");
                yield return AutoNvStable(0);
                RequireAutoNvReceipt(proof, true);
                yield break;
            }
            RequireAutoNv(phase == "save" && demoShowMode == 1, "bad purge phase");
            yield return AutoNvInitialIdle();
            yield return StartGameChooseDemo(2);
            AutoNvSentinels();
            string before = AutoNvBytes(), config = AutoNvConfig(configuredAutoPlay), rng = DemoCleanupRngBytes();
            Vector2 point = StartGameButtonPoint(uiBetButtons[1]);
            var probe = uiBetButtons[1].gameObject.AddComponent<RouletteStartGameReleaseProbe>();
            try
            {
                sourceInputModule.QueuePointerSample(243, point, true, false, true, true);
                RequireAutoNv(SourcePlayerInputs.DispatchOne(), "release-only held Down missing");
                PointerEventData released = sourceInputModule.QueuePointerSample(243, point, false, true, false, true);
                // Directly exercise the production queue/persistence boundary;
                // the following non-release case uses real StateBet entry.
                Debug.Log("ROULETTE_AUTO_NV_CALL_LEVEL_RELEASE_PURGE: queuedReleaseOnly=true");
                RequireAutoNv(PurgeSourcePlayerInputs() && probe.UpCount == 1 && probe.LastUpPosition == Vector2.zero &&
                    AutoNvBytes() == before && AutoNvConfig(configuredAutoPlay) == config && pcGameSoundsMuted &&
                    session.TotalBet == 0 && SourcePlayerInputs.Count == 0,
                    "release-only purge changed the stored configuration or wager");
                RequireSourcePointerReleased(released);
                Vector2 first = StartGameButtonPoint(uiBetButtons[27]);
                sourceInputModule.QueuePointerSample(244, first, true, false, true, true);
                RequireAutoNv(SourcePlayerInputs.DispatchOne(), "first held table Down missing");
                sourceInputModule.QueuePointerSample(245, point, true, false, true, true);
                RequireAutoNv(SourcePlayerInputs.DispatchOne(), "second held table Down missing");
                sourceInputModule.QueuePointerSample(244, first, false, true, false, true);
                sourceInputModule.QueuePointerSample(245, point + Vector2.right, false, false, true, true);
                PointerEventData tail = sourceInputModule.QueuePointerSample(245, point, false, true, false, true);
                RequireAutoNv(SourcePlayerInputs.DispatchOne() && sourceBetStateEntryPending && session.TotalBet == 1 &&
                    SourcePlayerInputs.Count == 2 && AutoNvBytes() == before, "first table Up did not defer entry with unchanged NV");
                yield return StartGameWaitForBetEntry();
                RequireSourcePointerReleased(tail);
                OriginalAutoPlayData expected;
                RequireAutoNv(new PcAutoPlayStateStore(pcAutoPlayStateStore.StatePath).TryLoad(out expected), "purge record missing");
                RequireAutoNv(expected != null && expected.AutoPlay == 0 && !expected.ReplenishCredits && !expected.DemoOnly &&
                    expected.PlayerInput && !expected.Turbo && expected.TurboOption == 0 && expected.AutoPlayOffWhenGameFinished &&
                    expected.SecsToAutoCompleteStart == 7 && expected.SecsToAutoCompleteWarning == 11 &&
                    expected.AutoCompleteShowCountdown && expected.DisableWhenGameFinished &&
                    AutoNvConfig(expected) == AutoNvConfig(configuredAutoPlay) && !pcGameSoundsMuted && !autoplay &&
                    session.Credit == 1000 && session.TotalBet == 1 && session.Bets[127] == 1 &&
                    sourceRoundEndCount == 0 && autoNvOutcome.Pockets == 0 && autoNvOutcome.Suits == 0 &&
                    DemoCleanupRngBytes() == rng && ReadPcCreditEvent() == null && probe.UpCount == 2,
                    "real StateBet purge failed its six-field durable reset and independent-field preservation");
                SaveAutoNvReceipt("purge", "reset");
            }
            finally { Destroy(probe); }
        }

        private IEnumerator AutoNvLegacy(string phase)
        {
            if (phase == "save")
            {
                yield return AutoNvInitialIdle();
                autoNvOutcome.Pocket = 16;
                yield return StateEndPlaceBet(1);
                RefreshPortraitUi();
                QueueStartGameButton(uiSpinButton, 246, true);
                yield return StateEndUntil(() => spinning && session.SpinPending, "legacy actual PLAY");
                yield return StateEndWaitForIdle(false);
                RequireStateEndTotals(999, 1, 0, 1, 0, 1);
                yield return AutoNvStable(1);
                // Only this unique test record is removed to represent an old
                // PC archive created before Auto NV existed.
                File.Delete(pcAutoPlayStateStore.StatePath);
                SaveAutoNvReceipt("legacy", "missing");
            }
            else
            {
                RequireAutoNv(phase == "load", "bad legacy phase");
                AutoNvReceipt proof = ReadAutoNvReceipt("legacy", "missing");
                RequireAutoNvReceipt(proof, false);
                RequireAutoNv(AutoNvConfig(configuredAutoPlay) == AutoNvConfig(new OriginalAutoPlayData()) &&
                    session.Credit == 999 && session.Rounds == 1 && !pcGameSoundsMuted && !autoplay &&
                    sourceRoundPhase == SourceRoundPhase.None,
                    "legacy completed archive without Auto record did not retain money/default config");
                RequireAutoNvStored();
                yield return AutoNvStable(0);
                RequireAutoNvReceipt(proof, false);
                yield return AutoNvLegacyStateProvenance();
            }
        }

        private IEnumerator AutoNvLegacyStateProvenance()
        {
            // A legacy state value 0 must not become evidence of native Idle
            // merely because a valid money-event repair writes the PC fallback.
            RefreshPortraitUi();
            DemoCleanupClick(uiClearButton);
            RequireAutoNv(session.TotalBet == 0 && !session.ResultPhase && stagedBets != null,
                "legacy provenance fixture needs a real cleared staged table");
            long priorCredit = session.Credit;
            AddPcVirtualCredits(100, true);
            SaveStagedBetsIfChanged();
            RequireAutoNv(session.Credit == priorCredit + 100, "legacy provenance deposit failed");
            StagedBetRecord fixture = JsonUtility.FromJson<StagedBetRecord>(PlayerPrefs.GetString(StagedBetKey, ""));
            fixture.credit = priorCredit;
            fixture.sourceGameplayState = 0;
            PlayerPrefs.SetString(StagedBetKey, JsonUtility.ToJson(fixture));
            PlayerPrefs.Save();
            string rng = DemoCleanupRngBytes(), bytes = AutoNvBytes(), archive = DemoCleanupArchive(false);
            string creditEvent = StartGameCreditEventFingerprint();
            Debug.Log("ROULETTE_AUTO_NV_CALL_LEVEL_LEGACY_STATE: originalState=0 staleCredit=" + priorCredit);
            LoadProfile();
            RequireAutoNv(session != null && session.Credit == priorCredit + 100 && stagedBets != null &&
                stagedBets.sourceGameplayState == 1 && !restoredExplicitSourceIdleState &&
                !configuredAutoPlayCheckPending, "money repair promoted legacy state into explicit Idle provenance");
            ArmPcAutoPlayInitialIdleEntry();
            RequireAutoNv(!configuredAutoPlayCheckPending, "legacy state repair armed an unsupported initial Check");
            yield return AutoNvStable(0);
            RequireAutoNv(AutoNvBytes() == bytes && DemoCleanupRngBytes() == rng && DemoCleanupArchive(false) == archive &&
                StartGameCreditEventFingerprint() == creditEvent && session.Credit == priorCredit + 100,
                "legacy state provenance check changed settings, funds, archive or RNG");
        }

        private IEnumerator AutoNvFresh()
        {
            AutoNvReceipt proof = ReadAutoNvReceipt("menu_turbo", "menu");
            RequireAutoNv(pcAutoPlayStateStore == null && AutoNvBytes() == proof.bytes &&
                AutoNvConfig(configuredAutoPlay) == AutoNvConfig(new OriginalAutoPlayData()) && !pcGameSoundsMuted &&
                session.Credit == 1000 && session.Rounds == 0 && !autoplay,
                "fresh history imported the saved configuration");
            string raw = AutoNvRawRecords(), rng = DemoCleanupRngBytes();
            yield return StartGameLiveFrames(3);
            yield return StartGameChooseDemo(1);
            AutoNvSentinels();
            RequireAutoNv(configuredAutoPlay.AutoPlay == 1 && !configuredAutoPlay.Turbo &&
                AutoNvBytes() == proof.bytes && AutoNvRawRecords() == raw && DemoCleanupRngBytes() == rng &&
                autoNvOutcome.Pockets == 0 && autoNvOutcome.Suits == 0 && sourceRoundEndCount == 0 && session.Rounds == 0,
                "fresh detached setters overwrote the established Auto/economy key or drew RNG");
        }

        private IEnumerator AutoNvWriteFailure()
        {
            yield return AutoNvPreparePending();
            string durable = StartGameDurableFingerprint(), bytes = AutoNvBytes();
            string next = pcAutoPlayStateStore.StatePath + ".new";
            RequireAutoNv(!File.Exists(next), "write-failure fixture refuses an existing .new record");
            bool caught = false;
            using (var blocked = new FileStream(next, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None))
            {
                try { configuredAutoPlay.SetTurboOption(10); }
                catch (IOException) { caught = true; }
                RequireAutoNv(caught && pcAutoPlayStorageFaulted && pcAutoPlayStorageFaultOperation == "saved" &&
                    !sourceInputModule.enabled && !autoplay && pcGameSoundsMuted &&
                    !portraitCanvas.gameObject.activeSelf && StartGameDurableFingerprint() == durable && AutoNvBytes() == bytes,
                    "write error did not fail-stop the live pending game and preserve its main record");
                DispatchSourceButton(KeyCode.Space);
                DispatchSourceButton(KeyCode.F8);
                for (int i = 0; i < 5; ++i) yield return null;
                RequireAutoNv(StartGameDurableFingerprint() == durable && AutoNvBytes() == bytes &&
                    session.Credit == 980 && session.Rounds == 0 && session.SpinPending && autoNvOutcome.Pockets == 1 &&
                    autoNvOutcome.Suits == 0 && sourceRoundEndCount == 0 && !demoMenuVisible,
                    "write fault continued a wheel/round/input after stopping");
                yield return CaptureAutoNvFault();
            }
            File.Delete(next);
        }

        private IEnumerator AutoNvStable(int ends)
        {
            string before = StartGameDurableFingerprint(), bytes = AutoNvBytes(), config = AutoNvConfig(configuredAutoPlay);
            int pockets = autoNvOutcome.Pockets, suits = autoNvOutcome.Suits;
            yield return StartGameLiveFrames(4);
            RequireAutoNv(StartGameDurableFingerprint() == before && AutoNvBytes() == bytes &&
                AutoNvConfig(configuredAutoPlay) == config && sourceRoundEndCount == ends && !autoplay &&
                autoNvOutcome.Pockets == pockets && autoNvOutcome.Suits == suits && !pcAutoPlayStorageFaulted,
                "later live frames repeated funds/RNG/End or changed config");
        }

        private static string AutoNvConfig(OriginalAutoPlayData data)
        {
            return string.Join("|", new[] { data.AutoPlay.ToString(), data.ReplenishCredits.ToString(), data.DemoOnly.ToString(),
                data.PlayerInput.ToString(), data.Turbo.ToString(), data.TurboOption.ToString(),
                data.AutoPlayOffWhenGameFinished.ToString(), data.SecsToAutoCompleteWarning.ToString(),
                data.SecsToAutoCompleteStart.ToString(), data.AutoCompleteShowCountdown.ToString(), data.DisableWhenGameFinished.ToString() });
        }

        private string AutoNvBytes()
        { return File.Exists(PcAutoPlayStatePath) ? Convert.ToBase64String(File.ReadAllBytes(PcAutoPlayStatePath)) : "missing"; }

        private string AutoNvRawRecords()
        {
            string[] keys = { recordedHistoryKey, recordedHistoryKey + ".Pages", recordedHistoryKey + ".RoundDetails.v1",
                PendingSpinKey, StagedBetKey, SettledTableUndoKey, PcCreditKey, GameResultsClearedKey };
            return string.Join("\n", keys.Select(key => key + ":" + PlayerPrefs.HasKey(key) + ":" +
                PlayerPrefs.GetString(key, "")).ToArray());
        }

        private void RequireAutoNvStored()
        {
            OriginalAutoPlayData loaded;
            RequireAutoNv(pcAutoPlayStateStore != null && new PcAutoPlayStateStore(pcAutoPlayStateStore.StatePath).TryLoad(out loaded) &&
                AutoNvConfig(loaded) == AutoNvConfig(configuredAutoPlay), "independent reader differs from the live configuration");
        }

        private void SaveAutoNvReceipt(string testCase, string phase)
        {
            var proof = new AutoNvReceipt { testCase = testCase, phase = phase, config = AutoNvConfig(configuredAutoPlay),
                bytes = AutoNvBytes(), economy = StartGameRestoredEconomy(), archive = DemoCleanupArchive(false),
                rng = DemoCleanupRngBytes(), undo = StateBetCleanupUndoFingerprint(), pending = PlayerPrefs.GetString(PendingSpinKey, ""),
                staged = PlayerPrefs.GetString(StagedBetKey, ""), settledUndo = PlayerPrefs.GetString(SettledTableUndoKey, ""),
                creditEvent = StartGameCreditEventFingerprint(), cleared = PlayerPrefs.GetString(GameResultsClearedKey, ""),
                raw = AutoNvRawRecords(), volume = pcPlayerVolumeValue, credit = session.Credit,
                rounds = session.Rounds, wagered = session.Wagered, returned = session.Returned };
            PlayerPrefs.SetString(AutoNvReceiptKey, JsonUtility.ToJson(proof));
            PlayerPrefs.Save();
        }

        private AutoNvReceipt ReadAutoNvReceipt(string testCase, string phase)
        {
            var proof = JsonUtility.FromJson<AutoNvReceipt>(PlayerPrefs.GetString(AutoNvReceiptKey, ""));
            RequireAutoNv(proof != null && proof.testCase == testCase && proof.phase == phase, "missing expected receipt " + testCase + "/" + phase);
            return proof;
        }

        private void RequireAutoNvReceipt(AutoNvReceipt proof, bool bytes)
        {
            var differences = new List<string>();
            StartGameReceiptDifference(differences, "config", proof.config, AutoNvConfig(configuredAutoPlay));
            if (bytes) StartGameReceiptDifference(differences, "Auto bytes", proof.bytes, AutoNvBytes());
            StartGameReceiptDifference(differences, "economy", proof.economy, DemoCleanupEconomy());
            StartGameReceiptDifference(differences, "archive", proof.archive, DemoCleanupArchive(false));
            StartGameReceiptDifference(differences, "RNG", proof.rng, DemoCleanupRngBytes());
            StartGameReceiptDifference(differences, "Undo", proof.undo, StateBetCleanupUndoFingerprint());
            string pending = PlayerPrefs.GetString(PendingSpinKey, "");
            if (!string.IsNullOrEmpty(proof.pending) && !string.IsNullOrEmpty(pending))
                StartGameReceiptDifference(differences, "pending", StateBetPendingCommitFingerprint(proof.pending), StateBetPendingCommitFingerprint(pending));
            else StartGameReceiptDifference(differences, "pending", proof.pending, pending);
            StartGameReceiptDifference(differences, "staged", proof.staged, PlayerPrefs.GetString(StagedBetKey, ""));
            StartGameReceiptDifference(differences, "settledUndo", proof.settledUndo, PlayerPrefs.GetString(SettledTableUndoKey, ""));
            StartGameReceiptDifference(differences, "creditEvent", proof.creditEvent, StartGameCreditEventFingerprint());
            StartGameReceiptDifference(differences, "cleared", proof.cleared, PlayerPrefs.GetString(GameResultsClearedKey, ""));
            RequireAutoNv(differences.Count == 0 && autoNvOutcome.Pockets == 0 && autoNvOutcome.Suits == 0 &&
                !autoplay && pcPlayerVolumeValue == proof.volume && PlayerPrefs.GetInt(pcPlayerVolumeKey, -1) == proof.volume &&
                pcGameSoundsMuted == configuredAutoPlay.TurboAutoPlay,
                "cold receipt differs: " + string.Join("; ", differences.ToArray()));
        }

        private string AutoNvCapturePath(string suffix)
        {
            string path = AutoNvArgument("-rouletteAutoPlayNvCapturePrefix") + "_" + suffix + ".png";
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            RequireAutoNv(!File.Exists(path), "refusing to overwrite a capture");
            return path;
        }

        private void CaptureAutoNv(string suffix)
        { RefreshPortraitUi(); CaptureActiveScreenOffscreen(AutoNvCapturePath(suffix)); }

        private IEnumerator CaptureAutoNvFault()
        {
            string path = AutoNvCapturePath("fault");
            yield return null;
            ScreenCapture.CaptureScreenshot(path, 2);
            float deadline = Time.realtimeSinceStartup + 10f;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < deadline) yield return null;
            RequireAutoNv(File.Exists(path) && new FileInfo(path).Length > 0, "fault screenshot was not written");
        }
    }
}
