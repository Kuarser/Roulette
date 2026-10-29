using System;
using System.Collections.Generic;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        private const string RecordedHistoryKey = "RouletteDeluxeSZ.RecordedPockets.v1";
        private readonly List<int> recordedPockets = new List<int>();
        private readonly List<byte[]> recordedSnapshots = new List<byte[]>();
        private int recordedHistoryVersion;
        private bool temporaryHistory;
        private bool recordedRawHistoryInvalid;
        private bool retainSettledPendingForUndo;
        private string recordedHistoryKey = RecordedHistoryKey;
        private string NeighborSelectionKey { get {
            return recordedHistoryKey == RecordedHistoryKey ?
                "RouletteDeluxeSZ.SelectedNeighbor.v1" :
                recordedHistoryKey + ".SelectedNeighbor.v1";
        } }

        private void LoadRecordedHistory()
        {
            recordedPockets.Clear();
            recordedSnapshots.Clear();
            recordedRawHistoryInvalid = false;
            latestRoundLifecycle = null;
            string[] args = Environment.GetCommandLineArgs();
            int recoveryKey = Array.IndexOf(args, "-rouletteRecoveryTestKey");
            int historyKey = Array.IndexOf(args, "-rouletteHistoryTestKey");
            if (recoveryKey >= 0)
                recordedHistoryKey = TestHistoryKey(args, recoveryKey, ".RecoveryTest");
            else if (historyKey >= 0)
                recordedHistoryKey = TestHistoryKey(args, historyKey, ".Test");
            else recordedHistoryKey = RecordedHistoryKey;
            temporaryHistory = Array.IndexOf(args, "-rouletteFreshHistory") >= 0;
            ConfigureRoundLifecycleSmokeHooks();
            ConfigureRngReceiptSmokeHooks();
            ConfigureAutoCompleteSmoke();
            ConfigureGameSuspensionSmoke();
            ConfigureAutoCompleteLegacySmoke();
            ConfigureGamePaymentSmokeHooks();
            if (temporaryHistory) return;
            string saved = PlayerPrefs.GetString(recordedHistoryKey, "");
            foreach (string part in saved.Split(','))
            {
                int pocket;
                if (int.TryParse(part, out pocket) && pocket >= 0 && pocket <= 36)
                {
                    if (recordedPockets.Count < HistogramCapacity) recordedPockets.Add(pocket);
                    else recordedRawHistoryInvalid = true;
                }
                else if (!string.IsNullOrEmpty(saved)) recordedRawHistoryInvalid = true;
            }
            string pages = PlayerPrefs.GetString(recordedHistoryKey + ".Pages", "");
            foreach (string encoded in pages.Split(';'))
            {
                try
                {
                    byte[] snapshot = Convert.FromBase64String(encoded);
                    if (snapshot.Length != HistogramCapacity)
                    {
                        if (!string.IsNullOrEmpty(pages)) recordedRawHistoryInvalid = true;
                        continue;
                    }
                    bool valid = true;
                    foreach (byte value in snapshot)
                        if (value > 36 && value != 0xff) { valid = false; break; }
                    if (valid && recordedSnapshots.Count < 50) recordedSnapshots.Add(snapshot);
                    else recordedRawHistoryInvalid = true;
                }
                catch (FormatException) { recordedRawHistoryInvalid = true; }
            }
            LoadRecordedRoundPages();
            ++recordedHistoryVersion;
            Debug.Log("ROULETTE_HISTORY_LOADED: count=" + recordedPockets.Count +
                " latest=" + (recordedPockets.Count == 0 ? -1 : recordedPockets[0]));
        }

        private static string TestHistoryKey(string[] args, int option, string prefix)
        {
            if (option + 1 >= args.Length || args[option + 1].StartsWith("-"))
                return RecordedHistoryKey + prefix;
            string suffix = args[option + 1];
            if (suffix.Length > 64 || suffix.Length == 0)
                throw new ArgumentException("Invalid history test key length");
            foreach (char value in suffix)
                if (!char.IsLetterOrDigit(value) && value != '_' && value != '-')
                    throw new ArgumentException("Invalid history test key character");
            return RecordedHistoryKey + prefix + "." + suffix;
        }

        private void RecordPocket(int pocket, RoundResult result = null,
            IRouletteSession source = null, bool trackLifecycle = false)
        {
            if (pocket < 0 || pocket > 36)
                throw new ArgumentOutOfRangeException("pocket");
            if (trackLifecycle)
            {
                retainSettledPendingForUndo = false;
                LifecycleCheckpoint("RecordPocket");
                LifecycleBeforeWrite("RoundOpen");
            }
            RecordedHistoryOverflowV1 overflow = CapturePcPaymentHistoryOverflow(trackLifecycle);
            recordedPockets.Insert(0, pocket);
            if (recordedPockets.Count > HistogramCapacity)
                recordedPockets.RemoveAt(HistogramCapacity);
            var snapshot = new byte[HistogramCapacity];
            for (int i = 0; i < snapshot.Length; ++i)
                snapshot[i] = i < recordedPockets.Count ? (byte)recordedPockets[i] : (byte)0xff;
            recordedSnapshots.Insert(0, snapshot);
            if (recordedSnapshots.Count > 50) recordedSnapshots.RemoveAt(50);
            RecordRoundDetailsWithPayment(result, source ?? session, trackLifecycle, overflow);
            ++recordedHistoryVersion;
            if (temporaryHistory) return;
            PlayerPrefs.SetString(recordedHistoryKey,
                string.Join(",", recordedPockets.ConvertAll(value => value.ToString()).ToArray()));
            PlayerPrefs.SetString(recordedHistoryKey + ".Pages",
                string.Join(";", recordedSnapshots.ConvertAll(
                    page => Convert.ToBase64String(page)).ToArray()));
            PlayerPrefs.Save();
            if (trackLifecycle) LifecycleCheckpoint("RoundOpenSaved");
            bool undoSaved = SaveSettledTableUndo(source ?? session);
            retainSettledPendingForUndo = trackLifecycle && !undoSaved && pendingSpin != null;
        }
    }
}
