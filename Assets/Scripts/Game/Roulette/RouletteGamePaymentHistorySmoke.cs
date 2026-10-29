using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class GamePaymentHistoryPair
        {
            public string pageJson, snapshotBase64, sha256;
        }

        [Serializable]
        private sealed class GamePaymentHistoryFixture
        {
            public int version = 1;
            public string origin, orderedSha256;
            public bool oldestPlaceholder;
            public long initialCredit, rounds, wagered, returned, wins;
            public GamePaymentHistoryPair[] pairs;
        }

        [Serializable]
        private sealed class GamePaymentHistoryObservation
        {
            public string point, stage, orderedSha256, rawEnvelopeJson, rawPages;
            public string[] pairSha256;
            public long at, credit;
            public int frame, visible, physical;
            public bool overflow, published;
        }

        [Serializable]
        private sealed class GamePaymentHistoryCapture
        {
            public string label, surface, captureMethod, path, sha256, pairSha256;
            public int width, height, frame, historyPage, recordIndex;
            public long at;
            public int[] histogramCounts;
        }

        [Serializable]
        private sealed class GamePaymentHistoryMalformed
        {
            public string kind, validRawJson, validRawSha256, corruptRawJson, corruptRawSha256, expectedFaultOperation;
        }

        private sealed partial class GamePaymentSmokeProof
        {
            public GamePaymentHistoryFixture historyFixture;
            public GamePaymentHistoryObservation[] historyObservations;
            public GamePaymentHistoryCapture[] historyCaptures;
            public GamePaymentHistoryMalformed historyMalformed;
        }

        private GamePaymentHistoryFixture gpHistoryFixture;
        private GamePaymentHistoryMalformed gpHistoryMalformed;
        private readonly List<GamePaymentHistoryObservation> gpHistoryObservations = new List<GamePaymentHistoryObservation>();
        private readonly List<GamePaymentHistoryCapture> gpHistoryCaptures = new List<GamePaymentHistoryCapture>();

        partial void ConfigureGamePaymentSmokeExtension(ref bool recognized)
        {
            if (!new[] { "history_full50", "history_placeholder50", "history_double_full50", "history_malformed_overflow" }
                .Contains(gpSmokeCase)) return;
            recognized = true;
            gpSmokeExternalCase = GamePaymentHistorySmoke;
        }

        partial void FillGamePaymentSmokeExtensionProof(GamePaymentSmokeProof proof)
        {
            proof.historyFixture = gpHistoryFixture ?? new GamePaymentHistoryFixture { version = 0,
                origin = "", orderedSha256 = "", pairs = new GamePaymentHistoryPair[0] };
            proof.historyObservations = gpHistoryObservations.ToArray();
            proof.historyCaptures = gpHistoryCaptures.ToArray();
            proof.historyMalformed = gpHistoryMalformed ?? new GamePaymentHistoryMalformed { kind = "",
                validRawJson = "", validRawSha256 = "", corruptRawJson = "", corruptRawSha256 = "", expectedFaultOperation = "" };
        }

        partial void CheckGamePaymentSmokeExtension(string point)
        {
            if (gpHistoryFixture == null || pcRoundLifecycleStorageFaulted || gpHistoryMalformed != null) return;
            GpHistoryObserve(point, point.StartsWith("GamePayment", StringComparison.Ordinal) ||
                point.StartsWith("History", StringComparison.Ordinal) || point.StartsWith("Cold", StringComparison.Ordinal) ||
                point == "UnpaidRoundObserved" || point.StartsWith("Feature", StringComparison.Ordinal) ||
                point.StartsWith("Auto:Feature", StringComparison.Ordinal) || point == gpSmokeCut);
        }

        private static GamePaymentHistoryPair GpHistoryPair(RecordedRound page, byte[] snapshot)
        {
            var pair = new GamePaymentHistoryPair { pageJson = JsonUtility.ToJson(page),
                snapshotBase64 = Convert.ToBase64String(snapshot) };
            pair.sha256 = RoundSmokeSha(pair.pageJson + "\n" + pair.snapshotBase64);
            return pair;
        }

        private static string GpHistoryOrder(IEnumerable<GamePaymentHistoryPair> pairs)
        { return RoundSmokeSha(string.Join("\n", pairs.Select(pair => pair.sha256).ToArray())); }

        private GamePaymentHistoryPair[] GpHistoryVisiblePairs()
        {
            RequireGpSmoke(CommittedHistoryCount() == 50 && recordedSnapshots.Count == 50 &&
                recordedRoundPages.Count == 50, "full history pair count differs");
            var pairs = new GamePaymentHistoryPair[50];
            for (int i = 0; i < pairs.Length; ++i)
            {
                RecordedRound page;
                byte[] snapshot;
                RequireGpSmoke(TryGetHistoryRecord(HistoryRecordIndex(i), out page, out snapshot) &&
                    page != null && snapshot != null && snapshot.Length == HistogramCapacity,
                    "public history pair is inaccessible at " + i);
                pairs[i] = GpHistoryPair(page, snapshot);
            }
            RequireGpSmoke(HistoryRecordIndex(50) == -1, "history exposes a 51st committed record");
            return pairs;
        }

        private void GpHistoryObserve(string point, bool retain = true)
        {
            GamePaymentHistoryPair[] pairs = GpHistoryVisiblePairs();
            bool published = pcGamePayment != null && pcGamePayment.historyPublished;
            bool pending = pcGamePayment != null && !published;
            RequireGpSmoke((pcHistoryOverflow != null) == pending &&
                (!pending || HasOwnedPcHistoryOverflow && HistoryRecordIndex(49) == 50) &&
                (pending || HistoryRecordIndex(49) == 49), "overflow ownership or public mapping differs");
            for (int i = published ? 1 : 0; i < 50; ++i)
            {
                GamePaymentHistoryPair expected = gpHistoryFixture.pairs[published ? i - 1 : i];
                RequireGpSmoke(pairs[i].pageJson == expected.pageJson &&
                    pairs[i].snapshotBase64 == expected.snapshotBase64 && pairs[i].sha256 == expected.sha256,
                    "history detail/snapshot pair changed at " + i + " during " + point);
            }
            if (published)
                RequireGpSmoke(pairs[0].sha256 == GpHistoryPair(recordedRoundPages[0], recordedSnapshots[0]).sha256 &&
                    !recordedRoundPages[0].awaitingFinalPayout, "published front pair differs from current paid round");
            if (!retain) return;
            RoundLifecycleSmokeRaw raw = ReadRoundSmokeRaw();
            gpHistoryObservations.Add(new GamePaymentHistoryObservation {
                point = point, stage = pcGamePayment == null ? "Fixture" : published ? "Published" :
                    pcGamePayment.IsResolved ? "CompleteUnpublished" : pcGamePayment.moneyApplied ? "Applied" : "Unpaid",
                at = Stopwatch.GetTimestamp(), frame = Time.frameCount, credit = session.Credit,
                visible = CommittedHistoryCount(), physical = recordedRoundPages.Count,
                overflow = pcHistoryOverflow != null, published = published,
                orderedSha256 = GpHistoryOrder(pairs), pairSha256 = pairs.Select(pair => pair.sha256).ToArray(),
                rawEnvelopeJson = raw.envelope, rawPages = raw.pages
            });
        }

        private void GpHistoryCreateFixture()
        {
            RequireGpSmoke(gpSmokePrevious == null && gpSmokeBoot.envelope == "" && gpSmokeBoot.pending == "" &&
                recordedSnapshots.Count == 0 && recordedRoundPages.Count == 0 && session.Credit == 1000 &&
                session.Rounds == 0 && !temporaryHistory && recordedHistoryKey.Contains(".Test."),
                "history fixture requires unused isolated runtime");
            bool placeholder = gpSmokeCase == "history_placeholder50";
            var rounds = new RecordedRound[50];
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            for (int i = 0; i < 50; ++i)
            {
                int pocket = i == 49 ? 0 : i % 36 + 1;
                var snapshot = new byte[HistogramCapacity];
                for (int j = 0; j < snapshot.Length; ++j) snapshot[j] = (byte)((i * 7 + j * 3) % 37);
                snapshot[0] = (byte)pocket;
                recordedSnapshots.Add(snapshot);
                var bets = new[] { new RecordedBet { id = 100, amount = 1, chips = new long[] { 1 } } };
                var round = new RecordedRound {
                    present = true, pocket = pocket, wager = 1, returned = 0, category = 1, denomination = 1,
                    creditAfter = 1000 + i, totalsRecorded = true, totalWagered = 50 - i,
                    totalReturned = 0, totalRounds = 50 - i, totalWins = 1,
                    wheelAngle = i * 5, ballX = 500 + i, ballY = 400 + i,
                    profile = "RouletteDeluxeSZ" + profiles[profileIndex], selectedChip = 1, bets = bets,
                    winningPays = i == 49 ? new[] { new RecordedPay { id = 100, amount = 36 } } : new RecordedPay[0],
                    historyPayOverrides = new RecordedHistoryPayOverride[0], evaluationOrder = new[] { 100 },
                    sourceTableBlock = Convert.ToBase64String(EncodeSourceTableBlock(bets)),
                    spinJournalId = (1000 + i).ToString("x32"), roundArchiveId = (2000 + i).ToString("x32"),
                    hasDoubleUp = i == 49,
                    doubleUp = i == 49 ? new RecordedDouble { startingStake = 36, complete = true, finalWin = 0,
                        steps = new[] { new RecordedDoubleStep { choice = 5, dealer = 0, wager = 36, won = 0 } } } : null
                };
                if (placeholder && i == 49) round = new RecordedRound();
                string before = JsonUtility.ToJson(round);
                RequireGpSmoke(!round.present ? IsDefaultLifecyclePage(round) : IsValidRecordedRound(round, snapshot),
                    "generated fixture page is invalid");
                RequireGpSmoke(before == JsonUtility.ToJson(round), "generated fixture needs semantic repair");
                rounds[i] = round;
            }
            recordedPockets.AddRange(recordedSnapshots[0].Select(value => (int)value));
            string encoded = JsonUtility.ToJson(new RecordedRoundArchive { rounds = rounds });
            PlayerPrefs.SetString(recordedHistoryKey, string.Join(",", recordedPockets.Select(v => v.ToString()).ToArray()));
            PlayerPrefs.SetString(recordedHistoryKey + ".Pages", string.Join(";",
                recordedSnapshots.Select(Convert.ToBase64String).ToArray()));
            PlayerPrefs.SetString(recordedHistoryKey + ".RoundDetails.v1", encoded);
            PlayerPrefs.Save();
            LoadRecordedRoundPages();
            ++recordedHistoryVersion;
            LoadProfile(false);
            RequireGpSmoke(!pcRoundLifecycleStorageFaulted && pcGamePayment == null && latestRoundLifecycle == null &&
                pcRoundArchiveSchemaVersion == 1 && session.Credit == 1000 && session.Rounds == 50 &&
                session.Wagered == 50 && session.Returned == 0 && session.Wins == 1,
                "actual legacy loader did not restore generated fixture");
            gpHistoryFixture = new GamePaymentHistoryFixture { origin = "codec-generated-50-committed-pairs-not-live-spins",
                oldestPlaceholder = placeholder, initialCredit = 1000, rounds = 50, wagered = 50, returned = 0, wins = 1,
                pairs = GpHistoryVisiblePairs() };
            gpHistoryFixture.orderedSha256 = GpHistoryOrder(gpHistoryFixture.pairs);
            GpSmokeCheckpoint("HistoryFixtureLoaded");
        }

        private void GpHistoryRestoreFixture()
        {
            gpHistoryFixture = gpSmokePrevious.historyFixture;
            gpHistoryMalformed = gpSmokePrevious.historyMalformed;
            if (gpHistoryMalformed != null && gpHistoryMalformed.kind == "") gpHistoryMalformed = null;
            RequireGpSmoke(gpHistoryFixture != null && gpHistoryFixture.version == 1 &&
                gpHistoryFixture.origin == "codec-generated-50-committed-pairs-not-live-spins" &&
                gpHistoryFixture.pairs != null && gpHistoryFixture.pairs.Length == 50 &&
                gpHistoryFixture.orderedSha256 == GpHistoryOrder(gpHistoryFixture.pairs), "cold fixture metadata differs");
            foreach (GamePaymentHistoryPair pair in gpHistoryFixture.pairs)
                RequireGpSmoke(pair.sha256 == RoundSmokeSha(pair.pageJson + "\n" + pair.snapshotBase64) &&
                    Convert.FromBase64String(pair.snapshotBase64).Length == HistogramCapacity,
                    "cold fixture pair digest differs");
        }

        private IEnumerator GamePaymentHistorySmoke()
        {
            gpSmokeBlockedEvent = "Credit";
            if (gpSmokePrevious != null) GpHistoryRestoreFixture();
            if (gpSmokeCase == "history_malformed_overflow" && gpSmokePrevious != null)
            {
                yield return GpHistoryObserveMalformed();
                yield break;
            }
            RequireGpSmoke(session != null && pcOutcome != null && sourceInputModule != null &&
                !pcRoundLifecycleStorageFaulted && !pcAutoPlayStorageFaulted && !ideckSecondDisplay &&
                !topperDedicatedDisplay && !autoplay && pcAutoCompleteGameSelected,
                "history case has no isolated inactive single-display runtime");
            if (gpSmokePrevious == null)
            {
                RequireGpSmoke(gpSmokeCase != "history_malformed_overflow" ||
                    gpSmokePhase == "seed" && gpSmokeCut == "MalformedOverflowSaved", "malformed case needs its exact seed cut");
                yield return GpSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                    !configuredAutoPlayCheckPending && !sourceBetStateEntryPending, "initial history Idle");
                GpHistoryCreateFixture();
                yield return GpSmokeQueue(uiClearButton, portraitCanvas, 760, "clear fixture table");
                yield return GpSmokeWait(() => session.TotalBet == 0 && SourcePlayerInputs.Count == 0 &&
                    !sourceBetStateEntryPending, "clear actual restored fixture wager");
                DemoCleanupClick(uiChipSelections[0].GetComponent<Button>());
                yield return GpSmokeQueue(uiBetButtons[27], portraitCanvas, 761, "history wager");
                yield return GpSmokeWait(() => session.TotalBet == 1 && sourceGameplayState == SourceGameplayState.Bet &&
                    !sourceBetStateEntryPending && SourcePlayerInputs.Count == 0, "history actual straight wager");
                yield return StartGameChooseDemo(30);
                yield return GpSmokeQueue(uiSpinButton, portraitCanvas, 762, "history PLAY commit");
                yield return GpSmokeWait(() => session.SpinPending && pendingSpin != null && gpSmokePockets == 1,
                    "history real wheel commit");
                RequireGpSmoke(pcOutcome.Receipt != null && pcOutcome.Receipt.rawResult == 30 &&
                    pcOutcome.Receipt.effectiveResult == 27 && session.Credit == 999 && session.Rounds == 50,
                    "history draw or wager differs");
                yield return GpSmokeWait(() => pcGamePayment != null && session.Rounds == 51, "history unpaid result");
                RequireGpSmoke(pcGamePayment.baseAward == 36 && pcGamePayment.priorHistoryCount == 50 &&
                    !pcGamePayment.moneyApplied && session.Credit == 999, "history record paid early");
                GpSmokeCheckpoint("UnpaidRoundObserved");
            }
            else
            {
                RequireGpSmoke(pcGamePayment != null && gpSmokePockets == 0 && gpSmokeSuits == 0 &&
                    ReadRoundSmokeRaw().rng == gpSmokePrevious.raw.rng, "cold history redrew or lost payment");
                var prior = JsonUtility.FromJson<PcGamePaymentRecordV1>(gpSmokePrevious.paymentJson);
                RequireGpSmoke(prior != null && prior.operationId == pcGamePayment.operationId &&
                    prior.roundArchiveId == pcGamePayment.roundArchiveId && prior.spinJournalId == pcGamePayment.spinJournalId,
                    "cold history payment identity differs");
                GpSmokeCheckpoint("ColdHistoryRestored");
            }
            if (gpSmokePhase == "stable")
            {
                RequireGpSmoke(gpSmokePrevious != null && gpSmokePrevious.phase == "resume" &&
                    gpSmokePrevious.checkpoint == "HistoryPaymentStable" && gpSmokePrevious.exitMode == "natural" &&
                    latestRoundLifecycle != null && latestRoundLifecycle.roundStage == PcRoundStage.Ended &&
                    pcGamePayment.historyPublished && pcGamePayment.IsResolved && sourceRoundEndCount == 0 &&
                    gpSmokeCompleteSpins == 0 && gpSmokeRecordPockets == 0 && gpSmokeReplyAttempts == 0,
                    "stable history is not already final");
                yield return GpSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                    !configuredAutoPlayCheckPending && !sourceBetStateEntryPending && !ResultDisplayActive &&
                    !winRackActive && SourcePlayerInputs.Count == 0 && latestRoundLifecycle != null &&
                    latestRoundLifecycle.roundStage == PcRoundStage.Ended && latestRoundLifecycle.idleEntry != null &&
                    latestRoundLifecycle.idleEntry.state == PcIdleState.Complete, "stable history cold Idle complete");
                GpSmokeRequireStableReloadRaw(gpSmokePrevious.raw);
                yield return GpHistoryCaptureOldest("stable", true);
                yield return GpSmokeHoldStage("history_stable", () => pcGamePayment.historyPublished &&
                    sourceRoundEndCount == 0 && gpSmokePockets == 0 && gpSmokeSuits == 0);
                GpSmokeCheckpoint("HistoryStableReload");
                yield return GpSmokeCapture("final", true);
                GpSmokeExit("HistoryStableReload", "natural");
                yield break;
            }
            if (gpSmokeCase == "history_malformed_overflow")
            {
                GpHistorySaveMalformed();
                yield break;
            }
            if (!pcGamePayment.moneyApplied && !pcGamePayment.IsResolved)
            {
                if (pcGamePayment.owner == PcGamePaymentOwner.Base && pcGamePayment.baseOffer == PcGamePaymentOffer.Open)
                {
                    yield return GpSmokeWait(() => SourceDoubleOfferActive && !winRackActive && SourcePlayerInputs.Count == 0,
                        "full history unpaid offer");
                    yield return GpHistoryCaptureOldest("unpaid", true);
                    yield return GpSmokeHoldStage("history_unpaid", () => SourceDoubleOfferActive &&
                        !pcGamePayment.moneyApplied && session.Credit == 999 && CommittedHistoryCount() == 50);
                    SetHistoryView(false);
                    RefreshPortraitUi(); RefreshDiagnosticUi();
                }
                if (gpSmokeCase == "history_double_full50") yield return GpSmokeDriveFeature();
                else if (pcGamePayment.baseOffer == PcGamePaymentOffer.Open)
                {
                    yield return GpSmokeQueue(uiSpinButton, portraitCanvas, 763, "history offer PLAY");
                    yield return GpSmokeWait(() => pcGamePayment.baseOffer == PcGamePaymentOffer.Take &&
                        SourcePlayerInputs.Count == 0, "history actual offer take");
                    gpSmokeActualOfferPlay = true;
                    GpSmokeCheckpoint("OfferPlayAccepted");
                }
                yield return GpSmokeWait(() => pcGamePayment.moneyApplied, "history one money application");
            }
            if (!pcGamePayment.IsResolved)
            {
                gpSmokeBlockedEvent = pcGamePayment.creditNotified ? "Complete" : "Credit";
                yield return GpHistoryCaptureOldest("applied", true);
                yield return GpSmokeHoldStage("history_applied", () => pcGamePayment.moneyApplied &&
                    !pcGamePayment.IsResolved && !pcGamePayment.historyPublished && sourceRoundEndCount == 0);
                RequestPcGameSuspend(false, "history-hold-complete");
                yield return GpSmokeWait(() => PcGameAppSuspended && pcGamePayoutEntered &&
                    !pcGamePayment.historyPublished, "actual App suspend before payment completion");
                gpSmokeBlockedEvent = "";
                yield return GpSmokeWait(() => pcGamePayment.IsResolved && !pcGamePayment.historyPublished,
                    "complete payment held unpublished by App");
                GpSmokeCheckpoint("HistoryCompleteUnpublished");
                yield return GpHistoryCaptureOldest("complete_unpublished", false);
                yield return GpSmokeHoldStage("history_complete_unpublished", () => PcGameAppSuspended &&
                    pcGamePayment.IsResolved && !pcGamePayment.historyPublished && sourceRoundEndCount == 0);
                RequestPcGameResume("history-publish-resume");
            }
            gpSmokeBlockedEvent = "";
            yield return GpSmokeWait(() => pcGamePayment.historyPublished && latestRoundLifecycle != null &&
                latestRoundLifecycle.roundStage == PcRoundStage.Ended && sourceGameplayState == SourceGameplayState.Idle &&
                !PcGameAppSuspended && !configuredAutoPlayCheckPending && !sourceBetStateEntryPending,
                "full history publication and End");
            RequireGpSmoke(session.Credit == GpSmokeExpectedCredit && session.Returned == GpSmokeExpectedAward &&
                session.Rounds == 51 && session.Wagered == 51 && session.Wins == 2 &&
                sourceRoundEndCount == 1 && !HasOwnedPcHistoryOverflow, "full history final money/statistics/End differs");
            GpSmokeCheckpoint("HistoryPublishedIdle");
            yield return GpHistoryCaptureOldest("published", !historyView);
            yield return GpSmokeHoldStage("history_published", () => pcGamePayment.historyPublished &&
                pcGamePayment.IsResolved && sourceRoundEndCount == 1 && session.Credit == GpSmokeExpectedCredit);
            yield return GpSmokeCapture("final", true);
            GpSmokeExit("HistoryPaymentStable", "natural");
        }

        private IEnumerator GpHistoryCaptureOldest(string label, bool navigate)
        {
            string rawBefore = JsonUtility.ToJson(ReadRoundSmokeRaw());
            int pockets = gpSmokePockets, suits = gpSmokeSuits, version = recordedHistoryVersion;
            if (navigate)
            {
                RequireGpSmoke(!PcGameAppSuspended, "ordinary history navigation while App suspended");
                SetHistoryView(true);
                RefreshPortraitUi(); RefreshDiagnosticUi(); Canvas.ForceUpdateCanvases();
                yield return null;
                for (int i = 1; i < 50; ++i)
                {
                    RequireGpSmoke(historyPrevious.IsActive() && historyPrevious.IsInteractable(), "history previous disabled early");
                    ClickThroughUiRaycast(historyPrevious);
                    RefreshHistoryPageUi(); Canvas.ForceUpdateCanvases();
                    RequireGpSmoke(historyPage == i && historyShownPage == i, "history previous pointer did not navigate once");
                }
            }
            RefreshPortraitUi(); RefreshDiagnosticUi(); Canvas.ForceUpdateCanvases();
            RequireGpSmoke(historyView && historyRoot.gameObject.activeInHierarchy && historyPage == 49 &&
                historyShownPage == 49 && !historyPrevious.interactable && historyNext.interactable,
                "oldest public history surface is not selected");
            GamePaymentHistoryPair expected = gpHistoryFixture.pairs[pcGamePayment.historyPublished ? 48 : 49];
            RecordedRound page;
            byte[] snapshot;
            RequireGpSmoke(TryGetHistoryRecord(HistoryRecordIndex(49), out page, out snapshot) &&
                GpHistoryPair(page, snapshot).sha256 == expected.sha256, "oldest displayed pair mapping differs");
            RequireGpSmoke(historyBall.gameObject.activeSelf == page.present &&
                ActiveHistoryLayerChildren(historyChipLayer) == (page.present ? 2 : 0) &&
                historyDoubleButton.gameObject.activeSelf == (page.present && page.doubleUp != null && page.doubleUp.complete),
                "oldest actual UI detail/placeholder/Double state differs");
            GpHistoryCapture(label + "_table", "portrait-history", expected, null);
            if (navigate && page.present && page.doubleUp != null && page.doubleUp.complete)
            {
                ClickThroughUiRaycast(historyDoubleButton);
                RequireGpSmoke(doubleHistoryView && doubleHistoryRoot.gameObject.activeInHierarchy &&
                    doubleHistoryRows.childCount > 0, "actual oldest Double history button did not render detail");
                GpHistoryCapture(label + "_double", "portrait-double-history", expected, null);
                ClickThroughUiRaycast(doubleHistoryExitButton);
                RequireGpSmoke(!doubleHistoryView, "Double history exit pointer failed");
            }
            // F4 history is Screen 1; expose its existing Screen 4 projection
            // without SetHistogramPreview resetting the selected history page.
            histogramPreview = true;
            RefreshTopperTarget(); RefreshDiagnosticUi(); RefreshHistogramUi(); Canvas.ForceUpdateCanvases();
            var counts = new int[37];
            foreach (byte pocket in snapshot) if (pocket <= 36) ++counts[pocket];
            RequireGpSmoke(histogramRoot.activeInHierarchy && histogramPage == HistoryRecordIndex(49) &&
                histogramCounts.SequenceEqual(counts), "oldest actual histogram snapshot differs");
            int maximum = counts.Max();
            for (int i = 0; i < 37; ++i)
            {
                int height = maximum == 0 ? 0 : (int)Math.Floor(127.0 * counts[i] / maximum + .5);
                RequireGpSmoke(histogramBars[i].gameObject.activeSelf == (height > 0) &&
                    histogramBars[i].rectTransform.sizeDelta.y == height, "oldest histogram bar geometry differs");
            }
            GpHistoryCapture(label + "_histogram", "topper-history-histogram", expected, counts);
            histogramPreview = false;
            RefreshTopperTarget(); RefreshPortraitUi(); RefreshDiagnosticUi();
            RequireGpSmoke(JsonUtility.ToJson(ReadRoundSmokeRaw()) == rawBefore && recordedHistoryVersion == version &&
                gpSmokePockets == pockets && gpSmokeSuits == suits, "history views mutated persistent business state");
            GpSmokeCheckpoint("HistoryCapture:" + label);
        }

        private void GpHistoryCapture(string label, string surface, GamePaymentHistoryPair pair, int[] counts)
        {
            string path = Path.Combine(Path.GetDirectoryName(gpSmokeCapturePath),
                Path.GetFileNameWithoutExtension(gpSmokeCapturePath) + "_history_" + label + ".png");
            RequireGpSmoke(!File.Exists(path), "history capture already exists");
            CaptureActiveScreenOffscreen(path);
            byte[] bytes = File.ReadAllBytes(path);
            RequireGpSmoke(bytes.Length > 24 && bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71,
                "history capture is not PNG");
            int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
            int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
            RequireGpSmoke(width == (counts == null ? 1080 : 1360) && height == (counts == null ? 1920 : 768),
                "history capture size differs");
            gpHistoryCaptures.Add(new GamePaymentHistoryCapture { label = label, surface = surface,
                captureMethod = "CaptureActiveScreenOffscreen-existing-uGUI-canvas", path = path,
                sha256 = RngSmokeBytesSha(bytes), width = width, height = height, at = Stopwatch.GetTimestamp(),
                frame = Time.frameCount, historyPage = historyPage, recordIndex = HistoryRecordIndex(historyPage),
                pairSha256 = pair.sha256, histogramCounts = counts ?? new int[0] });
        }

        private void GpHistorySaveMalformed()
        {
            RequireGpSmoke(pcHistoryOverflow != null && !pcGamePayment.historyPublished &&
                !pcGamePayment.moneyApplied, "malformed seed has no owned unpublished overflow");
            RoundLifecycleSmokeRaw valid = ReadRoundSmokeRaw();
            string encoded = JsonUtility.ToJson(valid);
            string needle = "\"snapshotBase64\":\"" + pcHistoryOverflow.snapshotBase64 + "\"";
            RequireGpSmoke(valid.envelope.IndexOf(needle, StringComparison.Ordinal) >= 0 &&
                valid.envelope.IndexOf(needle, StringComparison.Ordinal) == valid.envelope.LastIndexOf(needle, StringComparison.Ordinal),
                "overflow snapshot is not uniquely located");
            string corruptEnvelope = valid.envelope.Replace(needle, "\"snapshotBase64\":\"AQ==\"");
            PlayerPrefs.SetString(recordedHistoryKey + ".RoundDetails.v1", corruptEnvelope);
            PlayerPrefs.Save();
            string corrupt = JsonUtility.ToJson(ReadRoundSmokeRaw());
            gpHistoryMalformed = new GamePaymentHistoryMalformed { kind = "owned-overflow-snapshot-invalid-length",
                validRawJson = encoded, validRawSha256 = RoundSmokeSha(encoded), corruptRawJson = corrupt,
                corruptRawSha256 = RoundSmokeSha(corrupt), expectedFaultOperation = "loaded" };
            GpSmokeCheckpoint("MalformedOverflowSaved");
            throw new InvalidOperationException("Malformed seed cut returned");
        }

        private IEnumerator GpHistoryObserveMalformed()
        {
            RequireGpSmoke(gpHistoryMalformed != null && gpHistoryMalformed.corruptRawSha256 ==
                RoundSmokeSha(gpHistoryMalformed.corruptRawJson) && gpHistoryMalformed.validRawSha256 ==
                RoundSmokeSha(gpHistoryMalformed.validRawJson), "malformed history proof digests differ");
            RequireGpSmoke(pcRoundLifecycleStorageFaulted && pcAutoPlayStorageFaulted && session == null &&
                pcOutcome == null && !autoplay && !sourceTableInputEnabled &&
                (sourceInputModule == null || !sourceInputModule.enabled) &&
                pcAutoPlayStorageFaultOperation == gpHistoryMalformed.expectedFaultOperation,
                "actual cold history loader did not fail before model/RNG/input initialization");
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
            RequireGpSmoke(raw == gpHistoryMalformed.corruptRawJson && gpSmokePockets == 0 && gpSmokeSuits == 0 &&
                sourceRoundEndCount == 0, "malformed loader changed raw bytes or executed business effects");
            long begin = Stopwatch.GetTimestamp();
            int frame = Time.frameCount;
            while (OriginalRackClock.ElapsedMillisecondsLong(begin, Stopwatch.GetTimestamp(), Stopwatch.Frequency) < 1200 ||
                Time.frameCount - frame < 2)
            {
                yield return null;
                RequireGpSmoke(JsonUtility.ToJson(ReadRoundSmokeRaw()) == raw && pcRoundLifecycleStorageFaulted &&
                    session == null && pcOutcome == null && gpSmokePockets == 0 && gpSmokeSuits == 0 && sourceRoundEndCount == 0,
                    "malformed load continued or repaired the archive");
            }
            gpSmokeHolds.Add(new GamePaymentSmokeHold { stage = "history_malformed_fault", beginAt = begin,
                endAt = Stopwatch.GetTimestamp(), beginFrame = frame, endFrame = Time.frameCount,
                rawBeginSha256 = RoundSmokeSha(raw), rawEndSha256 = RoundSmokeSha(raw), rawStableRequired = true });
            yield return GpSmokeCapture("malformed_fault", true);
            GpSmokeExit("ExpectedMalformedLoad", "expected_fault");
        }
    }
}
