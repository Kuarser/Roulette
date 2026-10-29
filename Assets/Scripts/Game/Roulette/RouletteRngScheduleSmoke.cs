using System;
using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable]
        private sealed class RngScheduleReference
        {
            public string oracle;
            public RngScheduleCase[] cases;
            public RngWriterReference writers;
        }

        [Serializable]
        private sealed class RngScheduleCase
        {
            public uint seed;
            public int preAdvance, mixCount, pocket, savedIndex, savedConsumed;
            public uint[] savedFuture;
            public int suit, finalIndex, finalConsumed;
            public uint[] finalFuture;
        }

        [Serializable]
        private sealed class RngWriterReference
        {
            public uint seed;
            public int firstPocket, secondPocket, firstIndex, secondIndex;
            public uint[] firstFuture, secondFuture;
        }

        private IEnumerator RngScheduleSmoke()
        {
            yield return null;
            string[] args = Environment.GetCommandLineArgs();
            int option = Array.IndexOf(args, "-rouletteRngScheduleReference");
            if (option < 0 || option + 1 >= args.Length ||
                Array.IndexOf(args, "-rouletteRngScheduleSmoke") < 0 ||
                Array.IndexOf(args, "-rouletteRngStateTestKey") < 0)
                throw new ArgumentException("RNG schedule smoke needs its reference and isolated RNG key");
            string mainPath = Path.GetFullPath(PcRngStateStore.PathForCurrentProcess);
            if (!Path.GetFileName(mainPath).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal) ||
                !string.Equals(Path.GetDirectoryName(mainPath), Path.GetFullPath(Application.persistentDataPath),
                    StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("RNG schedule smoke requires an isolated RNG state path");
            RngScheduleReference reference = JsonUtility.FromJson<RngScheduleReference>(
                File.ReadAllText(args[option + 1]));
            if (reference == null || reference.cases == null || reference.cases.Length != 4 ||
                reference.oracle != "MSVC std::mt19937; ELF Boost quotient range")
                throw new InvalidOperationException("Invalid independent RNG schedule reference");
            if (reference.writers == null) throw new InvalidOperationException("Missing concurrent writer reference");
            var casePaths = new string[reference.cases.Length];
            for (int i = 0; i < casePaths.Length; ++i)
            {
                casePaths[i] = RngScheduleFixturePath(mainPath, "case" + i);
                RequireFreshRngSchedulePath(casePaths[i]);
            }
            string writersPath = RngScheduleFixturePath(mainPath, "writers");
            RequireFreshRngSchedulePath(writersPath);
            for (int i = 0; i < reference.cases.Length; ++i)
            {
                CheckRngSchedule(reference.cases[i], i, casePaths[i]);
                yield return null;
            }
            CheckRngConcurrentWriters(reference.writers, writersPath);
            Debug.Log("ROULETTE_RNG_SCHEDULE_PASSED: four fixed-state mix interleavings and concurrent writers, ten persisted states, 13000 independent future words");
            Application.Quit();
        }

        private static string RngScheduleFixturePath(string mainPath, string suffix)
        {
            return Path.Combine(Path.GetDirectoryName(mainPath),
                Path.GetFileNameWithoutExtension(mainPath) + "_" + suffix + ".bin");
        }

        private static void RequireFreshRngSchedulePath(string path)
        {
            foreach (string candidate in new[] { path, path + ".new" })
            {
                try { File.GetAttributes(candidate); }
                catch (FileNotFoundException) { continue; }
                catch (DirectoryNotFoundException) { continue; }
                throw new InvalidOperationException("RNG schedule evidence already exists: " + candidate);
            }
        }

        private static void CheckRngSchedule(RngScheduleCase expected, int caseIndex, string path)
        {
            RequireFreshRngSchedulePath(path);
            if (expected.mixCount < 10 || expected.mixCount > 15)
                throw new InvalidOperationException("Invalid reference mix count");
            var initial = new OriginalPlatformRngCore(expected.seed);
            for (int n = 0; n < expected.preAdvance; ++n) initial.NextRaw();
            using (var sampled = new ManualResetEventSlim(false))
            using (var allowMix = new ManualResetEventSlim(false))
            using (var prefixReady = new ManualResetEventSlim(false))
            using (var allowAfterSnapshot = new ManualResetEventSlim(false))
            using (var mixAttempted = new ManualResetEventSlim(false))
            using (var secondPrefixReady = new ManualResetEventSlim(false))
            using (var allowRemainder = new ManualResetEventSlim(false))
            {
                int mixed = 0, count = 0, draws = 0, snapshots = 0, saves = 0;
                Exception workerFailure = null;
                Action<string, int> observer = (stage, value) =>
                {
                    if (stage == "mix-count")
                    {
                        count = value;
                        sampled.Set();
                        if (!allowMix.Wait(5000)) throw new TimeoutException("RNG count barrier");
                    }
                    else if (stage == "mix-before-word" && value == 4) mixAttempted.Set();
                    else if (stage == "mix-word")
                    {
                        mixed = value;
                        if (value == 3)
                        {
                            prefixReady.Set();
                            if (!allowAfterSnapshot.Wait(5000))
                                throw new TimeoutException("RNG pre-snapshot mix barrier");
                        }
                        else if (value == 6)
                        {
                            secondPrefixReady.Set();
                            if (!allowRemainder.Wait(5000))
                                throw new TimeoutException("RNG captured-snapshot mix barrier");
                        }
                    }
                    else if (stage == "draw" && ++draws == 1)
                    {
                        allowMix.Set();
                        if (!prefixReady.Wait(5000))
                            throw new TimeoutException("Mix could not interleave between draw and snapshot");
                    }
                    else if (stage == "snapshot" && ++snapshots == 1)
                    {
                        if (value != expected.savedIndex)
                            throw new InvalidOperationException("Snapshot captured at wrong RNG index");
                        allowAfterSnapshot.Set();
                        if (!mixAttempted.Wait(5000))
                            throw new TimeoutException("Mix did not attempt the held save lock");
                        if (secondPrefixReady.Wait(200) || mixed != 3)
                            throw new InvalidOperationException("Mix advanced through the outer save lock");
                    }
                    else if (stage == "saved" && ++saves == 1 && !secondPrefixReady.Wait(5000))
                        throw new TimeoutException("Mix did not resume after releasing the save lock");
                };
                using (var provider = new PcOutcomeSource(initial.ExportSnapshot(), path, observer))
                {
                    var worker = new Thread(() =>
                    {
                        try { provider.MixForDiagnostic(); }
                        catch (Exception ex) { workerFailure = ex; }
                    }) { IsBackground = true, Name = "Roulette RNG schedule fixture" };
                    worker.Start();
                    try
                    {
                        if (!sampled.Wait(5000)) throw new TimeoutException("RNG mix did not start");
                        if (provider.DrawPocket() != expected.pocket || count != expected.mixCount ||
                            mixed != 6 || saves != 1)
                            throw new InvalidOperationException("RNG draw/mix/save trace mismatch");
                        // The live engine resumes only after save releases its
                        // outer lock; the persisted state is the earlier capture.
                        CheckRngSavedState(path, expected.seed, expected.savedIndex, expected.savedFuture);
                        allowRemainder.Set();
                        if (!worker.Join(5000)) throw new TimeoutException("RNG mix did not finish");
                        if (workerFailure != null) throw new InvalidOperationException("RNG mix worker failed", workerFailure);
                        if (mixed != expected.mixCount || provider.DrawDoubleSuit() != expected.suit ||
                            draws != 2 || snapshots != 2 || saves != 2)
                            throw new InvalidOperationException("RNG final mix/suit/save trace mismatch");
                        CheckRngSavedState(path, expected.seed, expected.finalIndex, expected.finalFuture);
                        Debug.Log("ROULETTE_RNG_SCHEDULE_CASE: case=" + caseIndex + " seed=" + expected.seed +
                            " pre=" + expected.preAdvance + " mix=" + count + " pocket=" + expected.pocket +
                            " savedIndex=" + expected.savedIndex + " suit=" + expected.suit +
                            " finalIndex=" + expected.finalIndex + " futureWords=2600 saveBlocksMix=True");
                    }
                    finally
                    {
                        allowMix.Set(); allowAfterSnapshot.Set(); allowRemainder.Set();
                        if (!worker.Join(5000)) throw new TimeoutException("RNG fixture worker did not terminate");
                    }
                }
            }
        }

        private static void CheckRngSavedState(string path, uint seed, int index, uint[] future)
        {
            uint[] snapshot;
            if (future == null || future.Length != 1300 || !PcRngStateStore.TryLoad(path, out snapshot) ||
                snapshot.Length != OriginalPlatformRngCore.SnapshotWordCount ||
                snapshot[0] != seed || snapshot[1] != (uint)index)
                throw new InvalidOperationException("RNG persisted state header/index mismatch");
            var restored = OriginalPlatformRngCore.FromSnapshot(snapshot);
            for (int i = 0; i < future.Length; ++i)
                if (restored.NextRaw() != future[i])
                    throw new InvalidOperationException("Independent RNG state continuation mismatch at " + i);
        }

        private static void CheckRngConcurrentWriters(RngWriterReference expected, string path)
        {
            RequireFreshRngSchedulePath(path);
            using (var firstCaptured = new ManualResetEventSlim(false))
            using (var secondDrawn = new ManualResetEventSlim(false))
            using (var secondAttempted = new ManualResetEventSlim(false))
            using (var allowFirstSave = new ManualResetEventSlim(false))
            using (var firstSaved = new ManualResetEventSlim(false))
            using (var secondCaptured = new ManualResetEventSlim(false))
            using (var allowSecondSave = new ManualResetEventSlim(false))
            using (var secondFinished = new ManualResetEventSlim(false))
            {
                int draws = 0, captures = 0, saves = 0;
                int firstPocket = -1, secondPocket = -1;
                Exception firstFailure = null, secondFailure = null;
                Action<string, int> observer = (stage, value) =>
                {
                    if (stage == "draw-before-lock" && firstCaptured.IsSet) secondAttempted.Set();
                    else if (stage == "draw" && Interlocked.Increment(ref draws) == 2) secondDrawn.Set();
                    else if (stage == "snapshot")
                    {
                        if (Interlocked.Increment(ref captures) == 1)
                        {
                            firstCaptured.Set();
                            if (!allowFirstSave.Wait(5000)) throw new TimeoutException("First RNG writer barrier");
                        }
                        else
                        {
                            secondCaptured.Set();
                            if (!allowSecondSave.Wait(5000)) throw new TimeoutException("Second RNG writer barrier");
                        }
                    }
                    else if (stage == "saved" && Interlocked.Increment(ref saves) == 1) firstSaved.Set();
                };
                using (var provider = new PcOutcomeSource(
                    new OriginalPlatformRngCore(expected.seed).ExportSnapshot(), path, observer))
                {
                    var first = new Thread(() =>
                    {
                        try { firstPocket = provider.DrawPocket(); }
                        catch (Exception ex) { firstFailure = ex; }
                    }) { IsBackground = true, Name = "Roulette RNG first writer" };
                    var second = new Thread(() =>
                    {
                        try { secondPocket = provider.DrawPocket(); }
                        catch (Exception ex) { secondFailure = ex; }
                        finally { secondFinished.Set(); }
                    }) { IsBackground = true, Name = "Roulette RNG second writer" };
                    bool secondStarted = false;
                    first.Start();
                    try
                    {
                        if (!firstCaptured.Wait(5000)) throw new TimeoutException("First RNG snapshot missing");
                        second.Start(); secondStarted = true;
                        if (!secondAttempted.Wait(5000)) throw new TimeoutException("Second RNG draw did not attempt the lock");
                        // The original outer save lock blocks both another
                        // draw and its subsequent snapshot/file replacement.
                        if (secondDrawn.Wait(200) || secondCaptured.IsSet || secondFinished.IsSet)
                            throw new InvalidOperationException("Concurrent RNG draw bypassed the outer save lock");
                        allowFirstSave.Set();
                        if (!firstSaved.Wait(5000) || !secondCaptured.Wait(5000))
                            throw new TimeoutException("Serialized RNG writers did not advance");
                        CheckRngSavedState(path, expected.seed, expected.firstIndex, expected.firstFuture);
                        allowSecondSave.Set();
                        if (!first.Join(5000) || !second.Join(5000))
                            throw new TimeoutException("Concurrent RNG writers did not finish");
                        if (firstFailure != null || secondFailure != null ||
                            firstPocket != expected.firstPocket || secondPocket != expected.secondPocket ||
                            draws != 2 || captures != 2 || saves != 2 || File.Exists(path + ".new"))
                            throw new InvalidOperationException("Concurrent RNG results or file transactions failed",
                                firstFailure ?? secondFailure);
                        CheckRngSavedState(path, expected.seed, expected.secondIndex, expected.secondFuture);
                        Debug.Log("ROULETTE_RNG_CONCURRENT_WRITERS_PASSED: pockets=" + firstPocket + "," +
                            secondPocket + " savedIndices=" + expected.firstIndex + "," + expected.secondIndex +
                            " futureWords=2600 noPendingFile=True saveBlocksDraw=True");
                    }
                    finally
                    {
                        allowFirstSave.Set(); allowSecondSave.Set();
                        if (!first.Join(5000) || (secondStarted && !second.Join(5000)))
                            throw new TimeoutException("RNG writer fixture did not terminate");
                    }
                }
            }
        }
    }
}
