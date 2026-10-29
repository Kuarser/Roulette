using System;
using System.Linq;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class OriginalRect
        {
            public int x, y, width, height, z;
            public Rect ToRect() { return new Rect(x, y, width, height); }
        }

        [Serializable]
        private sealed class OriginalLayout
        {
            public OriginalRect main_screen;
            public OriginalRect table_area;
            public OriginalRect wheel_ball;
            public OriginalRect wheel_main;
            public OriginalRect wheel_spindle;
            public OriginalRect[] chip_buttons;
            public OriginalRect undo_button;
            public OriginalRect clear_button;
            public OriginalRect[] bet_spots;
        }

        [Serializable]
        private sealed class NeighborSelectionLayout
        {
            public OriginalRect[] selections;
        }

        [Serializable]
        private sealed class OriginalPoint
        {
            public float x, y;
            public Vector2 ToVector() { return new Vector2(x, y); }
        }

        [Serializable]
        private sealed class OriginalBallLoop
        {
            public OriginalPoint[] points;
        }

        [Serializable]
        private sealed class OriginalBallMotion
        {
            public OriginalBallLoop[] descent;
            public OriginalBallLoop[] land;
        }

        private OriginalLayout originalLayout;
        private int[] originalBetHitOrder;
        private OriginalBallLoop originalBallLoop;
        private OriginalBallMotion originalBallMotion;
        private Texture2D originalBackground;
        private Texture2D[] originalOddsPanels;
        private Texture2D[] originalChips;
        private Texture2D[] originalPlacedChips;
        private Texture2D[][] originalMainChipButtonFrames;
        private Texture2D[] originalBetMarkerRejectedFrames;
        private Texture2D originalUndo, originalClear;
        private Texture2D neighborOff, neighborOn, tiersButton, voisinsButton, orphelinsButton;
        private NeighborSelectionLayout neighborSelectionLayout;
        private Texture2D[] neighborSelected;
        private Texture2D neighborMeterRed, neighborMeterBlack, neighborMeterGreen, neighborTouch;
        private Texture2D resultSpotRed, resultSpotBlack, resultSpotGreen;
        private Texture2D topperSpotRed, topperSpotBlack, topperSpotGreen;
        private OriginalBitmapFont topperResultFont;
        private OriginalBitmapFont neighborPickFont;
        private int selectedNeighbor = -1;
        private bool neighborEnabledWithoutPrecheck;
        private Texture2D doubleBackground, doubleCardback;
        private Texture2D doubleRedButtonDisabled, doubleBlackButtonDisabled;
        private Texture2D doubleRedButtonFlash, doubleBlackButtonFlash, doubleBlackButton;
        private Texture2D doubleTakeWinButton, doubleTakeWinFlash;
        private Texture2D doubleHelpBackground, doubleExitButton;
        private Texture2D[] doubleHelpRounds;
        private static readonly int[] DoubleHelpRoundX = {
            0, 0, 599, 600, 599, 600, 599, 600, 599, 599,
            593, 594, 592, 592, 593, 593, 593
        };
        private Texture2D doubleRedCard, doubleBlackCard;
        private Texture2D[] doubleSuitCards;
        private Texture2D suitBackground, suitCardback, suitHelpBackground;
        private Texture2D suitRedCard, suitBlackCard, suitWin, suitLose;
        private Texture2D[] suitButtons, suitButtonsFlash, suitButtonsDisabled;
        private Texture2D[] suitDeckButtons, suitDeckButtonsFlash, suitDeckButtonsDisabled;
        private Texture2D suitDeckBackground, suitTakeWinDeck, suitTakeWinDeckFlash;
        private Texture2D suitMaximumAward;
        private Texture2D[] suitHelpRounds;
        private Texture2D doubleCardContainer, doublePrompt, doublePrompt2x,
            doublePrompt4x, doubleWin, doubleLose, doubleMaximumAward;
        private AudioClip doubleWinSound, doubleLoseSound, doubleButtonPressSound;
        private float doubleRevealUntil;
        private float doubleIdleAnimationStartFloat = float.PositiveInfinity;
        private long doubleIdleAnimationStartedTimestamp = long.MaxValue;
        private float doubleIdleAnimationStartedAt {
            get { return doubleIdleAnimationStartFloat; }
            set {
                doubleIdleAnimationStartFloat = value;
                if (float.IsPositiveInfinity(value)) {
                    doubleIdleAnimationStartedTimestamp = long.MaxValue;
                    return;
                }
                long now = System.Diagnostics.Stopwatch.GetTimestamp();
                double offset = (double)value - Time.realtimeSinceStartup;
                doubleIdleAnimationStartedTimestamp = now + (long)Math.Round(
                    offset * System.Diagnostics.Stopwatch.Frequency);
            }
        }
        private float doubleEntranceStartFloat = float.NegativeInfinity;
        private long doubleEntranceStartedTimestamp;
        private float doubleEntranceStartedAt {
            get { return doubleEntranceStartFloat; }
            set {
                doubleEntranceStartFloat = value;
                if (float.IsNegativeInfinity(value)) {
                    doubleEntranceStartedTimestamp = 0;
                    return;
                }
                long now = System.Diagnostics.Stopwatch.GetTimestamp();
                double offset = (double)value - Time.realtimeSinceStartup;
                doubleEntranceStartedTimestamp = now + (long)Math.Round(
                    offset * System.Diagnostics.Stopwatch.Frequency);
            }
        }
        private int doubleRevealChoice;
        private bool doubleRevealWon;
        private bool doubleRevealMaximum;
        private int doubleAttractMultiplierOnReveal = 2;
        private Texture2D footerPanel, footerCreditMeterButton, footerCashMeterLabel,
            footerCashBetMeterLabel, footerCashPaidMeterLabel,
            footerDenomTouchBar,
            footerDenomTouchButton, footerInfoButton, footerInfoTop,
            footerInfoBottom, footerSeePaysButton, footerVolumeButton,
            footerMeterFont;
        private OriginalBitmapFont footerDenomFont, footerTitleFont;
        private OriginalBitmapFont footerCreditTitleFont, footerWinTitleFont,
            footerCashCreditFont, footerCashPaidFont;
        private OriginalBitmapFont footerTouchFont, footerDisclaimerFont;
        private OriginalBitmapFont footerMessageFont, footerPaidFont;
        private OriginalBitmapFont[] footerCreditMeterFonts, footerPaidMeterFonts,
            footerCreditTitleFonts, footerWinTitleFonts, footerDenomFonts,
            footerOneCreditFonts;
        private OriginalBitmapFont toteWhiteFont, toteRedFont;
        private OriginalBitmapFont toteGreenFont, toteYellowFont;
        private Texture2D[] footerGamePlayFrames;
        private Texture2D[] footerGamePlayIntroFrames;
        private Texture2D[] footerGamePlayOutroFrames;
        private enum FooterMoviePhase { Intro, Loop, Outro, Hidden }
        private FooterMoviePhase footerMoviePhase = FooterMoviePhase.Hidden;
        private OriginalFooterMovieClock footerMovieClock;
        private const float FooterMovieFrameSeconds = .042f;
        private Texture2D[] pressSpinFrames, pressSpinOutFrames;
        private Texture2D[] gameOverFrames, gameOverOutFrames;
        private float pressSpinAt, gameOverAt;
        private Texture2D ideckPanel, ideckAutoPlay, ideckDoubleUp, ideckTakeWin;
        private Texture2D ideckService, ideckCashout;
        private Texture2D ideckAutoConfirm, ideckServiceConfirm, ideckCollectConfirm,
            ideckAutoYes, ideckAutoNo;
        private Texture2D ideckDoublePanel, ideckRedDeck, ideckBlackDeck;
        private Texture2D ideckRedDeckFlash, ideckBlackDeckFlash, ideckTakeWinDeckFlash;
        private Texture2D ideckExitTakeWin, ideckExitRed, ideckExitBlack;
        private Texture2D[] paysPages, paysLinePages;
        private Texture2D paysBack, paysExit, paysNext;
        private int paysPage = -1;
        private Texture2D[] ideckChips;
        private bool ideckPreview;
        private bool autoConfirmPending;
        private bool topperPreview;
        private bool showingDoubleHelp;
        private float doubleHelpUntil;
        // Native presentation changes when its event is consumed. The clock
        // only measures animation progress while that view remains active.
        private bool IsDoubleReveal { get { return PcNativeDoubleFeatureTracked ?
            PcNativeDoubleFeatureReveal : doubleRevealUntilTimestamp >
            System.Diagnostics.Stopwatch.GetTimestamp(); } }
        // ResetLamps starts opcode 0x11 after 20 ms. AnimateLampsAndButtons
        // then advances through states 0..7 at 425 ms intervals, repeating
        // states 1..7 while the player may still choose a button.
        private static int DoubleIdleAnimationStep(float elapsed)
        {
            if (elapsed < .02f) return -1;
            if (elapsed < .445f) return 0;
            return 1 + (Mathf.FloorToInt((elapsed - .445f) / .425f) % 7);
        }
        private int CurrentDoubleIdleAnimationStep()
        {
            return DoubleIdleAnimationStepMilliseconds(
                OriginalRackClock.ElapsedMillisecondsLong(
                    doubleIdleAnimationStartedTimestamp,
                    System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency));
        }
        private static int DoubleIdleAnimationStepMilliseconds(long elapsedMs)
        {
            if (elapsedMs < 20) return -1;
            if (elapsedMs < 445) return 0;
            return 1 + (int)(((elapsedMs - 445) / 425) % 7);
        }
        private bool DoubleDecisionReady
        {
            get {
                if(pcRoundArchiveSchemaVersion==4&&pcDoubleFeature==null)return false;
                return session != null && session.InDoubleUp &&
                    (!PcNativeDoubleFeatureTracked || PcNativeDoubleFeatureDecisionReady) &&
                    System.Diagnostics.Stopwatch.GetTimestamp() >=
                    doubleIdleAnimationStartedTimestamp;
            }
        }
        private bool DoubleInitialMetersReady
        {
            get {
                if (PcNativeDoubleFeatureTracked)
                    return PcNativeDoubleFeatureVisible && pcDoubleFeatureView != OriginalDoubleFeatureView.Entrance &&
                        pcDoubleFeatureView != OriginalDoubleFeatureView.Inactive;
                return float.IsNegativeInfinity(doubleEntranceStartedAt) ||
                    OriginalRackClock.ElapsedMillisecondsLong(
                        doubleEntranceStartedTimestamp,
                        System.Diagnostics.Stopwatch.GetTimestamp(),
                        System.Diagnostics.Stopwatch.Frequency) >= 1000;
            }
        }
        private float DoubleEntranceProgress
        {
            get {
                if (session == null || session.DoubleSuitMode ||
                    float.IsNegativeInfinity(doubleEntranceStartedAt)) return 1f;
                long elapsedMs = OriginalRackClock.ElapsedMillisecondsLong(
                    doubleEntranceStartedTimestamp,
                    System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency);
                return SourceTransitionProgressMilliseconds(elapsedMs, 500);
            }
        }
        // CFeatureMgr::PlayFeatPvt uses duration / 33 integer steps and a
        // 33 ms Trigger. TransitionMovement displays the current point before
        // advancing it by a fixed step for the next callback.
        private static float SourceTransitionProgress(float elapsed, int durationMs)
        {
            int steps = durationMs / 33;
            int callbacks = Mathf.FloorToInt(Mathf.Max(0f, elapsed) * 1000f / 33f);
            return Mathf.Clamp(callbacks - 1, 0, steps) / (float)steps;
        }
        private static float SourceTransitionProgressMilliseconds(long elapsedMs,
            int durationMs)
        {
            int steps = durationMs / 33;
            long callbacks = Math.Max(0L, elapsedMs) / 33L;
            return (float)Math.Max(0L, Math.Min((long)steps, callbacks - 1L)) / steps;
        }
        // ResetLamps seeds the banner at 2x. CycleAttractBanner is called by
        // lamp states 0 and 1: the first two timer events are 20 and 445 ms,
        // then state 1 repeats every seven 425 ms events.
        private static int DoubleAttractMultiplier(float elapsed)
        {
            if (elapsed < .02f) return 0;
            if (elapsed < .445f) return 4;
            int eventsAfterFirst = 1 + Mathf.FloorToInt((elapsed - .445f) / .425f);
            return ((eventsAfterFirst - 1) / 7) % 2 == 0 ? 2 : 4;
        }
        private int CurrentDoubleAttractMultiplier()
        {
            return DoubleAttractMultiplierMilliseconds(
                OriginalRackClock.ElapsedMillisecondsLong(
                    doubleIdleAnimationStartedTimestamp,
                    System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency));
        }
        private static int DoubleAttractMultiplierMilliseconds(long elapsedMs)
        {
            if (elapsedMs < 20) return 0;
            if (elapsedMs < 445) return 4;
            long eventsAfterFirst = 1 + (elapsedMs - 445) / 425;
            return ((eventsAfterFirst - 1) / 7) % 2 == 0 ? 2 : 4;
        }
        private bool DoubleTakeWinFlash
        {
            get {
                if (session == null || !session.InDoubleUp || IsDoubleReveal ||
                    IsDoubleExit || showingDoubleHelp || doubleCollectPending)
                    return false;
                int step = CurrentDoubleIdleAnimationStep();
                return step == 0 || step == 1;
            }
        }
        // SuitGamble::SetButtonState: 0=disabled, 1=normal, 2=flash.
        private int DoubleColorButtonState(int choice)
        {
            if (showingDoubleHelp || IsDoubleExit) return 0;
            // HandleTakeWinPress plays the normal color features for the
            // 100 ms opcode-0x1a wait before TransOut starts.
            if (doubleCollectPending) return 1;
            if (IsDoubleReveal) return doubleRevealChoice == choice ? 2 : 0;
            if (!DoubleDecisionReady) return 0;
            int step = CurrentDoubleIdleAnimationStep();
            if ((choice == -1 && step == 3) ||
                (choice == -2 && step == 5) ||
                (session.DoubleSuitMode && step == 4 &&
                    (choice == 0 || choice == 1)) ||
                (session.DoubleSuitMode && step == 6 &&
                    (choice == 2 || choice == 3))) return 2;
            return 1;
        }
        private bool IsDoubleExit { get {
            if (PcNativeDoubleFeatureTracked) return PcNativeDoubleFeatureExiting;
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            return now >= doubleExitStartedTimestamp && now < doubleExitUntilTimestamp;
        } }
        private float DoubleExitProgress { get {
            // Suit or Australian-standard exit takes the 10 ms fallback
            // without playing fBackgroundOut or fDeckPanelOut.
            if (auNationalStandardsEnabled ||
                (session != null && session.DoubleSuitMode)) return 0f;
            int elapsedMs = OriginalRackClock.ElapsedMilliseconds(
                doubleExitStartedTimestamp, System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
            return SourceTransitionProgressMilliseconds(elapsedMs, 1000);
        } }
        private GUIStyle originalWhite, originalSmall, originalNumber, originalMeter, invisibleButton;
        private GUIStyle ideckButtonText;
        private const float PortraitWidth = 1080f;
        private const float PortraitHeight = 1920f;

        private void LoadOriginalScreen()
        {
            originalBackground = Resources.Load<Texture2D>("Original/background");
            originalOddsPanels = new[] { "89", "93", "97" }.Select(suffix =>
                Resources.Load<Texture2D>("Original/odds_" + suffix)).ToArray();
            TextAsset geometry = Resources.Load<TextAsset>("Original/layout");
            if (geometry != null) originalLayout = JsonUtility.FromJson<OriginalLayout>(geometry.text);
            if (originalLayout != null && originalLayout.bet_spots != null)
                originalBetHitOrder = Enumerable.Range(0, originalLayout.bet_spots.Length)
                    .OrderBy(i => originalLayout.bet_spots[i].width *
                                  originalLayout.bet_spots[i].height).ToArray();
            TextAsset ballLoop = Resources.Load<TextAsset>("Original/ball_loop");
            if (ballLoop != null) originalBallLoop = JsonUtility.FromJson<OriginalBallLoop>(ballLoop.text);
            TextAsset ballMotion = Resources.Load<TextAsset>("Original/ball_motion");
            if (ballMotion != null)
                originalBallMotion = JsonUtility.FromJson<OriginalBallMotion>(ballMotion.text);
            originalChips = Enumerable.Range(1, 4).Select(i =>
                Resources.Load<Texture2D>("Original/chip" + i)).ToArray();
            // BetSpot::SetState(5) selects frame 6 of Table01ButtonBet*.mng.
            // That source frame has a blank center for the aggregate label.
            originalPlacedChips = Enumerable.Range(1, 4).Select(i =>
                Resources.Load<Texture2D>("Original/PlacedChip/chip" + i)).ToArray();
            originalMainChipButtonFrames = Enumerable.Range(1, 4).Select(i =>
                Enumerable.Range(1, 4).Select(frame => Resources.Load<Texture2D>(
                    "Original/MainChipButton/Bet" + i + "_" + frame)).ToArray()).ToArray();
            // RouletteTable::HandleTableEvent selects sprite state 4 when
            // a hovered spot is capped; that is the fifth original MNG frame.
            originalBetMarkerRejectedFrames = Enumerable.Range(1, 4).Select(i =>
                Resources.Load<Texture2D>("Original/MainChipButton/Bet" + i + "_5"))
                .ToArray();
            originalUndo = Resources.Load<Texture2D>("Original/undo");
            originalClear = Resources.Load<Texture2D>("Original/clear");
            neighborOff = Resources.Load<Texture2D>("Original/neighbor_off");
            neighborOn = Resources.Load<Texture2D>("Original/neighbor_on");
            TextAsset neighborGeometry = Resources.Load<TextAsset>("Original/neighbor_selections");
            if (neighborGeometry != null)
                neighborSelectionLayout = JsonUtility.FromJson<NeighborSelectionLayout>(neighborGeometry.text);
            neighborSelected = Enumerable.Range(0, 37).Select(i =>
                Resources.Load<Texture2D>("Original/NeighborSelected/" + i)).ToArray();
            neighborMeterRed = Resources.Load<Texture2D>("Original/neighbor_meter_red");
            neighborMeterBlack = Resources.Load<Texture2D>("Original/neighbor_meter_black");
            neighborMeterGreen = Resources.Load<Texture2D>("Original/neighbor_meter_green");
            neighborTouch = Resources.Load<Texture2D>("Original/neighbor_touch");
            resultSpotRed = Resources.Load<Texture2D>("Original/result_spot_red");
            resultSpotBlack = Resources.Load<Texture2D>("Original/result_spot_black");
            resultSpotGreen = Resources.Load<Texture2D>("Original/result_spot_green");
            topperSpotRed = Resources.Load<Texture2D>("Original/topper_spot_red");
            topperSpotBlack = Resources.Load<Texture2D>("Original/topper_spot_black");
            topperSpotGreen = Resources.Load<Texture2D>("Original/topper_spot_green");
            topperResultFont = OriginalBitmapFont.Load("topper_result");
            neighborPickFont = OriginalBitmapFont.Load("neighbor_pick");
            int savedNeighbor = PlayerPrefs.GetInt(NeighborSelectionKey, -1);
            selectedNeighbor = savedNeighbor >= 0 && savedNeighbor <= 36 ? savedNeighbor : -1;
            // NeighborsGadget::Restore shows/enables a saved number without
            // calling ValidateBets, as DoUndo does for the previous action.
            neighborEnabledWithoutPrecheck = selectedNeighbor >= 0;
            tiersButton = Resources.Load<Texture2D>("Original/TiersBtn");
            voisinsButton = Resources.Load<Texture2D>("Original/VoisinsBtn");
            orphelinsButton = Resources.Load<Texture2D>("Original/OrphelinsBtn");
            doubleBackground = Resources.Load<Texture2D>("Original/double_background");
            doubleRedButtonDisabled = Resources.Load<Texture2D>("Original/double_red_button_disabled");
            doubleBlackButtonDisabled = Resources.Load<Texture2D>("Original/double_black_button_disabled");
            doubleRedButtonFlash = Resources.Load<Texture2D>("Original/double_red_button_flash");
            doubleBlackButtonFlash = Resources.Load<Texture2D>("Original/double_black_button_flash");
            doubleBlackButton = Resources.Load<Texture2D>("Original/double_black_button");
            doubleTakeWinButton = Resources.Load<Texture2D>("Original/double_takewin_button");
            doubleTakeWinFlash = Resources.Load<Texture2D>("Original/double_takewin_flash");
            doubleCardback = Resources.Load<Texture2D>("Original/double_cardback");
            doubleRedCard = Resources.Load<Texture2D>("Original/double_red");
            doubleBlackCard = Resources.Load<Texture2D>("Original/double_black");
            doubleSuitCards = new[] { "heart", "diamond", "club", "spade" }
                .Select(name => Resources.Load<Texture2D>("Original/double_" + name)).ToArray();
            suitBackground = SuitBitmap("bSuitGambleBackground");
            suitCardback = SuitBitmap("bSuitGambleCardback");
            suitHelpBackground = SuitBitmap("bSuitGambleHelpscreen");
            suitRedCard = SuitBitmap("bSuitGambleRed");
            suitBlackCard = SuitBitmap("bSuitGambleBlack");
            suitWin = SuitBitmap("bSuitGambleWin");
            suitLose = SuitBitmap("bSuitGambleLose");
            string[] suitNames = { "SuitGambleRed", "SuitGambleBlack",
                "Heart", "Diamond", "Club", "Spade" };
            suitButtons = suitNames.Select(n => SuitBitmap("b" + n + "Button")).ToArray();
            suitButtonsFlash = suitNames.Select(n => SuitBitmap("b" + n + "ButtonFlash")).ToArray();
            suitButtonsDisabled = suitNames.Select(n => SuitBitmap("b" + n + "ButtonDisabled")).ToArray();
            suitDeckButtons = suitNames.Select(n => SuitBitmap("b" + n + "DeckButton")).ToArray();
            suitDeckButtonsFlash = suitNames.Select(n => SuitBitmap("b" + n + "DeckButtonFlash")).ToArray();
            suitDeckButtonsDisabled = suitNames.Select(n => SuitBitmap("b" + n + "DeckButtonDisabled")).ToArray();
            suitDeckBackground = SuitBitmap("bDeckBackground");
            suitTakeWinDeck = SuitBitmap("bSuitGambleTakeWinDeckButton");
            suitTakeWinDeckFlash = SuitBitmap("bSuitGambleTakeWinDeckButtonFlash");
            // fSuitGambleMaximumAward names this bitmap, but the packaged
            // featdefs and IMG do not contain it. Do not substitute the
            // red/black award bitmap for a different source feature.
            suitMaximumAward = SuitBitmap("bSuitGambleMaximumAward");
            suitHelpRounds = new Texture2D[17];
            // Resolve the exact graph names. The packaged featdefs has only
            // differently named candidate images; the original bitmap manager
            // skips an unresolved name instead of substituting similar art.
            for (int rounds = 2; rounds <= 16; ++rounds)
                suitHelpRounds[rounds] = SuitBitmap(
                    "bTxtNumRoundsSG" + rounds.ToString("D2"));
            doubleCardContainer = Resources.Load<Texture2D>("Original/double_card_container");
            doublePrompt = Resources.Load<Texture2D>("Original/double_prompt");
            doublePrompt2x = Resources.Load<Texture2D>("Original/double_prompt_2x");
            doublePrompt4x = Resources.Load<Texture2D>("Original/double_prompt_4x");
            doubleWin = Resources.Load<Texture2D>("Original/double_win");
            doubleLose = Resources.Load<Texture2D>("Original/double_lose");
            doubleMaximumAward = Resources.Load<Texture2D>("Original/double_maximum_award");
            doubleWinSound = Resources.Load<AudioClip>("Audio/DoubleUp/win");
            doubleLoseSound = Resources.Load<AudioClip>("Audio/DoubleUp/lose");
            doubleButtonPressSound = Resources.Load<AudioClip>("Audio/DoubleUp/button_press");
            doubleHelpBackground = Resources.Load<Texture2D>("Original/double_help_background");
            doubleExitButton = Resources.Load<Texture2D>("Original/double_exit_button");
            doubleHelpRounds = new Texture2D[17];
            for (int rounds = 2; rounds <= 16; ++rounds)
                doubleHelpRounds[rounds] = Resources.Load<Texture2D>(
                    "Original/DoubleHelpRounds/" + rounds.ToString("D2"));
            footerPanel = Resources.Load<Texture2D>("Original/ProView_PanelBG");
            footerCreditMeterButton = Resources.Load<Texture2D>(
                "Original/ProView_CreditMeterButton");
            footerCashMeterLabel = Resources.Load<Texture2D>(
                "Original/ProView_CashMeterLabel");
            footerCashBetMeterLabel = Resources.Load<Texture2D>(
                "Original/ProView_CashBetMeterLabel");
            footerCashPaidMeterLabel = Resources.Load<Texture2D>(
                "Original/ProView_CashPaidMeterLabel");
            footerDenomTouchBar = Resources.Load<Texture2D>(
                "Original/ProView_DenomDecalTouchToChange");
            footerDenomTouchButton = Resources.Load<Texture2D>(
                "Original/ProView_DenomDecalTouchToChangeButton");
            footerInfoButton = Resources.Load<Texture2D>("Original/ProView_GameInfoButton");
            footerInfoTop = Resources.Load<Texture2D>("Original/ProView_GameInfoTop");
            footerInfoBottom = Resources.Load<Texture2D>("Original/ProView_GameInfoBottom");
            footerSeePaysButton = Resources.Load<Texture2D>("Original/ProView_SeePaysButton");
            footerVolumeButton = Resources.Load<Texture2D>("Original/ProView_VolumeLevelControl");
            footerMeterFont = Resources.Load<Texture2D>("Original/ProView_CashMeterFont1");
            footerPaidFont = OriginalBitmapFont.Load("winpaid");
            footerCreditMeterFonts = new[] { OriginalBitmapFont.Load("cash_meter"),
                OriginalBitmapFont.Load("cash_meter_small") };
            footerPaidMeterFonts = new[] { footerPaidFont,
                OriginalBitmapFont.Load("winpaid_small") };
            footerDenomFont = OriginalBitmapFont.Load("denom");
            footerTitleFont = OriginalBitmapFont.Load("onecredit");
            footerCreditTitleFont = OriginalBitmapFont.Load("cashbet");
            footerCashCreditFont = OriginalBitmapFont.Load("cash_credit");
            footerCashPaidFont = OriginalBitmapFont.Load("cash_paid");
            footerWinTitleFont = OriginalBitmapFont.Load("wincollect");
            footerCreditTitleFonts = new[] { footerCreditTitleFont,
                OriginalBitmapFont.Load("cashbet_narrow") };
            footerWinTitleFonts = new[] { footerWinTitleFont,
                OriginalBitmapFont.Load("wincollect_narrow") };
            footerDenomFonts = new[] { footerDenomFont,
                OriginalBitmapFont.Load("denom_small") };
            footerOneCreditFonts = new[] { footerTitleFont,
                OriginalBitmapFont.Load("onecredit_alt") };
            footerMessageFont = OriginalBitmapFont.Load("message");
            footerTouchFont = OriginalBitmapFont.Load("touch_to_change");
            footerDisclaimerFont = OriginalBitmapFont.Load("malfunction");
            footerGamePlayFrames = Resources.LoadAll<Texture2D>("Animations/GamePlayFooter")
                .OrderBy(texture => texture.name).ToArray();
            footerGamePlayIntroFrames = Resources.LoadAll<Texture2D>("Animations/GamePlayFooterIntro")
                .OrderBy(texture => texture.name).ToArray();
            footerGamePlayOutroFrames = Resources.LoadAll<Texture2D>("Animations/GamePlayFooterOutro")
                .OrderBy(texture => texture.name).ToArray();
            StartFooterMovie();
            toteWhiteFont = OriginalBitmapFont.Load("tote_white");
            toteRedFont = OriginalBitmapFont.Load("tote_red");
            toteGreenFont = OriginalBitmapFont.Load("tote_green");
            toteYellowFont = OriginalBitmapFont.Load("tote_yellow");
            pressSpinFrames = Resources.LoadAll<Texture2D>("Animations/PressSpinIn")
                .OrderBy(texture => texture.name).ToArray();
            pressSpinOutFrames = Resources.LoadAll<Texture2D>("Animations/PressSpinOut")
                .OrderBy(texture => texture.name).ToArray();
            gameOverFrames = Resources.LoadAll<Texture2D>("Animations/GameOverIn")
                .OrderBy(texture => texture.name).ToArray();
            gameOverOutFrames = Resources.LoadAll<Texture2D>("Animations/GameOverOut")
                .OrderBy(texture => texture.name).ToArray();
            ideckPanel = Resources.Load<Texture2D>("Original/ideck_panel");
            ideckAutoPlay = Resources.Load<Texture2D>("Original/ideck_autoplay");
            ideckDoubleUp = Resources.Load<Texture2D>("Original/ideck_doubleup");
            ideckTakeWin = Resources.Load<Texture2D>("Original/ideck_takewin");
            ideckService = Resources.Load<Texture2D>("Original/ideck_service");
            ideckCashout = Resources.Load<Texture2D>("Original/ideck_cashout");
            ideckAutoConfirm = Resources.Load<Texture2D>("Original/ideck_auto_confirm");
            ideckServiceConfirm = Resources.Load<Texture2D>("Original/ideck_service_confirm");
            ideckCollectConfirm = Resources.Load<Texture2D>("Original/ideck_collect_confirm");
            ideckAutoYes = Resources.Load<Texture2D>("Original/ideck_auto_yes");
            ideckAutoNo = Resources.Load<Texture2D>("Original/ideck_auto_no");
            ideckDoublePanel = Resources.Load<Texture2D>("Original/ideck_double_panel");
            ideckRedDeck = Resources.Load<Texture2D>("Original/ideck_red_deck");
            ideckBlackDeck = Resources.Load<Texture2D>("Original/ideck_black_deck");
            ideckRedDeckFlash = Resources.Load<Texture2D>("Original/ideck_red_deck_flash");
            ideckBlackDeckFlash = Resources.Load<Texture2D>("Original/ideck_black_deck_flash");
            ideckTakeWinDeckFlash = Resources.Load<Texture2D>("Original/ideck_takewin_flash");
            ideckExitTakeWin = Resources.Load<Texture2D>("Original/double_exit_takewin_disabled");
            ideckExitRed = Resources.Load<Texture2D>("Original/double_exit_red_disabled");
            ideckExitBlack = Resources.Load<Texture2D>("Original/double_exit_black_disabled");
            ideckChips = Enumerable.Range(1, 4).Select(i =>
                Resources.Load<Texture2D>("Original/ideck_betper0" + i)).ToArray();
            paysPages = Enumerable.Range(1, 4).Select(i =>
                Resources.Load<Texture2D>("Original/pays_0" + i)).ToArray();
            paysLinePages = new[] { "89", "93", "97" }.Select(suffix =>
                Resources.Load<Texture2D>("Original/pays_lines_" + suffix)).ToArray();
            paysBack = Resources.Load<Texture2D>("Original/pays_back");
            paysExit = Resources.Load<Texture2D>("Original/pays_exit");
            paysNext = Resources.Load<Texture2D>("Original/pays_next");
            if (originalLayout == null || originalLayout.bet_spots == null ||
                originalLayout.bet_spots.Length != 157 || originalBackground == null)
                Debug.LogError("Original portrait screen resources are incomplete");
        }

        private void PrepareOriginalStyles()
        {
            if (originalWhite != null) return;
            originalWhite = new GUIStyle(GUI.skin.label) {
                fontSize = 46, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            originalWhite.normal.textColor = Color.white;
            originalSmall = new GUIStyle(originalWhite) { fontSize = 21 };
            originalNumber = new GUIStyle(originalWhite) { fontSize = 56 };
            originalMeter = new GUIStyle(originalWhite) { fontSize = 30 };
            ideckButtonText = new GUIStyle(originalWhite) { fontSize = 13 };
            invisibleButton = new GUIStyle(GUIStyle.none);
        }

        private void DrawOriginalScreen()
        {
            PrepareOriginalStyles();
            float scale = Mathf.Min(Screen.width / PortraitWidth, Screen.height / PortraitHeight);
            float ox = (Screen.width - PortraitWidth * scale) * 0.5f;
            float oy = (Screen.height - PortraitHeight * scale) * 0.5f;
            Matrix4x4 prior = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(ox, oy, 0), Quaternion.identity,
                new Vector3(scale, scale, 1));
            GUI.DrawTexture(new Rect(0, 0, PortraitWidth, PortraitHeight), originalBackground);
            if (session != null)
            {
                DrawOriginalWheel();
                DrawOriginalHistory();
                DrawOriginalBetChips();
                DrawOriginalControls();
                DrawOriginalPrompts();
                DrawOriginalMeters();
                if (DoubleFeaturePresentationVisible) DrawOriginalDouble();
                if (Time.realtimeSinceStartup < feedbackUntil && !session.InDoubleUp &&
                    !IsDoubleReveal && !IsDoubleExit)
                    GUI.Label(new Rect(80, 1711, 920, 29), status, originalSmall);
            }
            else GUI.Label(new Rect(180, 900, 720, 80), status, originalSmall);
            GUI.matrix = prior;
        }

        private void SetIDeckPreview(bool enabled)
        {
            ideckAttractPreview = false;
            StopPcIDeckAttractMovies();
            if (ideckSecondDisplay) return;
            ideckPreview = enabled;
            if (enabled) { topperPreview = false; histogramPreview = false; }
            RefreshTopperTarget();
        }

        private void SetTopperPreview(bool enabled)
        {
            topperPreview = enabled;
            // Preview visibility does not restart an already running Screen 4
            // attract feature or its source-relative DDA frame clock.
            topperAttractPreview = topperAttractAutomatic;
            if (!topperAttractPreview) StopPcMovie(ref pcTopperAttractMovie);
            if (enabled) { ideckPreview = false; histogramPreview = false; }
            RefreshTopperTarget();
        }

        private void SetTopperAttractPreview()
        {
            SetTopperPreview(true);
            topperAttractAutomatic = false;
            topperAttractPreview = true;
            topperAttractStartedTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            StartPcTopperAttractMovie();
        }

        private void DrawTopperPreview()
        {
            if (topperFrames == null || topperFrames.Length == 0) return;
            float scale = Mathf.Min(Screen.width / 1360f, Screen.height / 768f);
            Matrix4x4 prior = GUI.matrix;
            Color priorColor = GUI.color;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = priorColor;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3((Screen.width - 1360f * scale) * .5f,
                    (Screen.height - 768f * scale) * .5f, 0),
                Quaternion.identity, new Vector3(scale, scale, 1));
            int frame = TopperAttractFrameIndex();
            GUI.DrawTexture(new Rect(48, 31, 1276, 348), topperFrames[frame]);
            GUI.matrix = prior;
        }

        private void DrawIDeckPreview()
        {
            if (ideckPanel == null || session == null) return;
            PrepareOriginalStyles();
            Color priorColor = GUI.color;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = priorColor;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 242f);
            Matrix4x4 prior = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(
                new Vector3((Screen.width - 1280f * scale) * .5f,
                    (Screen.height - 242f * scale) * .5f, 0),
                Quaternion.identity, new Vector3(scale, scale, 1));
            GUI.DrawTexture(new Rect(0, 0, 1280, 242), ideckPanel);
            if (ideckService != null)
                GUI.DrawTexture(new Rect(39, 12, 105, 82), ideckService);
            if (ideckCashout != null)
                GUI.DrawTexture(new Rect(47, 157, 89, 66), ideckCashout);
            GUI.Label(new Rect(39, 12, 105, 82), "SERVICE", ideckButtonText);
            GUI.Label(new Rect(47, 157, 89, 66), "CASHOUT", ideckButtonText);
            if (SourceDoubleOfferActive || !SourceRoundBlocksOrdinaryInput &&
                session.DoubleAvailable && DoubleUpInputAllowed && !spinning &&
                !ResultDisplayActive)
            {
                OriginalDeckButton(new Rect(294, 37, 168, 168), ideckTakeWin,
                    "TAKE WIN", () => {
                        if (SourceDoubleOfferActive) TakeSourceDoubleOffer();
                        else Try(() => session.NextRound());
                        SetIDeckPreview(false);
                    });
                OriginalDeckButton(new Rect(711, 37, 168, 168), ideckDoubleUp,
                    "DOUBLE UP", () => {
                        Try(BeginDouble);
                        SetIDeckPreview(false);
                    });
            }
            else
            {
                GUI.Label(new Rect(352, 1, 674, 23), "Select a Chip Value", originalSmall);
                int[] x = { 283, 491, 700, 907 };
                for (int i = 0; i < 4; ++i)
                {
                    int chip = chips[i];
                    Texture2D texture = ideckChips[i];
                    Rect rect = new Rect(x[i], 29, 188, 188);
                    if (texture != null) GUI.DrawTexture(rect, texture);
                    if (!SourceRoundBlocksOrdinaryInput &&
                        GUI.Button(rect, GUIContent.none, invisibleButton))
                    {
                        Try(() => session.SetChip(chip));
                        SetIDeckPreview(false);
                    }
                }
            }
            if (ideckAutoPlay != null)
            {
                Rect rect = new Rect(47, 88, 89, 66);
                GUI.DrawTexture(rect, ideckAutoPlay);
                GUI.Label(rect, "AUTO", ideckButtonText);
                if (GUI.Button(rect, GUIContent.none, invisibleButton))
                {
                    RequestAutoPlay();
                    SetIDeckPreview(false);
                }
            }
            GUI.matrix = prior;
        }

        private void OriginalDeckButton(Rect rect, Texture2D texture, string label, Action action)
        {
            if (texture != null) GUI.DrawTexture(rect, texture);
            GUI.Label(rect, label, originalSmall);
            if (GUI.Button(rect, GUIContent.none, invisibleButton)) action();
        }

        private void DrawOriginalWheel()
        {
            if (wheel != null)
            {
                Matrix4x4 prior = GUI.matrix;
                GUIUtility.RotateAroundPivot(-(float)wheelRenderedAngle,
                    new Vector2(472.5f, 383.5f));
                GUI.DrawTexture(originalLayout.wheel_main.ToRect(), wheel);
                GUI.matrix = prior;
            }
            if (wheelShine != null)
                GUI.DrawTexture(new Rect(200, 112, 544, 543), wheelShine);
            if (ball != null && ((spinning && wheelBallVisible) ||
                (!spinning && session.LastResult != null)))
            {
                int pocket = spinning ? session.PendingPocket : session.LastResult.Pocket;
                Vector2 position = OriginalBallPosition(pocket);
                GUI.DrawTexture(new Rect(position.x - 12f, position.y - 12f, 24f, 24f), ball);
            }
            if (spindle != null) GUI.DrawTexture(originalLayout.wheel_spindle.ToRect(), spindle);
            if (wheelShadow != null)
                GUI.DrawTexture(new Rect(145, 41, 657, 231), wheelShadow);
        }

        private Vector2 OriginalBallPosition(int pocket)
        {
            if (wheelMotionReady) return wheelBallPosition;
            // The landing-path index is a separate C-rand visual choice. It
            // cannot be reconstructed from the winning pocket after restart.
            // Position the settled ball at the pocket on the retained wheel.
            return BallCenterForPocket(pocket, wheelRenderedAngle);
        }

        private void DrawOriginalHistory()
        {
            int first = spinning ? 1 : 0;
            if (recordedPockets.Count <= first) return;
            int last = recordedPockets[first];
            if (toteMeterBoxFrames != null && toteMeterBoxFrames.Length == 3)
                GUI.DrawTexture(new Rect(914, 34, 114, 79),
                    toteMeterBoxFrames[ToteMeterBoxFrame(last)]);
            if (toteWhiteFont != null)
                toteWhiteFont.Draw(last.ToString(), new Rect(918, 38, 106, 70));
            else GUI.Label(new Rect(918, 38, 106, 70), last.ToString(), originalNumber);
            GUI.Label(new Rect(775, 755, 240, 92), last.ToString(), originalNumber);
            for (int i = first + 1; i < recordedPockets.Count && i < first + 20; ++i)
            {
                int pocket = recordedPockets[i];
                OriginalBitmapFont font = pocket == 0 ? toteGreenFont :
                    RedNumbers.Contains(pocket) ? toteRedFont : toteYellowFont;
                Rect bounds = new Rect(915, 121 + (i - first - 1) * 32, 112, 29);
                if (font != null) font.Draw(pocket.ToString(), bounds);
                else GUI.Label(bounds, pocket.ToString(), originalSmall);
            }
        }

        private bool PressSpinVisible
        {
            get { return session != null && session.TotalBet > 0 && !spinning &&
                         (!SourceRoundBlocksOrdinaryInput || gameOverMessageLatched) &&
                         !session.InDoubleUp && !IsDoubleReveal &&
                         Time.realtimeSinceStartup >= pressSpinAt &&
                         Time.realtimeSinceStartup < pressSpinAt + 3.4f; }
        }

        private void DrawOriginalPrompts()
        {
            if (SourceRoundBlocksOrdinaryInput && !gameOverMessageLatched) return;
            float now = Time.realtimeSinceStartup;
            if (!spinning && gameOverAt > 0 && now < gameOverAt + 2.5f)
            {
                DrawOriginalPromptSequence(gameOverFrames, gameOverOutFrames,
                    now - gameOverAt, 1.15f);
            }
            else if (PressSpinVisible)
            {
                DrawOriginalPromptSequence(pressSpinFrames, pressSpinOutFrames,
                    now - pressSpinAt, 2f);
            }
        }

        private static void DrawOriginalPromptSequence(Texture2D[] intro, Texture2D[] outro,
            float elapsed, float holdSeconds)
        {
            if (intro == null || intro.Length == 0 || outro == null || outro.Length == 0)
                return;
            float introSeconds = intro.Length * .042f;
            Texture2D[] sequence = elapsed < introSeconds + holdSeconds ? intro : outro;
            int frame = sequence == intro ?
                Mathf.Clamp(Mathf.FloorToInt(elapsed / .042f), 0, intro.Length - 1) :
                Mathf.Clamp(Mathf.FloorToInt((elapsed - introSeconds - holdSeconds) / .042f),
                    0, outro.Length - 1);
            GUI.DrawTexture(new Rect(752, 938, 310, 110), sequence[frame]);
        }

        private void DrawOriginalBetChips()
        {
            foreach (var bet in session.Bets
                .Where(pair => pair.Key >= 100 &&
                    pair.Key < 100 + originalLayout.bet_spots.Length)
                .OrderBy(pair => originalLayout.bet_spots[pair.Key - 100].z)
                .ThenBy(pair => pair.Key))
            {
                int index = bet.Key - 100;
                if (index < 0 || index >= originalLayout.bet_spots.Length) continue;
                Rect spot = originalLayout.bet_spots[index].ToRect();
                float cx = spot.x + spot.width / 2f, cy = spot.y + spot.height / 2f;
                Texture2D chip = originalPlacedChips[ClosestBetSelector(bet.Value)];
                if (chip != null) GUI.DrawTexture(new Rect(cx - 36, cy - 36, 72, 72), chip);
                GUI.Label(new Rect(cx - 30, cy - 18, 60, 36), bet.Value.ToString(), originalSmall);
            }
            int visibleWinningPayId = VisibleWinningPayId();
            if (betWinHighlight != null && visibleWinningPayId >= 0)
            {
                foreach (var bet in session.Bets.OrderBy(pair => pair.Key))
                {
                    int index = bet.Key - 100;
                    if (index < 0 || index >= originalLayout.bet_spots.Length ||
                        bet.Key != visibleWinningPayId)
                        continue;
                    Rect spot = originalLayout.bet_spots[index].ToRect();
                    float cx = spot.x + spot.width / 2f, cy = spot.y + spot.height / 2f;
                    GUI.DrawTexture(new Rect(cx - 59.5f, cy - 54f, 119f, 108f),
                        betWinHighlight);
                }
            }
            if (!spinning && !session.InDoubleUp && !IsDoubleReveal)
            {
                foreach (int picked in originalBetHitOrder)
                {
                    if (!GUI.Button(originalLayout.bet_spots[picked].ToRect(),
                            GUIContent.none, invisibleButton)) continue;
                    if (picked < 37) Number(picked);
                    else Bet(100 + picked);
                    break;
                }
            }
        }

        private void OriginalImageButton(OriginalRect box, Texture2D image, Action action)
        {
            if (box == null || image == null) return;
            Rect rect = box.ToRect();
            GUI.DrawTexture(rect, image);
            if (!SourceRoundBlocksOrdinaryInput && !session.InDoubleUp && !IsDoubleReveal &&
                GUI.Button(rect, GUIContent.none, invisibleButton)) action();
        }

        private void DrawOriginalControls()
        {
            int[] values = { 1, 2, 5, 10 };
            for (int i = 0; i < 4; ++i)
            {
                int value = values[i];
                OriginalImageButton(originalLayout.chip_buttons[i], originalChips[i],
                    () => Try(() => session.SetChip(value)));
                if (session.Chip == value)
                {
                    Rect rect = originalLayout.chip_buttons[i].ToRect();
                    GUI.Label(new Rect(rect.x, rect.y + 78, rect.width, 26), "▲", originalSmall);
                }
            }
            OriginalImageButton(originalLayout.undo_button, originalUndo,
                () => TrySourceTableEvent(UndoBet, betUndoSound, undoEvent: true));
            OriginalImageButton(originalLayout.clear_button, originalClear,
                () => TrySourceTableEvent(() => { if (session.ResultPhase) session.NextRound(); session.Clear(); }, betClearSound));

            if (neighborMode && neighborOn != null)
                GUI.DrawTexture(new Rect(777, 1123, 260, 260), neighborOn);
            else if (neighborOff != null)
                GUI.DrawTexture(new Rect(777, 1123, 260, 260), neighborOff);
            if (voisinsButton != null) GUI.DrawTexture(new Rect(743, 1075, 112, 108), voisinsButton);
            if (orphelinsButton != null) GUI.DrawTexture(new Rect(847, 1058, 120, 64), orphelinsButton);
            if (tiersButton != null) GUI.DrawTexture(new Rect(959, 1075, 113, 108), tiersButton);
            if (!SourceRoundBlocksOrdinaryInput && !session.InDoubleUp && !IsDoubleReveal &&
                GUI.Button(new Rect(795, 1160, 220, 220), GUIContent.none, invisibleButton))
                neighborMode = !neighborMode;

        }

        private void DrawOriginalMeters()
        {
            DrawFooterTexture(footerPanel, new Rect(0, 1740, 1080, 180));
            DrawFooterTexture(footerInfoButton, new Rect(134, 1816, 66, 62));
            DrawFooterText(footerTouchFont, "TOUCH TO CHANGE",
                new Rect(34, 1888, 83, 11), originalSmall);
            DrawFooterText(footerDisclaimerFont,
                "MALFUNCTION VOIDS ALL PAYS AND PLAYS",
                new Rect(293, 1884, 420, 11), originalSmall);
            // Source StateSpinDone clears the independent ProView flashing
            // message. The table GameOver animation is handled above.
            WinningPayRecord visiblePay = VisibleWinningPayRecord();
            string centerMessage = ProViewFlashingMessage();
            if (centerMessage.Length != 0)
                DrawFooterText(footerMessageFont,
                    centerMessage,
                    new Rect(402, 1774, 267, 28), originalSmall);
            if (visiblePay != null)
                DrawFooterText(footerMessageFont,
                    RoulettePayMessageFormatter.LinePays(
                        session.Profile.Records[visiblePay.BetId], visiblePay.Amount),
                    new Rect(108, 1774, 200, 28), originalSmall, -1);
            if (!currentGameResultsCleared && session.LastResult != null &&
                (!session.ResultPhase || session.LastResult.Returned == 0 || visiblePay != null))
                DrawFooterText(footerMessageFont,
                    "Game Pays " + (session.ResultPhase && visiblePay != null ?
                        session.LastResult.Returned : DisplayedPaid),
                    new Rect(770, 1774, 200, 28), originalSmall, 1);
            DrawProViewText(footerOneCreditFonts, "1 CREDIT", new Rect(43, 1815, 77, 13), originalSmall);
            DrawProViewText(footerCreditTitleFonts, "CREDITS", new Rect(337, 1815, 88, 15), originalSmall, -1);
            DrawProViewText(footerCreditTitleFonts, "BET", new Rect(549, 1815, 72, 15), originalSmall, -1);
            DrawProViewText(footerWinTitleFonts, "WIN", new Rect(732, 1815, 88, 15), originalSmall, -1);
            DrawProViewText(footerDenomFonts,
                RouletteDenominationFormatter.ForFilmedCabinet(session.Denomination),
                new Rect(35, 1828, 90, 59), originalMeter);
            DrawOriginalMeterDigits(DisplayedCredit.ToString(), new Rect(350, 1832, 188, 45));
            DrawOriginalMeterDigits(session.TotalBet.ToString(), new Rect(569, 1832, 149, 45));
            bool showPaid = SourceAuIdlePaidVisible || (!spinning &&
                (session.ResultPhase || currentGameResultsCleared && auNationalStandardsEnabled) &&
                session.LastResult != null && (DisplayedPaid > 0 || auNationalStandardsEnabled));
            string footerMessage = auNationalStandardsEnabled ?
                (showPaid ? DisplayedPaid.ToString() : "") :
                spinning || !session.ResultPhase || session.LastResult == null ||
                session.LastResult.Returned == 0 ? "GOOD LUCK!" :
                currentGameResultsCleared ? "" : DisplayedPaid.ToString();
            Texture2D footerFrame = CurrentFooterMovieFrame();
            if ((footerMessage == "GOOD LUCK!" || currentGameResultsCleared) && footerFrame != null)
                GUI.DrawTexture(new Rect(775, 1823, 258, 59), footerFrame);
            else if (footerMessageFont != null && footerMessage == "GOOD LUCK!")
                footerMessageFont.Draw(footerMessage, new Rect(782, 1833, 252, 54), nativeSize: true);
            else if (footerPaidFont != null && footerMessage != "GOOD LUCK!")
                DrawProViewText(footerPaidMeterFonts, footerMessage,
                    new Rect(782, 1833, 252, 54), originalSmall);
            else GUI.Label(new Rect(782, 1833, 252, 54), footerMessage, originalSmall);
        }

        private void StartFooterMovie()
        {
            StartFooterMovieAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void StartFooterMovieAt(long now, long frequency)
        {
            // ProView's Play/ShowGamePlayAnimation receiver returns before
            // touching its sprites when ShowOnlyGameWinsInWinMeter (AU) is
            // enabled. This gate also covers startup, table edits and Spin.
            if (auNationalStandardsEnabled) return;
            // Native idle/bet UpdateMeters and RestoreFromNV retain collect
            // instead of starting the gameplay movie while +0x25c is nonzero.
            if (PcCollectMeterVisible) { footerMoviePhase = FooterMoviePhase.Hidden; return; }
            // PlayGamePlayAnimation does not replay an already visible Intro.
            if (footerMoviePhase == FooterMoviePhase.Intro &&
                footerMovieClock != null && (footerMovieClock.Playing || footerMovieClock.Paused)) return;
            footerMoviePhase = footerGamePlayIntroFrames.Length > 0 ?
                FooterMoviePhase.Intro : FooterMoviePhase.Loop;
            Texture2D[] frames = footerMoviePhase == FooterMoviePhase.Intro ?
                footerGamePlayIntroFrames : footerGamePlayFrames;
            footerMovieClock = frames.Length > 0 ? new OriginalFooterMovieClock(
                frames.Length, footerMoviePhase == FooterMoviePhase.Loop, now, frequency) : null;
            TrackPcFooterMovieInstance();
        }

        private void StopFooterMovie()
        {
            StopFooterMovieAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency,
                System.Diagnostics.Stopwatch.GetTimestamp);
        }

        private void StopFooterMovieAt(long now, long frequency,
            Func<long> paidArmClock = null)
        {
            if (footerMoviePhase == FooterMoviePhase.Hidden ||
                footerMoviePhase == FooterMoviePhase.Outro || footerMovieClock == null) return;
            footerMoviePhase = footerGamePlayOutroFrames.Length > 0 ?
                FooterMoviePhase.Outro : FooterMoviePhase.Hidden;
            footerMovieClock = footerMoviePhase == FooterMoviePhase.Outro ?
                new OriginalFooterMovieClock(footerGamePlayOutroFrames.Length,
                    false, now, frequency) : null;
            TrackPcFooterMovieInstance();
            // ProView resets the two numeric labels only when this Stop
            // transitions a visible Intro/Loop to Outro and alpha > 0.999.
            // Neither repeated Stop nor Play cancels an existing fade.
            if (footerGamePlayOutroFrames.Length > 0 && uiPaid != null)
            {
                if (paidArmClock != null)
                    paidFadeClock.TryStartAt(now, frequency, ApplyPaidOpacity, paidArmClock);
                else if (paidFadeClock.TryStartAt(now, frequency))
                    ApplyPaidOpacity(PaidOpacity);
            }
        }

        private Texture2D CurrentFooterMovieFrame()
        {
            return CurrentFooterMovieFrameAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void RefreshFooterMovieUiAt(long now, long frequency)
        {
            uiFooterMovie.texture = CurrentFooterMovieFrameAt(now, frequency);
            uiFooterMovie.gameObject.SetActive(uiFooterMovie.texture != null);
        }

        private Texture2D CurrentFooterMovieFrameAt(long now, long frequency)
        {
            if (footerMovieClock == null || footerMoviePhase == FooterMoviePhase.Hidden)
                return null;
            int frame = footerMovieClock.UpdateAt(now, frequency);
            if (footerMoviePhase == FooterMoviePhase.Intro)
            {
                if (footerMovieClock.Playing || footerMovieClock.Paused)
                    return footerGamePlayIntroFrames[Math.Max(0, frame)];
                footerMoviePhase = FooterMoviePhase.Loop;
                // IntroAnimationComplete starts the separate loop movie.
                // PlayNow samples its own current microsecond clock. A late
                // callback does not spend the delay in the new loop.
                footerMovieClock = footerGamePlayFrames.Length > 0 ?
                    new OriginalFooterMovieClock(footerGamePlayFrames.Length,
                        true, now, frequency) : null;
                TrackPcFooterMovieInstance();
                if (footerMovieClock == null) return null;
                frame = footerMovieClock.UpdateAt(now, frequency);
            }
            if (footerMoviePhase == FooterMoviePhase.Loop && footerGamePlayFrames.Length > 0)
                return footerGamePlayFrames[Math.Max(0, frame)];
            if (footerMoviePhase == FooterMoviePhase.Outro)
            {
                if (footerMovieClock.Playing || footerMovieClock.Paused)
                    return footerGamePlayOutroFrames[Math.Max(0, frame)];
                footerMoviePhase = FooterMoviePhase.Hidden;
                footerMovieClock = null;
                TrackPcFooterMovieInstance();
            }
            return null;
        }

        private static void DrawFooterTexture(Texture2D texture, Rect rect)
        {
            if (texture != null) GUI.DrawTexture(rect, texture);
        }

        private static void DrawFooterText(OriginalBitmapFont font, string value,
            Rect rect, GUIStyle fallback, int horizontalAlign = 0)
        {
            if (font != null) font.Draw(value, rect, horizontalAlign, nativeSize: true);
            else
            {
                TextAnchor prior = fallback.alignment;
                fallback.alignment = horizontalAlign < 0 ? TextAnchor.MiddleLeft :
                    horizontalAlign > 0 ? TextAnchor.MiddleRight : TextAnchor.MiddleCenter;
                GUI.Label(rect, value, fallback);
                fallback.alignment = prior;
            }
        }

        private void DrawOriginalMeterDigits(string value, Rect bounds)
        {
            DrawProViewText(footerCreditMeterFonts, value, bounds, originalMeter);
        }

        private static void DrawProViewText(OriginalBitmapFont[] fonts, string value,
            Rect bounds, GUIStyle fallback, int horizontalAlign = 0)
        {
            OriginalBitmapFont selected = OriginalBitmapFont.SelectForWidth(fonts, value, bounds.width);
            DrawFooterText(selected, value, bounds, fallback, horizontalAlign);
        }

        private Texture2D DoubleHelpRoundsBitmap()
        {
            int count = session == null ? 0 : session.DoubleMaximumRounds;
            return doubleHelpRounds != null && count >= 2 && count < doubleHelpRounds.Length ?
                doubleHelpRounds[count] : null;
        }

        private static Texture2D SuitBitmap(string name)
        {
            return Resources.Load<Texture2D>("Original/SuitGamble/" + name);
        }

        private static Rect DoubleHelpRoundsRect(int count, Texture2D bitmap)
        {
            return new Rect(DoubleHelpRoundX[count], 1249,
                bitmap.width, bitmap.height);
        }

        private void DrawOriginalDouble()
        {
            if (!session.InDoubleUp) showingDoubleHelp = false;
            if (IsDoubleExit)
            {
                if (doubleBackground != null)
                    GUI.DrawTexture(new Rect(0, 1920f * DoubleExitProgress, 1080, 1920),
                        doubleBackground);
                return;
            }
            if (doubleBackground != null)
                GUI.DrawTexture(new Rect(0, 0, 1080, 1920), doubleBackground);
            if (doubleRedButtonDisabled != null)
                GUI.DrawTexture(new Rect(60, 892, 310, 193), doubleRedButtonDisabled);
            if (doubleBlackButtonDisabled != null)
                GUI.DrawTexture(new Rect(707, 892, 310, 193), doubleBlackButtonDisabled);
            int redState = DoubleColorButtonState(-1);
            int blackState = DoubleColorButtonState(-2);
            if (redState == 2 && doubleRedButtonFlash != null)
                GUI.DrawTexture(new Rect(60, 892, 310, 193), doubleRedButtonFlash);
            if (blackState == 1 && doubleBlackButton != null)
                GUI.DrawTexture(new Rect(707, 892, 310, 193), doubleBlackButton);
            if (blackState == 2 && doubleBlackButtonFlash != null)
                GUI.DrawTexture(new Rect(707, 892, 310, 193), doubleBlackButtonFlash);
            Texture2D takeWinArt = DoubleTakeWinFlash ?
                doubleTakeWinFlash : doubleTakeWinButton;
            if (!showingDoubleHelp && !IsDoubleReveal &&
                (DoubleDecisionReady || doubleCollectPending) && takeWinArt != null)
                GUI.DrawTexture(new Rect(112, 1331,
                    takeWinArt.width, takeWinArt.height), takeWinArt);
            Texture2D message = IsDoubleReveal ?
                (doubleRevealMaximum ?
                    (session.DoubleSuitMode ? suitMaximumAward : doubleMaximumAward) :
                    (doubleRevealWon ? doubleWin : doubleLose)) : doublePrompt;
            Rect messageRect = IsDoubleReveal ?
                (doubleRevealMaximum ? new Rect(239, 686, 585, 70) :
                    (doubleRevealWon ? new Rect(302, 696, 483, 62) :
                        new Rect(392, 697, 299, 62))) :
                new Rect(247, 696, 589, 63);
            // fMessageBox kills fPrompt; this package has no bMessageBox image.
            if (message != null && !PcNativeDoubleFeatureAutoTake && (IsDoubleReveal ||
                CurrentDoubleIdleAnimationStep() >= 0))
                GUI.DrawTexture(messageRect, message);
            Texture2D shownCard = doubleCardback;
            if (IsDoubleReveal && session.LastDoubleCard >= 0)
            {
                int suit = OriginalDoubleSuit.ViewIndex(session.LastDoubleCard);
                shownCard = doubleRevealChoice < 0 ?
                    (suit < 2 ? doubleRedCard : doubleBlackCard) : doubleSuitCards[suit];
            }
            bool suitReveal = IsDoubleReveal && doubleRevealChoice >= 0;
            if (!suitReveal && shownCard != null)
                GUI.DrawTexture(new Rect(401, 807, 271, 370), shownCard);
            if (suitReveal && shownCard != null)
                GUI.DrawTexture(new Rect(428, 840, 224, 308), shownCard);
            GUI.Label(new Rect(33, 716, 205, 58), session.DoubleStake.ToString(), originalMeter);
            GUI.Label(new Rect(846, 716, 205, 58),
                (session.DoubleStake * 2).ToString(), originalMeter);
            GUI.Label(new Rect(67, 1219, 235, 56),
                RouletteDenominationFormatter.ForFeatureCurrency(
                    checked(session.Credit * session.Denomination)), originalSmall);
            GUI.Label(new Rect(348, 1219, 194, 56),
                (session.LastResult == null ? 0 : session.LastResult.Wager).ToString(), originalSmall);
            GUI.Label(new Rect(566, 1219, 168, 56),
                RouletteDenominationFormatter.ForFeatureCurrency(session.Denomination),
                originalSmall);
            GUI.Label(new Rect(779, 1219, 240, 56), session.DoubleStake.ToString(), originalSmall);
            if (IsDoubleReveal || doubleCollectPending) return;
            if (showingDoubleHelp)
            {
                if (!auNationalStandardsEnabled && doubleHelpBackground != null)
                {
                    GUI.DrawTexture(new Rect(0, 543, 1080, 1377), doubleHelpBackground);
                    Texture2D rounds = DoubleHelpRoundsBitmap();
                    if (rounds != null)
                        GUI.DrawTexture(DoubleHelpRoundsRect(session.DoubleMaximumRounds, rounds), rounds);
                }
                if (doubleExitButton != null)
                    GUI.DrawTexture(new Rect(465, 1331, 145, 66), doubleExitButton);
                if (GUI.Button(new Rect(466, 1331, 143, 60), GUIContent.none, invisibleButton))
                    CloseDoubleHelp(true);
                return;
            }
            if (GUI.Button(new Rect(60, 892, 310, 193), GUIContent.none, invisibleButton)) Guess(-1);
            if (GUI.Button(new Rect(707, 892, 310, 193), GUIContent.none, invisibleButton)) Guess(-2);
            if (GUI.Button(new Rect(112, 1331, 142, 60), GUIContent.none, invisibleButton))
                Try(BeginCollectDouble);
            if (GUI.Button(new Rect(828, 1332, 142, 60), GUIContent.none, invisibleButton))
                OpenDoubleHelp();
        }
    }
}
