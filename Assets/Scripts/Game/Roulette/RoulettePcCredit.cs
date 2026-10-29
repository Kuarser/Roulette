using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace RouletteDeluxe
{
    // PC-only stand-in for the cabinet credit-out transaction. It keeps the
    // original request, credit-change and completion boundaries separate.
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class PcCreditEvent
        {
            public int version;
            public string id;
            public string anchor;
            public string profile;
            public string kind;
            public long baseCredit;
            public long before;
            public long after;
            public long collected;
            // v2 also records the platform money meter's denomination and
            // sub-credit remainder so a committed denomination switch can
            // be replayed before the staged table is restored.
            public long baseDenomination;
            public long baseRemainderCents;
            public long beforeDenomination;
            public long beforeRemainderCents;
            public long afterDenomination;
            public long afterRemainderCents;
            public long collectedCents;
            // 1=request accepted, 2=credit changed, 3=completion delivered.
            public int phase;
            public string digest;
        }

        private bool pcCollectPending;
        private PcCreditEvent pcCreditOverride;
        private string PcCreditKey { get { return recordedHistoryKey + ".PcCredit.v1"; } }

        private string PcCreditAnchor
        {
            get {
                if (recordedRoundPages.Count == 0 || recordedRoundPages[0] == null)
                    return "initial";
                RecordedRound latest = recordedRoundPages[0];
                return !string.IsNullOrEmpty(latest.roundArchiveId) ?
                    latest.roundArchiveId :
                    "legacy:" + recordedPockets.Count + ":" + latest.pocket;
            }
        }

        private static string PcCreditDigest(PcCreditEvent item)
        {
            string data = item.version + "|" + item.id + "|" + item.anchor +
                "|" + item.profile + "|" + item.kind + "|" +
                item.baseCredit + "|" + item.before + "|" + item.after +
                "|" + item.collected + "|" + item.phase;
            if (item.version >= 2)
                data += "|" + item.baseDenomination + "|" +
                    item.baseRemainderCents + "|" +
                    item.beforeDenomination + "|" +
                    item.beforeRemainderCents + "|" +
                    item.afterDenomination + "|" +
                    item.afterRemainderCents + "|" + item.collectedCents;
            using (SHA256 sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(
                    Encoding.UTF8.GetBytes(data))).Replace("-", "");
        }

        private PcCreditEvent ReadPcCreditEvent()
        {
            if (temporaryHistory) return null;
            string raw = PlayerPrefs.GetString(PcCreditKey, "");
            if (string.IsNullOrEmpty(raw)) return null;
            return ParseValidatedPcCreditEvent(raw);
        }
        private static PcCreditEvent ParseValidatedPcCreditEvent(string raw)
        {
            PcCreditEvent item;
            try { item = JsonUtility.FromJson<PcCreditEvent>(raw); }
            catch (ArgumentException error) {
                throw new InvalidOperationException("PC credit journal is unreadable", error);
            }
            if (item == null || (item.version != 1 && item.version != 2) ||
                string.IsNullOrEmpty(item.id) || string.IsNullOrEmpty(item.anchor) ||
                string.IsNullOrEmpty(item.profile) ||
                (item.kind != "collect" && item.kind != "deposit" &&
                 !(item.version == 2 && item.kind == "denom")) ||
                item.phase < 1 || item.phase > 3 || item.baseCredit < 0 ||
                item.before < 0 || item.after < 0 || item.collected < 0 ||
                item.digest != PcCreditDigest(item) ||
                (item.kind == "collect" &&
                    (item.after != 0 || item.collected != item.before)) ||
                (item.kind == "deposit" &&
                    (item.phase != 3 || item.collected != 0 ||
                     item.after <= item.before)) ||
                (item.version == 2 && !ValidPcMoneyEvent(item)))
                throw new InvalidOperationException("PC credit journal failed validation");
            return item;
        }

        private static bool ValidPcMoneyEvent(PcCreditEvent item)
        {
            if (item.baseDenomination < 1 || item.beforeDenomination < 1 ||
                item.afterDenomination < 1 ||
                item.baseRemainderCents < 0 ||
                item.baseRemainderCents >= item.baseDenomination ||
                item.beforeRemainderCents < 0 ||
                item.beforeRemainderCents >= item.beforeDenomination ||
                item.afterRemainderCents < 0 ||
                item.afterRemainderCents >= item.afterDenomination ||
                item.collectedCents < 0) return false;
            decimal beforeMoney = (decimal)item.before *
                item.beforeDenomination + item.beforeRemainderCents;
            decimal afterMoney = (decimal)item.after *
                item.afterDenomination + item.afterRemainderCents;
            if (beforeMoney > long.MaxValue || afterMoney > long.MaxValue)
                return false;
            if (item.kind == "collect")
                return item.afterRemainderCents == 0 &&
                    item.afterDenomination == item.beforeDenomination &&
                    item.collectedCents == beforeMoney;
            if (item.kind == "deposit")
                return item.collectedCents == 0 &&
                    item.afterDenomination == item.beforeDenomination &&
                    item.afterRemainderCents == item.beforeRemainderCents &&
                    afterMoney > beforeMoney;
            return item.kind == "denom" && item.collected == 0 &&
                item.collectedCents == 0 &&
                item.beforeDenomination != item.afterDenomination &&
                beforeMoney == afterMoney;
        }

        private void SelectProfileFromPcCreditEvent()
        {
            PcCreditEvent item = ReadPcCreditEvent();
            if (item == null || item.anchor != PcCreditAnchor) return;
            if (recordedRoundPages.Count > 0 &&
                recordedRoundPages[0].profile != item.profile)
                throw new InvalidOperationException(
                    "PC credit journal conflicts with the saved paytable");
            for (int index = 0; index < profiles.Length; ++index)
            {
                if (item.profile != "RouletteDeluxeSZ" + profiles[index]) continue;
                profileIndex = index;
                Debug.Log("ROULETTE_PROFILE_FROM_PC_CREDIT: " + profiles[index]);
                return;
            }
            throw new InvalidOperationException("PC credit journal has unknown paytable");
        }

        private void SavePcCreditEvent(PcCreditEvent item)
        {
            if (temporaryHistory) return;
            item.digest = PcCreditDigest(item);
            PlayerPrefs.SetString(PcCreditKey, JsonUtility.ToJson(item));
            PlayerPrefs.Save();
            if (pcPaymentCreditSavedTestHook != null) pcPaymentCreditSavedTestHook(item);
            if (item.version == 2 && item.kind == "collect" && item.phase == 2)
                SavePcCollectMeterCredit(item);
            if (item.version == 2 && item.kind == "denom" && item.phase == 2)
                ClearPcCollectMeter();
            if (item.kind == "collect" && item.version == 2 && item.phase >= 2)
                SavePcPaymentPresentationForCredit(item);
            if (item.kind == "collect" && item.phase == 3)
                LifecycleCheckpoint("CollectCompletedSaved");
        }

        private void PcCreditBaseline(PcCreditEvent prior, out long credit,
            out long denomination, out long remainderCents)
        {
            if (prior != null && prior.anchor == PcCreditAnchor &&
                prior.profile == session.Profile.Id && prior.version == 2)
            {
                credit = prior.baseCredit;
                denomination = prior.baseDenomination;
                remainderCents = prior.baseRemainderCents;
                return;
            }
            if (prior != null && prior.anchor == PcCreditAnchor &&
                prior.profile == session.Profile.Id)
                credit = prior.baseCredit;
            else
                credit = recordedRoundPages.Count == 0 ? 1000 :
                    recordedRoundPages[0].creditAfter;
            denomination = recordedRoundPages.Count == 0 ? 1 :
                recordedRoundPages[0].denomination;
            remainderCents = recordedRoundPages.Count == 0 ? 0 :
                recordedRoundPages[0].moneyRemainderCents;
        }

        private void RestorePcCreditEvent()
        {
            pcCreditOverride = null;
            PcCreditEvent item = ReadPcCreditEvent();
            LoadPcCollectMeter();
            LoadPcPaymentPresentation(false);
            if (item == null || item.anchor != PcCreditAnchor ||
                item.profile != session.Profile.Id) return;
            if (HasTrustedLatestLifecycle)
                RequireLifecycle(GetLatestLifecycleForRestore().roundStage == PcRoundStage.Ended,
                    "Credit event belongs to an unfinished round lifecycle");
            if (session.Credit != item.baseCredit ||
                (item.version == 2 &&
                 (session.Denomination != item.baseDenomination ||
                  ((RouletteSession)session).MoneyRemainderCents !=
                      item.baseRemainderCents)))
                throw new InvalidOperationException(
                    "PC credit journal conflicts with the saved game balance");
            RestorePcCollectMeterBeforeCredit(item);
            if (item.version == 2 && item.kind == "denom" && item.phase >= 2) ClearPcCollectMeter();
            // The PC virtual payout has no external device. A persisted
            // accepted request can deterministically finish after restart.
            if (item.phase == 1)
            {
                item.phase = 2;
                SavePcCreditEvent(item);
            }
            if (item.version == 2)
                ((RouletteSession)session).ApplyPlatformMoneyState(item.after,
                    item.afterDenomination, item.afterRemainderCents);
            else
                ((RouletteSession)session).ApplyPlatformCredit(item.after);
            if (item.kind == "denom") session.SetChip(1);
            if (item.kind == "collect" && session.ResultPhase)
                session.NextRound();
            if (item.kind == "collect") StartPcPlayerVolumeIdleTimeout();
            else if (item.kind == "deposit") CancelPcPlayerVolumeIdleTimeout();
            if (item.phase == 2)
            {
                item.phase = 3;
                SavePcCreditEvent(item);
            }
            pcCreditOverride = item;
            CompleteRestoredPcPaymentPresentation(item);
            if (PcPaymentPresentationEnabled) PublishPcPaymentMessages();
            Debug.Log("ROULETTE_PC_CREDIT_RESTORED: kind=" + item.kind +
                " credit=" + session.Credit + " phase=" + item.phase);
        }

        private bool PcCreditAllowsStagedCredit(long savedCredit)
        {
            PcCreditEvent item = pcCreditOverride;
            return item != null && item.anchor == PcCreditAnchor &&
                item.profile == session.Profile.Id &&
                (savedCredit == item.baseCredit ||
                 savedCredit == item.before || savedCredit == item.after);
        }

        private bool CanChangePcDenomination()
        {
            return !PcGameOrdinaryInputBlocked && session is RouletteSession &&
                !LifecycleBlocksPlayerInput &&
                session.Profile.Denominations.Length > 1 &&
                !session.SpinPending &&
                !session.ResultPhase && !session.InDoubleUp &&
                !spinning && !ResultDisplayActive && !autoplay &&
                !pcCollectPending && !autoConfirmPending &&
                !serviceConfirmPending && !collectConfirmPending &&
                !gameInfoOptionsOpen &&
                paysPage == -1 && !IsDoubleReveal && !IsDoubleExit;
        }

        private void RequestPcDenominationChange()
        {
            if (PcGameOrdinaryInputBlocked) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            ShowFeedback("SBox denomination adapter is required.");
#else
            if (!CanChangePcDenomination()) return;
            var local = (RouletteSession)session;
            int[] available = session.Profile.Denominations;
            int index = Array.IndexOf(available, (int)session.Denomination);
            if (index < 0) throw new InvalidOperationException("Current denomination is unavailable");
            long nextDenomination = available[(index + 1) % available.Length];
            if (nextDenomination == session.Denomination) return;
            // Preserve the full money meter, including a fractional credit.
            long moneyBefore = local.MoneyCents;
            long nextCredit = moneyBefore / nextDenomination;
            long nextRemainder = moneyBefore % nextDenomination;
            // Roulette::DoSelectDenom calls ClearTables(false) after the
            // platform accepts the new denomination. Clear the uncommitted
            // table before journaling this PC-only money change so recovery
            // cannot restore old-denomination bets against the new balance.
            if (session.TotalBet != 0) session.Clear();
            SaveStagedBetsIfChanged();
            PcCreditEvent prior = ReadPcCreditEvent();
            long baseline, baseDenomination, baseRemainder;
            PcCreditBaseline(prior, out baseline, out baseDenomination,
                out baseRemainder);
            var item = new PcCreditEvent {
                version = 2, id = Guid.NewGuid().ToString("N"),
                anchor = PcCreditAnchor, profile = session.Profile.Id,
                kind = "denom", baseCredit = baseline,
                before = session.Credit, after = nextCredit,
                collected = 0, phase = 1,
                baseDenomination = baseDenomination,
                baseRemainderCents = baseRemainder,
                beforeDenomination = session.Denomination,
                beforeRemainderCents = local.MoneyRemainderCents,
                afterDenomination = nextDenomination,
                afterRemainderCents = nextRemainder,
                collectedCents = 0
            };
            SavePcCreditEvent(item);
            local.SetDenomination(nextDenomination);
            if (local.Credit != nextCredit ||
                local.MoneyRemainderCents != nextRemainder ||
                local.MoneyCents != moneyBefore)
                throw new InvalidOperationException("PC denomination changed monetary value");
            item.phase = 2;
            SavePcCreditEvent(item);
            pcCreditOverride = item;
            item.phase = 3;
            SavePcCreditEvent(item);
            SaveStagedBetsIfChanged();
            RestartSourcePromptAfterCreditChange();
            Debug.Log("ROULETTE_PC_DENOM_CHANGED: " + item.beforeDenomination +
                "->" + item.afterDenomination + " cents=" + moneyBefore +
                " credit=" + session.Credit + " remainder=" + nextRemainder);
#endif
        }

        private void RequestPcCollect()
        {
            if (PcGameOrdinaryInputBlocked) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            ShowFeedback("SBox cashout adapter is required.");
#else
            if (pcCollectPending || session == null || session.SpinPending ||
                SourceRoundBlocksOrdinaryInput ||
                spinning || ResultDisplayActive || session.InDoubleUp ||
                IsDoubleReveal || IsDoubleExit || doubleCollectPending ||
                autoplay) return;
            var local = session as RouletteSession;
            if (local == null || local.MoneyCents < 1) return;
            PcCreditEvent prior = ReadPcCreditEvent();
            long baseline, baseDenomination, baseRemainder;
            PcCreditBaseline(prior, out baseline, out baseDenomination,
                out baseRemainder);
            var item = new PcCreditEvent {
                version = 2, id = Guid.NewGuid().ToString("N"),
                anchor = PcCreditAnchor, profile = session.Profile.Id,
                kind = "collect", baseCredit = baseline,
                before = session.Credit, after = 0,
                collected = session.Credit, phase = 1,
                baseDenomination = baseDenomination,
                baseRemainderCents = baseRemainder,
                beforeDenomination = session.Denomination,
                beforeRemainderCents = local.MoneyRemainderCents,
                afterDenomination = session.Denomination,
                afterRemainderCents = 0,
                collectedCents = local.MoneyCents
            };
            SavePcCreditEvent(item);
            pcCollectPending = true;
            StartCoroutine(CompletePcCollect(item));
            if (!ideckSecondDisplay) SetIDeckPreview(false);
            Debug.Log("ROULETTE_PC_COLLECT_REQUESTED: id=" + item.id +
                " credits=" + item.collected);
#endif
        }

        private IEnumerator CompletePcCollect(PcCreditEvent item)
        {
            yield return null;
            item.phase = 2;
            SavePcCreditEvent(item);
            ((RouletteSession)session).ApplyPlatformMoneyState(item.after,
                item.afterDenomination, item.afterRemainderCents);
            if (session.ResultPhase) session.NextRound();
            pcCreditOverride = item;
            Debug.Log("ROULETTE_PC_COLLECT_CREDIT_CHANGED: id=" + item.id +
                " credit=" + session.Credit);
            yield return null;
            item.phase = 3;
            SavePcCreditEvent(item);
            pcCreditOverride = item;
            pcCollectPending = false;
            try { EnsureCollectedLifecycleIdleEntry(item); }
            catch (Exception error)
            {
                FailPcRoundLifecycle("collected", error);
                yield break;
            }
            // CashoutRequest enters state 11. RunLoop assigns state 1 after
            // StateCashout returns, so the next input service must perform
            // the real Idle purge/Demo/Auto entry even with zero credit.
            sourceGameplayState = SourceGameplayState.Idle;
            sourceBetStateEntryPending = false;
            configuredAutoPlayCheckPending = true;
            configuredAutoPlayFinishPending = false;
            SaveStagedBetsIfChanged();
            StartPcPlayerVolumeIdleTimeout();
            RestartSourcePromptAfterCreditChange();
            ShowFeedback("PC virtual collect: " +
                RouletteDenominationFormatter.ForFeatureCurrency(
                    item.collectedCents) + ".");
            Debug.Log("ROULETTE_PC_COLLECT_COMPLETED: id=" + item.id +
                " credits=" + item.collected +
                " cents=" + item.collectedCents);
        }

        private void AddPcVirtualCredits(long amount, bool demoAutoplay = false, string operationId = null)
        {
            var local = session as RouletteSession;
            if (local == null || LifecycleBlocksPlayerInput || pcCollectPending || (autoplay && !demoAutoplay) || spinning ||
                ResultDisplayActive || session.SpinPending || session.InDoubleUp ||
                session.DoubleAvailable || amount < 1) return;
            PcCreditEvent prior = ReadPcCreditEvent();
            long baseline, baseDenomination, baseRemainder;
            PcCreditBaseline(prior, out baseline, out baseDenomination,
                out baseRemainder);
            long after = checked(session.Credit + amount);
            checked { long centsAfter = after * session.Denomination +
                local.MoneyRemainderCents; }
            var item = new PcCreditEvent {
                version = 2, id = operationId ?? Guid.NewGuid().ToString("N"),
                anchor = PcCreditAnchor, profile = session.Profile.Id,
                kind = "deposit", baseCredit = baseline,
                before = session.Credit, after = after,
                collected = 0, phase = 3,
                baseDenomination = baseDenomination,
                baseRemainderCents = baseRemainder,
                beforeDenomination = session.Denomination,
                beforeRemainderCents = local.MoneyRemainderCents,
                afterDenomination = session.Denomination,
                afterRemainderCents = local.MoneyRemainderCents,
                collectedCents = 0
            };
            SavePcCreditEvent(item);
            if (operationId != null) LifecycleCheckpoint("IdleDepositSaved");
            bool hadNoMoney = local.MoneyCents == 0;
            local.AddDemoCredits(amount);
            // PreSetCredits reopens inputs only across zero. A deposit into
            // an already positive meter does not undo a failed DoPlay lock.
            if (hadNoMoney && local.MoneyCents > 0) sourceTableInputEnabled = true;
            CancelPcPlayerVolumeIdleTimeout();
            pcCreditOverride = item;
            if (!demoAutoplay)
            {
                RestartSourcePromptAfterCreditChange();
                ShowFeedback("PC virtual credits +" + amount + ".");
            }
            Debug.Log("ROULETTE_PC_CREDIT_ADDED: id=" + item.id +
                " credit=" + session.Credit);
        }

        private IEnumerator PcDenomSaveSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcDenomTxnTest",
                    StringComparison.Ordinal) < 0 ||
                session == null || session.Credit != 1000 ||
                session.Denomination != 1 ||
                ((RouletteSession)session).MoneyRemainderCents != 0 ||
                recordedRoundPages.Count != 0 || uiDenomTouch == null ||
                footerDenomTouchBar == null || footerDenomTouchButton == null ||
                footerDenomTouchBar.width != 125 ||
                footerDenomTouchBar.height != 12 ||
                footerDenomTouchButton.width != 107 ||
                footerDenomTouchButton.height != 82)
                throw new InvalidOperationException(
                    "PC denomination save smoke needs a fresh isolated balance");
            AddPcVirtualCredits(1);
            if (((RouletteSession)session).MoneyCents != 1001)
                throw new InvalidOperationException("PC denomination setup missed one cent");
            session.PlaceBet(100, 1);
            if (session.TotalBet != 1)
                throw new InvalidOperationException("Denomination clear setup missed its table bet");
            long[] expected = {2, 5, 10, 20, 25};
            foreach (long denomination in expected)
            {
                RefreshPortraitUi();
                if (!uiDenomTouch.gameObject.activeInHierarchy)
                    throw new InvalidOperationException("Source denomination touch is hidden");
                ClickThroughUiRaycast(uiDenomTouch,
                    denomination == 2 ? new Vector2(50, -78) :
                    (Vector2?)null);
                var local = (RouletteSession)session;
                if (session.Denomination != denomination ||
                    local.MoneyCents != 1001 ||
                    session.Credit != 1001 / denomination ||
                    local.MoneyRemainderCents != 1001 % denomination)
                    throw new InvalidOperationException(
                        "ProView touch changed the PC money meter");
                if (session.TotalBet != 0 ||
                    stagedBets == null || stagedBets.bets.Length != 0)
                    throw new InvalidOperationException(
                        "Denomination touch did not durably clear the old table");
            }
            PcCreditEvent item = ReadPcCreditEvent();
            if (item == null || item.version != 2 || item.kind != "denom" ||
                item.phase != 3 || item.afterDenomination != 25 ||
                item.afterRemainderCents != 1 ||
                stagedBets == null || stagedBets.denomination != 25 ||
                stagedBets.moneyRemainderCents != 1)
                throw new InvalidOperationException(
                    "PC denomination transaction was not durably committed");
            Debug.Log("ROULETTE_PC_DENOM_SAVE_PASSED: 1001 cents -> 40 credits + 1 cent at denomination 25");
            Application.Quit();
        }

        private IEnumerator PcDenomLoadCollectSmoke()
        {
            yield return null;
            var local = session as RouletteSession;
            if (recordedHistoryKey.IndexOf(".Test.PcDenomTxnTest",
                    StringComparison.Ordinal) < 0 ||
                local == null || local.Denomination != 25 ||
                local.Credit != 40 || local.MoneyRemainderCents != 1 ||
                local.MoneyCents != 1001 || pcCreditOverride == null ||
                pcCreditOverride.kind != "denom")
                throw new InvalidOperationException(
                    "PC denomination balance did not survive process restart");
            RequestPcCollect();
            if (!pcCollectPending || ReadPcCreditEvent().collectedCents != 1001)
                throw new InvalidOperationException(
                    "PC cashout omitted the denomination remainder");
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || local.MoneyCents != 0 ||
                local.MoneyRemainderCents != 0 ||
                ReadPcCreditEvent().phase != 3)
                throw new InvalidOperationException(
                    "PC cashout did not clear the full money meter");
            Debug.Log("ROULETTE_PC_DENOM_COLLECT_PASSED: cashout=1001 cents, balance=0");
            Application.Quit();
        }

        private IEnumerator PcDenomCollectLoadSmoke()
        {
            yield return null;
            var local = session as RouletteSession;
            PcCreditEvent item = ReadPcCreditEvent();
            if (recordedHistoryKey.IndexOf(".Test.PcDenomTxnTest",
                    StringComparison.Ordinal) < 0 || local == null ||
                local.Denomination != 25 || local.MoneyCents != 0 ||
                local.Credit != 0 || local.MoneyRemainderCents != 0 ||
                item == null || item.kind != "collect" ||
                item.collectedCents != 1001 || item.phase != 3)
                throw new InvalidOperationException(
                    "PC denomination cashout was not recovered exactly once");
            Debug.Log("ROULETTE_PC_DENOM_COLLECT_LOAD_PASSED: 1001 cents settled once");
            Application.Quit();
        }

        private IEnumerator PcDenomRoundSaveSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcDenomRoundTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 1000 || recordedRoundPages.Count != 0 ||
                demoOutcome == null)
                throw new InvalidOperationException(
                    "PC denomination round smoke needs a fresh isolated archive");
            AddPcVirtualCredits(1);
            for (int i = 0; i < 5; ++i) RequestPcDenominationChange();
            var local = (RouletteSession)session;
            if (local.MoneyCents != 1001 || local.Denomination != 25 ||
                local.Credit != 40 || local.MoneyRemainderCents != 1)
                throw new InvalidOperationException("Denomination round setup failed");
            demoOutcome.SelectFixedPocket(0);
            session.PlaceBet(101, 1);
            RoundResult result = session.Spin();
            RecordPocket(result.Pocket, result, session);
            if (result.Pocket != 0 || result.Returned != 0 ||
                local.MoneyCents != 976 || local.Credit != 39 ||
                local.MoneyRemainderCents != 1 ||
                recordedRoundPages.Count != 1 ||
                recordedRoundPages[0].moneyRemainderCents != 1)
                throw new InvalidOperationException(
                    "Round archive lost the denomination remainder");
            Debug.Log("ROULETTE_PC_DENOM_ROUND_SAVE_PASSED: 976 cents archived as 39 credits + 1 cent");
            Application.Quit();
        }

        private IEnumerator PcDenomRoundLoadSmoke()
        {
            yield return null;
            var local = session as RouletteSession;
            if (recordedHistoryKey.IndexOf(".Test.PcDenomRoundTest",
                    StringComparison.Ordinal) < 0 || local == null ||
                recordedRoundPages.Count != 1 ||
                local.LastResult == null || local.LastResult.Pocket != 0 ||
                local.Denomination != 25 || local.Credit != 39 ||
                local.MoneyRemainderCents != 1 || local.MoneyCents != 976)
                throw new InvalidOperationException(
                    "Archived denomination remainder did not survive restart");
            RequestPcCollect();
            if (!pcCollectPending || ReadPcCreditEvent().collectedCents != 976)
                throw new InvalidOperationException(
                    "Archived denomination cashout omitted the remainder");
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || local.MoneyCents != 0)
                throw new InvalidOperationException(
                    "Archived denomination cashout did not complete");
            Debug.Log("ROULETTE_PC_DENOM_ROUND_LOAD_PASSED: 976 cents restored and collected");
            Application.Quit();
        }

        private IEnumerator PcDenomRemainderCollectSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcDenomRemainderTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 1000 || recordedRoundPages.Count != 0)
                throw new InvalidOperationException(
                    "PC remainder-only cashout needs fresh isolated credit");
            RequestPcCollect();
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || session.Credit != 0)
                throw new InvalidOperationException("Initial virtual cashout failed");
            AddPcVirtualCredits(1);
            for (int i = 0; i < 5; ++i) RequestPcDenominationChange();
            var local = (RouletteSession)session;
            if (local.Denomination != 25 || local.Credit != 0 ||
                local.MoneyRemainderCents != 1 || local.MoneyCents != 1)
                throw new InvalidOperationException("Remainder-only balance was not retained");
            RequestPcCollectConfirmation();
            if (!collectConfirmPending)
                throw new InvalidOperationException(
                    "COLLECT refused a nonzero money meter with zero playable credits");
            if (CanChangePcDenomination())
                throw new InvalidOperationException(
                    "Denomination remained available during COLLECT confirmation");
            RequestPcDenominationChange();
            if (local.Denomination != 25 || local.MoneyCents != 1)
                throw new InvalidOperationException(
                    "Denomination changed during COLLECT confirmation");
            ConfirmPcCollect(true);
            if (!pcCollectPending || ReadPcCreditEvent().collectedCents != 1)
                throw new InvalidOperationException("One-cent cashout was not requested");
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || local.MoneyCents != 0 ||
                local.MoneyRemainderCents != 0)
                throw new InvalidOperationException("One-cent cashout did not complete");
            Debug.Log("ROULETTE_PC_DENOM_REMAINDER_COLLECT_PASSED: 1 cent with zero playable credits");
            Application.Quit();
        }

        private IEnumerator PcDenomInterruptedSaveSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcDenomInterruptedTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 1000 || session.Denomination != 1 ||
                recordedRoundPages.Count != 0)
                throw new InvalidOperationException(
                    "Interrupted denomination smoke needs fresh isolated credit");
            session.PlaceBet(100, 1);
            if (session.TotalBet != 1)
                throw new InvalidOperationException(
                    "Interrupted denomination smoke missed its staged bet");
            session.Clear();
            SaveStagedBetsIfChanged();
            if (stagedBets == null || stagedBets.bets.Length != 0)
                throw new InvalidOperationException(
                    "Interrupted denomination smoke did not save the empty table");
            var item = new PcCreditEvent {
                version = 2, id = Guid.NewGuid().ToString("N"),
                anchor = PcCreditAnchor, profile = session.Profile.Id,
                kind = "denom", baseCredit = 1000,
                before = 1000, after = 500, collected = 0, phase = 1,
                baseDenomination = 1, baseRemainderCents = 0,
                beforeDenomination = 1, beforeRemainderCents = 0,
                afterDenomination = 2, afterRemainderCents = 0,
                collectedCents = 0
            };
            SavePcCreditEvent(item);
            if (ReadPcCreditEvent().phase != 1 ||
                session.Credit != 1000 || session.Denomination != 1)
                throw new InvalidOperationException(
                    "Interrupted denomination request was not durable before commit");
            Debug.Log("ROULETTE_PC_DENOM_INTERRUPTED_SAVE_PASSED: request saved before money changed");
            Application.Quit();
        }

        private IEnumerator PcDenomInterruptedLoadSmoke()
        {
            yield return null;
            var local = session as RouletteSession;
            PcCreditEvent item = ReadPcCreditEvent();
            if (recordedHistoryKey.IndexOf(".Test.PcDenomInterruptedTest",
                    StringComparison.Ordinal) < 0 || local == null ||
                item == null || item.kind != "denom" || item.phase != 3 ||
                local.Credit != 500 || local.Denomination != 2 ||
                local.MoneyRemainderCents != 0 || local.MoneyCents != 1000 ||
                stagedBets == null || stagedBets.credit != 500 ||
                stagedBets.denomination != 2 ||
                stagedBets.bets.Length != 0 || session.TotalBet != 0)
                throw new InvalidOperationException(
                    "Interrupted denomination request was not recovered exactly once");
            Debug.Log("ROULETTE_PC_DENOM_INTERRUPTED_LOAD_PASSED: request replayed once, 1000 cents conserved");
            Application.Quit();
        }

        private IEnumerator PcCollectSaveSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcCollectTxnTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 1000 || recordedRoundPages.Count != 0)
                throw new InvalidOperationException("PC collect save smoke needs fresh isolated credit");
            if (!Try(() => session.PlaceBet(100, 1)) || session.TotalBet != 1)
                throw new InvalidOperationException("PC collect staged wager setup failed");
            RequestPcCollect();
            if (!pcCollectPending || session.Credit != 1000 ||
                ReadPcCreditEvent().phase != 1)
                throw new InvalidOperationException("PC collect request was not durable");
            for (int frame = 0; frame < 5 && pcCollectPending; ++frame)
                yield return null;
            PcCreditEvent item = ReadPcCreditEvent();
            if (pcCollectPending || session.Credit != 0 || session.TotalBet != 1 ||
                item == null || item.phase != 3 || item.collected != 1000)
                throw new InvalidOperationException("PC collect did not finish once");
            Debug.Log("ROULETTE_PC_COLLECT_SAVE_PASSED: credit=0 bet=1 id=" + item.id);
            Application.Quit();
        }

        private IEnumerator PcCollectLoadSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcCollectTxnTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 0 || session.TotalBet != 1 ||
                pcCreditOverride == null || pcCreditOverride.phase != 3 ||
                pcCreditOverride.kind != "collect")
                throw new InvalidOperationException("PC collect restart resurrected credit or lost bets");
            AddPcVirtualCredits(100);
            if (session.Credit != 100 || session.TotalBet != 1 ||
                pcCreditOverride == null || pcCreditOverride.kind != "deposit" ||
                ReadPcCreditEvent().after != 100)
                throw new InvalidOperationException("PC virtual credit input was not durable");
            RefreshPortraitUi();
            if (shownPrompt != PromptKind.PressSpin ||
                (promptEligibilityMask & (1 << (int)PromptKind.PressSpin)) == 0 ||
                (promptEligibilityMask & (1 << (int)PromptKind.PlaceBets)) == 0)
                throw new InvalidOperationException(
                    "Credit change did not restart at the source first active message");
            Debug.Log("ROULETTE_PC_COLLECT_LOAD_PASSED: credit=100 bet=1");
            Application.Quit();
        }

        private IEnumerator PcVolumeTimeoutSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                session == null || session.Credit != 1000 || pcPlayerVolumeValue != 0)
                throw new InvalidOperationException(
                    "PC volume timeout smoke needs fresh isolated credit and volume keys");
            AdvancePcPlayerVolume();
            if (pcPlayerVolumeValue != 5 ||
                PlayerPrefs.GetInt(pcPlayerVolumeKey, -1) != 5)
                throw new InvalidOperationException("PC volume selection was not saved");
            RequestPcCollect();
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || session.Credit != 0 ||
                pcPlayerVolumeIdleDeadlineTimestamp <=
                    System.Diagnostics.Stopwatch.GetTimestamp())
                throw new InvalidOperationException("Cashout did not start 90s volume timer");
            AddPcVirtualCredits(100);
            if (session.Credit != 100 || pcPlayerVolumeIdleDeadlineTimestamp != 0 ||
                pcPlayerVolumeValue != 5)
                throw new InvalidOperationException("Credit input did not cancel volume timer");
            RequestPcCollect();
            for (int frame = 0; frame < 6 && pcCollectPending; ++frame)
                yield return null;
            if (pcCollectPending || session.Credit != 0 ||
                pcPlayerVolumeIdleDeadlineTimestamp == 0)
                throw new InvalidOperationException("Second cashout did not rearm volume timer");
            pcPlayerVolumeIdleDeadlineTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            TickPcPlayerVolumeIdleTimeout(System.Diagnostics.Stopwatch.GetTimestamp());
            if (pcPlayerVolumeValue != 0 || pcPlayerVolumeIdleDeadlineTimestamp != 0 ||
                PlayerPrefs.GetInt(pcPlayerVolumeKey, -1) != 0 ||
                Mathf.Abs(AudioListener.volume - Mathf.Pow(10f, -4f / 20f)) > .0001f)
                throw new InvalidOperationException("90s expiry did not persist source default volume");
            AddPcVirtualCredits(1);
            AdvancePcPlayerVolume();
            session = new RouletteSession(session.Profile, 1,
                new FixedPocketSmokeOutcome(0));
            session.PlaceBet(101, 1);
            session.BeginSpin();
            if (pcPlayerVolumeIdleDeadlineTimestamp != 0)
                throw new InvalidOperationException("New-credit spin retained volume timer");
            RoundResult loss = session.CompleteSpin();
            session.NextRound();
            if (loss.Returned != 0 || session.Credit != 0)
                throw new InvalidOperationException("Last-credit loss fixture differs");
            yield return null;
            if (pcPlayerVolumeIdleDeadlineTimestamp != 0)
                throw new InvalidOperationException("Round loss incorrectly posted GameReady timer");
            Debug.Log("ROULETTE_PC_VOLUME_TIMEOUT_PASSED: collect arms, credit cancels, loss is not app-ready");
            Application.Quit();
        }

        private IEnumerator PcVolumeReadySaveSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                recordedRoundPages.Count != 0 || session == null ||
                session.Credit != 1000 || pcPlayerVolumeValue != 0)
                throw new InvalidOperationException(
                    "PC GameReady save smoke needs fresh isolated keys");
            AdvancePcPlayerVolume();
            session = new RouletteSession(session.Profile, 1,
                new FixedPocketSmokeOutcome(0));
            session.PlaceBet(101, 1);
            RoundResult loss = session.Spin();
            session.NextRound();
            if (loss.Returned != 0 || session.Credit != 0)
                throw new InvalidOperationException("Zero-credit ready fixture differs");
            RecordPocket(loss.Pocket, loss, session);
            if (recordedRoundPages.Count != 1 ||
                recordedRoundPages[0].creditAfter != 0 ||
                pcPlayerVolumeIdleDeadlineTimestamp != 0)
                throw new InvalidOperationException("Losing round did not persist without a ready timer");
            Debug.Log("ROULETTE_PC_VOLUME_READY_SAVE_PASSED: zero-credit loss archived");
            Application.Quit();
        }

        private IEnumerator PcVolumeReadyLoadSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                pcPlayerVolumeKey.IndexOf(".Test.", StringComparison.Ordinal) < 0 ||
                recordedRoundPages.Count != 1 || session == null ||
                session.Credit != 0 || session.SpinPending ||
                pcPlayerVolumeValue != 5 ||
                pcPlayerVolumeIdleDeadlineTimestamp <=
                    System.Diagnostics.Stopwatch.GetTimestamp())
                throw new InvalidOperationException(
                    "Restored zero-credit GameReady did not start volume timer");
            pcPlayerVolumeIdleDeadlineTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp() - 1;
            TickPcPlayerVolumeIdleTimeout(System.Diagnostics.Stopwatch.GetTimestamp());
            if (pcPlayerVolumeValue != 0 || pcPlayerVolumeIdleDeadlineTimestamp != 0 ||
                PlayerPrefs.GetInt(pcPlayerVolumeKey, -1) != 0)
                throw new InvalidOperationException(
                    "Restored GameReady timeout did not persist volume reset");
            Debug.Log("ROULETTE_PC_VOLUME_READY_LOAD_PASSED: app-ready arms once after zero-credit restore");
            Application.Quit();
        }

        private IEnumerator PcCollectReopenSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcCollectTxnTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 100 || session.TotalBet != 1 ||
                pcCreditOverride == null || pcCreditOverride.kind != "deposit")
                throw new InvalidOperationException("PC deposit restart lost credit or table");
            Debug.Log("ROULETTE_PC_COLLECT_REOPEN_PASSED: credit=100 bet=1");
            Application.Quit();
        }

        private IEnumerator PcCollectPlaySmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcCollectTxnTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.Credit != 100 || session.TotalBet != 1)
                throw new InvalidOperationException("PC collect play needs restored credit and bet");
            BeginSpin();
            float deadline = Time.realtimeSinceStartup + 25f;
            while (spinning && Time.realtimeSinceStartup < deadline)
                yield return null;
            if (spinning || session.LastResult == null || session.Rounds != 1 ||
                session.Credit < 99 || recordedRoundPages.Count != 1 ||
                pcCreditOverride == null ||
                pcCreditOverride.anchor == PcCreditAnchor)
                throw new InvalidOperationException("PC collect credit did not enter a new settled round");
            Debug.Log("ROULETTE_PC_COLLECT_PLAY_PASSED: pocket=" +
                session.LastResult.Pocket + " credit=" + session.Credit);
            Application.Quit();
        }

        private IEnumerator PcCollectRoundReopenSmoke()
        {
            yield return null;
            if (recordedHistoryKey.IndexOf(".Test.PcCollectTxnTest",
                    StringComparison.Ordinal) < 0 || session == null ||
                session.LastResult == null || session.Credit < 99 ||
                recordedRoundPages.Count != 1 || pcCreditOverride != null)
                throw new InvalidOperationException("Old PC credit event overrode newer round");
            Debug.Log("ROULETTE_PC_COLLECT_ROUND_REOPEN_PASSED: pocket=" +
                session.LastResult.Pocket + " credit=" + session.Credit);
            Application.Quit();
        }

        private IEnumerator PcCollectInterruptedSaveSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-roulettePcCollectInterruptedSaveSmoke");
            int phase;
            if (option < 0 || option + 1 >= args.Length ||
                !int.TryParse(args[option + 1], out phase) ||
                (phase != 1 && phase != 2) || session == null ||
                session.Credit != 1000 || recordedRoundPages.Count != 0 ||
                !recordedHistoryKey.EndsWith(
                    ".Test.PcCollectCrashPhase" + phase + "Test",
                    StringComparison.Ordinal))
                throw new InvalidOperationException("Interrupted collect needs a fresh isolated profile");
            if (!Try(() => session.PlaceBet(100, 1)) || session.TotalBet != 1)
                throw new InvalidOperationException("Interrupted collect wager setup failed");
            var item = new PcCreditEvent {
                version = 1, id = Guid.NewGuid().ToString("N"),
                anchor = PcCreditAnchor, profile = session.Profile.Id,
                kind = "collect", baseCredit = session.Credit,
                before = session.Credit, after = 0,
                collected = session.Credit, phase = phase
            };
            SavePcCreditEvent(item);
            if (ReadPcCreditEvent().phase != phase || session.Credit != 1000)
                throw new InvalidOperationException("Interrupted collect setup did not persist");
            Debug.Log("ROULETTE_PC_COLLECT_INTERRUPTED_SAVE_PASSED: phase=" + phase);
            Application.Quit();
        }

        private IEnumerator PcCollectInterruptedLoadSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-roulettePcCollectInterruptedLoadSmoke");
            int phase;
            if (option < 0 || option + 1 >= args.Length ||
                !int.TryParse(args[option + 1], out phase) ||
                (phase != 1 && phase != 2) ||
                !recordedHistoryKey.EndsWith(
                    ".Test.PcCollectCrashPhase" + phase + "Test",
                    StringComparison.Ordinal) || session == null ||
                session.Credit != 0 || session.TotalBet != 1 ||
                pcCreditOverride == null || pcCreditOverride.phase != 3 ||
                pcCreditOverride.kind != "collect" ||
                ReadPcCreditEvent().phase != 3)
                throw new InvalidOperationException("Interrupted collect did not recover exactly once");
            Debug.Log("ROULETTE_PC_COLLECT_INTERRUPTED_LOAD_PASSED: phase=" +
                phase + " credit=0 bet=1");
            Application.Quit();
        }
    }
}
