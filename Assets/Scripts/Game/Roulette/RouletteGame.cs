using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame : MonoBehaviour
    {
        private static readonly int[] RedNumbers = { 1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36 };
        private static readonly int[] WheelOrder = { 0,32,15,19,4,21,2,25,17,34,6,27,13,36,11,30,8,23,10,
            5,24,16,33,1,20,14,31,9,22,18,29,7,28,12,35,3,26 };
        private readonly string[] profiles = { "89", "93", "97" };
        private readonly int[] chips = { 1, 2, 5, 10 };
        private IRouletteSession session;
        private IRouletteOutcomeSource outcome;
        private DemoPocketOutcomeSource demoOutcome;
        private Texture2D wheel;
        private Texture2D spindle;
        private Texture2D ball;
        private Texture2D wheelShine, wheelShadow;
        private Texture2D betWinHighlight;
        private Texture2D[] winnerFrames;
        private Texture2D[] topperFrames;
        private Texture2D[] ideckAttractBackgroundFrames, ideckAttractForegroundFrames;
        private const float IDeckAttractFrameSeconds = .066f;
        // featdefs.xml uses 42 ms per DDA frame. TopperAttractFrameIndex
        // advances from a monotonic timestamp to preserve phase at long uptime.
        private AudioSource audioSource;
        private AudioSource wheelAudioSource;
        private AudioSource resultVoiceSource;
        private AudioClip click;
        private AudioClip coin;
        private AudioClip noMoreBets;
        private AudioClip betPlacedSound, betRemovedSound, betSelectedSound;
        private AudioClip betLimitSound, betClearSound, betUndoSound;
        private AudioClip[] wheelSpinSounds;
        private AudioClip[] resultNumberVoice, resultWinVoice;
        private GUIStyle titleStyle, infoStyle, buttonStyle, numberStyle, labelStyle;
        private bool showAllBets, showStats, neighborMode, autoplay, spinning;
        // StaticConfigParams::GetAuNationalStandardsEnabled is supplied by
        // the cabinet platform, not by the packaged roulette paytable.
        private bool auNationalStandardsEnabled;
        private string search = "";
        private string status = "Loading original paytable...";
        private float feedbackUntil;
        private Vector2 betScroll;
        // The filmed cabinet's 27-pocket return matches the original 89
        // paytable for the visible split bets. A saved PC round still owns
        // its recorded profile and is selected before session creation.
        private int profileIndex = 0;
        private double wheelAngle;
        private float spinStartedAt;
        private float spinDuration;
        private float winnerUntil;
        private float resultDisplayUntil;
        private float resultSettledAt;
        private long resultSettledTimestamp;
        private long proViewFlashStartedTimestamp;
        private bool doubleCollectPending;
        private float doubleExitStartedAt;
        private float doubleExitUntil;
        private long doubleRevealUntilTimestamp;
        private long doubleExitStartedTimestamp;
        private long doubleExitUntilTimestamp;
        private long doubleHelpUntilTimestamp;
        private Coroutine autoRoutine;
        private string capturePath;
        private float captureAt;
        private bool captureTaken;

        private void Awake()
        {
            pcAutoCompleteOwnerThread = System.Threading.Thread.CurrentThread.ManagedThreadId;
            auNationalStandardsEnabled = Array.IndexOf(
                Environment.GetCommandLineArgs(), "-rouletteAuNationalStandards") >= 0;
            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = Color.black;
            }
            try { LoadRecordedHistory(); }
            catch (PcRoundLifecycleDataException error)
            {
                FailPcRoundLifecycle("loaded", error);
                return;
            }
            SelectInitialProfileFromRecordedRound();
            // Existing *Smoke fixtures assert the source 97-table straight
            // return of 36. Keep their explicit diagnostic context stable;
            // ordinary new sessions use the filmed 89-table default.
            if (Environment.GetCommandLineArgs().Any(arg =>
                arg.StartsWith("-roulette", StringComparison.Ordinal) &&
                arg.EndsWith("Smoke", StringComparison.Ordinal)))
            {
                profileIndex = 2;
                Debug.Log("ROULETTE_PROFILE_LEGACY_SMOKE: 97");
            }
            wheel = Resources.Load<Texture2D>("Art/wheel");
            spindle = Resources.Load<Texture2D>("Art/spindle");
            ball = Resources.Load<Texture2D>("Art/ball");
            wheelShine = Resources.Load<Texture2D>("Art/wheel_shine");
            wheelShadow = Resources.Load<Texture2D>("Art/wheel_shadow");
            betWinHighlight = Resources.Load<Texture2D>("Art/bet_win_highlight");
            winnerFrames = Resources.LoadAll<Texture2D>("Animations/Win").OrderBy(x => x.name).ToArray();
            topperFrames = Resources.LoadAll<Texture2D>("Animations/Topper").OrderBy(x => x.name).ToArray();
            click = Resources.Load<AudioClip>("Audio/click");
            coin = Resources.Load<AudioClip>("Audio/coin");
            noMoreBets = Resources.Load<AudioClip>("Audio/no_more_bets");
            betPlacedSound = Resources.Load<AudioClip>("Audio/RouletteBets/placed");
            betRemovedSound = Resources.Load<AudioClip>("Audio/RouletteBets/removed");
            betSelectedSound = Resources.Load<AudioClip>("Audio/RouletteBets/selected");
            betLimitSound = Resources.Load<AudioClip>("Audio/RouletteBets/limit");
            betClearSound = Resources.Load<AudioClip>("Audio/RouletteBets/clear");
            betUndoSound = Resources.Load<AudioClip>("Audio/RouletteBets/undo");
            wheelSpinSounds = Enumerable.Range(0, 37).Select(i =>
                Resources.Load<AudioClip>("Audio/WheelSpin/" + i)).ToArray();
            resultNumberVoice = Enumerable.Range(0, 37).Select(i =>
                Resources.Load<AudioClip>("Audio/RouletteVoice/number_" + i)).ToArray();
            resultWinVoice = Enumerable.Range(0, 37).Select(i =>
                Resources.Load<AudioClip>("Audio/RouletteVoice/win_" + i)).ToArray();
            audioSource = gameObject.AddComponent<AudioSource>();
            wheelAudioSource = gameObject.AddComponent<AudioSource>();
            resultVoiceSource = gameObject.AddComponent<AudioSource>();
            LoadPcPlayerVolume();
            LoadPcCashMeterMode();
            LoadWinRacking();
            LoadOriginalScreen();
            if (originalLayout == null || originalLayout.wheel_ball == null)
                throw new InvalidOperationException(
                    "Original roulette ball asset position is missing");
            Rect initialBall = originalLayout.wheel_ball.ToRect();
            wheelBallPosition = initialBall.center;
            InitializeWheelVisualRandom();
            // 本项目按需求先做本地押分：Android 设备也走 PC 本地数学（不再要求 SBox 适配器）。
            if (!InitializePcAutoPlayState()) return;
            if (!InitializePcSasHostConfiguration()) return;
            try { pcOutcome = CreateGamePaymentSmokeOutcome(); }
            catch (Exception error)
            {
                FailPcRoundLifecycle("rng-loaded", error);
                return;
            }
            AttachRngReceiptSmokeHooks();
            AttachAutoCompleteSmokeOutcome();
            AttachGameSuspensionSmokeOutcome();
            AttachGamePaymentSmokeOutcome();
            demoOutcome = new DemoPocketOutcomeSource(CreateRoundLifecycleSmokeOutcome(CreateAutoPlayNvOutcome(
                CreateStateEndLifecycleOutcome(pcOutcome))));
            outcome = demoOutcome;
            LoadProfile();
            if(!sasStartupRuntimePending)ArmPcAutoPlayInitialIdleEntry();
        }

        private void Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-roulettePcPaymentMessageSmoke") >= 0)
                StartCoroutine(PcPaymentMessageSmoke());
            if (pcAutoPlayStorageFaulted)
            {
                if (Array.IndexOf(args, "-rouletteAutoPlayNvSmoke") >= 0)
                    StartAutoPlayNvSmoke();
                if (Array.IndexOf(args, "-rouletteRoundLifecycleSmoke") >= 0)
                    StartRoundLifecycleSmoke();
                if (Array.IndexOf(args, "-rouletteRngReceiptSmoke") >= 0)
                    StartRngReceiptSmoke();
                if (Array.IndexOf(args, "-rouletteAutoCompleteSmoke") >= 0)
                    StartAutoCompleteSmoke();
                if (Array.IndexOf(args, "-rouletteGameSuspensionSmoke") >= 0)
                    StartGameSuspensionSmoke();
                if (Array.IndexOf(args, "-rouletteAutoCompleteLegacySmoke") >= 0)
                    StartAutoCompleteLegacySmoke();
                if (Array.IndexOf(args, "-rouletteGamePaymentSmoke") >= 0)
                    StartGamePaymentSmoke();
                return;
            }
            int gameDelayIndex = Array.IndexOf(args, "-rouletteGameDelayMs");
            int parsedGameDelay;
            if (gameDelayIndex >= 0 && gameDelayIndex + 1 < args.Length &&
                int.TryParse(args[gameDelayIndex + 1], out parsedGameDelay) &&
                parsedGameDelay >= 0)
                gameDelayMilliseconds = parsedGameDelay;
            int demoShowModeIndex = Array.IndexOf(args, "-rouletteDemoShowMode");
            if (demoShowModeIndex >= 0 && demoShowModeIndex + 1 < args.Length &&
                args[demoShowModeIndex + 1] == "1")
                demoShowMode = 1;
            bool demoMenuSmoke = Array.IndexOf(args, "-rouletteDemoMenuSmoke") >= 0;
            bool demoAutoCreditsSmoke = Array.IndexOf(args,
                "-rouletteDemoAutoCreditsSmoke") >= 0;
            bool captureDemoMenu = Array.IndexOf(args, "-rouletteCaptureDemoMenu") >= 0;
            if (demoMenuSmoke || demoAutoCreditsSmoke || captureDemoMenu)
                demoShowMode = 1;
            demoMenuState = demoShowMode > 0 ? 2 : 0;
            EnableSourceDemoMode();
            if (SourceRoundBlocksOrdinaryInput) DisableSourceDemoMode();
            if (Array.IndexOf(args, "-rouletteViewIDeck") >= 0)
                SetIDeckPreview(true);
            if (Array.IndexOf(args, "-rouletteViewIDeckAttract") >= 0 ||
                Array.IndexOf(args, "-rouletteIdeckAttractSmoke") >= 0)
                SetIDeckAttractPreview();
            bool ideckAutoSmoke = Array.IndexOf(args, "-rouletteIDeckAutoAttractSmoke") >= 0;
            if (ideckAutoSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 0,
                    new FixedPocketSmokeOutcome());
                SetIDeckPreview(true);
            }
            bool topperSmoke = Array.IndexOf(args, "-rouletteTopperSmoke") >= 0;
            bool topperAutoSmoke = Array.IndexOf(args, "-rouletteTopperAutoAttractSmoke") >= 0;
            if (topperAutoSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 0,
                    new FixedPocketSmokeOutcome());
                SetTopperPreview(true);
            }
            if (Array.IndexOf(args, "-rouletteViewTopper") >= 0 || topperSmoke)
                SetTopperPreview(true);
            if (Array.IndexOf(args, "-rouletteViewTopperAttract") >= 0)
                SetTopperAttractPreview();
            if (Array.IndexOf(args, "-rouletteViewHistogram") >= 0)
                SetHistogramPreview(true);
            bool winSmoke = Array.IndexOf(args, "-rouletteWinSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteWinTimingSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteRackInputSnapSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteAutoTimingSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteAutoWinFilmGateSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteAutoCancelResultSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteFooterMovieSmoke") >= 0;
            bool highWinAutoSmoke = Array.IndexOf(args, "-rouletteHighWinAutoSmoke") >= 0;
            bool autoLossGateSmoke = Array.IndexOf(args, "-rouletteAutoLossGateSmoke") >= 0;
            bool autoFundAfterSpinSmoke = Array.IndexOf(args,
                "-rouletteAutoFundAfterSpinSmoke") >= 0;
            bool autoDuringSpinSmoke = Array.IndexOf(args,
                "-rouletteAutoDuringSpinSmoke") >= 0;
            bool highWinManualSmoke = Array.IndexOf(args, "-rouletteHighWinManualSmoke") >= 0;
            bool skillStopSmoke = Array.IndexOf(args, "-rouletteSkillStopSmoke") >= 0;
            bool turboDemoSmoke = Array.IndexOf(args, "-rouletteTurboDemoSmoke") >= 0;
            bool demoFixedStopSmoke = Array.IndexOf(args,
                "-rouletteDemoFixedStopSmoke") >= 0;
            bool captureRound = Array.IndexOf(args, "-rouletteCaptureRound") >= 0 || winSmoke;
            bool captureStack = Array.IndexOf(args, "-rouletteCaptureStack") >= 0;
            bool captureChipDepth = Array.IndexOf(args, "-rouletteCaptureChipDepth") >= 0;
            bool captureChipDepthWin = Array.IndexOf(args, "-rouletteChipDepthWin") >= 0;
            bool historySmoke = Array.IndexOf(args, "-rouletteHistorySmoke") >= 0;
            bool uiLossSmoke = Array.IndexOf(args, "-rouletteUiLossSmoke") >= 0;
            bool uiZeroCreditSmoke = Array.IndexOf(args, "-rouletteUiZeroCreditSmoke") >= 0;
            bool uiSmoke = Array.IndexOf(args, "-rouletteUiSmoke") >= 0 ||
                historySmoke || uiLossSmoke || uiZeroCreditSmoke;
            if ((uiLossSmoke || uiZeroCreditSmoke) && session != null)
                session = new RouletteSession(session.Profile,
                    uiZeroCreditSmoke ? 1 : 1000,
                    new FixedPocketSmokeOutcome(16)) { DoubleEnabled = true };
            bool captureDoubleHelp = Array.IndexOf(args, "-rouletteCaptureDoubleHelp") >= 0;
            bool captureDouble = captureDoubleHelp ||
                Array.IndexOf(args, "-rouletteCaptureDouble") >= 0 ||
                Array.IndexOf(args, "-rouletteSuitSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteDoubleMainClickSmoke") >= 0;
            int capturePocketIndex = Array.IndexOf(args, "-rouletteCapturePocket");
            int capturePocket = -1;
            bool fixedCapturePocket = capturePocketIndex >= 0 &&
                capturePocketIndex + 1 < args.Length &&
                int.TryParse(args[capturePocketIndex + 1], out capturePocket) &&
                capturePocket >= 0 && capturePocket <= 36;
            bool doubleMaximumSmoke = Array.IndexOf(args, "-rouletteDoubleMaximumSmoke") >= 0;
            // These existing diagnostics commit their first spin inside Start,
            // before the input module's first Update. Finish the genuine
            // default initial Idle entry before constructing their fixture;
            // their immediate spin, Auto toggle and capture ordering can then
            // remain unchanged. Ordinary startup still uses the live module.
            if (demoFixedStopSmoke || turboDemoSmoke || skillStopSmoke ||
                highWinAutoSmoke || highWinManualSmoke || autoLossGateSmoke ||
                autoFundAfterSpinSmoke || autoDuringSpinSmoke || fixedCapturePocket ||
                captureRound || topperSmoke || captureChipDepthWin ||
                Array.IndexOf(args, "-rouletteCaptureSpin") >= 0 ||
                Array.IndexOf(args, "-rouletteCreditGuardSmoke") >= 0)
                PrepareImmediateDiagnosticIdleEntry();
            if (Array.IndexOf(args, "-rouletteCaptureAutoConfirm") >= 0)
                RequestAutoPlay();
            int paysIndex = Array.IndexOf(args, "-rouletteCapturePays");
            int capturedPaysPage;
            if (paysIndex >= 0 && paysIndex + 1 < args.Length &&
                int.TryParse(args[paysIndex + 1], out capturedPaysPage))
            {
                paysPage = Mathf.Clamp(capturedPaysPage - 1, 0, 3);
                ResetPaysTimeout();
            }
            if (doubleMaximumSmoke && session != null)
            {
                bool suitMaximum = Array.IndexOf(args, "-rouletteSuitMode") >= 0;
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(), 150, 16,
                    doubleSuitMode: suitMaximum) { DoubleEnabled = true };
                session.PlaceBet(100, 1);
                session.Spin();
                session.StartDouble();
                if (suitMaximum)
                {
                    doubleEntranceStartedAt = float.NegativeInfinity;
                    doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
                    Guess(0);
                    AudioClip archivedCreditEnd = Resources.Load<AudioClip>(
                        "Audio/DoubleUp/credit_end");
                    if (!doubleRevealWon || !doubleRevealMaximum ||
                        session.InDoubleUp || session.Credit != 1143 ||
                        archivedCreditEnd == null || suitMaximumAward != null)
                        throw new InvalidOperationException(
                            "Suit maximum award media or payout differs from source");
                    Debug.Log("ROULETTE_SUIT_MAXIMUM_PASSED: award=" + session.DoubleStake +
                        " credit-end archived=" + archivedCreditEnd.name +
                        " without undefined feature playback; missing bitmap kept absent");
                }
                else
                {
                    if (!session.GuessDouble(-1) || !session.GuessDouble(-1) ||
                        session.InDoubleUp || session.Credit != 1143)
                        throw new InvalidOperationException("Double maximum award smoke failed");
                    doubleRevealWon = true;
                    doubleRevealMaximum = true;
                    doubleRevealChoice = -1;
                    doubleRevealUntil = Time.realtimeSinceStartup + 1.25f;
                    doubleRevealUntilTimestamp = OriginalRackClock.AfterMilliseconds(
                        System.Diagnostics.Stopwatch.GetTimestamp(), 1250,
                        System.Diagnostics.Stopwatch.Frequency);
                    Debug.Log("ROULETTE_DOUBLE_MAXIMUM_PASSED: award=" + session.DoubleStake);
                }
            }
            else if (captureDouble && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(), doubleSuitMode:
                    Array.IndexOf(args, "-rouletteSuitMode") >= 0) { DoubleEnabled = true };
                for (int pocket = 0; pocket <= 36; ++pocket)
                    session.PlaceBet(100 + pocket, 1);
                session.Spin();
                session.StartDouble();
                doubleEntranceStartedAt = Time.realtimeSinceStartup;
                doubleIdleAnimationStartedAt = Time.realtimeSinceStartup + 1f;
                doubleIdleAnimationStartedTimestamp = OriginalRackClock.AfterMilliseconds(
                    doubleEntranceStartedTimestamp, 1000,
                    System.Diagnostics.Stopwatch.Frequency);
            }
            if (captureStack && session != null)
            {
                session.PlaceBet(100, 1);
                session.SetChip(5);
                session.PlaceBet(100, 5);
            }
            if (captureChipDepthWin && session != null)
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(4));
            if ((captureChipDepth || captureChipDepthWin) && session != null)
            {
                // Deliberately place the z=12 spot before the overlapping
                // z=10 spot. The source depth must win over click order.
                session.SetChip(2);
                session.PlaceBet(199, 2);
                session.SetChip(1);
                session.PlaceBet(104, 1);
                Debug.Log("ROULETTE_CHIP_DEPTH_FIXTURE: spot199 z12 before spot104 z10");
            }
            if (Array.IndexOf(args, "-rouletteCreditGuardSmoke") >= 0)
                CreditGuardSmoke();
            if (demoFixedStopSmoke && session != null && demoOutcome != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    demoOutcome) { DoubleEnabled = true };
                session.PlaceBet(127, 1);
                demoOutcome.SelectFixedPocket(27);
                BeginSpin();
            }
            else if (demoMenuSmoke && session != null && demoOutcome != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    demoOutcome) { DoubleEnabled = true };
                session.PlaceBet(127, 1);
            }
            else if (demoAutoCreditsSmoke && session != null && demoOutcome != null)
                session = new RouletteSession(session.Profile, 1000,
                    demoOutcome) { DoubleEnabled = true };
            else if (turboDemoSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
                session.PlaceBet(100, 1);
                configuredAutoPlay.SetOptions(true, false, true, true, true, 0);
                // Demo option 2 calls GameSounds::TurnOffAllSounds after
                // enabling Turbo AutoPlay in the original roulette binary.
                SetPcGameSoundsMuted(true);
                Debug.Log("ROULETTE_TURBO_DEMO_SOUNDS_OFF");
                ToggleAuto();
            }
            else if (skillStopSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
                session.PlaceBet(100, 1);
                BeginSpin();
            }
            else if ((highWinAutoSmoke || highWinManualSmoke) && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
                if (highWinAutoSmoke) EnableDeferredPaymentSmokeFixture();
                session.PlaceBet(100, 20);
                BeginSpin();
            }
            else if (autoLossGateSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(16)) { DoubleEnabled = true };
                EnableDeferredPaymentSmokeFixture();
                session.PlaceBet(100, 1);
                BeginSpin();
            }
            else if (autoFundAfterSpinSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 1,
                    new FixedPocketSmokeOutcome(16));
                session.PlaceBet(100, 1);
                BeginSpin();
            }
            else if (autoDuringSpinSmoke && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(16));
                session.PlaceBet(100, 1);
                BeginSpin();
            }
            else if (fixedCapturePocket && session != null)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(capturePocket),
                    baseMaximumWin: session.BaseMaximumWin) { DoubleEnabled = true };
                int startSlotOption = Array.IndexOf(args, "-rouletteWheelStartSlot");
                int startSlot;
                if (startSlotOption >= 0 && startSlotOption + 1 < args.Length &&
                    int.TryParse(args[startSlotOption + 1], out startSlot) &&
                    startSlot >= 0 && startSlot < 37)
                {
                    wheelAngle = OriginalWheelArithmetic.RestorePhase(startSlot);
                    wheelRenderedAngle = wheelAngle;
                    wheelMainRenderAngle = OriginalWheelArithmetic.RestoreMainAngle(startSlot);
                    wheelSpindleRenderAngle = OriginalWheelArithmetic.RenderAngle(wheelAngle);
                    Debug.Log("ROULETTE_DIAGNOSTIC_WHEEL_START_SLOT: " + startSlot);
                }
                session.PlaceBet(100 + capturePocket, 1);
                BeginSpin();
            }
            else if (winSmoke && session != null)
            {
                // Diagnostic wins must start from a fresh round even when a
                // previous Player run left an archived result on disk.
                session = new RouletteSession(session.Profile, 1000, outcome)
                    { DoubleEnabled = true };
                EnableDeferredPaymentSmokeFixture();
                for (int pocket = 0; pocket <= 36; ++pocket)
                    session.PlaceBet(100 + pocket, 1);
                BeginSpin();
            }
            else if ((Array.IndexOf(args, "-rouletteCaptureSpin") >= 0 || captureRound ||
                topperSmoke) && session != null)
            {
                session.PlaceBet(100, 1);
                BeginSpin();
            }
            else if (captureChipDepthWin && session != null) BeginSpin();
            int index = Array.IndexOf(args, "-rouletteCapture");
            if (index >= 0 && index + 1 < args.Length)
            {
                capturePath = args[index + 1];
                captureAt = Time.realtimeSinceStartup +
                    (captureRound ? spinDuration + .7f :
                        (captureStack || captureChipDepth) ? 1.2f :
                        doubleMaximumSmoke ? .4f : 3f);
                int at = Array.IndexOf(args, "-rouletteCaptureAt");
                float captureDelay;
                if (at >= 0 && at + 1 < args.Length &&
                    float.TryParse(args[at + 1], System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out captureDelay))
                    captureAt = Time.realtimeSinceStartup + captureDelay;
            }
            CreatePortraitUi();
            if (Array.IndexOf(args, "-roulettePaytableTestSmoke") >= 0)
                StartCoroutine(PaytableTestSmoke());
            if (Array.IndexOf(args, "-rouletteCompleteFontSmoke") >= 0)
                StartCoroutine(CompleteFontSmoke());
            if (Array.IndexOf(args, "-rouletteOperatorFallbackSmoke") >= 0)
                StartCoroutine(OperatorFallbackSmoke());
            if (Array.IndexOf(args, "-rouletteCaptureGameInfo") >= 0)
                ToggleGameInfoOptions();
            int markerPreviewOption = Array.IndexOf(args,
                "-rouletteBetMarkerPreviewSpot");
            int markerPreviewSpot;
            if (markerPreviewOption >= 0 && markerPreviewOption + 1 < args.Length &&
                int.TryParse(args[markerPreviewOption + 1], out markerPreviewSpot) &&
                markerPreviewSpot >= 0 && markerPreviewSpot < 157 && session != null)
            {
                if (Array.IndexOf(args, "-rouletteBetMarkerPreviewRejected") >= 0)
                    session.Profile.MaximumSpotBet = 0;
                ShowBetMarker(markerPreviewSpot);
                Debug.Log("ROULETTE_BET_MARKER_PREVIEW: spot=" + markerPreviewSpot +
                    " rejected=" + (session.Profile.MaximumSpotBet == 0));
            }
            if (captureDemoMenu) ToggleDemoMenu();
            if (captureDoubleHelp)
            {
                // Diagnostic help capture starts directly in the decision
                // state instead of waiting through the feature entrance.
                doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
                OpenDoubleHelp();
            }
            CreateDiagnosticUi();
            if (Array.IndexOf(args, "-rouletteServiceConfirmPreview") >= 0)
            {
                SetIDeckPreview(true);
                RequestPcService();
                Debug.Log("ROULETTE_PC_SERVICE_CONFIRM_PREVIEW");
            }
            if (Array.IndexOf(args, "-rouletteCollectConfirmPreview") >= 0)
            {
                SetIDeckPreview(true);
                RequestPcCollectConfirmation();
                Debug.Log("ROULETTE_PC_COLLECT_CONFIRM_PREVIEW");
            }
            int offscreenIndex = Array.IndexOf(args, "-rouletteOffscreenCapture");
            if (offscreenIndex >= 0 && offscreenIndex + 1 < args.Length)
            {
                float offscreenDelay = 1f;
                int offscreenAt = Array.IndexOf(args, "-rouletteOffscreenAt");
                float parsedDelay;
                if (offscreenAt >= 0 && offscreenAt + 1 < args.Length &&
                    float.TryParse(args[offscreenAt + 1],
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out parsedDelay) && parsedDelay >= 0f)
                    offscreenDelay = parsedDelay;
                StartCoroutine(CapturePortraitOffscreen(args[offscreenIndex + 1],
                    offscreenDelay));
            }
            if (resumePendingSpin && session != null && session.SpinPending &&
                Array.IndexOf(args, "-roulettePendingSaveSmoke") < 0 &&
                Array.IndexOf(args, "-roulettePendingLoadSmoke") < 0 &&
                Array.IndexOf(args, "-roulettePendingAlreadySettledSmoke") < 0)
            {
                StartFooterMovie();
                StartCoroutine(SpinVisual(session.PendingPocket));
            }
            if (Array.IndexOf(args, "-roulettePendingSaveSmoke") >= 0)
                StartCoroutine(PendingSpinSaveSmoke());
            if (Array.IndexOf(args, "-roulettePendingLoadSmoke") >= 0)
                StartCoroutine(PendingSpinLoadSmoke());
            if (Array.IndexOf(args, "-roulettePendingAlreadySettledSmoke") >= 0)
                StartCoroutine(PendingSpinAlreadySettledSmoke());
            if (Array.IndexOf(args, "-roulettePendingStaleSaveSmoke") >= 0)
                StartCoroutine(PendingSpinStaleSaveSmoke());
            if (Array.IndexOf(args, "-roulettePendingStaleLoadSmoke") >= 0)
                StartCoroutine(PendingSpinStaleLoadSmoke());
            if (Array.IndexOf(args, "-rouletteStagedSaveSmoke") >= 0)
                StartCoroutine(StagedBetsSaveSmoke());
            if (Array.IndexOf(args, "-rouletteStagedLoadSmoke") >= 0)
                StartCoroutine(StagedBetsLoadSmoke());
            if (Array.IndexOf(args, "-rouletteGroupUndoSaveSmoke") >= 0)
                StartCoroutine(GroupUndoSaveSmoke());
            if (Array.IndexOf(args, "-rouletteGroupUndoLoadSmoke") >= 0)
                StartCoroutine(GroupUndoLoadSmoke());
            if (Array.IndexOf(args, "-rouletteBetLimitSmoke") >= 0)
                StartCoroutine(BetLimitPromptSmoke());
            if (Array.IndexOf(args, "-rouletteFailurePromptClockSmoke") >= 0)
                StartCoroutine(FailurePromptClockSmoke());
            if (Array.IndexOf(args, "-roulettePromptDelayClockSmoke") >= 0)
                StartCoroutine(PromptDelayClockSmoke());
            if (Array.IndexOf(args, "-rouletteStartupMessageSmoke") >= 0)
                StartCoroutine(StartupMessageSmoke());
            if (Array.IndexOf(args, "-rouletteIdeckAttractSmoke") >= 0)
                StartCoroutine(IDeckAttractSmoke());
            if (ideckAutoSmoke)
                StartCoroutine(IDeckAutomaticAttractSmoke());
            if (topperAutoSmoke)
                StartCoroutine(TopperAutomaticAttractSmoke());
            int sequenceIndex = Array.IndexOf(args, "-rouletteCaptureSequence");
            if (sequenceIndex >= 0 && sequenceIndex + 1 < args.Length && spinning)
                StartCoroutine(CaptureWheelSequence(args[sequenceIndex + 1]));
            if (Array.IndexOf(args, "-rouletteViewHistory") >= 0)
                SetHistoryView(true);
            if (doubleMaximumSmoke && portraitCanvas != null)
            {
                RefreshPortraitUi();
                bool suitMaximum = session.DoubleSuitMode;
                if (!doubleRoot.gameObject.activeSelf ||
                    uiDoubleMessage.texture !=
                        (suitMaximum ? suitMaximumAward : doubleMaximumAward) ||
                    uiDoubleMessage.gameObject.activeSelf == suitMaximum ||
                    Vector2.Distance(uiDoubleMessage.rectTransform.anchoredPosition,
                        new Vector2(239, -686)) > .01f ||
                    Vector2.Distance(uiDoubleMessage.rectTransform.sizeDelta,
                        new Vector2(585, 70)) > .01f)
                    throw new InvalidOperationException("Double maximum award uGUI binding failed");
                Debug.Log("ROULETTE_DOUBLE_MAXIMUM_UI_PASSED: source bitmap availability and rectangle");
            }
            if (Array.IndexOf(args, "-rouletteAutoTimingSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteAutoWinFilmGateSmoke") >= 0)
            {
                ToggleAuto();
                StartCoroutine(AutoTimingSmoke());
            }
            if (Array.IndexOf(args, "-rouletteAutoCancelResultSmoke") >= 0)
            {
                ToggleAuto();
                StartCoroutine(AutoCancelResultSmoke());
            }
            if (Array.IndexOf(args, "-rouletteAutoFundStopSmoke") >= 0)
                StartCoroutine(AutoFundStopSmoke());
            if (Array.IndexOf(args, "-rouletteAutoManualPlayGateSmoke") >= 0)
                StartCoroutine(AutoManualPlayGateSmoke());
            if (highWinAutoSmoke)
            {
                ToggleAuto();
                StartCoroutine(HighWinAutoTimingSmoke());
            }
            if (autoLossGateSmoke)
            {
                ToggleAuto();
                StartCoroutine(AutoLossGateSmoke());
            }
            if (autoFundAfterSpinSmoke)
            {
                ToggleAuto();
                StartCoroutine(AutoFundAfterSpinSmoke());
            }
            if (autoDuringSpinSmoke)
                StartCoroutine(AutoDuringSpinSmoke());
            if (highWinManualSmoke)
                StartCoroutine(HighWinManualTimingSmoke());
            if (skillStopSmoke)
                StartCoroutine(SkillStopSmoke());
            if (Array.IndexOf(args, "-rouletteWinTimingSmoke") >= 0)
                StartCoroutine(WinTimingSmoke());
            if (Array.IndexOf(args, "-rouletteRackInputSnapSmoke") >= 0)
                StartCoroutine(RackInputSnapSmoke());
            if (Array.IndexOf(args, "-roulettePostResultBetSmoke") >= 0)
                StartCoroutine(PostResultBetSmoke());
            if (Array.IndexOf(args, "-rouletteBallFallbackSmoke") >= 0)
                StartCoroutine(BallFallbackSmoke());
            if (Array.IndexOf(args, "-rouletteResultCycleSmoke") >= 0)
                StartCoroutine(ResultCycleSmoke());
            if (Array.IndexOf(args, "-rouletteFooterMovieSmoke") >= 0)
                StartCoroutine(FooterMovieSmoke());
            if (Array.IndexOf(args, "-rouletteFooterMovieClockSmoke") >= 0)
                StartCoroutine(FooterMovieClockSmoke());
            if (Array.IndexOf(args, "-roulettePaidFadeSmoke") >= 0)
                StartCoroutine(PaidFadeSmoke());
            if (Array.IndexOf(args, "-rouletteDemoSoundGateSmoke") >= 0)
                StartCoroutine(DemoSoundGateSmoke());
            if (Array.IndexOf(args, "-rouletteDemoMenuDownSmoke") >= 0)
                StartCoroutine(DemoMenuDownSmoke());
            if (Array.IndexOf(args, "-rouletteConfiguredAutoPlaySmoke") >= 0)
                StartCoroutine(ConfiguredAutoPlaySmoke());
            if (Array.IndexOf(args, "-rouletteDemoResultCleanupSmoke") >= 0)
                StartCoroutine(DemoResultCleanupSmoke(false));
            if (Array.IndexOf(args, "-rouletteDemoResultCleanupLoadSmoke") >= 0)
                StartCoroutine(DemoResultCleanupSmoke(true));
            if (Array.IndexOf(args, "-rouletteStateBetResultCleanupSmoke") >= 0)
                StartCoroutine(StateBetResultCleanupSmoke(false));
            if (Array.IndexOf(args, "-rouletteStateBetResultCleanupLoadSmoke") >= 0)
                StartCoroutine(StateBetResultCleanupSmoke(true));
            if (Array.IndexOf(args, "-rouletteStartGameStateBetSmoke") >= 0)
                StartCoroutine(StartGameStateBetSmoke(false));
            if (Array.IndexOf(args, "-rouletteStartGameStateBetLoadSmoke") >= 0)
                StartCoroutine(StartGameStateBetSmoke(true));
            if (Array.IndexOf(args, "-rouletteStartGameConfiguredOrderSmoke") >= 0)
                StartCoroutine(StartGameConfiguredOrderSmoke());
            if (Array.IndexOf(args, "-rouletteDemoSelectedTextSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteDemoSelectedTextLoadSmoke") >= 0)
                StartCoroutine(DemoSelectedTextSmoke());
            if (Array.IndexOf(args, "-rouletteStateEndLifecycleSmoke") >= 0 ||
                Array.IndexOf(args, "-rouletteStateEndLifecycleLoadSmoke") >= 0)
                StartCoroutine(StateEndLifecycleSmoke());
            if (Array.IndexOf(args, "-rouletteStateBetPendingUndoSaveSmoke") >= 0)
                StartCoroutine(StateBetPendingUndoSmoke(false));
            if (Array.IndexOf(args, "-rouletteStateBetPendingUndoLoadSmoke") >= 0)
                StartCoroutine(StateBetPendingUndoSmoke(true));
            if (Array.IndexOf(args, "-rouletteDemoMathPathSmoke") >= 0)
                StartCoroutine(DemoMathPathSmoke());
            if (Array.IndexOf(args, "-rouletteSourceInputQueueSmoke") >= 0)
                StartCoroutine(SourceInputQueueSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleCollectSmoke") >= 0)
                StartCoroutine(DoubleCollectSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleExitSmoke") >= 0)
                StartCoroutine(DoubleExitSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleHelpSmoke") >= 0)
                StartCoroutine(DoubleHelpSmoke());
            if (Array.IndexOf(args, "-rouletteMainPlayDownSmoke") >= 0)
                StartCoroutine(MainPlayDownSmoke());
            if (Array.IndexOf(args, "-rouletteMainPlayMoveSmoke") >= 0)
                StartCoroutine(MainPlayMoveSmoke());
            if (Array.IndexOf(args, "-rouletteProViewVolumeMoveSmoke") >= 0)
                StartCoroutine(ProViewVolumeMoveSmoke());
            if (Array.IndexOf(args, "-rouletteSuitHelpLimitSmoke") >= 0)
                StartCoroutine(SuitHelpLimitSmoke());
            if (Array.IndexOf(args, "-rouletteSuitCashMeterSmoke") >= 0)
                StartCoroutine(SuitCashMeterSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleButtonStatesSmoke") >= 0)
                StartCoroutine(DoubleButtonStatesSmoke());
            if (Array.IndexOf(args, "-rouletteSuitSmoke") >= 0)
                StartCoroutine(SuitBranchSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleMainClickSmoke") >= 0)
                StartCoroutine(DoubleMainClickSmoke());
            if (Array.IndexOf(args, "-rouletteHistogramSmoke") >= 0)
                StartCoroutine(HistogramSmoke());
            if (Array.IndexOf(args, "-rouletteHistogramSaveSmoke") >= 0)
                StartCoroutine(HistogramSaveSmoke());
            if (Array.IndexOf(args, "-rouletteHistogramLoadSmoke") >= 0)
                StartCoroutine(HistogramLoadSmoke());
            if (Array.IndexOf(args, "-rouletteHistoryPageSmoke") >= 0)
                StartCoroutine(HistoryPageSmoke());
            if (Array.IndexOf(args, "-rouletteHistoryColorCapture") >= 0)
                StartCoroutine(HistoryColorCapture());
            if (Array.IndexOf(args, "-rouletteToteColorSmoke") >= 0)
                StartCoroutine(ToteColorSmoke());
            if (Array.IndexOf(args, "-rouletteRoundHistorySaveSmoke") >= 0)
                StartCoroutine(RoundHistorySaveSmoke());
            if (Array.IndexOf(args, "-rouletteRoundHistoryLoadSmoke") >= 0)
                StartCoroutine(RoundHistoryLoadSmoke());
            if (Array.IndexOf(args, "-rouletteHistoryNextSpinSmoke") >= 0)
                StartCoroutine(HistoryNextSpinSmoke());
            if (Array.IndexOf(args, "-rouletteHistoryNextSpinLoadSmoke") >= 0)
                StartCoroutine(HistoryNextSpinLoadSmoke());
            if (Array.IndexOf(args, "-rouletteTotalsSaveSmoke") >= 0)
                StartCoroutine(TotalsSaveSmoke());
            if (Array.IndexOf(args, "-rouletteTotalsLoadSmoke") >= 0)
                StartCoroutine(TotalsLoadSmoke());
            if (Array.IndexOf(args, "-rouletteRecoverySaveSmoke") >= 0)
                StartCoroutine(RecoverySaveSmoke());
            if (Array.IndexOf(args, "-rouletteRecoveryLoadSmoke") >= 0)
                StartCoroutine(RecoveryLoadSmoke());
            if (Array.IndexOf(args, "-rouletteRecoveryCompletedSmoke") >= 0)
                StartCoroutine(RecoveryCompletedSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleHistorySmoke") >= 0)
                StartCoroutine(DoubleHistorySmoke());
            if (Array.IndexOf(args, "-rouletteDoubleHistoryLifetimeSmoke") >= 0)
                StartCoroutine(DoubleHistoryLifetimeSmoke(false));
            if (Array.IndexOf(args, "-rouletteDoubleHistoryLifetimeLoadSmoke") >= 0)
                StartCoroutine(DoubleHistoryLifetimeSmoke(true));
            if (Array.IndexOf(args, "-rouletteRngScheduleSmoke") >= 0)
                StartCoroutine(RngScheduleSmoke());
            if (Array.IndexOf(args, "-rouletteDoubleAutoGuardSmoke") >= 0)
                StartCoroutine(DoubleAutoGuardSmoke());
            if (topperSmoke)
                StartCoroutine(TopperResultSmoke());
            if (Array.IndexOf(args, "-rouletteIdeckSmoke") >= 0)
                StartCoroutine(IDeckSmoke());
            if (Array.IndexOf(args, "-rouletteCollectSourceSmoke") >= 0)
                StartCoroutine(CollectSourceSmoke());
            if (Array.IndexOf(args, "-roulettePcCollectSaveSmoke") >= 0)
                StartCoroutine(PcCollectSaveSmoke());
            if (Array.IndexOf(args, "-roulettePcCollectLoadSmoke") >= 0)
                StartCoroutine(PcCollectLoadSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomSaveSmoke") >= 0)
                StartCoroutine(PcDenomSaveSmoke());
            if (Array.IndexOf(args, "-roulettePcCashMeterSaveSmoke") >= 0)
                StartCoroutine(PcCashMeterSaveSmoke());
            if (Array.IndexOf(args, "-rouletteProViewFontSmoke") >= 0)
                StartCoroutine(ProViewFontSmoke());
            if (Array.IndexOf(args, "-roulettePcCashMeterLoadSmoke") >= 0)
                StartCoroutine(PcCashMeterLoadSmoke());
            if (Array.IndexOf(args, "-roulettePcDemoCashMeterSmoke") >= 0)
                StartCoroutine(PcDemoCashMeterSmoke());
            if (Array.IndexOf(args, "-roulettePcCashMeterPackagedDefaultSmoke") >= 0)
                StartCoroutine(PcCashMeterPackagedDefaultSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomLoadCollectSmoke") >= 0)
                StartCoroutine(PcDenomLoadCollectSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomCollectLoadSmoke") >= 0)
                StartCoroutine(PcDenomCollectLoadSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomRoundSaveSmoke") >= 0)
                StartCoroutine(PcDenomRoundSaveSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomRoundLoadSmoke") >= 0)
                StartCoroutine(PcDenomRoundLoadSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomRemainderCollectSmoke") >= 0)
                StartCoroutine(PcDenomRemainderCollectSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomInterruptedSaveSmoke") >= 0)
                StartCoroutine(PcDenomInterruptedSaveSmoke());
            if (Array.IndexOf(args, "-roulettePcDenomInterruptedLoadSmoke") >= 0)
                StartCoroutine(PcDenomInterruptedLoadSmoke());
            if (Array.IndexOf(args, "-roulettePcVolumeTimeoutSmoke") >= 0)
                StartCoroutine(PcVolumeTimeoutSmoke());
            if (Array.IndexOf(args, "-roulettePcVolumeReadySaveSmoke") >= 0)
                StartCoroutine(PcVolumeReadySaveSmoke());
            if (Array.IndexOf(args, "-roulettePcVolumeReadyLoadSmoke") >= 0)
                StartCoroutine(PcVolumeReadyLoadSmoke());
            if (Array.IndexOf(args, "-roulettePcCollectReopenSmoke") >= 0)
                StartCoroutine(PcCollectReopenSmoke());
            if (Array.IndexOf(args, "-roulettePcCollectPlaySmoke") >= 0)
                StartCoroutine(PcCollectPlaySmoke());
            if (Array.IndexOf(args, "-roulettePcCollectRoundReopenSmoke") >= 0)
                StartCoroutine(PcCollectRoundReopenSmoke());
            if (Array.IndexOf(args, "-roulettePcCollectInterruptedSaveSmoke") >= 0)
                StartCoroutine(PcCollectInterruptedSaveSmoke());
            if (Array.IndexOf(args, "-roulettePcCollectInterruptedLoadSmoke") >= 0)
                StartCoroutine(PcCollectInterruptedLoadSmoke());
            if (Array.IndexOf(args, "-rouletteCollectRaycastSmoke") >= 0)
                StartCoroutine(CollectRaycastSmoke());
            if (Array.IndexOf(args, "-rouletteAutoConfirmSmoke") >= 0)
                StartCoroutine(AutoConfirmSmoke());
            if (Array.IndexOf(args, "-roulettePaysSmoke") >= 0)
                StartCoroutine(PaysSmoke());
            if (Array.IndexOf(args, "-rouletteSectorSmoke") >= 0)
                StartCoroutine(SectorSmoke());
            if (Array.IndexOf(args, "-rouletteUndoNeighborSmoke") >= 0)
                StartCoroutine(UndoNeighborSmoke());
            if (Array.IndexOf(args, "-rouletteNeighborRestoreSaveSmoke") >= 0)
                StartCoroutine(NeighborRestoreSaveSmoke());
            if (Array.IndexOf(args, "-rouletteNeighborRestoreLoadSmoke") >= 0)
                StartCoroutine(NeighborRestoreLoadSmoke());
            if (uiSmoke && portraitCanvas != null)
            {
                captureAt = float.PositiveInfinity;
                StartCoroutine(UiSmokeRound(historySmoke));
            }
            if (Array.IndexOf(args, "-rouletteBetSoundSmoke") >= 0 &&
                portraitCanvas != null)
                StartCoroutine(BetSoundSmoke());
            if (Array.IndexOf(args, "-rouletteMainTouchConfirmSmoke") >= 0 &&
                portraitCanvas != null)
                StartCoroutine(MainTouchConfirmationSmoke());
            if (Array.IndexOf(args, "-rouletteTableDragSmoke") >= 0 &&
                portraitCanvas != null)
                StartCoroutine(TableDragSmoke());
            if (turboDemoSmoke)
                StartCoroutine(TurboDemoSmoke());
            if (demoFixedStopSmoke)
                StartCoroutine(DemoFixedStopSmoke());
            if (demoMenuSmoke)
                StartCoroutine(DemoMenuSmoke());
            if (demoAutoCreditsSmoke)
                StartCoroutine(DemoAutoCreditsSmoke());
            if (Array.IndexOf(args, "-rouletteAutoPlayNvSmoke") >= 0)
                StartAutoPlayNvSmoke();
            try
            {
                PreparePcAutoCompleteStartup();
                RestorePcNativeDoubleFeature();
            }
            catch (Exception error) { FailPcRoundLifecycle("feature-restore", error); }
            pcLifecycleRuntimeReady = !pcAutoPlayStorageFaulted;
            if (Array.IndexOf(args, "-rouletteRoundLifecycleSmoke") >= 0)
                StartRoundLifecycleSmoke();
            if (Array.IndexOf(args, "-rouletteRngReceiptSmoke") >= 0)
                StartRngReceiptSmoke();
            if (Array.IndexOf(args, "-rouletteAutoCompleteSmoke") >= 0)
                StartAutoCompleteSmoke();
            if (Array.IndexOf(args, "-rouletteGameSuspensionSmoke") >= 0)
                StartGameSuspensionSmoke();
            if (Array.IndexOf(args, "-rouletteAutoCompleteLegacySmoke") >= 0)
                StartAutoCompleteLegacySmoke();
            if (Array.IndexOf(args, "-rouletteGamePaymentSmoke") >= 0)
                StartGamePaymentSmoke();
        }

        private void PrepareImmediateDiagnosticIdleEntry()
        {
            if (!configuredAutoPlayCheckPending) return;
            // A saved active configuration could fund or commit an existing
            // wager. Such a session cannot supply this default diagnostic
            // bootstrap; reject it before touching its configuration or money.
            if (session == null || sourceGameplayState != SourceGameplayState.Idle ||
                configuredAutoPlay.AutoPlay != 0 || autoplay || spinning ||
                session.SpinPending || session.InDoubleUp || pendingSpin != null ||
                SourceRoundBlocksOrdinaryInput || SourcePlayerInputs.Count != 0)
                throw new InvalidOperationException(
                    "Immediate spin diagnostic requires an idle default configuration");
            long credit = session.Credit, rounds = session.Rounds;
            int ends = sourceRoundEndCount;
            ServiceConfiguredAutoPlay();
            if (configuredAutoPlayCheckPending || spinning || session.SpinPending ||
                sourceGameplayState != SourceGameplayState.Idle || SourceRoundBlocksOrdinaryInput ||
                session.Credit != credit || session.Rounds != rounds || sourceRoundEndCount != ends)
                throw new InvalidOperationException(
                    "Immediate spin diagnostic did not finish its initial Idle entry without a round");
            Debug.Log("ROULETTE_DIAGNOSTIC_INITIAL_IDLE: serviced=true noRound=true");
        }

        private void Update()
        {
            if (pcAutoPlayStorageFaulted) return;
            // 设置请求每帧尝试消费：只要轮盘不在运行中，就立即进入后台设置。
            if (Model.Instance.rouletteFlow &&
                RouletteFlowBridge.Instance != null &&
                RouletteFlowBridge.Instance.TryEnterSetting())
                return;
            if (!ServicePcSasHostMessagesV1()) return;
            // Native label timers are dispatched even while their sprites
            // are hidden, including History and operator overlay service.
            ServicePaidFade();
            ServicePcPaymentMessages();
            ServicePcResultCycle();
            ServicePcPaysFlash();
            if (PcGameAppSuspended)
            {
                // App::WaitUntilUnsuspended services messages and independent
                // timers while ordinary Game progression remains blocked.
                ServicePcAutoComplete();
                ServiceSourceRoundLifecycle();
                long now = System.Diagnostics.Stopwatch.GetTimestamp();
                TickPaysTimeout(now);
                TickGameInfoOptions(now);
                TickPcPlayerVolumeIdleTimeout(now);
                TickPcServiceConfirmation(now);
                TickPcCollectConfirmation(now);
                TickPcAutoConfirmation(now);
                if (showingDoubleHelp && (session == null || !session.InDoubleUp || now >= doubleHelpUntilTimestamp))
                    CloseDoubleHelp(false);
                if (promptDelayedPlayTimestamp != 0 && now >= promptDelayedPlayTimestamp)
                    promptDelayedPlayTimestamp = 0;
                ServicePcScheduledCapture();
                return;
            }
            if (Input.GetKeyDown(KeyCode.F10)) TogglePcPaytableTest();
            if (Input.GetKeyDown(KeyCode.F11)) TogglePcGameStatistics();
            if (Input.GetKeyDown(KeyCode.F12)) RequestPcVideoDiagnostics();
            if (OperatorFallbackActive)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) ClosePcOperatorFallback();
                return;
            }
            if (PaytableTestActive)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) ClosePcPaytableTest();
                RefreshPaytableTestUi();
                return;
            }
            if (rackSnapPointerNeedsRelease &&
                (RouletteTouchFeed.Bound ? !RouletteTouchFeed.AnyPointerCaptured :
                    !Input.GetMouseButton(0)) &&
                Time.frameCount > rackSnapFrame + 1)
                rackSnapPointerNeedsRelease = false;
            bool rackInputSnappedThisFrame = false;
            // 轮盘画布在副屏，裸 Input 的鼠标坐标属于主屏。绑定模式下只有副屏的
            // 按下才算作 rack 输入，避免主屏同位置点击触发副屏的赢分接管。
            Vector2 boundPress = Vector2.zero;
            bool boundRackPress = RouletteTouchFeed.Bound &&
                RouletteTouchFeed.TryGetPress(out boundPress);
            bool rackPointerDown = RouletteTouchFeed.Bound ? boundRackPress :
                Input.GetMouseButtonDown(0);
            if (!UsesSourcePlayerInput && winRackActive && !topperPreview && !histogramPreview &&
                !historyView &&
                (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.D) ||
                 (rackPointerDown &&
                  (ideckPreview || portraitRoot == null ||
                   RectTransformUtility.RectangleContainsScreenPoint(
                       portraitRoot,
                       RouletteTouchFeed.Bound ? boundPress : (Vector2)Input.mousePosition,
                       null)))))
                rackInputSnappedThisFrame = TrySnapWinRackingInput(true,
                    rackPointerDown, Input.GetKeyDown(KeyCode.Space) ?
                        (Action)HandleSourceOfferPlay : Input.GetKeyDown(KeyCode.D) ?
                        EnterSourceDoubleOffer : (Action)null);
            TickWinRacking();
            if (showingDoubleHelp && (session == null || !session.InDoubleUp ||
                System.Diagnostics.Stopwatch.GetTimestamp() >= doubleHelpUntilTimestamp))
                CloseDoubleHelp(false);
            if (!PcNativeDoubleFeatureTracked && doubleCollectPending &&
                System.Diagnostics.Stopwatch.GetTimestamp() >= doubleExitUntilTimestamp)
            {
                doubleCollectPending = false;
                try {
                    long win = session.DoubleStake;
                    session.CollectDouble();
                    RecordDoubleTakeWin(win);
                }
                catch (Exception error) { ShowFeedback(error.Message); }
            }
            FinalizePendingHistoryRound();
            if (!UsesSourcePlayerInput)
            {
                ServiceSourceRoundLifecycle();
                ServiceSourceBetStateEntry();
                ServiceConfiguredAutoPlay();
                ServicePcAutoComplete();
            }
            TickPcServiceConfirmation(System.Diagnostics.Stopwatch.GetTimestamp());
            TickPcCollectConfirmation(System.Diagnostics.Stopwatch.GetTimestamp());
            TickPcAutoConfirmation(System.Diagnostics.Stopwatch.GetTimestamp());
            if (!UsesSourcePlayerInput && Input.GetKeyDown(KeyCode.F8)) ToggleDemoMenu();
            if (Input.GetKeyDown(KeyCode.F9)) Try(() => AddPcVirtualCredits(100));
            UpdateIDeckAutomaticAttract();
            UpdateTopperAutomaticAttract();
            if (!UsesSourcePlayerInput && session != null && !session.InDoubleUp &&
                !ideckPreview && !topperPreview && !histogramPreview &&
                !historyView && !demoMenuVisible &&
                !IsDoubleReveal && !IsDoubleExit)
            {
                if (!rackInputSnappedThisFrame && Input.GetKeyDown(KeyCode.Space))
                {
                    if (spinning) RequestWheelSkillStop();
                    else if (SourcePlayInputAllowed) BeginSpin();
                }
                if (!rackInputSnappedThisFrame && Input.GetKeyDown(KeyCode.D) && !spinning &&
                    (!ResultDisplayActive || SourceDoubleOfferActive) &&
                    DoubleUpInputAllowed && session.DoubleAvailable)
                    Try(BeginDouble);
                if (!rackInputSnappedThisFrame && portraitCanvas == null &&
                    Input.GetMouseButtonDown(0))
                {
                    Vector2 pointer = PortraitPointer(Input.mousePosition);
                    if (new Rect(775, 1823, 258, 59).Contains(pointer) && SourcePlayInputAllowed) BeginSpin();
                    else if (PressSpinVisible &&
                             new Rect(752, 938, 310, 110).Contains(pointer) && SourcePlayInputAllowed) BeginSpin();
                    else if (!spinning && (!ResultDisplayActive || SourceDoubleOfferActive) &&
                             DoubleUpInputAllowed && session.DoubleAvailable &&
                             new Rect(732, 1812, 43, 82).Contains(pointer))
                        Try(BeginDouble);
                }
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                SetIDeckPreview(false);
                SetTopperPreview(false);
                SetHistogramPreview(false);
                SetHistoryView(false);
            }
            if (Input.GetKeyDown(KeyCode.F2)) SetIDeckPreview(true);
            if (Input.GetKeyDown(KeyCode.F7)) SetIDeckAttractPreview();
            if (Input.GetKeyDown(KeyCode.F3)) SetTopperPreview(true);
            if (Input.GetKeyDown(KeyCode.F6)) SetTopperAttractPreview();
            if (Input.GetKeyDown(KeyCode.F4)) SetHistoryView(!historyView);
            if (Input.GetKeyDown(KeyCode.F5)) SetHistogramPreview(!histogramPreview);
            if (historyView)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) ShowHistoryPage(historyPage + 1);
                if (Input.GetKeyDown(KeyCode.RightArrow)) ShowHistoryPage(historyPage - 1);
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (doubleHistoryView) SetDoubleHistoryView(false);
                    else SetHistoryView(false);
                }
            }
            else if (histogramPreview)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    histogramPage = Mathf.Min(histogramPage + 1,
                        Mathf.Max(0, recordedSnapshots.Count - 1));
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    histogramPage = Mathf.Max(0, histogramPage - 1);
            }
            if (paysPage >= 0 && !PcPaysReturnPending)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) && paysPage > 0)
                {
                    paysPage--;
                    ResetPaysTimeout();
                    PlayPaysClick();
                }
                if (Input.GetKeyDown(KeyCode.RightArrow) && paysPage < 3)
                {
                    paysPage++;
                    ResetPaysTimeout();
                    PlayPaysClick();
                }
                if (Input.GetKeyDown(KeyCode.Escape)) ReturnFromPays();
            }
            TickPaysTimeout(System.Diagnostics.Stopwatch.GetTimestamp());
            TickGameInfoOptions(System.Diagnostics.Stopwatch.GetTimestamp());
            TickPcPlayerVolumeIdleTimeout(System.Diagnostics.Stopwatch.GetTimestamp());
            ServicePcScheduledCapture();
        }

        private void ServicePcScheduledCapture()
        {
            if (captureTaken || string.IsNullOrEmpty(capturePath) || Time.realtimeSinceStartup < captureAt) return;
            captureTaken = true;
            ScreenCapture.CaptureScreenshot(capturePath);
            Invoke("QuitCapture", 2f);
        }

        private void LateUpdate()
        {
            if (pcAutoPlayStorageFaulted) return;
            if (OperatorFallbackActive) return;
            if (PaytableTestActive) { RefreshPaytableTestUi(); return; }
            // SpinVisual advances in Unity's coroutine phase after Update.
            // Apply its latest wheel and ball positions before this frame renders.
            RefreshPortraitUi();
            RefreshPcSuspendedInputPresentation();
        }

        private void QuitCapture() { Application.Quit(); }

        private IEnumerator CaptureWheelSequence(string directory)
        {
            System.IO.Directory.CreateDirectory(directory);
            float[] seconds = { .75f, 2f, 4f, 5f, 6f, 7f, 8f, 9f, 10f, 11f, 12f };
            for (int i = 0; i < seconds.Length; ++i)
            {
                while (Time.realtimeSinceStartup - spinStartedAt < seconds[i])
                    yield return null;
                // Render the current uGUI state after the wheel coroutine and
                // LateUpdate. ScreenCaptureAsTexture returns null in batchmode.
                yield return new WaitForEndOfFrame();
                float frameElapsed = Time.realtimeSinceStartup - spinStartedAt;
                string path = System.IO.Path.Combine(directory,
                    "frame_" + i.ToString("D2") + ".png");
                try { CaptureActiveScreenOffscreen(path); }
                catch (Exception error)
                {
                    Debug.LogError("ROULETTE_FRAME_CAPTURE_FAILED: " + error);
                    Application.Quit();
                    yield break;
                }
                Debug.Log("ROULETTE_FRAME_CAPTURE: index=" + i +
                    " requested=" + seconds[i].ToString("F2",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    " elapsed=" + frameElapsed.ToString("F3",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    " captureMs=" + ((Time.realtimeSinceStartup - spinStartedAt -
                        frameElapsed) * 1000f).ToString("F1",
                        System.Globalization.CultureInfo.InvariantCulture) +
                    " unityFrame=" + Time.frameCount +
                    " stage=" + wheelStage + " phase=" + wheelRenderedAngle +
                    " ball=" + wheelBallPosition + " visible=" + wheelBallVisible);
            }
            yield return new WaitForSecondsRealtime(2f);
            Application.Quit();
        }

        private static Vector2 PortraitPointer(Vector3 screenPosition)
        {
            float scale = Mathf.Min(Screen.width / PortraitWidth, Screen.height / PortraitHeight);
            float offsetX = (Screen.width - PortraitWidth * scale) * .5f;
            float offsetY = (Screen.height - PortraitHeight * scale) * .5f;
            return new Vector2((screenPosition.x - offsetX) / scale,
                (Screen.height - screenPosition.y - offsetY) / scale);
        }

        private void OnDestroy()
        {
            // 屏幕绑定是静态状态，实例销毁时必须复位，否则下一局会继续把事件
            // 投递给已销毁的输入模块。
            RouletteTouchFeed.Detach(sourceInputModule);
            // Object destruction is not CancelAutoComplete: retain native
            // saved seconds and process counters for the next game instance.
            pcAutoCompleteIpcTimer.Delete();
            pcAutoCompleteNextTick = 0;
            var disposable = outcome as IDisposable;
            if (disposable != null) disposable.Dispose();
        }

        private void LoadProfile(bool restoreStagedBets = true)
        {
            if (pcAutoPlayStorageFaulted) return;
            restoredExplicitSourceIdleState = false;
            CancelPcPlayerVolumeIdleTimeout();
            sourceGameplayState = SourceGameplayState.Idle;
            sourceBetStateEntryPending = false;
            sourceTableInputEnabled = true;
            currentGameResultsCleared = false;
            currentTableResultMarkerCleared = false;
            sourceBetsChangedSincePlay = false;
            if(sasStartupArchiveRaw!=null){RestoreSasStartupProfileV4();return;}
            try
            {
                LoadPendingSpinJournal();
                SelectPcReceiptProfile();
                if (pendingSpin != null) profileIndex = pendingSpin.profileIndex;
                LoadStagedBetJournal();
                if (restoreStagedBets && pendingSpin == null && stagedBets != null &&
                    !pcReceiptOverridesStagedProfile)
                    profileIndex = stagedBets.profileIndex;
                SelectProfileFromPcCreditEvent();
                // The source feature has a separately configured suit branch.
                // The filmed cabinet remains red/black; expose the branch only
                // for explicit PC verification until its operator value is known.
                bool suitMode = Array.IndexOf(Environment.GetCommandLineArgs(),
                    "-rouletteSuitMode") >= 0;
                string[] args = Environment.GetCommandLineArgs();
                int baseLimitIndex = Array.IndexOf(args, "-rouletteBaseWinLimitCredits");
                long baseWinLimit = 0;
                if (baseLimitIndex >= 0 && (baseLimitIndex + 1 >= args.Length ||
                    !long.TryParse(args[baseLimitIndex + 1], out baseWinLimit) ||
                    baseWinLimit < 0))
                    throw new ArgumentException("Invalid PC base win limit in credits");
                session = new RouletteSession(RouletteProfile.Load(profiles[profileIndex]),
                    1000, outcome, doubleSuitMode: suitMode,
                    baseMaximumWin: baseWinLimit);
                Debug.Log("ROULETTE_PROFILE_ACTIVE: " + profiles[profileIndex]);
                if (baseWinLimit != 0)
                    Debug.Log("ROULETTE_PC_BASE_WIN_LIMIT: " + baseWinLimit);
                session.DoubleEnabled = true;
                if (!temporaryHistory) ((RouletteSession)session).EnableDeferredBaseSettlement();
                bool archivedRoundRestored = TryRestoreArchivedRound();
                if (HasTrustedLatestLifecycle && !archivedRoundRestored)
                    throw new PcRoundLifecycleDataException("Trusted lifecycle financial round did not restore");
                bool settledTableUndoRestored = archivedRoundRestored && RestoreSettledTableUndo();
                RestorePcCreditEvent();
                ReconcilePcRngReceipt();
                resumePendingSpin = TryRestorePendingSpin(archivedRoundRestored, settledTableUndoRestored);
                if (resumePendingSpin) AcknowledgePcSpinDraw();
                ValidatePendingSasIntentAtStartupV4();
                // Native +919d starts at one. The PC pending journal is
                // written only after committing the normal StatePlay draw,
                // whose substate 0 clears it; settled/fresh startup retains 1.
                gameOverMessageLatched = !resumePendingSpin;
                if (resumePendingSpin) sourceGameplayState = SourceGameplayState.Playing;
                if (restoreStagedBets && !resumePendingSpin && pendingSpin == null && LifecycleAllowsStagedTable)
                    TryRestoreStagedBets();
                if (archivedRoundRestored && !resumePendingSpin && session.ResultPhase &&
                    session.LastResult != null && session.LastResult.Returned > 0)
                {
                    // This archive has already paid its result. Resource loading
                    // starts an Intro for a new session; replaying that movie
                    // here would cover the restored paid meter indefinitely.
                    // Preserve the completed payout presentation. A new bet or
                    // spin restarts the ordinary Intro through its existing path.
                    footerMoviePhase = FooterMoviePhase.Hidden;
                    footerMovieClock = null;
                    Debug.Log("ROULETTE_RESTORED_PAYOUT_MOVIE_HIDDEN: paid=" +
                        session.LastResult.Returned);
                }
                if (LifecycleAllowsStagedTable)
                {
                    RestoreClearedGameResults();
                    RestoreEditedTableResultPresentation();
                }
                RestoreSourceRoundLifecycle();
                RestoreSourceTableInput();
                if (HasTrustedLatestLifecycle && !resumePendingSpin &&
                    latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Running)
                    sourceTableInputEnabled = latestRoundLifecycle.idleEntry.tableInputEnabled;
                if (pcCreditOverride != null &&
                    pcCreditOverride.version == 2 &&
                    pcCreditOverride.kind == "denom" && session.TotalBet != 0)
                    throw new InvalidOperationException(
                        "PC denomination journal lacks an empty staged table");
                // gamemgr posts GameReady when the application becomes ready,
                // including a restored zero-credit session. It is not a
                // per-round event after every losing result.
                if (session.Credit == 0 && !session.SpinPending &&
                    !session.InDoubleUp && !resumePendingSpin &&
                    pcPlayerVolumeIdleDeadlineTimestamp == 0)
                    StartPcPlayerVolumeIdleTimeout();
                status = session.Profile.Id + " / " + session.Profile.PaytableId +
                         " / 157 original bets loaded";
                showAllBets = false;
                showStats = false;
                autoplay = false;
                autoConfirmPending = false;
                ClosePays();
            }
            catch (Exception error)
            {
                if (HasTrustedLatestLifecycle || error is PcRoundLifecycleDataException ||
                    (pcOutcome != null && pcOutcome.Receipt != null))
                {
                    FailPcRoundLifecycle("restored", error);
                    return;
                }
                session = null;
                status = "Configuration error: " + error.Message;
                Debug.LogException(error);
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null) audioSource.PlayOneShot(clip, 0.55f);
        }

        private void PlaySpinAnnouncement()
        {
            // Roulette::StateSpin calls GameSounds::StopAll, then plays sound
            // 0x175 at RouletteVoiceOverLevelSnd (70 in the packaged IMG).
            audioSource.Stop();
            wheelAudioSource.Stop();
            resultVoiceSource.Stop();
            if (rackAudioSource != null) rackAudioSource.Stop();
            if (noMoreBets != null) audioSource.PlayOneShot(noMoreBets, .7f);
        }

        private void PlayBetSound(AudioClip clip)
        {
            if (clip == null) return;
            audioSource.PlayOneShot(clip, 1f);
            Debug.Log("ROULETTE_BET_SOUND: " + clip.name);
        }

        private void PlayResultVoice(RoundResult result)
        {
            if (result == null || result.Pocket < 0 || result.Pocket > 36) return;
            bool won = result.Returned > 0;
            AudioClip clip = won ? resultWinVoice[result.Pocket] :
                resultNumberVoice[result.Pocket];
            if (clip == null) return;
            resultVoiceSource.PlayOneShot(clip, .7f);
            Debug.Log("ROULETTE_RESULT_VOICE: pocket=" + result.Pocket +
                      " win=" + won + " clip=" + clip.name);
        }

        private bool Try(Action action, AudioClip actionSound = null,
            AudioClip failureSound = null, bool restartPrompt = true)
        {
            if (pcAutoPlayStorageFaulted) return false;
            try
            {
                action();
                if (pcAutoPlayStorageFaulted) return false;
                SaveStagedBetsIfChanged();
                if (restartPrompt) ResetBetPromptAfterSuccess();
                if (actionSound != null) PlayBetSound(actionSound);
                else Play(click);
                return true;
            }
            catch (BetRejectedException error)
            {
                if (failureSound != null) PlayBetSound(failureSound);
                QueueBetFailurePrompt(error.Reason);
                return false;
            }
            catch (Exception error)
            {
                if (pcAutoPlayStorageFaulted) return false;
                if (failureSound != null) PlayBetSound(failureSound);
                ShowFeedback(error.Message);
                return false;
            }
        }

        private void ShowFeedback(string message)
        {
            status = message;
            feedbackUntil = Time.realtimeSinceStartup + 4f;
        }

        private void UndoBet()
        {
            if (!session.CanUndo) throw new InvalidOperationException("Nothing to undo");
            if (session.ResultPhase) session.NextRound();
            session.Undo();
            int priorSpot = session.LastUndoSourceSpot;
            if (priorSpot >= 0 && priorSpot < 37)
            {
                selectedNeighbor = priorSpot;
                neighborEnabledWithoutPrecheck = true;
                PlayerPrefs.SetInt(NeighborSelectionKey, priorSpot);
                PlayerPrefs.Save();
                if (uiNeighborButton != null) RefreshNeighborUi();
            }
            else ClearNeighborSelection();
            for (int i = 0; i < session.LastUndoRemovalSoundCount; ++i)
                PlayBetSound(betRemovedSound);
        }

        private void Bet(int id)
        {
            if (PaytableTestActive || OperatorFallbackActive || session == null || spinning || pcCollectPending ||
                (Time.frameCount == rackSnapFrame ||
                 rackSnapPointerNeedsRelease) ||
                ResultDisplayActive || paysPage >= 0 ||
                IsDoubleExit) return;
            long before = session.TotalBet;
            TrySourceTableEvent(() => session.PlaceBet(id, session.Chip), betPlacedSound,
                betLimitSound, requestsBetState: true);
            if (session.TotalBet > before)
            {
                RestartSourcePrompt(PromptKind.PressSpin);
                pressSpinAt = Time.realtimeSinceStartup;
                if (footerMoviePhase == FooterMoviePhase.Hidden ||
                    footerMoviePhase == FooterMoviePhase.Outro) StartFooterMovie();
            }
        }

        private void Number(int number)
        {
            if (session == null || spinning || paysPage >= 0) return;
            Bet(100 + number);
        }

        private void BeginSpin()
        {
            BeginSpin(false);
        }

        private void BeginSpin(bool autoPlayRequest)
        {
            if (SourceDoubleOfferActive)
            {
                if (!autoPlayRequest) HandleSourceOfferPlay();
                return;
            }
            if (PrepareSourceSpin(autoPlayRequest)) CommitSourceSpin();
        }

        private bool PrepareSourceSpin(bool autoPlayRequest)
        {
            if (PcGameOrdinaryInputBlocked) return false;
            // AutoPlay invokes this through AutoLoop. Player input reaches
            // the same method, so enforce the disabled PLAY state here too.
            if (PaytableTestActive || OperatorFallbackActive || (autoplay && !autoPlayRequest) || pcCollectPending ||
                SourceRoundBlocksOrdinaryInput ||
                ResultDisplayActive || IsDoubleExit || configuredAutoPlayCheckPending ||
                demoMenuVisible || Time.frameCount == rackSnapFrame ||
                rackSnapPointerNeedsRelease) return false;
            Debug.Log("ROULETTE_SPIN_REQUEST: bet=" + (session == null ? 0 : session.TotalBet));
            if (session == null || outcome == null || spinning || session.SpinPending ||
                resumePendingSpin || session.InDoubleUp || paysPage >= 0) return false;
            ValidateCanBeginSuccessorRound();
            CloseGameInfoOptions();
            // Roulette::DoPlay disables the table before its StartGame call.
            // Ordinary StartGame disables Demo and clears the live result
            // before checking current funds or the presence of a wager.
            sourceTableInputEnabled = false;
            ClearNeighborSelection();
            DisableSourceDemoMode();
            ApplyClearedGameResultsPresentation(true);
            SaveClearedGameResults();
            if (session.Bets.Count == 0)
            {
                ShowFeedback("Place a bet first.");
                return false;
            }
            if (session.TotalBet > session.Credit ||
                session.TotalBet > session.Profile.MaximumTotalBet)
            {
                ShowFeedback("Insufficient credit for current bets.");
                return false;
            }
            sourceGameplayState = SourceGameplayState.Playing;
            sourceBetStateEntryPending = false;
            SavePcPaymentPresentationAdmittedStart();
            return true;
        }

        private void CommitSourceSpin()
        {
            try
            {
                ValidateCanBeginSuccessorRound();
                PersistIdleDrawInFlight();
                // Native RunLoop state 5 clears MenuOnSpriteLabel before
                // StatePlay's first purge/draw. StartGame rejection never
                // reaches this boundary; this does not reset index or arm.
                ClearSourceSelectedDemoText();
                PurgeSourcePlayerInputs();
                configuredAutoPlayCheckPending = false;
                configuredAutoPlayFinishPending = false;
                configuredResultDelayDeadlineTimestamp = 0;
                configuredResultDelayStartTimestamp = 0;
                pcResultDisplayWait = null;
                pcResultDisplayWaitPromptScheduled = false;
                pcResultDisplayWaitFrame = -1;
                if (session.ResultPhase)
                    session.NextRound();
                // A valid table touch may already have advanced NextRound
                // this frame. Commit the prior completed result before the
                // next stake debit and pending-spin journal are written.
                FinalizePendingHistoryRound();
                gameOverMessageLatched = false;
                sourceBetsChangedSincePlay = false;
                PreparePcSpinDraw();
                int pocket = session.BeginSpin();
                gameStartTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
                proViewFlashStartedTimestamp = 0;
                try { SavePendingSpin(); AcknowledgePcSpinDraw(); CompleteIdlePendingHandoff(); }
                catch
                {
                    session.CancelSpin();
                    throw;
                }
                try { ClearStagedBets(); }
                catch (Exception error)
                {
                    // The committed spin journal takes priority on restart;
                    // an old table snapshot is rejected by its round baseline.
                    Debug.LogWarning("Could not discard staged roulette table: " + error.Message);
                }
                ResetBetPromptAfterSuccess();
                Debug.Log("ROULETTE_SPIN_ACCEPTED: pocket=" + pocket +
                          " wager=" + session.TotalBet);
                StartFooterMovie();
                status = "Spinning...";
                ArmSourceRoundAfterCommit();
                // 必须在本局已经进入 Wheel 阶段之后发布，否则宿主看到的仍是
                // 尚未收尾的上一个阶段。
                RaiseFlowSpinCommitted();
                StartCoroutine(SpinVisual(pocket));
            }
            catch (Exception error)
            {
                if (pcAutoPlayStorageFaulted) return;
                if (HasTrustedLatestLifecycle || pcPreparedSpinDraw != null || PcReceiptEnabled)
                {
                    FailPcRoundLifecycle("committed", error);
                    return;
                }
                Debug.LogWarning("ROULETTE_SPIN_REJECTED: " + error);
                ShowFeedback(error.Message);
            }
        }

        private IEnumerator SpinVisual(int pocket)
        {
            if (gameStartTimestamp == 0 && !pcLifecycleColdRound)
                gameStartTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            spinning = true;
            if (!BeginWheelPlayback(pocket))
            {
                if (!temporaryHistory && pendingSpin != null)
                {
                    FailPcRoundLifecycle("displayed", new InvalidOperationException(
                        "Original wheel paths could not display the committed pending spin"));
                    yield break;
                }
                session.CancelSpin();
                ClearPendingSpin();
                spinning = false;
                sourceRoundPhase = SourceRoundPhase.None;
                ShowFeedback("Original wheel paths could not be loaded.");
                yield break;
            }
            // The original SetStop writes its predicted stop phase into
            // WheelData before StateSpin saves roulette NV. Restore uses that
            // phase as the start angle when a pending wheel is replayed.
            try { SavePendingWheelStopIndex(wheelFinalStopIndex); }
            catch (Exception error)
            {
                // The already committed pocket and wager remain recoverable
                // from the earlier pending-spin record.
                Debug.LogWarning("Could not persist pending wheel phase: " + error.Message);
            }
            // RouletteWheel::Play arms the 42 ms timer after preparing the
            // paths. Anchor both clocks after the equivalent setup here.
            spinStartedAt = Time.realtimeSinceStartup;
            pcWheelTimerEpoch = System.Diagnostics.Stopwatch.GetTimestamp();
            pcWheelTimerConsumed = pcWheelDeliveredFrames = 0;
            pcWheelTimerPaused = false;
            ++pcWheelTimerGeneration;
            int diagnosticStopTick = -1;
            string[] diagnosticArgs = Environment.GetCommandLineArgs();
            int diagnosticStopOption = Array.IndexOf(diagnosticArgs,
                "-rouletteSkillStopAfterTicks");
            if (Array.IndexOf(diagnosticArgs, "-rouletteCapturePocket") >= 0 &&
                diagnosticStopOption >= 0 &&
                diagnosticStopOption + 1 < diagnosticArgs.Length)
            {
                int selectedTick;
                if (int.TryParse(diagnosticArgs[diagnosticStopOption + 1], out selectedTick) &&
                    selectedTick > 0 && selectedTick < 10000)
                    diagnosticStopTick = selectedTick;
            }
            // Roulette::StateSpin starts the wheel before StopAll and
            // NoMoreBetsSnd. Both fresh and resumed spins enter here.
            PlaySpinAnnouncement();
            if (TurboAutoPlayActive) SnapWheelForTurboAutoPlay();
            bool finished = false;
            while (!finished)
            {
                if (PcGameAppSuspended || pcWheelTimerPaused) { yield return null; continue; }
                int targetFrames = OriginalWheelClock.DueFrames(
                    pcWheelTimerEpoch, System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency);
                // ipc.ko retains one pending timer count per missed period;
                // sys_ipc_receivex consumes one count per receive. Preserve
                // those logical wheel steps after a delayed Unity frame.
                while (pcWheelTimerConsumed < targetFrames && !finished && !PcGameAppSuspended)
                {
                    finished = StepWheelPlayback();
                    ++pcWheelTimerConsumed;
                    ++pcWheelDeliveredFrames;
                    if (!finished && pcWheelDeliveredFrames == diagnosticStopTick)
                    {
                        Debug.Log("ROULETTE_DIAGNOSTIC_SKILL_STOP: tick=" +
                            pcWheelDeliveredFrames + " stage=" + wheelStage +
                            " pathFrame=" + wheelPathFrame +
                            " displayAngle=" + wheelRenderedAngle +
                            " ball=" + wheelBallPosition);
                        RequestWheelSkillStop();
                    }
                }
                yield return null;
            }
            while (PcGameAppSuspended) yield return null;
            bool sasRound = PcPendingUsesV4;
            RoundResult result;
            try { result = sasRound ? CompleteSasPendingIntentV4() : session.CompleteSpin(); }
            catch (Exception error) { FailPcRoundLifecycle("settled", error); yield break; }
            LifecycleCheckpoint("CompleteSpin");
            // The new live result owns its feature decision immediately.
            // Keep the old persisted view marker until RecordPocket saves
            // the new archive identity, then remove that obsolete marker.
            currentGameResultsCleared = false;
            currentTableResultMarkerCleared = false;
            resultCycleStartedAt = result.Returned > 0 && !TurboAutoPlayActive ?
                Time.realtimeSinceStartup : -1f;
            resultCycleStartedTimestamp = result.Returned > 0 &&
                !TurboAutoPlayActive ? System.Diagnostics.Stopwatch.GetTimestamp() : 0;
            try
            {
                if (!sasRound) RecordPocket(result.Pocket, result, trackLifecycle: !temporaryHistory);
                BeginSourceBaseResult(result);
                ResetClearedGameResultsForNewRound();
                if (!sasRound) ReleaseSettledPendingSpin();
            }
            catch (Exception error)
            {
                FailPcRoundLifecycle("settled", error);
                yield break;
            }
            PlayResultVoice(result);
            Debug.Log("ROULETTE_SPIN_SETTLED: pocket=" + result.Pocket +
                      " wager=" + result.Wager + " returned=" + result.Returned);
            spinning = false;
            gameOverAt = result.Returned == 0 ? Time.realtimeSinceStartup : 0f;
            pressSpinAt = Time.realtimeSinceStartup +
                (result.Returned == 0 ? 2.5f : 6f);
            if (result.Returned > 0)
            {
                if (TurboAutoPlayActive) CompleteTurboWinWithoutRack(result.Returned);
                else StartWinRacking(result.Returned);
            }
            else
            {
                BeginSourceResultDisplay(result.Returned, false);
            }
            ServiceSourceRoundLifecycle();
            status = "Pocket " + result.Pocket + " / bet " + result.Wager +
                     " / returned " + result.Returned + " / category " + result.Category;
        }

        private IEnumerator AutoLoop()
        {
            if (Model.Instance.rouletteFlow) { autoRoutine = null; yield break; }
            // The cabinet receives the AutoPlay toggle in an input callback,
            // then checks funds when RunLoop dispatches state 4.
            yield return null;
            while (autoplay)
            {
                if (!PcGameAppSuspended && session != null && !spinning && !session.InDoubleUp &&
                    !SourceRoundBlocksOrdinaryInput &&
                    paysPage < 0 && !demoMenuVisible && !ResultDisplayActive &&
                    !configuredAutoPlayCheckPending)
                {
                    DisableSourceDemoMode(); // RunLoop state 4, before CheckFund
                    // Game::RunLoop state 4 clears AutoPlay when CheckFund
                    // rejects a zero or unaffordable staged wager.
                    if (session.Bets.Count == 0 || session.TotalBet == 0 ||
                        session.TotalBet > session.Credit ||
                        session.TotalBet > session.Profile.MaximumTotalBet)
                    {
                        if (!PersistExplicitLifecycleIdleEntry()) yield break;
                        autoplay = false;
                        autoConfirmPending = false;
                        configuredAutoPlayCheckPending = true;
                        ScheduleIdlePromptAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                            System.Diagnostics.Stopwatch.Frequency);
                        Debug.Log("ROULETTE_AUTOPLAY_FUNDS_STOP: bet=" +
                                  session.TotalBet + " credit=" + session.Credit);
                    }
                    else BeginSpin(true);
                }
                // StateDisplayResults completes its own AutoPlay waits and
                // then returns to game-state dispatch. Do not add an
                // unrelated 50 ms poll interval before the next spin.
                yield return null;
            }
            autoRoutine = null;
        }

        private IEnumerator AutoFundStopSmoke()
        {
            yield return null;
            var profile = session.Profile;
            session = new RouletteSession(profile, 1000,
                new FixedPocketSmokeOutcome());
            ToggleAuto();
            yield return null;
            yield return null;
            if (autoplay || autoRoutine != null || !TableTouchInputEnabled())
                throw new InvalidOperationException(
                    "AutoPlay stayed active with no staged wager");

            session = new RouletteSession(profile, 1,
                new FixedPocketSmokeOutcome(0));
            session.PlaceBet(101, 1);
            RoundResult loss = session.Spin();
            session.NextRound();
            if (loss.Returned != 0 || session.Credit != 0 ||
                session.TotalBet != 1)
                throw new InvalidOperationException(
                    "AutoPlay insufficient-credit fixture differs");
            ToggleAuto();
            yield return null;
            yield return null;
            if (autoplay || autoRoutine != null || TableTouchInputEnabled() ||
                session.Credit != 0 || session.TotalBet != 1)
                throw new InvalidOperationException(
                    "AutoPlay stayed active after the retained wager exhausted credit");
            Debug.Log("ROULETTE_AUTOPLAY_FUND_STOP_PASSED: empty bet and zero credit");
            Application.Quit();
        }

        private IEnumerator AutoManualPlayGateSmoke()
        {
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(16));
            session.PlaceBet(100, 1);
            ToggleAuto();
            // AutoLoop has yielded to the next frame. A player PLAY request
            // in the same frame must not debit ahead of its scheduled turn.
            BeginSpin();
            if (!autoplay || spinning || session.SpinPending ||
                session.Credit != 1000 || session.TotalBet != 1)
                throw new InvalidOperationException(
                    "Manual PLAY committed while AutoPlay owned the round");
            yield return null;
            if (!autoplay || !spinning || !session.SpinPending ||
                session.Credit != 999)
                throw new InvalidOperationException(
                    "Scheduled AutoPlay spin did not commit exactly once");
            BeginSpin();
            if (session.Credit != 999)
                throw new InvalidOperationException(
                    "Second manual PLAY debited during AutoPlay spin");
            ToggleAuto();
            Debug.Log("ROULETTE_AUTOPLAY_MANUAL_PLAY_GATE_PASSED: " +
                      "manualIgnored=true scheduledDebit=1");
            Application.Quit();
        }

        private IEnumerator AutoFundAfterSpinSmoke()
        {
            yield return new WaitUntil(() =>
                session != null && session.Rounds == 1 && !spinning);
            if (!autoplay || session.Credit != 0 || session.TotalBet != 1 ||
                !ResultDisplayActive)
                throw new InvalidOperationException(
                    "Last-credit AutoPlay loss did not enter result wait");
            yield return new WaitUntil(() => !autoplay);
            RefreshPortraitUi();
            if (session.Rounds != 1 || spinning || session.Credit != 0 ||
                session.TotalBet != 1 || TableTouchInputEnabled() ||
                ProViewFlashingMessage() != "")
                throw new InvalidOperationException(
                    "Last-credit AutoPlay loss retriggered a round or its manual result message");
            Debug.Log("ROULETTE_AUTOPLAY_LAST_CREDIT_STOP_PASSED: " +
                      "pocket=16 credit=0 rounds=1");
            Application.Quit();
        }

        private IEnumerator AutoDuringSpinSmoke()
        {
            yield return null;
            if (!spinning || autoplay)
                throw new InvalidOperationException(
                    "Mid-spin AutoPlay rejection fixture did not start manually");
            if (!ideckSecondDisplay) SetIDeckPreview(true);
            RequestAutoPlay();
            if (!autoConfirmPending)
                throw new InvalidOperationException(
                    "iDeck AutoPlay confirmation was not presented");
            RefreshDiagnosticUi();
            yield return null;
            ClickThroughUiRaycast(ideckAutoYesButton);
            if (autoplay || autoConfirmPending)
                throw new InvalidOperationException(
                    "AutoPlay was enabled during original StatePlay");
            while (spinning) yield return null;
            if (session.Rounds != 1 || session.LastResult == null ||
                session.LastResult.Pocket != 16 || autoplay)
                throw new InvalidOperationException(
                    "Mid-spin AutoPlay request changed manual settlement");
            Debug.Log("ROULETTE_AUTOPLAY_MID_SPIN_REJECT_PASSED: pocket=16 rounds=1");
            Application.Quit();
        }

        private sealed class FixedPocketSmokeOutcome : IRouletteOutcomeSource
        {
            private readonly int pocket;
            public FixedPocketSmokeOutcome(int pocket = 0) { this.pocket = pocket; }
            public int DrawPocket() { return pocket; }
            public int DrawDoubleSuit() { return 3; }
        }

        private void CreditGuardSmoke()
        {
            session = new RouletteSession(session.Profile, 37,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            for (int pocket = 0; pocket <= 36; ++pocket)
                session.PlaceBet(100 + pocket, 1);
            RoundResult won = session.Spin();
            if (won.Returned != 36 || session.Credit != 36 ||
                !session.ResultPhase || !session.DoubleAvailable)
                throw new InvalidOperationException("Credit guard smoke setup failed");
            BeginSpin();
            if (spinning || session.SpinPending || !session.ResultPhase ||
                !session.DoubleAvailable || session.Credit != 36)
                throw new InvalidOperationException(
                    "Insufficient-credit spin discarded the winning round");
            Debug.Log("ROULETTE_CREDIT_GUARD_PASSED: bet=37 credit=36 double preserved");
        }

        private IEnumerator SkillStopSmoke()
        {
            yield return new WaitUntil(() => spinning && wheelStage == WheelStage.Loop);
            float requestedAt = Time.realtimeSinceStartup;
            RequestWheelSkillStop();
            if (auNationalStandardsEnabled)
            {
                if (wheelSkillStopRequested || wheelStage != WheelStage.Loop ||
                    !wheelAudioSource.isPlaying)
                    throw new InvalidOperationException(
                        "Australian standards allowed a disabled wheel skill stop");
                yield return new WaitUntil(() => !spinning);
                if (session.LastResult == null || session.LastResult.Pocket != 0 ||
                    session.LastResult.Returned != 36 || wheelStage != WheelStage.Done)
                    throw new InvalidOperationException(
                        "Australian standards spin failed to settle normally");
                Debug.Log("ROULETTE_AU_SKILL_STOP_DISABLED_PASSED: pocket=0 elapsed=" +
                    (Time.realtimeSinceStartup - requestedAt).ToString("F3",
                        System.Globalization.CultureInfo.InvariantCulture));
                yield break;
            }
            if (!wheelSkillStopRequested || wheelStage != WheelStage.EaseOut ||
                wheelAudioSource.isPlaying)
                throw new InvalidOperationException("Physical START did not trigger wheel skill stop");
            yield return new WaitUntil(() => !spinning);
            float elapsed = Time.realtimeSinceStartup - requestedAt;
            if (session.LastResult == null || session.LastResult.Pocket != 0 ||
                session.LastResult.Returned != 36 || wheelStage != WheelStage.Done ||
                elapsed > 2f)
                throw new InvalidOperationException("Skill stop did not settle the selected outcome");
            Debug.Log("ROULETTE_SKILL_STOP_PASSED: pocket=0 elapsed=" +
                elapsed.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
        }

        private bool ResultDisplayActive
        {
            get { return ResultDisplayActiveAt(System.Diagnostics.Stopwatch.GetTimestamp()); }
        }

        private bool ResultDisplayActiveAt(long nowTimestamp)
        {
            // Observation does not service timers or dispatch player input.
            return winRackActive || pcResultDisplayWaitServicing ||
                (pcResultDisplayWait != null && !pcResultDisplayWait.Completed);
        }

        private string ProViewFlashingMessage()
        {
            if (pcPaymentMessagesActive && pcPaymentFlashingLabel != null && !spinning)
                return pcPaymentFlashingLabel.Visible ? pcPaymentFlashingLabel.Text : "";
            if (sourceEndingDelayPromptDeadline != 0)
                return sourceEndingDelayPromptMessage;
            if (SourceDoubleOfferActive)
                return SourceOfferFlashingMessage(System.Diagnostics.Stopwatch.GetTimestamp());
            if (SourceRoundBlocksOrdinaryInput && !gameOverMessageLatched && !spinning)
                return "";
            if (spinning)
                return autoplay ? "Auto Play Mode" :
                    auNationalStandardsEnabled ? "" : "Good Luck!";
            // StateDisplayResults selects play-credit state 1 immediately
            // after the manual result/rack. Its 20/30 35 ms service waits
            // run only while the source AutoPlay byte is set. State 1
            // flashes for 2500 ms on, 250 ms off.
            if (autoplay || session == null || session.LastResult == null ||
                spinning || winRackActive || proViewFlashStartedTimestamp == 0 ||
                session.InDoubleUp || paysPage >= 0)
                return "";
            long elapsedMs = OriginalRackClock.ElapsedMillisecondsLong(
                proViewFlashStartedTimestamp,
                System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
            return elapsedMs % 2750L < 2500L ? "Game Over" : "";
        }

        private IEnumerator AutoLossGateSmoke()
        {
            yield return new WaitUntil(() => session != null &&
                session.Rounds == 1 && !spinning);
            if (session.LastResult == null || session.LastResult.Returned != 0 ||
                !ResultDisplayActive || resultDisplayDeadlineTimestamp == 0)
                throw new InvalidOperationException(
                    "AutoPlay losing result did not enter its source wait");
            OriginalResultDisplayWaitClock firstWait = pcResultDisplayWait;
            long firstStarted = resultSettledTimestamp;
            if (firstWait == null || firstWait.InitialWaits != 20)
                throw new InvalidOperationException("Auto loss did not enter twenty source waits");
            ObserveDeferredPaymentSmokeUnpaid(999, 0, 1, true);
            float timeout = Time.realtimeSinceStartup + 5f;
            while (!spinning && Time.realtimeSinceStartup < timeout)
            {
                RequireDeferredPaymentSmokeStillWaiting();
                yield return null;
            }
            if (!spinning || !firstWait.Completed || firstWait.TimeoutCount != 20 || firstWait.InputCount != 0 ||
                firstWait.CompletedAt < OriginalRackClock.AfterMilliseconds(firstStarted, 700,
                    System.Diagnostics.Stopwatch.Frequency))
                throw new InvalidOperationException("Auto loss did not complete twenty fresh no-input waits");
            CompleteDeferredPaymentSmoke(999, 0, 1, true);
            ToggleAuto();
            Debug.Log("ROULETTE_AUTO_LOSS_GATE_PASSED: requestedMs=700");
            Application.Quit();
        }

        private bool TurboAutoPlayActive
        {
            get { return configuredAutoPlay.TurboAutoPlay; }
        }

        private IEnumerator TurboDemoSmoke()
        {
            yield return new WaitUntil(() => spinning);
            if (!autoplay || !configuredAutoPlay.TurboAutoPlay || AudioListener.volume != 0f ||
                !wheelTurboSnapRequested ||
                wheelStage != WheelStage.EaseOut)
                throw new InvalidOperationException("Turbo initial state differs: auto=" + autoplay +
                    " turbo=" + configuredAutoPlay.Turbo + " volume=" + AudioListener.volume +
                    " snapped=" + wheelTurboSnapRequested + " stage=" + wheelStage);
            yield return new WaitUntil(() => !spinning);
            if (session.LastResult == null || session.LastResult.Pocket != 0 ||
                session.LastResult.Returned != 36 || session.Credit != 1035 ||
                winRackActive || wheelStage != WheelStage.Done ||
                winnerUntil > Time.realtimeSinceStartup || !ResultDisplayActive ||
                footerMoviePhase != FooterMoviePhase.Outro)
                throw new InvalidOperationException("Turbo payout or result state differs");
            ToggleAuto();
            Debug.Log("ROULETTE_TURBO_DEMO_PASSED: pocket=0 returned=36 " +
                      "rackSkipped=true cycleResultSkipped=true");
            Application.Quit();
        }

        private IEnumerator DemoFixedStopSmoke()
        {
            yield return new WaitUntil(() => spinning);
            yield return new WaitUntil(() => !spinning);
            if (session.LastResult == null || session.LastResult.Pocket != 27 ||
                session.LastResult.Returned != 36 || session.Credit != 1035 ||
                demoOutcome == null || demoOutcome.SelectedPocket != -1)
                throw new InvalidOperationException(
                    "Original one-shot demo stop did not settle correctly");
            Debug.Log("ROULETTE_DEMO_FIXED_STOP_PASSED: pocket=27 " +
                      "returned=36 credit=1035 cleared=true");
            Application.Quit();
        }

        private void ToggleAuto()
        {
            if (PcGameOrdinaryInputBlocked) return;
            // 轮盘流程模式下由宿主 FSM 推进，轮盘自带 AutoPlay 会绕过 FSM。
            if (Model.Instance.rouletteFlow) return;
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            bool leavingAutoResultWait = autoplay && !spinning &&
                !winRackActive && session != null &&
                session.LastResult != null &&
                pcResultDisplayWait != null && !pcResultDisplayWait.Completed;
            autoplay = !autoplay;
            if (!autoplay && leavingAutoResultWait)
            {
                if (proViewFlashStartedTimestamp == 0)
                    proViewFlashStartedTimestamp = now;
            }
            if (autoplay && autoRoutine == null) autoRoutine = StartCoroutine(AutoLoop());
            if (!autoplay && leavingAutoResultWait)
            {
                ServiceSourceRoundLifecycle();
                ServiceConfiguredAutoPlay();
            }
        }

        private void PlaceWheelNeighbor(int number)
        {
            if (session == null || spinning || ResultDisplayActive || paysPage >= 0 || session.InDoubleUp ||
                number < 0) return;
            long before = session.TotalBet;
            TrySourceTableEvent(() => session.PlaceNeighbor(number), betPlacedSound,
                betLimitSound, clearRejectedResult: false, requestsBetState: true);
            if (session.TotalBet > before)
            {
                RestartSourcePrompt(PromptKind.PressSpin);
                // The source PlaceBetsForNeighborsOf calls GameSounds::Play
                // once for each of its four added spots.
                for (int i = 1; i < 4; ++i) PlayBetSound(betPlacedSound);
                pressSpinAt = Time.realtimeSinceStartup;
                neighborTouchUntil = Time.realtimeSinceStartup + .16f;
            }
        }

        private void SelectNeighbor(int number)
        {
            if (!TableTouchInputEnabled()) return;
            if (!session.CanPlaceNeighbor(number))
            {
                BetRejectionReason? reason = session.NeighborRejectionReason(number);
                if (reason.HasValue) QueueBetFailurePrompt(reason.Value);
                ClearNeighborSelection();
                return;
            }
            if (selectedNeighbor == number)
            {
                neighborEnabledWithoutPrecheck = false;
                RefreshNeighborUi();
                return;
            }
            selectedNeighbor = number;
            neighborEnabledWithoutPrecheck = false;
            PlayerPrefs.SetInt(NeighborSelectionKey, number);
            PlayerPrefs.Save();
            RefreshNeighborUi();
        }

        private void ClearNeighborSelection()
        {
            neighborEnabledWithoutPrecheck = false;
            if (selectedNeighbor != -1)
            {
                selectedNeighbor = -1;
                PlayerPrefs.SetInt(NeighborSelectionKey, -1);
                PlayerPrefs.Save();
            }
            if (uiNeighborButton != null) RefreshNeighborUi();
        }

        private void PlaceSector(RouletteSector sector)
        {
            if (session == null || spinning || ResultDisplayActive || paysPage >= 0 ||
                session.InDoubleUp) return;
            long before = session.TotalBet;
            TrySourceTableEvent(() => session.PlaceSector(sector), null, betLimitSound,
                clearRejectedResult: false, requestsBetState: true);
            if (session.TotalBet > before)
            {
                RestartSourcePrompt(PromptKind.PressSpin);
                pressSpinAt = Time.realtimeSinceStartup;
            }
        }

        private void RequestAutoPlay()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (autoplay) { ToggleAuto(); return; }
            if (SourceRoundBlocksOrdinaryInput) return;
            if (autoConfirmPending || serviceConfirmPending ||
                collectConfirmPending || pcCollectPending) return;
            autoConfirmPending = true;
            autoConfirmStartedTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp();
            autoConfirmDeadlineTimestamp = autoConfirmStartedTimestamp +
                AutoConfirmMilliseconds *
                System.Diagnostics.Stopwatch.Frequency / 1000;
            SetAutoConfirmButtonVisibility(true, true);
        }

        private void ConfirmAutoPlay(bool accepted)
        {
            if (accepted && PcGameOrdinaryInputBlocked) return;
            if (!autoConfirmPending) return;
            autoConfirmPending = false;
            autoConfirmDeadlineTimestamp = 0;
            SetAutoConfirmButtonVisibility(true, true);
            // Roulette::DoAutoPlayOnOff accepts enable only in RunLoop states
            // 1/2. A wheel or feature is still in StatePlay (state 3).
            if (accepted && !autoplay && !spinning && !winRackActive &&
                !SourceRoundBlocksOrdinaryInput &&
                session != null && !session.InDoubleUp &&
                !IsDoubleReveal && !IsDoubleExit && !doubleCollectPending)
                ToggleAuto();
            if (!ideckSecondDisplay) SetIDeckPreview(false);
        }

        private void PrepareStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 27, fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft };
            titleStyle.normal.textColor = new Color(1f, 0.82f, 0.28f);
            infoStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleLeft };
            infoStyle.normal.textColor = Color.white;
            labelStyle = new GUIStyle(infoStyle) { fontSize = 14 };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold };
            numberStyle = new GUIStyle(buttonStyle) { fontSize = 19 };
        }

        private bool Button(Rect rect, string label, GUIStyle style = null)
        {
            return GUI.Button(rect, label, style ?? buttonStyle);
        }

        private void DrawNumber(int number, Rect rect)
        {
            Color old = GUI.backgroundColor;
            GUI.backgroundColor = number == 0 ? new Color(0.10f, 0.62f, 0.27f) :
                RedNumbers.Contains(number) ? new Color(0.75f, 0.16f, 0.12f) :
                new Color(0.19f, 0.19f, 0.21f);
            long bet = session.Bets.ContainsKey(100 + number) ? session.Bets[100 + number] : 0;
            if (Button(rect, bet > 0 ? number + "\n" + bet : number.ToString(), numberStyle)) Number(number);
            GUI.backgroundColor = old;
        }

        private void DrawHeader()
        {
            for (int i = 0; i < profiles.Length; ++i)
            {
                int selected = i;
                Color old = GUI.backgroundColor;
                GUI.backgroundColor = profileIndex == i ? new Color(1f, 0.76f, 0.25f) : Color.white;
                if (Button(new Rect(1090 + i * 94, 22, 86, 42), profiles[i] + "%") && i != profileIndex)
                {
                    profileIndex = selected;
                    ClearStagedBets();
                    LoadProfile(false);
                }
                GUI.backgroundColor = old;
            }
            if (Button(new Rect(1390, 22, 80, 42), "STATS")) showStats = !showStats;
            if (Button(new Rect(1480, 22, 85, 42), "ALL BETS")) showAllBets = !showAllBets;
        }

        private void DrawTable()
        {
            GUI.Box(new Rect(500, 105, 1065, 510), GUIContent.none);
            GUI.Label(new Rect(520, 112, 700, 36), "BETTING TABLE  /  CLICK A NUMBER OR OUTSIDE BET", infoStyle);
            DrawNumber(0, new Rect(520, 163, 90, 342));
            for (int row = 0; row < 12; ++row)
                for (int col = 0; col < 3; ++col)
                    DrawNumber(row * 3 + col + 1, new Rect(620 + col * 304, 163 + row * 29, 296, 28));

            var outside = session.Profile.Records.Values.Where(x =>
                x.Type == "ROULETTE_LOW_HIGH" || x.Type == "ROULETTE_ODD_EVEN" ||
                x.Type == "ROULETTE_RED_BLACK" || x.Type == "ROULETTE_DOZENS" ||
                x.Type == "ROULETTE_COLUMNS").OrderBy(x => x.Id).ToArray();
            for (int i = 0; i < outside.Length && i < 18; ++i)
            {
                WinRecord record = outside[i];
                int col = i % 6, row = i / 6;
                string text = ShortName(record);
                if (Button(new Rect(520 + col * 172, 520 + row * 28, 166, 27), text)) Bet(record.Id);
            }
        }

        private string ShortName(WinRecord record)
        {
            if (record.Type == "ROULETTE_STRAIGHT") return record.Numbers[0].ToString();
            if (record.Type == "ROULETTE_LOW_HIGH") return record.Numbers.Min() == 1 ? "1–18" : "19–36";
            if (record.Type == "ROULETTE_ODD_EVEN") return record.Numbers.Contains(1) ? "ODD" : "EVEN";
            if (record.Type == "ROULETTE_RED_BLACK") return record.Numbers.Contains(1) ? "RED" : "BLACK";
            if (record.Type == "ROULETTE_DOZENS") return record.Numbers.Min() + "–" + record.Numbers.Max();
            if (record.Type == "ROULETTE_COLUMNS") return "COLUMN " + record.Numbers.Min();
            return record.Type.Replace("ROULETTE_", "").Replace('_', ' ') + " " +
                   record.Numbers.First() + "–" + record.Numbers.Last();
        }

        private void DrawControls()
        {
            GUI.Box(new Rect(25, 630, 1540, 245), GUIContent.none);
            GUI.Label(new Rect(45, 640, 300, 40), "CREDIT  " + session.Credit, titleStyle);
            GUI.Label(new Rect(385, 640, 320, 40), "BET  " + session.TotalBet, titleStyle);
            GUI.Label(new Rect(730, 640, 400, 40), "RETURN  " +
                (session.LastResult == null ? 0 : session.LastResult.Returned), titleStyle);
            GUI.Label(new Rect(45, 690, 120, 34), "CHIP", infoStyle);
            for (int i = 0; i < chips.Length; ++i)
            {
                int selected = chips[i];
                Color old = GUI.backgroundColor;
                GUI.backgroundColor = session.Chip == selected ? Color.yellow : Color.white;
                if (Button(new Rect(120 + i * 86, 686, 78, 40), selected.ToString()))
                    if (!SourceRoundBlocksOrdinaryInput && session.Chip != selected)
                        Try(() => session.SetChip(selected), betSelectedSound);
                GUI.backgroundColor = old;
            }
            GUI.Label(new Rect(485, 690, 135, 34), "DENOM", infoStyle);
            int denomIndex = Array.IndexOf(session.Profile.Denominations, (int)session.Denomination);
            if (Button(new Rect(575, 686, 60, 40), "−"))
                if (!SourceRoundBlocksOrdinaryInput)
                    Try(() => session.SetDenomination(session.Profile.Denominations[Math.Max(0, denomIndex - 1)]));
            GUI.Label(new Rect(645, 690, 100, 34), session.Denomination.ToString(), infoStyle);
            if (!SourceRoundBlocksOrdinaryInput && Button(new Rect(735, 686, 60, 40), "+"))
                Try(() => session.SetDenomination(session.Profile.Denominations[Math.Min(session.Profile.Denominations.Length - 1, denomIndex + 1)]));
            if (!SourceRoundBlocksOrdinaryInput && Button(new Rect(820, 686, 130, 40), neighborMode ? "NEIGHBOR ON" : "NEIGHBOR")) neighborMode = !neighborMode;
            if (Button(new Rect(960, 686, 130, 40), autoplay ? "AUTO ON" : "AUTO PLAY")) RequestAutoPlay();
            bool doubleEnabled = GUI.Toggle(new Rect(1120, 690, 170, 34), session.DoubleEnabled, "Double Up");
            if (!SourceRoundBlocksOrdinaryInput) session.DoubleEnabled = doubleEnabled;

            if (Button(new Rect(45, 760, 150, 65), "UNDO"))
                TrySourceTableEvent(UndoBet, betUndoSound, undoEvent: true);
            if (Button(new Rect(205, 760, 150, 65), "CLEAR"))
                TrySourceTableEvent(() => {
                    if (session.ResultPhase) session.NextRound();
                    session.Clear();
                }, betClearSound);
            if (!SourceRoundBlocksOrdinaryInput && Button(new Rect(365, 760, 150, 65), "REPEAT")) Try(() => session.Repeat());
            if (DoubleUpInputAllowed && session.DoubleAvailable &&
                Button(new Rect(685, 760, 160, 65), "DOUBLE UP"))
                Try(BeginDouble);
            if (session.ResultPhase && !session.InDoubleUp && Button(new Rect(855, 760, 145, 65), "NEXT"))
            {
                if (SourceDoubleOfferActive) TakeSourceDoubleOffer();
                else if (!SourceRoundBlocksOrdinaryInput) Try(() => session.NextRound());
            }
            Color original = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.96f, 0.68f, 0.1f);
            if (Button(new Rect(1250, 755, 280, 78), spinning ? "SPINNING" : "SPIN")) BeginSpin();
            GUI.backgroundColor = original;
        }

        private void DrawAllBets()
        {
            GUI.Box(new Rect(495, 100, 1070, 525), GUIContent.none);
            GUI.Label(new Rect(515, 112, 600, 40), "ALL 157 ORIGINAL BETS", titleStyle);
            if (Button(new Rect(1450, 112, 90, 38), "CLOSE")) showAllBets = false;
            search = GUI.TextField(new Rect(515, 158, 1010, 35), search);
            var records = session.Profile.Records.Values.OrderBy(x => x.Id)
                .Where(x => string.IsNullOrEmpty(search) ||
                    (x.Id + " " + x.Type + " " + string.Join(",", x.Numbers.Select(n => n.ToString()).ToArray()))
                    .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
            betScroll = GUI.BeginScrollView(new Rect(510, 204, 1020, 395), betScroll,
                new Rect(0, 0, 985, records.Length * 32));
            for (int i = 0; i < records.Length; ++i)
            {
                WinRecord record = records[i];
                string label = record.Id + "  " + record.Type.Replace("ROULETTE_", "") +
                    "  [" + string.Join(",", record.Numbers.Select(n => n.ToString()).ToArray()) +
                    "]  pays " + record.ReturnMultiple + "×";
                if (Button(new Rect(2, i * 32, 965, 30), label)) Bet(record.Id);
            }
            GUI.EndScrollView();
        }

        private void DrawStats()
        {
            GUI.Box(new Rect(1050, 85, 515, 515), GUIContent.none);
            GUI.Label(new Rect(1070, 105, 400, 45), "SESSION STATS", titleStyle);
            if (Button(new Rect(1450, 110, 90, 35), "CLOSE")) showStats = false;
            GUI.Label(new Rect(1070, 165, 465, 265),
                "Rounds: " + session.Rounds + "\n" +
                "Wagered: " + session.Wagered + "\n" +
                "Returned: " + session.Returned + "\n" +
                "Wins: " + session.Wins + "\n" +
                "Sampled RTP: " + (session.Wagered == 0 ? "—" :
                    (100.0 * session.Returned / session.Wagered).ToString("F2") + "%") + "\n" +
                "Last category: " + (session.LastResult == null ? 0 : session.LastResult.Category), infoStyle);
        }

        private void DrawDouble()
        {
            GUI.Box(new Rect(500, 290, 600, 290), GUIContent.none);
            GUI.Label(new Rect(525, 310, 500, 42), "DOUBLE UP / STAKE " + session.DoubleStake, titleStyle);
            GUI.Label(new Rect(525, 360, 500, 34), "Round " + (session.DoubleRounds + 1) + " / 16", infoStyle);
            if (Button(new Rect(530, 420, 165, 50), "RED")) Guess(-1);
            if (Button(new Rect(710, 420, 165, 50), "BLACK")) Guess(-2);
            if (Button(new Rect(890, 420, 165, 50), "TAKE WIN")) Try(BeginCollectDouble);
            if (Button(new Rect(530, 485, 125, 50), "HEARTS")) Guess(0);
            if (Button(new Rect(665, 485, 125, 50), "DIAMONDS")) Guess(1);
            if (Button(new Rect(800, 485, 125, 50), "CLUBS")) Guess(2);
            if (Button(new Rect(935, 485, 125, 50), "SPADES")) Guess(3);
        }

        private void Guess(int choice)
        {
            if (PcGameOrdinaryInputBlocked) return;
            try
            {
                if (!DoubleDecisionReady || doubleCollectPending ||
                    IsDoubleReveal || showingDoubleHelp) return;
                if(pcRoundArchiveSchemaVersion==4)
                {
                    CommitSasDoubleDrawV4(choice);
                    return;
                }
                long wager = session.DoubleStake;
                doubleAttractMultiplierOnReveal = session.DoubleSuitMode ?
                    Math.Max(2, CurrentDoubleAttractMultiplier()) : 2;
                PreparePcDoubleDraw(choice, wager);
                doubleRevealWon = session.GuessDouble(choice);
                RecordDoubleChoice(choice, wager, doubleRevealWon);
                CompletePcDoubleDraw();
                if (PcNativeDoubleFeatureTracked)
                {
                    status = doubleRevealWon ? "Double Up win" : "Double Up lost";
                    return;
                }
                doubleRevealMaximum = doubleRevealWon && !session.InDoubleUp;
                status = doubleRevealWon ? "Double Up win" : "Double Up lost";
                doubleRevealChoice = choice;
                AudioClip resultSound = doubleRevealWon ? doubleWinSound : doubleLoseSound;
                if (resultSound != null) audioSource.PlayOneShot(resultSound, .7f);
                int revealMs = doubleRevealMaximum && auNationalStandardsEnabled ?
                    3000 : 1250;
                doubleRevealUntil = Time.realtimeSinceStartup + revealMs / 1000f;
                doubleRevealUntilTimestamp = OriginalRackClock.AfterMilliseconds(
                    System.Diagnostics.Stopwatch.GetTimestamp(), revealMs,
                    System.Diagnostics.Stopwatch.Frequency);
                if (doubleRevealWon && session.InDoubleUp)
                {
                    doubleIdleAnimationStartedAt = doubleRevealUntil;
                    doubleIdleAnimationStartedTimestamp = doubleRevealUntilTimestamp;
                }
                if (!session.InDoubleUp)
                {
                    doubleExitStartedAt = doubleRevealUntil;
                    doubleExitUntil = doubleExitStartedAt +
                        (session.DoubleSuitMode || auNationalStandardsEnabled ? .01f : 1f);
                    doubleExitStartedTimestamp = doubleRevealUntilTimestamp;
                    doubleExitUntilTimestamp = OriginalRackClock.AfterMilliseconds(
                        doubleExitStartedTimestamp,
                        session.DoubleSuitMode || auNationalStandardsEnabled ? 10 : 1000,
                        System.Diagnostics.Stopwatch.Frequency);
                    MarkSourceDoubleExit();
                }
            }
            catch (Exception error)
            {
                if (PcReceiptEnabled) FailPcRoundLifecycle("rng-double", error);
                else status = error.Message;
            }
        }

        private void BeginCollectDouble()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!DoubleDecisionReady || IsDoubleReveal ||
                doubleCollectPending || showingDoubleHelp)
                throw new InvalidOperationException("Double Up take win unavailable");
            if (PcNativeDoubleFeatureTracked)
            {
                BeginPcNativeDoubleTake();
                return;
            }
            if (doubleButtonPressSound != null)
                audioSource.PlayOneShot(doubleButtonPressSound, .7f);
            // Opcode 0x1a fires after 100 ms, then fBackgroundOut runs for 1000 ms.
            doubleCollectPending = true;
            doubleExitStartedAt = Time.realtimeSinceStartup + .1f;
            doubleExitUntil = doubleExitStartedAt +
                (session.DoubleSuitMode || auNationalStandardsEnabled ? .01f : 1f);
            doubleExitStartedTimestamp = OriginalRackClock.AfterMilliseconds(
                System.Diagnostics.Stopwatch.GetTimestamp(), 100,
                System.Diagnostics.Stopwatch.Frequency);
            doubleExitUntilTimestamp = OriginalRackClock.AfterMilliseconds(
                doubleExitStartedTimestamp,
                session.DoubleSuitMode || auNationalStandardsEnabled ? 10 : 1000,
                System.Diagnostics.Stopwatch.Frequency);
            MarkSourceDoubleExit();
        }

        private void OpenDoubleHelp()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!DoubleDecisionReady || IsDoubleReveal ||
                doubleCollectPending || showingDoubleHelp) return;
            if (doubleButtonPressSound != null)
                audioSource.PlayOneShot(doubleButtonPressSound, .7f);
            showingDoubleHelp = true;
            doubleHelpUntil = Time.realtimeSinceStartup + 30f;
            doubleHelpUntilTimestamp = OriginalRackClock.AfterMilliseconds(
                System.Diagnostics.Stopwatch.GetTimestamp(), 30000,
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void CloseDoubleHelp(bool buttonPressed)
        {
            if (buttonPressed && PcGameOrdinaryInputBlocked) return;
            if (!showingDoubleHelp) return;
            if (buttonPressed && doubleButtonPressSound != null)
                audioSource.PlayOneShot(doubleButtonPressSound, .7f);
            showingDoubleHelp = false;
            doubleHelpUntilTimestamp = 0;
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
        }

        private IEnumerator DoubleHelpSmoke()
        {
            auNationalStandardsEnabled = false;
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            RefreshPortraitUi();
            yield return null;
            PointerEventData helpPress = PressThroughUiRaycast(uiDoubleHelpButton);
            if (!showingDoubleHelp)
                throw new InvalidOperationException("Double Up Help waited until release");
            ReleaseUiPointer(uiDoubleHelpButton, helpPress);
            if (!showingDoubleHelp)
                throw new InvalidOperationException("Double Up Help changed on release");
            RefreshPortraitUi();
            yield return null;
            if (!showingDoubleHelp || !uiDoubleHelp.activeSelf ||
                doubleHelpUntil - Time.realtimeSinceStartup < 29f)
                throw new InvalidOperationException("Double Up Help did not open for 30 seconds");
            if (uiDoubleRedButton.interactable || uiDoubleBlackButton.interactable ||
                uiDoubleTakeWinButton.interactable)
                throw new InvalidOperationException("Double Up Help left color buttons enabled");
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (ideckRedButton.gameObject.activeSelf ||
                ideckBlackButton.gameObject.activeSelf ||
                ideckTakeWinButton.gameObject.activeSelf)
                throw new InvalidOperationException("Double Up Help left iDeck decision buttons visible");
            SetIDeckPreview(false);
            RefreshPortraitUi();
            if (!uiDoubleHelpRounds.gameObject.activeSelf ||
                uiDoubleHelpRounds.texture != doubleHelpRounds[16] ||
                uiDoubleHelpRounds.rectTransform.anchoredPosition !=
                    new Vector2(593, -1249))
                throw new InvalidOperationException("Double Up Help source round-16 bitmap is wrong");
            if (session.DoubleMaximumWin != 2000000000L ||
                !uiDoubleHelpBackground.gameObject.activeSelf ||
                uiDoubleHelpWinLimit.Value != "")
                throw new InvalidOperationException(
                    "Source 2,000,000,000 fallback showed a maximum-win Help meter");
            if (uiDoubleCredit.Value != "" || uiDoubleBet.Value != "" ||
                uiDoubleCurrentWin.Value != "" || uiDoubleToWin.Value != "")
                throw new InvalidOperationException("Double Up Help kept feature amount meters visible");
            if (uiDoubleDenom.Value == "")
                throw new InvalidOperationException("Separate denomination meter stopped during Help");
            ClickThroughUiRaycast(uiDoubleHelpBlocker);
            if (!showingDoubleHelp)
                throw new InvalidOperationException("Double Up Help closed outside original EXIT area");
            PointerEventData exitPress =
                PressThroughUiRaycast(uiDoubleHelpExitButton);
            if (showingDoubleHelp)
                throw new InvalidOperationException("Double Up Help EXIT waited until release");
            ReleaseUiPointer(uiDoubleHelpExitButton, exitPress);
            if (showingDoubleHelp)
                throw new InvalidOperationException("Double Up Help EXIT repeated on release");
            RefreshPortraitUi();
            if (uiDoubleCredit.Value == "" || uiDoubleCurrentWin.Value == "")
                throw new InvalidOperationException("Double Up meters did not return after Help EXIT");
            auNationalStandardsEnabled = true;
            ClickThroughUiRaycast(uiDoubleHelpButton);
            RefreshPortraitUi();
            yield return null;
            if (!showingDoubleHelp ||
                uiDoubleHelpBackground.gameObject.activeSelf ||
                uiDoubleHelpRounds.gameObject.activeSelf ||
                uiDoubleHelpWinLimit.Value != "")
                throw new InvalidOperationException(
                    "Undefined Australian no-limit Help art or meter appeared");
            ClickThroughUiRaycast(uiDoubleHelpExitButton);
            auNationalStandardsEnabled = false;
            RefreshPortraitUi();
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(), doubleMaximumRounds: 4)
                { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            RefreshPortraitUi();
            yield return null;
            ClickThroughUiRaycast(uiDoubleHelpButton);
            RefreshPortraitUi();
            yield return null;
            if (!uiDoubleHelpRounds.gameObject.activeSelf ||
                uiDoubleHelpRounds.texture != doubleHelpRounds[4] ||
                uiDoubleHelpRounds.rectTransform.anchoredPosition !=
                    new Vector2(599, -1249))
                throw new InvalidOperationException("Double Up Help did not select configured round bitmap");
            doubleHelpUntil = Time.realtimeSinceStartup - .01f;
            doubleHelpUntilTimestamp = System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            yield return null;
            if (showingDoubleHelp)
                throw new InvalidOperationException("Double Up Help timer did not close");
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(), doubleMaximumWin: 10000)
                { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            RefreshPortraitUi();
            yield return null;
            ClickThroughUiRaycast(uiDoubleHelpButton);
            RefreshPortraitUi();
            yield return null;
            if (!uiDoubleHelpBackground.gameObject.activeSelf ||
                uiDoubleHelpWinLimit.Value != "")
                throw new InvalidOperationException(
                    "Non-Australian red/black Help showed its gated limit meter");
            ClickThroughUiRaycast(uiDoubleHelpExitButton);
            auNationalStandardsEnabled = true;
            RefreshPortraitUi();
            yield return null;
            ClickThroughUiRaycast(uiDoubleHelpButton);
            RefreshPortraitUi();
            yield return null;
            if (uiDoubleHelpBackground.gameObject.activeSelf ||
                uiDoubleHelpRounds.gameObject.activeSelf ||
                uiDoubleHelpWinLimit.Value != "RM50.00" ||
                uiDoubleHelpWinLimit.GlyphCount != 5)
                throw new InvalidOperationException("Australian configured maximum-win Help meter is wrong");
            RequireFeatureMeterNative(uiDoubleHelpWinLimit, "RM50.00", "double_win_limit",
                new Rect(732, 1335, 204, 32), 16, 25);
            CaptureFeatureMeterFrame("redblack_help_limit");
            Debug.Log("ROULETTE_DOUBLE_HELP_PASSED: source EXIT region, ordinary 16/4 rounds, absent Australian round feature, timeout, platform-gated win limit");
            Debug.Log("ROULETTE_REDBLACK_HELP_NATIVE_PASSED: native 25px maximum-limit font");
        }

        private IEnumerator SuitHelpLimitSmoke()
        {
            yield return null;
            if (!session.DoubleSuitMode)
                throw new InvalidOperationException("Suit Help limit smoke requires -rouletteSuitMode");
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(), doubleMaximumWin: 10000,
                doubleSuitMode: true) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            RefreshPortraitUi();
            yield return null;
            ClickThroughUiRaycast(uiDoubleHelpButton);
            RefreshPortraitUi();
            yield return null;
            if (!showingDoubleHelp || uiDoubleHelpWinLimit.Value != "RM25.00" ||
                uiDoubleHelpWinLimit.GlyphCount != 5 ||
                uiDoubleHelpRounds.gameObject.activeSelf ||
                uiDoubleCredit.Value != session.Credit.ToString() ||
                uiDoubleBet.Value != session.LastResult.Wager.ToString())
                throw new InvalidOperationException("Suit Help source maximum-win meter or exact-name round lookup is wrong");
            Debug.Log("ROULETTE_SUIT_HELP_LIMIT_PASSED: original font, quarter limit, missing exact-name round bitmap");
        }

        private IEnumerator SuitCashMeterSmoke()
        {
            yield return null;
            if (!session.DoubleSuitMode)
                throw new InvalidOperationException("Suit cash meter smoke requires -rouletteSuitMode");
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome(), doubleMaximumWin: 10000,
                doubleSuitMode: true) { DoubleEnabled = true };
            session.SetDenomination(25);
            // The OS keeps money in cents: 1000 cents becomes 40 credits at
            // 25 cents. Add 960 local test credits to exercise the original
            // 1000-credit suit-meter frame without minting money on switch.
            ((RouletteSession)session).AddDemoCredits(960);
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            // Hold within the source 2x attract interval for a stable
            // denomination comparison across Player frame rates.
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - .5f;
            RefreshPortraitUi();
            yield return null;
            string cashCredit = RouletteDenominationFormatter.ForFeatureCurrency(
                checked(session.Credit * session.Denomination));
            string cashBet = RouletteDenominationFormatter.ForFeatureCurrency(
                checked(session.LastResult.Wager * session.Denomination));
            string cashStake = RouletteDenominationFormatter.ForFeatureCurrency(
                checked(session.DoubleStake * session.Denomination));
            string cashToWin = RouletteDenominationFormatter.ForFeatureCurrency(
                checked(session.DoubleStake * session.Denomination *
                    Math.Max(2, CurrentDoubleAttractMultiplier())));
            if (uiSuitCreditCash.Value != cashCredit || uiSuitBetCash.Value != cashBet ||
                uiSuitCurrentWinCash.Value != cashStake ||
                uiSuitToWinCash.Value != cashToWin ||
                uiSuitCreditCash.GlyphCount == 0)
                throw new InvalidOperationException("Suit cash meters did not use source amount and denomination");
            RequireFeatureMeterNative(uiDoubleCredit, "999", "suit_meter",
                new Rect(378, 1582, 154, 28), 13, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiDoubleBet, "1", "suit_meter",
                new Rect(555, 1582, 147, 28), 10, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiDoubleCurrentWin, "36", "suit_meter",
                new Rect(185, 855, 154, 28), 13, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiDoubleToWin, "72", "suit_meter",
                new Rect(840, 855, 155, 28), 13, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiSuitCreditCash, cashCredit, "suit_cash_meter",
                new Rect(378, 1555, 154, 28), 14, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiSuitBetCash, cashBet, "suit_cash_meter",
                new Rect(555, 1555, 147, 28), 14, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiSuitCurrentWinCash, cashStake, "suit_cash_meter",
                new Rect(185, 825, 154, 28), 14, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiSuitToWinCash, cashToWin, "suit_cash_meter",
                new Rect(840, 825, 155, 28), 14, 26, measureHeight: 28);
            RequireFeatureMeterNative(uiDoubleDisclaimer, "MALFUNCTION VOIDS ALL PAYS AND PLAYS",
                "malfunction", new Rect(293, 1884, 420, 11), 10, 13);
            CaptureFeatureMeterFrame("suit_cash_25");
            ClickThroughUiRaycast(uiDoubleHelpButton);
            RefreshPortraitUi();
            yield return null;
            if (!showingDoubleHelp || uiSuitCreditCash.Value != cashCredit ||
                uiSuitBetCash.Value != cashBet ||
                uiDoubleHelpWinLimit.Value != "RM625.00")
                throw new InvalidOperationException("Suit cash meters or maximum limit changed incorrectly in Help");
            RequireFeatureMeterNative(uiDoubleHelpWinLimit, "RM625.00", "double_win_limit",
                new Rect(577, 1376, 204, 32), 16, 25);
            CaptureFeatureMeterFrame("suit_cash_help");
            Debug.Log("ROULETTE_SUIT_CASH_METER_PASSED: four yellow source meters, denomination 25, Help persistence");
            Debug.Log("ROULETTE_SUIT_METERS_NATIVE_PASSED: four white, four yellow, native 26px / measured 28px, Help limit 25px");
        }

        private IEnumerator DoubleMainClickSmoke()
        {
            yield return null;
            if (session.DoubleSuitMode || !session.InDoubleUp)
                throw new InvalidOperationException("Red/black click setup is wrong");
            if (SourceTransitionProgress(.032f, 500) != 0f ||
                SourceTransitionProgress(.034f, 500) != 0f ||
                Mathf.Abs(SourceTransitionProgress(.067f, 500) - 1f / 15f) > .0001f ||
                Mathf.Abs(SourceTransitionProgress(.496f, 500) - 14f / 15f) > .0001f ||
                SourceTransitionProgress(.529f, 500) != 1f ||
                Mathf.Abs(SourceTransitionProgress(1.001f, 1000) - 29f / 30f) > .0001f)
                throw new InvalidOperationException("Source 33 ms transition steps are wrong");
            if (SourceTransitionProgressMilliseconds(65, 500) != 0f ||
                Mathf.Abs(SourceTransitionProgressMilliseconds(66, 500) -
                    1f / 15f) > .0001f ||
                SourceTransitionProgressMilliseconds(528, 500) != 1f)
                throw new InvalidOperationException(
                    "Monotonic 33 ms entrance boundary is wrong");
            if (doubleRoot.Find("Card frame") != null)
                throw new InvalidOperationException("Suit-only card container leaked into red/black");
            RefreshPortraitUi();
            if (uiTouch.Visible ||
                DoubleDecisionReady || uiDoubleMessage.gameObject.activeSelf ||
                uiDoubleCurrentWin.Value != "" || uiDoubleToWin.Value != "" ||
                uiDoubleCredit.Value != "" || uiDoubleBet.Value != "" ||
                uiDoubleDenom.Value != "" || uiDoubleStake.Value != "" ||
                uiDoubleRedButton.interactable || uiDoubleBlackButton.interactable ||
                uiDoubleTakeWinButton.interactable ||
                uiDoubleHelpButton.interactable ||
                uiDoubleBlackButtonOverlay.gameObject.activeSelf ||
                uiDoubleTakeWinOverlay.gameObject.activeSelf ||
                doubleEntranceLayer == null ||
                doubleEntranceLayer.anchoredPosition.y >= 0f)
                throw new InvalidOperationException("Red/black entrance exposed decision controls");
            Guess(-1);
            if (IsDoubleReveal || session.DoubleStake != 36)
                throw new InvalidOperationException("Red/black accepted a choice during entrance");
            bool earlyCollectBlocked = false;
            try { BeginCollectDouble(); }
            catch (InvalidOperationException) { earlyCollectBlocked = true; }
            OpenDoubleHelp();
            if (!earlyCollectBlocked || doubleCollectPending || showingDoubleHelp)
                throw new InvalidOperationException("Double Up accepted collect/help during entrance");
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (ideckRedButton.gameObject.activeSelf ||
                ideckBlackButton.gameObject.activeSelf ||
                ideckTakeWinButton.gameObject.activeSelf ||
                ideckRedBlackControls.GetComponent<RectTransform>().anchoredPosition.y >= 0f)
                throw new InvalidOperationException("iDeck exposed Double Up controls during entrance");
            SetIDeckPreview(false);
            yield return new WaitUntil(() => DoubleDecisionReady);
            yield return new WaitForSecondsRealtime(.03f);
            yield return null;
            RefreshPortraitUi();
            if (!uiDoubleMessage.gameObject.activeSelf ||
                uiDoubleCurrentWin.Value != "36" || uiDoubleToWin.Value != "72" ||
                !uiDoubleRedButton.interactable ||
                !uiDoubleTakeWinButton.interactable ||
                doubleEntranceLayer.anchoredPosition != Vector2.zero)
                throw new InvalidOperationException("Red/black decision did not open after entrance");
            RequireFeatureMeterNative(uiDoubleCurrentWin, "36", "double_meter",
                new Rect(37, 720, 194, 50), 14, 31);
            RequireFeatureMeterNative(uiDoubleToWin, "72", "double_meter",
                new Rect(852, 720, 193, 50), 13, 31);
            RequireFeatureMeterNative(uiDoubleCredit, "RM9.63", "double_meter",
                new Rect(69, 1220, 233, 42), 18, 31);
            RequireFeatureMeterNative(uiDoubleBet, "37", "double_meter",
                new Rect(352, 1220, 164, 42), 14, 31);
            RequireFeatureMeterNative(uiDoubleDenom, "RM0.01", "double_meter",
                new Rect(569, 1220, 164, 42), 18, 31);
            RequireFeatureMeterNative(uiDoubleStake, "36", "double_meter",
                new Rect(782, 1220, 234, 42), 14, 31);
            RequireFeatureMeterNative(uiDoubleDisclaimer, "MALFUNCTION VOIDS ALL PAYS AND PLAYS",
                "malfunction", new Rect(293, 1884, 420, 11), 10, 13);
            uiDoubleBet.Set("12345678901234567890");
            RequireFeatureMeterNative(uiDoubleBet, "12345678901234567890", "double_meter",
                new Rect(352, 1220, 164, 42), 14, 31);
            uiDoubleBet.Set("37");
            CaptureFeatureMeterFrame("redblack_ready");
            PointerEventData redPress = PressThroughUiRaycast(uiDoubleRedButton);
            if (!doubleRevealWon || doubleRevealChoice != -1 ||
                session.DoubleStake != 72 || !session.InDoubleUp)
                throw new InvalidOperationException("Main red button waited until release");
            ReleaseUiPointer(uiDoubleRedButton, redPress);
            if (doubleRevealChoice != -1 || session.DoubleStake != 72)
                throw new InvalidOperationException("Main red button repeated on release");
            while (IsDoubleReveal) yield return null;
            RefreshPortraitUi();
            yield return null;
            RequireFeatureMeterNative(uiDoubleCurrentWin, "72", "double_meter",
                new Rect(37, 720, 194, 50), 13, 31);
            RequireFeatureMeterNative(uiDoubleToWin, "144", "double_meter",
                new Rect(852, 720, 193, 50), 14, 31);
            CaptureFeatureMeterFrame("redblack_win_72");
            PointerEventData takeWinPress = PressThroughUiRaycast(uiDoubleTakeWinButton);
            if (!doubleCollectPending)
                throw new InvalidOperationException("Main Take Win waited until release");
            ReleaseUiPointer(uiDoubleTakeWinButton, takeWinPress);
            if (!doubleCollectPending)
                throw new InvalidOperationException("Main Take Win changed on release");
            while (doubleCollectPending) yield return null;
            if (session.InDoubleUp || session.Credit != 1035)
                throw new InvalidOperationException("Main Take Win did not settle 72 credits");
            Debug.Log("ROULETTE_DOUBLE_MAIN_CLICK_PASSED: red 2x and Take Win 72");
            Debug.Log("ROULETTE_REDBLACK_METERS_NATIVE_PASSED: six native 31px meters, integer centres, overflow and 72 win");
            if (string.IsNullOrEmpty(capturePath)) Application.Quit();
        }

        private IEnumerator SuitBranchSmoke()
        {
            yield return null;
            if (!session.DoubleSuitMode || !session.InDoubleUp ||
                uiSuitChoiceButtons == null || ideckSuitChoiceButtons == null ||
                uiDoubleBackground.texture != suitBackground ||
                uiDoubleCard.texture != suitCardback ||
                doubleRoot.Find("Card frame") == null)
                throw new InvalidOperationException("Suit feature source art did not bind");
            RefreshPortraitUi();
            if (DoubleDecisionReady || uiDoubleMessage.gameObject.activeSelf ||
                uiDoubleCurrentWin.Value != "" || uiDoubleToWin.Value != "" ||
                uiDoubleCredit.Value != "" || uiDoubleBet.Value != "" ||
                uiSuitChoiceButtons[0].gameObject.activeSelf ||
                uiDoubleRedButton.interactable)
                throw new InvalidOperationException("Suit entrance exposed decision controls");
            Guess(0);
            if (IsDoubleReveal || session.DoubleStake != 36)
                throw new InvalidOperationException("Suit accepted a choice during entrance");
            yield return new WaitUntil(() => DoubleDecisionReady);
            RefreshPortraitUi();
            if (!uiSuitChoiceButtons[0].interactable ||
                uiSuitChoiceOverlays[0].texture != suitButtons[2])
                throw new InvalidOperationException("Suit decision state is wrong");
            if (doublePrompt2x == null || doublePrompt4x == null ||
                doublePrompt2x.width != 618 || doublePrompt2x.height != 49 ||
                doublePrompt4x.width != 703 || doublePrompt4x.height != 52 ||
                DoubleAttractMultiplier(.01f) != 0 ||
                DoubleAttractMultiplier(.02f) != 4 ||
                DoubleAttractMultiplier(.45f) != 2 ||
                DoubleAttractMultiplier(3.43f) != 4 ||
                DoubleAttractMultiplier(6.40f) != 2)
                throw new InvalidOperationException("Source Double Up prompt cycle is wrong");
            long distantIdleMs = 3000000000L;
            if (DoubleAttractMultiplierMilliseconds(distantIdleMs) !=
                DoubleAttractMultiplierMilliseconds(445L +
                    (distantIdleMs - 445L) % (425L * 14L)))
                throw new InvalidOperationException(
                    "Double Up prompt cycle failed after long uptime");
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - .2f;
            RefreshPortraitUi();
            if (!uiDoubleMessage.gameObject.activeSelf ||
                uiDoubleMessage.texture != doublePrompt4x ||
                uiDoubleToWin.Value != "144")
                throw new InvalidOperationException("Suit 4x prompt or win meter is wrong");
            RequireFeatureMeterNative(uiDoubleToWin, "144", "suit_meter",
                new Rect(840, 855, 155, 28), 10, 26, measureHeight: 28);
            CaptureFeatureMeterFrame("suit_4x");
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - .5f;
            RefreshPortraitUi();
            if (uiDoubleMessage.texture != doublePrompt2x ||
                uiDoubleToWin.Value != "72")
                throw new InvalidOperationException("Suit 2x prompt or win meter is wrong");
            RequireFeatureMeterNative(uiDoubleToWin, "72", "suit_meter",
                new Rect(840, 855, 155, 28), 13, 26, measureHeight: 28);
            yield return null;
            PointerEventData suitPress = PressThroughUiRaycast(uiSuitChoiceButtons[0]);
            if (!doubleRevealWon || doubleRevealChoice != 0 ||
                session.DoubleStake != 144)
                throw new InvalidOperationException("Main suit choice waited until release");
            ReleaseUiPointer(uiSuitChoiceButtons[0], suitPress);
            RefreshPortraitUi();
            if (!doubleRevealWon || doubleRevealChoice != 0 ||
                session.LastDoubleCard != 3 || session.DoubleStake != 144 ||
                uiSuitChoiceOverlays[0].texture != suitButtonsFlash[2] ||
                !uiSuitChoiceOverlays[0].gameObject.activeSelf ||
                uiSuitChoiceOverlays[1].gameObject.activeSelf ||
                uiDoubleCard.texture != doubleSuitCards[0])
                throw new InvalidOperationException("Suit win reveal or 4x award is wrong");
            while (IsDoubleReveal) yield return null;
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            if (!ideckSuitControls.activeInHierarchy ||
                !ideckSuitChoiceButtons[3].interactable ||
                !ideckSuitTakeWin.interactable)
                throw new InvalidOperationException("Suit iDeck controls are unavailable");
            PointerEventData deckSuitPress =
                PressThroughUiRaycast(ideckSuitChoiceButtons[3]);
            if (doubleRevealWon || doubleRevealChoice != 1 ||
                session.InDoubleUp || session.DoubleStake != 0)
                throw new InvalidOperationException("iDeck suit choice waited until release");
            ReleaseUiPointer(ideckSuitChoiceButtons[3], deckSuitPress);
            // The single-display iDeck preview closes on a selection, as it
            // does for live input. Reopen it to inspect the reveal artwork.
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (doubleRevealWon || doubleRevealChoice != 1 ||
                ideckSuitChoiceButtons[3].GetComponent<RawImage>().texture !=
                    suitDeckButtonsFlash[3] || session.InDoubleUp ||
                session.DoubleStake != 0)
                throw new InvalidOperationException("Suit iDeck loss reveal is wrong: " +
                    "won=" + doubleRevealWon + " choice=" + doubleRevealChoice +
                    " texture=" + ideckSuitChoiceButtons[3].GetComponent<RawImage>().texture.name +
                    " active=" + session.InDoubleUp + " stake=" + session.DoubleStake);
            Debug.Log("ROULETTE_SUIT_BRANCH_PASSED: source PNGs, main uGUI heart win, " +
                "4x award, iDeck diamond loss");
        }

        private IEnumerator DoubleButtonStatesSmoke()
        {
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            SetIDeckPreview(false);
            RefreshPortraitUi();
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (DoubleColorButtonState(-1) != 1 ||
                DoubleColorButtonState(-2) != 1 ||
                uiDoubleRedButtonOverlay.gameObject.activeSelf ||
                !uiDoubleBlackButtonOverlay.gameObject.activeSelf ||
                uiDoubleBlackButtonOverlay.texture != doubleBlackButton ||
                !ideckRedButton.interactable || !ideckBlackButton.interactable ||
                !ideckTakeWinButton.interactable ||
                ideckRedButton.GetComponent<RawImage>().texture != ideckRedDeck ||
                ideckBlackButton.GetComponent<RawImage>().texture != ideckBlackDeck)
                throw new InvalidOperationException("Double Up normal button features are wrong");
            if (DoubleIdleAnimationStep(.01f) != -1 ||
                DoubleIdleAnimationStep(.02f) != 0 ||
                DoubleIdleAnimationStep(.45f) != 1 ||
                DoubleIdleAnimationStep(1.30f) != 3 ||
                DoubleIdleAnimationStep(2.15f) != 5 ||
                DoubleIdleAnimationStep(3.43f) != 1)
                throw new InvalidOperationException("Double Up 20/425 ms lamp cycle is wrong");
            long distantLampMs = 3000000000L;
            if (DoubleIdleAnimationStepMilliseconds(distantLampMs) !=
                DoubleIdleAnimationStepMilliseconds(445L +
                    (distantLampMs - 445L) % (425L * 7L)))
                throw new InvalidOperationException(
                    "Double Up lamp cycle failed after long uptime");
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - .2f;
            SetIDeckPreview(false);
            RefreshPortraitUi();
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (!DoubleTakeWinFlash ||
                uiDoubleTakeWinOverlay.texture != doubleTakeWinFlash ||
                !ideckTakeWinFlashOverlay.gameObject.activeSelf ||
                !ideckTakeWinButton.interactable)
                throw new InvalidOperationException("Idle Take Win flash mismatch: phase=" +
                    CurrentDoubleIdleAnimationStep() + " flash=" + DoubleTakeWinFlash +
                    " main=" + uiDoubleTakeWinOverlay.texture.name +
                    " deck=" + ideckTakeWinFlashOverlay.gameObject.activeSelf +
                    " touch=" + ideckTakeWinButton.interactable);
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - 1.4f;
            SetIDeckPreview(false);
            RefreshPortraitUi();
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (DoubleColorButtonState(-1) != 2 ||
                uiDoubleRedButtonOverlay.texture != doubleRedButtonFlash ||
                ideckRedButton.GetComponent<RawImage>().texture != ideckRedDeckFlash ||
                !uiDoubleRedButton.interactable || !ideckRedButton.interactable)
                throw new InvalidOperationException("Idle RED flash disabled its touch area");
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - 2.25f;
            SetIDeckPreview(false);
            RefreshPortraitUi();
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (DoubleColorButtonState(-2) != 2 ||
                uiDoubleBlackButtonOverlay.texture != doubleBlackButtonFlash ||
                ideckBlackButton.GetComponent<RawImage>().texture != ideckBlackDeckFlash ||
                !uiDoubleBlackButton.interactable || !ideckBlackButton.interactable)
                throw new InvalidOperationException("Idle BLACK flash disabled its touch area");
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup - 1.4f;
            RefreshDiagnosticUi();
            // A preview switch needs one rendered layout frame before its
            // screen-space GraphicRaycaster has usable bounds.
            yield return null;
            RefreshDiagnosticUi();
            if (ideckSecondDisplay) Guess(-1);
            else
            {
                PointerEventData redPress = PressThroughUiRaycast(ideckRedButton);
                if (!IsDoubleReveal || doubleRevealChoice != -1)
                    throw new InvalidOperationException("iDeck RED waited until release");
                ReleaseUiPointer(ideckRedButton, redPress);
            }
            if (!IsDoubleReveal || doubleRevealChoice != -1)
                throw new InvalidOperationException("Idle flashing RED did not accept an iDeck click");
            SetIDeckPreview(false);
            RefreshPortraitUi();
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            if (DoubleColorButtonState(-1) != 2 ||
                DoubleColorButtonState(-2) != 0 ||
                uiDoubleRedButtonOverlay.texture != doubleRedButtonFlash ||
                !uiDoubleRedButtonOverlay.gameObject.activeSelf ||
                uiDoubleBlackButtonOverlay.gameObject.activeSelf ||
                !ideckRedButton.gameObject.activeSelf ||
                ideckRedButton.interactable ||
                ideckRedButton.GetComponent<RawImage>().texture != ideckRedDeckFlash ||
                ideckBlackButton.gameObject.activeSelf ||
                ideckTakeWinButton.gameObject.activeSelf ||
                uiDoubleTakeWinButton.interactable)
                throw new InvalidOperationException("Double Up selected flash/disabled button features are wrong");
            while (IsDoubleReveal) yield return null;
            RefreshDiagnosticUi();
            if (session.InDoubleUp &&
                (DoubleColorButtonState(-1) != 1 ||
                 DoubleColorButtonState(-2) != 1 ||
                 !ideckRedButton.interactable || !ideckBlackButton.interactable))
                throw new InvalidOperationException("Double Up buttons did not recover after reveal");
            Debug.Log("ROULETTE_DOUBLE_BUTTON_STATES_PASSED: normal, 20/425 ms idle lamp cycle, selected flash, disabled peer, recovery");
        }

        private IEnumerator DoubleCollectSmoke()
        {
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            long expectedAward = session.DoubleStake * 2;
            if (!session.GuessDouble(-1) || session.DoubleStake != expectedAward)
                throw new InvalidOperationException("Double Up setup failed");
            long creditBefore = session.Credit;
            SetIDeckPreview(true);
            RefreshPortraitUi();
            // Let the preview Canvas finish its layout before a screen-space raycast.
            yield return null;
            RefreshPortraitUi();
            if (!ideckTakeWinButton.interactable ||
                !ideckTakeWinButton.gameObject.activeSelf)
                throw new InvalidOperationException("iDeck Take Win is unavailable before collection");
            float clickedAt = Time.realtimeSinceStartup;
            PointerEventData takeWinPress = PressThroughUiRaycast(ideckTakeWinButton);
            if (!doubleCollectPending)
                throw new InvalidOperationException("iDeck Take Win waited until release");
            ReleaseUiPointer(ideckTakeWinButton, takeWinPress);
            if (!doubleCollectPending)
                throw new InvalidOperationException("iDeck Take Win changed on release");
            RefreshPortraitUi();
            if (DoubleColorButtonState(-1) != 1 ||
                DoubleColorButtonState(-2) != 1 ||
                !uiDoubleTakeWinOverlay.gameObject.activeSelf ||
                uiDoubleTakeWinButton.interactable)
                throw new InvalidOperationException(
                    "Take Win 100 ms wait did not retain source button art and disable input");
            float transitionAt = doubleExitStartedAt;
            float completeAt = doubleExitUntil;
            if (session.Credit != creditBefore || !session.InDoubleUp)
                throw new InvalidOperationException("Take Win credited before the original timer");
            if (auNationalStandardsEnabled)
            {
                if (Mathf.Abs(transitionAt - clickedAt - .1f) > .02f ||
                    Mathf.Abs(completeAt - transitionAt - .01f) > .002f ||
                    DoubleExitProgress != 0f)
                    throw new InvalidOperationException(
                        "Australian Take Win did not schedule the 100+10 ms source exit");
                while (doubleCollectPending &&
                       Time.realtimeSinceStartup < completeAt + 2f)
                    yield return null;
                RefreshPortraitUi();
                if (doubleCollectPending || session.InDoubleUp ||
                    session.Credit != creditBefore + expectedAward ||
                    doubleRoot.gameObject.activeSelf)
                    throw new InvalidOperationException(
                        "Australian Take Win did not credit after the short exit");
                Debug.Log("ROULETTE_AU_DOUBLE_COLLECT_PASSED: 100+10 ms award=" +
                    expectedAward);
                yield break;
            }
            while (Time.realtimeSinceStartup < transitionAt + .15f)
                yield return null;
            if (!doubleCollectPending || !IsDoubleExit ||
                session.Credit != creditBefore)
                throw new InvalidOperationException("Take Win credited during background-out transition");
            yield return null;
            if (portraitCanvas != null &&
                (!doubleExitVisualActive || !uiDoubleBackground.gameObject.activeSelf ||
                    !uiDoubleBackground.gameObject.activeInHierarchy ||
                    uiDoubleMessage.gameObject.activeSelf))
                throw new InvalidOperationException("Take Win background-out uGUI state is incorrect");
            if (ideckCanvas != null && !ideckSecondDisplay)
            {
                SetIDeckPreview(true);
                yield return null;
                RectTransform deckOut = ideckExitPanel.GetComponent<RectTransform>();
                if (!ideckCanvas.enabled || !ideckExitPanel.activeSelf ||
                    ideckRedBlackControls.activeSelf ||
                    deckOut.anchoredPosition.y >= 0f ||
                    deckOut.anchoredPosition.y <= -600f ||
                    ideckExitTakeWin == null || ideckExitRed == null ||
                    ideckExitBlack == null)
                    throw new InvalidOperationException("Take Win iDeck panel-out uGUI state is incorrect");
                SetIDeckPreview(false);
            }
            while (doubleCollectPending && Time.realtimeSinceStartup < completeAt + 2f)
                yield return null;
            if (doubleCollectPending || session.InDoubleUp ||
                session.Credit != creditBefore + expectedAward ||
                Time.realtimeSinceStartup + .002f < completeAt)
                throw new InvalidOperationException("Take Win timer did not complete correctly");
            Debug.Log("ROULETTE_DOUBLE_COLLECT_PASSED: 100 ms plus 1000 ms background-out, award=" +
                expectedAward);
        }

        private IEnumerator DoubleExitSmoke()
        {
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            session.StartDouble();
            doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
            float lossStartedAt = Time.realtimeSinceStartup;
            Guess(-2); // Fixed dealer suit is heart, so BLACK loses.
            if (auNationalStandardsEnabled)
            {
                if (session.InDoubleUp || !IsDoubleReveal ||
                    Mathf.Abs(doubleRevealUntil - lossStartedAt - 1.25f) > .02f ||
                    Mathf.Abs(doubleExitUntil - doubleExitStartedAt - .01f) > .002f ||
                    DoubleExitProgress != 0f)
                    throw new InvalidOperationException(
                        "Australian Double Up loss did not use the 10 ms exit fallback");
                while (Time.realtimeSinceStartup < doubleExitUntil + .05f)
                    yield return null;
                RefreshPortraitUi();
                if (doubleRoot.gameObject.activeSelf)
                    throw new InvalidOperationException(
                        "Australian Double Up loss did not return to the game");

                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(), doubleMaximumWin: 80)
                    { DoubleEnabled = true };
                session.PlaceBet(100, 1);
                session.Spin();
                session.StartDouble();
                doubleEntranceStartedAt = float.NegativeInfinity;
                doubleIdleAnimationStartedAt = Time.realtimeSinceStartup;
                float maximumStartedAt = Time.realtimeSinceStartup;
                Guess(-1); // Fixed heart is red; the next win would exceed 80.
                if (!doubleRevealWon || !doubleRevealMaximum || session.InDoubleUp ||
                    Mathf.Abs(doubleRevealUntil - maximumStartedAt - 3f) > .02f ||
                    Mathf.Abs(doubleExitUntil - doubleExitStartedAt - .01f) > .002f ||
                    DoubleExitProgress != 0f)
                    throw new InvalidOperationException(
                        "Australian maximum award did not use 3000 ms and 10 ms timers");
                while (Time.realtimeSinceStartup < doubleExitUntil + .05f)
                    yield return null;
                RefreshPortraitUi();
                if (doubleRoot.gameObject.activeSelf)
                    throw new InvalidOperationException(
                        "Australian maximum award did not return to the game");
                Debug.Log("ROULETTE_AU_DOUBLE_EXIT_PASSED: loss=1250+10 ms max=3000+10 ms");
                yield break;
            }
            if (session.InDoubleUp || !IsDoubleReveal ||
                Mathf.Abs(doubleExitUntil - doubleExitStartedAt - 1f) > .001f)
                throw new InvalidOperationException("Losing Double Up exit was not scheduled");
            while (!IsDoubleExit && Time.realtimeSinceStartup < doubleExitUntil)
                yield return null;
            yield return null;
            if (!IsDoubleExit || portraitCanvas == null ||
                !doubleRoot.gameObject.activeSelf || !doubleExitVisualActive ||
                uiDoubleMessage.gameObject.activeSelf ||
                !uiDoubleBackground.gameObject.activeInHierarchy)
                throw new InvalidOperationException("Losing Double Up did not enter background-out");
            while (Time.realtimeSinceStartup < doubleExitUntil)
                yield return null;
            yield return null;
            if (doubleRoot.gameObject.activeSelf || session.InDoubleUp)
                throw new InvalidOperationException("Losing Double Up did not return to the game");
            Debug.Log("ROULETTE_DOUBLE_EXIT_PASSED: 1250 ms reveal plus 1000 ms background-out");
        }
    }
}
