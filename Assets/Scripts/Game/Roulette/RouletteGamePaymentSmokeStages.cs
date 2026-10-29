using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class GamePaymentSmokeCapture
        {
            public string label, path, sha256;
            public int width, height;
            public long requestedAt, completedAt;
            public GamePaymentSmokeEvent state;
        }

        [Serializable]
        private sealed class GamePaymentSmokeHold
        {
            public string stage, rawBeginSha256, rawEndSha256;
            public long beginAt, endAt;
            public int beginFrame, endFrame;
            public bool rawStableRequired;
        }

        [Serializable]
        private sealed class GamePaymentSmokeFunding
        {
            public string point, receiptJson, rawJson, rawSha256;
            public long at, credit;
            public int frame;
        }

        private string gpSmokeCapturePath;
        private string gpSmokeBlockedEvent = "";
        private int gpSmokeExpectedSuits;
        private readonly List<GamePaymentSmokeCapture> gpSmokeCaptures = new List<GamePaymentSmokeCapture>();
        private readonly List<GamePaymentSmokeHold> gpSmokeHolds = new List<GamePaymentSmokeHold>();
        private readonly List<GamePaymentSmokeFunding> gpSmokeFunding = new List<GamePaymentSmokeFunding>();
        private Func<IEnumerator> gpSmokeExternalCase;

        // The separately owned history/legacy harness supplies fixed typed
        // proof members through the partial proof type, never an opaque blob.
        partial void ConfigureGamePaymentSmokeExtension(ref bool recognized);
        partial void CheckGamePaymentSmokeExtension(string point);
        partial void FillGamePaymentSmokeExtensionProof(GamePaymentSmokeProof proof);

        private bool GpSmokeIsDoubleCase { get { return gpSmokeCase.Contains("double") || gpSmokeCase.StartsWith("suit_", StringComparison.Ordinal) || gpSmokeCase == "pause_before"; } }
        private bool GpSmokeIsLossCase { get { return gpSmokeCase == "zero" || gpSmokeCase == "double_loss" || gpSmokeCase == "suit_loss"; } }
        private bool GpSmokeIsFaultCase { get { return gpSmokeCase.StartsWith("write_fail", StringComparison.Ordinal); } }
        private bool GpSmokeIsLowWalletCase { get { return gpSmokeCase.StartsWith("low_wallet_", StringComparison.Ordinal); } }
        private bool GpSmokeIsSuitCase { get { return gpSmokeCase.StartsWith("suit_", StringComparison.Ordinal); } }
        private long GpSmokeInitialCredit { get { return GpSmokeIsLowWalletCase ? 1 : 1000; } }
        private long GpSmokeCreditBefore { get { return GpSmokeInitialCredit - 1; } }
        private long GpSmokeExpectedAward { get { return GpSmokeIsLossCase ? 0 :
            GpSmokeIsDoubleCase ? (session != null && session.DoubleSuitMode ? 144 : 72) : 36; } }
        private long GpSmokeExpectedCredit { get { return GpSmokeCreditBefore + GpSmokeExpectedAward; } }

        private bool GpSmokePaymentEventAllowed(string next)
        {
            return next != gpSmokeBlockedEvent && (next != "Complete" || !gpSmokeHoldCompletion);
        }

        private void GpSmokeSuspensionEvent(string effect, string operation)
        {
            if (!gpSmokeConfigured || gpSmokeExiting) return;
            GamePaymentSmokeEvent state = GpSmokeSnapshot("Suspension:" + effect);
            state.controlOperationId = operation;
            gpSmokeEvents.Add(state);
        }

        private IEnumerator GpSmokeCapture(string label, bool final = false)
        {
            RequireGpSmoke(!string.IsNullOrWhiteSpace(label) &&
                label.All(c => char.IsLetterOrDigit(c) || c == '_'), "invalid capture label");
            string path = final ? gpSmokeCapturePath : Path.Combine(Path.GetDirectoryName(gpSmokeCapturePath),
                Path.GetFileNameWithoutExtension(gpSmokeCapturePath) + "_" + label + ".png");
            RequireGpSmoke(!File.Exists(path) && gpSmokeCaptures.All(c => c.label != label), "capture already exists");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            if (!pcAutoPlayStorageFaulted)
            {
                RefreshPortraitUi();
                RefreshDiagnosticUi();
                Canvas.ForceUpdateCanvases();
            }
            yield return null;
            GamePaymentSmokeEvent state = GpSmokeSnapshot("Capture:" + label);
            gpSmokeEvents.Add(state);
            var capture = new GamePaymentSmokeCapture { label = label, path = path,
                requestedAt = Stopwatch.GetTimestamp(), state = state };
            ScreenCapture.CaptureScreenshot(path, 2);
            float deadline = Time.realtimeSinceStartup + 10;
            while ((!File.Exists(path) || new FileInfo(path).Length == 0) && Time.realtimeSinceStartup < deadline)
                yield return null;
            RequireGpSmoke(File.Exists(path) && new FileInfo(path).Length > 24, "capture did not finish");
            byte[] bytes = File.ReadAllBytes(path);
            RequireGpSmoke(bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71 &&
                bytes[12] == 73 && bytes[13] == 72 && bytes[14] == 68 && bytes[15] == 82,
                "capture is not a PNG with IHDR");
            capture.width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
            capture.height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
            RequireGpSmoke(capture.width == Screen.width * 2 && capture.height == Screen.height * 2,
                "capture resolution differs from the current Player surface");
            capture.sha256 = RngSmokeBytesSha(bytes);
            capture.completedAt = Stopwatch.GetTimestamp();
            gpSmokeCaptures.Add(capture);
            Debug.Log("ROULETTE_GAME_PAYMENT_CAPTURE: label=" + label + " path=" + path +
                " sha256=" + capture.sha256 + " width=" + capture.width + " height=" + capture.height);
        }

        private IEnumerator GpSmokeHoldStage(string label, Func<bool> invariant, bool requireRawStable = true)
        {
            RequireGpSmoke(invariant != null && invariant() && !pcAutoPlayStorageFaulted,
                label + " did not reach its observable stage");
            RequireGpSmoke(gpSmokeHolds.All(h => h.stage != label), "hold stage repeated");
            string raw = JsonUtility.ToJson(ReadRoundSmokeRaw());
            var hold = new GamePaymentSmokeHold { stage = label, beginAt = Stopwatch.GetTimestamp(),
                beginFrame = Time.frameCount, rawBeginSha256 = RoundSmokeSha(raw), rawStableRequired = requireRawStable };
            yield return GpSmokeCapture(label + "_begin");
            RequireGpSmoke(invariant() && (!requireRawStable || JsonUtility.ToJson(ReadRoundSmokeRaw()) == raw),
                label + " changed while its initial PNG was written");
            // The full timed interval services real Player frames; encoding
            // the initial PNG is outside the observed 1.2 second interval.
            hold.beginAt = Stopwatch.GetTimestamp();
            hold.beginFrame = Time.frameCount;
            sourceInputModule.enabled = true;
            while (OriginalRackClock.ElapsedMillisecondsLong(hold.beginAt, Stopwatch.GetTimestamp(), Stopwatch.Frequency) < 1200 ||
                Time.frameCount - hold.beginFrame < 2)
            {
                yield return null;
                RequireGpSmoke(invariant() && !pcAutoPlayStorageFaulted &&
                    (!requireRawStable || JsonUtility.ToJson(ReadRoundSmokeRaw()) == raw),
                    label + " changed during its observable hold");
            }
            sourceInputModule.enabled = false;
            hold.endAt = Stopwatch.GetTimestamp();
            hold.endFrame = Time.frameCount;
            hold.rawEndSha256 = RoundSmokeSha(JsonUtility.ToJson(ReadRoundSmokeRaw()));
            RequireGpSmoke(invariant() && (!requireRawStable || hold.rawEndSha256 == hold.rawBeginSha256),
                label + " changed at the end of its hold");
            gpSmokeHolds.Add(hold);
            yield return GpSmokeCapture(label + "_held");
            RequireGpSmoke(invariant() && (!requireRawStable || JsonUtility.ToJson(ReadRoundSmokeRaw()) == raw),
                label + " changed while its held PNG was written");
            GpSmokeCheckpoint("Held:" + label);
        }

        private void GpSmokeRequireStableReloadRaw(RoundLifecycleSmokeRaw previousRaw)
        {
            RequireGpSmoke(previousRaw != null && latestRoundLifecycle != null &&
                latestRoundLifecycle.roundStage == PcRoundStage.Ended && latestRoundLifecycle.idleEntry != null &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete,
                "stable raw comparison precedes the real cold Idle completion");
            RoundLifecycleSmokeRaw currentRaw = ReadRoundSmokeRaw();
            string currentEnvelope = currentRaw.envelope;
            // Every non-archive key retains the predecessor's exact bytes.
            currentRaw.envelope = previousRaw.envelope;
            RequireGpSmoke(JsonUtility.ToJson(currentRaw) == JsonUtility.ToJson(previousRaw),
                "stable reload changed a non-archive persistent key");

            LifecycleJsonNode beforeRoot = new LifecycleJsonReader(previousRaw.envelope).Read();
            LifecycleJsonNode afterRoot = new LifecycleJsonReader(currentEnvelope).Read();
            RequireGpSmoke(beforeRoot.Kind == '{' && afterRoot.Kind == '{' &&
                beforeRoot.Members.Count == 6 && afterRoot.Members.Count == 6 &&
                beforeRoot.Members["lifecycleSchemaVersion"].Text(previousRaw.envelope) == "3" &&
                afterRoot.Members["lifecycleSchemaVersion"].Text(currentEnvelope) == "3",
                "stable reload requires two exact payment archives");
            LifecycleJsonNode beforeLifecycle = beforeRoot.Members["lifecycle"];
            LifecycleJsonNode afterLifecycle = afterRoot.Members["lifecycle"];
            ValidateLifecycleJsonShape(beforeLifecycle, typeof(PcRoundLifecycleRecord), previousRaw.envelope);
            ValidateLifecycleJsonShape(afterLifecycle, typeof(PcRoundLifecycleRecord), currentEnvelope);
            RequireGpSmoke(beforeLifecycle.Kind == '{' && afterLifecycle.Kind == '{' &&
                beforeLifecycle.Members["roundStage"].Text(previousRaw.envelope) == "4" &&
                afterLifecycle.Members["roundStage"].Text(currentEnvelope) == "4",
                "stable reload archive is not an ended round");
            LifecycleJsonNode beforeIdle = beforeLifecycle.Members["idleEntry"];
            LifecycleJsonNode afterIdle = afterLifecycle.Members["idleEntry"];
            RequireGpSmoke(beforeIdle.Kind == '{' && afterIdle.Kind == '{', "stable reload Idle record is absent");
            PcIdleEntry before = JsonUtility.FromJson<PcIdleEntry>(beforeIdle.Text(previousRaw.envelope));
            PcIdleEntry after = JsonUtility.FromJson<PcIdleEntry>(afterIdle.Text(currentEnvelope));
            RequireGpSmoke(before != null && after != null && before.state == PcIdleState.Complete &&
                after.state == PcIdleState.Complete && before.step == PcIdleStep.Complete && after.step == PcIdleStep.Complete &&
                !before.depositRequired && !after.depositRequired && !before.prepareRequested && !after.prepareRequested &&
                !before.prepareAccepted && !after.prepareAccepted && before.creditBefore == before.creditAfter &&
                after.creditBefore == after.creditAfter,
                "stable reload Idle deposited, prepared or did not complete");
            string[] oldIds = { before.idleEntryId, before.depositEventId, before.successorSpinId };
            string[] newIds = { after.idleEntryId, after.depositEventId, after.successorSpinId };
            RequireGpSmoke(oldIds.All(PcRoundLifecycleRecord.IsCanonicalId) &&
                newIds.All(PcRoundLifecycleRecord.IsCanonicalId) && oldIds.Distinct().Count() == 3 &&
                newIds.Distinct().Count() == 3 && newIds.All(id => !oldIds.Contains(id)),
                "stable reload did not create three distinct canonical Idle operation identities");

            // The established recovery contract permits this new Idle entry.
            // Restore only its three token values at their parsed exact paths;
            // the remaining archive bytes, including all money/history/End
            // fields and every other Idle field, must match byte for byte.
            string normalized = currentEnvelope;
            string[] names = { "idleEntryId", "depositEventId", "successorSpinId" };
            foreach (string name in names.OrderByDescending(name => afterIdle.Members[name].Start))
            {
                LifecycleJsonNode token = afterIdle.Members[name];
                normalized = normalized.Substring(0, token.Start) + beforeIdle.Members[name].Text(previousRaw.envelope) +
                    normalized.Substring(token.End);
            }
            RequireGpSmoke(normalized == previousRaw.envelope,
                "stable reload changed archive bytes outside the three new Idle identities");
        }

        private IEnumerator GpSmokeStableReload()
        {
            RequireGpSmoke(gpSmokePrevious != null && gpSmokePrevious.exitMode == "natural" &&
                gpSmokePrevious.checkpoint == "PaymentStable", "stable reload lacks a completed predecessor");
            PcRoundLifecycleRecord prior = JsonUtility.FromJson<PcRoundLifecycleRecord>(gpSmokePrevious.lifecycleJson);
            RequireGpSmoke(prior != null && prior.roundStage == PcRoundStage.Ended && prior.idleEntry != null &&
                prior.idleEntry.state == PcIdleState.Complete && pcGamePayment.IsResolved && pcGamePayment.historyPublished,
                "stable reload predecessor is not fully closed");
            yield return GpSmokeWait(() => sourceGameplayState == SourceGameplayState.Idle &&
                !configuredAutoPlayCheckPending && !sourceBetStateEntryPending && !ResultDisplayActive &&
                !winRackActive && SourcePlayerInputs.Count == 0 && latestRoundLifecycle != null &&
                latestRoundLifecycle.roundStage == PcRoundStage.Ended && latestRoundLifecycle.idleEntry != null &&
                latestRoundLifecycle.idleEntry.state == PcIdleState.Complete, "stable cold Idle");
            RequireGpSmoke(gpSmokePockets == 0 && gpSmokeSuits == 0 && gpSmokeCompleteSpins == 0 &&
                gpSmokeRecordPockets == 0 && gpSmokeReplyAttempts == 0 && sourceRoundEndCount == 0 &&
                session.Credit == gpSmokePrevious.credit && session.Wagered == gpSmokePrevious.wagered &&
                session.Returned == gpSmokePrevious.returned && session.Rounds == gpSmokePrevious.rounds &&
                session.Wins == gpSmokePrevious.wins,
                "stable reload changed the closed round or its financial statistics");
            GpSmokeRequireStableReloadRaw(gpSmokePrevious.raw);
            yield return GpSmokeHoldStage("stable_reload", () => gpSmokePockets == 0 && gpSmokeSuits == 0 &&
                gpSmokeCompleteSpins == 0 && gpSmokeRecordPockets == 0 && gpSmokeReplyAttempts == 0 &&
                sourceRoundEndCount == 0 && session.Credit == gpSmokePrevious.credit &&
                pcGamePayment.IsResolved && pcGamePayment.historyPublished && CommittedHistoryCount() == 1);
            yield return GpSmokeCapture("final", true);
            GpSmokeExit("PaymentStableReload", "natural");
        }
    }
}
