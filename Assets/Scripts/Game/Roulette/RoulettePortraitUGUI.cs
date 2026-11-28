using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    // The cabinet-sized playing surface and auxiliary pages use uGUI.
    public sealed partial class RouletteGame
    {
        private sealed class PictureLabel
        {
            private readonly RectTransform root;
            private readonly OriginalBitmapFont font;
            private readonly OriginalBitmapFont[] fontCandidates;
            private readonly bool nativeSize;
            private readonly int horizontalAlign;
            private readonly bool alignToRectEdge;
            private readonly bool raycastTarget;
            private readonly int verticalAlign;
            private string current;
            private CanvasGroup opacityGroup;
            public int GlyphCount { get { return root.childCount; } }
            public string Value { get { return current; } }
            public bool Visible { get { return root.gameObject.activeSelf; } }
            public RectTransform Root { get { return root; } }
            public OriginalBitmapFont SelectedFont { get; private set; }
            // Keep opacity on the label, so Set() can replace its glyphs
            // without resetting the ProView Paid meter's timer-driven alpha.
            public float Opacity
            {
                get { return opacityGroup == null ? 1f : opacityGroup.alpha; }
                set
                {
                    if (opacityGroup == null)
                    {
                        if (value == 1f) return;
                        opacityGroup = root.gameObject.AddComponent<CanvasGroup>();
                    }
                    opacityGroup.alpha = value;
                }
            }

            public void SetVisible(bool visible)
            {
                root.gameObject.SetActive(visible);
            }

            public PictureLabel(RectTransform parent, Rect bounds, OriginalBitmapFont font,
                string name = "Bitmap text", bool nativeSize = false,
                int horizontalAlign = 0, bool raycastTarget = false,
                bool alignToRectEdge = false,
                OriginalBitmapFont[] fontCandidates = null, int verticalAlign = 0)
            {
                root = UiRect(parent, bounds, name);
                this.font = font;
                this.nativeSize = nativeSize;
                this.horizontalAlign = horizontalAlign;
                this.raycastTarget = raycastTarget;
                this.alignToRectEdge = alignToRectEdge;
                this.fontCandidates = fontCandidates;
                this.verticalAlign = verticalAlign;
            }

            public void Set(string value)
            {
                if (current == value) return;
                current = value;
                for (int i = root.childCount - 1; i >= 0; --i)
                {
                    GameObject old = root.GetChild(i).gameObject;
                    old.SetActive(false);
                    Destroy(old);
                }
                SelectedFont = fontCandidates == null ? font :
                    OriginalBitmapFont.SelectForWidth(fontCandidates, value, root.rect.width);
                OriginalBitmapFont selected = SelectedFont;
                if (selected == null || string.IsNullOrEmpty(value)) return;
                float width = 0;
                Rect uv;
                int glyphWidth;
                foreach (int c in selected.CharacterCodes(value))
                    if (selected.TryGetGlyph(c, out uv, out glyphWidth)) width += glyphWidth;
                if (width <= 0) return;
                float scale = nativeSize || fontCandidates != null ? 1f :
                    Mathf.Min(root.rect.width / width, root.rect.height / selected.Height);
                // ToteBoardMetersPosition supplies the box centre as the
                // SpriteLabel anchor. Sprite::Align(-1) starts at that point;
                // Align(+1) ends there. The two colors occupy opposite sides.
                float x = alignToRectEdge ?
                    (horizontalAlign < 0 ? 0f :
                        horizontalAlign > 0 ? root.rect.width - width * scale :
                        (root.rect.width - width * scale) * .5f) :
                    (horizontalAlign < 0 ? root.rect.width * .5f :
                        horizontalAlign > 0 ? root.rect.width * .5f - width * scale :
                        (root.rect.width - width * scale) * .5f);
                float y = (root.rect.height - selected.Height * scale) * .5f;
                if (fontCandidates != null || verticalAlign != 0)
                {
                    // ButtonPanel and Sprite::Align use integer centres.
                    x = horizontalAlign < 0 ? 0f :
                        horizontalAlign > 0 ? root.rect.width - width :
                        Mathf.Floor(root.rect.width / 2f) - Mathf.Floor(width / 2f);
                    y = verticalAlign < 0 ? 0f :
                        verticalAlign > 0 ? root.rect.height - selected.Height :
                        Mathf.Floor(root.rect.height / 2f) - Mathf.Floor(selected.Height / 2f);
                }
                foreach (int c in selected.CharacterCodes(value))
                {
                    Texture2D glyphTexture;
                    if (!selected.TryGetGlyph(c, out glyphTexture, out uv, out glyphWidth)) continue;
                    RawImage glyph = UiImage(root,
                        new Rect(x, y, glyphWidth * scale, selected.GlyphHeight * scale),
                        glyphTexture, "Glyph");
                    glyph.uvRect = uv;
                    glyph.raycastTarget = raycastTarget;
                    x += glyphWidth * scale;
                }
            }
        }

        private static PictureLabel ProViewLabel(RectTransform parent, Rect bounds,
            OriginalBitmapFont[] fonts, string name, int horizontalAlign = 0)
        {
            return new PictureLabel(parent, bounds, fonts[0], name,
                horizontalAlign: horizontalAlign, alignToRectEdge: true,
                raycastTarget: false, fontCandidates: fonts);
        }

        private static PictureLabel ProViewLabel(RectTransform parent, Rect bounds,
            OriginalBitmapFont font, string name, int horizontalAlign = 0)
        {
            return ProViewLabel(parent, bounds, new[] { font }, name, horizontalAlign);
        }

        private static PictureLabel FeatureMeterLabel(RectTransform parent, Rect bounds,
            OriginalBitmapFont font, string name)
        {
            // CMeterMgr creates a centred bitmap ScaledLabel. A single
            // registered font retains its native size, including overflow.
            return new PictureLabel(parent, bounds, font, name,
                fontCandidates: new[] { font });
        }

        private enum PromptKind { None, PlaceBets, PressSpin, GameOver, InsertCredits,
            BetLimitReached, BetSpotLimitReached }
        private enum PromptPlaybackPhase { None, Begin, Hold, End, Blank }
        private Canvas portraitCanvas;
        private RectTransform portraitRoot, betChipLayer, promptRoot, doubleRoot,
            doubleEntranceLayer;
        private RawImage uiWheel, uiSpindle, uiBall, uiPrompt, uiFooterMovie, uiResultOval;
        private RawImage uiTableNumberMark;
        private Texture2D[] tableWinZeroFrames, tableWinNonZeroFrames;
        private int tableMarkedPocket = -1;
        private long tableMarkStartedTimestamp;
        private RawImage uiOddsPanel;
        private RawImage uiTotalWinTitle;
        private RawImage uiDoubleBackground, uiDoubleMessage, uiDoubleCard;
        private RawImage uiDoubleRedButtonOverlay, uiDoubleBlackButtonOverlay;
        private RawImage uiDoubleTakeWinOverlay;
        private Button uiDoubleRedButton, uiDoubleBlackButton, uiDoubleTakeWinButton;
        private Button[] uiSuitChoiceButtons;
        private RawImage[] uiSuitChoiceOverlays;
        private RawImage uiDoubleHelpBackground, uiDoubleHelpRounds;
        private PictureLabel uiDoubleHelpWinLimit, uiPaid;
        private bool doubleExitVisualActive;
        private GameObject uiDoubleHelp;
        private Button uiDoubleHelpButton, uiDoubleHelpBlocker, uiDoubleHelpExitButton;
        private PictureLabel uiResultNumber, uiToteNumber;
        private RawImage uiToteMeterBox;
        private Texture2D[] toteMeterBoxFrames;

        private int ToteMeterBoxFrame(int pocket)
        {
            // FileDefs ToteBoardNumberInfo: zero=0, red=1, black=2.
            // ToteBoardDisplay::AddNumber calls Sprite2DMovie::SetFrame.
            return pocket == 0 ? 0 : RedNumbers.Contains(pocket) ? 1 : 2;
        }
        private PictureLabel uiTotalWinAmount;
        private PictureLabel[] uiHistory;
        private PictureLabel uiDenom, uiCreditTitle, uiCashCredit, uiCashBet, uiCashPaid,
            uiBetTitle, uiWinTitle;
        private PictureLabel uiOneCredit, uiTouch, uiDisclaimer, uiGameOver, uiLinePays, uiGamePays;
        private PictureLabel uiCredit, uiBet;
        private RawImage uiCashTitle, uiCashBetBackground, uiCashPaidBackground;
        private Text uiFeedback;
        private PictureLabel uiDoubleCredit, uiDoubleStake;
        private PictureLabel uiDoubleCurrentWin, uiDoubleToWin, uiDoubleBet, uiDoubleDenom;
        private PictureLabel uiDoubleDisclaimer;
        private PictureLabel uiSuitCreditCash, uiSuitBetCash;
        private PictureLabel uiSuitCurrentWinCash, uiSuitToWinCash;
        private RawImage[] uiChipSelections;
        private RouletteOriginalButtonFrames[] uiChipButtonStates;
        private RawImage uiBetMarker;
        private int hoveredBetSpot = -1;
        private Button[] uiBetButtons;
        private Button uiZeroBetButton, uiSpinButton, uiPromptButton, uiUndoButton,
            uiClearButton, uiDenomTouch, uiCreditMeterButton;
        private Texture2D[] mainUndoFrames, mainClearFrames;
        private RouletteOriginalButtonFrames uiUndoButtonFrames, uiClearButtonFrames;
        private Button uiVoisinsButton, uiOrphelinsButton, uiTiersButton, uiNeighborButton;
        private RawImage uiNeighborSelection, uiNeighborMeterBackground, uiNeighborTouch;
        private PictureLabel uiNeighborNumber;
        private float neighborTouchUntil;
        private GameObject uiPaysRoot;
        private RectTransform uiGameInfoOptionsRoot;
        private Button uiGameInfoSeePays, uiGameInfoVolume;
        private bool gameInfoOptionsOpen;
        private long gameInfoOptionsDeadlineTimestamp;
        private const int GameInfoOptionsTimeoutMilliseconds = 60000;
        private const string DefaultPcPlayerVolumeKey = "RouletteDeluxe.PcPlayerVolume.v1";
        private const string DefaultPcCashMeterKey = "RouletteDeluxe.PcCashMeter.v1";
        private string pcCashMeterKey = DefaultPcCashMeterKey;
        private bool pcCashMeterMode;
        private bool pcCashMeterJurisdictionEnabled;
        // 副屏物理竖装修正：台面绕 Z 旋转的角度。0 = 保持原布局。
        // 先按 90 定稿验证；启动参数 -rouletteDeckRotation 仍可覆盖。
        private const int DefaultDeckRotationDegrees = 90;
        private int deckRotationDegrees = DefaultDeckRotationDegrees;

        // 副屏是 1920x1080 的横屏面板，安装时物理转了 90°。
        // 台面按 1080x1920 编排，因此需软件反向旋转才能正视。
        // 只有 ±90 能让包围盒变成 1920x1080 并铺满；180 不改变包围盒，
        // 在横屏面板上必然左右留白且上下被裁，仅作排查用，不可部署。
        private void LoadDeckRotationMode()
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-rouletteDeckRotation");
            if (index < 0 || index + 1 >= args.Length) return;
            int degrees;
            if (!int.TryParse(args[index + 1], out degrees))
            {
                Debug.LogError("ROULETTE_DECK_ROTATION: 参数不是整数，保持默认 " +
                    deckRotationDegrees);
                return;
            }
            // 允许显式关闭，便于回归对比。
            if (degrees != 0 && degrees != 90 && degrees != -90 && degrees != 180)
            {
                Debug.LogError("ROULETTE_DECK_ROTATION: 非法角度 " + degrees +
                    "，保持默认 " + deckRotationDegrees);
                return;
            }
            deckRotationDegrees = degrees;
            if (degrees == 180)
                Debug.LogWarning("ROULETTE_DECK_ROTATION: 180 不铺满，仅用于排查");
        }

        private void LoadPcCashMeterMode()
        {
            string[] args = Environment.GetCommandLineArgs();
            int keyIndex = Array.IndexOf(args, "-rouletteCashMeterTestKey");
            if (keyIndex >= 0 && keyIndex + 1 < args.Length)
                pcCashMeterKey = args[keyIndex + 1];
            pcCashMeterMode = PlayerPrefs.GetInt(pcCashMeterKey, 0) != 0;
            // The packaged jurisdiction.bin has DisplayMeterInCashInMenu=0.
            // This diagnostic flag models a live cabinet where that bit is on.
            pcCashMeterJurisdictionEnabled =
                Array.IndexOf(args, "-rouletteCashMeterJurisdictionEnabled") >= 0;
        }

        private void TogglePcCashMeterMode()
        {
            if (PcGameOrdinaryInputBlocked) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            return;
#else
            // Game::DoToggleCredits(true) grants FileDefs::DemoOptionsCredits
            // on touch when the packaged demo show mode is enabled.
            if (demoShowMode > 0)
                AddPcVirtualCredits(2000, true);
            pcCashMeterMode = !pcCashMeterMode;
            PlayerPrefs.SetInt(pcCashMeterKey, pcCashMeterMode ? 1 : 0);
            PlayerPrefs.Save();
            RefreshPortraitUi();
#endif
        }
        private string pcPlayerVolumeKey = DefaultPcPlayerVolumeKey;
        private int pcPlayerVolumeValue;
        private bool pcGameSoundsMuted;
        private long pcPlayerVolumeIdleDeadlineTimestamp;
        private const int PcPlayerVolumeIdleTimeoutMilliseconds = 90000;

        private void LoadPcPlayerVolume()
        {
            string[] args = Environment.GetCommandLineArgs();
            int keyIndex = Array.IndexOf(args, "-rouletteVolumeTestKey");
            if (keyIndex >= 0 && keyIndex + 1 < args.Length)
                pcPlayerVolumeKey = args[keyIndex + 1];
            int saved = PlayerPrefs.GetInt(pcPlayerVolumeKey, 0);
            pcPlayerVolumeValue = saved == 5 || saved == 10 ? saved : 0;
            ApplyPcPlayerVolume();
        }

        private void ApplyPcPlayerVolume()
        {
            // Packaged OS playerVolume.cfg, master-fader row 4: 0/4/4 dB.
            // Normalize its 4 dB high setting to Unity's 1.0 output ceiling.
            float gainDb = pcPlayerVolumeValue == 0 ? 0f : 4f;
            AudioListener.volume = pcGameSoundsMuted ? 0f :
                Mathf.Pow(10f, (gainDb - 4f) / 20f);
            if (proViewVolumeFrames != null && proViewVolumeFrames.Length == 3)
            {
                footerVolumeButton = proViewVolumeFrames[pcPlayerVolumeValue / 5];
                if (uiGameInfoVolume != null)
                    ((RawImage)uiGameInfoVolume.targetGraphic).texture = footerVolumeButton;
            }
        }

        private void SetPcGameSoundsMuted(bool muted)
        {
            // GameSounds stores its TurnOn/TurnOff gate at +8. PlayerVolume
            // controls a separate gain and must preserve that gate.
            pcGameSoundsMuted = muted;
            ApplyPcPlayerVolume();
        }

        private void AdvancePcPlayerVolume()
        {
            pcPlayerVolumeValue = pcPlayerVolumeValue == 0 ? 5 :
                pcPlayerVolumeValue == 5 ? 10 : 0;
            PlayerPrefs.SetInt(pcPlayerVolumeKey, pcPlayerVolumeValue);
            PlayerPrefs.Save();
            ApplyPcPlayerVolume();
        }

        private void StartPcPlayerVolumeIdleTimeout()
        {
            // OS PlayerVolume starts this timer after cashout completion or
            // a zero-credit game-application-ready event.
            if (session == null || session.Credit != 0) return;
            pcPlayerVolumeIdleDeadlineTimestamp = ResultClockDeadline(
                System.Diagnostics.Stopwatch.GetTimestamp(),
                PcPlayerVolumeIdleTimeoutMilliseconds / 1000f,
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void CancelPcPlayerVolumeIdleTimeout()
        {
            // The source 0x3100 credit event cancels the idle timer.
            pcPlayerVolumeIdleDeadlineTimestamp = 0;
        }

        private void TickPcPlayerVolumeIdleTimeout(long nowTimestamp)
        {
            if (pcPlayerVolumeIdleDeadlineTimestamp == 0 ||
                nowTimestamp < pcPlayerVolumeIdleDeadlineTimestamp) return;
            pcPlayerVolumeIdleDeadlineTimestamp = 0;
            pcPlayerVolumeValue = 0;
            PlayerPrefs.SetInt(pcPlayerVolumeKey, 0);
            PlayerPrefs.Save();
            ApplyPcPlayerVolume();
            Debug.Log("ROULETTE_PC_VOLUME_IDLE_RESET: 0 after 90000ms");
        }
        private RawImage uiPaysImage, uiPaysLines;
        private Button uiInfoButton, uiPaysBack, uiPaysNext, uiPaysExit;
        private Texture2D[] proViewVolumeFrames;

        private Texture2D[] BindProViewButtonFrames(Button button, string name)
        {
            Texture2D[] frames = Resources.LoadAll<Texture2D>(
                "Original/ProViewStates/" + name).OrderBy(frame => frame.name).ToArray();
            if (frames.Length != 4)
                throw new InvalidOperationException("Original ProView button frames missing: " + name);
            // Button::UpdateImage: neutral=0, depressed=1, selected=2,
            // disabled=3. Touch movement has no mouse-hover picture state.
            button.gameObject.AddComponent<RouletteOriginalButtonFrames>()
                .Configure(button, frames, showHoverFrame: false, pressFrameIndex: 1,
                    selectedFrameIndex: 2);
            return frames;
        }
        private int paysClickCount;
        private long paysTimeoutDeadlineTimestamp;
        private const int PaysTimeoutMilliseconds = 60000;

        private void PlayPaysClick()
        {
            // GameSounds ID 0x11 uses ClickSnd, falling back to VolumeSnd.
            // The packaged VolumeSnd is the extracted Button_Click.wav.
            if (audioSource == null || click == null) return;
            audioSource.PlayOneShot(click, 1f);
            paysClickCount++;
        }

        private void ResetPaysTimeout()
        {
            EnsurePcPaysFlash();
            paysTimeoutDeadlineTimestamp = ResultClockDeadline(
                System.Diagnostics.Stopwatch.GetTimestamp(),
                PaysTimeoutMilliseconds / 1000f,
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void ClosePays()
        {
            StopPcPaysFlash();
            bool returning = paysPage >= 0;
            if (returning) RestoreSourceTableInput();
            paysPage = -1;
            paysTimeoutDeadlineTimestamp = 0;
            if (returning && SourceDoubleOfferActive) StartSourceOfferPrompt();
        }

        private void ReturnFromPays()
        {
            if (paysPage < 0 || PcPaysReturnPending) return;
            EnsurePcPaysFlash();
            PlayPaysClick();
            paysTimeoutDeadlineTimestamp = 0;
            pcPaysFlash.RequestReturnAt(System.Diagnostics.Stopwatch.GetTimestamp());
        }

        private void TickPaysTimeout(long nowTimestamp)
        {
            if (paysPage >= 0 && paysTimeoutDeadlineTimestamp > 0 &&
                nowTimestamp >= paysTimeoutDeadlineTimestamp)
                ReturnFromPays();
        }

        private void CloseGameInfoOptions()
        {
            gameInfoOptionsOpen = false;
            gameInfoOptionsDeadlineTimestamp = 0;
        }

        private void ToggleGameInfoOptions()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (SourceDoubleOfferActive && !SourceOfferDecisionDispatchAllowed) return;
            if (spinning || (ResultDisplayActive && !SourceDoubleOfferActive) || session == null ||
                session.InDoubleUp || paysPage >= 0) return;
            PlayPaysClick();
            if (gameInfoOptionsOpen)
            {
                CloseGameInfoOptions();
                return;
            }
            gameInfoOptionsOpen = true;
            gameInfoOptionsDeadlineTimestamp = ResultClockDeadline(
                System.Diagnostics.Stopwatch.GetTimestamp(),
                GameInfoOptionsTimeoutMilliseconds / 1000f,
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void OpenPaysFromGameInfo()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!gameInfoOptionsOpen || paysPage >= 0) return;
            PurgeSourcePlayerInputs();
            PlayPaysClick();
            CloseGameInfoOptions();
            paysPage = 0;
            sourceTableInputEnabled = false;
            ResetPaysTimeout();
            PurgeSourcePlayerInputs();
        }

        private void TogglePcPlayerVolume()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (!gameInfoOptionsOpen) return;
            AdvancePcPlayerVolume();
        }

        private void TickGameInfoOptions(long nowTimestamp)
        {
            if (!gameInfoOptionsOpen) return;
            if (gameInfoOptionsDeadlineTimestamp > 0 &&
                nowTimestamp >= gameInfoOptionsDeadlineTimestamp)
            {
                CloseGameInfoOptions();
                return;
            }
            // Its independent timeout can expire while App is suspended.
            // Pointer input remains queued for the resume purge instead.
            if (PcGameOrdinaryInputBlocked || uiGameInfoOptionsRoot == null) return;
            // 轮盘画布在副屏，裸 Input 表示的是主屏指针；绑定模式下改用按屏按下点，
            // 否则主屏任意点击都会关掉副屏这个菜单。
            if (RouletteTouchFeed.Bound)
            {
                Vector2 boundPress;
                if (RouletteTouchFeed.TryGetPress(out boundPress) &&
                    !RectTransformUtility.RectangleContainsScreenPoint(
                        uiGameInfoOptionsRoot, boundPress, null))
                    CloseGameInfoOptions();
                return;
            }
            bool outsideMouse = Input.GetMouseButtonDown(0) &&
                !RectTransformUtility.RectangleContainsScreenPoint(
                    uiGameInfoOptionsRoot, Input.mousePosition, null);
            bool outsideTouch = false;
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began &&
                    !RectTransformUtility.RectangleContainsScreenPoint(
                        uiGameInfoOptionsRoot, touch.position, null))
                    outsideTouch = true;
            }
            if (outsideMouse || outsideTouch) CloseGameInfoOptions();
        }
        private OriginalBitmapFont chipStackFont, cyclePayFont;
        private Texture2D[] placeBetsIn, placeBetsOut;
        private Texture2D[] insertCreditsIn, insertCreditsOut;
        private Texture2D[] betLimitIn, betLimitOut, betSpotLimitIn, betSpotLimitOut;
        private PromptKind failurePrompt;
        private long failurePromptStartedTimestamp;
        private PromptKind shownPrompt, promptStartAfterFailure, promptStartNow;
        private PromptKind sourceNamedPrompt;
        private int promptEligibilityMask = -1;
        private PromptPlaybackPhase promptPhase;
        private long promptPhaseTimestamp;
        private long promptReadyTimestamp;
        private long promptDelayedPlayTimestamp;
        private bool sourcePromptStopped;
        private bool forcedInsertCredits;
        // Roulette::StatePlay clears source byte 0x919d when the next game
        // starts, not when a post-result table touch creates the next bet.
        private bool gameOverMessageLatched = true;
        private long displayedStackHash = long.MinValue;

        private static RectTransform UiRect(RectTransform parent, Rect bounds, string name)
        {
            var host = new GameObject(name, typeof(RectTransform));
            RectTransform rect = host.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(bounds.x, -bounds.y);
            rect.sizeDelta = new Vector2(bounds.width, bounds.height);
            return rect;
        }

        private static RawImage UiImage(RectTransform parent, Rect bounds,
            Texture2D texture, string name)
        {
            RectTransform rect = UiRect(parent, bounds, name);
            RawImage image = rect.gameObject.AddComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            return image;
        }

        private static Text UiText(RectTransform parent, Rect bounds,
            string value, int size, Color color)
        {
            RectTransform rect = UiRect(parent, bounds, "Text");
            Text label = rect.gameObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            label.fontSize = size;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = color;
            label.raycastTarget = false;
            label.text = value;
            return label;
        }

        private Button UiButton(RectTransform parent, Rect bounds, Action action,
            Texture2D texture = null)
        {
            RectTransform rect = UiRect(parent, bounds, "Button");
            RawImage graphic = rect.gameObject.AddComponent<RawImage>();
            graphic.texture = texture == null ? Texture2D.whiteTexture : texture;
            graphic.color = texture == null ? new Color(1, 1, 1, 0) : Color.white;
            if (portraitRoot != null &&
                (parent == portraitRoot || parent.IsChildOf(portraitRoot)))
                AttachMainTouchCancel(rect.gameObject);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = graphic;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(() => {
                if (!PcGameOrdinaryInputBlocked) action();
            });
            return button;
        }

        private Button UiPointerDownButton(RectTransform parent, Rect bounds,
            Action action, Texture2D texture = null)
        {
            Button button = UiButton(parent, bounds, () => { }, texture);
            button.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(button, () => {
                    if (!PcGameOrdinaryInputBlocked) action();
                });
            return button;
        }

        private void AttachMainTouchCancel(GameObject target)
        {
            var observer = target.GetComponent<RouletteMainTouchCancel>();
            if (observer == null)
                observer = target.AddComponent<RouletteMainTouchCancel>();
            observer.Configure(OnMainScreenPointerDown);
        }

        private void OnMainScreenPointerDown()
        {
            if (PcGameOrdinaryInputBlocked) return;
            if (CancelPcPendingDeckConfirmationFromMainTouch())
                RefreshDiagnosticUi();
        }

        private static Texture2D[] LoadMainTableButtonFrames(string name)
        {
            var frames = new Texture2D[4];
            for (int i = 0; i < frames.Length; ++i)
            {
                frames[i] = Resources.Load<Texture2D>(
                    "Original/MainButtons/" + name + "_" + (i + 1));
                if (frames[i] == null)
                    throw new InvalidOperationException(
                        "Original table button frame missing: " + name + " " + (i + 1));
            }
            return frames;
        }

        private void CreatePortraitUi()
        {
            if (originalBackground == null || originalLayout == null || portraitCanvas != null) return;
            var canvasObject = new GameObject("Roulette original uGUI", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            portraitCanvas = canvasObject.GetComponent<Canvas>();
            portraitCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            portraitCanvas.sortingOrder = 20;
            // 轮盘台面固定生成在 Display2（Display.displays[1]，与 PopCanvas 同一块副屏），
            // Editor 与 Player 一致，不再回退主屏。
            portraitCanvas.targetDisplay = 1;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = deckRotationDegrees == 0
                ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1;
            if (FindObjectOfType<EventSystem>() == null)
            {
                var eventObject = new GameObject("Roulette UI events", typeof(EventSystem),
                    typeof(RouletteSourceInputModule));
                sourceInputModule = eventObject.GetComponent<RouletteSourceInputModule>();
                sourceInputModule.Configure(this);
                DontDestroyOnLoad(eventObject);
            }
            else
            {
                EventSystem events = FindObjectOfType<EventSystem>();
                sourceInputModule = events.GetComponent<RouletteSourceInputModule>();
                if (sourceInputModule == null)
                {
                    foreach (BaseInputModule existing in events.GetComponents<BaseInputModule>())
                        existing.enabled = false;
                    sourceInputModule = events.gameObject.AddComponent<RouletteSourceInputModule>();
                }
                sourceInputModule.Configure(this);
            }
            // 屏幕绑定：轮盘画布只接受它所在屏的触摸事件。
            // 该屏没有配置触摸设备时 Bound 为 false，输入退回全局 Input。
            RouletteTouchFeed.Attach(sourceInputModule, portraitCanvas.targetDisplay);
            portraitRoot = UiRect(canvasObject.GetComponent<RectTransform>(),
                new Rect(0, 0, 1080, 1920), "1080x1920 original screen");
            portraitRoot.anchorMin = portraitRoot.anchorMax = new Vector2(.5f, .5f);
            portraitRoot.pivot = new Vector2(.5f, .5f);
            portraitRoot.anchoredPosition = Vector2.zero;
            if (deckRotationDegrees != 0)
                portraitRoot.localEulerAngles = new Vector3(0, 0, deckRotationDegrees);
            UiImage(portraitRoot, new Rect(0, 0, 1080, 1920), originalBackground, "Table background");
            uiWheel = UiImage(portraitRoot, originalLayout.wheel_main.ToRect(), wheel, "Wheel");
            Rect wheelBounds = originalLayout.wheel_main.ToRect();
            SetSourceRotationPivot(uiWheel.rectTransform, wheelBounds);
            UiImage(portraitRoot, new Rect(200, 112, 544, 543), wheelShine, "Wheel shine");
            uiBall = UiImage(portraitRoot, originalLayout.wheel_ball.ToRect(),
                ball, "Ball");
            Rect spindleBounds = originalLayout.wheel_spindle.ToRect();
            uiSpindle = UiImage(portraitRoot, spindleBounds, spindle, "Spindle");
            SetSourceRotationPivot(uiSpindle.rectTransform, spindleBounds);
            UiImage(portraitRoot, new Rect(145, 41, 657, 231), wheelShadow, "Wheel shadow");
            // filedefs.xml RouletteOddsPanel01..03: same source position,
            // selected by the active 89/93/97 paytable percentage.
            uiOddsPanel = UiImage(portraitRoot, new Rect(13, 665, 176, 166),
                originalOddsPanels[profileIndex], "Original roulette odds panel");

            chipStackFont = OriginalBitmapFont.Load("chip_value");
            cyclePayFont = OriginalBitmapFont.Load("cycle_pay");
            if (cyclePayFont == null)
                throw new InvalidOperationException("Original CycleFont is missing");
            uiResultOval = UiImage(portraitRoot, new Rect(786, 811, 240, 112),
                resultSpotBlack, "Winning number oval");
            // CMeterObj's bitmap ScaledLabel chooses a font by text width;
            // SpriteLabel then draws Font::CalcSize at native glyph size.
            uiResultNumber = new PictureLabel(portraitRoot,
                new Rect(848, 820, 116, 82), toteWhiteFont,
                "Winning number", nativeSize: true);
            toteMeterBoxFrames = Resources.LoadAll<Texture2D>(
                "Animations/ToteMeterBox").OrderBy(frame => frame.name).ToArray();
            if (toteMeterBoxFrames.Length != 3 || toteMeterBoxFrames.Any(
                    frame => frame.width != 114 || frame.height != 79))
                throw new InvalidOperationException(
                    "Original tote meter box frames are incomplete");
            uiToteMeterBox = UiImage(portraitRoot, new Rect(914, 34, 114, 79),
                toteMeterBoxFrames[0], "Original latest tote box");
            uiToteNumber = new PictureLabel(portraitRoot,
                new Rect(918, 38, 106, 70), toteWhiteFont,
                "Latest tote number", nativeSize: true);
            uiHistory = new PictureLabel[19];
            for (int i = 0; i < uiHistory.Length; ++i)
                uiHistory[i] = new PictureLabel(portraitRoot,
                    new Rect(915, 121 + i * 32, 112, 29), toteYellowFont);
            // History glyph colors are chosen per pocket, so individual color
            // widgets live in this separate layer and refresh on a new result.
            RectTransform historyColors = UiRect(portraitRoot,
                new Rect(0, 0, 1080, 1920), "Colored history");
            uiColoredHistoryRoot = historyColors;

            // RouletteTable::MarkNumber plays the original 24 fps movie
            // centered on the numbered BetSpot. Its z=2 is below chips.
            tableWinZeroFrames = Resources.LoadAll<Texture2D>(
                "Animations/TableWinZero").OrderBy(frame => frame.name).ToArray();
            tableWinNonZeroFrames = Resources.LoadAll<Texture2D>(
                "Animations/TableWinNonZero").OrderBy(frame => frame.name).ToArray();
            if (tableWinZeroFrames.Length != 48 || tableWinNonZeroFrames.Length != 48)
                throw new InvalidOperationException(
                    "Original table number mark animation is incomplete");
            uiTableNumberMark = UiImage(portraitRoot,
                new Rect(0, 0, tableWinNonZeroFrames[0].width,
                    tableWinNonZeroFrames[0].height),
                tableWinNonZeroFrames[0], "Original table number mark");
            uiTableNumberMark.gameObject.SetActive(false);

            // Larger hit regions are created first. The smallest overlapping
            // original spot therefore receives the pointer event.
            uiBetButtons = new Button[originalLayout.bet_spots.Length];
            for (int i = originalBetHitOrder.Length - 1; i >= 0; --i)
            {
                int spot = originalBetHitOrder[i];
                Button betButton = UiButton(portraitRoot,
                    originalLayout.bet_spots[spot].ToRect(), () => { });
                betButton.gameObject.AddComponent<RouletteTableRelease>()
                    .Configure(betButton, spot, () => {
                        if (!TableTouchInputEnabled()) return;
                        if (spot < 37) Number(spot); else Bet(100 + spot);
                    }, HideBetMarker, () => UpdateBetHover(spot),
                    OnMainScreenPointerDown);
                uiBetButtons[spot] = betButton;
                betButton.gameObject.name = "Bet spot " + spot;
                if (spot == 0) uiZeroBetButton = betButton;
                var hover = betButton.gameObject.AddComponent<RouletteNeighborHover>();
                int hoveredSpot = spot;
                hover.OnEnter = () => UpdateBetHover(hoveredSpot);
                hover.OnDragMove = MoveBetMarker;
                hover.OnExit = pointer => {
                    if (!TableTouchInputEnabled() || pointer == null ||
                        EventSystem.current == null) return;
                    var hovered = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer, hovered);
                    if (!hovered.Any(hit => hit.gameObject
                        .GetComponentInParent<RouletteTableRelease>() != null))
                    {
                        HideBetMarker();
                        // Native HandleTableEvent emits no null hover when
                        // leaving the table. Keep the last valid straight
                        // number so one pointer can reach the neighbor button.
                    }
                };
            }
            betChipLayer = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920), "Placed chips");
            // BetMarker is a separate unscaled Table01ButtonBet sprite at
            // the hovered BetSpot center, above the placed-chip sprites.
            uiBetMarker = UiImage(portraitRoot, new Rect(0, 0, 90, 90),
                originalMainChipButtonFrames[0][0], "Original BetMarker");
            uiBetMarker.gameObject.SetActive(false);

            uiChipSelections = new RawImage[4];
            uiChipButtonStates = new RouletteOriginalButtonFrames[4];
            for (int i = 0; i < 4; ++i)
            {
                int selected = chips[i];
                Rect bounds = originalLayout.chip_buttons[i].ToRect();
                Button chipButton = UiButton(portraitRoot, bounds,
                    () => { }, originalChips[i]);
                chipButton.gameObject.AddComponent<RoulettePointerDownAction>()
                    .Configure(chipButton, () => {
                        if (PcGameOrdinaryInputBlocked || session == null ||
                            session.Chip == selected) return;
                        if (Try(() => session.SetChip(selected), betSelectedSound))
                            RefreshPortraitUi();
                    });
                uiChipSelections[i] = chipButton.targetGraphic as RawImage;
                uiChipButtonStates[i] = chipButton.gameObject
                    .AddComponent<RouletteOriginalButtonFrames>();
                uiChipButtonStates[i].Configure(chipButton,
                    originalMainChipButtonFrames[i], false, true, false);
            }
            mainUndoFrames = LoadMainTableButtonFrames("Undo");
            mainClearFrames = LoadMainTableButtonFrames("Clear");
            uiUndoButton = UiButton(portraitRoot, originalLayout.undo_button.ToRect(), () => {
                if (TableTouchInputEnabled() && TrySourceTableEvent(UndoBet, betUndoSound, undoEvent: true))
                    RestartSourcePrompt(session.Credit > 0 ?
                        PromptKind.PlaceBets : PromptKind.InsertCredits);
            }, mainUndoFrames[0]);
            uiUndoButtonFrames = uiUndoButton.gameObject
                .AddComponent<RouletteOriginalButtonFrames>();
            uiUndoButtonFrames.Configure(uiUndoButton, mainUndoFrames,
                showHoverFrame: false, pressFrameIndex: 1);
            uiClearButton = UiButton(portraitRoot, originalLayout.clear_button.ToRect(), () => {
                if (TableTouchInputEnabled() &&
                    TrySourceTableEvent(() => {
                        if (session.ResultPhase) session.NextRound();
                        session.Clear();
                        ClearNeighborSelection();
                    },
                        betClearSound))
                    RestartSourcePrompt(session.Credit > 0 ?
                        PromptKind.PlaceBets : PromptKind.InsertCredits);
            }, mainClearFrames[0]);
            uiClearButtonFrames = uiClearButton.gameObject
                .AddComponent<RouletteOriginalButtonFrames>();
            uiClearButtonFrames.Configure(uiClearButton, mainClearFrames,
                showHoverFrame: false, pressFrameIndex: 1);
            uiNeighbor = UiImage(portraitRoot, new Rect(777, 1123, 260, 260), neighborOff, "Neighbor wheel");
            uiNeighborMeterBackground = UiImage(portraitRoot,
                new Rect(839, 1265, 136, 58), neighborMeterGreen, "Selected neighbor meter");
            uiNeighborNumber = new PictureLabel(portraitRoot,
                new Rect(839, 1265, 136, 58), neighborPickFont);
            uiNeighborSelection = UiImage(portraitRoot,
                neighborSelectionLayout.selections[0].ToRect(), neighborSelected[0],
                "Original selected-neighbor highlight");
            uiNeighborTouch = UiImage(portraitRoot,
                new Rect(799, 1145, 216, 216), neighborTouch, "Original neighbor touch");
            uiNeighborButton = UiButton(portraitRoot, new Rect(775, 1123, 260, 260),
                () => { });
            uiNeighborButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(uiNeighborButton, () => {
                    if (TableTouchInputEnabled()) PlaceWheelNeighbor(selectedNeighbor);
                });
            uiNeighborButton.gameObject.name = "Place selected neighbors";
            uiVoisinsButton = UiButton(portraitRoot, new Rect(743, 1075, 112, 108),
                () => { }, voisinsButton);
            uiVoisinsButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(uiVoisinsButton, () => {
                    if (TableTouchInputEnabled()) PlaceSector(RouletteSector.Voisins);
                });
            uiVoisinsButton.gameObject.name = "Voisins sector bet";
            uiOrphelinsButton = UiButton(portraitRoot, new Rect(847, 1058, 120, 64),
                () => { }, orphelinsButton);
            uiOrphelinsButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(uiOrphelinsButton, () => {
                    if (TableTouchInputEnabled()) PlaceSector(RouletteSector.Orphelins);
                });
            uiOrphelinsButton.gameObject.name = "Orphelins sector bet";
            uiTiersButton = UiButton(portraitRoot, new Rect(959, 1075, 113, 108),
                () => { }, tiersButton);
            uiTiersButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(uiTiersButton, () => {
                    if (TableTouchInputEnabled()) PlaceSector(RouletteSector.Tiers);
                });
            uiTiersButton.gameObject.name = "Tiers sector bet";

            promptRoot = UiRect(portraitRoot, new Rect(752, 938, 310, 110), "Game message");
            uiPrompt = UiImage(promptRoot, new Rect(0, 0, 310, 110), null, "MNG frame");
            // PressSpin is an AnimMessageManager picture. The source PLAY
            // sprite is the separate panel button stored at +0x7ca0.
            uiPromptButton = UiButton(promptRoot, new Rect(0, 0, 310, 110),
                () => { });
            uiPromptButton.GetComponent<RawImage>().raycastTarget = false;
            uiPromptButton.gameObject.name = "Press Spin message visual";
            placeBetsIn = Resources.LoadAll<Texture2D>("Animations/PlaceYourBetsIn")
                .OrderBy(x => x.name).ToArray();
            placeBetsOut = Resources.LoadAll<Texture2D>("Animations/PlaceYourBetsOut")
                .OrderBy(x => x.name).ToArray();
            insertCreditsIn = Resources.LoadAll<Texture2D>("Animations/InsertCreditsIn")
                .OrderBy(x => x.name).ToArray();
            insertCreditsOut = Resources.LoadAll<Texture2D>("Animations/InsertCreditsOut")
                .OrderBy(x => x.name).ToArray();
            betLimitIn = Resources.LoadAll<Texture2D>("Animations/BetLimitReachedIn")
                .OrderBy(x => x.name).ToArray();
            betLimitOut = Resources.LoadAll<Texture2D>("Animations/BetLimitReachedOut")
                .OrderBy(x => x.name).ToArray();
            betSpotLimitIn = Resources.LoadAll<Texture2D>("Animations/BetStackLimitIn")
                .OrderBy(x => x.name).ToArray();
            betSpotLimitOut = Resources.LoadAll<Texture2D>("Animations/BetStackLimitOut")
                .OrderBy(x => x.name).ToArray();

            BuildFooterUi();
            BuildTotalWinUi();
            BuildPaysUi();
            uiFeedback = UiText(portraitRoot, new Rect(80, 1711, 920, 29), "", 22, Color.white);
            BuildDoubleUi();
            CreateHistoryPageUi();
            CreateDoubleHistoryUi();
            BuildDemoMenuUi();
            // Buttons receive the cancellation callback before their own
            // pointer action. Other source sprites can also receive a touch.
            foreach (Graphic graphic in
                portraitRoot.GetComponentsInChildren<Graphic>(true))
                if (graphic.raycastTarget)
                    AttachMainTouchCancel(graphic.gameObject);
            ScheduleIdlePromptAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
            RefreshPortraitUi();
            // CanvasScaler 由 typeof(CanvasScaler) 在构造时挂载，其 OnEnable 里的
            // Handle() 早于下方对 referenceResolution 等字段的赋值执行，因此
            // scaleFactor 会停留在按默认值算出的 1，直到下一帧 preWillRenderCanvases。
            // 诊断需要真实值，这里提前推一次布局（幂等，本帧渲染前本来也会发生）。
            Canvas.ForceUpdateCanvases();
            //检查输出
            Debug.Log($"ROULETTE_CANVAS_ROUTE: displays={Display.displays.Length} " +
                $"target={portraitCanvas.targetDisplay} " +
                $"screen={Screen.width}x{Screen.height} orientation={Screen.orientation}");
            var sc = portraitCanvas.GetComponent<CanvasScaler>();
            Debug.Log($"ROULETTE_SCALE: canvas={portraitRoot.rect.width}x{portraitRoot.rect.height} " +
                $"scale={sc.scaleFactor:F3} match={sc.matchWidthOrHeight:F1} " +
                $"d1={(Display.displays.Length > 1 ? Display.displays[1].renderingWidth + "x" + Display.displays[1].renderingHeight : "n/a")}");
            LogDeckDisplayDiagnostics(sc);
        }

        // 副屏竖装诊断。真实生效的是 Canvas.scaleFactor：
        // CanvasScaler.scaleFactor 只回读序列化字段 m_ScaleFactor，SetScaleFactor 不会写它。
        private void LogDeckDisplayDiagnostics(CanvasScaler scaler)
        {
            var canvasRect = portraitCanvas.GetComponent<RectTransform>().rect;
            Debug.Log($"ROULETTE_DECK_DIAG: displays={Display.displays.Length} " +
                $"target={portraitCanvas.targetDisplay} " +
                $"canvas={canvasRect.width:F1}x{canvasRect.height:F1} " +
                $"deck={portraitRoot.rect.width:F1}x{portraitRoot.rect.height:F1} " +
                $"scalerScale={scaler.scaleFactor:F4} canvasScale={portraitCanvas.scaleFactor:F4} " +
                $"match={scaler.matchWidthOrHeight:F2} " +
                $"ref={scaler.referenceResolution.x}x{scaler.referenceResolution.y} " +
                $"screen={Screen.width}x{Screen.height} orientation={Screen.orientation}");
            // 世界角点包围盒 = 台面在该屏 surface 上实际占据的像素尺寸。
            var corners = new Vector3[4];
            portraitRoot.GetWorldCorners(corners);
            float minX = corners[0].x, maxX = corners[0].x;
            float minY = corners[0].y, maxY = corners[0].y;
            for (int i = 1; i < 4; ++i)
            {
                minX = Mathf.Min(minX, corners[i].x); maxX = Mathf.Max(maxX, corners[i].x);
                minY = Mathf.Min(minY, corners[i].y); maxY = Mathf.Max(maxY, corners[i].y);
            }
            Debug.Log($"ROULETTE_DECK_BOX: world={maxX - minX:F1}x{maxY - minY:F1} " +
                $"rotateZ={deckRotationDegrees} " +
                $"localZ={Mathf.DeltaAngle(0f, portraitRoot.localEulerAngles.z):F1} " +
                $"lossyScale={portraitRoot.lossyScale.x:F4}");
            for (int i = 0; i < Display.displays.Length; ++i)
            {
                Display display = Display.displays[i];
                Debug.Log($"ROULETTE_DECK_DISPLAY[{i}]: " +
                    $"rendering={display.renderingWidth}x{display.renderingHeight} " +
                    $"system={display.systemWidth}x{display.systemHeight} " +
                    $"active={display.active}");
            }
        }

        private RectTransform uiColoredHistoryRoot;
        private RawImage uiNeighbor;
        private int displayedHistoryCount = -1;

        private void BuildTotalWinUi()
        {
            uiTotalWinTitle = UiImage(portraitRoot, new Rect(378, 618, 185, 38),
                Resources.Load<Texture2D>("Art/total_win_title"), "Original TOTAL WIN");
            uiTotalWinAmount = new PictureLabel(portraitRoot,
                new Rect(0, 656, 941, 139), OriginalBitmapFont.Load("win_total"),
                "Original total-win amount", nativeSize: true, verticalAlign: -1);
            uiTotalWinTitle.gameObject.SetActive(false);
        }

        private void BuildFooterUi()
        {
            UiImage(portraitRoot, new Rect(0, 1740, 1080, 180), footerPanel, "ProView panel");
            uiInfoButton = UiButton(portraitRoot, new Rect(134, 1816, 66, 62),
                ToggleGameInfoOptions, footerInfoButton);
            BindProViewButtonFrames(uiInfoButton, "GameInfo");
            uiInfoButton.gameObject.name = "ProView INFO";
            uiDisclaimer = ProViewLabel(portraitRoot, new Rect(293, 1884, 420, 11),
                footerDisclaimerFont, "ProView disclaimer");
            uiDisclaimer.Set("MALFUNCTION VOIDS ALL PAYS AND PLAYS");
            uiGameOver = ProViewLabel(portraitRoot, new Rect(402, 1774, 267, 28),
                footerMessageFont, "ProView flashing phrase");
            uiLinePays = ProViewLabel(portraitRoot,
                new Rect(108, 1774, 200, 28), footerMessageFont,
                "ProView line pays", horizontalAlign: -1);
            uiGamePays = ProViewLabel(portraitRoot,
                new Rect(770, 1774, 200, 28), footerMessageFont,
                "ProView game pays", horizontalAlign: 1);
            uiOneCredit = ProViewLabel(portraitRoot, new Rect(43, 1815, 77, 13),
                footerOneCreditFonts, "One credit title");
            uiOneCredit.Set("1 CREDIT");
            uiCreditMeterButton = UiButton(portraitRoot,
                new Rect(335, 1812, 197, 66), () => { }, footerCreditMeterButton);
            uiCreditMeterButton.gameObject.name = "ProView_CreditMeterButton";
            uiCreditMeterButton.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(uiCreditMeterButton, TogglePcCashMeterMode);
            uiCreditTitle = ProViewLabel(portraitRoot,
                new Rect(337, 1815, 88, 15), footerCreditTitleFonts, "Credit title", -1);
            uiCreditTitle.Set("CREDITS");
            uiCashTitle = UiImage(portraitRoot, new Rect(440, 1815, 92, 19),
                footerCashMeterLabel, "ProView_CashMeterLabel");
            uiCashTitle.raycastTarget = false;
            uiBetTitle = ProViewLabel(portraitRoot, new Rect(549, 1815, 72, 15),
                footerCreditTitleFonts, "Bet title", -1);
            uiBetTitle.Set("BET");
            uiCashBetBackground = UiImage(portraitRoot,
                new Rect(631, 1815, 87, 19), footerCashBetMeterLabel,
                "ProView_CashBetMeterLabel");
            uiCashBetBackground.raycastTarget = false;
            uiCashBet = ProViewLabel(portraitRoot,
                new Rect(631, 1815, 87, 19), footerCashCreditFont,
                "Original cash bet meter");
            uiWinTitle = ProViewLabel(portraitRoot, new Rect(732, 1815, 88, 15),
                footerWinTitleFonts, "Win title", -1);
            uiWinTitle.Set("WIN");
            uiCashPaidBackground = UiImage(portraitRoot,
                new Rect(830, 1815, 204, 19), footerCashPaidMeterLabel,
                "ProView_CashPaidMeterLabel");
            uiCashPaidBackground.raycastTarget = false;
            uiCashPaid = ProViewLabel(portraitRoot,
                new Rect(830, 1815, 204, 19), footerCashPaidFont,
                "Original cash paid meter");
            uiDenom = ProViewLabel(portraitRoot, new Rect(35, 1828, 90, 59),
                footerDenomFonts, "Denomination");
            uiCredit = ProViewLabel(portraitRoot,
                new Rect(350, 1832, 188, 45), footerCreditMeterFonts, "Credit digits");
            uiCashCredit = ProViewLabel(portraitRoot,
                new Rect(440, 1815, 92, 19), footerCashCreditFont,
                "Original cash meter");
            uiBet = ProViewLabel(portraitRoot, new Rect(569, 1832, 149, 45),
                footerCreditMeterFonts, "Bet digits");
            UiImage(portraitRoot, new Rect(13, 1887, 125, 12),
                footerDenomTouchBar, "Original denomination touch bar");
            uiDenomTouch = UiButton(portraitRoot, new Rect(25, 1816, 107, 82),
                () => { }, footerDenomTouchButton);
            uiDenomTouch.gameObject.name = "ProView_DenomDecalTouchToChangeButton";
            uiDenomTouch.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(uiDenomTouch, RequestPcDenominationChange);
            uiTouch = ProViewLabel(portraitRoot, new Rect(34, 1888, 83, 11),
                footerTouchFont, "ProView denomination touch phrase");
            uiTouch.Set("TOUCH TO CHANGE");
            // The paid label uses its own original IMG font and independent
            // alpha timer. The Outro movie draws above the fading digits.
            uiPaid = ProViewLabel(portraitRoot, new Rect(782, 1833, 252, 54),
                footerPaidMeterFonts, "Paid credits");
            uiFooterMovie = UiImage(portraitRoot, new Rect(775, 1823, 258, 59), null, "Original Bink frame");
            uiSpinButton = UiButton(portraitRoot,
                new Rect(775, 1823, 258, 59), () => { });
            uiSpinButton.gameObject.AddComponent<RouletteCapturedMainTouchAction>()
                .Configure(uiSpinButton, BeginSpin);
            uiSpinButton.gameObject.name = "Spin footer";
            // The packaged menu enables player volume. Its ProView panel
            // expands above INFO with these four native IMG resources.
            uiGameInfoOptionsRoot = UiRect(portraitRoot,
                new Rect(134, 1641, 101, 237), "ProView game information options");
            UiImage(uiGameInfoOptionsRoot, new Rect(2, 0, 99, 16),
                footerInfoTop, "Game info top");
            uiGameInfoVolume = UiButton(uiGameInfoOptionsRoot,
                new Rect(2, 16, 99, 65), () => { }, footerVolumeButton);
            uiGameInfoVolume.gameObject.AddComponent<RouletteCapturedMainTouchAction>()
                .Configure(uiGameInfoVolume, TogglePcPlayerVolume);
            uiGameInfoVolume.gameObject.name = "Player volume";
            proViewVolumeFrames = Resources.LoadAll<Texture2D>(
                "Original/ProViewStates/Volume").OrderBy(frame => frame.name).ToArray();
            if (proViewVolumeFrames.Length != 3)
                throw new InvalidOperationException("Original ProView volume frames missing");
            ApplyPcPlayerVolume();
            uiGameInfoSeePays = UiButton(uiGameInfoOptionsRoot,
                new Rect(2, 81, 99, 66), () => { }, footerSeePaysButton);
            uiGameInfoSeePays.gameObject.AddComponent<RouletteCapturedMainTouchAction>()
                .Configure(uiGameInfoSeePays, OpenPaysFromGameInfo);
            uiGameInfoSeePays.gameObject.name = "SEE PAYS";
            BindProViewButtonFrames(uiGameInfoSeePays, "SeePays");
            UiImage(uiGameInfoOptionsRoot, new Rect(2, 147, 99, 17),
                footerInfoBottom, "Game info bottom");
            uiGameInfoOptionsRoot.gameObject.SetActive(false);
        }

        private void BuildPaysUi()
        {
            RectTransform root = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920),
                "Original pays pages");
            uiPaysRoot = root.gameObject;
            uiPaysImage = UiImage(root, new Rect(0, 748, 1080, 1054),
                paysPages[0], "Original pays page");
            uiPaysLines = UiImage(root, new Rect(30, 777, 997, 1004),
                paysLinePages[profileIndex], "Original pay lines");
            // Keep navigation on the canvas root. The full-screen pays image is
            // a visual layer; its nested controls did not receive player raycasts.
            uiPaysBack = UiButton(portraitRoot, new Rect(15, 1807, 99, 91),
                () => { }, paysBack);
            uiPaysBack.gameObject.AddComponent<RoulettePaysNavigationAction>()
                .Configure(uiPaysBack, () => {
                    if (PcGameOrdinaryInputBlocked || PcPaysReturnPending) return;
                    paysPage = Mathf.Max(0, paysPage - 1);
                    ResetPaysTimeout();
                    PlayPaysClick();
                });
            uiPaysBack.gameObject.name = "Pays BACK";
            BindProViewButtonFrames(uiPaysBack, "Previous");
            uiPaysExit = UiButton(portraitRoot, new Rect(114, 1807, 81, 90),
                () => { }, paysExit);
            uiPaysExit.gameObject.AddComponent<RoulettePaysNavigationAction>()
                .Configure(uiPaysExit, () => {
                    if (!PcGameOrdinaryInputBlocked) ReturnFromPays();
                });
            uiPaysExit.gameObject.name = "Pays EXIT";
            BindProViewButtonFrames(uiPaysExit, "Exit");
            uiPaysNext = UiButton(portraitRoot, new Rect(195, 1807, 92, 91),
                () => { }, paysNext);
            uiPaysNext.gameObject.AddComponent<RoulettePaysNavigationAction>()
                .Configure(uiPaysNext, () => {
                    if (PcGameOrdinaryInputBlocked || PcPaysReturnPending) return;
                    paysPage = Mathf.Min(3, paysPage + 1);
                    ResetPaysTimeout();
                    PlayPaysClick();
                });
            uiPaysNext.gameObject.name = "Pays NEXT";
            BindProViewButtonFrames(uiPaysNext, "Next");
            uiPaysRoot.SetActive(false);
            uiPaysBack.gameObject.SetActive(false);
            uiPaysExit.gameObject.SetActive(false);
            uiPaysNext.gameObject.SetActive(false);
        }

        private void BuildDoubleUi()
        {
            bool suit = session.DoubleSuitMode;
            doubleRoot = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920), "Double Up");
            doubleEntranceLayer = suit ? null : UiRect(doubleRoot,
                new Rect(0, 0, 1080, 1920), "Original 500 ms background entrance");
            RectTransform backgroundParent = suit ? doubleRoot : doubleEntranceLayer;
            OriginalBitmapFont meterFont = OriginalBitmapFont.Load(
                suit ? "suit_meter" : "double_meter");
            OriginalBitmapFont suitCashFont = suit ?
                OriginalBitmapFont.Load("suit_cash_meter") : null;
            // The source suit background has a transparent 96x24 window at
            // (47,1861). Keep the base game's denomination meter from showing
            // through that window while the feature owns Screen 1.
            if (suit)
            {
                RawImage backing = UiImage(doubleRoot, new Rect(0, 0, 1080, 1920),
                    Texture2D.whiteTexture, "Feature screen backing");
                backing.color = Color.black;
                backing.raycastTarget = false;
            }
            uiDoubleBackground = UiImage(backgroundParent, new Rect(0, 0, 1080, 1920),
                suit ? suitBackground : doubleBackground, "Double background");
            uiDoubleBackground.raycastTarget = true;
            Rect redRect = suit ? new Rect(71, 910, 282, 162) :
                new Rect(60, 892, 310, 193);
            Rect blackRect = suit ? new Rect(727, 910, 282, 162) :
                new Rect(707, 892, 310, 193);
            UiImage(backgroundParent, redRect,
                suit ? suitButtonsDisabled[0] : doubleRedButtonDisabled,
                "Original disabled red button");
            UiImage(backgroundParent, blackRect,
                suit ? suitButtonsDisabled[1] : doubleBlackButtonDisabled,
                "Original disabled black button");
            uiDoubleRedButtonOverlay = UiImage(doubleRoot,
                redRect, suit ? suitButtons[0] : doubleRedButtonFlash,
                "Original red button state");
            uiDoubleBlackButtonOverlay = UiImage(doubleRoot,
                blackRect, suit ? suitButtons[1] : doubleBlackButton,
                "Original black button state");
            if (suit)
            {
                Rect[] rects = { new Rect(71, 1072, 135, 120),
                    new Rect(217, 1072, 136, 120),
                    new Rect(727, 1072, 136, 120),
                    new Rect(874, 1072, 135, 120) };
                uiSuitChoiceButtons = new Button[4];
                uiSuitChoiceOverlays = new RawImage[4];
                for (int i = 0; i < 4; ++i)
                {
                    int choice = i;
                    UiImage(doubleRoot, rects[i], suitButtonsDisabled[i + 2],
                        "Original disabled suit button");
                    uiSuitChoiceOverlays[i] = UiImage(doubleRoot, rects[i],
                        suitButtons[i + 2], "Original suit button state");
                    uiSuitChoiceButtons[i] = UiPointerDownButton(doubleRoot, rects[i],
                        () => { if (!IsDoubleReveal && !doubleCollectPending) Guess(choice); },
                        suitButtons[i + 2]);
                }
            }
            uiDoubleTakeWinOverlay = UiImage(doubleRoot,
                new Rect(112, 1331, 142, 60), doubleTakeWinButton,
                "Original Take Win button");
            uiDoubleMessage = UiImage(doubleRoot, new Rect(247, 696, 589, 63),
                doublePrompt, "Double prompt");
            if (suit)
                UiImage(doubleRoot, new Rect(392, 804, 296, 391),
                    doubleCardContainer, "Card frame");
            uiDoubleCard = UiImage(backgroundParent, suit ? new Rect(428, 840, 224, 308) :
                new Rect(401, 807, 271, 370),
                suit ? suitCardback : doubleCardback, "Double card");
            uiDoubleCurrentWin = FeatureMeterLabel(doubleRoot,
                suit ? new Rect(185, 855, 154, 28) :
                    new Rect(37, 720, 194, 50), meterFont, "Original feature gamble amount");
            uiDoubleToWin = FeatureMeterLabel(doubleRoot,
                suit ? new Rect(840, 855, 155, 28) :
                    new Rect(852, 720, 193, 50), meterFont, "Original feature gamble win");
            uiDoubleCredit = FeatureMeterLabel(doubleRoot,
                suit ? new Rect(378, 1582, 154, 28) :
                    new Rect(69, 1220, 233, 42), meterFont, "Original feature credit");
            uiDoubleBet = FeatureMeterLabel(doubleRoot,
                suit ? new Rect(555, 1582, 147, 28) :
                    new Rect(352, 1220, 164, 42), meterFont, "Original feature bet");
            if (suit)
            {
                uiSuitCreditCash = FeatureMeterLabel(doubleRoot,
                    new Rect(378, 1555, 154, 28), suitCashFont,
                    "Original suit cash credit");
                uiSuitBetCash = FeatureMeterLabel(doubleRoot,
                    new Rect(555, 1555, 147, 28), suitCashFont,
                    "Original suit cash bet");
                uiSuitCurrentWinCash = FeatureMeterLabel(doubleRoot,
                    new Rect(185, 825, 154, 28), suitCashFont,
                    "Original suit cash gamble amount");
                uiSuitToWinCash = FeatureMeterLabel(doubleRoot,
                    new Rect(840, 825, 155, 28), suitCashFont,
                    "Original suit cash gamble win");
            }
            uiDoubleDenom = FeatureMeterLabel(doubleRoot,
                new Rect(569, 1220, 164, 42), meterFont, "Original feature denomination");
            uiDoubleStake = FeatureMeterLabel(doubleRoot,
                new Rect(782, 1220, 234, 42), meterFont, "Original feature win");
            uiDoubleRedButton = UiPointerDownButton(doubleRoot, redRect,
                () => { if (!IsDoubleReveal && !doubleCollectPending) Guess(-1); });
            uiDoubleBlackButton = UiPointerDownButton(doubleRoot, blackRect,
                () => { if (!IsDoubleReveal && !doubleCollectPending) Guess(-2); });
            uiDoubleTakeWinButton = UiPointerDownButton(doubleRoot,
                new Rect(112, 1331, 142, 60), () => {
                if (!IsDoubleReveal && !doubleCollectPending) Try(BeginCollectDouble);
            });
            uiDoubleHelpButton = UiPointerDownButton(doubleRoot,
                suit ? new Rect(494, 452, 66, 66) :
                    new Rect(828, 1332, 142, 60), OpenDoubleHelp,
                suit ? SuitBitmap("bSuitGambleHelp") : null);
            // The ProView disclaimer remains visible below the feature art
            // in the cabinet recording. The feature background covers the
            // base game's footer, so it needs its own source-font copy.
            uiDoubleDisclaimer = ProViewLabel(doubleRoot,
                new Rect(293, 1884, 420, 11), footerDisclaimerFont,
                "Feature ProView disclaimer");
            uiDoubleDisclaimer.Set("MALFUNCTION VOIDS ALL PAYS AND PLAYS");
            RectTransform helpRoot = UiRect(doubleRoot,
                new Rect(0, 0, 1080, 1920), "Double help");
            // The source disables the game controls while help is displayed.
            uiDoubleHelpBlocker = UiButton(helpRoot,
                new Rect(0, 0, 1080, 1920), () => { });
            uiDoubleHelpBackground = UiImage(helpRoot,
                suit ? new Rect(18, 756, 1044, 725) :
                new Rect(0, 543, 1080, 1377),
                suit ? suitHelpBackground : doubleHelpBackground,
                "Original help background");
            uiDoubleHelpRounds = UiImage(helpRoot,
                suit ? new Rect(191, 1413, 37, 30) : new Rect(593, 1249, 40, 31),
                suit ? suitHelpRounds[16] : doubleHelpRounds[16],
                "Original maximum rounds");
            uiDoubleHelpWinLimit = FeatureMeterLabel(helpRoot,
                suit ? new Rect(577, 1376, 204, 32) :
                    new Rect(732, 1335, 204, 32),
                OriginalBitmapFont.Load("double_win_limit"),
                "Original maximum-win meter");
            uiDoubleHelpExitButton = UiPointerDownButton(helpRoot,
                new Rect(466, 1331, 143, 60),
                () => CloseDoubleHelp(true), doubleExitButton);
            uiDoubleHelp = helpRoot.gameObject;
            doubleRoot.gameObject.SetActive(false);
        }

        private bool TableTouchInputEnabled()
        {
            return !PcGameOrdinaryInputBlocked && !SourceRoundBlocksOrdinaryInput && !PaytableTestActive && !OperatorFallbackActive && session != null && !pcCollectPending && !autoplay &&
                sourceTableInputEnabled && !rackSnapPointerNeedsRelease &&
                !spinning && !ResultDisplayActive && !session.InDoubleUp &&
                !IsDoubleReveal && !IsDoubleExit && !historyView && paysPage < 0 &&
                !demoMenuVisible;
        }

        private void ShowBetMarker(int spot)
        {
            if (!TableTouchInputEnabled() || uiBetMarker == null ||
                spot < 0 || spot >= originalLayout.bet_spots.Length) return;
            hoveredBetSpot = spot;
            RefreshBetMarker();
        }

        private void UpdateBetHover(int spot)
        {
            // HandleTableEvent returns when down/move still resolves to the
            // preceding BetSpot. In particular, do not restart a rejection
            // prompt on every drag sample over one number.
            if (!TableTouchInputEnabled() || spot == hoveredBetSpot) return;
            ShowBetMarker(spot);
            if (spot < 37) SelectNeighbor(spot);
            else ClearNeighborSelection();
        }

        private void MoveBetMarker(PointerEventData pointer)
        {
            if (!TableTouchInputEnabled() || pointer == null ||
                EventSystem.current == null) return;
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            RouletteTableRelease target = hits.Count == 0 ? null :
                hits[0].gameObject.GetComponentInParent<RouletteTableRelease>();
            if (target == null)
            {
                HideBetMarker();
                return;
            }
            UpdateBetHover(target.SpotIndex);
        }

        private void HideBetMarker()
        {
            hoveredBetSpot = -1;
            if (uiBetMarker != null) uiBetMarker.gameObject.SetActive(false);
        }

        private void RefreshBetMarker()
        {
            if (uiBetMarker == null) return;
            if (!portraitCanvas.enabled || !TableTouchInputEnabled() || hoveredBetSpot < 0 ||
                hoveredBetSpot >= originalLayout.bet_spots.Length)
            {
                HideBetMarker();
                return;
            }
            int chipIndex = Array.IndexOf(chips, (int)session.Chip);
            if (chipIndex < 0) { HideBetMarker(); return; }
            long prior;
            session.Bets.TryGetValue(100 + hoveredBetSpot, out prior);
            // The source shows state 4 (X frame) for a total or spot cap.
            bool rejected = session.Chip >
                session.Profile.MaximumTotalBet - session.TotalBet ||
                session.Chip > session.Profile.MaximumSpotBet - prior;
            uiBetMarker.texture = rejected ?
                originalBetMarkerRejectedFrames[chipIndex] :
                originalMainChipButtonFrames[chipIndex][0];
            Rect spot = originalLayout.bet_spots[hoveredBetSpot].ToRect();
            uiBetMarker.rectTransform.anchoredPosition = new Vector2(
                spot.x + (spot.width - 90f) * .5f,
                -spot.y - (spot.height - 90f) * .5f);
            uiBetMarker.gameObject.SetActive(true);
        }

        private void RefreshTableNumberMark()
        {
            if (session == null || currentTableResultMarkerCleared || spinning || session.LastResult == null ||
                session.LastResult.Pocket < 0 || session.LastResult.Pocket > 36)
            {
                uiTableNumberMark.gameObject.SetActive(false);
                tableMarkedPocket = -1;
                tableMarkStartedTimestamp = 0;
                StopPcMovie(ref pcTableMarkerMovie);
                return;
            }
            int pocket = session.LastResult.Pocket;
            Texture2D[] frames = pocket == 0 ? tableWinZeroFrames :
                tableWinNonZeroFrames;
            if (tableMarkedPocket != pocket || tableMarkStartedTimestamp == 0)
            {
                tableMarkedPocket = pocket;
                tableMarkStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                StartPcTableMarkerMovie(frames);
                Rect spot = originalLayout.bet_spots[pocket].ToRect();
                uiTableNumberMark.rectTransform.anchoredPosition = new Vector2(
                    spot.x + (spot.width - frames[0].width) * .5f,
                    -spot.y - (spot.height - frames[0].height) * .5f);
                Debug.Log("ROULETTE_TABLE_MARK_NUMBER: pocket=" + pocket +
                    " size=" + frames[0].width + "x" + frames[0].height);
            }
            int frameIndex = PcTableMarkerFrameAt(frames,
                System.Diagnostics.Stopwatch.GetTimestamp());
            Texture2D frame = frames[frameIndex];
            uiTableNumberMark.texture = frame;
            uiTableNumberMark.rectTransform.sizeDelta =
                new Vector2(frame.width, frame.height);
            uiTableNumberMark.gameObject.SetActive(true);
        }

        private void RefreshPortraitUi()
        {
            if (OperatorFallbackActive) return;
            if (PaytableTestActive) { RefreshPaytableTestUi(); return; }
            if (portraitCanvas == null) return;
            RefreshDiagnosticUi();
            portraitCanvas.enabled = PcGameScreensVisible && !ideckPreview && !topperPreview &&
                (!histogramPreview || historyView);
            if (session != null)
            {
                bool tableInputLocked = !TableTouchInputEnabled();
                for (int i = 0; i < uiBetButtons.Length; ++i)
                    uiBetButtons[i].interactable = !tableInputLocked;
                uiVoisinsButton.interactable = !tableInputLocked;
                uiOrphelinsButton.interactable = !tableInputLocked;
                uiTiersButton.interactable = !tableInputLocked;
                // Sprite::Enable changes input bit 2, not visibility bit 0.
                // Source Undo/Clear remain drawn when their touch is disabled.
                uiUndoButton.gameObject.SetActive(true);
                uiClearButton.gameObject.SetActive(true);
                uiUndoButton.interactable = !tableInputLocked && session.CanUndo;
                uiClearButton.interactable = !tableInputLocked && session.Bets.Count > 0;
                uiUndoButtonFrames.Refresh();
                uiClearButtonFrames.Refresh();
                for (int i = 0; i < uiChipSelections.Length; ++i)
                {
                    uiChipButtonStates[i].GetComponent<Button>().interactable =
                        !tableInputLocked;
                    uiChipButtonStates[i].SetSelected(session.Chip == chips[i]);
                    uiChipButtonStates[i].Refresh();
                }
            }
            RefreshBetMarker();
            if (!portraitCanvas.enabled || session == null) return;
            // Roulette::CheckPlayButtonState only enables PLAY for a wager
            // that can be committed with the current balance and table cap.
            uiSpinButton.interactable = SourcePlayInputAllowed;
            // 旋转模式下按宽对齐由 CreatePortraitUi 一次设定；这里按主屏比例
            // 每帧重算会把它冲掉。非旋转模式保持原有行为。
            if (deckRotationDegrees == 0)
            {
                float wideScale = Screen.width / 1080f, tallScale = Screen.height / 1920f;
                portraitCanvas.GetComponent<CanvasScaler>().matchWidthOrHeight =
                    wideScale < tallScale ? 0f : 1f;
            }
            bool inDouble = DoubleFeaturePresentationVisible;
            doubleRoot.gameObject.SetActive(inDouble);
            bool showingPays = paysPage >= 0 && !inDouble;
            uiGameInfoOptionsRoot.gameObject.SetActive(gameInfoOptionsOpen &&
                !showingPays && !inDouble && !spinning);
            uiPaysRoot.SetActive(showingPays);
            uiPaysBack.gameObject.SetActive(showingPays && !PcPaysReturnPending);
            uiPaysExit.gameObject.SetActive(showingPays && !PcPaysReturnPending);
            uiPaysNext.gameObject.SetActive(showingPays && !PcPaysReturnPending);
            if (paysPage >= 0)
            {
                uiPaysImage.texture = paysPages[paysPage];
                // filedefs.xml gives Pays1/4 height 1054 and Pays2/3
                // height 1053. Draw each extracted IMG bitmap at its native
                // dimensions instead of stretching the middle pages.
                uiPaysImage.rectTransform.sizeDelta = new Vector2(
                    paysPages[paysPage].width, paysPages[paysPage].height);
                uiPaysLines.gameObject.SetActive(paysPage == 3);
                uiPaysLines.texture = paysLinePages[profileIndex];
                uiPaysBack.interactable = paysPage > 0;
                uiPaysNext.interactable = paysPage < 3;
                // Original Button::Enabling updates the movie frame
                // synchronously. Do not rely on component LateUpdate order.
                uiPaysBack.GetComponent<RouletteOriginalButtonFrames>().Refresh();
                uiPaysNext.GetComponent<RouletteOriginalButtonFrames>().Refresh();
                uiPaysExit.GetComponent<RouletteOriginalButtonFrames>().Refresh();
            }
            uiWheel.rectTransform.localEulerAngles =
                new Vector3(0, 0, -wheelMainRenderAngle);
            uiSpindle.rectTransform.localEulerAngles =
                new Vector3(0, 0, -wheelSpindleRenderAngle);
            uiOddsPanel.texture = originalOddsPanels[profileIndex];
            RefreshTableNumberMark();
            UpdateBallUi();
            bool showTotalWin = !inDouble && !showingPays &&
                VisibleWinningPayId() >= 0;
            uiTotalWinTitle.gameObject.SetActive(showTotalWin);
            uiTotalWinAmount.Set(showTotalWin ?
                "(" + session.LastResult.Returned + ")" : "");
            RefreshHistoryUi();
            RefreshChipStacksUi();
            RefreshPromptUi();
            RefreshNeighborUi();
            uiDenom.Set(RouletteDenominationFormatter.ForFilmedCabinet(session.Denomination));
            // Game::UpdateTouchToChangeLabels hides the label on Double Up
            // and SEE PAYS; it is independent of the button's bet-state gate.
            uiTouch.SetVisible(!inDouble && !showingPays &&
                session.Profile.Denominations.Length > 1);
            uiDenomTouch.gameObject.SetActive(CanChangePcDenomination());
            // Both ProView show methods ignore the hide request. Credits
            // stay visible, and cash visibility depends on jurisdiction,
            // independently of the saved display-as-cash mode.
            uiCreditTitle.SetVisible(true);
            bool showCash = pcCashMeterJurisdictionEnabled;
            uiCashTitle.gameObject.SetActive(showCash);
            uiCashCredit.SetVisible(showCash);
            uiCredit.Set(DisplayedCredit.ToString());
            // Original ButtonPanel::SetCreditMeterValue uses displayed credits
            // times denomination. The money remainder is retained for cashout.
            uiCashCredit.Set(showCash ?
                RouletteDenominationFormatter.ForFeatureCurrency(
                    DisplayedCredit * session.Denomination) : "");
            uiBet.Set(session.TotalBet.ToString());
            uiCashBet.Set(RouletteDenominationFormatter.ForFeatureCurrency(
                session.TotalBet * session.Denomination));
            if (pcPaymentMessagesActive && pcPaymentFlashingLabel != null)
            {
                uiGameOver.Set(pcPaymentFlashingLabel.Text);
                uiGameOver.SetVisible(pcPaymentFlashingLabel.Visible);
            }
            else
            {
                uiGameOver.SetVisible(true);
                uiGameOver.Set(ProViewFlashingMessage());
            }
            WinningPayRecord visiblePay = VisibleWinningPayRecord();
            uiLinePays.Set(visiblePay == null ? "" :
                RoulettePayMessageFormatter.LinePays(
                    session.Profile.Records[visiblePay.BetId], visiblePay.Amount));
            uiGamePays.Set(currentGameResultsCleared || spinning || session.LastResult == null ? "" :
                session.ResultPhase && session.LastResult.Returned > 0 ?
                    (visiblePay == null ? "" : "Game Pays " + session.LastResult.Returned) :
                    "Game Pays " + DisplayedPaid);
            bool cycleMessagesVisible = pcResultCycleClock == null || !pcResultCycleClock.Paused;
            uiLinePays.SetVisible(cycleMessagesVisible);
            uiGamePays.SetVisible(cycleMessagesVisible);
            // ProView SetPaidMeterValue leaves non-AU zero text blank;
            // the idle gameplay Intro also hides the paid meters. AU uses
            // ShowOnlyGameWinsInWinMeter and keeps a visible numeric zero.
            bool showPaid = SourceAuIdlePaidVisible || (!spinning &&
                (session.ResultPhase || currentGameResultsCleared && auNationalStandardsEnabled) &&
                session.LastResult != null && session.LastResult.Returned > 0 &&
                (!currentGameResultsCleared || auNationalStandardsEnabled));
            bool showCollect = PcCollectMeterVisible;
            uiWinTitle.Set(showCollect ? PcPaymentPhrase(new OriginalPaymentMessageSlot(199, null, null)) : "WIN");
            uiPaid.Set(showCollect ? OriginalCollectMeterFormatting.PanelCredits(pcCollectMeterCredits) : showPaid ? DisplayedPaid.ToString() : "");
            uiCashPaidBackground.gameObject.SetActive(showPaid || showCollect);
            uiCashPaid.SetVisible(showPaid || showCollect);
            uiCashPaid.Set(showCollect ? OriginalCollectMeterFormatting.PanelCash(pcCollectMeterCredits, session.Denomination,
                OriginalMonetaryLocales.Load(OriginalMonetaryLocales.PcVirtualLocaleName)) : showPaid ?
                RouletteDenominationFormatter.ForFeatureCurrency(
                    DisplayedPaid * session.Denomination) : "");
            ApplyPaidOpacity(PaidOpacity);
            RefreshFooterMovieUiAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
            uiFeedback.text = Time.realtimeSinceStartup < feedbackUntil ? status : "";
            if (inDouble) RefreshDoubleUi();
            RefreshHistoryPageUi();
            RefreshDoubleHistoryUi();
        }

        private void UpdateBallUi()
        {
            bool visible = (spinning && wheelBallVisible) ||
                (!spinning && session.LastResult != null);
            uiBall.gameObject.SetActive(visible);
            if (!visible) return;
            int pocket = spinning ? session.PendingPocket : session.LastResult.Pocket;
            Vector2 position = OriginalBallPosition(pocket);
            uiBall.rectTransform.anchoredPosition =
                new Vector2(position.x - 12f, -(position.y - 12f));
        }

        private void RefreshNeighborUi()
        {
            bool active = selectedNeighbor >= 0 && TableTouchInputEnabled() &&
                (neighborEnabledWithoutPrecheck || session.CanPlaceNeighbor(selectedNeighbor));
            uiNeighbor.texture = active ? neighborOn : neighborOff;
            uiNeighborButton.interactable = active;
            uiNeighborMeterBackground.gameObject.SetActive(active);
            uiNeighborSelection.gameObject.SetActive(active);
            uiNeighborTouch.gameObject.SetActive(active &&
                Time.realtimeSinceStartup < neighborTouchUntil);
            uiNeighborNumber.Set(active ? selectedNeighbor.ToString() : "");
            if (!active) return;
            uiNeighborMeterBackground.texture = selectedNeighbor == 0 ? neighborMeterGreen :
                RedNumbers.Contains(selectedNeighbor) ? neighborMeterRed : neighborMeterBlack;
            uiNeighborSelection.texture = neighborSelected[selectedNeighbor];
            Rect bounds = neighborSelectionLayout.selections[selectedNeighbor].ToRect();
            uiNeighborSelection.rectTransform.anchoredPosition =
                new Vector2(bounds.x, -bounds.y);
            uiNeighborSelection.rectTransform.sizeDelta = bounds.size;
        }

        private void RefreshHistoryUi()
        {
            int first = 0;
            bool hasLast = recordedPockets.Count > first;
            int last = hasLast ? recordedPockets[first] : -1;
            uiResultOval.gameObject.SetActive(hasLast);
            uiResultNumber.Set(hasLast ? last.ToString() : "");
            uiToteNumber.Set(hasLast ? last.ToString() : "");
            uiToteMeterBox.gameObject.SetActive(hasLast);
            if (hasLast) uiToteMeterBox.texture = toteMeterBoxFrames[ToteMeterBoxFrame(last)];
            if (hasLast)
                uiResultOval.texture = last == 0 ? resultSpotGreen :
                    RedNumbers.Contains(last) ? resultSpotRed : resultSpotBlack;
            int signature = recordedHistoryVersion * 2 + first;
            if (displayedHistoryCount == signature) return;
            displayedHistoryCount = signature;
            for (int i = uiColoredHistoryRoot.childCount - 1; i >= 0; --i)
            {
                GameObject old = uiColoredHistoryRoot.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }
            for (int i = 0; i < uiHistory.Length; ++i)
            {
                int historyIndex = first + 1 + i;
                if (historyIndex >= recordedPockets.Count) break;
                int pocket = recordedPockets[historyIndex];
                OriginalBitmapFont font = pocket == 0 ? toteGreenFont :
                    RedNumbers.Contains(pocket) ? toteRedFont : toteYellowFont;
                var label = new PictureLabel(uiColoredHistoryRoot,
                    new Rect(915, 121 + i * 32, 112, 29), font,
                    "Previous tote number", nativeSize: true,
                    horizontalAlign: pocket == 0 ? 0 :
                        RedNumbers.Contains(pocket) ? 1 : -1);
                label.Set(pocket.ToString());
            }
        }

        private void RefreshChipStacksUi()
        {
            long signature = 17;
            foreach (var pair in session.Bets.OrderBy(x => x.Key))
            {
                signature = unchecked(signature * 31 + pair.Key);
                signature = unchecked(signature * 31 + pair.Value);
            }
            int visibleWinningPayId = VisibleWinningPayId();
            bool showWin = visibleWinningPayId >= 0;
            signature = unchecked(signature * 31 + visibleWinningPayId);
            if (signature == displayedStackHash) return;
            displayedStackHash = signature;
            for (int i = betChipLayer.childCount - 1; i >= 0; --i)
            {
                GameObject old = betChipLayer.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }
            foreach (var pair in session.Bets
                .Where(bet => bet.Key >= 100 &&
                    bet.Key < 100 + originalLayout.bet_spots.Length)
                .OrderBy(bet => originalLayout.bet_spots[bet.Key - 100].z)
                .ThenBy(bet => bet.Key))
            {
                int index = pair.Key - 100;
                if (index < 0 || index >= originalLayout.bet_spots.Length) continue;
                Rect spot = originalLayout.bet_spots[index].ToRect();
                float cx = spot.x + spot.width * .5f, cy = spot.y + spot.height * .5f;
                int chipIndex = ClosestBetSelector(pair.Value);
                UiImage(betChipLayer, new Rect(cx - 36, cy - 36, 72, 72),
                    originalPlacedChips[chipIndex], "Placed chip " + pair.Key);
                if (pair.Value > 0)
                {
                    var amount = new PictureLabel(betChipLayer,
                        new Rect(cx - 15, cy - 12, 30, 24), chipStackFont,
                        "Stack amount", nativeSize: true);
                    amount.Set(pair.Value.ToString());
                }
            }
            // The IMG gives every winning highlight z=35, above all bet
            // sprites (z=10..14), independent of the wager insertion order.
            if (showWin)
            {
                foreach (var pair in session.Bets.OrderBy(bet => bet.Key))
                {
                    int index = pair.Key - 100;
                    if (index < 0 || index >= originalLayout.bet_spots.Length ||
                        pair.Key != visibleWinningPayId)
                        continue;
                    Rect spot = originalLayout.bet_spots[index].ToRect();
                    float cx = spot.x + spot.width * .5f, cy = spot.y + spot.height * .5f;
                    UiImage(betChipLayer, new Rect(cx - 59.5f, cy - 54, 119, 108),
                        betWinHighlight, "Winning spot");
                    WinningPayRecord pay = VisibleWinningPayRecord();
                    if (pay != null)
                        new PictureLabel(betChipLayer,
                            new Rect(cx - 59.5f, cy - 54, 119, 108),
                            cyclePayFont, "Winning spot payout", nativeSize: true)
                            .Set(SourceBetSpotPayout(pay.Amount));
                }
            }
        }

        private static string SourceBetSpotPayout(long amount)
        {
            // BetSpot::SetPayout stores an int, then ShowWinningAnim uses %d.
            return unchecked((int)amount).ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        private int ClosestBetSelector(long amount)
        {
            for (int candidate = chips.Length - 1; candidate > 0; --candidate)
                if (amount >= chips[candidate]) return candidate;
            return 0;
        }

        private Texture2D[] PromptFrames(PromptKind kind, bool outro)
        {
            if (kind == PromptKind.PlaceBets) return outro ? placeBetsOut : placeBetsIn;
            if (kind == PromptKind.PressSpin) return outro ? pressSpinOutFrames : pressSpinFrames;
            if (kind == PromptKind.GameOver) return outro ? gameOverOutFrames : gameOverFrames;
            if (kind == PromptKind.InsertCredits) return outro ? insertCreditsOut : insertCreditsIn;
            if (kind == PromptKind.BetLimitReached) return outro ? betLimitOut : betLimitIn;
            if (kind == PromptKind.BetSpotLimitReached) return outro ? betSpotLimitOut : betSpotLimitIn;
            return null;
        }

        private void QueueBetFailurePrompt(BetRejectionReason reason)
        {
            sourcePromptStopped = false;
            PromptKind requested = reason == BetRejectionReason.SpotLimit ?
                PromptKind.BetSpotLimitReached : reason == BetRejectionReason.TotalLimit ?
                PromptKind.BetLimitReached : PromptKind.InsertCredits;
            if (requested == PromptKind.InsertCredits)
            {
                // Insert Credits keeps the default repeat type 3 in the source.
                forcedInsertCredits = true;
                failurePrompt = PromptKind.None;
                promptStartNow = PromptKind.InsertCredits;
                promptPhase = PromptPlaybackPhase.None;
                promptEligibilityMask = -1;
            }
            else failurePrompt = requested;
            failurePromptStartedTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            Debug.Log("ROULETTE_BET_REJECTED: " + reason + " prompt=" + requested);
        }

        private void ResetBetPromptAfterSuccess()
        {
            sourcePromptStopped = false;
            forcedInsertCredits = false;
            sourceNamedPrompt = PromptKind.None;
            failurePrompt = PromptKind.None;
            promptStartAfterFailure = PromptKind.None;
            promptStartNow = PromptKind.None;
            promptPhase = PromptPlaybackPhase.None;
            promptEligibilityMask = -1;
        }

        private void RestartSourcePrompt(PromptKind kind)
        {
            RestartSourcePromptAt(kind, System.Diagnostics.Stopwatch.GetTimestamp());
        }

        private void RestartSourcePromptAt(PromptKind kind, long nowTimestamp)
        {
            // The roulette table event handler and DoUndo/DoClear call
            // SetMessages(true, name, 0), restarting at the named message.
            ResetBetPromptAfterSuccess();
            promptStartNow = kind;
            sourceNamedPrompt = kind;
            promptReadyTimestamp = nowTimestamp;
        }

        private void ScheduleIdlePromptAt(long idleEnteredTimestamp, long clockFrequency)
        {
            sourcePromptStopped = false;
            // StateIdle uses SetMessages(true, "", 1000). Timer +0x24
            // remains armed across Stop(false)/immediate Play. Its repeated
            // Begin call is suppressed by SpriteCollection's same-movie guard.
            promptReadyTimestamp = OriginalRackClock.AfterMilliseconds(
                idleEnteredTimestamp, 1000, clockFrequency);
            promptDelayedPlayTimestamp = promptReadyTimestamp;
        }

        private void ScheduleResultIdlePrompt(long clockFrequency)
        {
            // Fresh 35 ms waits can return late or through input. The idle
            // prompt is scheduled from their actual completed boundary.
            if (pcResultDisplayWait != null && !pcResultDisplayWait.Completed) return;
            long idleAt = pcResultDisplayWait == null ? resultSettledTimestamp :
                pcResultDisplayWait.CompletedAt;
            ScheduleIdlePromptAt(idleAt, clockFrequency);
        }

        private void RestartSourcePromptAfterCreditChange()
        {
            RestartSourcePromptAfterCreditChangeAt(System.Diagnostics.Stopwatch.GetTimestamp());
        }

        private void RestartSourcePromptAfterCreditChangeAt(long nowTimestamp)
        {
            // Roulette::UpdateDisplay and CashoutCompleted call
            // SetMessages(true, "", 0). The first active list node starts now.
            ResetBetPromptAfterSuccess();
            promptReadyTimestamp = nowTimestamp;
        }

        private IEnumerator PostResultBetSmoke()
        {
            yield return null;
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome(16));
            session.PlaceBet(100, 1);
            session.Spin();
            gameOverMessageLatched = true;
            selectedNeighbor = 0;
            RefreshPortraitUi();
            if (!session.ResultPhase || !uiNeighborButton.interactable ||
                !session.CanPlaceNeighbor(0))
                throw new InvalidOperationException(
                    "Original idle table did not reopen neighbor input after a loss");
            ClickThroughUiRaycast(uiNeighborButton);
            if (session.ResultPhase || session.TotalBet != 5 ||
                session.Bets.Count != 5)
                throw new InvalidOperationException(
                    "Neighbor touch did not begin the next betting round");
            RefreshPortraitUi();
            if ((promptEligibilityMask & (1 << (int)PromptKind.GameOver)) == 0)
                throw new InvalidOperationException(
                    "Post-result wager cleared the source Game Over message early");

            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome(16));
            session.PlaceBet(100, 1);
            session.Spin();
            gameOverMessageLatched = true;
            RefreshPortraitUi();
            if (!uiBetButtons[1].interactable)
                throw new InvalidOperationException(
                    "Original idle table did not reopen direct input after a loss");
            ClickThroughUiRaycast(uiBetButtons[1]);
            if (session.ResultPhase || session.TotalBet != 2 ||
                !session.Bets.ContainsKey(101))
                throw new InvalidOperationException(
                    "Direct bet did not begin the next betting round");
            RefreshPortraitUi();
            if ((promptEligibilityMask & (1 << (int)PromptKind.GameOver)) == 0)
                throw new InvalidOperationException(
                    "Direct wager cleared the source Game Over message early");

            RouletteProfile rejectedProfile = RouletteProfile.Load("97");
            rejectedProfile.MaximumSpotBet = 1;
            session = new RouletteSession(rejectedProfile, 1000,
                new FixedPocketSmokeOutcome(0)) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            gameOverMessageLatched = true;
            currentGameResultsCleared = false;
            currentTableResultMarkerCleared = false;
            sourceBetsChangedSincePlay = false;
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiBetButtons[0]);
            if (!session.ResultPhase || session.DoubleAvailable ||
                session.TotalBet != 1 || session.LastResult.Returned != 36 ||
                !currentGameResultsCleared || sourceBetsChangedSincePlay)
                throw new InvalidOperationException(
                    "Rejected direct table event did not clear only its live result");

            rejectedProfile = RouletteProfile.Load("97");
            rejectedProfile.MaximumTotalBet = 1;
            session = new RouletteSession(rejectedProfile, 1000,
                new FixedPocketSmokeOutcome(0)) { DoubleEnabled = true };
            session.PlaceBet(100, 1);
            session.Spin();
            gameOverMessageLatched = true;
            currentGameResultsCleared = false;
            currentTableResultMarkerCleared = false;
            sourceBetsChangedSincePlay = false;
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiVoisinsButton);
            if (!session.ResultPhase || !session.DoubleAvailable ||
                session.TotalBet != 1 || session.LastResult.Returned != 36 ||
                currentGameResultsCleared || sourceBetsChangedSincePlay)
                throw new InvalidOperationException(
                    "Rejected sector bet discarded the original Double Up offer");
            Debug.Log("ROULETTE_POST_RESULT_BET_PASSED: neighbor=4 direct=1 " +
                      "rejectedDirectClearedView=true rejectedSectorPreservedView=true");
            Application.Quit();
        }

        private IEnumerator BallFallbackSmoke()
        {
            yield return null;
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome(27));
            session.PlaceBet(127, 1);
            session.Spin();
            wheelMotionReady = false;
            wheelRenderedAngle = 0d;
            wheelMainRenderAngle = wheelSpindleRenderAngle = 36000f;
            RefreshPortraitUi();
            Vector2 center = OriginalBallPosition(27);
            if (center != new Vector2(279f, 316f) ||
                !uiBall.gameObject.activeSelf ||
                uiBall.rectTransform.anchoredPosition != new Vector2(267f, -304f))
                throw new InvalidOperationException(
                    "Restored ball did not align with original pocket-27 wheel geometry");
            Debug.Log("ROULETTE_BALL_FALLBACK_PASSED: pocket=27 center=279,316");
            Application.Quit();
        }

        private IEnumerator BetLimitPromptSmoke()
        {
            yield return null;
            if (!gameOverMessageLatched || sourceGameplayState != SourceGameplayState.Idle ||
                session.Rounds != 0)
                throw new InvalidOperationException("Fresh source state did not enable GameOver");
            Debug.Log("ROULETTE_BET_LIMIT_FRESH_SOURCE_PASSED: state=Idle gameOver=true rounds=0");
            // This independent AnimMessageManager fixture exercises two-message and
            // single-message clocks. Native fresh Idle has an additional GameOver
            // member; synthetic model NextRound below does not clear native +0x919d.
            gameOverMessageLatched = false;
            sourceGameplayState = SourceGameplayState.Bet;
            sourceBetStateEntryPending = false;
            sourceTableInputEnabled = true;
            configuredAutoPlayCheckPending = false;
            configuredAutoPlayFinishPending = false;
            foreach (BetRejectionReason reason in new[] {
                BetRejectionReason.TotalLimit, BetRejectionReason.SpotLimit,
                BetRejectionReason.InsufficientCredit })
            {
                RouletteProfile limitProfile = RouletteProfile.Load("97");
                if (reason == BetRejectionReason.TotalLimit)
                    limitProfile.MaximumTotalBet = 1;
                if (reason == BetRejectionReason.SpotLimit)
                    limitProfile.MaximumSpotBet = 1;
                session = new RouletteSession(limitProfile,
                    reason == BetRejectionReason.InsufficientCredit ? 1 : 10,
                    new FixedPocketSmokeOutcome());
                session.PlaceBet(100, 1);
                Bet(reason == BetRejectionReason.SpotLimit ? 100 : 101);
                RefreshPortraitUi();
                PromptKind expected = reason == BetRejectionReason.TotalLimit ?
                    PromptKind.BetLimitReached : reason == BetRejectionReason.SpotLimit ?
                    PromptKind.BetSpotLimitReached : PromptKind.InsertCredits;
                Texture2D[] frames = PromptFrames(expected, false);
                bool correctPrompt = expected == PromptKind.InsertCredits ?
                    shownPrompt == expected : failurePrompt == expected;
                if (session.TotalBet != 1 || !correctPrompt ||
                    !promptRoot.gameObject.activeSelf || frames.Length == 0 ||
                    uiPrompt.texture != frames[0] || uiPromptButton.interactable)
                    throw new InvalidOperationException("Original bet-limit message failed: " + reason);
                Debug.Log("ROULETTE_BET_LIMIT_UI_PASSED: " + reason +
                    " frame=" + uiPrompt.texture.name);
                yield return null;
            }
            RouletteProfile overlap = RouletteProfile.Load("97");
            overlap.MaximumSpotBet = 1;
            session = new RouletteSession(overlap, 1, new FixedPocketSmokeOutcome());
            session.PlaceBet(100, 1);
            Bet(100);
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.InsertCredits || session.TotalBet != 1 ||
                uiPrompt.texture != insertCreditsIn[0] || uiPromptButton.interactable)
                throw new InvalidOperationException("Credit/spot overlap displayed the wrong prompt");
            Debug.Log("ROULETTE_BET_LIMIT_PRIORITY_PASSED: credit before spot");
            yield return new WaitForSecondsRealtime(.85f);
            RefreshPortraitUi();
            if (!promptRoot.gameObject.activeSelf ||
                uiPrompt.texture != insertCreditsIn[insertCreditsIn.Length - 1])
                throw new InvalidOperationException("Original message did not hold its final Begin frame");
            yield return new WaitForSecondsRealtime(3f);
            RefreshPortraitUi();
            if (!promptRoot.gameObject.activeSelf ||
                !insertCreditsOut.Contains(uiPrompt.texture))
                throw new InvalidOperationException("Original message End animation was not triggered");
            yield return new WaitForSecondsRealtime(.65f);
            RefreshPortraitUi();
            if (promptRoot.gameObject.activeSelf)
                throw new InvalidOperationException("Original message did not enter its blank interval");
            yield return new WaitForSecondsRealtime(3f);
            RefreshPortraitUi();
            if (!promptRoot.gameObject.activeSelf ||
                shownPrompt != PromptKind.PressSpin || !uiPromptButton.interactable)
                throw new InvalidOperationException("Original message did not return to Press Spin");
            Debug.Log("ROULETTE_BET_MESSAGE_TIMING_PASSED: Begin, hold, End, blank, next");
            yield return new WaitForSecondsRealtime(7.5f);
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.InsertCredits ||
                !promptRoot.gameObject.activeSelf || uiPromptButton.interactable)
                throw new InvalidOperationException("Repeatable Insert Credits did not return");
            Debug.Log("ROULETTE_INSERT_CREDITS_REPEAT_PASSED: Insert Credits, Press Spin");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 10,
                new FixedPocketSmokeOutcome());
            session.PlaceBet(100, 1);
            promptEligibilityMask = -1;
            promptPhase = PromptPlaybackPhase.None;
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.PressSpin || !uiPromptButton.interactable)
                throw new InvalidOperationException("Multi-message cycle did not start at Press Spin");
            yield return new WaitForSecondsRealtime(.85f);
            RefreshPortraitUi();
            if (promptPhase != PromptPlaybackPhase.Hold ||
                uiPrompt.texture != pressSpinFrames[pressSpinFrames.Length - 1])
                throw new InvalidOperationException("Press Spin did not hold its Begin frame");
            yield return new WaitForSecondsRealtime(3f);
            RefreshPortraitUi();
            if (promptPhase != PromptPlaybackPhase.End ||
                !pressSpinOutFrames.Contains(uiPrompt.texture) ||
                uiPromptButton.interactable)
                throw new InvalidOperationException("Press Spin End phase mismatch");
            yield return new WaitForSecondsRealtime(.65f);
            RefreshPortraitUi();
            if (promptPhase != PromptPlaybackPhase.Blank ||
                promptRoot.gameObject.activeSelf)
                throw new InvalidOperationException("Press Spin blank phase mismatch");
            yield return new WaitForSecondsRealtime(3f);
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.PlaceBets ||
                !promptRoot.gameObject.activeSelf || uiPromptButton.interactable)
                throw new InvalidOperationException("Source-order Place Bets transition failed");
            Debug.Log("ROULETTE_NORMAL_MESSAGE_CYCLE_PASSED: Press Spin to Place Bets");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome());
            Bet(100);
            Bet(101);
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.PressSpin || session.TotalBet != 2)
                throw new InvalidOperationException("Successful bet did not restart Press Spin");
            uiUndoButton.onClick.Invoke();
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.PlaceBets || session.TotalBet != 1)
                throw new InvalidOperationException("Undo did not restart Place Bets");
            uiClearButton.onClick.Invoke();
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.PlaceBets || session.TotalBet != 0)
                throw new InvalidOperationException("Clear did not restart Place Bets");
            Debug.Log("ROULETTE_SOURCE_PROMPT_RESTART_PASSED: bet, undo, clear");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 2,
                new FixedPocketSmokeOutcome());
            session.PlaceBet(100, 1);
            RefreshPortraitUi();
            if ((promptEligibilityMask & (1 << (int)PromptKind.PlaceBets)) == 0 ||
                (promptEligibilityMask & (1 << (int)PromptKind.PressSpin)) == 0 ||
                (promptEligibilityMask & (1 << (int)PromptKind.InsertCredits)) != 0)
                throw new InvalidOperationException("Source SetMessages affordability gate mismatch");
            session.SetChip(2);
            RefreshPortraitUi();
            if ((promptEligibilityMask & (1 << (int)PromptKind.PlaceBets)) != 0 ||
                (promptEligibilityMask & (1 << (int)PromptKind.PressSpin)) == 0)
                throw new InvalidOperationException("Place Bet ignored credit-bound table limit");
            Bet(101);
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.InsertCredits)
                throw new InvalidOperationException("Rejected next chip did not request Insert Credits");
            Debug.Log("ROULETTE_SOURCE_MESSAGE_GATE_PASSED: current bet versus next chip");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 1,
                new FixedPocketSmokeOutcome());
            session.PlaceBet(100, 1);
            session.SetChip(2);
            RefreshPortraitUi();
            int twoMessages = (1 << (int)PromptKind.InsertCredits) |
                (1 << (int)PromptKind.PressSpin);
            if ((promptEligibilityMask & twoMessages) != twoMessages ||
                (promptEligibilityMask & (1 << (int)PromptKind.PlaceBets)) != 0 ||
                shownPrompt != PromptKind.InsertCredits)
                throw new InvalidOperationException("Original linked-list first message mismatch");
            Debug.Log("ROULETTE_SOURCE_FIRST_MESSAGE_PASSED: Insert Credits");
            ResetBetPromptAfterSuccess();
            RouletteProfile capProfile = RouletteProfile.Load("97");
            capProfile.MaximumTotalBet = 2;
            session = new RouletteSession(capProfile, 10,
                new FixedPocketSmokeOutcome());
            session.PlaceBet(100, 1);
            session.SetChip(2);
            RefreshPortraitUi();
            if ((promptEligibilityMask & (1 << (int)PromptKind.PlaceBets)) != 0 ||
                (promptEligibilityMask & (1 << (int)PromptKind.PressSpin)) == 0)
                throw new InvalidOperationException("Source selected-chip table limit gate mismatch");
            Debug.Log("ROULETTE_SOURCE_PLACE_BET_GATE_PASSED: credit and operator cap combined");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 2,
                new FixedPocketSmokeOutcome());
            session.PlaceBet(100, 1);
            session.PlaceBet(101, 1);
            session.SetChip(2);
            if (!TrySourceTableEvent(UndoBet, betUndoSound, undoEvent: true))
                throw new InvalidOperationException("Undo setup failed");
            RestartSourcePrompt(PromptKind.PlaceBets);
            RefreshPortraitUi();
            if (session.TotalBet != 1 || shownPrompt != PromptKind.PlaceBets ||
                (promptEligibilityMask & (1 << (int)PromptKind.PlaceBets)) == 0)
                throw new InvalidOperationException("Source named Place Bet was not forced after Undo");
            Debug.Log("ROULETTE_SOURCE_NAMED_MESSAGE_PASSED: Undo forces Place Bet");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 0,
                new FixedPocketSmokeOutcome());
            Bet(100);
            RefreshPortraitUi();
            yield return new WaitForSecondsRealtime(3.9f);
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.InsertCredits ||
                promptPhase != PromptPlaybackPhase.Hold ||
                uiPrompt.texture != insertCreditsIn[insertCreditsIn.Length - 1] ||
                !promptRoot.gameObject.activeSelf)
                throw new InvalidOperationException("Single active Insert Credits did not hold");
            Debug.Log("ROULETTE_SINGLE_MESSAGE_HOLD_PASSED: Insert Credits");
            ResetBetPromptAfterSuccess();
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome(0));
            bool priorAuStandards = auNationalStandardsEnabled;
            auNationalStandardsEnabled = false;
            session.PlaceBet(100, 1);
            session.Spin();
            spinning = true;
            autoplay = false;
            RefreshPortraitUi();
            if (uiGameOver.Value != "Good Luck!")
                throw new InvalidOperationException("Manual spin omitted the ProView flashing phrase");
            string[] spinPhraseArgs = Environment.GetCommandLineArgs();
            int spinPhraseCapture = Array.IndexOf(spinPhraseArgs,
                "-rouletteSpinPhraseCapture");
            if (spinPhraseCapture >= 0 && spinPhraseCapture + 1 < spinPhraseArgs.Length)
            {
                yield return null;
                CaptureActiveScreenOffscreen(spinPhraseArgs[spinPhraseCapture + 1]);
            }
            autoplay = true;
            RefreshPortraitUi();
            if (uiGameOver.Value != "Auto Play Mode")
                throw new InvalidOperationException("Autoplay spin omitted the ProView flashing phrase");
            auNationalStandardsEnabled = true;
            autoplay = false;
            RefreshPortraitUi();
            if (uiGameOver.Value != "")
                throw new InvalidOperationException(
                    "Australian manual spin retained the source-disabled Good Luck phrase");
            autoplay = true;
            RefreshPortraitUi();
            if (uiGameOver.Value != "Auto Play Mode")
                throw new InvalidOperationException(
                    "Australian autoplay spin omitted its separate ProView phrase");
            auNationalStandardsEnabled = priorAuStandards;
            autoplay = false;
            spinning = false;
            RefreshPortraitUi();
            if (uiGameOver.Value != "")
                throw new InvalidOperationException("SpinDone retained the ProView flashing phrase");
            foreach (int pocket in new[] { 0, 16 })
            {
                session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                    new FixedPocketSmokeOutcome(pocket));
                session.PlaceBet(100, 1);
                session.Spin();
                resultSettledAt = Time.realtimeSinceStartup;
                resultSettledTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
                proViewFlashStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                resultDisplayUntil = resultSettledAt +
                    (pocket == 0 ? 1.05f : .7f);
                ScheduleIdlePromptAt(resultSettledTimestamp,
                    System.Diagnostics.Stopwatch.Frequency);
                winRackActive = pocket == 0;
                RefreshPortraitUi();
                if ((promptEligibilityMask & (1 << (int)PromptKind.GameOver)) != 0)
                    throw new InvalidOperationException("Game Over appeared during result display: " + pocket);
                if (uiGameOver.Value != (pocket == 0 ? "" : "Game Over"))
                    throw new InvalidOperationException(
                        "ProView result entry did not follow the manual source path: " + pocket);
                winRackActive = false;
                resultSettledAt = Time.realtimeSinceStartup - 1.1f;
                resultSettledTimestamp = System.Diagnostics.Stopwatch.GetTimestamp() -
                    System.Diagnostics.Stopwatch.Frequency * 11L / 10L;
                proViewFlashStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    System.Diagnostics.Stopwatch.Frequency * 11L / 10L;
                resultDisplayUntil = Time.realtimeSinceStartup - .05f;
                ScheduleIdlePromptAt(resultSettledTimestamp,
                    System.Diagnostics.Stopwatch.Frequency);
                RefreshPortraitUi();
                if ((promptEligibilityMask & (1 << (int)PromptKind.GameOver)) == 0)
                    throw new InvalidOperationException("Settled round omitted Game Over: " + pocket);
                if (uiGameOver.Value != "Game Over")
                    throw new InvalidOperationException(
                        "Manual result entry omitted the ProView Game Over: " + pocket);
                if (pocket == 16)
                {
                    int resultPhraseCapture = Array.IndexOf(spinPhraseArgs,
                        "-rouletteResultPhraseCapture");
                    if (resultPhraseCapture >= 0 &&
                        resultPhraseCapture + 1 < spinPhraseArgs.Length)
                    {
                        yield return new WaitForEndOfFrame();
                        CaptureActiveScreenOffscreen(
                            spinPhraseArgs[resultPhraseCapture + 1]);
                    }
                }
                resultSettledAt = Time.realtimeSinceStartup - 2.6f;
                resultSettledTimestamp = System.Diagnostics.Stopwatch.GetTimestamp() -
                    System.Diagnostics.Stopwatch.Frequency * 26L / 10L;
                proViewFlashStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    System.Diagnostics.Stopwatch.Frequency * 26L / 10L;
                RefreshPortraitUi();
                if (uiGameOver.Value != "")
                    throw new InvalidOperationException("ProView Game Over did not enter its 250 ms hidden phase");
                resultSettledAt = Time.realtimeSinceStartup - 2.8f;
                resultSettledTimestamp = System.Diagnostics.Stopwatch.GetTimestamp() -
                    System.Diagnostics.Stopwatch.Frequency * 28L / 10L;
                proViewFlashStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    System.Diagnostics.Stopwatch.Frequency * 28L / 10L;
                RefreshPortraitUi();
                if (uiGameOver.Value != "Game Over")
                    throw new InvalidOperationException("ProView Game Over did not repeat after its hidden phase");
                long flashCycleMs = 180L * 24L * 60L * 60L * 1000L;
                flashCycleMs -= flashCycleMs % 2750L;
                long frequency = System.Diagnostics.Stopwatch.Frequency;
                long hiddenMs = flashCycleMs + 2600L;
                proViewFlashStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    hiddenMs / 1000L * frequency -
                    hiddenMs % 1000L * frequency / 1000L;
                RefreshPortraitUi();
                if (uiGameOver.Value != "")
                    throw new InvalidOperationException(
                        "Long-uptime ProView Game Over missed its hidden phase");
                long visibleMs = flashCycleMs + 2800L;
                proViewFlashStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() -
                    visibleMs / 1000L * frequency -
                    visibleMs % 1000L * frequency / 1000L;
                RefreshPortraitUi();
                if (uiGameOver.Value != "Game Over")
                    throw new InvalidOperationException(
                        "Long-uptime ProView Game Over missed its visible phase");
                session.NextRound();
                RefreshPortraitUi();
                if ((promptEligibilityMask & (1 << (int)PromptKind.GameOver)) != 0)
                    throw new InvalidOperationException("New round retained Game Over: " + pocket);
            }
            Debug.Log("ROULETTE_GAME_OVER_GATE_PASSED: winning and losing results");
            if (string.IsNullOrEmpty(capturePath)) Application.Quit();
        }

        private IEnumerator StartupMessageSmoke()
        {
            yield return null;
            RefreshPortraitUi();
            if (promptRoot.gameObject.activeSelf)
                throw new InvalidOperationException("Initial idle message appeared before source delay");
            yield return new WaitForSecondsRealtime(1.1f);
            RefreshPortraitUi();
            if (!promptRoot.gameObject.activeSelf || shownPrompt != PromptKind.PlaceBets ||
                uiPrompt.texture != placeBetsIn[0] && !placeBetsIn.Contains(uiPrompt.texture))
                throw new InvalidOperationException("Initial Place Bets did not start after source delay");
            Debug.Log("ROULETTE_INITIAL_MESSAGE_DELAY_PASSED: 1000 ms");
        }

        private void RefreshPromptUi()
        {
            RefreshPromptUiAt(System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
        }

        private void RefreshPromptUiAt(long nowTimestamp, long clockFrequency)
        {
            if (promptDelayedPlayTimestamp != 0 && nowTimestamp >= promptDelayedPlayTimestamp)
            {
                // PlayMessage may call PlayAnim while Begin is active, but
                // SpriteCollection::PlayAnim refuses an already selected,
                // playing movie. Preserve the current frame, including a
                // rejection Begin. A first delayed start is handled by the
                // readiness/phase initialization below.
                promptDelayedPlayTimestamp = 0;
            }
            // Stop(false) leaves the independent delayed timer armed, but
            // its PlayMessage callback checks running byte +0x10 and cannot
            // restart a stopped manager. A later SetMessages/Play can.
            if (sourcePromptStopped)
            {
                uiPromptButton.interactable = false;
                promptRoot.gameObject.SetActive(false);
                return;
            }
            if (failurePrompt != PromptKind.None)
            {
                uiPromptButton.interactable = false;
                Texture2D[] failureIntro = PromptFrames(failurePrompt, false);
                Texture2D[] failureOutro = PromptFrames(failurePrompt, true);
                int age = OriginalRackClock.ElapsedMilliseconds(
                    failurePromptStartedTimestamp, nowTimestamp, clockFrequency);
                if (failureIntro != null && failureIntro.Length > 0 &&
                    failureOutro != null && failureOutro.Length > 0)
                {
                    const int failureFrameMilliseconds = 42;
                    // Source HandleMovieEvent starts a 3000 ms hold after Begin,
                    // then schedules PlayNext 3000 ms after End completes.
                    int outroAt = failureIntro.Length * failureFrameMilliseconds + 3000;
                    int blankAt = outroAt + failureOutro.Length * failureFrameMilliseconds;
                    int nextAt = blankAt + 3000;
                    if (age < nextAt)
                    {
                        if (age >= blankAt)
                        {
                            promptRoot.gameObject.SetActive(false);
                            return;
                        }
                        bool leaving = age >= outroAt;
                        Texture2D[] failureFrames = leaving ? failureOutro : failureIntro;
                        int frameAge = leaving ? age - outroAt : age;
                        int index = Mathf.Clamp(frameAge / failureFrameMilliseconds,
                            0, failureFrames.Length - 1);
                        uiPrompt.texture = failureFrames[index];
                        promptRoot.gameObject.SetActive(true);
                        return;
                    }
                }
                promptStartAfterFailure = failurePrompt;
                failurePrompt = PromptKind.None;
                shownPrompt = PromptKind.None;
                promptEligibilityMask = -1;
                promptPhase = PromptPlaybackPhase.None;
            }
            // Roulette::SetMessages enables these four messages independently.
            // AnimMessageManager::PlayNext then cycles through active messages.
            var active = new List<PromptKind>(4);
            // StateIdle restarts source messages with a 1000 ms Play delay.
            bool idleMessageReady =
                (forcedInsertCredits || nowTimestamp >= promptReadyTimestamp) &&
                (!session.ResultPhase ||
                 (!winRackActive && !ResultDisplayActiveAt(nowTimestamp)));
            if (!spinning && idleMessageReady && !session.InDoubleUp && paysPage < 0 &&
                (!SourceRoundBlocksOrdinaryInput || gameOverMessageLatched))
            {
                if (gameOverMessageLatched ||
                    session.ResultPhase && session.LastResult != null ||
                    sourceNamedPrompt == PromptKind.GameOver)
                    active.Add(PromptKind.GameOver);
                if (!PcCurrentResultHistoryUnpublished && (forcedInsertCredits || session.Credit <= 0 ||
                    session.Credit < session.TotalBet || session.Credit < session.Chip ||
                    sourceNamedPrompt == PromptKind.InsertCredits))
                    active.Add(PromptKind.InsertCredits);
                if (!PcCurrentResultHistoryUnpublished && (session.TotalBet > 0 && session.TotalBet <= session.Credit &&
                    session.TotalBet <= session.Profile.MaximumTotalBet ||
                    sourceNamedPrompt == PromptKind.PressSpin))
                    active.Add(PromptKind.PressSpin);
                // Source UpdateDisplay sets the table maximum to the lesser of
                // available credit and the configured operator cap. The table
                // manager then tests current wagers plus the selected chip.
                if (!PcCurrentResultHistoryUnpublished && (session.Credit > 0 && session.Chip > 0 &&
                    session.TotalBet <= session.Credit - session.Chip &&
                    session.TotalBet <= session.Profile.MaximumTotalBet - session.Chip ||
                    sourceNamedPrompt == PromptKind.PlaceBets))
                    active.Add(PromptKind.PlaceBets);
            }
            int mask = 0;
            foreach (PromptKind kind in active) mask |= 1 << (int)kind;
            if (active.Count == 0)
            {
                promptEligibilityMask = mask;
                promptPhase = PromptPlaybackPhase.None;
                shownPrompt = PromptKind.None;
                uiPromptButton.interactable = false;
                promptRoot.gameObject.SetActive(false);
                return;
            }
            if (mask != promptEligibilityMask || promptPhase == PromptPlaybackPhase.None)
            {
                promptEligibilityMask = mask;
                shownPrompt = promptStartNow != PromptKind.None &&
                    active.Contains(promptStartNow) ? promptStartNow :
                    promptStartAfterFailure == PromptKind.None ? active[0] :
                    NextActivePrompt(promptStartAfterFailure, active);
                promptStartNow = PromptKind.None;
                promptStartAfterFailure = PromptKind.None;
                promptPhase = PromptPlaybackPhase.Begin;
                promptPhaseTimestamp = nowTimestamp;
            }
            Texture2D[] intro = PromptFrames(shownPrompt, false);
            Texture2D[] outro = PromptFrames(shownPrompt, true);
            if (intro == null || intro.Length == 0 || outro == null || outro.Length == 0)
            {
                uiPromptButton.interactable = false;
                promptRoot.gameObject.SetActive(false);
                return;
            }
            const int frameMilliseconds = 42;
            for (int transitions = 0; transitions < 16; ++transitions)
            {
                int durationMilliseconds = promptPhase == PromptPlaybackPhase.Begin ?
                    intro.Length * frameMilliseconds :
                    promptPhase == PromptPlaybackPhase.Hold ||
                    promptPhase == PromptPlaybackPhase.Blank ? 3000 :
                    outro.Length * frameMilliseconds;
                if (promptPhase == PromptPlaybackPhase.Hold && active.Count == 1 ||
                    nowTimestamp < OriginalRackClock.AfterMilliseconds(
                        promptPhaseTimestamp, durationMilliseconds, clockFrequency)) break;
                promptPhaseTimestamp = OriginalRackClock.AfterMilliseconds(
                    promptPhaseTimestamp, durationMilliseconds, clockFrequency);
                if (promptPhase == PromptPlaybackPhase.Begin)
                    promptPhase = PromptPlaybackPhase.Hold;
                else if (promptPhase == PromptPlaybackPhase.Hold)
                    promptPhase = PromptPlaybackPhase.End;
                else if (promptPhase == PromptPlaybackPhase.End)
                    promptPhase = PromptPlaybackPhase.Blank;
                else
                {
                    shownPrompt = NextActivePrompt(shownPrompt, active);
                    intro = PromptFrames(shownPrompt, false);
                    outro = PromptFrames(shownPrompt, true);
                    promptPhase = PromptPlaybackPhase.Begin;
                }
            }
            if (promptPhase == PromptPlaybackPhase.Blank)
            {
                uiPromptButton.interactable = false;
                promptRoot.gameObject.SetActive(false);
                return;
            }
            Texture2D[] frames = promptPhase == PromptPlaybackPhase.End ? outro : intro;
            int frame = promptPhase == PromptPlaybackPhase.Hold ? frames.Length - 1 :
                Mathf.Clamp(OriginalRackClock.ElapsedMilliseconds(
                    promptPhaseTimestamp, nowTimestamp, clockFrequency) / frameMilliseconds,
                    0, frames.Length - 1);
            uiPrompt.texture = frames[frame];
            uiPromptButton.interactable = shownPrompt == PromptKind.PressSpin &&
                promptPhase != PromptPlaybackPhase.End && !spinning && session.TotalBet > 0 &&
                session.TotalBet <= session.Credit && !session.InDoubleUp &&
                !autoplay;
            promptRoot.gameObject.SetActive(true);
        }

        private static PromptKind NextActivePrompt(PromptKind previous,
            List<PromptKind> active)
        {
            // AddSprite hooks each newly created node before the old head.
            PromptKind[] sourceOrder = { PromptKind.GameOver,
                PromptKind.BetSpotLimitReached, PromptKind.BetLimitReached,
                PromptKind.InsertCredits, PromptKind.PressSpin, PromptKind.PlaceBets };
            int start = Array.IndexOf(sourceOrder, previous);
            for (int offset = 1; offset <= sourceOrder.Length; ++offset)
            {
                PromptKind candidate = sourceOrder[(start + offset + sourceOrder.Length) %
                    sourceOrder.Length];
                if (active.Contains(candidate)) return candidate;
            }
            return active[0];
        }

        private void RefreshDoubleUi()
        {
            if (IsDoubleExit)
            {
                if (!doubleExitVisualActive)
                {
                    for (int i = 0; i < doubleRoot.childCount; ++i)
                    {
                        Transform child = doubleRoot.GetChild(i);
                        child.gameObject.SetActive(child ==
                            (doubleEntranceLayer == null ?
                                uiDoubleBackground.transform : doubleEntranceLayer));
                    }
                    if (doubleEntranceLayer != null)
                        for (int i = 0; i < doubleEntranceLayer.childCount; ++i)
                        {
                            Transform child = doubleEntranceLayer.GetChild(i);
                            child.gameObject.SetActive(child == uiDoubleBackground.transform);
                        }
                    doubleExitVisualActive = true;
                }
                doubleRoot.anchoredPosition = new Vector2(0, -1920f * DoubleExitProgress);
                return;
            }
            if (doubleExitVisualActive)
            {
                for (int i = 0; i < doubleRoot.childCount; ++i)
                    doubleRoot.GetChild(i).gameObject.SetActive(true);
                if (doubleEntranceLayer != null)
                    for (int i = 0; i < doubleEntranceLayer.childCount; ++i)
                        doubleEntranceLayer.GetChild(i).gameObject.SetActive(true);
                doubleExitVisualActive = false;
            }
            doubleRoot.anchoredPosition = Vector2.zero;
            if (doubleEntranceLayer != null)
                doubleEntranceLayer.anchoredPosition =
                    new Vector2(0, -1920f * (1f - DoubleEntranceProgress));
            int redState = DoubleColorButtonState(-1);
            int blackState = DoubleColorButtonState(-2);
            bool suitMode = session.DoubleSuitMode;
            // The original base feature always draws disabled underlays. The
            // normal and flash features cover them only while enabled.
            uiDoubleRedButtonOverlay.texture = suitMode ?
                (redState == 2 ? suitButtonsFlash[0] : suitButtons[0]) :
                doubleRedButtonFlash;
            uiDoubleRedButtonOverlay.gameObject.SetActive(suitMode ? redState != 0 : redState == 2);
            uiDoubleBlackButtonOverlay.texture = suitMode ?
                (blackState == 2 ? suitButtonsFlash[1] : suitButtons[1]) :
                (blackState == 2 ? doubleBlackButtonFlash : doubleBlackButton);
            uiDoubleBlackButtonOverlay.gameObject.SetActive(blackState != 0);
            if (suitMode)
                for (int i = 0; i < 4; ++i)
                {
                    int state = DoubleColorButtonState(i);
                    uiSuitChoiceOverlays[i].texture = state == 2 ?
                        suitButtonsFlash[i + 2] : suitButtons[i + 2];
                    uiSuitChoiceOverlays[i].gameObject.SetActive(state != 0);
                    uiSuitChoiceButtons[i].gameObject.SetActive(state != 0);
                    uiSuitChoiceButtons[i].GetComponent<RawImage>().texture =
                        state == 2 ? suitButtonsFlash[i + 2] : suitButtons[i + 2];
                    // The idle lamp animation changes art, not the touch area.
                    uiSuitChoiceButtons[i].interactable = session.InDoubleUp &&
                        DoubleDecisionReady && !IsDoubleReveal &&
                        !showingDoubleHelp && !doubleCollectPending;
                }
            uiDoubleTakeWinOverlay.texture = DoubleTakeWinFlash ?
                doubleTakeWinFlash : doubleTakeWinButton;
            uiDoubleTakeWinOverlay.rectTransform.sizeDelta =
                new Vector2(uiDoubleTakeWinOverlay.texture.width,
                    uiDoubleTakeWinOverlay.texture.height);
            uiDoubleTakeWinOverlay.gameObject.SetActive(!suitMode && !showingDoubleHelp &&
                !IsDoubleReveal && (DoubleDecisionReady || doubleCollectPending));
            uiDoubleRedButton.interactable = DoubleDecisionReady && !IsDoubleReveal &&
                !showingDoubleHelp && !doubleCollectPending;
            uiDoubleBlackButton.interactable = DoubleDecisionReady && !IsDoubleReveal &&
                !showingDoubleHelp && !doubleCollectPending;
            uiDoubleTakeWinButton.interactable = !suitMode && DoubleDecisionReady &&
                !showingDoubleHelp && !IsDoubleReveal && !doubleCollectPending;
            uiDoubleHelpButton.interactable = DoubleDecisionReady &&
                !showingDoubleHelp && !IsDoubleReveal && !doubleCollectPending;
            int attractMultiplier = suitMode ? CurrentDoubleAttractMultiplier() : 2;
            Texture2D message = suitMode && IsDoubleReveal && !doubleRevealMaximum ?
                (doubleRevealWon ? suitWin : suitLose) : IsDoubleReveal ?
                (doubleRevealMaximum ? (suitMode ? suitMaximumAward : doubleMaximumAward) :
                    (doubleRevealWon ? doubleWin : doubleLose)) : suitMode ?
                (attractMultiplier == 4 ? doublePrompt4x : doublePrompt2x) :
                doublePrompt;
            uiDoubleMessage.texture = message;
            // Auto Take plays fMessageBox, whose trigger kills fPrompt. Its
            // bMessageBox is absent from the packaged bitmap definitions.
            uiDoubleMessage.gameObject.SetActive(message != null && !showingDoubleHelp &&
                !PcNativeDoubleFeatureAutoTake &&
                (IsDoubleReveal || (DoubleDecisionReady &&
                    CurrentDoubleIdleAnimationStep() >= 0 &&
                    (!suitMode || attractMultiplier != 0))));
            Rect messageRect = suitMode && IsDoubleReveal && !doubleRevealMaximum ?
                (doubleRevealWon ? new Rect(406, 742, 269, 39) :
                    new Rect(461, 743, 154, 39)) : IsDoubleReveal ?
                (doubleRevealMaximum ? new Rect(239, 686, 585, 70) :
                    (doubleRevealWon ? new Rect(302, 696, 483, 62) :
                        new Rect(392, 697, 299, 62))) : suitMode ?
                (attractMultiplier == 4 ? new Rect(181, 673, 703, 52) :
                    new Rect(233, 675, 618, 49)) : new Rect(247, 696, 589, 63);
            uiDoubleMessage.rectTransform.anchoredPosition =
                new Vector2(messageRect.x, -messageRect.y);
            uiDoubleMessage.rectTransform.sizeDelta = messageRect.size;
            Texture2D shownCard = suitMode ? suitCardback : doubleCardback;
            if (IsDoubleReveal && session.LastDoubleCard >= 0)
            {
                int suit = OriginalDoubleSuit.ViewIndex(session.LastDoubleCard);
                shownCard = doubleRevealChoice < 0 ?
                    (suit < 2 ? (session.DoubleSuitMode ? suitRedCard : doubleRedCard) :
                        (session.DoubleSuitMode ? suitBlackCard : doubleBlackCard)) :
                    doubleSuitCards[suit];
            }
            uiDoubleCard.texture = shownCard;
            Rect cardRect = session.DoubleSuitMode ? new Rect(428, 840, 224, 308) :
                IsDoubleReveal && doubleRevealChoice >= 0 ?
                new Rect(428, 840, 224, 308) : new Rect(401, 807, 271, 370);
            uiDoubleCard.rectTransform.anchoredPosition =
                new Vector2(cardRect.x, -cardRect.y);
            uiDoubleCard.rectTransform.sizeDelta = cardRect.size;
            // DisplayHelpScreen stops only fAmountMeters (red/black). Suit
            // counters belong to fSGAmountMeters and remain visible below
            // the suit Help bitmap while that feature is open.
            bool amountMetersVisible = DoubleInitialMetersReady &&
                (suitMode || !showingDoubleHelp);
            uiDoubleCredit.Set(!amountMetersVisible ? "" : suitMode ? session.Credit.ToString() :
                RouletteDenominationFormatter.ForFeatureCurrency(
                    checked(session.Credit * session.Denomination)));
            uiDoubleBet.Set(!amountMetersVisible ? "" : (session.LastResult == null ? 0 :
                session.LastResult.Wager).ToString());
            // fDenomMeter is separate from fAmountMeters. It remains active
            // behind the full-height red/black Help background.
            uiDoubleDenom.Set(!DoubleInitialMetersReady || suitMode ? "" :
                RouletteDenominationFormatter.ForFeatureCurrency(session.Denomination));
            uiDoubleStake.Set(!amountMetersVisible || suitMode ? "" :
                session.DoubleStake.ToString());
            uiDoubleCurrentWin.Set(!amountMetersVisible ? "" :
                session.DoubleStake.ToString());
            int meterMultiplier = suitMode ? IsDoubleReveal ?
                doubleAttractMultiplierOnReveal : Math.Max(2, attractMultiplier) : 2;
            uiDoubleToWin.Set(!amountMetersVisible ? "" :
                checked(session.DoubleStake * meterMultiplier).ToString());
            if (suitMode)
            {
                long cashCredit = checked(session.Credit * session.Denomination);
                long cashBet = checked((session.LastResult == null ? 0 :
                    session.LastResult.Wager) * session.Denomination);
                long cashStake = checked(session.DoubleStake * session.Denomination);
                long cashToWin = checked(cashStake * meterMultiplier);
                uiSuitCreditCash.Set(!amountMetersVisible ? "" :
                    RouletteDenominationFormatter.ForFeatureCurrency(cashCredit));
                uiSuitBetCash.Set(!amountMetersVisible ? "" :
                    RouletteDenominationFormatter.ForFeatureCurrency(cashBet));
                uiSuitCurrentWinCash.Set(!amountMetersVisible ? "" :
                    RouletteDenominationFormatter.ForFeatureCurrency(cashStake));
                uiSuitToWinCash.Set(!amountMetersVisible ? "" :
                    RouletteDenominationFormatter.ForFeatureCurrency(cashToWin));
            }
            int maximumRounds = session.DoubleMaximumRounds;
            // The packaged RBDU0 graph has the generic red/black Help
            // background and both suit variants. Its two Australian
            // red/black background feature names are absent, so their
            // PlayFeature calls leave the underlying feature screen visible.
            uiDoubleHelpBackground.gameObject.SetActive(
                suitMode || !auNationalStandardsEnabled);
            // SetVars selects fMaxRoundsRB## for Australian red/black, but
            // the packaged feature graph has no objects with those names.
            Texture2D helpRounds = suitMode ?
                maximumRounds >= 2 && maximumRounds < suitHelpRounds.Length ?
                    suitHelpRounds[maximumRounds] : null :
                auNationalStandardsEnabled ? null : DoubleHelpRoundsBitmap();
            uiDoubleHelpRounds.texture = helpRounds;
            uiDoubleHelpRounds.gameObject.SetActive(helpRounds != null);
            // SetVars compares the absolute distance from 2,000,000,000
            // against 0.1. Exactly that fallback value selects the source
            // no-maximum Help branch and does not start a maximum-win meter.
            // Red/black also requires Australian national standards; without
            // that platform flag DisplayHelpScreen uses fHelpBackground.
            if (session.DoubleMaximumWin != 2000000000L &&
                (suitMode || auNationalStandardsEnabled))
            {
                decimal divisor = suitMode ? 4m : 2m;
                decimal amount = decimal.Truncate(
                    (decimal)session.DoubleMaximumWin * session.Denomination / divisor);
                uiDoubleHelpWinLimit.Set(RouletteDenominationFormatter.ForFeatureCurrency(
                    (long)Math.Min(amount, long.MaxValue)));
            }
            else uiDoubleHelpWinLimit.Set("");
            if (helpRounds != null)
            {
                Rect bounds = suitMode ? new Rect(
                    session.DoubleMaximumRounds < 3 ? 201 :
                        session.DoubleMaximumRounds < 10 ? 199 : 191,
                    1413, helpRounds.width, helpRounds.height) :
                    DoubleHelpRoundsRect(session.DoubleMaximumRounds, helpRounds);
                uiDoubleHelpRounds.rectTransform.anchoredPosition =
                    new Vector2(bounds.x, -bounds.y);
                uiDoubleHelpRounds.rectTransform.sizeDelta = bounds.size;
            }
            uiDoubleHelp.SetActive(showingDoubleHelp && !IsDoubleReveal);
        }

        private IEnumerator UiSmokeRound(bool historySmoke)
        {
            yield return null;
            yield return null;
            if (session.TotalBet == 0 && uiSpinButton.interactable)
                throw new InvalidOperationException("PLAY enabled without a wager");
            ClickThroughUiRaycast(uiZeroBetButton);
            if (session.TotalBet != 1)
                throw new InvalidOperationException("uGUI bet button did not place a chip");
            yield return null;
            if (!uiSpinButton.interactable)
                throw new InvalidOperationException("PLAY stayed disabled after a valid wager");
            int historyBeforeSpin = recordedPockets.Count;
            ClickThroughUiRaycast(uiSpinButton);
            if (!spinning || !session.SpinPending ||
                recordedPockets.Count != historyBeforeSpin)
                throw new InvalidOperationException("uGUI spin did not enter pending wheel state: " +
                    "spinning=" + spinning + " pending=" + session.SpinPending +
                    " history=" + recordedPockets.Count + "/" + historyBeforeSpin +
                    " status=" + status);
            RefreshPortraitUi();
            if (!uiUndoButton.gameObject.activeSelf ||
                !uiClearButton.gameObject.activeSelf ||
                uiUndoButton.interactable || uiClearButton.interactable)
                throw new InvalidOperationException("Source table controls visibility/input mismatch during spin");
            Debug.Log("ROULETTE_UGUI_CLICK_FLOW_PASSED: bet, spin");
            float spindleStartAngle = uiSpindle.rectTransform.localEulerAngles.z;
            yield return new WaitForSecondsRealtime(.18f);
            RefreshPortraitUi();
            float spindleAngle = uiSpindle.rectTransform.localEulerAngles.z;
            float mainAngle = uiWheel.rectTransform.localEulerAngles.z;
            if (uiWheel.rectTransform.anchoredPosition != new Vector2(472,-383) ||
                uiSpindle.rectTransform.anchoredPosition != new Vector2(472,-384) ||
                Mathf.Abs(Mathf.DeltaAngle(mainAngle, -wheelMainRenderAngle)) > .01f ||
                Mathf.Abs(Mathf.DeltaAngle(spindleAngle, -wheelSpindleRenderAngle)) > .01f)
                throw new InvalidOperationException("Source rotation pivot/float conversion differs");
            if (!spinning || Mathf.Abs(Mathf.DeltaAngle(spindleStartAngle,
                    spindleAngle)) < 1f ||
                Mathf.Abs(Mathf.DeltaAngle(mainAngle, spindleAngle)) > .01f)
                throw new InvalidOperationException(
                    "Original spindle did not rotate with the main wheel");
            Debug.Log("ROULETTE_SPINDLE_ROTATION_PASSED: wheel=" + mainAngle +
                      " spindle=" + spindleAngle);
            yield return new WaitUntil(() => !spinning);
            if (session.SpinPending || recordedPockets.Count != historyBeforeSpin + 1 ||
                recordedPockets[0] != session.LastResult.Pocket)
                throw new InvalidOperationException("Wheel stop did not publish the new result");
            if (Vector2.Distance(wheelBallPosition,
                    BallCenterForPocket(session.LastResult.Pocket,
                        wheelRenderedAngle)) > .01f)
                throw new InvalidOperationException("settled ball does not match the pocket");
            if (recordedRoundPages.Count == 0 ||
                !recordedRoundPages[0].sourceWheelStopRecorded ||
                recordedRoundPages[0].sourceWheelStopIndex != wheelFinalStopIndex ||
                Vector2.Distance(RestoredBallCenter(recordedRoundPages[0]),
                    OriginalWheelArithmetic.BallCenter(SourceStopIndex(session.LastResult.Pocket),
                        OriginalWheelArithmetic.RestorePhase(wheelFinalStopIndex), true)) > .01f)
                throw new InvalidOperationException(
                    "Completed wheel did not archive its source stop index");
            Debug.Log("ROULETTE_BALL_SETTLED_PASSED: pocket=" + session.LastResult.Pocket);
            if (session.LastResult.Returned == 0)
            {
                if (ResultDisplayActive)
                    throw new InvalidOperationException("Manual loss entered AutoPlay result wait");
                RefreshPortraitUi();
                bool creditAllowsInput = session.Credit > 0;
                bool wagerAllowsSpin = session.TotalBet > 0 &&
                    session.TotalBet <= session.Credit &&
                    session.TotalBet <= session.Profile.MaximumTotalBet;
                if (uiSpinButton.interactable != wagerAllowsSpin ||
                    uiZeroBetButton.interactable != creditAllowsInput ||
                    uiVoisinsButton.interactable != creditAllowsInput ||
                    uiOrphelinsButton.interactable != creditAllowsInput ||
                    uiTiersButton.interactable != creditAllowsInput ||
                    uiChipSelections[1].texture != originalMainChipButtonFrames[1][
                        creditAllowsInput ? 0 : 2])
                    throw new InvalidOperationException("Manual loss table input gate mismatch");
                if (!creditAllowsInput)
                {
                    long wagerBeforeBlockedClick = session.TotalBet;
                    ClickThroughUiRaycast(uiZeroBetButton);
                    if (session.TotalBet != wagerBeforeBlockedClick)
                        throw new InvalidOperationException(
                            "Zero-credit table accepted a bet click");
                    Debug.Log("ROULETTE_ZERO_CREDIT_TABLE_TOUCH_GATE_PASSED");
                }
                Debug.Log(creditAllowsInput ?
                    "ROULETTE_MANUAL_LOSS_NO_WAIT_PASSED: input reopened at idle" :
                    "ROULETTE_ZERO_CREDIT_TABLE_LOCK_PASSED: table remains locked at idle");
            }
            if (historySmoke)
            {
                yield return new WaitUntil(() => !ResultDisplayActive);
                yield return null;
                ClickThroughUiRaycast(uiSpinButton);
                if (!spinning) throw new InvalidOperationException("second spin did not start");
                yield return new WaitUntil(() => !spinning);
                yield return null;
                if (recordedPockets.Count < 2 || uiColoredHistoryRoot.childCount < 1 ||
                    recordedPockets[0] != session.History[0].Pocket ||
                    recordedPockets[1] != session.History[1].Pocket)
                    throw new InvalidOperationException("tote history did not show two results");
                Debug.Log("ROULETTE_HISTORY_SMOKE_PASSED: latest=" + recordedPockets[0] +
                    " previous=" + recordedPockets[1]);
            }
            captureAt = Time.realtimeSinceStartup + .7f;
        }

        private IEnumerator WinTimingSmoke()
        {
            yield return new WaitUntil(() => !spinning);
            if (!winRackActive || winRackTier != 1 || winRackPaid != 0 ||
                winRackFinal != 36 || DisplayedCredit != session.Credit - 36)
                throw new InvalidOperationException("Original tier 2 rack did not start at zero");
            yield return new WaitForSecondsRealtime(.8f);
            if (!winRackActive || winRackPaid <= 0 || winRackPaid >= winRackFinal ||
                DisplayedCredit != session.Credit - winRackFinal + winRackPaid ||
                !ResultDisplayActive)
                throw new InvalidOperationException("Win-rack meters did not advance during payout");
            RefreshPortraitUi();
            if (!uiTotalWinTitle.gameObject.activeSelf ||
                VisibleWinningPayId() != session.LastResult.WinningPays[0].BetId)
                throw new InvalidOperationException(
                    "Original first pay did not show during manual win rack");
            BeginSpin();
            if (spinning || session.Rounds != 1)
                throw new InvalidOperationException("A new spin interrupted the win rack");
            yield return new WaitUntil(() => !winRackActive);
            yield return null;
            if (session.LastResult == null || session.LastResult.Returned <= 0 ||
                uiTotalWinTitle.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Manual result cycle did not hide its first pay after 1500 ms");
            yield return new WaitForSecondsRealtime(Mathf.Max(0f,
                resultCycleStartedAt + 3.12f - Time.realtimeSinceStartup));
            RefreshPortraitUi();
            if (!uiTotalWinTitle.gameObject.activeSelf ||
                VisibleWinningPayId() != session.LastResult.WinningPays[0].BetId)
                throw new InvalidOperationException(
                    "Manual result cycle did not show its next pay");
            yield return new WaitForSecondsRealtime(Mathf.Max(0f,
                resultCycleStartedAt + 4.62f - Time.realtimeSinceStartup));
            yield return null;
            if (uiTotalWinTitle.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Manual result cycle did not hide its repeated pay");
            Debug.Log("ROULETTE_WIN_TIMING_PASSED: sourceCycleMs=1500 rackMs=2194");
        }

        private IEnumerator FooterMovieSmoke()
        {
            yield return null;
            if (footerGamePlayIntroFrames.Length != 36 ||
                footerGamePlayFrames.Length != 36 ||
                footerGamePlayOutroFrames.Length != 12 ||
                footerMoviePhase != FooterMoviePhase.Intro)
                throw new InvalidOperationException("Original footer Bink frames or intro missing");
            yield return new WaitForSecondsRealtime(
                footerGamePlayIntroFrames.Length * FooterMovieFrameSeconds + .1f);
            if (CurrentFooterMovieFrame() == null || footerMoviePhase != FooterMoviePhase.Loop)
                throw new InvalidOperationException("Footer intro did not enter its loop");
            yield return new WaitUntil(() => !spinning);
            yield return null;
            if (session.LastResult == null || session.LastResult.Returned <= 0 ||
                footerMoviePhase != FooterMoviePhase.Outro)
                throw new InvalidOperationException("Winning payout did not start footer outro");
            if (uiTotalWinAmount.Value != "(36)" || !uiTotalWinTitle.gameObject.activeSelf)
                throw new InvalidOperationException("Normal spin total-win meter missing");
            RequireTotalWinNative(uiTotalWinAmount, "(36)", 326);
            string[] totalArgs = Environment.GetCommandLineArgs();
            int totalCapture = Array.IndexOf(totalArgs, "-rouletteTotalWinCapture");
            if (totalCapture >= 0 && totalCapture + 1 < totalArgs.Length)
                CaptureActiveScreenOffscreen(totalArgs[totalCapture + 1]);
            if (uiPaid.Value != DisplayedPaid.ToString() ||
                uiPaid.GlyphCount == 0 || !uiCashPaid.Visible ||
                uiCashPaid.Value != RouletteDenominationFormatter.ForFeatureCurrency(
                    DisplayedPaid * session.Denomination) ||
                DisplayedCredit >= session.Credit)
                throw new InvalidOperationException(
                    "Original paid, cash paid, or credit meters are missing during the rack");
            yield return new WaitForSecondsRealtime(
                footerGamePlayOutroFrames.Length * FooterMovieFrameSeconds + .1f);
            if (CurrentFooterMovieFrame() != null || footerMoviePhase != FooterMoviePhase.Hidden)
                throw new InvalidOperationException("Footer outro did not finish");
            if (!winRackActive)
                throw new InvalidOperationException("Win rack ended before the original footer outro");
            yield return new WaitUntil(() => !winRackActive);
            yield return null;
            if (uiPaid.Value != "36" || uiPaid.GlyphCount != 2)
                throw new InvalidOperationException("Original paid meter did not finish at 36");
            if (ResultDisplayActive)
                throw new InvalidOperationException("Manual win entered the AutoPlay result wait");
            BeginSpin();
            if (!spinning || footerMoviePhase != FooterMoviePhase.Intro)
                throw new InvalidOperationException("Next round did not restart footer intro");
            Debug.Log("ROULETTE_FOOTER_MOVIE_PASSED: 36 intro, 36 loop, 12 outro frames");
            Debug.Log("ROULETTE_TOTAL_WIN_NORMAL_SPIN_PASSED: 36-credit native meter during source payout path");
        }

        private IEnumerator AutoTimingSmoke()
        {
            yield return new WaitUntil(() => session != null && session.Rounds == 1);
            if (!winRackAutoSkip || winRackFinal != 36)
                throw new InvalidOperationException("Autoplay win rack did not select source skip path");
            yield return new WaitUntil(() => !winRackActive);
            if (resultSettledAt - winRackStartedAt > .5f)
                throw new InvalidOperationException("Autoplay counted the entire win rack");
            yield return null;
            if (session.LastResult == null || session.LastResult.Returned <= 0 ||
                !ResultDisplayActive || !uiTotalWinTitle.gameObject.activeSelf)
                throw new InvalidOperationException("Autoplay did not show the first winning result");
            if (uiPaid.Value != winRackFinal.ToString() ||
                uiPaid.GlyphCount == 0 || DisplayedCredit != session.Credit +
                    (DeferredPaymentSmokeEnabled ? session.LastResult.Returned : 0))
                throw new InvalidOperationException(
                    "Autoplay did not advance the paid and credit meters in one service");
            ObserveDeferredPaymentSmokeUnpaid(963, 36, 37, true);
            OriginalResultDisplayWaitClock firstWait = pcResultDisplayWait;
            long firstStarted = resultSettledTimestamp;
            if (firstWait == null || firstWait.InitialWaits != 30)
                throw new InvalidOperationException("Auto win did not enter thirty source WaitInput calls");
            float timeout = Time.realtimeSinceStartup + 5f;
            while (!spinning && Time.realtimeSinceStartup < timeout)
            {
                if (!firstWait.Completed && session.Rounds != 1)
                    throw new InvalidOperationException("Auto win advanced before its result waits returned");
                RequireDeferredPaymentSmokeStillWaiting();
                yield return null;
            }
            if (!spinning || !firstWait.Completed || firstWait.TimeoutCount != 30 || firstWait.InputCount != 0 ||
                firstWait.HighDelayDueTimestamp != 0 || session.Rounds != 1 ||
                firstWait.CompletedAt < OriginalRackClock.AfterMilliseconds(firstStarted, 1050,
                    System.Diagnostics.Stopwatch.Frequency))
                throw new InvalidOperationException("Auto win did not complete thirty fresh no-input waits");
            CompleteDeferredPaymentSmoke(963, 36, 37, true);
            ToggleAuto();
            Debug.Log("ROULETTE_AUTOPLAY_RESULT_GATE_PASSED: waits=30 timeout=30 input=0 nominalMs=1050 actualMs=" +
                OriginalRackClock.ElapsedMillisecondsLong(firstStarted, firstWait.CompletedAt,
                    System.Diagnostics.Stopwatch.Frequency));
            if (Array.IndexOf(Environment.GetCommandLineArgs(),
                "-rouletteAutoWinFilmGateSmoke") >= 0)
                Application.Quit();
        }

        private IEnumerator ResultCycleSmoke()
        {
            yield return null;
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome(0));
            int[] ids = session.Profile.Records.Where(pair => pair.Value.Covers(0))
                .Select(pair => pair.Key).OrderBy(id => id).Take(2).ToArray();
            if (ids.Length != 2)
                throw new InvalidOperationException("No two source pocket-zero pays for cycle test");
            session.PlaceBet(ids[0], 1);
            session.PlaceBet(ids[1], 1);
            RoundResult result = session.Spin();
            if (result.WinningPays.Count != 2 ||
                result.WinningPays[0].BetId != 100 ||
                result.WinningPays[0].Amount != 36 ||
                result.WinningPays[1].BetId != 251 ||
                result.WinningPays[1].Amount != 9 ||
                result.WinningPays.Sum(pay => pay.Amount) != result.Returned)
                throw new InvalidOperationException("Per-spot win records do not sum to return");
            resultCycleStartedAt = Time.realtimeSinceStartup;
            resultCycleStartedTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            long clockFrequency = System.Diagnostics.Stopwatch.Frequency;
            long distantStart = long.MaxValue / 4;
            long delayTicks = (clockFrequency * 3 + 1) / 2;
            if (CycleElapsedSeconds(distantStart,
                    distantStart + delayTicks - 1, clockFrequency) >= 1.5d ||
                CycleElapsedSeconds(distantStart,
                    distantStart + delayTicks, clockFrequency) < 1.5d ||
                WinPayAtTime(result, CycleElapsedSeconds(distantStart,
                    distantStart + delayTicks, clockFrequency)) != -1)
                throw new InvalidOperationException(
                    "Long-uptime result cycle crossed its 1500 ms boundary incorrectly");
            long distantDeadline = ResultClockDeadline(distantStart, 1.23f,
                clockFrequency);
            if (!ResultClockActive(distantDeadline, distantDeadline - 1) ||
                ResultClockActive(distantDeadline, distantDeadline))
                throw new InvalidOperationException(
                    "Long-uptime result display gate crossed its deadline incorrectly");
            // Native Start creates a Trigger with a one-millisecond first
            // receipt. Let that callback display the first winning pay.
            yield return null;
            RefreshPortraitUi();
            int first = result.WinningPays[0].BetId;
            int second = result.WinningPays[1].BetId;
            if (!uiTotalWinTitle.gameObject.activeSelf ||
                VisibleWinningPayId() != first || ActiveWinningHighlights() != 1 ||
                ActiveWinningPayoutGlyphs() != 2 ||
                uiLinePays.Value != "Straight Up Pays 36" ||
                uiGamePays.Value != "Game Pays " + result.Returned)
                throw new InvalidOperationException("First source win record is not isolated");
            yield return new WaitForSecondsRealtime(1.58f);
            RefreshPortraitUi();
            if (uiTotalWinTitle.gameObject.activeSelf ||
                VisibleWinningPayId() != -1 || ActiveWinningHighlights() != 0 ||
                ActiveWinningPayoutGlyphs() != 0 ||
                uiLinePays.Value != "" || uiGamePays.Value != "")
                throw new InvalidOperationException("Source win record did not hide");
            yield return new WaitForSecondsRealtime(1.55f);
            RefreshPortraitUi();
            if (!uiTotalWinTitle.gameObject.activeSelf ||
                VisibleWinningPayId() != second || ActiveWinningHighlights() != 1 ||
                ActiveWinningPayoutGlyphs() != 1 ||
                uiLinePays.Value != "First Four Pays 9" ||
                uiGamePays.Value != "Game Pays " + result.Returned)
                throw new InvalidOperationException("Second source win record is not isolated");
            Debug.Log("ROULETTE_RESULT_CYCLE_PASSED: first=" + first +
                " second=" + second + " delayMs=1500 total=" + result.Returned);
            Application.Quit();
        }

        private int ActiveWinningHighlights()
        {
            int count = 0;
            for (int i = 0; i < betChipLayer.childCount; ++i)
                if (betChipLayer.GetChild(i).gameObject.activeSelf &&
                    betChipLayer.GetChild(i).name == "Winning spot") ++count;
            return count;
        }

        private int ActiveWinningPayoutGlyphs()
        {
            int count = 0;
            for (int i = 0; i < betChipLayer.childCount; ++i)
            {
                Transform child = betChipLayer.GetChild(i);
                if (!child.gameObject.activeSelf ||
                    child.name != "Winning spot payout") continue;
                for (int glyph = 0; glyph < child.childCount; ++glyph)
                    if (child.GetChild(glyph).gameObject.activeSelf) ++count;
            }
            return count;
        }

        private IEnumerator BetSoundSmoke()
        {
            yield return null;
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            RefreshPortraitUi();
            if (((RawImage)uiUndoButton.targetGraphic).texture != mainUndoFrames[3] ||
                ((RawImage)uiClearButton.targetGraphic).texture != mainClearFrames[3])
                throw new InvalidOperationException(
                    "Empty table did not show original disabled Undo/Clear frames");
            Button mainChip = uiChipButtonStates[1].GetComponent<Button>();
            var chipPress = new PointerEventData(EventSystem.current) {
                button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(mainChip.gameObject, chipPress,
                ExecuteEvents.pointerDownHandler);
            if (session.Chip != 2)
                throw new InvalidOperationException(
                    "Original main chip did not select on pointer down");
            ExecuteEvents.Execute(mainChip.gameObject, chipPress,
                ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(mainChip.gameObject, chipPress,
                ExecuteEvents.pointerClickHandler);
            session.SetChip(1);
            RefreshPortraitUi();
            mainChip.interactable = false;
            ExecuteEvents.Execute(mainChip.gameObject, chipPress,
                ExecuteEvents.pointerDownHandler);
            if (session.Chip != 1)
                throw new InvalidOperationException("Disabled main chip accepted pointer down");
            mainChip.interactable = true;
            ClickThroughUiRaycast(mainChip);
            if (session.Chip != 2)
                throw new InvalidOperationException("Source chip selection sound path failed");
            session.SetChip(1);
            ExecuteEvents.Execute(mainChip.gameObject,
                new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            if (session.Chip != 2)
                throw new InvalidOperationException("PC keyboard submit lost chip selection");
            Debug.Log("ROULETTE_MAIN_CHIP_DOWN_PASSED: selection on press, disabled gate, no release replay");
            ClickThroughUiRaycast(uiZeroBetButton);
            if (session.TotalBet != 2)
                throw new InvalidOperationException("Source placed-bet sound path failed");
            RefreshPortraitUi();
            yield return null;
            if (((RawImage)uiUndoButton.targetGraphic).texture != mainUndoFrames[0] ||
                ((RawImage)uiClearButton.targetGraphic).texture != mainClearFrames[0])
                throw new InvalidOperationException(
                    "Staged bet did not show original Undo/Clear normal frames");
            PointerEventData undoPress = PressThroughUiRaycast(uiUndoButton);
            if (session.TotalBet != 2 ||
                ((RawImage)uiUndoButton.targetGraphic).texture != mainUndoFrames[1])
                throw new InvalidOperationException(
                    "Undo acted before source release or missed pressed frame");
            ReleaseUiPointer(uiUndoButton, undoPress);
            if (session.TotalBet != 0 || session.CanUndo)
                throw new InvalidOperationException("Source undo-bets sound path failed");
            ClickThroughUiRaycast(uiZeroBetButton);
            RefreshPortraitUi();
            yield return null;
            PointerEventData clearPress = PressThroughUiRaycast(uiClearButton);
            if (session.TotalBet != 2 ||
                ((RawImage)uiClearButton.targetGraphic).texture != mainClearFrames[1])
                throw new InvalidOperationException(
                    "Clear acted before source release or missed pressed frame");
            ReleaseUiPointer(uiClearButton, clearPress);
            if (session.TotalBet != 0 || session.CanUndo)
                throw new InvalidOperationException("Source clear-bets sound path failed");
            RefreshPortraitUi();
            if (!uiUndoButton.gameObject.activeSelf || !uiClearButton.gameObject.activeSelf ||
                uiUndoButton.interactable || uiClearButton.interactable ||
                ((RawImage)uiUndoButton.targetGraphic).texture != mainUndoFrames[3] ||
                ((RawImage)uiClearButton.targetGraphic).texture != mainClearFrames[3])
                throw new InvalidOperationException(
                    "Cleared table did not retain source-visible disabled controls");
            session = new RouletteSession(session.Profile, 1,
                new FixedPocketSmokeOutcome()) { DoubleEnabled = true };
            session.SetChip(2);
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiZeroBetButton);
            if (session.TotalBet != 0 || session.Credit != 1)
                throw new InvalidOperationException("Source bet-limit sound path failed");
            Debug.Log("ROULETTE_BET_SOUNDS_PASSED: selected, placed, removed, undo, clear, limit");
        }

        private IEnumerator MainTouchConfirmationSmoke()
        {
            yield return null;
            for (int mode = 0; mode < 3; ++mode)
            {
                session = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome());
                RefreshPortraitUi();
                yield return null;
                if (mode == 0) RequestPcService();
                else if (mode == 1) RequestPcCollectConfirmation();
                else RequestAutoPlay();
                if (mode == 0 ? !serviceConfirmPending :
                    mode == 1 ? !collectConfirmPending : !autoConfirmPending)
                    throw new InvalidOperationException(
                        "Source deck confirmation did not open for main touch test");
                long initialCredit = session.Credit;
                PointerEventData pointer = PressThroughUiRaycast(uiZeroBetButton);
                if (serviceConfirmPending || collectConfirmPending ||
                    autoConfirmPending || pcServiceRequested || pcCollectPending ||
                    autoplay || session.TotalBet != 0 || session.Credit != initialCredit)
                    throw new InvalidOperationException(
                        "Main touch did not cancel deck confirmation before bet release");
                ReleaseUiPointer(uiZeroBetButton, pointer);
                if (session.TotalBet != 1 || session.Credit != initialCredit)
                    throw new InvalidOperationException(
                        "Main touch did not continue to the source table wager");
            }
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome());
            RefreshPortraitUi();
            yield return null;
            RequestPcService();
            Button mainChip = uiChipButtonStates[1].GetComponent<Button>();
            PointerEventData chipPointer = PressThroughUiRaycast(mainChip);
            if (serviceConfirmPending || session.Chip != 2 ||
                session.TotalBet != 0)
                throw new InvalidOperationException(
                    "Main chip did not cancel confirmation before selection");
            ReleaseUiPointer(mainChip, chipPointer);

            session.SetChip(1);
            session.PlaceBet(100, 1);
            RefreshPortraitUi();
            yield return null;
            RequestPcCollectConfirmation();
            PointerEventData undoPointer = PressThroughUiRaycast(uiUndoButton);
            if (collectConfirmPending || session.TotalBet != 1 || pcCollectPending)
                throw new InvalidOperationException(
                    "Main Undo did not cancel confirmation before release");
            ReleaseUiPointer(uiUndoButton, undoPointer);
            if (session.TotalBet != 0)
                throw new InvalidOperationException("Main Undo did not execute on release");
            RefreshPortraitUi();
            yield return null;
            RequestPcService();
            PointerEventData disabledPointer = PressThroughUiRaycast(uiUndoButton);
            if (!serviceConfirmPending)
                throw new InvalidOperationException(
                    "Disabled main control canceled confirmation");
            ReleaseUiPointer(uiUndoButton, disabledPointer);
            ConfirmPcService(false);

            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome());
            RefreshPortraitUi();
            yield return null;
            PointerEventData heldPointer = PressThroughUiRaycast(uiZeroBetButton);
            RequestPcService();
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, heldPointer,
                ExecuteEvents.dragHandler);
            if (serviceConfirmPending || session.TotalBet != 0 ||
                pcServiceRequested)
                throw new InvalidOperationException(
                    "Captured table movement did not cancel pending confirmation");
            ReleaseUiPointer(uiZeroBetButton, heldPointer);
            if (session.TotalBet != 1)
                throw new InvalidOperationException(
                    "Captured table movement lost the source touch release wager");
            Debug.Log("ROULETTE_MAIN_TOUCH_CONFIRM_CANCEL_PASSED: bet, chip, Undo, disabled, move");
            Application.Quit();
        }

        private IEnumerator TableDragSmoke()
        {
            yield return null;
            yield return null;
            session = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome());
            RefreshPortraitUi();
            Button from = uiZeroBetButton;
            Button to = uiBetButtons[1];
            Canvas.ForceUpdateCanvases();
            Vector2 start = RectTransformUtility.WorldToScreenPoint(null,
                from.transform.TransformPoint(((RectTransform)from.transform).rect.center));
            Vector2 end = RectTransformUtility.WorldToScreenPoint(null,
                to.transform.TransformPoint(((RectTransform)to.transform).rect.center));
            var pointer = new PointerEventData(EventSystem.current) {
                position = start, button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0 || hits[0].gameObject != from.gameObject)
                throw new InvalidOperationException("Drag source is covered");
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.pointerDownHandler);
            if (hoveredBetSpot != 0 || !uiBetMarker.gameObject.activeSelf ||
                pointer.useDragThreshold)
                throw new InvalidOperationException(
                    "Table down did not arm source BetMarker movement");
            pointer.position = end;
            hits.Clear();
            EventSystem.current.RaycastAll(pointer, hits);
            if (hits.Count == 0 || hits[0].gameObject != to.gameObject)
                throw new InvalidOperationException("Drag destination is covered");
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.dragHandler);
            if (hoveredBetSpot != 1 || !uiBetMarker.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Table move did not reposition source BetMarker");
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.pointerUpHandler);
            if (session.TotalBet != 1 || !session.Bets.ContainsKey(101) ||
                session.Bets.ContainsKey(100) || uiBetMarker.gameObject.activeSelf)
                throw new InvalidOperationException("Release spot did not receive dragged wager");
            session.Clear();
            RefreshPortraitUi();
            pointer.position = start;
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.pointerDownHandler);
            pointer.position = RectTransformUtility.WorldToScreenPoint(null,
                uiInfoButton.transform.TransformPoint(
                    ((RectTransform)uiInfoButton.transform).rect.center));
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.pointerUpHandler);
            if (session.TotalBet != 0)
                throw new InvalidOperationException("Release outside table placed a wager");
            RefreshPortraitUi();
            ClickThroughUiRaycast(from);
            if (session.TotalBet != 1 || !session.Bets.ContainsKey(100))
                throw new InvalidOperationException("Same-spot release did not place wager");
            session.Clear();
            RefreshPortraitUi();
            ExecuteEvents.Execute(to.gameObject,
                new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            if (session.TotalBet != 1 || !session.Bets.ContainsKey(101))
                throw new InvalidOperationException("PC keyboard Submit lost table betting");
            session.Clear();
            RefreshPortraitUi();
            to.interactable = false;
            ExecuteEvents.Execute(to.gameObject,
                new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            if (session.TotalBet != 0)
                throw new InvalidOperationException("Disabled table spot accepted Submit");
            to.interactable = true;
            RouletteProfile limited = RouletteProfile.Load("97");
            limited.MaximumTotalBet = 3;
            session = new RouletteSession(limited, 1000,
                new FixedPocketSmokeOutcome());
            HideBetMarker();
            ClearNeighborSelection();
            RefreshPortraitUi();
            pointer.position = start;
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.pointerDownHandler);
            if (hoveredBetSpot != 0 || failurePrompt != PromptKind.BetLimitReached)
                throw new InvalidOperationException(
                    "Initial table down did not send the source hover rejection");
            long firstFailureAt = failurePromptStartedTimestamp;
            yield return new WaitForSecondsRealtime(.06f);
            var sourceHover = from.GetComponent<RouletteNeighborHover>();
            sourceHover.OnPointerEnter(pointer);
            sourceHover.OnDrag(pointer);
            sourceHover.OnDrag(pointer);
            if (failurePromptStartedTimestamp != firstFailureAt || hoveredBetSpot != 0)
                throw new InvalidOperationException(
                    "Repeated same-spot move restarted the source hover prompt");
            pointer.position = end;
            sourceHover.OnDrag(pointer);
            if (hoveredBetSpot != 1 || failurePromptStartedTimestamp <= firstFailureAt)
                throw new InvalidOperationException(
                    "Changed-spot move did not send a new source hover event");
            pointer.position = RectTransformUtility.WorldToScreenPoint(null,
                uiInfoButton.transform.TransformPoint(
                    ((RectTransform)uiInfoButton.transform).rect.center));
            ExecuteEvents.Execute(from.gameObject, pointer,
                ExecuteEvents.pointerUpHandler);
            if (session.TotalBet != 0 || uiBetMarker.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Off-table release after hover placed an unexpected bet");
            Debug.Log("ROULETTE_TABLE_RELEASE_PASSED: release target, outside cancel, same spot, Submit, disabled Submit, hover change gate");
            if (string.IsNullOrEmpty(capturePath)) Application.Quit();
        }

        private IEnumerator AutoCancelResultSmoke()
        {
            yield return new WaitUntil(() => session != null && session.Rounds == 1);
            yield return new WaitUntil(() => !winRackActive);
            yield return null;
            if (!autoplay || !ResultDisplayActive || session.LastResult == null ||
                session.LastResult.Returned <= 0 ||
                ProViewFlashingMessage() != "")
                throw new InvalidOperationException("AutoPlay cancellation setup failed");
            ObserveDeferredPaymentSmokeUnpaid(963, 36, 37, true);
            ToggleAuto();
            if (autoplay) throw new InvalidOperationException("AutoPlay cancellation retained active Auto");
            float cancelledTimeout = Time.realtimeSinceStartup + 1f;
            while (ResultDisplayActive && Time.realtimeSinceStartup < cancelledTimeout)
            {
                RequireDeferredPaymentSmokeStillWaiting();
                yield return null;
            }
            if (DeferredPaymentSmokeEnabled)
            {
                RequireDeferredPaymentSmoke(!ResultDisplayActive && pcResultDisplayWait.Completed,
                    "cancellation did not return its existing result wait");
                yield return WaitForDeferredPaymentSmokeIdle(() => pcGamePayment != null &&
                    pcGamePayment.historyPublished && sourceRoundEndCount == 1 &&
                    sourceRoundPhase == SourceRoundPhase.None && !configuredAutoPlayCheckPending,
                    "cancelled result StatePayout/Idle");
                CompleteDeferredPaymentSmoke(963, 36, 37, false);
            }
            RefreshPortraitUi();
            if (ResultDisplayActive || !uiSpinButton.interactable || ProViewFlashingMessage() != "Game Over")
                throw new InvalidOperationException("AutoPlay cancellation did not finish its current WaitInput");
            yield return new WaitForSecondsRealtime(
                WinningResultDisplayTicks * ResultWaitSeconds + .2f);
            if (session.Rounds != 1 || spinning)
                throw new InvalidOperationException("AutoPlay continued after cancellation");
            BeginSpin();
            if (!spinning)
                throw new InvalidOperationException("Manual spin did not resume after AutoPlay cancellation");
            Debug.Log("ROULETTE_AUTOPLAY_RESULT_CANCEL_PASSED: manual spin resumed");
        }

        private IEnumerator HighWinAutoTimingSmoke()
        {
            yield return new WaitUntil(() => session != null && session.Rounds == 1);
            if (!winRackAutoSkip || winRackTier != 4 || winRackFinal != 720)
                throw new InvalidOperationException("High-win tier 5 rack did not start");
            yield return new WaitUntil(() => !winRackActive);
            if (resultSettledAt - winRackStartedAt > .5f)
                throw new InvalidOperationException("High-win AutoPlay counted the full rack");
            yield return null;
            OriginalResultDisplayWaitClock firstWait = pcResultDisplayWait;
            long firstStarted = resultSettledTimestamp;
            if (session.LastResult == null || session.LastResult.Returned <= 500 ||
                firstWait == null || firstWait.InitialWaits != 30 || !ResultDisplayActive)
                throw new InvalidOperationException("Active Auto high win did not enter thirty source waits");
            long firstReturn = session.LastResult.Returned;
            ObserveDeferredPaymentSmokeUnpaid(980, 720, 20, true);
            float timeout = Time.realtimeSinceStartup + 5f;
            while (!spinning && Time.realtimeSinceStartup < timeout)
            {
                RequireDeferredPaymentSmokeStillWaiting();
                yield return null;
            }
            if (!spinning || !firstWait.Completed || firstWait.TimeoutCount != 30 || firstWait.InputCount != 0 ||
                firstWait.HighDelayDueTimestamp != 0 ||
                firstWait.CompletedAt < OriginalRackClock.AfterMilliseconds(firstStarted, 1050,
                    System.Diagnostics.Stopwatch.Frequency))
                throw new InvalidOperationException("Active-only high win added a configured delay or skipped its fresh waits");
            CompleteDeferredPaymentSmoke(980, 720, 20, true);
            ToggleAuto();
            Debug.Log("ROULETTE_HIGH_WIN_AUTO_DELAY_PASSED: returned=" + firstReturn +
                " waits=30 timeout=30 input=0 configuredDelay=false");
        }

        private IEnumerator HighWinManualTimingSmoke()
        {
            yield return new WaitUntil(() => session != null && session.Rounds == 1);
            if (!winRackActive || winRackTier != 4 || winRackFinal != 720 ||
                winRackAutoSkip)
                throw new InvalidOperationException("Manual tier 5 win rack did not start");
            yield return new WaitForSecondsRealtime(4f);
            if (!winRackActive || winRackPaid <= 0 || winRackPaid >= 720 ||
                DisplayedCredit != session.Credit - 720 + winRackPaid)
                throw new InvalidOperationException("Manual tier 5 meter did not advance");
            yield return new WaitUntil(() => !winRackActive);
            float seconds = resultSettledAt - winRackStartedAt;
            if (winRackPaid != 720 || seconds < 8f || seconds > 12f)
                throw new InvalidOperationException("Manual tier 5 rack duration or final meter failed");
            Debug.Log("ROULETTE_HIGH_WIN_MANUAL_RACK_PASSED: paid=720 seconds=" +
                seconds.ToString("F3", System.Globalization.CultureInfo.InvariantCulture));
        }

        private IEnumerator UndoNeighborSmoke()
        {
            yield return null;
            yield return null;
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome());
            selectedNeighbor = -1;
            neighborEnabledWithoutPrecheck = false;
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiZeroBetButton);
            ClickThroughUiRaycast(uiBetButtons[1]);
            RefreshPortraitUi();
            if (!uiUndoButton.interactable)
                throw new InvalidOperationException("Undo control did not open after direct bets");
            ClickThroughUiRaycast(uiUndoButton);
            if (selectedNeighbor != 0 || !uiNeighborButton.interactable ||
                session.TotalBet != 1)
                throw new InvalidOperationException(
                    "Undo did not restore prior single-number neighbor selection");
            ClickThroughUiRaycast(uiUndoButton);
            if (selectedNeighbor != -1 || uiNeighborButton.interactable ||
                session.TotalBet != 0)
                throw new InvalidOperationException(
                    "Undo did not clear neighbor selection after final operation");

            ClickThroughUiRaycast(uiZeroBetButton);
            SelectNeighbor(0);
            RefreshPortraitUi();
            if (selectedNeighbor != 0 || !uiClearButton.interactable)
                throw new InvalidOperationException(
                    "Clear test did not reach a selected neighbor with a wager");
            ClickThroughUiRaycast(uiClearButton);
            if (session.TotalBet != 0 || selectedNeighbor != -1 ||
                uiNeighborButton.interactable ||
                PlayerPrefs.GetInt(NeighborSelectionKey, -2) != -1)
                throw new InvalidOperationException(
                    "Clear did not disable and persist the cleared neighbor selector");

            RouletteProfile limited = RouletteProfile.Load("97");
            limited.MaximumTotalBet = 5;
            session = new RouletteSession(limited, 1000,
                new FixedPocketSmokeOutcome());
            selectedNeighbor = 0;
            neighborEnabledWithoutPrecheck = false;
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiNeighborButton);
            ClickThroughUiRaycast(uiBetButtons[1]);
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiUndoButton);
            if (session.TotalBet != 4 || session.CanPlaceNeighbor(0) ||
                selectedNeighbor != 0 || !uiNeighborButton.interactable ||
                !neighborEnabledWithoutPrecheck)
                throw new InvalidOperationException(
                    "Undo did not restore source neighbor button over the current limit");

            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome());
            selectedNeighbor = -1;
            neighborEnabledWithoutPrecheck = false;
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiOrphelinsButton);
            ClickThroughUiRaycast(uiBetButtons[1]);
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiUndoButton);
            if (selectedNeighbor != -1 || uiNeighborButton.interactable ||
                session.TotalBet != OriginalSectorBets.Spots(RouletteSector.Orphelins).Length)
                throw new InvalidOperationException(
                    "Undo treated a prior French group as a single-number neighbor");
            Debug.Log("ROULETTE_UNDO_NEIGHBOR_SOURCE_PASSED: direct, empty, clear, neighbor limit, French group");
            Application.Quit();
        }

        private IEnumerator NeighborRestoreSaveSmoke()
        {
            yield return null;
            PlayerPrefs.SetInt(NeighborSelectionKey, 0);
            PlayerPrefs.Save();
            Debug.Log("ROULETTE_NEIGHBOR_RESTORE_SAVE_PASSED: saved zero to isolated test key");
            Application.Quit();
        }

        private IEnumerator NeighborRestoreLoadSmoke()
        {
            yield return null;
            yield return null;
            if (selectedNeighbor != 0 || !neighborEnabledWithoutPrecheck)
                throw new InvalidOperationException(
                    "Startup did not restore the saved neighbor selector");
            RouletteProfile limited = RouletteProfile.Load("97");
            limited.MaximumTotalBet = 1;
            session = new RouletteSession(limited, 1000,
                new FixedPocketSmokeOutcome());
            RefreshPortraitUi();
            if (session.CanPlaceNeighbor(0) || !uiNeighborButton.interactable ||
                uiNeighbor.texture != neighborOn ||
                !uiNeighborSelection.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Saved neighbor selector was not enabled without prevalidation");
            PlayerPrefs.DeleteKey(NeighborSelectionKey);
            PlayerPrefs.Save();
            Debug.Log("ROULETTE_NEIGHBOR_RESTORE_LOAD_PASSED: saved zero enabled over wager limit");
            Application.Quit();
        }

        private IEnumerator SectorSmoke()
        {
            yield return null;
            yield return null;
            var buttons = new[] { uiOrphelinsButton, uiTiersButton, uiVoisinsButton };
            var sectors = new[] { RouletteSector.Orphelins, RouletteSector.Tiers,
                RouletteSector.Voisins };
            for (int i = 0; i < buttons.Length; ++i)
            {
                session.Clear();
                int[] spots = OriginalSectorBets.Spots(sectors[i]);
                if (i == 0)
                {
                    var press = new PointerEventData(EventSystem.current) {
                        button = PointerEventData.InputButton.Left };
                    ExecuteEvents.Execute(buttons[i].gameObject, press,
                        ExecuteEvents.pointerDownHandler);
                    if (session.TotalBet != spots.Length)
                        throw new InvalidOperationException(
                            "French bet did not start on pointer down");
                    ExecuteEvents.Execute(buttons[i].gameObject, press,
                        ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.Execute(buttons[i].gameObject, press,
                        ExecuteEvents.pointerClickHandler);
                }
                else ClickThroughUiRaycast(buttons[i]);
                if (session.TotalBet != spots.Length ||
                    session.Bets.Count != spots.Distinct().Count())
                    throw new InvalidOperationException("uGUI sector bet failed: " + sectors[i]);
                RefreshPortraitUi();
                yield return null;
                ClickThroughUiRaycast(uiUndoButton);
                if (session.TotalBet != 0 || session.LastUndoRemovalSoundCount != 0)
                    throw new InvalidOperationException("uGUI sector undo failed: " + sectors[i]);
            }
            session.Clear();
            Canvas.ForceUpdateCanvases();
            RectTransform zeroRect = (RectTransform)uiZeroBetButton.transform;
            Vector2 point = RectTransformUtility.WorldToScreenPoint(null,
                zeroRect.TransformPoint(zeroRect.rect.center));
            var eventData = new PointerEventData(EventSystem.current) {
                position = point, button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, hits);
            if (hits.Count == 0 || hits[0].gameObject != uiZeroBetButton.gameObject)
                throw new InvalidOperationException("Zero bet hover is covered by " +
                    (hits.Count == 0 ? "nothing" : hits[0].gameObject.name));
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                ExecuteEvents.pointerEnterHandler);
            if (selectedNeighbor != 0)
                throw new InvalidOperationException("Table zero did not select its neighbors");
            Rect zeroSpot = originalLayout.bet_spots[0].ToRect();
            Vector2 zeroMarkerPosition = new Vector2(
                zeroSpot.x + (zeroSpot.width - 90f) * .5f,
                -zeroSpot.y - (zeroSpot.height - 90f) * .5f);
            if (!uiBetMarker.gameObject.activeSelf || hoveredBetSpot != 0 ||
                uiBetMarker.texture != originalMainChipButtonFrames[0][0] ||
                Vector2.Distance(uiBetMarker.rectTransform.anchoredPosition,
                    zeroMarkerPosition) > .01f)
                throw new InvalidOperationException(
                    "Source BetMarker did not follow the hovered zero spot");
            int originalSpotLimit = session.Profile.MaximumSpotBet;
            session.Profile.MaximumSpotBet = 0;
            RefreshBetMarker();
            if (uiBetMarker.texture != originalBetMarkerRejectedFrames[0])
                throw new InvalidOperationException(
                    "Capped BetMarker did not show original X frame");
            session.Profile.MaximumSpotBet = originalSpotLimit;
            RefreshBetMarker();
            RectTransform oneRect = (RectTransform)uiBetButtons[1].transform;
            eventData.position = RectTransformUtility.WorldToScreenPoint(null,
                oneRect.TransformPoint(oneRect.rect.center));
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                ExecuteEvents.pointerExitHandler);
            if (selectedNeighbor != 0)
                throw new InvalidOperationException(
                    "Crossing between BetSpots cleared the source hover early");
            ExecuteEvents.Execute(uiBetButtons[1].gameObject, eventData,
                ExecuteEvents.pointerEnterHandler);
            if (selectedNeighbor != 1 || hoveredBetSpot != 1 ||
                !uiBetMarker.gameObject.activeSelf)
                throw new InvalidOperationException(
                    "Moving onto another number did not update neighbor/BetMarker");
            eventData.position = Vector2.zero;
            ExecuteEvents.Execute(uiBetButtons[1].gameObject, eventData,
                ExecuteEvents.pointerExitHandler);
            if (selectedNeighbor != 1 || !uiNeighborButton.interactable ||
                uiBetMarker.gameObject.activeSelf || hoveredBetSpot != -1 ||
                PlayerPrefs.GetInt(NeighborSelectionKey, -2) != 1)
                throw new InvalidOperationException(
                    "Leaving the table changed the retained source neighbor selection");
            eventData.position = point;
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                ExecuteEvents.pointerEnterHandler);
            if (selectedNeighbor != 0 || !uiNeighborButton.interactable)
                throw new InvalidOperationException(
                    "Returning to the table did not reselect the neighbor");
            yield return null;
            var neighborPress = new PointerEventData(EventSystem.current) {
                button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(uiNeighborButton.gameObject, neighborPress,
                ExecuteEvents.pointerDownHandler);
            if (session.TotalBet != 4 || session.Bets.ContainsKey(100))
                throw new InvalidOperationException(
                    "Selected neighbor button did not act on pointer down");
            ExecuteEvents.Execute(uiNeighborButton.gameObject, neighborPress,
                ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(uiNeighborButton.gameObject, neighborPress,
                ExecuteEvents.pointerClickHandler);
            if (session.TotalBet != 4)
                throw new InvalidOperationException("Neighbor release repeated the group bet");
            RefreshPortraitUi();
            yield return null;
            ClickThroughUiRaycast(uiUndoButton);
            if (session.LastUndoRemovalSoundCount != 0)
                throw new InvalidOperationException("Neighbor undo played direct-removal sound");
            ExecuteEvents.Execute(uiBetButtons[37].gameObject, eventData,
                ExecuteEvents.pointerEnterHandler);
            if (selectedNeighbor != -1 || uiNeighborButton.interactable)
                throw new InvalidOperationException(
                    "Non-number BetSpot did not clear source neighbor selection");
            RouletteProfile neighborLimit = RouletteProfile.Load("97");
            neighborLimit.MaximumTotalBet = 1;
            eventData.position = Vector2.zero;
            ExecuteEvents.Execute(uiBetButtons[37].gameObject, eventData,
                ExecuteEvents.pointerExitHandler);
            eventData.position = point;
            session = new RouletteSession(neighborLimit, 1000,
                new FixedPocketSmokeOutcome());
            selectedNeighbor = 0;
            RefreshPortraitUi();
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                ExecuteEvents.pointerEnterHandler);
            if (selectedNeighbor != -1 || uiNeighborButton.interactable ||
                failurePrompt != PromptKind.BetLimitReached)
                throw new InvalidOperationException(
                    "Invalid neighbor group did not clear selection and show source limit");
            foreach (BetRejectionReason reason in new[] {
                BetRejectionReason.InsufficientCredit,
                BetRejectionReason.SpotLimit })
            {
                RouletteProfile hoverProfile = RouletteProfile.Load("97");
                if (reason == BetRejectionReason.SpotLimit)
                    hoverProfile.MaximumSpotBet = 1;
                // A replacement session still needs an actual leave/reenter:
                // source hover validation only runs when the BetSpot changes.
                eventData.position = Vector2.zero;
                ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                    ExecuteEvents.pointerExitHandler);
                eventData.position = point;
                session = new RouletteSession(hoverProfile,
                    reason == BetRejectionReason.InsufficientCredit ? 1 : 1000,
                    new FixedPocketSmokeOutcome());
                if (reason == BetRejectionReason.SpotLimit)
                    session.PlaceBet(103, 1);
                selectedNeighbor = 0;
                ResetBetPromptAfterSuccess();
                RefreshPortraitUi();
                ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                    ExecuteEvents.pointerEnterHandler);
                RefreshPortraitUi();
                PromptKind expected = reason == BetRejectionReason.SpotLimit ?
                    PromptKind.BetSpotLimitReached : PromptKind.InsertCredits;
                bool shown = expected == PromptKind.InsertCredits ?
                    shownPrompt == expected : failurePrompt == expected;
                if (selectedNeighbor != -1 || uiNeighborButton.interactable ||
                    !shown || !promptRoot.gameObject.activeSelf ||
                    uiPrompt.texture != PromptFrames(expected, false)[0])
                    throw new InvalidOperationException(
                        "Neighbor hover rejected with wrong source prompt: " + reason);
            }
            eventData.position = Vector2.zero;
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, eventData,
                ExecuteEvents.pointerExitHandler);
            eventData.position = point;
            session = new RouletteSession(RouletteProfile.Load("97"), 1000,
                new FixedPocketSmokeOutcome());
            ResetBetPromptAfterSuccess();
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiZeroBetButton);
            if (session.TotalBet != 1 || !session.Bets.ContainsKey(100))
                throw new InvalidOperationException("Table zero no longer places a straight bet");
            session.Clear();
            RefreshPortraitUi();
            ToggleAuto();
            RefreshPortraitUi();
            if (!autoplay || uiZeroBetButton.interactable ||
                uiVoisinsButton.interactable || uiOrphelinsButton.interactable ||
                uiTiersButton.interactable || uiSpinButton.interactable ||
                !uiUndoButton.gameObject.activeSelf ||
                !uiClearButton.gameObject.activeSelf ||
                uiUndoButton.interactable || uiClearButton.interactable)
                throw new InvalidOperationException("AutoPlay idle table did not lock all uGUI input");
            long wagerBeforeAutoClick = session.TotalBet;
            ClickThroughUiRaycast(uiZeroBetButton);
            ClickThroughUiRaycast(uiOrphelinsButton);
            if (session.TotalBet != wagerBeforeAutoClick)
                throw new InvalidOperationException("AutoPlay idle table accepted a bet click");
            ToggleAuto();
            yield return new WaitForSecondsRealtime(.06f);
            RefreshPortraitUi();
            if (autoplay || !uiZeroBetButton.interactable ||
                !uiOrphelinsButton.interactable)
                throw new InvalidOperationException("Manual table did not reopen after AutoPlay");
            ClickThroughUiRaycast(uiZeroBetButton);
            if (session.TotalBet != 1)
                throw new InvalidOperationException("Manual table did not accept bet after AutoPlay");
            session.Clear();
            Debug.Log("ROULETTE_AUTOPLAY_TABLE_TOUCH_GATE_PASSED");
            Debug.Log("ROULETTE_SECTOR_UI_PASSED: three French bets, table hover, neighbor confirm, straight bet, undo");
            Debug.Log("ROULETTE_GROUP_UNDO_SOUND_PASSED: four grouped actions play undo without remove");
            captureAt = Time.realtimeSinceStartup + .5f;
            if (string.IsNullOrEmpty(capturePath)) Application.Quit();
        }

        private string proViewStateCaptureDirectory;

        private void CaptureProViewState(string name)
        {
            if (!string.IsNullOrEmpty(proViewStateCaptureDirectory))
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(
                    proViewStateCaptureDirectory, name + ".png"));
        }

        private void RequireProViewState(Button button, string name, int frame)
        {
            Texture2D expected = Resources.Load<Texture2D>(
                "Original/ProViewStates/" + name + "/frame_" + (frame + 1).ToString("D4"));
            if (expected == null || ((RawImage)button.targetGraphic).texture != expected)
                throw new InvalidOperationException(
                    "Original ProView state differs: " + name + " frame=" + frame);
        }

        private IEnumerator PaysSmoke()
        {
            string[] launch = Environment.GetCommandLineArgs();
            int capture = Array.IndexOf(launch, "-rouletteProViewStateCapture");
            if (capture >= 0 && capture + 1 < launch.Length)
            {
                proViewStateCaptureDirectory = launch[capture + 1];
                System.IO.Directory.CreateDirectory(proViewStateCaptureDirectory);
            }
            yield return null;
            yield return null;
            RequireProViewState(uiInfoButton, "GameInfo", 0);
            var infoFrames = uiInfoButton.GetComponent<RouletteOriginalButtonFrames>();
            infoFrames.OnPointerEnter(new PointerEventData(EventSystem.current));
            RequireProViewState(uiInfoButton, "GameInfo", 0);
            PointerEventData infoPointer = PressThroughUiRaycast(uiInfoButton);
            RequireProViewState(uiInfoButton, "GameInfo", 1);
            ReleaseUiPointer(uiInfoButton, infoPointer);
            RequireProViewState(uiInfoButton, "GameInfo", 0);
            // Exercise the generic original Button's selected and disabled
            // states, then restore the real INFO options flow.
            infoFrames.SetSelected(true);
            RequireProViewState(uiInfoButton, "GameInfo", 2);
            CaptureProViewState("info_selected");
            infoFrames.SetSelected(false);
            uiInfoButton.interactable = false;
            infoFrames.Refresh();
            RequireProViewState(uiInfoButton, "GameInfo", 3);
            CaptureProViewState("info_disabled");
            uiInfoButton.interactable = true;
            infoFrames.Refresh();
            CloseGameInfoOptions();
            Debug.Log("ROULETTE_PAYS_SMOKE: INFO");
            ClickThroughUiRaycast(uiInfoButton);
            if (!gameInfoOptionsOpen || paysPage >= 0 ||
                gameInfoOptionsDeadlineTimestamp <=
                    System.Diagnostics.Stopwatch.GetTimestamp() ||
                footerInfoTop == null || footerInfoBottom == null ||
                footerSeePaysButton == null || footerVolumeButton == null)
                throw new InvalidOperationException("INFO did not open original options");
            if (click == null)
                throw new InvalidOperationException("Original Pays click WAV is missing");
            yield return null;
            yield return new WaitForEndOfFrame();
            float priorVolume = AudioListener.volume;
            int priorVolumeValue = pcPlayerVolumeValue;
            for (int i = 0; i < 3; ++i)
            {
                int expectedVolumeValue = pcPlayerVolumeValue == 0 ? 5 :
                    pcPlayerVolumeValue == 5 ? 10 : 0;
                PointerEventData volumePointer =
                    PressThroughUiRaycast(uiGameInfoVolume);
                if (pcPlayerVolumeValue != expectedVolumeValue ||
                    PlayerPrefs.GetInt(pcPlayerVolumeKey, -1) != expectedVolumeValue ||
                    ((RawImage)uiGameInfoVolume.targetGraphic).texture !=
                        proViewVolumeFrames[expectedVolumeValue / 5])
                    throw new InvalidOperationException(
                        "Player volume did not advance through 0/5/10");
                ReleaseUiPointer(uiGameInfoVolume, volumePointer);
                CaptureProViewState("volume_" + expectedVolumeValue);
            }
            if (pcPlayerVolumeValue != priorVolumeValue ||
                (priorVolumeValue == 0 && AudioListener.volume >= 1f) ||
                (priorVolumeValue != 0 && AudioListener.volume != 1f))
                throw new InvalidOperationException(
                    "Player volume did not restore source-relative gain");
            AudioListener.volume = priorVolume;
            PointerEventData seePaysPointer = PressThroughUiRaycast(uiGameInfoSeePays);
            if (paysPage != 0 || gameInfoOptionsOpen)
                throw new InvalidOperationException("SEE PAYS did not open first page");
            RequireProViewState(uiGameInfoSeePays, "SeePays", 1);
            ReleaseUiPointer(uiGameInfoSeePays, seePaysPointer);
            yield return null;
            yield return new WaitForEndOfFrame();
            RequirePaysNativeSize(0, 1054);
            RequireProViewState(uiPaysBack, "Previous", 3);
            float firstPaysFlashDeadline = Time.realtimeSinceStartup + 1f;
            while (pcPaysFlash != null && pcPaysFlash.CallbackCount == 0 &&
                Time.realtimeSinceStartup < firstPaysFlashDeadline) yield return null;
            if (pcPaysFlash == null || pcPaysFlash.CallbackCount == 0)
                throw new InvalidOperationException("Pays did not receive its first 16ms flash Trigger");
            RequireProViewState(uiPaysNext, "Next", pcPaysFlash.NextSelected ? 2 : 0);
            CaptureProViewState("pays_first");
            if (uiTouch.Visible)
                throw new InvalidOperationException(
                    "Denomination touch label remained visible on Pays");
            Debug.Log("ROULETTE_PAYS_SMOKE: NEXT");
            int clicksBeforeNext = paysClickCount;
            PointerEventData nextPointer = PressThroughUiRaycast(uiPaysNext);
            if (paysPage != 1 || paysClickCount != clicksBeforeNext + 1)
                throw new InvalidOperationException("NEXT did not act on touch-down");
            RequireProViewState(uiPaysNext, "Next", 1);
            CaptureProViewState("next_pressed");
            ReleaseUiPointer(uiPaysNext, nextPointer);
            if (paysPage != 1 || paysClickCount != clicksBeforeNext + 1)
                throw new InvalidOperationException("NEXT repeated on touch release");
            yield return null;
            yield return new WaitForEndOfFrame();
            RequirePaysNativeSize(1, 1053);
            nextPointer = PressThroughUiRaycast(uiPaysNext);
            if (paysPage != 2)
                throw new InvalidOperationException("NEXT did not reach pays page 3 on down");
            yield return null;
            yield return new WaitForEndOfFrame();
            RequirePaysNativeSize(2, 1053);
            ExecuteEvents.Execute(uiPaysNext.gameObject, nextPointer,
                ExecuteEvents.initializePotentialDrag);
            if (nextPointer.useDragThreshold)
                throw new InvalidOperationException(
                    "Pays navigation still applies Unity's movement threshold");
            nextPointer.delta = new Vector2(4, 0);
            ExecuteEvents.Execute(uiPaysNext.gameObject, nextPointer,
                ExecuteEvents.dragHandler);
            if (paysPage != 3 || paysClickCount != clicksBeforeNext + 3)
                throw new InvalidOperationException("NEXT ignored captured touch movement");
            ReleaseUiPointer(uiPaysNext, nextPointer);
            if (paysPage != 3 || paysClickCount != clicksBeforeNext + 3)
                throw new InvalidOperationException("NEXT repeated on release after drag");
            yield return null;
            yield return new WaitForEndOfFrame();
            RequirePaysNativeSize(3, 1054);
            if (uiPaysNext.interactable)
                throw new InvalidOperationException("NEXT stayed enabled on last Pays page");
            RequireProViewState(uiPaysNext, "Next", 3);
            CaptureProViewState("pays_last");
            Debug.Log("ROULETTE_PAYS_SMOKE: BACK");
            PointerEventData backPointer = PressThroughUiRaycast(uiPaysBack);
            if (paysPage != 2 || paysClickCount != clicksBeforeNext + 4)
                throw new InvalidOperationException("BACK did not act on touch-down");
            RequireProViewState(uiPaysBack, "Previous", 1);
            ReleaseUiPointer(uiPaysBack, backPointer);
            if (paysPage != 2 || paysClickCount != clicksBeforeNext + 4)
                throw new InvalidOperationException("BACK repeated on release");
            yield return null;
            Debug.Log("ROULETTE_PAYS_SMOKE: EXIT");
            PointerEventData exitPointer = PressThroughUiRaycast(uiPaysExit);
            if (paysPage != 2 || !PcPaysReturnPending || paysClickCount != clicksBeforeNext + 5)
                throw new InvalidOperationException("EXIT did not queue its source return Trigger on touch-down");
            RequireProViewState(uiPaysExit, "Exit", 1);
            ReleaseUiPointer(uiPaysExit, exitPointer);
            if (!PcPaysReturnPending || paysClickCount != clicksBeforeNext + 5)
                throw new InvalidOperationException("EXIT repeated on release");
            if (paysTimeoutDeadlineTimestamp != 0)
                throw new InvalidOperationException("EXIT retained Pays timeout");
            float paysExitDeadline = Time.realtimeSinceStartup + 1f;
            while (paysPage >= 0 && Time.realtimeSinceStartup < paysExitDeadline) yield return null;
            if (paysPage >= 0 || pcPaysFlash != null)
                throw new InvalidOperationException("EXIT did not close through its source timer callback");
            yield return new WaitForEndOfFrame();
            if (!uiTouch.Visible)
                throw new InvalidOperationException(
                    "Denomination touch label did not return after Pays");
            ClickThroughUiRaycast(uiInfoButton);
            if (!gameInfoOptionsOpen || paysPage >= 0)
                throw new InvalidOperationException("INFO did not reopen options");
            gameInfoOptionsDeadlineTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            TickGameInfoOptions(System.Diagnostics.Stopwatch.GetTimestamp());
            if (gameInfoOptionsOpen || paysPage >= 0)
                throw new InvalidOperationException("INFO options did not time out");
            ClickThroughUiRaycast(uiInfoButton);
            if (!gameInfoOptionsOpen || gameInfoOptionsDeadlineTimestamp <=
                System.Diagnostics.Stopwatch.GetTimestamp())
                throw new InvalidOperationException("INFO did not restart options timer");
            yield return null;
            yield return new WaitForEndOfFrame();
            var secondSeePaysPointer = new PointerEventData(EventSystem.current) {
                button = PointerEventData.InputButton.Left };
            ExecuteEvents.Execute(uiGameInfoSeePays.gameObject,
                secondSeePaysPointer, ExecuteEvents.initializePotentialDrag);
            if (secondSeePaysPointer.useDragThreshold)
                throw new InvalidOperationException(
                    "SEE PAYS still applies Unity's movement threshold");
            secondSeePaysPointer.delta = new Vector2(4, 0);
            ExecuteEvents.Execute(uiGameInfoSeePays.gameObject,
                secondSeePaysPointer, ExecuteEvents.dragHandler);
            ReleaseUiPointer(uiGameInfoSeePays, secondSeePaysPointer);
            if (paysPage != 0 || paysTimeoutDeadlineTimestamp <=
                System.Diagnostics.Stopwatch.GetTimestamp())
                throw new InvalidOperationException(
                    "SEE PAYS movement did not arm Pays timeout");
            int clicksBeforeTimeout = paysClickCount;
            paysTimeoutDeadlineTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            TickPaysTimeout(System.Diagnostics.Stopwatch.GetTimestamp());
            if (paysPage != 0 || !PcPaysReturnPending || paysTimeoutDeadlineTimestamp != 0 ||
                paysClickCount != clicksBeforeTimeout + 1)
                throw new InvalidOperationException("Pays timeout did not request its source return Trigger");
            paysExitDeadline = Time.realtimeSinceStartup + 1f;
            while (paysPage >= 0 && Time.realtimeSinceStartup < paysExitDeadline) yield return null;
            if (paysPage >= 0 || pcPaysFlash != null)
                throw new InvalidOperationException("Pays timeout return Trigger did not destroy the view");
            Debug.Log("ROULETTE_PAYS_FLOW_PASSED");
            Debug.Log("ROULETTE_PROVIEW_STATE_FRAMES_PASSED: volume 0/5/10, Button 0/1/2/3, Pays navigation");
            if (string.IsNullOrEmpty(capturePath)) Application.Quit();
        }

        private void RequirePaysNativeSize(int page, int height)
        {
            if (paysPage != page || uiPaysImage.texture != paysPages[page] ||
                uiPaysImage.rectTransform.sizeDelta != new Vector2(1080, height))
                throw new InvalidOperationException(
                    "Original pays page size differs at page " + (page + 1) +
                    ": active=" + paysPage + " texture=" +
                    (uiPaysImage.texture == null ? "null" : uiPaysImage.texture.name) +
                    " size=" + uiPaysImage.rectTransform.sizeDelta);
        }

        private static PointerEventData PressThroughUiRaycast(Button button,
            Vector2? localPoint = null)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null,
                button.transform.TransformPoint(localPoint ??
                    ((RectTransform)button.transform).rect.center));
            var eventData = new PointerEventData(EventSystem.current) {
                position = screenPoint, button = PointerEventData.InputButton.Left };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, hits);
            bool confirmationArea = hits.Count > 0 &&
                hits[0].gameObject.name == "Confirmation touch area" &&
                hits[0].gameObject.transform.parent == button.transform;
            if (hits.Count == 0 ||
                hits[0].gameObject != button.gameObject && !confirmationArea)
                throw new InvalidOperationException("uGUI hit target is covered: " + button.name +
                    " by " + (hits.Count == 0 ? "nothing" : hits[0].gameObject.name) +
                    " at " + screenPoint + " screen " + Screen.width + "x" + Screen.height +
                    " active=" + button.gameObject.activeInHierarchy +
                    " canvas=" + button.GetComponentInParent<Canvas>().enabled);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, eventData,
                ExecuteEvents.pointerDownHandler);
            return eventData;
        }

        private static void ReleaseUiPointer(Button button,
            PointerEventData eventData)
        {
            ExecuteEvents.Execute(button.gameObject, eventData,
                ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(button.gameObject, eventData,
                ExecuteEvents.pointerClickHandler);
        }

        private static void ClickThroughUiRaycast(Button button,
            Vector2? localPoint = null)
        {
            ReleaseUiPointer(button, PressThroughUiRaycast(button, localPoint));
        }

        private IEnumerator MainPlayDownSmoke()
        {
            yield return null;
            if (portraitCanvas == null || uiSpinButton == null)
                throw new InvalidOperationException("Main PLAY control is missing");
            Canvas.ForceUpdateCanvases();
            Vector2 featureSlot = RectTransformUtility.WorldToScreenPoint(null,
                portraitRoot.TransformPoint(new Vector3(753f, -1853f, 0f)));
            var slotPointer = new PointerEventData(EventSystem.current) {
                position = featureSlot,
                button = PointerEventData.InputButton.Left };
            var slotHits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(slotPointer, slotHits);
            if (slotHits.Any(hit =>
                hit.gameObject.GetComponentInParent<Button>() != null))
                throw new InvalidOperationException(
                    "PC-only transparent footer Double Up button remains");
            outcome = new FixedPocketSmokeOutcome(16);
            session = new RouletteSession(session.Profile, 1000, outcome);
            RefreshPortraitUi();
            if (uiSpinButton.interactable)
                throw new InvalidOperationException("Empty table enabled PLAY");
            PointerEventData emptyPress = PressThroughUiRaycast(uiSpinButton);
            if (spinning || session.SpinPending)
                throw new InvalidOperationException("Disabled PLAY started a spin");
            ReleaseUiPointer(uiSpinButton, emptyPress);
            session.PlaceBet(100, 1);
            RefreshPortraitUi();
            if (!uiSpinButton.interactable)
                throw new InvalidOperationException("Valid wager did not enable PLAY");
            if (uiPromptButton.GetComponent<RawImage>().raycastTarget)
                throw new InvalidOperationException(
                    "Animated Press Spin message intercepted touches");
            uiPromptButton.onClick.Invoke();
            if (spinning || session.SpinPending)
                throw new InvalidOperationException(
                    "Animated Press Spin message remained an extra PLAY action");
            PointerEventData playPress = PressThroughUiRaycast(uiSpinButton);
            if (!spinning || !session.SpinPending ||
                session.PendingPocket != 16 || session.Credit != 999)
                throw new InvalidOperationException(
                    "Main PLAY waited for release or failed to commit once");
            long committedCredit = session.Credit;
            ReleaseUiPointer(uiSpinButton, playPress);
            if (!spinning || !session.SpinPending ||
                session.Credit != committedCredit)
                throw new InvalidOperationException("Main PLAY repeated on release");
            Debug.Log("ROULETTE_MAIN_PLAY_DOWN_PASSED: disabled ignored, committed pocket 16 before release, no release repeat");
            Application.Quit();
        }

        private IEnumerator MainPlayMoveSmoke()
        {
            yield return null;
            if (portraitCanvas == null || uiSpinButton == null ||
                uiSpinButton.GetComponent<RouletteCapturedMainTouchAction>() == null)
                throw new InvalidOperationException("Captured main PLAY handler is missing");
            outcome = new FixedPocketSmokeOutcome(16);
            session = new RouletteSession(session.Profile, 1000, outcome);
            RefreshPortraitUi();
            PointerEventData held = PressThroughUiRaycast(uiSpinButton);
            if (spinning || session.SpinPending)
                throw new InvalidOperationException("Disabled PLAY press started a spin");
            // Keep the same captured pointer while a wager enables PLAY.
            session.PlaceBet(100, 1);
            RefreshPortraitUi();
            ExecuteEvents.Execute(uiSpinButton.gameObject, held,
                ExecuteEvents.dragHandler);
            if (!spinning || !session.SpinPending ||
                session.PendingPocket != 16 || session.Credit != 999)
                throw new InvalidOperationException(
                    "Captured PLAY movement did not commit the wager");
            ExecuteEvents.Execute(uiSpinButton.gameObject, held,
                ExecuteEvents.dragHandler);
            ReleaseUiPointer(uiSpinButton, held);
            if (session.Credit != 999 || !session.SpinPending)
                throw new InvalidOperationException("PLAY movement repeated the debit");
            Debug.Log("ROULETTE_MAIN_PLAY_MOVE_PASSED: captured move committed pocket 16 once");
            Application.Quit();
        }

        private IEnumerator ProViewVolumeMoveSmoke()
        {
            yield return null;
            if (portraitCanvas == null || uiInfoButton == null ||
                uiGameInfoVolume == null ||
                uiGameInfoVolume.GetComponent<RouletteCapturedMainTouchAction>() == null)
                throw new InvalidOperationException("Captured ProView volume handler is missing");
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiInfoButton);
            RefreshPortraitUi();
            if (!gameInfoOptionsOpen)
                throw new InvalidOperationException("INFO release did not open options");
            yield return null;
            yield return new WaitForEndOfFrame();
            int before = pcPlayerVolumeValue;
            int afterDown = before == 0 ? 5 : before == 5 ? 10 : 0;
            int afterMove = afterDown == 0 ? 5 : afterDown == 5 ? 10 : 0;
            PointerEventData held = PressThroughUiRaycast(uiGameInfoVolume);
            if (pcPlayerVolumeValue != afterDown)
                throw new InvalidOperationException("ProView volume press missed its first frame");
            ExecuteEvents.Execute(uiGameInfoVolume.gameObject, held,
                ExecuteEvents.dragHandler);
            if (pcPlayerVolumeValue != afterMove ||
                PlayerPrefs.GetInt(pcPlayerVolumeKey, -1) != afterMove)
                throw new InvalidOperationException(
                    "Captured ProView volume movement did not persist the next frame");
            ReleaseUiPointer(uiGameInfoVolume, held);
            if (pcPlayerVolumeValue != afterMove)
                throw new InvalidOperationException("ProView volume changed again on release");
            Debug.Log("ROULETTE_PROVIEW_VOLUME_MOVE_PASSED: down=" + afterDown +
                      " move=" + afterMove + " release unchanged");
            Application.Quit();
        }

        private IEnumerator PcCashMeterSaveSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcCashMeterKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                session == null || session.Credit != 1000 || pcCashMeterMode ||
                !pcCashMeterJurisdictionEnabled ||
                portraitCanvas == null || footerCreditMeterButton == null ||
                footerCashMeterLabel == null || footerCashCreditFont == null)
                throw new InvalidOperationException(
                    "Cash meter smoke needs a fresh isolated PC session and source art");
            var local = session as RouletteSession;
            if (local == null)
                throw new InvalidOperationException("PC cash meter needs a local session");
            local.ApplyPlatformMoneyState(40, 25, 1);
            RefreshPortraitUi();
            if (uiCredit.Value != "40" || !uiCashCredit.Visible ||
                uiCashCredit.Value != "RM10.00" ||
                uiCashBet.Value != "RM0.00" || uiCashPaid.Visible ||
                footerCashBetMeterLabel == null || footerCashPaidMeterLabel == null ||
                footerCashPaidFont == null ||
                footerCreditMeterButton.width != 197 ||
                footerCreditMeterButton.height != 66)
                throw new InvalidOperationException("Original credit meter initial view differs");
            local.PlaceBet(100, 1);
            RefreshPortraitUi();
            if (uiCashBet.Value != "RM0.25" || uiBet.Value != "1")
                throw new InvalidOperationException("Original cash bet meter did not follow denomination");
            local.Clear();
            RefreshPortraitUi();
            ClickThroughUiRaycast(uiCreditMeterButton);
            if (!pcCashMeterMode || PlayerPrefs.GetInt(pcCashMeterKey, -1) != 1 ||
                uiCashCredit.Value != "RM10.00" || !uiCashCredit.Visible ||
                !uiCreditTitle.Visible || uiCredit.Value != "40" ||
                !uiCashTitle.gameObject.activeSelf ||
                local.Credit != 40 || local.MoneyRemainderCents != 1 ||
                local.MoneyCents != 1001)
                throw new InvalidOperationException(
                    "Credit touch lost cash conversion, mode, or ledger remainder");
            Debug.Log("ROULETTE_PC_CASH_METER_SAVE_PASSED: 40 credits and RM10.00 stay visible across touch; 1 cent remains in ledger");
            Application.Quit();
        }

        private IEnumerator PcCashMeterLoadSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcCashMeterKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                !pcCashMeterMode || !pcCashMeterJurisdictionEnabled ||
                PlayerPrefs.GetInt(pcCashMeterKey, -1) != 1 ||
                portraitCanvas == null || !uiCashCredit.Visible ||
                !uiCreditTitle.Visible || uiCredit.Value != "1000" ||
                session == null || session.Credit != 1000)
                throw new InvalidOperationException("Cash meter mode did not survive PC restart");
            ClickThroughUiRaycast(uiCreditMeterButton);
            if (pcCashMeterMode || PlayerPrefs.GetInt(pcCashMeterKey, -1) != 0 ||
                !uiCreditTitle.Visible || !uiCashCredit.Visible ||
                !uiCashTitle.gameObject.activeSelf ||
                uiCashCredit.Value != "RM10.00" ||
                uiCredit.Value != "1000" || session.Credit != 1000)
                throw new InvalidOperationException("Second meter touch did not restore credits");
            Debug.Log("ROULETTE_PC_CASH_METER_LOAD_PASSED: saved mode restored and toggled back; both meters remain visible");
            Application.Quit();
        }

        private IEnumerator PcDemoCashMeterSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcCashMeterKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                demoShowMode != 1 || pcCashMeterMode ||
                !pcCashMeterJurisdictionEnabled || session == null ||
                session.Credit != 1000)
                throw new InvalidOperationException("Demo meter needs isolated mode-1 keys");
            ClickThroughUiRaycast(uiCreditMeterButton);
            if (!pcCashMeterMode || session.Credit != 3000 ||
                uiCashCredit.Value != "RM30.00" ||
                PlayerPrefs.GetInt(pcCashMeterKey, -1) != 1 ||
                ReadPcCreditEvent().after != 3000)
                throw new InvalidOperationException(
                    "Demo meter touch did not grant 2000 source credits and show cash");
            Debug.Log("ROULETTE_PC_DEMO_CASH_METER_PASSED: touch grants 2000 demo credits and shows RM30.00");
            Application.Quit();
        }

        private IEnumerator PcCashMeterPackagedDefaultSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcCashMeterKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcCashMeterJurisdictionEnabled || pcCashMeterMode ||
                session == null || session.Credit != 1000)
                throw new InvalidOperationException("Cash default needs fresh packaged-config keys");
            ClickThroughUiRaycast(uiCreditMeterButton);
            if (!pcCashMeterMode || PlayerPrefs.GetInt(pcCashMeterKey, -1) != 1 ||
                uiCashCredit.Visible || uiCashTitle.gameObject.activeSelf ||
                !uiCreditTitle.Visible || uiCredit.Value != "1000" ||
                session.Credit != 1000)
                throw new InvalidOperationException(
                    "Packaged cash-disabled jurisdiction showed a cash label");
            Debug.Log("ROULETTE_PC_CASH_METER_DEFAULT_PASSED: packaged jurisdiction hides cash while preserving credits and mode");
            Application.Quit();
        }
    }
}
