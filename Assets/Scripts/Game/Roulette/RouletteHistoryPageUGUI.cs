using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private bool historyView;
        private int historyPage;
        private int historyShownVersion = -1;
        private int historyShownPage = -1;
        private RectTransform historyRoot, historyChipLayer, historyWinLayer,
            historyNumbersRoot;
        private List<WinningPayRecord> historyWinningPays =
            new List<WinningPayRecord>();
        private RouletteProfile historyResultProfile;
        private long historyWinCycleStartedTimestamp;
        private int historyVisiblePayIndex = -2;
        private long historyResultTotal;
        private RawImage historyWheel, historySpindle, historyBall, historyResultOval;
        private RawImage historyTableNumberMark;
        private int historyMarkedPocket = -1;
        private long historyTableMarkStartedTimestamp;
        private RawImage historyTotalWinTitle;
        private PictureLabel historyResultNumber, historyToteNumber;
        private RawImage historyToteMeterBox;
        private PictureLabel historyTotalWinAmount;
        private PictureLabel historyCredit, historyBet;
        private PictureLabel historyPageNumber, historyInfo, historyPaid;
        private PictureLabel historyCreditTitle, historyBetTitle, historyWinTitle;
        private RawImage historyCashCreditBackground, historyCashPaidBackground;
        private PictureLabel historyCashCredit, historyCashBet, historyCashPaid;
        private OriginalBitmapFont historyMeterFont;
        private PictureLabel historyDenom, historyLinePays, historyGamePays;
        private Button historyPrevious, historyNext, historyGame;

        private void CreateHistoryPageUi()
        {
            historyMeterFont = OriginalBitmapFont.Load("history_meter");
            if (historyMeterFont == null)
                throw new InvalidOperationException("Original history font missing");
            historyRoot = UiRect(portraitRoot, new Rect(0, 0, 1080, 1920),
                "Original Screen 1 history");
            RawImage background = UiImage(historyRoot, new Rect(0, 0, 1080, 1920),
                originalBackground, "History table background");
            background.raycastTarget = true;
            Rect wheelBounds = originalLayout.wheel_main.ToRect();
            historyWheel = UiImage(historyRoot, wheelBounds, wheel, "Historical wheel");
            SetSourceRotationPivot(historyWheel.rectTransform, wheelBounds);
            UiImage(historyRoot, new Rect(200, 112, 544, 543), wheelShine,
                "Historical wheel shine");
            historyBall = UiImage(historyRoot, originalLayout.wheel_ball.ToRect(), ball,
                "Historical ball");
            Rect spindleBounds = originalLayout.wheel_spindle.ToRect();
            historySpindle = UiImage(historyRoot, spindleBounds, spindle,
                "Historical spindle");
            SetSourceRotationPivot(historySpindle.rectTransform, spindleBounds);
            UiImage(historyRoot, new Rect(145, 41, 657, 231), wheelShadow,
                "Historical wheel shadow");
            historyResultOval = UiImage(historyRoot, new Rect(786, 811, 240, 112),
                resultSpotBlack, "Historical winning number oval");
            historyResultNumber = new PictureLabel(historyRoot,
                new Rect(848, 820, 116, 82), toteWhiteFont,
                "Historical winning number", nativeSize: true);
            historyToteMeterBox = UiImage(historyRoot, new Rect(914, 34, 114, 79),
                toteMeterBoxFrames[0], "Historical latest tote box");
            historyToteNumber = new PictureLabel(historyRoot,
                new Rect(918, 38, 106, 70), toteWhiteFont,
                "Historical latest tote number", nativeSize: true);
            historyNumbersRoot = UiRect(historyRoot, new Rect(0, 0, 1080, 1920),
                "Historical previous numbers");
            historyTableNumberMark = UiImage(historyRoot,
                new Rect(0, 0, tableWinNonZeroFrames[0].width,
                    tableWinNonZeroFrames[0].height),
                tableWinNonZeroFrames[0], "Historical table number mark");
            historyTableNumberMark.gameObject.SetActive(false);
            historyChipLayer = UiRect(historyRoot, new Rect(0, 0, 1080, 1920),
                "Restored source table bets");
            historyWinLayer = UiRect(historyRoot, new Rect(0, 0, 1080, 1920),
                "Historical cycled bet pays");
            // CreateCycleResults receives the history Screen 1 sprite map.
            // ShowWin uses the same source total-win image and text as play.
            historyTotalWinTitle = UiImage(historyRoot,
                new Rect(378, 618, 185, 38),
                Resources.Load<Texture2D>("Art/total_win_title"),
                "Historical TOTAL WIN");
            historyTotalWinAmount = new PictureLabel(historyRoot,
                new Rect(0, 656, 941, 139),
                OriginalBitmapFont.Load("win_total"),
                "Historical total-win amount", nativeSize: true, verticalAlign: -1);
            historyTotalWinTitle.gameObject.SetActive(false);

            // HistoryDisableShrink=1 selects the DS type in the original
            // FileDefs button and meter tables, located above the wheel.
            UiImage(historyRoot, new Rect(176, 5, 120, 15),
                Resources.Load<Texture2D>("Original/History/DSInfo"),
                "Original history information meter");
            UiImage(historyRoot, new Rect(176, 5, 120, 15),
                Resources.Load<Texture2D>("Original/History/TextDSInfo"),
                "Original history information text ground");
            historyInfo = new PictureLabel(historyRoot,
                new Rect(176, 5, 120, 15), historyMeterFont,
                nativeSize: true);
            RawImage pageGround = UiImage(historyRoot, new Rect(176, 23, 120, 15),
                Resources.Load<Texture2D>("Original/History/DSPageNum"),
                "Original history page meter");
            pageGround.color = new Color32(20, 20, 20, 255);
            historyPageNumber = new PictureLabel(historyRoot,
                new Rect(176, 23, 120, 15), historyMeterFont,
                nativeSize: true);
            historyPrevious = UiButton(historyRoot, new Rect(315, 6, 120, 48),
                () => { },
                Resources.Load<Texture2D>("Original/History/DSPrevious_1"));
            historyPrevious.gameObject.name = "History previous";
            BindHistoryButtonFrames(historyPrevious, "DSPrevious");
            historyPrevious.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(historyPrevious, () => ShowHistoryPage(historyPage + 1));
            BindHistoryButtonLabel(historyPrevious, "Back");
            historyNext = UiButton(historyRoot, new Rect(443, 6, 120, 48),
                () => { },
                Resources.Load<Texture2D>("Original/History/DSNext_1"));
            historyNext.gameObject.name = "History next";
            BindHistoryButtonFrames(historyNext, "DSNext");
            historyNext.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(historyNext, () => ShowHistoryPage(historyPage - 1));
            BindHistoryButtonLabel(historyNext, "Next");
            historyGame = UiButton(historyRoot, new Rect(177, 40, 120, 28),
                () => { },
                Resources.Load<Texture2D>("Original/History/DSGame_1"));
            historyGame.gameObject.name = "Return to game";
            BindHistoryButtonFrames(historyGame, "DSGame");
            historyGame.gameObject.AddComponent<RoulettePointerDownAction>()
                .Configure(historyGame, () => SetHistoryView(false));
            BindHistoryButtonLabel(historyGame, "Return");

            UiImage(historyRoot, new Rect(0, 1740, 1080, 180), footerPanel,
                "Historical ProView panel");
            UiImage(historyRoot, new Rect(95, 1780, 888, 28),
                Resources.Load<Texture2D>("Original/ProViewHistory_PanelBG"),
                "Original history ProView strip");
            historyLinePays = ProViewLabel(historyRoot,
                new Rect(108, 1774, 200, 28), footerMessageFont,
                "History line pays", horizontalAlign: -1);
            historyGamePays = ProViewLabel(historyRoot,
                new Rect(770, 1774, 200, 28), footerMessageFont,
                "History game pays", horizontalAlign: 1);
            ProViewLabel(historyRoot, new Rect(43, 1815, 77, 13),
                footerOneCreditFonts, "One credit title").Set("1 CREDIT");
            historyCreditTitle = ProViewLabel(historyRoot, new Rect(337, 1815, 88, 15),
                footerCreditTitleFonts, "History credit title", -1);
            historyCreditTitle.Set("CREDITS");
            historyCashCreditBackground = UiImage(historyRoot,
                new Rect(440, 1815, 92, 19), footerCashMeterLabel,
                "History ProView_CashMeterLabel");
            historyCashCreditBackground.raycastTarget = false;
            historyCashCredit = ProViewLabel(historyRoot,
                new Rect(440, 1815, 92, 19), footerCashCreditFont,
                "History cash credit meter");
            historyBetTitle = ProViewLabel(historyRoot, new Rect(549, 1815, 72, 15),
                footerCreditTitleFonts, "History bet title", -1);
            historyBetTitle.Set("BET");
            RawImage cashBetBackground = UiImage(historyRoot,
                new Rect(631, 1815, 87, 19), footerCashBetMeterLabel,
                "History ProView_CashBetMeterLabel");
            cashBetBackground.raycastTarget = false;
            historyCashBet = ProViewLabel(historyRoot,
                new Rect(631, 1815, 87, 19), footerCashCreditFont,
                "History cash bet meter");
            historyWinTitle = ProViewLabel(historyRoot, new Rect(732, 1815, 88, 15),
                footerWinTitleFonts, "History win title", -1);
            historyWinTitle.Set("WIN");
            historyCashPaidBackground = UiImage(historyRoot,
                new Rect(830, 1815, 204, 19), footerCashPaidMeterLabel,
                "History ProView_CashPaidMeterLabel");
            historyCashPaidBackground.raycastTarget = false;
            historyCashPaid = ProViewLabel(historyRoot,
                new Rect(830, 1815, 204, 19), footerCashPaidFont,
                "History cash paid meter");
            historyDenom = ProViewLabel(historyRoot,
                new Rect(35, 1828, 90, 59), footerDenomFonts, "History denomination");
            historyCredit = ProViewLabel(historyRoot,
                new Rect(350, 1832, 188, 45), footerCreditMeterFonts, "History credit digits");
            historyBet = ProViewLabel(historyRoot,
                new Rect(569, 1832, 149, 45), footerCreditMeterFonts, "History bet digits");
            RawImage paidGround = UiImage(historyRoot, new Rect(782, 1833, 252, 54),
                Resources.Load<Texture2D>("Original/ProView_PaidMeterLabel"),
                "Original Paid meter ground");
            paidGround.color = new Color32(20, 20, 20, 255);
            historyPaid = ProViewLabel(historyRoot,
                new Rect(782, 1833, 252, 54),
                footerPaidMeterFonts, "History paid digits");
            historyRoot.gameObject.SetActive(false);
        }

        private static void BindHistoryButtonFrames(Button button, string name)
        {
            var states = new Texture2D[4];
            for (int i = 0; i < states.Length; ++i)
                states[i] = Resources.Load<Texture2D>(
                    "Original/History/" + name + "_" + (i + 1));
            button.gameObject.AddComponent<RouletteOriginalButtonFrames>()
                // Game::CreateHistoryButtons constructs Button, whose
                // UpdateImage maps depressed=1, selected=2, disabled=3.
                .Configure(button, states, showHoverFrame: false,
                    pressFrameIndex: 1, selectedFrameIndex: 2);
        }

        private void BindHistoryButtonLabel(Button button, string text)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            // The source Button centers ScaledLabel inside its image bounds.
            var label = new PictureLabel(buttonRect,
                new Rect(0, 0, buttonRect.rect.width, buttonRect.rect.height),
                historyMeterFont, nativeSize: true, raycastTarget: false);
            label.Set(text);
            button.GetComponent<RouletteOriginalButtonFrames>()
                .BindDepressedLabel(label.Root);
        }

        private void SetHistoryView(bool enabled)
        {
            if (!enabled) SetDoubleHistoryView(false);
            historyView = enabled && historyRoot != null;
            if (historyView)
            {
                historyPage = 0;
                ideckPreview = false;
                topperPreview = false;
                ClosePays();
            }
            // The original cabinet displays the histogram on Screen 4 while
            // the table history and navigation remain on Screen 1.
            histogramPreview = historyView && topperDedicatedDisplay;
            histogramPage = historyView ? HistoryRecordIndex(historyPage) : 0;
            RefreshTopperTarget();
            historyShownPage = -1;
        }

        private void ShowHistoryPage(int page)
        {
            if (!historyView) return;
            SetDoubleHistoryView(false);
            historyPage = Mathf.Clamp(page, 0,
                Mathf.Max(0, CommittedHistoryCount() - 1));
            histogramPage = HistoryRecordIndex(historyPage);
        }

        private void RefreshHistoryTableNumberMark(int pocket, bool newPage)
        {
            if (pocket < 0 || pocket > 36)
            {
                historyTableNumberMark.gameObject.SetActive(false);
                historyMarkedPocket = -1;
                historyTableMarkStartedTimestamp = 0;
                return;
            }
            Texture2D[] frames = pocket == 0 ? tableWinZeroFrames :
                tableWinNonZeroFrames;
            if (newPage || historyMarkedPocket != pocket ||
                historyTableMarkStartedTimestamp == 0)
            {
                historyMarkedPocket = pocket;
                historyTableMarkStartedTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                Rect spot = originalLayout.bet_spots[pocket].ToRect();
                historyTableNumberMark.rectTransform.anchoredPosition = new Vector2(
                    spot.x + (spot.width - frames[0].width) * .5f,
                    -spot.y - (spot.height - frames[0].height) * .5f);
            }
            long elapsedMs = OriginalRackClock.ElapsedMillisecondsLong(
                historyTableMarkStartedTimestamp,
                System.Diagnostics.Stopwatch.GetTimestamp(),
                System.Diagnostics.Stopwatch.Frequency);
            int frameIndex = (int)((elapsedMs % 2000L) * 24L / 1000L);
            Texture2D frame = frames[frameIndex];
            historyTableNumberMark.texture = frame;
            historyTableNumberMark.rectTransform.sizeDelta =
                new Vector2(frame.width, frame.height);
            historyTableNumberMark.gameObject.SetActive(true);
        }

        private void RefreshHistoryPageUi()
        {
            if (historyRoot == null) return;
            historyRoot.gameObject.SetActive(historyView);
            if (!historyView) return;
            // Game::DisplayHistoryPage shows the existing panel meters but
            // does not load their values from the selected GameHistory.
            // Restored table chips and pays are historical; panel meters
            // retain the current game values while browsing older rounds.
            historyCredit.Set(session == null ? "" : DisplayedCredit.ToString());
            historyBet.Set(session == null ? "" : session.TotalBet.ToString());
            bool showCollect = session != null && PcCollectMeterVisible;
            historyWinTitle.Set(showCollect ? PcPaymentPhrase(new OriginalPaymentMessageSlot(199, null, null)) : "WIN");
            historyPaid.Set(session == null ? "" : showCollect ?
                OriginalCollectMeterFormatting.PanelCredits(pcCollectMeterCredits) : DisplayedPaid.ToString());
            bool showCashCredit = session != null && pcCashMeterJurisdictionEnabled;
            historyCashCreditBackground.gameObject.SetActive(showCashCredit);
            historyCashCredit.SetVisible(showCashCredit);
            historyCashCredit.Set(showCashCredit ?
                RouletteDenominationFormatter.ForFeatureCurrency(
                    DisplayedCredit * session.Denomination) : "");
            historyCashBet.Set(session == null ? "" :
                RouletteDenominationFormatter.ForFeatureCurrency(
                    session.TotalBet * session.Denomination));
            historyCashPaidBackground.gameObject.SetActive(session != null);
            historyCashPaid.SetVisible(session != null);
            historyCashPaid.Set(session == null ? "" : showCollect ?
                OriginalCollectMeterFormatting.PanelCash(pcCollectMeterCredits, session.Denomination,
                    OriginalMonetaryLocales.Load(OriginalMonetaryLocales.PcVirtualLocaleName)) :
                RouletteDenominationFormatter.ForFeatureCurrency(DisplayedPaid * session.Denomination));
            int pageCount = CommittedHistoryCount();
            historyPage = Mathf.Clamp(historyPage, 0, Mathf.Max(0, pageCount - 1));
            // Publication changes the mapping from visible pages to stored
            // records, including the overflow slot. Resolve it on refresh.
            histogramPage = HistoryRecordIndex(historyPage);
            if (historyShownVersion == recordedHistoryVersion &&
                historyShownPage == historyPage)
            {
                RefreshHistoryTableNumberMark(historyMarkedPocket, false);
                RefreshHistoryWinCycle();
                return;
            }
            if (doubleHistoryView) SetDoubleHistoryView(false);
            historyShownVersion = recordedHistoryVersion;
            historyShownPage = historyPage;
            int sourceIndex = HistoryRecordIndex(historyPage);
            RecordedRound round;
            byte[] pockets;
            bool hasPage = TryGetHistoryRecord(sourceIndex, out round, out pockets);
            bool hasDetail = hasPage && round != null && round.present;
            // DisplayHistoryPage creates and initializes a fresh RouletteWheel
            // for each base history page. Initialize calls srand(time(NULL))
            // on the process-global visual rand stream.
            if (hasPage) InitializeWheelVisualRandom();
            int pocket = hasPage ? pockets[0] : -1;
            RefreshHistoryTableNumberMark(pocket, true);
            historyResultOval.gameObject.SetActive(hasPage);
            historyResultNumber.Set(hasPage ? pocket.ToString() : "");
            historyToteNumber.Set(hasPage ? pocket.ToString() : "");
            historyToteMeterBox.gameObject.SetActive(hasPage);
            if (hasPage)
                historyToteMeterBox.texture = toteMeterBoxFrames[ToteMeterBoxFrame(pocket)];
            if (hasPage)
                historyResultOval.texture = pocket == 0 ? resultSpotGreen :
                    RedNumbers.Contains(pocket) ? resultSpotRed : resultSpotBlack;
            historyWheel.rectTransform.localEulerAngles =
                new Vector3(0, 0, hasDetail ? -(round.sourceWheelStopRecorded ?
                    OriginalWheelArithmetic.RestoreMainAngle(round.sourceWheelStopIndex) :
                    OriginalWheelArithmetic.RenderAngle(RestoredWheelPhase(round))) : 0);
            historySpindle.rectTransform.localEulerAngles =
                new Vector3(0, 0, hasDetail ? -OriginalWheelArithmetic.RenderAngle(RestoredWheelPhase(round)) : 0);
            historyBall.gameObject.SetActive(hasDetail);
            if (hasDetail)
            {
                // Original DisplayHistoryPage constructs a new wheel and
                // restores its saved stop index and pocket from WheelData.
                Vector2 position = RestoredBallCenter(round);
                historyBall.rectTransform.anchoredPosition =
                    new Vector2(position.x - 12f, -(position.y - 12f));
            }
            ClearHistoryLayer(historyNumbersRoot);
            if (hasPage)
                for (int i = 1; i < 20 && pockets[i] <= 36; ++i)
                {
                    int previous = pockets[i];
                    OriginalBitmapFont font = previous == 0 ? toteGreenFont :
                        RedNumbers.Contains(previous) ? toteRedFont : toteYellowFont;
                    new PictureLabel(historyNumbersRoot,
                        new Rect(915, 121 + (i - 1) * 32, 112, 29), font,
                        "Historical previous tote number", nativeSize: true,
                        horizontalAlign: previous == 0 ? 0 :
                            RedNumbers.Contains(previous) ? 1 : -1)
                        .Set(previous.ToString());
                }
            ClearHistoryLayer(historyChipLayer);
            historyResultProfile = null;
            historyResultTotal = hasDetail ? round.returned : 0;
            historyWinningPays = hasDetail ? RestoreHistoryTable(round) :
                new List<WinningPayRecord>();
            historyWinCycleStartedTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp();
            historyVisiblePayIndex = -2;
            historyDenom.Set(hasDetail ?
                RouletteDenominationFormatter.ForFilmedCabinet(round.denomination) : "");
            historyLinePays.Set("");
            historyGamePays.Set(hasDetail ? "Game Pays " + historyResultTotal : "");
            historyInfo.Set(hasPage ? "Base Game" : "");
            // Packaged PHRASE_HISTORY_TEXT_COUNT is "%2ld of %2ld".
            historyPageNumber.Set(hasPage ? string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0,2} of {1,2}", historyPage + 1, pageCount) : "");
            historyPrevious.interactable = historyPage + 1 < pageCount;
            historyNext.interactable = historyPage > 0;
            if (historyDoubleButton != null)
                historyDoubleButton.gameObject.SetActive(hasDetail &&
                    round.doubleUp != null && round.doubleUp.complete);
            historyPrevious.GetComponent<RouletteOriginalButtonFrames>().Refresh();
            historyNext.GetComponent<RouletteOriginalButtonFrames>().Refresh();
            RefreshHistoryWinCycle();
        }

        private static void ClearHistoryLayer(RectTransform root)
        {
            for (int i = root.childCount - 1; i >= 0; --i)
            {
                GameObject old = root.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }
        }

        private static int ActiveHistoryLayerChildren(RectTransform root)
        {
            int count = 0;
            for (int i = 0; i < root.childCount; ++i)
                if (root.GetChild(i).gameObject.activeSelf) ++count;
            return count;
        }

        private List<WinningPayRecord> RestoreHistoryTable(RecordedRound round)
        {
            var winningPays = new List<WinningPayRecord>();
            byte[] table = string.IsNullOrEmpty(round.sourceTableBlock) ?
                EncodeSourceTableBlock(round.bets) :
                Convert.FromBase64String(round.sourceTableBlock);
            RouletteProfile profile = null;
            try
            {
                string prefix = "RouletteDeluxeSZ";
                if (round.profile.StartsWith(prefix, StringComparison.Ordinal))
                    profile = RouletteProfile.Load(round.profile.Substring(prefix.Length));
            }
            catch (Exception error) { Debug.LogWarning("History profile unavailable: " + error.Message); }
            historyResultProfile = profile;
            foreach (int index in Enumerable.Range(0, originalLayout.bet_spots.Length)
                .OrderBy(i => originalLayout.bet_spots[i].z).ThenBy(i => i))
            {
                int amount = table[index * 2];
                if (amount == 0) continue;
                Rect spot = originalLayout.bet_spots[index].ToRect();
                float cx = spot.x + spot.width * .5f;
                float cy = spot.y + spot.height * .5f;
                // Source RestoreTableBets calls RouletteTable::SetBet with the
                // aggregate saved byte. SetBet selects the largest chip value
                // no greater than that amount; individual chip IDs were not
                // stored in the original 0x284-byte history table block.
                int chipIndex = ClosestBetSelector(amount);
                UiImage(historyChipLayer, new Rect(cx - 36, cy - 36, 72, 72),
                    originalPlacedChips[chipIndex], "Restored chip " + index);
                new PictureLabel(historyChipLayer,
                    new Rect(cx - 15, cy - 12, 30, 24), chipStackFont,
                    "Restored chip amount", nativeSize: true)
                    .Set(amount.ToString());
            }
            // The source restores the table in ascending spot ID order,
            // evaluates and sorts those pays, then applies saved overrides
            // without sorting again. Keep that order even when a live round
            // originally evaluated equal-value spots in a different order.
            if (profile != null)
            {
                for (int index = 0; index < originalLayout.bet_spots.Length; ++index)
                {
                    if (table[index * 2] == 0) continue;
                    WinRecord record;
                    if (!profile.Records.TryGetValue(100 + index, out record) ||
                        !record.Covers(round.pocket)) continue;
                    winningPays.Add(new WinningPayRecord {
                        BetId = 100 + index,
                        Amount = (long)table[index * 2] * record.ReturnMultiple,
                        WinLevel = record.WinLevel
                    });
                    RouletteSession.SortSourceWinningPays(winningPays);
                }
            }
            if (round.winningPays != null)
            {
                foreach (WinningPayRecord pay in winningPays)
                {
                    RecordedPay saved = Array.Find(round.winningPays,
                        item => item != null && item.id == pay.BetId);
                    if (saved != null) pay.Amount = saved.amount;
                }
            }
            if (round.historyPayOverrides != null)
                foreach (RecordedHistoryPayOverride entry in
                    round.historyPayOverrides)
                    if (entry != null && entry.payIndex >= 0 &&
                        entry.payIndex < winningPays.Count)
                    {
                        WinningPayRecord pay = winningPays[entry.payIndex];
                        // Source ProcessHistoryOverridePays replaces this
                        // slot and applies its amount difference to the
                        // whole result. Repeated slot entries apply in order.
                        historyResultTotal = checked(historyResultTotal +
                            entry.amount - pay.Amount);
                        pay.Amount = entry.amount;
                    }
            return winningPays;
        }

        private void RefreshHistoryWinCycle()
        {
            int visible = -1;
            if (historyWinningPays.Count > 0)
            {
                double elapsed = CycleElapsedSeconds(
                    historyWinCycleStartedTimestamp,
                    System.Diagnostics.Stopwatch.GetTimestamp(),
                    System.Diagnostics.Stopwatch.Frequency);
                long halfCycle = (long)Math.Floor(
                    elapsed / OriginalCycleResultDelaySeconds);
                if ((halfCycle & 1) == 0)
                    visible = (int)((halfCycle / 2) % historyWinningPays.Count);
            }
            if (visible == historyVisiblePayIndex) return;
            historyVisiblePayIndex = visible;
            ClearHistoryLayer(historyWinLayer);
            historyTotalWinTitle.gameObject.SetActive(visible >= 0);
            historyTotalWinAmount.Set(visible >= 0 ?
                "(" + historyResultTotal + ")" : "");
            if (visible < 0)
            {
                historyLinePays.Set("");
                if (historyWinningPays.Count > 0) historyGamePays.Set("");
                return;
            }
            WinningPayRecord pay = historyWinningPays[visible];
            int spotIndex = pay.BetId - 100;
            Rect spot = originalLayout.bet_spots[spotIndex].ToRect();
            float cx = spot.x + spot.width * .5f;
            float cy = spot.y + spot.height * .5f;
            Rect bounds = new Rect(cx - 59.5f, cy - 54f, 119f, 108f);
            UiImage(historyWinLayer, bounds, betWinHighlight,
                "Historical winning spot");
            new PictureLabel(historyWinLayer, bounds, cyclePayFont,
                "Historical winning payout", nativeSize: true)
                .Set(SourceBetSpotPayout(pay.Amount));
            historyLinePays.Set(RoulettePayMessageFormatter.LinePays(
                historyResultProfile.Records[pay.BetId], pay.Amount));
            historyGamePays.Set("Game Pays " + historyResultTotal);
        }

        private IEnumerator HistoryPageSmoke()
        {
            string[] launch = Environment.GetCommandLineArgs();
            int captureOption = Array.IndexOf(launch, "-rouletteHistoryPageCapture");
            string captureDirectory = captureOption >= 0 && captureOption + 1 < launch.Length ?
                launch[captureOption + 1] : null;
            if (!string.IsNullOrEmpty(captureDirectory))
                System.IO.Directory.CreateDirectory(captureDirectory);
            yield return null;
            if (historyRoot == null || originalLayout.bet_spots.Length != 157)
                throw new InvalidOperationException("History screen is not bound");
            if (historyPrevious.GetComponent<RectTransform>().anchoredPosition !=
                    new Vector2(315, -6) ||
                historyNext.GetComponent<RectTransform>().anchoredPosition !=
                    new Vector2(443, -6) ||
                historyGame.GetComponent<RectTransform>().anchoredPosition !=
                    new Vector2(177, -40))
                throw new InvalidOperationException("Original DS history coordinates differ");
            temporaryHistory = true;
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            var source = new RouletteSession(session.Profile, 1000,
                new FixedPocketSmokeOutcome());
            source.PlaceBet(100, 1);
            RoundResult older = source.Spin();
            RecordPocket(older.Pocket, older, source);
            source.NextRound();
            source.Clear();
            source.SetChip(5);
            source.PlaceBet(100, 5);
            source.SetChip(1);
            source.PlaceBet(100, 1);
            source.PlaceBet(256, 1);
            RoundResult newer = source.Spin();
            RecordPocket(newer.Pocket, newer, source);
            recordedRoundPages[0].wheelAngle = 45f;
            recordedRoundPages[0].sourceWheelStopRecorded = true;
            recordedRoundPages[0].sourceWheelStopIndex = 9;
            recordedRoundPages[0].ballX = 1;
            recordedRoundPages[0].ballY = 1;
            double restoredPhase = RestoredWheelPhase(recordedRoundPages[0]);
            Vector2 savedBall = RestoredBallCenter(recordedRoundPages[0]);
            recordedRoundPages[0].awaitingFinalPayout = true;
            session.PlaceBet(100, 1);
            selectedNeighbor = 0;
            SetHistoryView(true);
            yield return null;
            yield return null;
            if (uiZeroBetButton.interactable || selectedNeighbor != 0 ||
                session.TotalBet != 1)
                throw new InvalidOperationException(
                    "History did not block table input or preserve the active wager");
            var blockedPointer = new PointerEventData(EventSystem.current) {
                position = Vector2.zero };
            ExecuteEvents.Execute(uiZeroBetButton.gameObject, blockedPointer,
                ExecuteEvents.pointerExitHandler);
            if (selectedNeighbor != 0)
                throw new InvalidOperationException(
                    "Opening history cleared the live neighbor selection");
            if (historyPageNumber.Value != " 1 of  1" ||
                historyPrevious.interactable || HistoryRecordIndex(0) != 1 ||
                historyMarkedPocket != 0 ||
                !historyToteMeterBox.gameObject.activeSelf ||
                historyToteMeterBox.texture != toteMeterBoxFrames[0] ||
                !historyTableNumberMark.gameObject.activeSelf ||
                !tableWinZeroFrames.Contains(
                    historyTableNumberMark.texture as Texture2D) ||
                historyTableNumberMark.rectTransform.sizeDelta !=
                    new Vector2(570, 144) ||
                historyLinePays.Value != "Straight Up Pays 36" ||
                historyGamePays.Value != "Game Pays " + older.Returned ||
                !historyTotalWinTitle.gameObject.activeSelf ||
                historyTotalWinAmount.Value != "(" + older.Returned + ")")
                throw new InvalidOperationException("Unfinished feature entered visible history");
            recordedRoundPages[0].awaitingFinalPayout = false;
            ++recordedHistoryVersion;
            yield return null;
            yield return null;
            if (!historyRoot.gameObject.activeSelf || historyPageNumber.Value != " 1 of  2" ||
                historyChipLayer.childCount < 4 || !historyBall.gameObject.activeSelf ||
                recordedRoundPages[0].creditAfter == session.Credit ||
                historyCredit.Value != session.Credit.ToString() ||
                recordedRoundPages[0].wager == session.TotalBet ||
                historyBet.Value != session.TotalBet.ToString() ||
                recordedRoundPages[0].returned == DisplayedPaid ||
                historyPaid.Value != DisplayedPaid.ToString() ||
                historyCashCredit.Visible ||
                historyCashBet.Value != "RM0.01" ||
                historyCashPaid.Value != "RM0.00" ||
                Vector2.Distance(historyBall.rectTransform.anchoredPosition,
                    new Vector2(savedBall.x - 12f, -(savedBall.y - 12f))) > .01f ||
                Mathf.Abs(historyWheel.rectTransform.localEulerAngles.z -
                    Mathf.Repeat(-OriginalWheelArithmetic.RestoreMainAngle(recordedRoundPages[0].sourceWheelStopIndex), 360f)) > .01f ||
                Mathf.Abs(historySpindle.rectTransform.localEulerAngles.z -
                    Mathf.Repeat(-OriginalWheelArithmetic.RenderAngle(restoredPhase), 360f)) > .01f ||
                historyPaid.GlyphCount == 0 || historyInfo.GlyphCount == 0 ||
                !historyPrevious.interactable || historyNext.interactable ||
                ((RawImage)historyNext.targetGraphic).texture !=
                    Resources.Load<Texture2D>("Original/History/DSNext_4"))
                throw new InvalidOperationException("Latest history page did not render: " +
                    "active=" + historyRoot.gameObject.activeSelf +
                    " page=" + historyPageNumber.Value +
                    " chips=" + historyChipLayer.childCount +
                    " ball=" + historyBall.gameObject.activeSelf +
                    " paid=" + historyPaid.GlyphCount +
                    " info=" + historyInfo.GlyphCount +
                    " previous=" + historyPrevious.interactable +
                    " next=" + historyNext.interactable +
                    " nextFrame=" + ((RawImage)historyNext.targetGraphic).texture.name);
            if (historyWinningPays.Count != 2 ||
                ActiveHistoryLayerChildren(historyWinLayer) != 2 ||
                historyLinePays.Value != "Straight Up Pays 216" ||
                !historyTotalWinTitle.gameObject.activeSelf ||
                historyTotalWinAmount.Value != "(" + recordedRoundPages[0].returned + ")" ||
                historyGamePays.Value != "Game Pays " + recordedRoundPages[0].returned)
                throw new InvalidOperationException(
                    "Original history did not show one sorted bet pay");
            // The source history page shows the existing physical meters.
            // During a rollup they contain the current displayed partial
            // values, while the session ledger already contains the final
            // credit. Also exercise the jurisdiction-gated cash label.
            bool wasRackActive = winRackActive;
            long oldRackBaseCredit = winRackBaseCredit;
            long oldRackPaid = winRackPaid;
            bool oldCashGate = pcCashMeterJurisdictionEnabled;
            try
            {
                winRackActive = true;
                winRackBaseCredit = session.Credit - 10;
                winRackPaid = 4;
                pcCashMeterJurisdictionEnabled = true;
                RefreshHistoryPageUi();
                if (historyCredit.Value != (session.Credit - 6).ToString() ||
                    historyPaid.Value != "4" ||
                    !historyCashCredit.Visible ||
                    !historyCashCreditBackground.gameObject.activeSelf ||
                    historyCashCredit.Value !=
                        RouletteDenominationFormatter.ForFeatureCurrency(
                            (session.Credit - 6) * session.Denomination) ||
                    historyCashPaid.Value !=
                        RouletteDenominationFormatter.ForFeatureCurrency(
                            4 * session.Denomination))
                    throw new InvalidOperationException(
                        "History did not retain partial live panel meters");
            }
            finally
            {
                winRackActive = wasRackActive;
                winRackBaseCredit = oldRackBaseCredit;
                winRackPaid = oldRackPaid;
                pcCashMeterJurisdictionEnabled = oldCashGate;
                RefreshHistoryPageUi();
            }
            if (historyCredit.Value != session.Credit.ToString() ||
                historyCashCredit.Visible || historyPaid.Value != "0")
                throw new InvalidOperationException(
                    "History panel meters did not return to the live state");
            int visualSeedOption = Array.IndexOf(launch, "-rouletteVisualSeed");
            bool fixedVisualSeed = visualSeedOption >= 0 &&
                visualSeedOption + 1 < launch.Length &&
                launch[visualSeedOption + 1] == "1";
            if (fixedVisualSeed && wheelVisualRandom.NextPath(37) != 30)
                throw new InvalidOperationException(
                    "Opening source history did not reseed the visual wheel RNG");
            long cycleNow = System.Diagnostics.Stopwatch.GetTimestamp();
            long cycleFrequency = System.Diagnostics.Stopwatch.Frequency;
            historyWinCycleStartedTimestamp = cycleNow - (long)(1.55d * cycleFrequency);
            RefreshHistoryWinCycle();
            if (ActiveHistoryLayerChildren(historyWinLayer) != 0 ||
                historyLinePays.Value != "" ||
                historyTotalWinTitle.gameObject.activeSelf ||
                historyTotalWinAmount.Value != "" ||
                historyGamePays.Value != "")
                throw new InvalidOperationException(
                    "Original history pay did not hide between cycle entries");
            historyWinCycleStartedTimestamp = cycleNow - (long)(3.05d * cycleFrequency);
            RefreshHistoryWinCycle();
            if (ActiveHistoryLayerChildren(historyWinLayer) != 2 ||
                historyLinePays.Value != "Split Pays 18" ||
                !historyTotalWinTitle.gameObject.activeSelf ||
                historyTotalWinAmount.Value != "(" + recordedRoundPages[0].returned + ")" ||
                historyGamePays.Value != "Game Pays " + recordedRoundPages[0].returned)
                throw new InvalidOperationException(
                    "Original history did not advance to the next bet pay");
            if (!string.IsNullOrEmpty(captureDirectory))
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(captureDirectory,
                    "history_second_pay.png"));
            historyWinCycleStartedTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
            RefreshHistoryWinCycle();
            if (historyChipLayer.Find("Restored chip 0").GetComponent<RawImage>().texture !=
                    originalPlacedChips[2] ||
                historyChipLayer.Find("Restored chip 156").GetComponent<RawImage>().texture !=
                    originalPlacedChips[0])
                throw new InvalidOperationException("History chip selector ignored aggregate bet value");
            if (!string.IsNullOrEmpty(captureDirectory))
            {
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(captureDirectory,
                    "history_latest.png"));
            }
            // The source's history override pass replaces individual pay
            // amounts after evaluation. Verify the archived values reach the
            // active overlay without altering the recorded total.
            RecordedPay firstSavedPay = recordedRoundPages[0].winningPays[0];
            RecordedPay secondSavedPay = recordedRoundPages[0].winningPays[1];
            long firstOriginalAmount = firstSavedPay.amount;
            long secondOriginalAmount = secondSavedPay.amount;
            firstSavedPay.amount++;
            secondSavedPay.amount--;
            Array.Reverse(recordedRoundPages[0].winningPays);
            ++recordedHistoryVersion;
            RefreshHistoryPageUi();
            if (historyWinningPays[0].BetId != firstSavedPay.id ||
                historyWinningPays[0].Amount != firstOriginalAmount + 1 ||
                historyWinningPays[1].Amount != secondOriginalAmount - 1 ||
                historyGamePays.Value != "Game Pays " + recordedRoundPages[0].returned)
                throw new InvalidOperationException(
                    "Saved per-bet history override did not reach the cycle");
            Array.Reverse(recordedRoundPages[0].winningPays);
            firstSavedPay.amount = firstOriginalAmount;
            secondSavedPay.amount = secondOriginalAmount;
            RecordedPay[] currentPays = recordedRoundPages[0].winningPays;
            recordedRoundPages[0].winningPays = null;
            ++recordedHistoryVersion;
            RefreshHistoryPageUi();
            if (historyWinningPays[0].Amount != firstOriginalAmount ||
                historyWinningPays[1].Amount != secondOriginalAmount)
                throw new InvalidOperationException(
                    "Earlier history archive did not re-evaluate table bets");
            recordedRoundPages[0].winningPays = currentPays;
            ++recordedHistoryVersion;
            RefreshHistoryPageUi();
            recordedRoundPages[0].historyPayOverrides =
                new[] {
                    new RecordedHistoryPayOverride {
                        payIndex = 1, amount = secondOriginalAmount + 1,
                        progressiveLevel = 0, kind = 2
                    },
                    new RecordedHistoryPayOverride {
                        payIndex = 1, amount = secondOriginalAmount + 2,
                        progressiveLevel = 0, kind = 2
                    }
                };
            ++recordedHistoryVersion;
            RefreshHistoryPageUi();
            if (historyWinningPays[0].Amount != firstOriginalAmount ||
                historyWinningPays[1].Amount != secondOriginalAmount + 2 ||
                historyResultTotal != recordedRoundPages[0].returned + 2 ||
                historyTotalWinAmount.Value != "(" +
                    (recordedRoundPages[0].returned + 2) + ")" ||
                historyGamePays.Value != "Game Pays " +
                    (recordedRoundPages[0].returned + 2))
                throw new InvalidOperationException(
                    "Source history pay-slot override did not update the total");
            recordedRoundPages[0].historyPayOverrides = null;
            ++recordedHistoryVersion;
            RefreshHistoryPageUi();
            var pointer = new PointerEventData(EventSystem.current);
            RectTransform previousLabel = historyPrevious.transform.Find(
                "Bitmap text") as RectTransform;
            if (previousLabel == null || previousLabel.anchoredPosition != Vector2.zero ||
                previousLabel.rect.size != historyPrevious.GetComponent<RectTransform>().rect.size)
                throw new InvalidOperationException("History label is not centered inside source Button bounds");
            ExecuteEvents.Execute(historyPrevious.gameObject, pointer,
                ExecuteEvents.pointerEnterHandler);
            if (((RawImage)historyPrevious.targetGraphic).texture !=
                Resources.Load<Texture2D>("Original/History/DSPrevious_1") ||
                previousLabel.anchoredPosition != Vector2.zero)
                throw new InvalidOperationException("History hover changed the source neutral frame");
            PointerEventData previousPress =
                PressThroughUiRaycast(historyPrevious);
            if (historyPage != 1)
                throw new InvalidOperationException(
                    "Original history Previous did not act on pointer down");
            if (((RawImage)historyPrevious.targetGraphic).texture !=
                Resources.Load<Texture2D>("Original/History/DSPrevious_2") ||
                previousLabel.anchoredPosition != new Vector2(1, -1))
                throw new InvalidOperationException("History pressed frame did not appear");
            if (!string.IsNullOrEmpty(captureDirectory))
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(captureDirectory,
                    "history_previous_pressed.png"));
            ReleaseUiPointer(historyPrevious, previousPress);
            if (previousLabel.anchoredPosition != Vector2.zero)
                throw new InvalidOperationException("History label did not return on release");
            if (historyPage != 1)
                throw new InvalidOperationException(
                    "History Previous repeated on pointer release");
            ExecuteEvents.Execute(historyPrevious.gameObject, pointer,
                ExecuteEvents.pointerExitHandler);
            if (((RawImage)historyPrevious.targetGraphic).texture !=
                Resources.Load<Texture2D>("Original/History/DSPrevious_1"))
                throw new InvalidOperationException("History normal frame did not return");
            yield return null;
            yield return null;
            Vector2 olderBall = RestoredBallCenter(recordedRoundPages[1]);
            if (historyPage != 1 || historyPageNumber.Value != " 2 of  2" ||
                historyChipLayer.childCount < 2 || !historyNext.interactable ||
                recordedRoundPages[1].creditAfter == session.Credit ||
                historyCredit.Value != session.Credit.ToString() ||
                recordedRoundPages[1].returned == DisplayedPaid ||
                historyBet.Value != session.TotalBet.ToString() ||
                historyPaid.Value != DisplayedPaid.ToString() ||
                historyCashBet.Value != "RM0.01" ||
                historyCashPaid.Value != "RM0.00" ||
                historyPrevious.interactable ||
                historyWinningPays.Count != 1 ||
                ActiveHistoryLayerChildren(historyWinLayer) != 2 ||
                historyLinePays.Value != "Straight Up Pays 36" ||
                !historyTotalWinTitle.gameObject.activeSelf ||
                historyTotalWinAmount.Value != "(" + recordedRoundPages[1].returned + ")" ||
                historyGamePays.Value != "Game Pays " + recordedRoundPages[1].returned ||
                Vector2.Distance(historyBall.rectTransform.anchoredPosition,
                    new Vector2(olderBall.x - 12f,
                        -(olderBall.y - 12f))) > .01f ||
                Mathf.Abs(historySpindle.rectTransform.localEulerAngles.z -
                    historyWheel.rectTransform.localEulerAngles.z) > .01f ||
                ((RawImage)historyPrevious.targetGraphic).texture !=
                    Resources.Load<Texture2D>("Original/History/DSPrevious_4"))
                throw new InvalidOperationException("Previous history page did not render");
            if (fixedVisualSeed && wheelVisualRandom.NextPath(37) != 30)
                throw new InvalidOperationException(
                    "Changing source history page did not reseed the visual wheel RNG");
            if (!string.IsNullOrEmpty(captureDirectory))
            {
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(captureDirectory,
                    "history_previous.png"));
            }
            PointerEventData nextPress = PressThroughUiRaycast(historyNext);
            RectTransform nextLabel = historyNext.transform.Find("Bitmap text") as RectTransform;
            if (nextLabel.anchoredPosition != new Vector2(1, -1))
                throw new InvalidOperationException("History Next label did not depress");
            if (historyPage != 0)
                throw new InvalidOperationException(
                    "Original history Next did not act on pointer down");
            ReleaseUiPointer(historyNext, nextPress);
            if (nextLabel.anchoredPosition != Vector2.zero)
                throw new InvalidOperationException("History Next label did not return on release");
            if (historyPage != 0)
                throw new InvalidOperationException(
                    "History Next repeated on pointer release");
            PointerEventData returnPress = PressThroughUiRaycast(historyGame);
            RectTransform returnLabel = historyGame.transform.Find("Bitmap text") as RectTransform;
            if (returnLabel.anchoredPosition != new Vector2(1, -1))
                throw new InvalidOperationException("History Return label did not depress");
            if (historyView)
                throw new InvalidOperationException(
                    "Original history Return did not act on pointer down");
            ReleaseUiPointer(historyGame, returnPress);
            if (returnLabel.anchoredPosition != Vector2.zero)
                throw new InvalidOperationException("History Return label did not return on release");
            yield return new WaitForEndOfFrame();
            RefreshPortraitUi();
            if (historyView || historyRoot.gameObject.activeSelf ||
                !uiZeroBetButton.interactable || selectedNeighbor != 0 ||
                session.TotalBet != 1)
                throw new InvalidOperationException("History GAME button did not exit");
            Debug.Log("ROULETTE_HISTORY_PAGE_PASSED: source DS sprites, bet recall, blocked table, navigation, exit");
        }

        private IEnumerator ToteColorSmoke()
        {
            string[] launch = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(launch, "-rouletteToteColorSmoke");
            if (option + 1 >= launch.Length || launch[option + 1].StartsWith("-"))
                throw new ArgumentException("Tote color capture directory is missing");
            string directory = launch[option + 1];
            System.IO.Directory.CreateDirectory(directory);
            yield return null;
            temporaryHistory = true;
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            ++recordedHistoryVersion;
            RefreshPortraitUi();
            if (uiToteMeterBox.gameObject.activeSelf || uiToteNumber.Value != "")
                throw new InvalidOperationException("Empty tote rendered a saved result");
            foreach (int pocket in new[] { 0, 1, 2 })
            {
                var source = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(pocket));
                source.PlaceBet(100, 1);
                RoundResult result = source.Spin();
                RecordPocket(pocket, result, source);
                RefreshPortraitUi();
                yield return null;
                if (!uiToteMeterBox.gameObject.activeSelf ||
                    uiToteMeterBox.texture != toteMeterBoxFrames[pocket] ||
                    uiToteNumber.Value != pocket.ToString() ||
                    uiToteMeterBox.rectTransform.anchoredPosition != new Vector2(914, -34) ||
                    uiToteMeterBox.rectTransform.sizeDelta != new Vector2(114, 79) ||
                    uiToteMeterBox.transform.GetSiblingIndex() >=
                        uiToteNumber.Root.GetSiblingIndex())
                    throw new InvalidOperationException("Main tote color or layer differs: " + pocket);
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory,
                    "main_" + pocket + ".png"));
            }
            SetHistoryView(true);
            for (int page = 0; page < 3; ++page)
            {
                ShowHistoryPage(page);
                yield return null;
                yield return null;
                int pocket = 2 - page;
                if (!historyToteMeterBox.gameObject.activeSelf ||
                    historyToteMeterBox.texture != toteMeterBoxFrames[pocket] ||
                    historyToteNumber.Value != pocket.ToString() ||
                    historyToteMeterBox.rectTransform.anchoredPosition != new Vector2(914, -34) ||
                    historyToteMeterBox.rectTransform.sizeDelta != new Vector2(114, 79) ||
                    historyToteMeterBox.transform.GetSiblingIndex() >=
                        historyToteNumber.Root.GetSiblingIndex())
                    throw new InvalidOperationException("History tote color or layer differs: " + pocket);
                CaptureActiveScreenOffscreen(System.IO.Path.Combine(directory,
                    "history_" + pocket + ".png"));
            }
            SetHistoryView(false);
            yield return null;
            if (uiToteMeterBox.texture != toteMeterBoxFrames[2] ||
                uiToteNumber.Value != "2")
                throw new InvalidOperationException("Returning from history replaced the live tote");
            Debug.Log("ROULETTE_TOTE_COLOR_PASSED: main/history 0=green 1=red 2=black");
            Application.Quit();
        }

        private IEnumerator HistoryColorCapture()
        {
            string[] launch = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(launch, "-rouletteHistoryColorCapture");
            if (option < 0 || option + 1 >= launch.Length ||
                launch[option + 1].StartsWith("-"))
                throw new ArgumentException("History color capture path is missing");
            yield return null;
            temporaryHistory = true;
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRoundPages.Clear();
            foreach (int pocket in new[] { 18, 6, 2 })
            {
                var source = new RouletteSession(session.Profile, 1000,
                    new FixedPocketSmokeOutcome(pocket));
                source.PlaceBet(100, 1);
                RoundResult result = source.Spin();
                if (result.Pocket != pocket || result.Returned != 0)
                    throw new InvalidOperationException(
                        "History color fixture must contain committed losing rounds");
                RecordPocket(pocket, result, source);
            }
            SetHistoryView(true);
            yield return null;
            yield return null;
            if (historyToteNumber.Value != "2" ||
                historyToteMeterBox.texture != toteMeterBoxFrames[2] ||
                historyNumbersRoot.childCount != 2 ||
                historyResultNumber.Value != "2" ||
                historyMarkedPocket != 2 ||
                !historyTableNumberMark.gameObject.activeSelf ||
                !tableWinNonZeroFrames.Contains(
                    historyTableNumberMark.texture as Texture2D) ||
                historyTableNumberMark.rectTransform.sizeDelta !=
                    new Vector2(244, 182))
                throw new InvalidOperationException(
                    "Committed red/yellow history sequence did not render");
            CaptureActiveScreenOffscreen(launch[option + 1]);
            Debug.Log("ROULETTE_HISTORY_COLOR_CAPTURE_PASSED: latest=2 previous=6,18");
        }
    }
}
