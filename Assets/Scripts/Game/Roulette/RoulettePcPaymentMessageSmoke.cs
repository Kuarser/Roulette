using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private Action<PcCreditEvent> pcPaymentCreditSavedTestHook;
        private bool pcPaymentSmokeCut;
        [Serializable] private sealed class CollectMeterSmokeReport
        {
            public string phase, title, creditsText, cashText, meterJournal, financialJournal;
            public int checks, pockets, financialPhase;
            public long credit, denomination, rounds;
            public double collected;
            public bool visible, cleared, au;
        }
        private void SaveCollectMeterSmokeReport(string phase, int checks)
        {
            var financial = ReadPcCreditEvent();
            var report = new CollectMeterSmokeReport { phase = phase, checks = checks, title = uiWinTitle.Value,
                creditsText = uiPaid.Value, cashText = uiCashPaid.Value, credit = session.Credit, denomination = session.Denomination,
                rounds = session.Rounds, pockets = recordedPockets.Count, collected = pcCollectMeterCredits,
                visible = PcCollectMeterVisible, cleared = pcCollectMeterRecord != null && pcCollectMeterRecord.cleared,
                au = auNationalStandardsEnabled, financialPhase = financial.phase,
                meterJournal = PlayerPrefs.GetString(PcCollectMeterKey, ""), financialJournal = PlayerPrefs.GetString(PcCreditKey, "") };
            string path = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            if (File.Exists(path)) throw new Exception("Frozen collect meter output exists");
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log("ROULETTE_PC_COLLECT_METER_PASSED phase=" + phase + " checks=" + checks);
        }
        private IEnumerator PcCollectMeterSmoke(string phase)
        {
            PaymentSmokeRequire(recordedHistoryKey.Contains("PcPaymentMessageTestMeter") && session != null &&
                !temporaryHistory && recordedPockets.Count == 0 && session.Rounds == 0, "owned collect meter scope");
            int checks = 1;
            bool fresh = phase == "meter_live" || phase == "meter_fraction" || phase == "meter_au" ||
                phase == "meter_denomsetup" || phase == "meter_cut";
            if (fresh)
            {
                PaymentSmokeRequire(session.Credit == 1000 && !PlayerPrefs.HasKey(PcCollectMeterKey), "fresh meter writer"); checks++;
                if (phase == "meter_fraction")
                {
                    AddPcVirtualCredits(1);
                    for (int i = 0; i < session.Profile.Denominations.Length && session.Denomination != 25; i++) RequestPcDenominationChange();
                    PaymentSmokeRequire(session.Denomination == 25 && session.Credit == 40 &&
                        ((RouletteSession)session).MoneyRemainderCents == 1, "fractional financial amount"); checks++;
                }
                if (phase == "meter_cut") pcPaymentCreditSavedTestHook = item => {
                    if (item.kind != "collect" || item.phase != 2) return;
                    PaymentSmokeRequire(session.Credit == 1000 && !PlayerPrefs.HasKey(PcCollectMeterKey), "cut before display/wallet application");
                    pcPaymentSmokeCut = true; SaveCollectMeterSmokeReport(phase, checks + 1); Application.Quit(0);
                    throw new OperationCanceledException("Owned collect meter phase 2 cut");
                };
                HandlePcCashoutRelease(); ConfirmPcCollect(true);
                for (int i = 0; i < 20 && pcCollectPending && !pcPaymentSmokeCut; i++) yield return null;
                if (pcPaymentSmokeCut) yield break;
                PaymentSmokeRequire(!pcCollectPending && session.Credit == 0 && ReadPcCreditEvent().phase == 3,
                    "real virtual collect completion"); checks++;
            }
            else if (phase == "meter_deposit")
            {
                PaymentSmokeRequire(session.Credit == 0 && pcCollectMeterCredits == 1000, "deposit follows completed collect"); checks++;
                AddPcVirtualCredits(100); yield return null; yield return null;
            }
            else if (phase == "meter_clear")
            {
                PaymentSmokeRequire(session.Credit == 100 && pcCollectMeterCredits == 1000, "clear follows deposited collect"); checks++;
                PaymentSmokeRequire(!PrepareSourceSpin(false), "zero-bet Start invokes real ClearPaid before rejection"); checks++;
            }
            else if (phase == "meter_denom")
            {
                PaymentSmokeRequire(session.Credit == 0 && pcCollectMeterCredits == 1000, "denom follows completed collect"); checks++;
                for (int i = 0; i < session.Profile.Denominations.Length && session.Denomination != 25; i++) RequestPcDenominationChange();
            }
            else
            {
                string before = PlayerPrefs.GetString(PcCollectMeterKey);
                RestorePcCollectMeterBeforeCredit(ReadPcCreditEvent());
                PaymentSmokeRequire(PlayerPrefs.GetString(PcCollectMeterKey) == before, "cold meter admission idempotent"); checks++;
            }
            bool cleared = phase.StartsWith("meter_clear") || phase == "meter_denom" || phase == "meter_denomcold";
            bool au = phase == "meter_au" || phase == "meter_aucold";
            bool fractional = phase.StartsWith("meter_fraction");
            long expectedCredit = phase.StartsWith("meter_deposit") || phase.StartsWith("meter_clear") ? 100 : 0;
            long expectedDenom = fractional || phase == "meter_denom" || phase == "meter_denomcold" ? 25 : 1;
            PaymentSmokeRequire(session.Credit == expectedCredit && session.Denomination == expectedDenom &&
                auNationalStandardsEnabled == au && pcCollectMeterCredits == (cleared ? 0 : fractional ? 40.04 : 1000),
                "wallet/denomination and derived display amount"); checks++;
            PaymentSmokeRequire(pcCollectMeterRecord != null && pcCollectMeterRecord.cleared == cleared &&
                PcCollectMeterVisible == (!cleared && !au), "native AU and cleared display gates"); checks++;
            RefreshPortraitUi(); yield return null; RefreshPortraitUi();
            string expectedCredits = cleared ? "" : au ? "0" : fractional ? "40.04" : "1000";
            string expectedCash = cleared ? "" : au ? "RM0.00" : fractional ? "RM10.01" : "RM10.00";
            PaymentSmokeRequire(uiWinTitle.Value == (cleared || au ? "WIN" : "COLLECT") && uiPaid.Value == expectedCredits &&
                uiCashPaid.Value == expectedCash && uiWinTitle.GlyphCount == (cleared || au ? 3 : 7) &&
                uiPaid.GlyphCount == expectedCredits.Length, "actual bitmap title, credits and cash"); checks++;
            PaymentSmokeRequire(recordedPockets.Count == 0 && session.Rounds == 0, "meter never draws"); checks++;
            PaymentSmokeRequire(!PcCollectMeterVisible || (footerMoviePhase == FooterMoviePhase.Hidden &&
                !uiFooterMovie.gameObject.activeSelf), "native collect digits are not covered by gameplay movie"); checks++;
            ScreenCapture.CaptureScreenshot(PaymentSmokeArg("-roulettePcPaymentMessageOutput") + ".png");
            yield return new WaitForEndOfFrame();
            SaveCollectMeterSmokeReport(phase, checks); Application.Quit(0);
        }
        [Serializable] private sealed class PaymentChainSmokeReport
        {
            public string phase, mutation, expectedRejection, status, key, financialKey;
            public string validJournal, corruptJournal, financialJournal;
            public string journalSha256, financialSha256;
            public int checks, pockets;
            public long credit, rounds;
            public bool sessionPublished;
        }
        private void SavePaymentChainSmokeReport(PaymentChainSmokeReport report)
        {
            string path = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            if (File.Exists(path)) throw new Exception("Frozen payment chain smoke output exists");
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log("ROULETTE_PC_PAYMENT_CHAIN_PASSED phase=" + report.phase + " checks=" + report.checks);
        }
        private IEnumerator PcPaymentChainSmoke(string phase)
        {
            PaymentSmokeRequire(auNationalStandardsEnabled && recordedHistoryKey.Contains("PcPaymentMessageTestChain") &&
                !temporaryHistory, "owned AU chain scope");
            int checks = 1;
            if (phase.EndsWith("_cold", StringComparison.Ordinal))
            {
                var prior = JsonUtility.FromJson<PaymentChainSmokeReport>(File.ReadAllText(PaymentSmokeArg("-roulettePcPaymentMessagePrevious")));
                PaymentSmokeRequire(prior.phase == phase.Substring(0, phase.Length - 5) + "_write" && prior.checks == 5,
                    "cold proof belongs to writer"); checks++;
                PaymentSmokeRequire(session == null && status == "Configuration error: " + prior.expectedRejection,
                    "production startup rejects exact invalid chain before publishing session"); checks++;
                PaymentSmokeRequire(PlayerPrefs.GetString(prior.key) == prior.corruptJournal &&
                    PlayerPrefs.GetString(prior.financialKey) == prior.financialJournal,
                    "rejected startup preserves both durable journals"); checks++;
                PaymentSmokeRequire(recordedPockets.Count == 0 && !spinning && !pcPaymentMessagesActive,
                    "rejected startup publishes no payment and draws nothing"); checks++;
                prior.phase = phase; prior.checks = checks; prior.status = status; prior.sessionPublished = session != null;
                SavePaymentChainSmokeReport(prior); Application.Quit(0); yield break;
            }
            PaymentSmokeRequire(phase.EndsWith("_write", StringComparison.Ordinal) && session != null &&
                session.Credit == 1000 && session.Rounds == 0 && !PlayerPrefs.HasKey(PcPaymentPresentationKey),
                "fresh chain writer scope"); checks++;
            HandlePcCashoutRelease(); ConfirmPcCollect(true);
            for (int frame = 0; frame < 20 && pcCollectPending; frame++) yield return null;
            PaymentSmokeRequire(!pcCollectPending && session.Credit == 0 && pcPaymentMessageState.TotalPaid == 1000 &&
                !pcPaymentMessageState.CollectPending && ReadPcCreditEvent().phase == 3,
                "actual collect completes before one-way corruption"); checks++;
            string valid = PlayerPrefs.GetString(PcPaymentPresentationKey);
            string financial = PlayerPrefs.GetString(PcCreditKey);
            var journal = JsonUtility.FromJson<PaymentPresentationJournal>(valid);
            var entry = journal.entries[0]; var credit = ParseValidatedPcCreditEvent(entry.credit);
            string mutation = phase.Substring(6, phase.Length - 12), expected;
            switch (mutation)
            {
                case "owner": journal.owner = new string('0', 64); expected = "Payment presentation owner"; break;
                case "version": journal.version = 2; expected = "Payment presentation owner"; break;
                case "digest": expected = "Payment presentation digest"; break;
                case "kind": entry.kind = "unknown"; expected = "Payment presentation kind"; break;
                case "creditdigest": credit.digest = new string('0', 64); entry.credit = JsonUtility.ToJson(credit);
                    expected = "PC credit journal failed validation"; break;
                case "phase": credit.phase = 1; credit.digest = PcCreditDigest(credit); entry.credit = JsonUtility.ToJson(credit);
                    expected = "Payment presentation source receipt"; break;
                case "profile": credit.profile = "RouletteDeluxeSZ89"; credit.digest = PcCreditDigest(credit); entry.credit = JsonUtility.ToJson(credit);
                    expected = "Payment presentation source receipt"; break;
                case "anchor": credit.anchor = "other-round"; credit.digest = PcCreditDigest(credit); entry.credit = JsonUtility.ToJson(credit);
                    expected = "Payment presentation source receipt"; break;
                case "duplicate": journal.entries.Add(new PaymentPresentationEntry { kind = entry.kind, credit = entry.credit, receipt = entry.receipt });
                    expected = "Payment presentation source receipt"; break;
                case "receipt": byte[] bytes = Convert.FromBase64String(entry.receipt); bytes[bytes.Length - 1] ^= 1;
                    entry.receipt = Convert.ToBase64String(bytes); expected = "Receipt checksum"; break;
                case "predecessor": entry.receipt = PcVirtualPaymentPresentationReceipt.Begin(PaymentIdentity(credit), true,
                    credit.collectedCents, credit.beforeDenomination, new OriginalPaymentMessageState(0, 0, 1, false)).Complete().Encode();
                    expected = "Receipt ownership"; break;
                case "completion": entry.receipt = PcVirtualPaymentPresentationReceipt.Begin(PaymentIdentity(credit), true,
                    credit.collectedCents, credit.beforeDenomination, new OriginalPaymentMessageState()).Encode();
                    expected = "Payment presentation completion"; break;
                default: throw new Exception("Unknown chain mutation " + mutation);
            }
            journal.digest = "";
            journal.digest = mutation == "digest" ? new string('0', 64) : PaymentTextHash(JsonUtility.ToJson(journal));
            string corrupt = JsonUtility.ToJson(journal);
            PaymentSmokeRequire(corrupt != valid && PlayerPrefs.GetString(PcCreditKey) == financial,
                "only presentation candidate changed"); checks++;
            SetPcGameAppSuspended(true, "payment-chain-write");
            PlayerPrefs.SetString(PcPaymentPresentationKey, corrupt); PlayerPrefs.Save();
            PaymentSmokeRequire(PlayerPrefs.GetString(PcPaymentPresentationKey) == corrupt &&
                PlayerPrefs.GetString(PcCreditKey) == financial && recordedPockets.Count == 0 && session.Rounds == 0,
                "one-way corruption durable without financial mutation or draw"); checks++;
            SavePaymentChainSmokeReport(new PaymentChainSmokeReport { phase = phase, mutation = mutation, expectedRejection = expected,
                key = PcPaymentPresentationKey, financialKey = PcCreditKey, validJournal = valid, corruptJournal = corrupt,
                financialJournal = financial, journalSha256 = PaymentTextHash(corrupt), financialSha256 = PaymentTextHash(financial),
                checks = checks, credit = session.Credit, rounds = session.Rounds, pockets = recordedPockets.Count,
                sessionPublished = true, status = status });
            Application.Quit(0);
        }
        private IEnumerator PcCollectMeterBadSmoke(string phase)
        {
            PaymentSmokeRequire(recordedHistoryKey.Contains("PcPaymentMessageTestMeterBad") && !temporaryHistory,
                "owned corrupt collect meter scope");
            int checks = 1;
            if (phase.EndsWith("_cold", StringComparison.Ordinal))
            {
                var prior = JsonUtility.FromJson<PaymentChainSmokeReport>(File.ReadAllText(PaymentSmokeArg("-roulettePcPaymentMessagePrevious")));
                PaymentSmokeRequire(prior.phase == phase.Substring(0, phase.Length - 5) + "_write" && prior.checks == 5,
                    "cold meter proof belongs to writer"); checks++;
                PaymentSmokeRequire(session == null && status == "Configuration error: " + prior.expectedRejection,
                    "startup rejects exact corrupt meter before session publication"); checks++;
                PaymentSmokeRequire(PlayerPrefs.GetString(prior.key) == prior.corruptJournal &&
                    PlayerPrefs.GetString(prior.financialKey) == prior.financialJournal,
                    "rejection preserves meter and financial journals"); checks++;
                PaymentSmokeRequire(recordedPockets.Count == 0 && !spinning && !PcCollectMeterVisible &&
                    pcCollectMeterRecord == null && pcCollectMeterCredits == 0 && !pcPaymentMessagesActive,
                    "rejection publishes no meter, payment or draw"); checks++;
                prior.phase = phase; prior.checks = checks; prior.status = status; prior.sessionPublished = false;
                SavePaymentChainSmokeReport(prior); Application.Quit(0); yield break;
            }
            PaymentSmokeRequire(phase.EndsWith("_write", StringComparison.Ordinal) && session != null &&
                session.Credit == 1000 && session.Rounds == 0 && !PlayerPrefs.HasKey(PcCollectMeterKey),
                "fresh corrupt meter writer"); checks++;
            HandlePcCashoutRelease(); ConfirmPcCollect(true);
            for (int frame = 0; frame < 20 && pcCollectPending; frame++) yield return null;
            PaymentSmokeRequire(!pcCollectPending && session.Credit == 0 && pcCollectMeterCredits == 1000 &&
                ReadPcCreditEvent().phase == 3, "actual collect completes before meter corruption"); checks++;
            string valid = PlayerPrefs.GetString(PcCollectMeterKey), financial = PlayerPrefs.GetString(PcCreditKey);
            var record = JsonUtility.FromJson<PcCollectMeterRecord>(valid);
            var credit = ParseValidatedPcCreditEvent(record.credit);
            string mutation = phase.Substring(9, phase.Length - 15), expected;
            switch (mutation)
            {
                case "owner": record.owner = new string('0', 64); expected = "Collect meter owner"; break;
                case "version": record.version = 2; expected = "Collect meter owner"; break;
                case "digest": expected = "Collect meter digest"; break;
                case "creditdigest": credit.digest = new string('0', 64); expected = "PC credit journal failed validation"; break;
                case "phase": credit.phase = 3; expected = "Collect meter source receipt"; break;
                case "profile": credit.profile = "RouletteDeluxeSZ89"; expected = "Collect meter source receipt"; break;
                case "anchor": credit.anchor = "other-round"; expected = "Collect meter source receipt"; break;
                case "baseline": credit.baseCredit++; expected = "Collect meter financial baseline"; break;
                case "basedenom": credit.baseDenomination = 25; expected = "Collect meter financial baseline"; break;
                case "baseremainder": credit.baseDenomination = 25; credit.baseRemainderCents = 1;
                    expected = "Collect meter financial baseline"; break;
                case "empty": record.credit = ""; expected = "Collect meter empty active source"; break;
                default: throw new Exception("Unknown collect meter mutation " + mutation);
            }
            if (mutation != "creditdigest") credit.digest = PcCreditDigest(credit);
            if (mutation != "empty") record.credit = JsonUtility.ToJson(credit);
            record.digest = "";
            record.digest = mutation == "digest" ? new string('0', 64) : PaymentTextHash(JsonUtility.ToJson(record));
            string corrupt = JsonUtility.ToJson(record);
            PaymentSmokeRequire(corrupt != valid && PlayerPrefs.GetString(PcCreditKey) == financial,
                "only meter candidate changed"); checks++;
            SetPcGameAppSuspended(true, "collect-meter-corruption");
            PlayerPrefs.SetString(PcCollectMeterKey, corrupt); PlayerPrefs.Save();
            PaymentSmokeRequire(PlayerPrefs.GetString(PcCollectMeterKey) == corrupt &&
                PlayerPrefs.GetString(PcCreditKey) == financial && recordedPockets.Count == 0 && session.Rounds == 0,
                "one-way corruption persists without financial mutation or draw"); checks++;
            SavePaymentChainSmokeReport(new PaymentChainSmokeReport { phase = phase, mutation = mutation,
                expectedRejection = expected, key = PcCollectMeterKey, financialKey = PcCreditKey,
                validJournal = valid, corruptJournal = corrupt, financialJournal = financial,
                journalSha256 = PaymentTextHash(corrupt), financialSha256 = PaymentTextHash(financial),
                checks = checks, credit = session.Credit, rounds = session.Rounds, pockets = recordedPockets.Count,
                sessionPublished = true, status = status });
            Application.Quit(0);
        }
        [Serializable] private sealed class CollectWideGeometry
        { public int[] widths; public int x, y, height, totalWidth; }
        [Serializable] private sealed class CollectWideCase
        { public long amount; public string credits, cash; public int fontIndex;
          public CollectWideGeometry creditGeometry, cashGeometry; }
        [Serializable] private sealed class CollectWideReference
        { public CollectWideCase[] cases; }
        private int VerifyCollectWideGlyphs(PictureLabel label, string text, OriginalBitmapFont font,
            CollectWideGeometry expected, Vector2 position, Vector2 size)
        {
            PaymentSmokeRequire(label.Value == text && label.Visible && label.SelectedFont == font &&
                label.Root.anchoredPosition == position && label.Root.sizeDelta == size,
                "wide collect text/source bounds/font choice");
            int index = 0, x = expected.x;
            foreach (Transform child in label.Root)
            {
                if (!child.gameObject.activeSelf) continue;
                var glyph = child.GetComponent<UnityEngine.UI.RawImage>();
                PaymentSmokeRequire(index < expected.widths.Length && glyph != null && glyph.texture == font.Texture &&
                    !glyph.raycastTarget && glyph.rectTransform.sizeDelta == new Vector2(expected.widths[index], expected.height) &&
                    glyph.rectTransform.anchoredPosition == new Vector2(x, -expected.y),
                    "wide collect native glyph geometry");
                Rect uv; int width;
                PaymentSmokeRequire(font.TryGetGlyph(text[index], out uv, out width) && glyph.uvRect == uv &&
                    width == expected.widths[index], "wide collect original glyph UV");
                x += expected.widths[index++];
            }
            PaymentSmokeRequire(index == text.Length && index == expected.widths.Length &&
                x - expected.x == expected.totalWidth, "all wide collect glyphs retained");
            return 2 + index * 2;
        }
        private IEnumerator PcCollectWideSmoke(string phase)
        {
            PaymentSmokeRequire(recordedHistoryKey.Contains("PcPaymentMessageTestMeterWide") && session != null &&
                !temporaryHistory && !auNationalStandardsEnabled && recordedPockets.Count == 0 && session.Rounds == 0,
                "owned wide collect scope");
            int checks = 1;
            int caseIndex = int.Parse(PaymentSmokeArg("-roulettePcCollectWideIndex"));
            var reference = JsonUtility.FromJson<CollectWideReference>(File.ReadAllText(PaymentSmokeArg("-roulettePcCollectWideReference")));
            var expected = reference.cases[caseIndex];
            if (phase == "meterwide_live")
            {
                PaymentSmokeRequire(session.Credit == 1000 && !PlayerPrefs.HasKey(PcCollectMeterKey), "fresh wide writer"); checks++;
                AddPcVirtualCredits(expected.amount - 1000);
                PaymentSmokeRequire(session.Credit == expected.amount && session.Denomination == 1, "exact wide wallet before collect"); checks++;
                HandlePcCashoutRelease(); ConfirmPcCollect(true);
                for (int i = 0; i < 20 && pcCollectPending; i++) yield return null;
            }
            else
            {
                PaymentSmokeRequire(phase == "meterwide_cold", "known wide cold phase"); checks++;
                string before = PlayerPrefs.GetString(PcCollectMeterKey), financial = PlayerPrefs.GetString(PcCreditKey);
                RestorePcCollectMeterBeforeCredit(ReadPcCreditEvent());
                PaymentSmokeRequire(before == PlayerPrefs.GetString(PcCollectMeterKey) && financial == PlayerPrefs.GetString(PcCreditKey),
                    "wide cold admission idempotent"); checks++;
            }
            PaymentSmokeRequire(session.Credit == 0 && session.Denomination == 1 && !pcCollectPending &&
                ReadPcCreditEvent().phase == 3 && pcCollectMeterCredits == expected.amount && PcCollectMeterVisible,
                "wide financial completion and display amount"); checks++;
            RefreshPortraitUi(); yield return null; RefreshPortraitUi();
            checks += VerifyCollectWideGlyphs(uiPaid, expected.credits, footerPaidMeterFonts[expected.fontIndex],
                expected.creditGeometry, new Vector2(782, -1833), new Vector2(252, 54));
            checks += VerifyCollectWideGlyphs(uiCashPaid, expected.cash, footerCashPaidFont,
                expected.cashGeometry, new Vector2(830, -1815), new Vector2(204, 19));
            PaymentSmokeRequire(uiWinTitle.Value == "COLLECT" && uiWinTitle.GlyphCount == 7 &&
                footerMoviePhase == FooterMoviePhase.Hidden && !uiFooterMovie.gameObject.activeSelf &&
                recordedPockets.Count == 0 && session.Rounds == 0, "wide title/no animation/no draw"); checks++;
            ScreenCapture.CaptureScreenshot(PaymentSmokeArg("-roulettePcPaymentMessageOutput") + ".png");
            yield return new WaitForEndOfFrame(); SaveCollectMeterSmokeReport(phase, checks); Application.Quit(0);
        }
        [Serializable] private sealed class CollectHistorySmokeReport
        {
            public string phase, title, creditsText, cashText, meterJournal, financialJournal;
            public int checks, historyCount, pockets;
            public long credit, rounds;
            public bool au, collectVisible;
        }
        private IEnumerator PcCollectHistorySmoke(string phase)
        {
            PaymentSmokeRequire(phase == "meterhistory_cold" && recordedHistoryKey.Contains("PcPaymentMessageTestMeter") &&
                session != null && !temporaryHistory && session.Rounds == 0 && recordedPockets.Count == 0,
                "owned existing paid history observer");
            int checks = 1;
            string meter = PlayerPrefs.GetString(PcCollectMeterKey), financial = PlayerPrefs.GetString(PcCreditKey);
            long credit = session.Credit;
            string title = PaymentSmokeArg("-roulettePcCollectHistoryTitle"), digits = PaymentSmokeArg("-roulettePcCollectHistoryDigits"),
                cash = PaymentSmokeArg("-roulettePcCollectHistoryCash");
            SetHistoryView(true); RefreshPortraitUi(); yield return null; RefreshPortraitUi();
            PaymentSmokeRequire(historyView && historyRoot.gameObject.activeSelf && historyWinTitle.Value == title &&
                historyPaid.Value == digits && historyCashPaid.Value == cash && historyCashPaid.Visible &&
                historyPaid.GlyphCount == digits.Length && historyWinTitle.GlyphCount == title.Length,
                "actual history title/credit/cash glyphs"); checks++;
            PaymentSmokeRequire(uiWinTitle.Value == title && uiPaid.Value == (PcCollectMeterVisible ? digits : auNationalStandardsEnabled ? "0" : "") &&
                historyPaid.SelectedFont == uiPaid.SelectedFont,
                "history retains current main panel values/font"); checks++;
            RefreshHistoryPageUi(); yield return null; RefreshHistoryPageUi();
            PaymentSmokeRequire(historyPaid.Value == digits && historyCashPaid.Value == cash && historyWinTitle.Value == title,
                "same-page refresh retains current collect"); checks++;
            string path = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            ScreenCapture.CaptureScreenshot(path + ".png"); yield return new WaitForEndOfFrame();
            SetHistoryView(false); RefreshPortraitUi(); yield return null; RefreshPortraitUi();
            PaymentSmokeRequire(!historyView && !historyRoot.gameObject.activeSelf && session.Credit == credit &&
                PlayerPrefs.GetString(PcCollectMeterKey) == meter && PlayerPrefs.GetString(PcCreditKey) == financial,
                "return from history preserves wallet and both journals"); checks++;
            PaymentSmokeRequire(recordedPockets.Count == 0 && session.Rounds == 0 &&
                (!PcCollectMeterVisible || (footerMoviePhase == FooterMoviePhase.Hidden && !uiFooterMovie.gameObject.activeSelf)),
                "history observer has no draw or movie resurrection"); checks++;
            if (File.Exists(path)) throw new Exception("Frozen history collect output exists");
            File.WriteAllText(path, JsonUtility.ToJson(new CollectHistorySmokeReport { phase = phase, title = title,
                creditsText = digits, cashText = cash, meterJournal = meter, financialJournal = financial,
                checks = checks, historyCount = CommittedHistoryCount(), pockets = recordedPockets.Count,
                credit = session.Credit, rounds = session.Rounds, au = auNationalStandardsEnabled, collectVisible = PcCollectMeterVisible }, true));
            Debug.Log("ROULETTE_PC_COLLECT_HISTORY_PASSED checks=" + checks); Application.Quit(0);
        }
        [Serializable] private sealed class CollectNavReport
        {
            public string phase, meterJournal, financialJournal, archivedPages, pocketsJournal, rngPath, rngSha256;
            public int checks, historyCount, pockets, endCount;
            public long credit, rounds, collected;
            public long denomination, collectedCents, remainderBeforeCollect, returned;
            public double meterCredits;
            public bool au, winningFraction;
        }
        private bool CollectNavColdArchiveEqual(string before, string after)
        {
            var a = JsonUtility.FromJson<RecordedRoundArchiveV3>(before);
            var b = JsonUtility.FromJson<RecordedRoundArchiveV3>(after);
            if (a == null || b == null || a.lifecycle == null || b.lifecycle == null ||
                a.lifecycle.idleEntry == null || b.lifecycle.idleEntry == null ||
                b.lifecycle.idleEntry.creditBefore != 0 || b.lifecycle.idleEntry.creditAfter != 0 ||
                b.lifecycle.idleEntry.state != PcIdleState.Complete ||
                b.lifecycle.idleEntry.consumedCollectEventId != ReadPcCreditEvent().id) return false;
            // Startup creates a new legitimate Idle execution identity.
            // Every other archived field must remain exactly equal.
            a.lifecycle.idleEntry = null; b.lifecycle.idleEntry = null;
            return JsonUtility.ToJson(a) == JsonUtility.ToJson(b);
        }
        private IEnumerator PcCollectNavSmoke(string phase)
        {
            PaymentSmokeRequire(recordedHistoryKey.Contains("PcPaymentMessageTestMeterNav") && session is RouletteSession &&
                !temporaryHistory, "owned committed history collect scope");
            int checks = 1; long collected;
            bool winningFraction = Array.IndexOf(Environment.GetCommandLineArgs(), "-roulettePcCollectWinningFraction") >= 0;
            if (winningFraction) PaymentSmokeRequire(recordedHistoryKey.Contains("PcPaymentMessageTestMeterNavWinFrac"),
                "winning fractional navigation owns isolated scope");
            if (phase == "meternav_write")
            {
                PaymentSmokeRequire(session.Credit == 1000 && session.Rounds == 0 && recordedPockets.Count == 0 &&
                    !PlayerPrefs.HasKey(PcCreditKey) && !PlayerPrefs.HasKey(recordedHistoryKey + ".RoundDetails.v1"),
                    "fresh navigation producer"); checks++;
                // A test-only operator setting keeps these ordinary rounds
                // independent of the separately qualified Double feature.
                session.DoubleEnabled = false;
                if (winningFraction)
                {
                    AddPcVirtualCredits(1);
                    for (int i = 0; i < session.Profile.Denominations.Length && session.Denomination != 25; i++) RequestPcDenominationChange();
                    PaymentSmokeRequire(session.Denomination == 25 && session.Credit == 40 &&
                        ((RouletteSession)session).MoneyRemainderCents == 1, "normal deposit and denomination preserve one cent"); checks++;
                }
                for (int round = 0; round < 2; round++)
                {
                    float until = Time.realtimeSinceStartup + 100f;
                    while (SourceRoundBlocksOrdinaryInput || ResultDisplayActive || spinning || winRackActive ||
                        configuredAutoPlayCheckPending || Time.frameCount == rackSnapFrame)
                    { PaymentSmokeRequire(Time.realtimeSinceStartup < until, "ordinary round becomes idle"); yield return null; }
                    if (winningFraction)
                    {
                        if (session.ResultPhase) session.NextRound();
                        session.Clear();
                        for (int pocket = 0; pocket <= 36; pocket++) session.PlaceBet(100 + pocket, 1);
                        PaymentSmokeRequire(session.TotalBet == 37, "all straight spots admitted by normal betting"); checks++;
                    }
                    else session.PlaceBet(100 + round, 1);
                    SaveStagedBetsIfChanged(); BeginSpin();
                    PaymentSmokeRequire(spinning && session.SpinPending, "normal spin commits actual RNG draw"); checks++;
                    until = Time.realtimeSinceStartup + 100f;
                    while (spinning || session.SpinPending || SourceRoundBlocksOrdinaryInput || ResultDisplayActive || winRackActive)
                    { PaymentSmokeRequire(Time.realtimeSinceStartup < until, "normal round reaches source End"); yield return null; }
                    PaymentSmokeRequire(session.Rounds == round + 1 && recordedPockets.Count == round + 1 &&
                        CommittedHistoryCount() == round + 1, "actual round history committed"); checks++;
                    if (winningFraction)
                    {
                        var settled = recordedRoundPages[0];
                        PaymentSmokeRequire(settled.wager == 37 && settled.returned == 36 && settled.denomination == 25 &&
                            session.Credit == 39 - round && ((RouletteSession)session).MoneyRemainderCents == 1,
                            "real winning round preserves exact fractional wallet"); checks++;
                    }
                }
                collected = session.Credit;
                HandlePcCashoutRelease(); ConfirmPcCollect(true);
                for (int i = 0; i < 20 && pcCollectPending; i++) yield return null;
                PaymentSmokeRequire(!pcCollectPending && session.Credit == 0 && ReadPcCreditEvent().phase == 3 &&
                    pcCollectMeterCredits == (winningFraction ? 38.04 : collected), "one actual collect after two committed rounds"); checks++;
            }
            else
            {
                PaymentSmokeRequire(phase == "meternav_cold" || phase == "meternav_observe", "known navigation observer"); checks++;
                if (phase == "meternav_observe")
                {
                    var paid = ReadPcCreditEvent();
                    PaymentSmokeRequire(paid.kind == "collect" && paid.phase == 3 && paid.collected == 997 &&
                        session.Credit == 0, "retained failed producer is already paid"); checks++;
                    collected = paid.collected;
                }
                else
                {
                var prior = JsonUtility.FromJson<CollectNavReport>(File.ReadAllText(PaymentSmokeArg("-roulettePcPaymentMessagePrevious")));
                PaymentSmokeRequire((prior.phase == "meternav_write" || prior.phase == "meternav_observe") && prior.au == auNationalStandardsEnabled &&
                    prior.winningFraction == winningFraction &&
                    prior.financialJournal == PlayerPrefs.GetString(PcCreditKey) && prior.meterJournal == PlayerPrefs.GetString(PcCollectMeterKey) &&
                    CollectNavColdArchiveEqual(prior.archivedPages, PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1")) &&
                    prior.pocketsJournal == PlayerPrefs.GetString(recordedHistoryKey) &&
                    prior.rngSha256 == PaymentTextHash(Convert.ToBase64String(File.ReadAllBytes(PcRngStateStore.PathForCurrentProcess))),
                    "cold producer financial/history/RNG identity retained"); checks++;
                collected = prior.collected;
                }
            }
            PaymentSmokeRequire(session.Rounds == 2 && recordedPockets.Count == 2 && CommittedHistoryCount() == 2 &&
                session.Credit == 0 && pcCollectMeterCredits == (winningFraction ? 38.04 : collected), "two settled pages and current paid wallet"); checks++;
            if (winningFraction)
            {
                var paid = ReadPcCreditEvent();
                PaymentSmokeRequire(collected == 38 && session.Denomination == 25 && session.Returned == 72 && session.Wins == 2 &&
                    paid.collectedCents == 951 && paid.beforeRemainderCents == 1 && paid.beforeDenomination == 25 &&
                    recordedRoundPages.TrueForAll(item => item.wager == 37 && item.returned == 36 && item.denomination == 25),
                    "both committed winning pages and exact collect cents retained"); checks++;
            }
            float idleUntil = Time.realtimeSinceStartup + 30f;
            while (configuredAutoPlayCheckPending || SourceRoundBlocksOrdinaryInput)
            { PaymentSmokeRequire(Time.realtimeSinceStartup < idleUntil, "post-collect Idle service completes before snapshot"); yield return null; }
            yield return null;
            string meter = PlayerPrefs.GetString(PcCollectMeterKey), financial = PlayerPrefs.GetString(PcCreditKey),
                archived = PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1"), pocketJournal = PlayerPrefs.GetString(recordedHistoryKey);
            byte[] rng = File.ReadAllBytes(PcRngStateStore.PathForCurrentProcess);
            string digits = auNationalStandardsEnabled ? "0" : OriginalCollectMeterFormatting.PanelCredits(pcCollectMeterCredits);
            string cash = auNationalStandardsEnabled ? "RM0.00" : OriginalCollectMeterFormatting.PanelCash(pcCollectMeterCredits,
                session.Denomination, OriginalMonetaryLocales.Load(OriginalMonetaryLocales.PcVirtualLocaleName));
            SetHistoryView(true); RefreshPortraitUi(); yield return null; RefreshPortraitUi();
            string path = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            for (int page = 0; page < 3; page++)
            {
                int expectedPage = page == 1 ? 1 : 0;
                PaymentSmokeRequire(historyView && historyPage == expectedPage && historyPageNumber.Value ==
                    (expectedPage == 0 ? " 1 of  2" : " 2 of  2") && historyResultNumber.Value ==
                    recordedRoundPages[HistoryRecordIndex(expectedPage)].pocket.ToString() && historyPaid.Value == digits &&
                    historyCashPaid.Value == cash && historyWinTitle.Value == (auNationalStandardsEnabled ? "WIN" : "COLLECT") &&
                    historyCredit.Value == "0" && historyPaid.GlyphCount == digits.Length,
                    "selected archived result retains current collect panel"); checks++;
                if (page < 2)
                {
                    ScreenCapture.CaptureScreenshot(path + ".page" + expectedPage + ".png"); yield return new WaitForEndOfFrame();
                    var button = page == 0 ? historyPrevious : historyNext;
                    var pointer = PressThroughUiRaycast(button); ReleaseUiPointer(button, pointer);
                    yield return null; RefreshPortraitUi(); yield return null; RefreshPortraitUi();
                }
            }
            var returnPointer = PressThroughUiRaycast(historyGame); ReleaseUiPointer(historyGame, returnPointer);
            yield return null; RefreshPortraitUi(); yield return null; RefreshPortraitUi();
            PaymentSmokeRequire(!historyView && !historyRoot.gameObject.activeSelf && uiWinTitle.Value ==
                (auNationalStandardsEnabled ? "WIN" : "COLLECT") && uiPaid.Value == digits && uiCashPaid.Value == cash,
                "real Return button restores current main panel"); checks++;
            PaymentSmokeRequire(session.Credit == 0 && session.Rounds == 2 && recordedPockets.Count == 2 &&
                meter == PlayerPrefs.GetString(PcCollectMeterKey) && financial == PlayerPrefs.GetString(PcCreditKey) &&
                archived == PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1") &&
                pocketJournal == PlayerPrefs.GetString(recordedHistoryKey) &&
                Convert.ToBase64String(rng) == Convert.ToBase64String(File.ReadAllBytes(PcRngStateStore.PathForCurrentProcess)),
                "navigation changes no financial/history/outcome RNG state"); checks++;
            if (File.Exists(path)) throw new Exception("Frozen collect navigation report exists");
            File.WriteAllText(path, JsonUtility.ToJson(new CollectNavReport { phase = phase, checks = checks,
                meterJournal = meter, financialJournal = financial, archivedPages = archived, pocketsJournal = pocketJournal,
                rngPath = PcRngStateStore.PathForCurrentProcess, rngSha256 = PaymentTextHash(Convert.ToBase64String(rng)),
                credit = session.Credit, rounds = session.Rounds, collected = collected, pockets = recordedPockets.Count,
                historyCount = CommittedHistoryCount(), endCount = sourceRoundEndCount, au = auNationalStandardsEnabled,
                winningFraction = winningFraction, denomination = session.Denomination, meterCredits = pcCollectMeterCredits,
                collectedCents = ReadPcCreditEvent().collectedCents, remainderBeforeCollect = ReadPcCreditEvent().beforeRemainderCents,
                returned = session.Returned }, true));
            Debug.Log("ROULETTE_PC_COLLECT_NAV_PASSED phase=" + phase + " checks=" + checks); Application.Quit(0);
        }
        [Serializable] private sealed class PaymentSmokeReport
        {
            public string phase, journal, journalSha256, creditDigest, text;
            public int checks, glyphs, pockets, entries;
            public long credit, rounds, denomination, bet;
            public double collect, totalPaid;
            public bool pending;
        }
        private string PaymentSmokeArg(string name)
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, name);
            if (at < 0 || at + 1 >= args.Length) throw new Exception("Missing payment smoke argument " + name);
            return args[at + 1];
        }
        private void PaymentSmokeRequire(bool value, string message)
        { if (!value) throw new Exception("Payment message smoke: " + message); }
        private void SavePaymentSmokeReport(string phase, int checks)
        {
            string raw = PlayerPrefs.GetString(PcPaymentPresentationKey, "");
            var item = ReadPcCreditEvent();
            var report = new PaymentSmokeReport { phase = phase, checks = checks, journal = raw,
                journalSha256 = PaymentTextHash(raw), creditDigest = item == null ? "" : item.digest,
                credit = session.Credit, collect = pcPaymentMessageState.Collect, totalPaid = pcPaymentMessageState.TotalPaid,
                pending = pcPaymentMessageState.CollectPending, text = uiGameOver.Value, glyphs = uiGameOver.GlyphCount,
                rounds = session.Rounds, pockets = recordedPockets.Count, denomination = session.Denomination,
                bet = session.TotalBet, entries = pcPaymentPresentationJournal.entries.Count };
            string path = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            if (File.Exists(path)) throw new Exception("Frozen payment smoke output exists");
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            Debug.Log("ROULETTE_PC_PAYMENT_MESSAGE_PASSED phase=" + phase + " checks=" + checks);
        }
        private IEnumerator PcPaymentMessageSmoke()
        {
            var body = PcPaymentMessageSmokeBody();
            while (true)
            {
                bool moved;
                try { moved = body.MoveNext(); }
                catch (Exception error) { Debug.LogError("ROULETTE_PC_PAYMENT_MESSAGE_FAILED " + error); Application.Quit(8); yield break; }
                if (!moved) yield break;
                yield return body.Current;
            }
        }
        private IEnumerator PcPaymentMessageSmokeBody()
        {
            yield return null;
            string phase = PaymentSmokeArg("-roulettePcPaymentMessagePhase");
            if (phase.StartsWith("successor_", StringComparison.Ordinal))
            {
                var successor = PcPaymentSuccessorSmoke(phase);
                while (successor.MoveNext()) yield return successor.Current;
                yield break;
            }
            if (phase == "meterdeck_live")
            {
                var deck = CollectRaycastSmoke(true);
                while (deck.MoveNext()) yield return deck.Current;
                yield break;
            }
            if (phase.StartsWith("meternav_", StringComparison.Ordinal))
            {
                var nav = PcCollectNavSmoke(phase);
                while (nav.MoveNext()) yield return nav.Current;
                yield break;
            }
            if (phase.StartsWith("meterhistory_", StringComparison.Ordinal))
            {
                var history = PcCollectHistorySmoke(phase);
                while (history.MoveNext()) yield return history.Current;
                yield break;
            }
            if (phase.StartsWith("meterwide_", StringComparison.Ordinal))
            {
                var wide = PcCollectWideSmoke(phase);
                while (wide.MoveNext()) yield return wide.Current;
                yield break;
            }
            if (phase.StartsWith("meterbad_", StringComparison.Ordinal))
            {
                var bad = PcCollectMeterBadSmoke(phase);
                while (bad.MoveNext()) yield return bad.Current;
                yield break;
            }
            if (phase.StartsWith("meter_", StringComparison.Ordinal))
            {
                var meter = PcCollectMeterSmoke(phase);
                while (meter.MoveNext()) yield return meter.Current;
                yield break;
            }
            if (phase.StartsWith("chain_", StringComparison.Ordinal))
            {
                var chain = PcPaymentChainSmoke(phase);
                while (chain.MoveNext()) yield return chain.Current;
                yield break;
            }
            PaymentSmokeRequire(auNationalStandardsEnabled && recordedHistoryKey.Contains("PcPaymentMessageTest") &&
                session != null && uiGameOver != null && !temporaryHistory, "requires owned AU formal scene");
            int checks = 1;
            double expectedTotal = 1000; string expectedText = "COLLECT RM10.00";
            long expectedDenomination = 1;
            if (phase == "multi" || phase == "multicold") { expectedTotal = 2000; expectedText = "COLLECT RM10.00 (TOTAL PAID RM20.00)"; }
            if (phase == "denom" || phase == "denomcold") { expectedTotal = 10; expectedDenomination = 100; }
            if (phase == "long" || phase == "longcold") { expectedTotal = 9007199254740991d; expectedText = "COLLECT RM90,071,992,547,409.91"; }
            if (phase == "reset" || phase == "resetcold")
            {
                if (phase == "reset")
                {
                    PaymentSmokeRequire(session.Credit == 0 && pcPaymentMessageState.TotalPaid == 2000, "reset starts from two collects"); checks++;
                    AddPcVirtualCredits(100);
                    yield return null; yield return null;
                    string before = PlayerPrefs.GetString(PcPaymentPresentationKey);
                    PaymentSmokeRequire(!PrepareSourceSpin(false) && PlayerPrefs.GetString(PcPaymentPresentationKey) == before &&
                        pcPaymentMessageState.TotalPaid == 2000, "rejected zero-bet start retains payment fields"); checks++;
                    session.PlaceBet(100, 1);
                    PaymentSmokeRequire(PrepareSourceSpin(false), "positive source StartGame admission"); checks++;
                }
                PaymentSmokeRequire(session.Credit == 100 && !pcPaymentMessagesActive && pcPaymentMessageState.Collect == 0 &&
                    pcPaymentMessageState.Cancel == 0 && pcPaymentMessageState.TotalPaid == 0 && !pcPaymentMessageState.CollectPending &&
                    pcPaymentPresentationJournal.entries.Count == 1 && pcPaymentPresentationJournal.entries[0].kind == "start",
                    "admitted start retires chain and restores zero fields"); checks++;
                RefreshPortraitUi();
                PaymentSmokeRequire(recordedPockets.Count == 0 && session.Rounds == 0, "start admission fixture does not commit a draw"); checks++;
                SavePaymentSmokeReport(phase, checks); Application.Quit(0); yield break;
            }
            if (phase == "live" || phase == "cut" || phase == "multi" || phase == "denom" || phase == "long")
            {
                PaymentSmokeRequire(session.Credit == 1000 && recordedPockets.Count == 0 &&
                    !PlayerPrefs.HasKey(PcPaymentPresentationKey), "fresh isolated scope"); checks++;
                if (phase == "denom")
                {
                    PaymentSmokeRequire(Array.IndexOf(session.Profile.Denominations, 100) >= 0, "source denomination 100 is admitted"); checks++;
                    for (int changes = 0; changes < session.Profile.Denominations.Length && session.Denomination != 100; changes++)
                        RequestPcDenominationChange();
                    PaymentSmokeRequire(session.Denomination == 100 && session.Credit == 10 && ((RouletteSession)session).MoneyCents == 1000,
                        "denomination changes preserve full money"); checks++;
                }
                if (phase == "long")
                {
                    AddPcVirtualCredits(9007199254740991L - 1000);
                    PaymentSmokeRequire(session.Credit == 9007199254740991L, "owned wide virtual credit"); checks++;
                }
                if (phase == "cut")
                    pcPaymentCreditSavedTestHook = item => {
                        if (item.kind != "collect" || item.phase != 3) return;
                        PaymentSmokeRequire(session.Credit == 0 && pcPaymentMessageState.CollectPending &&
                            pcPaymentMessageState.TotalPaid == 0, "cut after financial completion before presentation completion");
                        pcPaymentSmokeCut = true; SavePaymentSmokeReport(phase, checks + 1); Application.Quit(0);
                        throw new OperationCanceledException("Owned payment presentation cut fixture");
                    };
                HandlePcCashoutRelease(); ConfirmPcCollect(true);
                PaymentSmokeRequire(pcCollectPending && ReadPcCreditEvent().phase == 1, "separate accepted request"); checks++;
                for (int frame = 0; frame < 20 && pcCollectPending && !pcPaymentSmokeCut; frame++) yield return null;
                if (pcPaymentSmokeCut) yield break;
                PaymentSmokeRequire(!pcCollectPending && session.Credit == 0 && pcPaymentMessageState.TotalPaid == (phase == "multi" ? 1000 : expectedTotal) &&
                    !pcPaymentMessageState.CollectPending, "completed once"); checks++;
                if (phase == "multi")
                {
                    AddPcVirtualCredits(1000);
                    PaymentSmokeRequire(session.Credit == 1000 && pcPaymentMessageState.TotalPaid == 1000, "deposit does not reset payment totals"); checks++;
                    HandlePcCashoutRelease(); ConfirmPcCollect(true);
                    for (int frame = 0; frame < 20 && pcCollectPending; frame++) yield return null;
                    PaymentSmokeRequire(session.Credit == 0 && !pcCollectPending && pcPaymentMessageState.TotalPaid == 2000 &&
                        pcPaymentPresentationJournal.entries.Count == 2, "second collection appends once"); checks++;
                }
            }
            else if (phase == "cold" || phase == "recover" || phase == "multicold" || phase == "denomcold" || phase == "longcold")
            {
                PaymentSmokeRequire(session.Credit == 0 && pcPaymentMessageState.TotalPaid == expectedTotal &&
                    !pcPaymentMessageState.CollectPending && ReadPcCreditEvent().phase == 3, "cold completed fields"); checks++;
                string before = PlayerPrefs.GetString(PcPaymentPresentationKey);
                CompleteRestoredPcPaymentPresentation(ReadPcCreditEvent());
                PaymentSmokeRequire(PlayerPrefs.GetString(PcPaymentPresentationKey) == before, "cold completion not repeated"); checks++;
            }
            else throw new Exception("Unknown payment smoke phase");
            PaymentSmokeRequire(session.Denomination == expectedDenomination, "live/cold denomination precedes formatting"); checks++;
            float until = Time.realtimeSinceStartup + 12f;
            while (Time.realtimeSinceStartup < until && (uiGameOver.Value != expectedText || !uiGameOver.Visible)) yield return null;
            PaymentSmokeRequire(uiGameOver.Value == expectedText && uiGameOver.Visible && uiGameOver.GlyphCount == expectedText.Length &&
                pcPaymentFlashingLabel.MessageCount(2) == 4, "actual original glyphs and four slots"); checks++;
            // Source single-font ScaledLabel retains native size even when
            // the measured text exceeds its nominal 267px box. These widths
            // are independently summed from the frozen original glyph metrics.
            int expectedWidth = expectedText.Contains("TOTAL PAID") ? 295 : phase.StartsWith("long") ? 251 : 127;
            Vector3[] bounds = new Vector3[4], glyph = new Vector3[4]; uiGameOver.Root.GetWorldCorners(bounds);
            float left = float.PositiveInfinity, right = float.NegativeInfinity;
            float bottom = float.PositiveInfinity, top = float.NegativeInfinity;
            for (int i = 0; i < uiGameOver.Root.childCount; i++)
            {
                var child = uiGameOver.Root.GetChild(i) as RectTransform;
                if (!child.gameObject.activeSelf) continue;
                child.GetWorldCorners(glyph);
                left = Mathf.Min(left, glyph[0].x); right = Mathf.Max(right, glyph[2].x);
                bottom = Mathf.Min(bottom, glyph[0].y); top = Mathf.Max(top, glyph[2].y);
            }
            float scaleX = uiGameOver.Root.lossyScale.x, scaleY = uiGameOver.Root.lossyScale.y;
            PaymentSmokeRequire(Mathf.Abs((right - left) / scaleX - expectedWidth) < .02f &&
                Mathf.Abs((top - bottom) / scaleY - 21) < .02f &&
                Mathf.Abs(left - (bounds[0].x + (133 - expectedWidth / 2) * scaleX)) < .02f &&
                Mathf.Abs(top - (bounds[1].y - 4 * scaleY)) < .02f,
                "original glyph size and integer centre, including source overflow");
            checks++;
            string output = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            ScreenCapture.CaptureScreenshot(output + ".png");
            yield return new WaitForEndOfFrame();
            int cursor = pcPaymentFlashingLabel.Cursor; string text = uiGameOver.Value;
            ulong callbacks = pcPaymentFlashingLabel.Callbacks;
            SetPcGameAppSuspended(true, "payment-message-smoke");
            yield return new WaitForSecondsRealtime(2.9f);
            PaymentSmokeRequire(pcPaymentFlashingLabel.Callbacks >= callbacks + 2 && pcPaymentFlashingLabel.Cursor == cursor &&
                uiGameOver.Value == text && uiGameOver.Visible, "pause advances phase while keeping text and visibility"); checks++;
            SetPcGameAppSuspended(false, "payment-message-smoke");
            PaymentSmokeRequire(recordedPockets.Count == 0 && session.Rounds == 0 && session.Credit == 0,
                "no draw or wallet application from presentation"); checks++;
            SavePaymentSmokeReport(phase, checks); Application.Quit(0);
        }
        [Serializable] private sealed class PaymentSuccessorReport
        {
            public string phase, financialJournal, meterJournal, presentationJournal, archivedPages, pocketsJournal;
            public string rngPath, rngBase64, text, title;
            public int checks, pockets, historyCount;
            public long credit, rounds, denomination, returned;
        }
        private IEnumerator PcPaymentSuccessorSmoke(string phase)
        {
            PaymentSmokeRequire(auNationalStandardsEnabled && session is RouletteSession && !temporaryHistory &&
                recordedHistoryKey.Contains("PcPaymentMessageTestSuccessorAu1"), "owned AU successor scope");
            int checks = 1;
            if (phase == "successor_write")
            {
                PaymentSmokeRequire(session.Credit == 1000 && session.Rounds == 0 && recordedPockets.Count == 0 &&
                    !PlayerPrefs.HasKey(PcCreditKey) && !PlayerPrefs.HasKey(PcPaymentPresentationKey), "fresh successor writer"); checks++;
                session.DoubleEnabled = false;
                HandlePcCashoutRelease(); ConfirmPcCollect(true);
                for (int i = 0; i < 20 && pcCollectPending; i++) yield return null;
                PaymentSmokeRequire(session.Credit == 0 && !pcCollectPending && ReadPcCreditEvent().phase == 3 &&
                    pcPaymentMessageState.TotalPaid == 1000 && !pcPaymentMessageState.CollectPending,
                    "one actual collect precedes successor"); checks++;
                AddPcVirtualCredits(100);
                yield return null; yield return null;
                string before = PlayerPrefs.GetString(PcPaymentPresentationKey);
                PaymentSmokeRequire(!PrepareSourceSpin(false) && before == PlayerPrefs.GetString(PcPaymentPresentationKey) &&
                    pcPaymentMessageState.TotalPaid == 1000, "zero bet rejection retains paid presentation"); checks++;
                float until = Time.realtimeSinceStartup + 100f;
                while (SourceRoundBlocksOrdinaryInput || ResultDisplayActive || spinning || winRackActive ||
                    configuredAutoPlayCheckPending || Time.frameCount == rackSnapFrame)
                { PaymentSmokeRequire(Time.realtimeSinceStartup < until, "successor reaches idle"); yield return null; }
                for (int pocket = 0; pocket <= 36; pocket++) session.PlaceBet(100 + pocket, 1);
                SaveStagedBetsIfChanged(); BeginSpin();
                PaymentSmokeRequire(spinning && session.SpinPending && session.TotalBet == 37,
                    "positive normal BeginSpin admits real outcome RNG"); checks++;
                PaymentSmokeRequire(pcPaymentMessageState.Collect == 0 && pcPaymentMessageState.Cancel == 0 &&
                    pcPaymentMessageState.TotalPaid == 0 && !pcPaymentMessageState.CollectPending &&
                    pcPaymentPresentationJournal.entries.Count == 1 && pcPaymentPresentationJournal.entries[0].kind == "start",
                    "admitted successor retires completed payment chain"); checks++;
                until = Time.realtimeSinceStartup + 100f;
                while (spinning || session.SpinPending || SourceRoundBlocksOrdinaryInput || ResultDisplayActive || winRackActive)
                { PaymentSmokeRequire(Time.realtimeSinceStartup < until, "successor completes original source End"); yield return null; }
            }
            else
            {
                PaymentSmokeRequire(phase == "successor_cold", "known read only successor phase"); checks++;
                var prior = JsonUtility.FromJson<PaymentSuccessorReport>(File.ReadAllText(PaymentSmokeArg("-roulettePcPaymentMessagePrevious")));
                PaymentSmokeRequire(prior.phase == "successor_write" && prior.financialJournal == PlayerPrefs.GetString(PcCreditKey) &&
                    prior.meterJournal == PlayerPrefs.GetString(PcCollectMeterKey) &&
                    prior.presentationJournal == PlayerPrefs.GetString(PcPaymentPresentationKey) &&
                    PaymentSuccessorColdArchiveEqual(prior.archivedPages, PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1")) &&
                    prior.pocketsJournal == PlayerPrefs.GetString(recordedHistoryKey) &&
                    prior.rngBase64 == Convert.ToBase64String(File.ReadAllBytes(PcRngStateStore.PathForCurrentProcess)),
                    "cold successor preserves durable wallet meter presentation history and outcome RNG"); checks++;
            }
            PaymentSmokeRequire(session.Credit == 99 && session.Rounds == 1 && session.Returned == 36 &&
                recordedPockets.Count == 1 && CommittedHistoryCount() == 1 && session.Denomination == 1,
                "actual successor archive and wallet match 37 wager 36 return"); checks++;
            PaymentSmokeRequire(pcPaymentMessageState.Collect == 0 && pcPaymentMessageState.Cancel == 0 &&
                pcPaymentMessageState.TotalPaid == 0 && !pcPaymentMessageState.CollectPending && !pcPaymentMessagesActive &&
                pcPaymentPresentationJournal.entries.Count == 1 && pcPaymentPresentationJournal.entries[0].kind == "start",
                "completed successor keeps admitted zero message state"); checks++;
            RefreshPortraitUi();
            PaymentSmokeRequire(uiWinTitle.Value == "WIN", "AU successor retains source WIN title"); checks++;
            string path = PaymentSmokeArg("-roulettePcPaymentMessageOutput");
            if (File.Exists(path)) throw new Exception("Frozen successor report exists");
            ScreenCapture.CaptureScreenshot(path + ".png"); yield return new WaitForEndOfFrame();
            File.WriteAllText(path, JsonUtility.ToJson(new PaymentSuccessorReport { phase = phase, checks = checks,
                financialJournal = PlayerPrefs.GetString(PcCreditKey), meterJournal = PlayerPrefs.GetString(PcCollectMeterKey),
                presentationJournal = PlayerPrefs.GetString(PcPaymentPresentationKey),
                archivedPages = PlayerPrefs.GetString(recordedHistoryKey + ".RoundDetails.v1"),
                pocketsJournal = PlayerPrefs.GetString(recordedHistoryKey), rngPath = PcRngStateStore.PathForCurrentProcess,
                rngBase64 = Convert.ToBase64String(File.ReadAllBytes(PcRngStateStore.PathForCurrentProcess)),
                text = uiGameOver.Value, title = uiWinTitle.Value, credit = session.Credit, rounds = session.Rounds,
                returned = session.Returned, denomination = session.Denomination, pockets = recordedPockets.Count,
                historyCount = CommittedHistoryCount() }, true));
            Debug.Log("ROULETTE_PC_PAYMENT_SUCCESSOR_PASSED phase=" + phase + " checks=" + checks); Application.Quit(0);
        }
        private bool PaymentSuccessorColdArchiveEqual(string before, string after)
        {
            var a = JsonUtility.FromJson<RecordedRoundArchiveV3>(before);
            var b = JsonUtility.FromJson<RecordedRoundArchiveV3>(after);
            if (a == null || b == null || a.lifecycle == null || b.lifecycle == null ||
                a.lifecycle.idleEntry == null || b.lifecycle.idleEntry == null ||
                b.lifecycle.idleEntry.creditBefore != 99 || b.lifecycle.idleEntry.creditAfter != 99 ||
                b.lifecycle.idleEntry.state != PcIdleState.Complete ||
                a.lifecycle.idleEntry.consumedCollectEventId != b.lifecycle.idleEntry.consumedCollectEventId) return false;
            a.lifecycle.idleEntry = null; b.lifecycle.idleEntry = null;
            return JsonUtility.ToJson(a) == JsonUtility.ToJson(b);
        }
    }
}
