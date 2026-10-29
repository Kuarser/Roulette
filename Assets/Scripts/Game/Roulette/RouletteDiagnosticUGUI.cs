using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private Canvas ideckCanvas, topperCanvas;
        private bool ideckSecondDisplay;
        private int ideckDisplayIndex = -1;
        private bool topperDedicatedDisplay;
        private int topperDisplayIndex = -1;
        private RawImage topperMovie;
        private bool topperAttractPreview;
        private bool topperAttractAutomatic;
        private long topperAttractEligibleTimestamp = -1;
        private long topperAttractStartedTimestamp;
        private RawImage topperResultSpot;
        private PictureLabel topperResultNumber;
        private GameObject ideckNormalControls, ideckChipControls, ideckDoubleControls;
        private Button ideckOfferTakeWinButton, ideckOfferDoubleUpButton;
        private GameObject ideckRedBlackControls, ideckAutoConfirmControls,
            ideckServiceConfirmControls, ideckCollectConfirmControls;
        private GameObject ideckSuitControls;
        private Button[] ideckSuitChoiceButtons;
        private Button ideckSuitTakeWin;
        private RawImage ideckTakeWinFlashOverlay, ideckSuitTakeWinFlashOverlay;
        private GameObject ideckExitPanel;
        private Button ideckRedButton, ideckBlackButton, ideckTakeWinButton,
            ideckAutoYesButton, ideckAutoNoButton,
            ideckServiceButton, ideckCashoutButton, ideckAutoPlayButton,
            ideckServiceYesButton, ideckServiceNoButton,
            ideckCollectYesButton, ideckCollectNoButton;
        private long autoConfirmStartedTimestamp, autoConfirmDeadlineTimestamp;
        private const long AutoConfirmMilliseconds = 15000;
        private bool serviceConfirmPending, pcServiceRequested;
        private long serviceConfirmStartedTimestamp, serviceConfirmDeadlineTimestamp;
        private const long ServiceConfirmMilliseconds = 15000;
        private bool collectConfirmPending;
        private long collectConfirmStartedTimestamp, collectConfirmDeadlineTimestamp;
        private const long CollectConfirmMilliseconds = 15000;
        private Button[] ideckChipButtons;
        private RawImage[] ideckChipTouchImages;
        private Texture2D[] ideckTouchFrames;
        private int ideckTouchChipIndex = -1;
        private long ideckTouchStartedTimestamp;
        private const long IDeckTouchFrameMilliseconds = 42;
        private bool ideckAttractPreview;
        private bool ideckAttractAutomatic;
        private float ideckAttractEligibleSince = -1f;
        private float ideckAttractStartedAt;
        private RawImage ideckAttractBackground, ideckAttractForeground;

        private void SetIDeckAttractPreview()
        {
            if (!ideckSecondDisplay) SetIDeckPreview(true);
            StartIDeckAttract(false);
        }

        private void StartIDeckAttract(bool automatic)
        {
            if (ideckAttractBackgroundFrames == null)
                ideckAttractBackgroundFrames = Resources.LoadAll<Texture2D>(
                    "Animations/IDeckAttractBackground").OrderBy(x => x.name).ToArray();
            if (ideckAttractForegroundFrames == null)
                ideckAttractForegroundFrames = Resources.LoadAll<Texture2D>(
                    "Animations/IDeckAttractForeground").OrderBy(x => x.name).ToArray();
            ideckAttractPreview = true;
            ideckAttractAutomatic = automatic;
            ideckAttractStartedAt = Time.realtimeSinceStartup;
            StartPcIDeckAttractMovies(System.Diagnostics.Stopwatch.GetTimestamp());
        }

        private void UpdateIDeckAutomaticAttract()
        {
            // Game::ActivateIDeckAttract arms AttractScreenInterval only in a
            // valid, zero-balance game state and disarms it on state change.
            bool eligible = session != null && session.Credit <= 0 &&
                session.Bets.Count == 0 && !spinning && !ResultDisplayActive &&
                !session.InDoubleUp && !IsDoubleReveal && !IsDoubleExit &&
                !autoplay && !autoConfirmPending && !serviceConfirmPending &&
                !collectConfirmPending && paysPage < 0 &&
                !showingDoubleHelp && !historyView;
            if (!eligible)
            {
                ideckAttractEligibleSince = -1f;
                if (ideckAttractAutomatic)
                {
                    ideckAttractAutomatic = false;
                    ideckAttractPreview = false;
                    StopPcIDeckAttractMovies();
                }
                return;
            }
            if (ideckAttractEligibleSince < 0f)
                ideckAttractEligibleSince = Time.realtimeSinceStartup;
            if (!ideckAttractPreview &&
                Time.realtimeSinceStartup - ideckAttractEligibleSince >= 8f)
                StartIDeckAttract(true);
        }

        private void UpdateTopperAutomaticAttract()
        {
            // Game::AttractUpdate/ActivateAttract use the packaged enabled
            // Attract option and filedefs.xml's 8000 ms interval. The game
            // screen feature runs even when Screen 4 is not attached.
            bool eligible = session != null && session.Credit <= 0 &&
                session.Bets.Count == 0 && !spinning && !ResultDisplayActive &&
                !session.InDoubleUp && !IsDoubleReveal && !IsDoubleExit &&
                !autoplay && !autoConfirmPending && !serviceConfirmPending &&
                !collectConfirmPending && paysPage < 0 &&
                !showingDoubleHelp && !historyView && !demoMenuVisible &&
                !pcCollectPending && !doubleCollectPending;
            if (!eligible)
            {
                topperAttractEligibleTimestamp = -1;
                if (topperAttractAutomatic)
                {
                    topperAttractAutomatic = false;
                    topperAttractPreview = false;
                    StopPcMovie(ref pcTopperAttractMovie);
                    Debug.Log("ROULETTE_TOPPER_ATTRACT_AUTO_STOP: result restored");
                }
                return;
            }
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            if (topperAttractEligibleTimestamp < 0)
                topperAttractEligibleTimestamp = now;
            if (!topperAttractPreview &&
                OriginalRackClock.ElapsedMilliseconds(topperAttractEligibleTimestamp,
                    now, System.Diagnostics.Stopwatch.Frequency) >= 8000)
            {
                topperAttractAutomatic = true;
                topperAttractPreview = true;
                topperAttractStartedTimestamp = now;
                StartPcTopperAttractMovie();
                Debug.Log("ROULETTE_TOPPER_ATTRACT_AUTO_START: 8000ms idle gate");
            }
        }

        private int TopperAttractFrameIndex()
        {
            if (topperFrames == null || topperFrames.Length == 0) return 0;
            return PcTopperAttractFrameAt(System.Diagnostics.Stopwatch.GetTimestamp());
        }

        private IEnumerator TopperAutomaticAttractSmoke()
        {
            yield return null;
            recordedPockets.Insert(0, 17);
            yield return null;
            if (!topperResultSpot.enabled || topperMovie.enabled)
                throw new InvalidOperationException("Topper did not preserve last result before attract");
            yield return new WaitForSecondsRealtime(7.7f);
            if (topperAttractPreview)
                throw new InvalidOperationException("Topper attract started before 8000 ms");
            yield return new WaitForSecondsRealtime(.5f);
            // The coroutine resumes before this frame's UI refresh.
            yield return null;
            if (!topperAttractAutomatic || !topperMovie.enabled ||
                topperResultSpot.enabled || topperResultNumber.GlyphCount != 0)
                throw new InvalidOperationException("Topper zero-credit attract did not start: " +
                    topperAttractAutomatic + "/" + topperMovie.enabled + "/" +
                    topperResultSpot.enabled + "/" + topperResultNumber.GlyphCount);
            long phaseOrigin = topperAttractStartedTimestamp;
            SetTopperPreview(false);
            yield return null;
            SetTopperPreview(true);
            yield return null;
            if (!topperAttractAutomatic || !topperAttractPreview ||
                topperAttractStartedTimestamp != phaseOrigin || !topperMovie.enabled)
                throw new InvalidOperationException("Topper preview restarted automatic DDA phase");
            string[] args = Environment.GetCommandLineArgs();
            int capture = Array.IndexOf(args, "-rouletteTopperAutoAttractCapture");
            if (capture >= 0 && capture + 1 < args.Length)
                CaptureActiveScreenOffscreen(args[capture + 1]);
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome());
            yield return new WaitForSecondsRealtime(.1f);
            if (topperAttractPreview || topperAttractAutomatic || topperMovie.enabled ||
                !topperResultSpot.enabled || topperResultNumber.Value != "17")
                throw new InvalidOperationException("Topper result did not resume after credit");
            recordedPockets.RemoveAt(0);
            SetTopperPreview(false);
            Debug.Log("ROULETTE_TOPPER_ATTRACT_AUTO_PASSED: 8000ms gate, DDA, result restore");
        }

        private IEnumerator IDeckAutomaticAttractSmoke()
        {
            yield return new WaitForSecondsRealtime(7.7f);
            if (ideckAttractPreview)
                throw new InvalidOperationException("iDeck attract started before 8000 ms");
            yield return new WaitForSecondsRealtime(.5f);
            if (!ideckAttractPreview || !ideckAttractAutomatic ||
                ideckNormalControls.activeSelf)
                throw new InvalidOperationException("iDeck zero-credit attract did not start");
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome());
            // The state update and the uGUI LateUpdate occur after a coroutine
            // resumes; allow the following render before checking the panel.
            yield return new WaitForSecondsRealtime(.1f);
            if (ideckAttractPreview || ideckAttractAutomatic ||
                !ideckNormalControls.activeSelf)
                throw new InvalidOperationException("iDeck attract did not stop on credit change");
            Debug.Log("ROULETTE_IDECK_ATTRACT_AUTO_PASSED: 8000ms gate and exit");
        }

        private void IDeckAttractFramesAt(float elapsed,
            out int backgroundFrame, out int foregroundFrame)
        {
            int backgroundLength = ideckAttractBackgroundFrames.Length;
            int foregroundLength = ideckAttractForegroundFrames.Length;
            int cycle = backgroundLength * 2;
            int tick = Mathf.FloorToInt(elapsed / IDeckAttractFrameSeconds) % cycle;
            backgroundFrame = tick % backgroundLength;
            // The source completion callback leaves the sprite visible. The
            // next group calls PlayAttractAnimation(false) and hides it.
            foregroundFrame = tick < backgroundLength ?
                Mathf.Min(tick, foregroundLength - 1) : -1;
        }

        private Canvas CreatePreviewCanvas(string name, Vector2 reference,
            out RectTransform root)
        {
            var host = new GameObject(name, typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = host.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 21;
            CanvasScaler scaler = host.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = reference;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            var blackout = new GameObject("Black preview surround", typeof(RectTransform),
                typeof(RawImage));
            RectTransform blackoutRect = blackout.GetComponent<RectTransform>();
            blackoutRect.SetParent(host.transform, false);
            blackoutRect.anchorMin = Vector2.zero;
            blackoutRect.anchorMax = Vector2.one;
            blackoutRect.offsetMin = blackoutRect.offsetMax = Vector2.zero;
            RawImage blackImage = blackout.GetComponent<RawImage>();
            blackImage.texture = Texture2D.whiteTexture;
            blackImage.color = Color.black;
            blackImage.raycastTarget = false;
            root = UiRect(host.GetComponent<RectTransform>(),
                new Rect(0, 0, reference.x, reference.y), name + " content");
            root.anchorMin = root.anchorMax = new Vector2(.5f, .5f);
            root.pivot = new Vector2(.5f, .5f);
            root.anchoredPosition = Vector2.zero;
            canvas.enabled = false;
            return canvas;
        }

        private void CreateDiagnosticUi()
        {
            if (portraitCanvas == null || ideckCanvas != null) return;
            string[] launchArgs = Environment.GetCommandLineArgs();
            // Explicit PC window/diagnostic mode. Do not ask the desktop
            // driver to open additional display surfaces in this mode.
            bool singleDisplayRequested = Array.IndexOf(launchArgs,
                "-rouletteSingleDisplay") >= 0;
            if (Array.IndexOf(launchArgs, "-rouletteDisplayRouteSmoke") >= 0)
                ValidateDisplayRouting();
            RectTransform deck;
            ideckCanvas = CreatePreviewCanvas("Original iDeck uGUI",
                new Vector2(1280, 242), out deck);
#if !UNITY_EDITOR
            // Explicit iDeck previews and primary-screen raycast checks stay
            // on one display; activating another display changes Unity's
            // interpretation of synthetic screen coordinates on this host.
            bool singleDisplaySmoke = Array.IndexOf(launchArgs,
                "-roulettePaysSmoke") >= 0 ||
                Array.IndexOf(launchArgs,
                    "-rouletteIdeckSmoke") >= 0 ||
                Array.IndexOf(launchArgs,
                    "-rouletteCollectRaycastSmoke") >= 0 ||
                Array.IndexOf(launchArgs,
                    "-rouletteAutoConfirmSmoke") >= 0;
            if (IDeckOutputIndex(Display.displays.Length) >= 0 &&
                !ideckPreview && !topperPreview &&
                !singleDisplaySmoke && !singleDisplayRequested)
            {
                ideckDisplayIndex = IDeckOutputIndex(Display.displays.Length);
                if (Array.IndexOf(launchArgs, "-rouletteDisplayRouteCapture") >= 0)
                    StartCoroutine(ActivateCaptureDeckAfterPrimaryFrame());
                else
                {
                    Display.displays[ideckDisplayIndex].Activate();
                    ideckCanvas.targetDisplay = ideckDisplayIndex;
                    ideckSecondDisplay = true;
                }
            }
#endif
            UiImage(deck, new Rect(0, 0, 1280, 242), ideckPanel, "iDeck panel");
            RectTransform normal = UiRect(deck, new Rect(0, 0, 1280, 242), "Betting controls");
            ideckNormalControls = normal.gameObject;
            ideckServiceButton = UiButton(normal, new Rect(39, 12, 105, 82),
                () => { }, ideckService);
            BindIDeckButtonFrames(ideckServiceButton, "ButtonService");
            ideckServiceButton.gameObject.AddComponent<RouletteDeckReleaseAction>()
                .Configure(ideckServiceButton, RequestPcService, false);
            ideckCashoutButton = UiButton(normal, new Rect(47, 157, 89, 66),
                () => { }, ideckCashout);
            BindIDeckButtonFrames(ideckCashoutButton, "ButtonCashout");
            ideckCashoutButton.gameObject.AddComponent<RouletteDeckReleaseAction>()
                .Configure(ideckCashoutButton, HandlePcCashoutRelease, false);
            // filedefs.xml positions these picture-font lines inside each button.
            new PictureLabel(normal, new Rect(56, 39, 70, 27),
                OriginalBitmapFont.Load("ideck_service"), raycastTarget: false).Set("SERVICE");
            new PictureLabel(normal, new Rect(56, 176, 71, 27),
                OriginalBitmapFont.Load("ideck_cashout"), raycastTarget: false).Set("COLLECT");
            ideckAutoPlayButton = UiButton(normal, new Rect(47, 88, 89, 66),
                () => { }, ideckAutoPlay);
            BindIDeckButtonFrames(ideckAutoPlayButton, "ButtonAutoPlay");
            ideckAutoPlayButton.gameObject.AddComponent<RouletteDeckReleaseAction>()
                .Configure(ideckAutoPlayButton, RequestAutoPlay, true);
            new PictureLabel(normal, new Rect(56, 107, 71, 27),
                OriginalBitmapFont.Load("ideck_autoplay"), raycastTarget: false).Set("AUTOPLAY");

            RectTransform chipRoot = UiRect(normal, new Rect(0, 0, 1280, 242), "Chip selection");
            ideckChipControls = chipRoot.gameObject;
            new PictureLabel(chipRoot, new Rect(352, 1, 674, 23),
                OriginalBitmapFont.Load("ideck_title")).Set("Select a Chip Value");
            int[] x = { 283, 491, 700, 907 };
            ideckChipButtons = new Button[4];
            ideckChipTouchImages = new RawImage[4];
            ideckTouchFrames = Resources.LoadAll<Texture2D>("Animations/IDeckTouch")
                .OrderBy(frame => frame.name).ToArray();
            if (ideckTouchFrames.Length != 24 ||
                ideckTouchFrames.Any(frame => frame.width != 188 || frame.height != 188))
                throw new InvalidOperationException("Original iDeck touch animation frames missing");
            for (int i = 0; i < 4; ++i)
            {
                int chipIndex = i;
                int value = chips[i];
                Button chipButton = UiButton(chipRoot, new Rect(x[i], 29, 188, 188),
                    () => { }, ideckChips[i]);
                chipButton.gameObject.AddComponent<RoulettePointerDownAction>()
                    .Configure(chipButton, () => {
                        if (PcGameOrdinaryInputBlocked || SourceRoundBlocksOrdinaryInput) return;
                        // The source stops the touched button's old animation
                        // even when its selector is already current.
                        StopIDeckTouchAnimation();
                        if (session != null && session.Chip != value)
                            if (Try(() => session.SetChip(value), betSelectedSound,
                                    null, false)) StartIDeckTouchAnimation(chipIndex);
                        SetIDeckPreview(false);
                    });
                ideckChipButtons[i] = chipButton;
                BindIDeckButtonFrames(chipButton, "ButtonBetPer0" + (i + 1));
                // XML (-5,4)+(198,180)/2 centers the native 188x188 movie
                // at (94,94). SpriteMovie is not stretched to that XML size.
                ideckChipTouchImages[i] = UiImage(chipButton.GetComponent<RectTransform>(),
                    new Rect(0, 0, 188, 188), ideckTouchFrames[0], "Original chip touch animation");
                ideckChipTouchImages[i].raycastTarget = false;
                ideckChipTouchImages[i].gameObject.SetActive(false);
            }
            RectTransform doubleControls = UiRect(normal,
                new Rect(0, 0, 1280, 242), "Double decision");
            ideckDoubleControls = doubleControls.gameObject;
            Action takeWinAction = () => {
                if (PcGameOrdinaryInputBlocked) return;
                if (SourceDoubleOfferActive) TakeSourceDoubleOffer();
                else if (!SourceRoundBlocksOrdinaryInput && !ResultDisplayActive)
                    Try(() => session.NextRound());
                SetIDeckPreview(false);
            };
            Button takeWinButton = UiButton(doubleControls,
                new Rect(294, 37, 168, 168), () => { }, ideckTakeWin);
            ideckOfferTakeWinButton = takeWinButton;
            takeWinButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(takeWinButton, takeWinAction);
            BindIDeckButtonFrames(takeWinButton, "ButtonTakeWin");
            new PictureLabel(doubleControls, new Rect(312, 96, 130, 53),
                OriginalBitmapFont.Load("ideck_double_decision"),
                raycastTarget: false).Set("TAKE WIN");
            Action doubleUpAction = () => {
                if (PcGameOrdinaryInputBlocked) return;
                if (ResultDisplayActive && !SourceDoubleOfferActive) return;
                Try(BeginDouble);
                SetIDeckPreview(false);
            };
            Button doubleUpButton = UiButton(doubleControls,
                new Rect(711, 37, 168, 168), () => { }, ideckDoubleUp);
            ideckOfferDoubleUpButton = doubleUpButton;
            doubleUpButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(doubleUpButton, doubleUpAction);
            BindIDeckButtonFrames(doubleUpButton, "ButtonDoubleUp");
            new PictureLabel(doubleControls, new Rect(730, 96, 130, 53),
                OriginalBitmapFont.Load("ideck_double_decision"),
                raycastTarget: false).Set("DOUBLE UP");

            RectTransform redBlack = UiRect(deck, new Rect(0, 0, 1280, 242),
                "Red and black Double Up controls");
            ideckRedBlackControls = redBlack.gameObject;
            UiImage(redBlack, new Rect(0, 0, 1280, 242), ideckDoublePanel,
                "Original Double Up iDeck background");
            UiImage(redBlack, new Rect(71, 137, 161, 69), ideckExitTakeWin,
                "Original disabled Take Win underlay");
            UiImage(redBlack, new Rect(351, 22, 313, 195), ideckExitRed,
                "Original disabled Red underlay");
            UiImage(redBlack, new Rect(774, 22, 312, 195), ideckExitBlack,
                "Original disabled Black underlay");
            ideckTakeWinButton = UiPointerDownButton(redBlack,
                new Rect(71, 137, 161, 69), () => {
                if (session != null && session.InDoubleUp && !IsDoubleReveal &&
                    !doubleCollectPending && !showingDoubleHelp)
                {
                    Try(BeginCollectDouble);
                    if (!ideckSecondDisplay) SetIDeckPreview(false);
                }
            }, ideckExitTakeWin);
            ideckRedButton = UiPointerDownButton(redBlack,
                new Rect(351, 23, 312, 193), () => {
                if (session != null && session.InDoubleUp && !IsDoubleReveal && !doubleCollectPending)
                {
                    Guess(-1);
                    if (!ideckSecondDisplay) SetIDeckPreview(false);
                }
            }, ideckRedDeck);
            ideckBlackButton = UiPointerDownButton(redBlack,
                new Rect(774, 23, 312, 193), () => {
                if (session != null && session.InDoubleUp && !IsDoubleReveal && !doubleCollectPending)
                {
                    Guess(-2);
                    if (!ideckSecondDisplay) SetIDeckPreview(false);
                }
            }, ideckBlackDeck);
            ideckTakeWinFlashOverlay = UiImage(redBlack,
                new Rect(70, 136, 162, 70), ideckTakeWinDeckFlash,
                "Original flashing Take Win iDeck button");
            ideckTakeWinFlashOverlay.raycastTarget = false;

            RectTransform suitDeck = UiRect(deck, new Rect(0, 0, 1280, 242),
                "Original suit Double Up controls");
            ideckSuitControls = suitDeck.gameObject;
            UiImage(suitDeck, new Rect(0, 0, 1280, 242), suitDeckBackground,
                "Original suit iDeck background");
            Rect[] suitRects = { new Rect(475, 10, 227, 129),
                new Rect(848, 10, 227, 129), new Rect(475, 142, 107, 94),
                new Rect(594, 142, 108, 94), new Rect(848, 142, 108, 94),
                new Rect(967, 142, 108, 94) };
            ideckSuitChoiceButtons = new Button[6];
            for (int i = 0; i < 6; ++i)
            {
                int choice = i < 2 ? -i - 1 : i - 2;
                UiImage(suitDeck, suitRects[i], suitDeckButtonsDisabled[i],
                    "Original disabled suit iDeck button");
                ideckSuitChoiceButtons[i] = UiPointerDownButton(suitDeck,
                    suitRects[i], () => {
                    if (session != null && session.InDoubleUp && !IsDoubleReveal &&
                        !doubleCollectPending && !showingDoubleHelp)
                    {
                        Guess(choice);
                        if (!ideckSecondDisplay) SetIDeckPreview(false);
                    }
                }, suitDeckButtons[i]);
            }
            UiImage(suitDeck, new Rect(161, 71, 150, 111),
                SuitBitmap("bSuitGambleTakeWinDeckButtonDisabled"),
                "Original disabled suit Take Win");
            ideckSuitTakeWin = UiPointerDownButton(suitDeck,
                new Rect(161, 71, 150, 111), () => {
                    if (session != null && session.InDoubleUp && !IsDoubleReveal &&
                        !doubleCollectPending && !showingDoubleHelp)
                    {
                        Try(BeginCollectDouble);
                        if (!ideckSecondDisplay) SetIDeckPreview(false);
                    }
                }, suitTakeWinDeck);
            ideckSuitTakeWinFlashOverlay = UiImage(suitDeck,
                new Rect(161, 71, 150, 111), suitTakeWinDeckFlash,
                "Original flashing suit Take Win iDeck button");
            ideckSuitTakeWinFlashOverlay.raycastTarget = false;

            RectTransform exitPanel = UiRect(deck, new Rect(0, 0, 1280, 242),
                "Double Up deck panel out");
            ideckExitPanel = exitPanel.gameObject;
            UiImage(exitPanel, new Rect(71, 137, 161, 69), ideckExitTakeWin,
                "Original disabled Take Win");
            UiImage(exitPanel, new Rect(351, 22, 313, 195), ideckExitRed,
                "Original disabled Red");
            UiImage(exitPanel, new Rect(774, 22, 312, 195), ideckExitBlack,
                "Original disabled Black");
            ideckExitPanel.SetActive(false);

            RectTransform autoConfirm = UiRect(deck, new Rect(0, 0, 1280, 242),
                "Start Autoplay confirmation");
            ideckAutoConfirmControls = autoConfirm.gameObject;
            UiImage(autoConfirm, new Rect(0, 0, 1280, 242), ideckAutoConfirm,
                "Original autoplay confirmation background");
            ideckAutoYesButton = UiButton(autoConfirm, new Rect(981, 75, 123, 91),
                () => { }, ideckAutoYes);
            ideckAutoYesButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(ideckAutoYesButton, () => ConfirmAutoPlay(true));
            KeepConfirmationHitAreaActive(ideckAutoYesButton);
            ideckAutoNoButton = UiButton(autoConfirm,
                new Rect(1119, 75, 123, 91), () => { }, ideckAutoNo);
            ideckAutoNoButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(ideckAutoNoButton, () => {
                    if (!PcGameOrdinaryInputBlocked) ConfirmAutoPlay(false);
                });
            KeepConfirmationHitAreaActive(ideckAutoNoButton);
            new PictureLabel(autoConfirm, new Rect(180, 81, 240, 40),
                OriginalBitmapFont.Load("ideck_confirm_notify")).Set("YOU PRESSED THE");
            new PictureLabel(autoConfirm, new Rect(180, 122, 240, 40),
                OriginalBitmapFont.Load("ideck_confirm_notify")).Set("AUTOPLAY BUTTON");
            new PictureLabel(autoConfirm, new Rect(432, 67, 536, 107),
                OriginalBitmapFont.Load("ideck_confirm_query")).Set("START AUTOPLAY?");
            new PictureLabel(autoConfirm, new Rect(1001, 100, 82, 40),
                OriginalBitmapFont.Load("ideck_confirm_yes_no"), raycastTarget: false).Set("YES");
            new PictureLabel(autoConfirm, new Rect(1139, 100, 82, 40),
                OriginalBitmapFont.Load("ideck_confirm_yes_no"), raycastTarget: false).Set("NO");

            RectTransform serviceConfirm = UiRect(deck,
                new Rect(0, 0, 1280, 242), "Service confirmation");
            ideckServiceConfirmControls = serviceConfirm.gameObject;
            UiImage(serviceConfirm, new Rect(0, 0, 1280, 242),
                ideckServiceConfirm, "Original service confirmation background");
            ideckServiceYesButton = UiButton(serviceConfirm,
                new Rect(981, 75, 123, 91), () => { }, ideckAutoYes);
            ideckServiceYesButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(ideckServiceYesButton, () => ConfirmPcService(true));
            KeepConfirmationHitAreaActive(ideckServiceYesButton);
            ideckServiceNoButton = UiButton(serviceConfirm,
                new Rect(1119, 75, 123, 91), () => { }, ideckAutoNo);
            ideckServiceNoButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(ideckServiceNoButton, () => {
                    if (!PcGameOrdinaryInputBlocked) ConfirmPcService(false);
                });
            KeepConfirmationHitAreaActive(ideckServiceNoButton);
            new PictureLabel(serviceConfirm, new Rect(180, 81, 240, 40),
                OriginalBitmapFont.Load("ideck_confirm_notify")).Set("YOU PRESSED THE");
            new PictureLabel(serviceConfirm, new Rect(180, 122, 240, 40),
                OriginalBitmapFont.Load("ideck_confirm_notify")).Set("SERVICE BUTTON");
            new PictureLabel(serviceConfirm, new Rect(432, 67, 536, 107),
                OriginalBitmapFont.Load("ideck_confirm_query")).Set("NEED SERVICE?");
            new PictureLabel(serviceConfirm, new Rect(1001, 100, 82, 40),
                OriginalBitmapFont.Load("ideck_confirm_yes_no"), raycastTarget: false).Set("YES");
            new PictureLabel(serviceConfirm, new Rect(1139, 100, 82, 40),
                OriginalBitmapFont.Load("ideck_confirm_yes_no"), raycastTarget: false).Set("NO");

            RectTransform collectConfirm = UiRect(deck,
                new Rect(0, 0, 1280, 242), "Collect confirmation");
            ideckCollectConfirmControls = collectConfirm.gameObject;
            UiImage(collectConfirm, new Rect(0, 0, 1280, 242),
                ideckCollectConfirm, "Original collect confirmation background");
            ideckCollectYesButton = UiButton(collectConfirm,
                new Rect(981, 75, 123, 91), () => { }, ideckAutoYes);
            ideckCollectYesButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(ideckCollectYesButton, () => ConfirmPcCollect(true));
            KeepConfirmationHitAreaActive(ideckCollectYesButton);
            ideckCollectNoButton = UiButton(collectConfirm,
                new Rect(1119, 75, 123, 91), () => { }, ideckAutoNo);
            ideckCollectNoButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(ideckCollectNoButton, () => {
                    if (!PcGameOrdinaryInputBlocked) ConfirmPcCollect(false);
                });
            KeepConfirmationHitAreaActive(ideckCollectNoButton);
            new PictureLabel(collectConfirm, new Rect(180, 81, 240, 40),
                OriginalBitmapFont.Load("ideck_confirm_notify")).Set("YOU PRESSED THE");
            new PictureLabel(collectConfirm, new Rect(180, 122, 240, 40),
                OriginalBitmapFont.Load("ideck_confirm_notify")).Set("COLLECT BUTTON");
            new PictureLabel(collectConfirm, new Rect(432, 67, 536, 107),
                OriginalBitmapFont.Load("ideck_confirm_query")).Set("COLLECT NOW?");
            new PictureLabel(collectConfirm, new Rect(1001, 100, 82, 40),
                OriginalBitmapFont.Load("ideck_confirm_yes_no"), raycastTarget: false).Set("YES");
            new PictureLabel(collectConfirm, new Rect(1139, 100, 82, 40),
                OriginalBitmapFont.Load("ideck_confirm_yes_no"), raycastTarget: false).Set("NO");

            // The source iDeck controller plays two full-width Bink button
            // animations concurrently, then background alone, then repeats.
            ideckAttractBackground = UiImage(deck, new Rect(0, 0, 1280, 242),
                null, "Original iDeck attract background");
            ideckAttractForeground = UiImage(deck, new Rect(0, 0, 1280, 242),
                null, "Original iDeck attract foreground");
            ideckAttractBackground.raycastTarget = false;
            ideckAttractForeground.raycastTarget = false;

            RectTransform topper;
            topperCanvas = CreatePreviewCanvas("Original topper uGUI",
                new Vector2(1360, 768), out topper);
#if !UNITY_EDITOR
            int topperIndex = TopperOutputIndex(Display.displays.Length);
            if (topperIndex >= 0 && !topperPreview && !histogramPreview &&
                !singleDisplayRequested)
            {
                Display.displays[topperIndex].Activate();
                topperDedicatedDisplay = true;
                topperDisplayIndex = topperIndex;
                topperCanvas.targetDisplay = topperIndex;
            }
#endif
            if (singleDisplayRequested)
                Debug.Log("ROULETTE_PC_SINGLE_DISPLAY: iDeck=" + ideckSecondDisplay +
                    " topper=" + topperDedicatedDisplay);
            if (Array.IndexOf(launchArgs, "-rouletteDisplayRouteSmoke") >= 0)
                Debug.Log("ROULETTE_DISPLAY_OUTPUTS: detected=" +
                    Display.displays.Length + " iDeck=" + ideckDisplayIndex +
                    " topper=" + topperDisplayIndex);
            RawImage black = UiImage(topper, new Rect(0, 0, 1360, 768),
                Texture2D.whiteTexture, "Topper background");
            black.color = Color.black;
            topperMovie = UiImage(topper, new Rect(48, 31, 1276, 348),
                topperFrames.Length > 0 ? topperFrames[0] : null, "DDA movie");
            topperResultSpot = UiImage(topper, new Rect(224, 31, 912, 431),
                topperSpotRed, "Original topper result spot");
            topperResultSpot.raycastTarget = false;
            topperResultNumber = new PictureLabel(topper,
                new Rect(478, 120, 405, 234), topperResultFont,
                "Topper winning number", nativeSize: true);
            CreateHistogramUi(topper);
        }

        private static void BindIDeckButtonFrames(Button button, string sourceName)
        {
            var states = new Texture2D[4];
            for (int i = 0; i < states.Length; ++i)
                states[i] = Resources.Load<Texture2D>(
                    "Original/IDeck/" + sourceName + "_" + (i + 1));
            var frames = button.gameObject.AddComponent<RouletteOriginalButtonFrames>();
            frames.Configure(button, states);
            button.gameObject.AddComponent<RouletteDeckMoveState>()
                .Configure(frames);
        }

        private IEnumerator ActivateCaptureDeckAfterPrimaryFrame()
        {
            // Explicit capture diagnostic: let the command-line primary
            // resolution settle before creating the secondary swapchain.
            yield return null;
            Display.displays[ideckDisplayIndex].Activate();
            ideckCanvas.targetDisplay = ideckDisplayIndex;
            ideckSecondDisplay = true;
            RefreshDiagnosticUi();
            Debug.Log("ROULETTE_CAPTURE_DECK_ACTIVATED: target=" + ideckDisplayIndex);
        }

        private static void KeepConfirmationHitAreaActive(Button button)
        {
            // The Menu registers touch handlers on both confirmation sprites
            // and their labels. Blink hides the sprite, but its label remains
            // a touch target. menudefs.xml places the 82x40 label at (20,25)
            // inside each 123x91 button; only that area remains live in blink.
            var area = new GameObject("Confirmation touch area",
                typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)area.transform;
            rect.SetParent(button.transform, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(20f, -25f);
            rect.sizeDelta = new Vector2(82f, 40f);
            var hit = area.GetComponent<Image>();
            hit.color = new Color(1f, 1f, 1f, 0f);
            hit.raycastTarget = true;
        }

        private void TickPcAutoConfirmation(long timestamp)
        {
            if (!autoConfirmPending) return;
            if (timestamp >= autoConfirmDeadlineTimestamp)
            {
                ConfirmAutoPlay(false);
                return;
            }
            long elapsed = OriginalRackClock.ElapsedMilliseconds(
                autoConfirmStartedTimestamp, timestamp,
                System.Diagnostics.Stopwatch.Frequency);
            if (elapsed < 1000) SetAutoConfirmButtonVisibility(true, true);
            else SetAutoConfirmButtonVisibility(
                (elapsed / 1000 & 1) != 0, (elapsed / 1000 & 1) == 0);
        }

        private void SetAutoConfirmButtonVisibility(bool yes, bool no)
        {
            if (ideckAutoYesButton == null || ideckAutoNoButton == null) return;
            ideckAutoYesButton.targetGraphic.enabled = yes;
            ideckAutoNoButton.targetGraphic.enabled = no;
        }

        private void HandlePcCashoutRelease()
        {
            if (PcGameOrdinaryInputBlocked) return;
            // Game::CashoutRequest closes Pays before its main-state gate.
            // Feature Help hides this iDeck control. PC has no cabinet
            // cashout completion.
            paysPage = -1;
            if (session == null || spinning || session.SpinPending ||
                SourceRoundBlocksOrdinaryInput ||
                ResultDisplayActive || session.InDoubleUp ||
                IsDoubleReveal || IsDoubleExit || doubleCollectPending)
                return;
            RequestPcCollectConfirmation();
        }

        private void RequestPcService()
        {
            if (PcGameOrdinaryInputBlocked && !(PcServiceKeepAliveReady && pcResultDisplayWaitServicing)) return;
            if (serviceConfirmPending || collectConfirmPending) return;
            // SendServiceRelease carries !Game::IsGameOver. During a live
            // round the Menu completes the service request directly; its
            // fifteen-second confirmation belongs to the completed-game path.
            if (sourceRoundPhase != SourceRoundPhase.None || spinning || session != null && session.InDoubleUp)
            {
                pcServiceRequested = !pcServiceRequested;
                ShowFeedback(pcServiceRequested ? "PC virtual service requested." : "PC virtual service request cleared.");
                Debug.Log("ROULETTE_PC_SERVICE_LIVE_RELEASE: requested=" + pcServiceRequested +
                    " suspended=" + PcGameAppSuspended + " activeAuto=" + autoplay);
                return;
            }
            if (pcServiceRequested)
            {
                // The Menu release path clears an active request without
                // showing another YES/NO confirmation.
                pcServiceRequested = false;
                ShowFeedback("PC virtual service request cleared.");
                return;
            }
            serviceConfirmPending = true;
            serviceConfirmStartedTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp();
            serviceConfirmDeadlineTimestamp = serviceConfirmStartedTimestamp +
                ServiceConfirmMilliseconds *
                System.Diagnostics.Stopwatch.Frequency / 1000;
            SetServiceConfirmButtonVisibility(true, true);
            Debug.Log("ROULETTE_PC_SERVICE_CONFIRM_OPEN: 15000 ms");
        }

        private void ConfirmPcService(bool accepted)
        {
            if (accepted && PcGameOrdinaryInputBlocked) return;
            if (!serviceConfirmPending) return;
            serviceConfirmPending = false;
            serviceConfirmDeadlineTimestamp = 0;
            SetServiceConfirmButtonVisibility(true, true);
            if (accepted)
            {
                // Local PC substitute for the Menu/GameMgr acknowledgement.
                // No cabinet attendant request is sent from this build.
                pcServiceRequested = true;
                ShowFeedback("PC virtual service requested.");
            }
            else ShowFeedback("Service request canceled.");
        }

        // Game::HandleEvent cancels an active iDeck confirmation before
        // forwarding an enabled main-screen touch to the roulette table.
        private bool CancelPcPendingDeckConfirmationFromMainTouch()
        {
            if (PcGameOrdinaryInputBlocked) return false;
            if (serviceConfirmPending) { ConfirmPcService(false); return true; }
            if (collectConfirmPending) { ConfirmPcCollect(false); return true; }
            if (autoConfirmPending) { ConfirmAutoPlay(false); return true; }
            return false;
        }

        private void TickPcServiceConfirmation(long timestamp)
        {
            if (!serviceConfirmPending) return;
            if (timestamp >= serviceConfirmDeadlineTimestamp)
            {
                ConfirmPcService(false);
                return;
            }
            long elapsed = OriginalRackClock.ElapsedMilliseconds(
                serviceConfirmStartedTimestamp, timestamp,
                System.Diagnostics.Stopwatch.Frequency);
            // The Menu's 1000 ms DispatchYesNoTimer initially leaves both
            // buttons shown, then alternates the button sprites.
            if (elapsed < 1000) SetServiceConfirmButtonVisibility(true, true);
            else SetServiceConfirmButtonVisibility(
                (elapsed / 1000 & 1) != 0, (elapsed / 1000 & 1) == 0);
        }

        private void SetServiceConfirmButtonVisibility(bool yes, bool no)
        {
            if (ideckServiceYesButton == null || ideckServiceNoButton == null) return;
            ideckServiceYesButton.targetGraphic.enabled = yes;
            ideckServiceNoButton.targetGraphic.enabled = no;
        }

        private void RequestPcCollectConfirmation()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (collectConfirmPending || serviceConfirmPending || autoConfirmPending ||
                pcCollectPending || autoplay ||
                !(session is RouletteSession) ||
                ((RouletteSession)session).MoneyCents < 1)
                return;
            collectConfirmPending = true;
            collectConfirmStartedTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp();
            collectConfirmDeadlineTimestamp = collectConfirmStartedTimestamp +
                CollectConfirmMilliseconds *
                System.Diagnostics.Stopwatch.Frequency / 1000;
            SetCollectConfirmButtonVisibility(true, true);
            Debug.Log("ROULETTE_PC_COLLECT_CONFIRM_OPEN: 15000 ms");
        }

        private void ConfirmPcCollect(bool accepted)
        {
            if (accepted && PcGameOrdinaryInputBlocked) return;
            if (!collectConfirmPending) return;
            collectConfirmPending = false;
            collectConfirmDeadlineTimestamp = 0;
            SetCollectConfirmButtonVisibility(true, true);
            if (accepted) RequestPcCollect();
            else ShowFeedback("Collect canceled.");
        }

        private void TickPcCollectConfirmation(long timestamp)
        {
            if (!collectConfirmPending) return;
            if (timestamp >= collectConfirmDeadlineTimestamp)
            {
                ConfirmPcCollect(false);
                return;
            }
            long elapsed = OriginalRackClock.ElapsedMilliseconds(
                collectConfirmStartedTimestamp, timestamp,
                System.Diagnostics.Stopwatch.Frequency);
            if (elapsed < 1000) SetCollectConfirmButtonVisibility(true, true);
            else SetCollectConfirmButtonVisibility(
                (elapsed / 1000 & 1) != 0, (elapsed / 1000 & 1) == 0);
        }

        private void SetCollectConfirmButtonVisibility(bool yes, bool no)
        {
            if (ideckCollectYesButton == null || ideckCollectNoButton == null) return;
            ideckCollectYesButton.targetGraphic.enabled = yes;
            ideckCollectNoButton.targetGraphic.enabled = no;
        }

        private IEnumerator DoubleAutoGuardSmoke()
        {
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            SetIDeckPreview(true);
            autoplay = true;
            bool refused = false;
            try { BeginDouble(); }
            catch (InvalidOperationException) { refused = true; }
            RefreshDiagnosticUi();
            if (!refused || session.InDoubleUp || session.Credit != 1035 ||
                ideckDoubleControls.activeSelf)
                throw new InvalidOperationException("AutoPlay exposed or entered Double Up");
            autoplay = false;
            SetIDeckPreview(false);
            Debug.Log("ROULETTE_DOUBLE_AUTO_GUARD_PASSED: source AutoPlay exclusion");
        }

        private void StartIDeckTouchAnimation(int chipIndex)
        {
            StopIDeckTouchAnimation();
            ideckTouchChipIndex = chipIndex;
            ideckTouchStartedTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            StartPcIDeckTouchMovie();
            RefreshIDeckTouchAnimation(ideckTouchStartedTimestamp);
        }

        private void StopIDeckTouchAnimation()
        {
            ideckTouchChipIndex = -1;
            ideckTouchStartedTimestamp = 0;
            StopPcMovie(ref pcIDeckTouchMovie);
            if (ideckChipTouchImages == null) return;
            foreach (RawImage image in ideckChipTouchImages)
                if (image != null) image.gameObject.SetActive(false);
        }

        private void RefreshIDeckTouchAnimation(long timestamp)
        {
            if (ideckTouchChipIndex < 0) return;
            if (session == null || session.Chip != chips[ideckTouchChipIndex])
            {
                StopIDeckTouchAnimation();
                return;
            }
            if (pcIDeckTouchMovie == null || pcIDeckTouchMovie.SourceEpoch != ideckTouchStartedTimestamp)
                StartPcIDeckTouchMovie();
            int frame = pcIDeckTouchMovie == null ? 0 : pcIDeckTouchMovie.SampleAt(timestamp);
            if (pcIDeckTouchMovie == null || (!pcIDeckTouchMovie.Playing && !pcIDeckTouchMovie.Paused))
            {
                // SpriteMovie plays one pass; its completion handler calls
                // CoreButton::StopAllAnimations and hides the movie.
                StopIDeckTouchAnimation();
                return;
            }
            RawImage image = ideckChipTouchImages[ideckTouchChipIndex];
            image.texture = ideckTouchFrames[frame];
            image.gameObject.SetActive(true);
        }

        private void RefreshDiagnosticUi()
        {
            if (ideckCanvas == null || topperCanvas == null) return;
            RefreshIDeckTouchAnimation(System.Diagnostics.Stopwatch.GetTimestamp());
            ideckCanvas.enabled = PcGameScreensVisible && (ideckPreview || ideckSecondDisplay);
            topperCanvas.enabled = PcGameScreensVisible && (topperPreview || histogramPreview || topperDedicatedDisplay);
            if (ideckCanvas.enabled && session != null)
            {
                // The original deck retains the selected Bet Per Unit button
                // and clears its peers when the chosen selector changes.
                for (int i = 0; i < ideckChipButtons.Length; ++i)
                {
                    ideckChipButtons[i].interactable = !SourceRoundBlocksOrdinaryInput;
                    ideckChipButtons[i].GetComponent<RouletteOriginalButtonFrames>()
                        .SetSelected(session.Chip == chips[i]);
                }
                ideckAutoPlayButton.interactable = !SourceRoundBlocksOrdinaryInput || autoplay;
                ideckCashoutButton.interactable = !SourceRoundBlocksOrdinaryInput;
                ideckServiceButton.GetComponent<RouletteOriginalButtonFrames>()
                    .SetSelected(pcServiceRequested);
                ideckAttractBackground.enabled = ideckAttractPreview;
                ideckAttractForeground.enabled = ideckAttractPreview;
                if (ideckAttractPreview && ideckAttractBackgroundFrames != null &&
                    ideckAttractBackgroundFrames.Length > 0)
                {
                    float elapsed = Mathf.Max(0f,
                        Time.realtimeSinceStartup - ideckAttractStartedAt);
                    // Type 3 is Background in the source enum. The first
                    // playback fades from .01 to 1 over 2000 ms; repeats do
                    // not restart the fader.
                    float t = Mathf.Clamp01(elapsed / 2f);
                    // SpriteFader FadeEffect 5 is EaseInOut: 3t^2-2t^3.
                    float fade = t * t * (3f - 2f * t);
                    ideckAttractBackground.color = new Color(1f, 1f, 1f,
                        .01f + .99f * fade);
                    PcIDeckAttractFramesAt(System.Diagnostics.Stopwatch.GetTimestamp(), out int backgroundFrame,
                        out int foregroundFrame);
                    ideckAttractBackground.texture =
                        ideckAttractBackgroundFrames[backgroundFrame];
                    ideckAttractForeground.enabled = foregroundFrame >= 0;
                    if (foregroundFrame >= 0)
                        ideckAttractForeground.texture =
                            ideckAttractForegroundFrames[foregroundFrame];
                }
                bool doubleDecision = SourceDoubleOfferActive ||
                    !SourceRoundBlocksOrdinaryInput && session.DoubleAvailable &&
                    DoubleUpInputAllowed && !spinning && !ResultDisplayActive;
                // StateDoubleUp keeps PLAY for closing Pays, while its
                // Double and Take Win buttons remain disabled in that view.
                ideckOfferTakeWinButton.interactable = doubleDecision && paysPage < 0;
                ideckOfferDoubleUpButton.interactable = doubleDecision && paysPage < 0;
                bool doubleGuess = DoubleFeaturePresentationVisible;
                ideckNormalControls.SetActive(!ideckAttractPreview &&
                    !autoConfirmPending && !serviceConfirmPending &&
                    !collectConfirmPending &&
                    !doubleGuess && !IsDoubleExit);
                // In the cabinet footage, 2 and 10 stay visible while the
                // TAKE WIN and DOUBLE UP buttons replace the 1 and 5 slots.
                ideckChipControls.SetActive(!IsDoubleExit);
                ideckDoubleControls.SetActive(doubleDecision && !IsDoubleExit);
                ideckRedBlackControls.SetActive(!session.DoubleSuitMode &&
                    !ideckAttractPreview && doubleGuess &&
                    !autoConfirmPending && !IsDoubleExit);
                ideckSuitControls.SetActive(session.DoubleSuitMode &&
                    !ideckAttractPreview && doubleGuess &&
                    !autoConfirmPending && !IsDoubleExit);
                int redState = DoubleColorButtonState(-1);
                int blackState = DoubleColorButtonState(-2);
                ideckRedButton.gameObject.SetActive(redState != 0);
                ideckBlackButton.gameObject.SetActive(blackState != 0);
                ideckTakeWinButton.gameObject.SetActive((DoubleDecisionReady || doubleCollectPending) && !showingDoubleHelp &&
                    !IsDoubleReveal && !IsDoubleExit);
                ideckTakeWinFlashOverlay.gameObject.SetActive(DoubleTakeWinFlash);
                ideckRedButton.interactable = DoubleDecisionReady && !IsDoubleReveal &&
                    !showingDoubleHelp && !doubleCollectPending;
                ideckBlackButton.interactable = DoubleDecisionReady && !IsDoubleReveal &&
                    !showingDoubleHelp && !doubleCollectPending;
                ideckTakeWinButton.interactable = DoubleDecisionReady &&
                    !doubleCollectPending && !showingDoubleHelp && !IsDoubleReveal;
                ideckRedButton.GetComponent<RawImage>().texture = redState == 2 ?
                    ideckRedDeckFlash : ideckRedDeck;
                ideckBlackButton.GetComponent<RawImage>().texture = blackState == 2 ?
                    ideckBlackDeckFlash : ideckBlackDeck;
                if (session.DoubleSuitMode)
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        int state = DoubleColorButtonState(i < 2 ? -i - 1 : i - 2);
                        ideckSuitChoiceButtons[i].gameObject.SetActive(state != 0);
                        ideckSuitChoiceButtons[i].interactable = DoubleDecisionReady &&
                            !IsDoubleReveal && !showingDoubleHelp && !doubleCollectPending;
                        ideckSuitChoiceButtons[i].GetComponent<RawImage>().texture =
                            state == 2 ? suitDeckButtonsFlash[i] : suitDeckButtons[i];
                    }
                    ideckSuitTakeWin.gameObject.SetActive((DoubleDecisionReady || doubleCollectPending) && !showingDoubleHelp &&
                        !IsDoubleReveal && !IsDoubleExit);
                    ideckSuitTakeWinFlashOverlay.gameObject.SetActive(DoubleTakeWinFlash);
                    ideckSuitTakeWin.interactable = DoubleDecisionReady &&
                        !doubleCollectPending && !showingDoubleHelp && !IsDoubleReveal;
                }
                ideckRedBlackControls.GetComponent<RectTransform>().anchoredPosition =
                    IsDoubleExit ? new Vector2(0, -600f * DoubleExitProgress) :
                    new Vector2(0, -600f * (1f - DoubleEntranceProgress));
                ideckExitPanel.SetActive(!ideckAttractPreview && IsDoubleExit);
                ideckExitPanel.GetComponent<RectTransform>().anchoredPosition =
                    IsDoubleExit ? new Vector2(0, -600f * DoubleExitProgress) : Vector2.zero;
                ideckAutoConfirmControls.SetActive(!ideckAttractPreview && autoConfirmPending);
                ideckServiceConfirmControls.SetActive(!ideckAttractPreview &&
                    serviceConfirmPending);
                ideckCollectConfirmControls.SetActive(!ideckAttractPreview &&
                    collectConfirmPending);
                if (ideckSecondDisplay)
                {
                    Display display = Display.displays[ideckDisplayIndex];
                    FitPreview(ideckCanvas, 1280, 242,
                        display.renderingWidth, display.renderingHeight);
                }
                else FitPreview(ideckCanvas, 1280, 242, Screen.width, Screen.height);
            }
            if (topperCanvas.enabled)
            {
                // The source DDA belongs to fDigitalTopperAttractLoop. Normal
                // result display resumes only after that feature is stopped.
                topperMovie.enabled = topperAttractPreview && !histogramPreview;
                if (topperMovie.enabled && topperFrames.Length > 0)
                    topperMovie.texture = topperFrames[TopperAttractFrameIndex()];
                // AddNumber starts fTopperWinN and stops the preceding feature.
                // The previous outcome remains visible until a new one settles.
                int lastPocket = recordedPockets.Count > 0 ? recordedPockets[0] : -1;
                topperResultSpot.enabled = lastPocket >= 0 && !topperAttractPreview;
                if (topperResultSpot.enabled)
                {
                    topperResultSpot.texture = lastPocket == 0 ? topperSpotGreen :
                        Array.IndexOf(RedNumbers, lastPocket) >= 0 ? topperSpotRed : topperSpotBlack;
                    topperResultNumber.Set(lastPocket.ToString());
                }
                else topperResultNumber.Set("");
                RefreshHistogramUi();
                if (topperDedicatedDisplay && !topperPreview && !histogramPreview)
                {
                    Display display = Display.displays[topperDisplayIndex];
                    FitPreview(topperCanvas, 1360, 768,
                        display.renderingWidth, display.renderingHeight);
                }
                else FitPreview(topperCanvas, 1360, 768, Screen.width, Screen.height);
            }
        }

        private void RefreshTopperTarget()
        {
            if (topperCanvas != null && topperDedicatedDisplay)
                topperCanvas.targetDisplay = topperPreview ||
                    (histogramPreview && !historyView) ? 0 : topperDisplayIndex;
        }

        private static int IDeckOutputIndex(int count)
        {
            return count >= 3 ? 2 : count == 2 ? 1 : -1;
        }

        private static int TopperOutputIndex(int count)
        {
            // Screen 4 remains output 3 on a full cabinet layout. With three
            // PC displays, route the topper to the otherwise unused output 1.
            return count >= 4 ? 3 : count == 3 ? 1 : -1;
        }

        private static void ValidateDisplayRouting()
        {
            int[] deck = { -1, -1, 1, 2, 2 };
            int[] topper = { -1, -1, -1, 1, 3 };
            for (int count = 1; count <= 4; ++count)
            {
                if (IDeckOutputIndex(count) != deck[count] ||
                    TopperOutputIndex(count) != topper[count] ||
                    IDeckOutputIndex(count) == TopperOutputIndex(count) &&
                    IDeckOutputIndex(count) >= 0)
                    throw new InvalidOperationException(
                        "Original display routing overlaps outputs: " + count);
            }
            Debug.Log("ROULETTE_DISPLAY_ROUTING_PASSED: 1 preview; 2 iDeck@1; " +
                "3 topper@1 iDeck@2; 4 iDeck@2 topper@3");
        }

        private static void FitPreview(Canvas canvas, float width, float height,
            float displayWidth, float displayHeight)
        {
            float w = displayWidth / width, h = displayHeight / height;
            canvas.GetComponent<CanvasScaler>().matchWidthOrHeight = w < h ? 0f : 1f;
        }

        private IEnumerator AutoConfirmSmoke()
        {
            yield return null;
            if (session == null || ideckAutoYesButton == null ||
                ideckAutoNoButton == null || ideckSecondDisplay)
                throw new InvalidOperationException("AUTOPLAY confirmation smoke needs primary iDeck");
            long credit = session.Credit;
            long wager = session.TotalBet;
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            yield return new WaitForEndOfFrame();
            PointerEventData autoDrag = PressThroughUiRaycast(ideckAutoPlayButton);
            if (autoConfirmPending)
                throw new InvalidOperationException("AUTOPLAY acted before touch release");
            ExecuteEvents.Execute(ideckAutoPlayButton.gameObject, autoDrag,
                ExecuteEvents.initializePotentialDrag);
            if (autoDrag.useDragThreshold)
                throw new InvalidOperationException("iDeck move retained Unity drag threshold");
            autoDrag.delta = new Vector2(4, 0);
            ExecuteEvents.Execute(ideckAutoPlayButton.gameObject, autoDrag,
                ExecuteEvents.dragHandler);
            if ((ideckAutoPlayButton.targetGraphic as RawImage).texture !=
                Resources.Load<Texture2D>("Original/IDeck/ButtonAutoPlay_1"))
                throw new InvalidOperationException("AUTOPLAY move did not restore deck frame");
            ReleaseUiPointer(ideckAutoPlayButton, autoDrag);
            RefreshDiagnosticUi();
            if (!autoConfirmPending || !ideckAutoConfirmControls.activeInHierarchy)
                throw new InvalidOperationException("AUTOPLAY move/release did not confirm");
            // The confirmation label is first registered with GraphicRaycaster
            // after Unity completes the new canvas frame.
            yield return new WaitForEndOfFrame();
            long frequency = System.Diagnostics.Stopwatch.Frequency;
            TickPcAutoConfirmation(autoConfirmStartedTimestamp + frequency * 1100 / 1000);
            if (!ideckAutoYesButton.targetGraphic.enabled ||
                ideckAutoNoButton.targetGraphic.enabled)
                throw new InvalidOperationException("AUTOPLAY first blink differs from Menu");
            Canvas.ForceUpdateCanvases();
            var edge = new PointerEventData(EventSystem.current) {
                position = RectTransformUtility.WorldToScreenPoint(null,
                    ideckAutoNoButton.transform.TransformPoint(new Vector3(5f, -5f, 0f))),
                button = PointerEventData.InputButton.Left
            };
            var edgeHits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(edge, edgeHits);
            if (edgeHits.Any(hit => hit.gameObject == ideckAutoNoButton.gameObject ||
                hit.gameObject.transform.parent == ideckAutoNoButton.transform))
                throw new InvalidOperationException("AUTOPLAY hidden button edge remained clickable");
            yield return null;
            ClickThroughUiRaycast(ideckAutoNoButton);
            if (autoConfirmPending || autoplay || session.Credit != credit ||
                session.TotalBet != wager)
                throw new InvalidOperationException("AUTOPLAY hidden NO failed");
            SetIDeckPreview(true);
            RequestAutoPlay();
            RefreshDiagnosticUi();
            autoConfirmDeadlineTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            TickPcAutoConfirmation(System.Diagnostics.Stopwatch.GetTimestamp());
            if (autoConfirmPending || autoplay || session.Credit != credit ||
                session.TotalBet != wager)
                throw new InvalidOperationException("AUTOPLAY timeout failed");
            SetIDeckPreview(true);
            RequestAutoPlay();
            RefreshDiagnosticUi();
            TickPcAutoConfirmation(autoConfirmStartedTimestamp + frequency * 2100 / 1000);
            if (ideckAutoYesButton.targetGraphic.enabled ||
                !ideckAutoNoButton.targetGraphic.enabled)
                throw new InvalidOperationException("AUTOPLAY second blink differs from Menu");
            yield return null;
            ClickThroughUiRaycast(ideckAutoYesButton);
            if (autoConfirmPending || !autoplay || session.Credit != credit ||
                session.TotalBet != wager)
                throw new InvalidOperationException("AUTOPLAY hidden YES failed");
            ToggleAuto();
            Debug.Log("ROULETTE_AUTO_CONFIRM_FLOW_PASSED: touch-down YES/NO, hidden labels, 1000ms blink, timeout, no ledger change");
            Application.Quit();
        }

        private IEnumerator CollectSourceSmoke()
        {
            yield return null;
            if (session == null || ideckCashoutButton == null)
                throw new InvalidOperationException("COLLECT smoke has no session or iDeck button");
            long credit = session.Credit;
            long wager = session.TotalBet;
            status = "COLLECT_IGNORED_DURING_SPIN";
            paysPage = 0;
            spinning = true;
            HandlePcCashoutRelease();
            spinning = false;
            if (paysPage != -1 || status != "COLLECT_IGNORED_DURING_SPIN" ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("Spin COLLECT breached source state gate");
            paysPage = 0;
            HandlePcCashoutRelease();
            if (paysPage != -1 || !collectConfirmPending || pcCollectPending ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("COLLECT skipped confirmation or changed credit");
            ConfirmPcCollect(false);
            if (collectConfirmPending || pcCollectPending || session.Credit != credit)
                throw new InvalidOperationException("COLLECT NO changed credit");
            HandlePcCashoutRelease();
            ConfirmPcCollect(true);
            if (!pcCollectPending || session.Credit != credit)
                throw new InvalidOperationException("COLLECT YES skipped request stage");
            for (int frame = 0; frame < 5 && session.Credit != 0; ++frame)
                yield return null;
            if (session.Credit != 0 || !pcCollectPending)
                throw new InvalidOperationException("COLLECT credit callback was not separate");
            for (int frame = 0; frame < 5 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || session.Credit != 0 ||
                !status.StartsWith("PC virtual collect:", StringComparison.Ordinal))
                throw new InvalidOperationException("PC COLLECT completion failed");
            Debug.Log("ROULETTE_COLLECT_SOURCE_GATE_PASSED: spin rejected, request/credit/completion separated");
            Application.Quit();
        }

        private IEnumerator CollectRaycastSmoke(bool reportMeter = false)
        {
            yield return null;
            if (session == null || ideckCashoutButton == null ||
                ideckServiceButton == null || ideckSecondDisplay)
                throw new InvalidOperationException("COLLECT raycast needs the primary iDeck display");
            if (reportMeter) PaymentSmokeRequire(recordedHistoryKey.Contains("PcPaymentMessageTestMeterDeck") &&
                !temporaryHistory && session.Credit == 1000 && session.Rounds == 0 && recordedPockets.Count == 0 &&
                !PlayerPrefs.HasKey(PcCreditKey) && !PlayerPrefs.HasKey(PcCollectMeterKey), "fresh owned iDeck meter producer");
            long credit = session.Credit;
            long wager = session.TotalBet;
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            ClickThroughUiRaycast(ideckServiceButton);
            RefreshDiagnosticUi();
            if (!serviceConfirmPending || !ideckServiceConfirmControls.activeInHierarchy ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("SERVICE raycast or ledger failed");
            yield return null;
            ClickThroughUiRaycast(ideckServiceNoButton);
            if (serviceConfirmPending || pcServiceRequested ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("SERVICE NO did not cancel cleanly");
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            paysPage = 0;
            status = "COLLECT_RAYCAST_SPIN_IGNORED";
            spinning = true;
            ClickThroughUiRaycast(ideckCashoutButton);
            spinning = false;
            if (paysPage != -1 || status != "COLLECT_RAYCAST_SPIN_IGNORED" ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("Spin COLLECT raycast breached state gate");
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            paysPage = 0;
            ClickThroughUiRaycast(ideckCashoutButton);
            RefreshDiagnosticUi();
            if (paysPage != -1 || !collectConfirmPending || pcCollectPending ||
                !ideckCollectConfirmControls.activeInHierarchy ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("Idle COLLECT raycast skipped confirmation");
            if (reportMeter)
            {
                ScreenCapture.CaptureScreenshot(PaymentSmokeArg("-roulettePcPaymentMessageOutput") + ".confirm.png");
                yield return new WaitForEndOfFrame();
            }
            long collectStart = collectConfirmStartedTimestamp;
            long collectFrequency = System.Diagnostics.Stopwatch.Frequency;
            TickPcCollectConfirmation(collectStart + collectFrequency * 1100 / 1000);
            if (!ideckCollectYesButton.targetGraphic.enabled ||
                ideckCollectNoButton.targetGraphic.enabled)
                throw new InvalidOperationException("COLLECT first blink differs from Menu");
            TickPcCollectConfirmation(collectStart + collectFrequency * 2100 / 1000);
            if (ideckCollectYesButton.targetGraphic.enabled ||
                !ideckCollectNoButton.targetGraphic.enabled)
                throw new InvalidOperationException("COLLECT second blink differs from Menu");
            TickPcCollectConfirmation(collectStart + collectFrequency * 100 / 1000);
            yield return null;
            ClickThroughUiRaycast(ideckCollectNoButton);
            if (collectConfirmPending || pcCollectPending || session.Credit != credit ||
                session.TotalBet != wager)
                throw new InvalidOperationException("COLLECT NO changed ledger");
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            ClickThroughUiRaycast(ideckCashoutButton);
            collectConfirmDeadlineTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            TickPcCollectConfirmation(System.Diagnostics.Stopwatch.GetTimestamp());
            if (collectConfirmPending || pcCollectPending || session.Credit != credit ||
                session.TotalBet != wager)
                throw new InvalidOperationException("COLLECT timeout changed ledger");
            SetIDeckPreview(true);
            RefreshDiagnosticUi();
            yield return null;
            ClickThroughUiRaycast(ideckCashoutButton);
            RefreshDiagnosticUi();
            yield return null;
            TickPcCollectConfirmation(collectConfirmStartedTimestamp +
                System.Diagnostics.Stopwatch.Frequency * 2100 / 1000);
            if (ideckCollectYesButton.targetGraphic.enabled)
                throw new InvalidOperationException("COLLECT hidden YES setup failed");
            ClickThroughUiRaycast(ideckCollectYesButton);
            if (collectConfirmPending || !pcCollectPending ||
                session.Credit != credit || session.TotalBet != wager)
                throw new InvalidOperationException("COLLECT YES skipped request stage");
            for (int frame = 0; frame < 5 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || session.Credit != 0)
                throw new InvalidOperationException("Raycast COLLECT did not finish");
            if (reportMeter)
            {
                RefreshPortraitUi(); yield return null; RefreshPortraitUi();
                PaymentSmokeRequire(ReadPcCreditEvent().phase == 3 && pcCollectMeterCredits == 1000 &&
                    PcCollectMeterVisible == !auNationalStandardsEnabled && !ideckPreview && !collectConfirmPending,
                    "raycast completion publishes exactly one owned collect");
                PaymentSmokeRequire(uiWinTitle.Value == (auNationalStandardsEnabled ? "WIN" : "COLLECT") &&
                    uiPaid.Value == (auNationalStandardsEnabled ? "0" : "1000") &&
                    uiCashPaid.Value == (auNationalStandardsEnabled ? "RM0.00" : "RM10.00") &&
                    uiPaid.GlyphCount == (auNationalStandardsEnabled ? 1 : 4), "raycast main meter glyphs");
                PaymentSmokeRequire(recordedPockets.Count == 0 && session.Rounds == 0 &&
                    (auNationalStandardsEnabled || (footerMoviePhase == FooterMoviePhase.Hidden && !uiFooterMovie.gameObject.activeSelf)),
                    "deck callback does not draw or cover collect");
                ScreenCapture.CaptureScreenshot(PaymentSmokeArg("-roulettePcPaymentMessageOutput") + ".png");
                yield return new WaitForEndOfFrame();
                SaveCollectMeterSmokeReport("meterdeck_live", 16);
            }
            Debug.Log("ROULETTE_COLLECT_RAYCAST_PASSED: SERVICE and COLLECT hit targets, YES/NO/blink/timeout, spin gate, virtual completion");
            Application.Quit();
        }

        private IEnumerator IDeckSmoke()
        {
            yield return null;
            yield return null;
            if (!ideckSecondDisplay)
            {
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
                yield return null;
            }
            if (!ideckCanvas.enabled)
                throw new InvalidOperationException("iDeck smoke has no active canvas");
            string[] pictureFonts = { "ideck_title", "ideck_service", "ideck_autoplay",
                "ideck_cashout", "ideck_double_decision", "ideck_confirm_notify",
                "ideck_confirm_query", "ideck_confirm_yes_no" };
            foreach (string name in pictureFonts)
            {
                OriginalBitmapFont font = OriginalBitmapFont.Load(name);
                Rect glyph;
                int glyphWidth;
                if (font == null || !font.TryGetGlyph('A', out glyph, out glyphWidth) ||
                    glyphWidth <= 0)
                    throw new InvalidOperationException("Missing source iDeck picture font: " + name);
            }
            Debug.Log("ROULETTE_IDECK_SOURCE_FONTS_PASSED: eight original atlas bindings");
            string[] sourceButtons = { "ButtonBetPer01", "ButtonBetPer02",
                "ButtonBetPer03", "ButtonBetPer04", "ButtonAutoPlay",
                "ButtonTakeWin", "ButtonDoubleUp", "ButtonService",
                "ButtonCashout" };
            foreach (string sourceButton in sourceButtons)
                for (int i = 1; i <= 4; ++i)
                {
                    Texture2D frame = Resources.Load<Texture2D>(
                        "Original/IDeck/" + sourceButton + "_" + i);
                    if (frame == null)
                        throw new InvalidOperationException("Missing original iDeck button frame: " +
                            sourceButton + " #" + i);
                }
            Button chipButton = ideckChipControls.GetComponentInChildren<Button>(true);
            var chipFrames = chipButton.GetComponent<RouletteOriginalButtonFrames>();
            var chipImage = chipButton.targetGraphic as RawImage;
            var pointer = new PointerEventData(EventSystem.current);
            chipFrames.OnPointerEnter(pointer);
            if (chipImage.texture != Resources.Load<Texture2D>(
                    "Original/IDeck/ButtonBetPer01_2"))
                throw new InvalidOperationException("iDeck hover frame did not appear");
            chipFrames.OnPointerDown(pointer);
            if (chipImage.texture != Resources.Load<Texture2D>(
                    "Original/IDeck/ButtonBetPer01_3"))
                throw new InvalidOperationException("iDeck pressed frame did not appear");
            chipFrames.OnPointerUp(pointer);
            chipButton.interactable = false;
            chipFrames.Refresh();
            if (chipImage.texture != Resources.Load<Texture2D>(
                    "Original/IDeck/ButtonBetPer01_4"))
                throw new InvalidOperationException("iDeck disabled frame did not appear");
            chipButton.interactable = true;
            chipFrames.OnPointerExit(pointer);
            Debug.Log("ROULETTE_IDECK_BUTTON_STATES_PASSED: 36 original frames and interaction states");
            foreach (var item in new[] {
                new { Button = ideckServiceButton, Name = "ButtonService" },
                new { Button = ideckCashoutButton, Name = "ButtonCashout" } })
            {
                var frames = item.Button.GetComponent<RouletteOriginalButtonFrames>();
                var image = item.Button.targetGraphic as RawImage;
                if (frames == null || image.texture != Resources.Load<Texture2D>(
                        "Original/IDeck/" + item.Name + "_1"))
                    throw new InvalidOperationException("iDeck cabinet button normal frame missing");
                frames.OnPointerEnter(pointer);
                if (image.texture != Resources.Load<Texture2D>(
                        "Original/IDeck/" + item.Name + "_2"))
                    throw new InvalidOperationException("iDeck cabinet button hover frame missing");
                frames.OnPointerDown(pointer);
                if (image.texture != Resources.Load<Texture2D>(
                        "Original/IDeck/" + item.Name + "_3"))
                    throw new InvalidOperationException("iDeck cabinet button press frame missing");
                frames.OnPointerUp(pointer);
                item.Button.interactable = false;
                frames.Refresh();
                if (image.texture != Resources.Load<Texture2D>(
                        "Original/IDeck/" + item.Name + "_4"))
                    throw new InvalidOperationException("iDeck cabinet button disabled frame missing");
                item.Button.interactable = true;
                frames.OnPointerExit(pointer);
            }
            if (ideckCanvas.enabled && ideckNormalControls.activeInHierarchy &&
                !autoConfirmPending && session != null)
            {
                long originalCredit = session.Credit;
                long originalBet = session.TotalBet;
                PointerEventData serviceDrag = PressThroughUiRaycast(ideckServiceButton);
                ExecuteEvents.Execute(ideckServiceButton.gameObject, serviceDrag,
                    ExecuteEvents.initializePotentialDrag);
                serviceDrag.delta = new Vector2(4, 0);
                ExecuteEvents.Execute(ideckServiceButton.gameObject, serviceDrag,
                    ExecuteEvents.dragHandler);
                if ((ideckServiceButton.targetGraphic as RawImage).texture !=
                    Resources.Load<Texture2D>("Original/IDeck/ButtonService_1"))
                    throw new InvalidOperationException("SERVICE move did not restore deck frame");
                ReleaseUiPointer(ideckServiceButton, serviceDrag);
                if (serviceConfirmPending || pcServiceRequested)
                    throw new InvalidOperationException("Moved SERVICE touch was accepted");
                ClickThroughUiRaycast(ideckServiceButton);
                RefreshDiagnosticUi();
                if (!serviceConfirmPending ||
                    !ideckServiceConfirmControls.activeInHierarchy ||
                    session.Credit != originalCredit || session.TotalBet != originalBet)
                    throw new InvalidOperationException("PC service confirmation or ledger failed");
                long serviceStart = serviceConfirmStartedTimestamp;
                long frequency = System.Diagnostics.Stopwatch.Frequency;
                TickPcServiceConfirmation(serviceStart + frequency * 1100 / 1000);
                if (!ideckServiceYesButton.targetGraphic.enabled ||
                    ideckServiceNoButton.targetGraphic.enabled)
                    throw new InvalidOperationException("SERVICE first blink differs from Menu");
                TickPcServiceConfirmation(serviceStart + frequency * 2100 / 1000);
                if (ideckServiceYesButton.targetGraphic.enabled ||
                    !ideckServiceNoButton.targetGraphic.enabled)
                    throw new InvalidOperationException("SERVICE second blink differs from Menu");
                TickPcServiceConfirmation(serviceStart + frequency * 100 / 1000);
                if (!ideckServiceYesButton.targetGraphic.enabled ||
                    !ideckServiceNoButton.targetGraphic.enabled)
                    throw new InvalidOperationException("SERVICE initial button visibility failed");
                yield return null;
                ClickThroughUiRaycast(ideckServiceNoButton);
                if (serviceConfirmPending || pcServiceRequested)
                    throw new InvalidOperationException("SERVICE NO did not cancel");
                RefreshDiagnosticUi();
                yield return null;
                ClickThroughUiRaycast(ideckServiceButton);
                RefreshDiagnosticUi();
                yield return null;
                ClickThroughUiRaycast(ideckServiceYesButton);
                RefreshDiagnosticUi();
                if (!pcServiceRequested || serviceConfirmPending ||
                    ideckServiceButton.GetComponent<RouletteOriginalButtonFrames>() == null ||
                    (ideckServiceButton.targetGraphic as RawImage).texture !=
                    Resources.Load<Texture2D>("Original/IDeck/ButtonService_2"))
                    throw new InvalidOperationException("SERVICE YES did not select source button");
                yield return null;
                ClickThroughUiRaycast(ideckServiceButton);
                if (pcServiceRequested || serviceConfirmPending ||
                    session.Credit != originalCredit || session.TotalBet != originalBet)
                    throw new InvalidOperationException("SERVICE second press did not clear request");
                RefreshDiagnosticUi();
                yield return null;
                ClickThroughUiRaycast(ideckServiceButton);
                serviceConfirmDeadlineTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() - 1;
                TickPcServiceConfirmation(System.Diagnostics.Stopwatch.GetTimestamp());
                if (serviceConfirmPending || pcServiceRequested)
                    throw new InvalidOperationException("SERVICE confirmation timeout failed");
                Debug.Log("ROULETTE_PC_SERVICE_FLOW_PASSED: original confirmation, YES/NO/1000ms blink/timeout, selected state, no ledger change");
                if (!ideckSecondDisplay) SetIDeckPreview(true);
                RefreshDiagnosticUi();
                yield return null;
                PointerEventData collectDrag = PressThroughUiRaycast(ideckCashoutButton);
                ExecuteEvents.Execute(ideckCashoutButton.gameObject, collectDrag,
                    ExecuteEvents.initializePotentialDrag);
                collectDrag.delta = new Vector2(4, 0);
                ExecuteEvents.Execute(ideckCashoutButton.gameObject, collectDrag,
                    ExecuteEvents.dragHandler);
                if ((ideckCashoutButton.targetGraphic as RawImage).texture !=
                    Resources.Load<Texture2D>("Original/IDeck/ButtonCashout_1"))
                    throw new InvalidOperationException("COLLECT move did not restore deck frame");
                ReleaseUiPointer(ideckCashoutButton, collectDrag);
                if (collectConfirmPending || pcCollectPending ||
                    session.Credit != originalCredit)
                    throw new InvalidOperationException("Moved COLLECT touch was accepted");
                paysPage = 0;
                status = "COLLECT_IGNORED_DURING_SPIN";
                spinning = true;
                HandlePcCashoutRelease();
                spinning = false;
                if (paysPage != -1 || status != "COLLECT_IGNORED_DURING_SPIN" ||
                    session.Credit != originalCredit || session.TotalBet != originalBet)
                    throw new InvalidOperationException("COLLECT was accepted during a spin");
                if (!ideckSecondDisplay) SetIDeckPreview(true);
                RefreshDiagnosticUi();
                yield return null;
                paysPage = 0;
                ClickThroughUiRaycast(ideckCashoutButton);
                RefreshDiagnosticUi();
                if (paysPage != -1 || !collectConfirmPending || pcCollectPending ||
                    session.Credit != originalCredit || session.TotalBet != originalBet)
                    throw new InvalidOperationException("PC collect skipped source confirmation");
                yield return null;
                ClickThroughUiRaycast(ideckCollectYesButton);
                if (collectConfirmPending || !pcCollectPending ||
                    session.Credit != originalCredit)
                    throw new InvalidOperationException("PC collect YES skipped request admission");
                for (int frame = 0; frame < 5 && pcCollectPending; ++frame)
                    yield return null;
                if (pcCollectPending || session.Credit != 0)
                    throw new InvalidOperationException("PC collect completion did not clear credit");
                Debug.Log("ROULETTE_IDECK_CABINET_BUTTONS_PASSED: Pays closes, spin gate, virtual payout completed");
                session = new RouletteSession(session.Profile, 1000, outcome)
                    { DoubleEnabled = true };
                RestartSourcePrompt(PromptKind.PlaceBets);
                if (!ideckSecondDisplay) SetIDeckPreview(true);
                RefreshDiagnosticUi();
            }
            if (ideckCanvas.enabled && ideckChipControls.activeInHierarchy &&
                !autoConfirmPending && session != null && !session.InDoubleUp)
            {
                yield return new WaitForSecondsRealtime(1.1f);
                // Single-display iDeck preview pauses portrait rendering.
                // Initialize a running message on Screen 1, then restore
                // the iDeck preview for the real button raycast.
                SetIDeckPreview(false);
                RefreshPortraitUi();
                if (shownPrompt != PromptKind.PlaceBets ||
                    promptPhase == PromptPlaybackPhase.None)
                    throw new InvalidOperationException("iDeck prompt continuity setup failed");
                long originalPromptAt = promptPhaseTimestamp;
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
                yield return null;
                Button[] chipButtons = ideckChipControls.GetComponentsInChildren<Button>(true);
                long previousChip = session.Chip;
                PointerEventData chipPress = PressThroughUiRaycast(chipButtons[1]);
                if (session.Chip != chips[1] || ideckPreview)
                    throw new InvalidOperationException(
                        "iDeck chip button did not select on touch-down");
                ReleaseUiPointer(chipButtons[1], chipPress);
                if (session.Chip != chips[1])
                    throw new InvalidOperationException(
                        "iDeck chip selection changed on touch release");
                RefreshPortraitUi();
                if (shownPrompt != PromptKind.PlaceBets ||
                    promptPhaseTimestamp != originalPromptAt)
                    throw new InvalidOperationException(
                        "iDeck chip selection restarted the main-screen message");
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
                var selectedImage = chipButtons[1].targetGraphic as RawImage;
                if (selectedImage.texture != Resources.Load<Texture2D>(
                        "Original/IDeck/ButtonBetPer02_2") ||
                    chipImage.texture != Resources.Load<Texture2D>(
                        "Original/IDeck/ButtonBetPer01_1"))
                    throw new InvalidOperationException("iDeck selected chip frame did not persist");
                RefreshPortraitUi();
                if (uiChipSelections[1].texture != originalMainChipButtonFrames[1][1] ||
                    uiChipSelections[0].texture != originalMainChipButtonFrames[0][0])
                    throw new InvalidOperationException("Main-table chip selection did not follow iDeck");
                uiChipButtonStates[0].OnPointerEnter(pointer);
                if (uiChipSelections[0].texture != originalMainChipButtonFrames[0][0])
                    throw new InvalidOperationException("Unselected main chip changed frame on hover");
                uiChipButtonStates[0].OnPointerExit(pointer);
                uiChipButtonStates[1].OnPointerDown(pointer);
                if (uiChipSelections[1].texture != originalMainChipButtonFrames[1][1])
                    throw new InvalidOperationException("Main-table chip selection changed on press");
                uiChipButtonStates[1].OnPointerUp(pointer);
                if (uiChipSelections[1].texture != originalMainChipButtonFrames[1][1])
                    throw new InvalidOperationException("Main-table chip selected frame did not return");
                spinning = true;
                RefreshPortraitUi();
                if (uiChipSelections[1].texture != originalMainChipButtonFrames[1][1] ||
                    uiChipSelections[0].texture != originalMainChipButtonFrames[0][2] ||
                    uiChipButtonStates[0].GetComponent<Button>().interactable)
                    throw new InvalidOperationException("Main-table chip spin lock frames failed: " +
                        uiChipSelections[1].texture.name + ", " +
                        uiChipSelections[0].texture.name);
                spinning = false;
                RefreshPortraitUi();
                if (uiChipSelections[0].texture != originalMainChipButtonFrames[0][0] ||
                    !uiChipButtonStates[0].GetComponent<Button>().interactable)
                    throw new InvalidOperationException("Main-table chip input did not unlock");
                if (ideckTouchChipIndex != 1 || ideckTouchStartedTimestamp == 0 ||
                    !ideckChipTouchImages[1].gameObject.activeSelf ||
                    ideckChipTouchImages[1].raycastTarget ||
                    ideckChipTouchImages[1].rectTransform.anchoredPosition != Vector2.zero ||
                    ideckChipTouchImages[1].rectTransform.sizeDelta != new Vector2(188, 188) ||
                    ideckChipTouchImages.Where((image, index) => index != 1)
                        .Any(image => image.gameObject.activeSelf))
                    throw new InvalidOperationException("Source changed-chip touch animation did not start alone");
                long touchStarted = ideckTouchStartedTimestamp;
                RefreshIDeckTouchAnimation(OriginalRackClock.AfterMilliseconds(
                    touchStarted, 294, System.Diagnostics.Stopwatch.Frequency));
                if (ideckChipTouchImages[1].texture != ideckTouchFrames[7])
                    throw new InvalidOperationException("Source iDeck touch frame interval differs");
                string[] touchArgs = Environment.GetCommandLineArgs();
                int touchCapture = Array.IndexOf(touchArgs, "-rouletteIDeckTouchCapture");
                if (touchCapture >= 0 && touchCapture + 1 < touchArgs.Length)
                    CaptureActiveScreenOffscreen(touchArgs[touchCapture + 1]);
                // Preview switches reactivate the canvas and its graphics.
                // Let Unity register them before the next synthetic touch.
                yield return null;
                PointerEventData sameChipPress = PressThroughUiRaycast(chipButtons[1]);
                if (ideckTouchChipIndex != -1 || session.Chip != chips[1] ||
                    ideckChipTouchImages.Any(image => image.gameObject.activeSelf))
                    throw new InvalidOperationException("Source same-chip press did not stop without replay");
                ReleaseUiPointer(chipButtons[1], sameChipPress);
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
                yield return null;
                PointerEventData otherChipPress = PressThroughUiRaycast(chipButtons[2]);
                if (ideckTouchChipIndex != 2 || session.Chip != chips[2] ||
                    ideckChipTouchImages[1].gameObject.activeSelf)
                    throw new InvalidOperationException("Source touch animation did not move to the changed chip");
                ReleaseUiPointer(chipButtons[2], otherChipPress);
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
                touchStarted = ideckTouchStartedTimestamp;
                RefreshIDeckTouchAnimation(OriginalRackClock.AfterMilliseconds(
                    touchStarted, 41, System.Diagnostics.Stopwatch.Frequency));
                if (ideckChipTouchImages[2].texture != ideckTouchFrames[0])
                    throw new InvalidOperationException("Touch frame advanced before 42 ms");
                RefreshIDeckTouchAnimation(OriginalRackClock.AfterMilliseconds(
                    touchStarted, 42, System.Diagnostics.Stopwatch.Frequency));
                if (ideckChipTouchImages[2].texture != ideckTouchFrames[1])
                    throw new InvalidOperationException("Touch frame did not advance at 42 ms");
                RefreshIDeckTouchAnimation(OriginalRackClock.AfterMilliseconds(
                    touchStarted, 1007, System.Diagnostics.Stopwatch.Frequency));
                if (ideckChipTouchImages[2].texture != ideckTouchFrames[23])
                    throw new InvalidOperationException("Final source touch frame was not retained");
                RefreshIDeckTouchAnimation(OriginalRackClock.AfterMilliseconds(
                    touchStarted, 1008, System.Diagnostics.Stopwatch.Frequency));
                if (ideckTouchChipIndex != -1 ||
                    ideckChipTouchImages.Any(image => image.gameObject.activeSelf))
                    throw new InvalidOperationException("Source one-pass touch animation did not finish");
                Debug.Log("ROULETTE_IDECK_TOUCH_ANIMATION_PASSED: changed chip, same-chip stop, peers, 24 frames at 42 ms, completion");
                session.SetChip(previousChip);
                RefreshDiagnosticUi();
                RefreshPortraitUi();
                Debug.Log("ROULETTE_IDECK_CHIP_RAYCAST_PASSED: original chip button selects chip 2");
                Debug.Log("ROULETTE_IDECK_CHIP_SELECTION_PASSED: selected frame follows active chip");
                Debug.Log("ROULETTE_IDECK_PROMPT_CONTINUITY_PASSED: source chip event leaves main message running");
                Debug.Log("ROULETTE_MAIN_CHIP_SPIN_LOCK_PASSED: selected chip retained, peers disabled, inputs restored");
            }
            // Both source decision buttons execute on touch-down. Give each
            // its own winning round, then check that release does not repeat.
            if (ideckCanvas.enabled && session != null && !session.InDoubleUp)
            {
                for (int i = 0; i < 2; ++i)
                {
                    session = new RouletteSession(session.Profile, 1000,
                        new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
                    session.PlaceBet(100, 1);
                    session.Spin();
                    SetIDeckPreview(true);
                    RefreshDiagnosticUi();
                    yield return null;
                    if (!session.DoubleAvailable ||
                        !ideckDoubleControls.activeInHierarchy)
                        throw new InvalidOperationException(
                            "iDeck Double decision smoke did not reach a win");
                    Button[] decisions = ideckDoubleControls.GetComponentsInChildren<Button>(true);
                    if (decisions.Length != 2)
                        throw new InvalidOperationException("iDeck Double decision buttons are missing");
                    PointerEventData press = PressThroughUiRaycast(decisions[i]);
                    long creditAfterDown = session.Credit;
                    float entranceAfterDown = doubleEntranceStartedAt;
                    if (ideckPreview || i == 0 && session.ResultPhase ||
                        i == 1 && !session.InDoubleUp)
                        throw new InvalidOperationException(
                            "iDeck Double decision waited until touch release");
                    ReleaseUiPointer(decisions[i], press);
                    if (session.Credit != creditAfterDown ||
                        i == 0 && session.ResultPhase ||
                        i == 1 && (!session.InDoubleUp ||
                            doubleEntranceStartedAt != entranceAfterDown))
                        throw new InvalidOperationException(
                            "iDeck Double decision repeated on release");
                }
                Debug.Log("ROULETTE_IDECK_DECISION_TOUCH_DOWN_PASSED: TAKE WIN and DOUBLE UP centres, no release repeat");
                session = new RouletteSession(session.Profile, 1000, outcome)
                    { DoubleEnabled = true };
                SetIDeckPreview(true);
                RefreshDiagnosticUi();
            }
            if (autoConfirmPending)
            {
                ClickThroughUiRaycast(ideckAutoYesButton);
                if (autoConfirmPending || !autoplay)
                    throw new InvalidOperationException("iDeck AUTO confirmation did not start Autoplay");
                Debug.Log("ROULETTE_IDECK_AUTOPLAY_FLOW_PASSED");
                if (session == null || session.TotalBet == 0)
                {
                    yield return null;
                    yield return null;
                    if (autoplay)
                        throw new InvalidOperationException(
                            "iDeck AutoPlay stayed on without a staged wager");
                    Debug.Log("ROULETTE_IDECK_AUTOPLAY_EMPTY_STOP_PASSED");
                }
            }
            else if (session != null && session.InDoubleUp)
            {
                ClickThroughUiRaycast(ideckRedButton);
                if (session.LastDoubleCard < 0)
                    throw new InvalidOperationException("iDeck RED did not reveal a card");
                Debug.Log("ROULETTE_IDECK_DOUBLE_FLOW_PASSED");
            }
            Debug.Log("ROULETTE_IDECK_SMOKE_PASSED");
            Application.Quit();
        }

        private IEnumerator IDeckAttractSmoke()
        {
            yield return null;
            if (!ideckAttractPreview || !ideckCanvas.enabled ||
                ideckAttractBackgroundFrames == null ||
                ideckAttractBackgroundFrames.Length != 180 ||
                ideckAttractForegroundFrames == null ||
                ideckAttractForegroundFrames.Length != 105 ||
                ideckNormalControls.activeSelf)
                throw new InvalidOperationException("iDeck source attract resources/state missing");
            int background, foreground;
            IDeckAttractFramesAt(0f, out background, out foreground);
            if (background != 0 || foreground != 0)
                throw new InvalidOperationException("iDeck attract group 0 did not start together");
            IDeckAttractFramesAt(105 * IDeckAttractFrameSeconds + .001f,
                out background, out foreground);
            if (background != 105 || foreground != 104)
                throw new InvalidOperationException("iDeck attract foreground did not hold its last frame");
            IDeckAttractFramesAt(180 * IDeckAttractFrameSeconds + .001f,
                out background, out foreground);
            if (background != 0 || foreground != -1)
                throw new InvalidOperationException("iDeck attract background-only group missing");
            IDeckAttractFramesAt(360 * IDeckAttractFrameSeconds + .001f,
                out background, out foreground);
            if (background != 0 || foreground != 0)
                throw new InvalidOperationException("iDeck attract sequence did not repeat");
            Debug.Log("ROULETTE_IDECK_ATTRACT_PASSED: source group order and 66ms frames");
        }

        private IEnumerator TopperResultSmoke()
        {
            yield return null;
            if (recordedPockets.Count == 0 && topperResultSpot.enabled)
                throw new InvalidOperationException("Topper showed an outcome before settlement");
            while (spinning) yield return null;
            yield return null;
            if (session == null || session.LastResult == null ||
                recordedPockets.Count == 0 ||
                recordedPockets[0] != session.LastResult.Pocket ||
                topperResultSpot == null || !topperResultSpot.enabled ||
                topperResultFont == null || topperResultNumber == null ||
                topperResultNumber.GlyphCount !=
                    session.LastResult.Pocket.ToString().Length)
                throw new InvalidOperationException("Topper result did not follow settled history");
            int pocket = session.LastResult.Pocket;
            Texture2D expected = pocket == 0 ? topperSpotGreen :
                Array.IndexOf(RedNumbers, pocket) >= 0 ? topperSpotRed : topperSpotBlack;
            if (topperResultSpot.texture != expected)
                throw new InvalidOperationException("Topper result used the wrong original spot color");
            if (topperMovie.enabled)
                throw new InvalidOperationException("Attract movie remained visible during gameplay");
            SetTopperAttractPreview();
            yield return null;
            if (!topperMovie.enabled || topperResultSpot.enabled ||
                topperResultNumber.GlyphCount != 0)
                throw new InvalidOperationException("Attract mode did not hide the result feature");
            SetTopperPreview(true);
            yield return null;
            if (topperMovie.enabled || !topperResultSpot.enabled ||
                topperResultNumber.GlyphCount != pocket.ToString().Length)
                throw new InvalidOperationException("Gameplay topper did not resume after attract");
            BeginSpin();
            if (!spinning || recordedPockets[0] != pocket)
                throw new InvalidOperationException("Next spin changed topper history early");
            yield return null;
            if (!topperResultSpot.enabled || topperResultSpot.texture != expected ||
                topperResultNumber.GlyphCount != pocket.ToString().Length)
                throw new InvalidOperationException("Topper did not retain previous outcome during spin");
            while (spinning) yield return null;
            yield return null;
            int nextPocket = session.LastResult.Pocket;
            Texture2D nextExpected = nextPocket == 0 ? topperSpotGreen :
                Array.IndexOf(RedNumbers, nextPocket) >= 0 ? topperSpotRed : topperSpotBlack;
            if (recordedPockets.Count < 2 || recordedPockets[0] != nextPocket ||
                recordedPockets[1] != pocket || topperResultSpot.texture != nextExpected ||
                topperResultNumber.GlyphCount != nextPocket.ToString().Length)
                throw new InvalidOperationException("Topper did not replace previous result after second settlement");
            Debug.Log("ROULETTE_TOPPER_RESULT_PASSED: previous=" + pocket +
                " latest=" + nextPocket + " font=" + topperResultFont.Texture.name);
        }
    }
}
