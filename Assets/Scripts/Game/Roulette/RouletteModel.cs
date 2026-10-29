using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace RouletteDeluxe
{
    public enum BetRejectionReason { TotalLimit, SpotLimit, InsufficientCredit }

    public sealed class BetRejectedException : InvalidOperationException
    {
        public BetRejectionReason Reason { get; private set; }

        public BetRejectedException(BetRejectionReason reason) : base(reason.ToString())
        {
            Reason = reason;
        }
    }

    public sealed class WinRecord
    {
        public int Id;
        public string Type;
        public int MinimumBet;
        public int BetMultiple;
        public int WinLevel;
        public int ReturnMultiple;
        public int[] Numbers;

        public bool Covers(int pocket) { return Array.IndexOf(Numbers, pocket) >= 0; }
        public bool LowRtp { get { return Type == "ROULETTE_STRAIGHT" || Type == "ROULETTE_SPLIT"; } }
    }

    public sealed class RouletteProfile
    {
        public string Id;
        public string PaytableId;
        public string PaytableResourceSha256 { get; private set; }
        public string GameConfigResourceSha256 { get; private set; }
        public int MaximumSpotBet;
        public int MaximumTotalBet;
        public double MinimumRtp;
        public double MaximumRtp;
        public int[] Denominations;
        public double[] CategoryDividers;
        public readonly Dictionary<int, WinRecord> Records = new Dictionary<int, WinRecord>();

        private static string Capture(string source, string pattern)
        {
            Match match = Regex.Match(source, pattern, RegexOptions.Singleline);
            if (!match.Success) throw new FormatException("Missing profile value: " + pattern);
            return match.Groups[1].Value;
        }

        private static int Integer(string source, string name)
        {
            return int.Parse(Capture(source, @"\b" + name + @"\s*=\s*(-?\d+)"), CultureInfo.InvariantCulture);
        }

        private static double Decimal(string source, string name)
        {
            return double.Parse(Capture(source, @"\b" + name + @"\s*=\s*([0-9.]+)"), CultureInfo.InvariantCulture);
        }

        private static string Element(string source, string name)
        {
            return Capture(source, "<" + name + @">\s*([^<]+?)\s*</" + name + ">").Trim();
        }

        private static readonly Dictionary<string,RouletteProfile> resourceSnapshots = new Dictionary<string,RouletteProfile>(StringComparer.Ordinal);
        private static RouletteProfile CopyResourceSnapshot(RouletteProfile source)
        {
            var copy=new RouletteProfile{Id=source.Id,PaytableId=source.PaytableId,PaytableResourceSha256=source.PaytableResourceSha256,GameConfigResourceSha256=source.GameConfigResourceSha256,
                MaximumSpotBet=source.MaximumSpotBet,MaximumTotalBet=source.MaximumTotalBet,MinimumRtp=source.MinimumRtp,MaximumRtp=source.MaximumRtp,
                Denominations=(int[])source.Denominations.Clone(),CategoryDividers=(double[])source.CategoryDividers.Clone()};
            foreach(var pair in source.Records){var r=pair.Value;copy.Records.Add(pair.Key,new WinRecord{Id=r.Id,Type=r.Type,MinimumBet=r.MinimumBet,BetMultiple=r.BetMultiple,WinLevel=r.WinLevel,ReturnMultiple=r.ReturnMultiple,Numbers=(int[])r.Numbers.Clone()});}
            return copy;
        }

        private static System.Diagnostics.Stopwatch BeginProfileTiming()
        {
#if UNITY_EDITOR
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-rouletteProfileTiming")>=0)return System.Diagnostics.Stopwatch.StartNew();
#endif
            return null;
        }
        private static void ProfileTiming(System.Diagnostics.Stopwatch timer,string phase)
        {
            if(timer==null)return;
            Debug.Log("ROULETTE_PROFILE_TIMING phase="+phase+" milliseconds="+timer.Elapsed.TotalMilliseconds.ToString("F3",CultureInfo.InvariantCulture));timer.Restart();
        }
        public static RouletteProfile Load(string suffix)
        {
            var profileTimer=BeginProfileTiming();
            TextAsset paytable = Resources.Load<TextAsset>("Profiles/" + suffix + "/paytable");
            TextAsset config = Resources.Load<TextAsset>("Profiles/" + suffix + "/gameConfig");
            if (paytable == null || config == null) throw new InvalidOperationException("Missing source configuration for " + suffix);
            ProfileTiming(profileTimer,"resource-load");
            var profile = new RouletteProfile();
            using (var hash = SHA256.Create())
            {
                profile.PaytableResourceSha256 = BitConverter.ToString(hash.ComputeHash(paytable.bytes)).Replace("-", "");
                profile.GameConfigResourceSha256 = BitConverter.ToString(hash.ComputeHash(config.bytes)).Replace("-", "");
            }
            ProfileTiming(profileTimer,"resource-hash");
            // Every load observes the actual resource hashes. Only parsing is
            // reused; callers receive independent records and arrays.
            RouletteProfile cached;
            if(resourceSnapshots.TryGetValue(suffix,out cached)&&cached.PaytableResourceSha256==profile.PaytableResourceSha256&&cached.GameConfigResourceSha256==profile.GameConfigResourceSha256)
            {
                var copy=CopyResourceSnapshot(cached);ProfileTiming(profileTimer,"snapshot-copy");return copy;
            }
            string pay=paytable.text,game=config.text;
            profile.Id = Element(game, "GameId");
            profile.PaytableId = Element(game, "PaytableID");
            string payName = Capture(pay, "\\bpaytablename\\s*=\\s*\"([^\"]+)\"");
            if (payName != profile.Id || profile.Id != "RouletteDeluxeSZ" + suffix)
                throw new FormatException("Profile identifiers disagree");
            if (Integer(pay, "doublezero") != 0 || Integer(pay, "enprisonrule") != 0 ||
                Integer(pay, "alwayspaypatterns") != 0)
                throw new FormatException("Unsupported paytable rules");
            profile.MaximumSpotBet = Integer(pay, "betspotmaxwager");
            profile.MaximumTotalBet = Integer(pay, "maxCurrencyWager");
            profile.MinimumRtp = Decimal(pay, "gameminimumpct");
            profile.MaximumRtp = Decimal(pay, "gamemaximumpct");
            profile.Denominations = Element(game, "AllowedDenominations")
                .Split(',').Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
            profile.CategoryDividers = Regex.Matches(pay, @"wagercat\d\dd\d\dpct\s*=\s*([0-9.]+)")
                .Cast<Match>().Select(x => double.Parse(x.Groups[1].Value, CultureInfo.InvariantCulture)).ToArray();

            MatchCollection matches = Regex.Matches(pay, @"\{\{wtype\s*=\s*([A-Z_]+)\}([^}]*)\}");
            foreach (Match match in matches)
            {
                string body = match.Groups[2].Value;
                var record = new WinRecord();
                record.Type = match.Groups[1].Value;
                record.Id = Integer(body, "id");
                record.MinimumBet = Integer(body, "minbet");
                record.BetMultiple = Integer(body, "xbet");
                record.WinLevel = Integer(body, "winlvl");
                record.ReturnMultiple = Integer(body, "win");
                record.Numbers = Regex.Matches(body, @"\bnn\s*=\s*(\d+)").Cast<Match>()
                    .Select(x => int.Parse(x.Groups[1].Value, CultureInfo.InvariantCulture)).ToArray();
                if (record.Numbers.Length == 0 || record.Numbers.Distinct().Count() != record.Numbers.Length ||
                    record.Numbers.Any(x => x < 0 || x > 36) || record.MinimumBet <= 0 ||
                    record.BetMultiple <= 0 || record.WinLevel <= 0 ||
                    record.ReturnMultiple <= 0 || profile.Records.ContainsKey(record.Id))
                    throw new FormatException("Invalid win record " + record.Id);
                double expected = record.LowRtp ? profile.MinimumRtp : profile.MaximumRtp;
                double actual = 100.0 * record.Numbers.Length * record.ReturnMultiple / 37.0;
                if (Math.Abs(expected - actual) > 0.000001) throw new FormatException("RTP mismatch at " + record.Id);
                profile.Records.Add(record.Id, record);
            }
            if (profile.Records.Count != 157 || Enumerable.Range(100, 157).Any(x => !profile.Records.ContainsKey(x)))
                throw new FormatException("Expected win records 100..256");
            if (profile.CategoryDividers.Length != (suffix == "97" ? 0 : 15))
                throw new FormatException("Invalid wager category count");
            if (Math.Abs(double.Parse(Element(game, "GamePct"), CultureInfo.InvariantCulture) -
                         Math.Round(profile.MaximumRtp, 2, MidpointRounding.AwayFromZero)) > 0.000001)
                throw new FormatException("Game RTP differs from paytable");
            ProfileTiming(profileTimer,"parse-rules");
            resourceSnapshots[suffix]=CopyResourceSnapshot(profile);
            return profile;
        }

        public int WagerCategory(IDictionary<int, long> bets)
        {
            long total = 0, low = 0;
            foreach (var pair in bets)
            {
                total += pair.Value;
                if (Records[pair.Key].LowRtp) low += pair.Value;
            }
            if (total == 0) return 0;
            if (CategoryDividers.Length == 0) return 1;
            double rtp = (low * MinimumRtp + (total - low) * MaximumRtp) / total;
            for (int i = 0; i < CategoryDividers.Length; ++i)
                if (rtp >= CategoryDividers[i]) return i + 1;
            return 16;
        }
    }

    public interface IRouletteOutcomeSource
    {
        int DrawPocket();
        // Original RBDU0 feature requests a value in the inclusive range 0..3.
        int DrawDoubleSuit();
    }

    // Pure projection of an already drawn value. Receipt persistence calls
    // this before returning through the same outcome decorators; it must not
    // consume an outcome, change Demo state, or advance a diagnostic counter.
    public interface IRouletteOutcomeMapping
    {
        int ResolvePocket(int raw);
        int ResolveDoubleSuit(int raw);
    }

    // DemoOptionsMenu stores every menu choice for StateDrawStops. Deluxe
    // separately arms a one-shot visual path for fixed choices. Normal/Auto/
    // Turbo replace the mathematical choice but leave that visual path armed.
    public sealed class DemoPocketOutcomeSource : IRouletteOutcomeSource, IRouletteOutcomeMapping, IDisposable
    {
        private readonly IRouletteOutcomeSource source;
        private int selectedOption;
        private int armedVisualPath = -1;

        public int SelectedOption { get { return selectedOption; } }
        public int SelectedPocket { get { return selectedOption >= 3 ? selectedOption - 3 : -1; } }
        public int ArmedVisualPath { get { return armedVisualPath; } }

        public DemoPocketOutcomeSource(IRouletteOutcomeSource source)
        {
            if (source == null) throw new ArgumentNullException("source");
            this.source = source;
        }

        public void SelectOption(int option, bool armVisualPath = true)
        {
            if (option < 0 || option >= 40)
                throw new ArgumentOutOfRangeException("option");
            selectedOption = option;
            if (armVisualPath && option >= 3) ArmSelectedVisualPath();
        }

        public void ArmSelectedVisualPath()
        {
            if (selectedOption < 3)
                throw new InvalidOperationException("Only a fixed demo choice arms a wheel path");
            armedVisualPath = selectedOption - 3;
        }

        public void SelectFixedPocket(int pocket)
        {
            if (pocket < 0 || pocket > 36)
                throw new ArgumentOutOfRangeException("pocket");
            SelectOption(pocket + 3);
        }

        // DemoOptionsMenu::ResetDemoMode does not clear RouletteWheel's arm.
        public void ClearSelection() { selectedOption = 0; }

        public bool TryConsumeVisualPath(int pathCount, out int path)
        {
            if (pathCount <= 0) throw new ArgumentOutOfRangeException("pathCount");
            path = armedVisualPath;
            if (path < 0) return false;
            armedVisualPath = -1;
            if (path >= pathCount) path = 0;
            return true;
        }

        public int DrawPocket()
        {
            int randomPocket = source.DrawPocket();
            int selected = SelectedPocket;
            if (randomPocket < 0 || randomPocket > 36 || selected < 0)
                return randomPocket;
            ClearSelection();
            Debug.Log("ROULETTE_DEMO_FIXED_STOP: rng=" + randomPocket +
                      " selected=" + selected);
            return selected;
        }

        public int DrawDoubleSuit() { return source.DrawDoubleSuit(); }

        public int ResolvePocket(int raw)
        {
            var mapping = source as IRouletteOutcomeMapping;
            int value = mapping == null ? raw : mapping.ResolvePocket(raw);
            return value >= 0 && value <= 36 && SelectedPocket >= 0 ? SelectedPocket : value;
        }

        public int ResolveDoubleSuit(int raw)
        {
            var mapping = source as IRouletteOutcomeMapping;
            return mapping == null ? raw : mapping.ResolveDoubleSuit(raw);
        }

        internal void RestoreReceiptVisualPath(int path)
        {
            if (path < -1 || path > 36) throw new ArgumentOutOfRangeException("path");
            armedVisualPath = path;
        }

        public void Dispose()
        {
            var disposable = source as IDisposable;
            if (disposable != null) disposable.Dispose();
        }
    }

    public static class OriginalDoubleSuit
    {
        // RBDU0: 0 club, 1 spade, 2 diamond, 3 heart.
        // The original bitmap order in this view is heart, diamond, club, spade.
        public static int ViewIndex(int value)
        {
            switch (value)
            {
                case 0: return 2;
                case 1: return 3;
                case 2: return 1;
                case 3: return 0;
                default: throw new ArgumentOutOfRangeException("value");
            }
        }

        public static bool IsRed(int value) { return value >= 2 && value <= 3; }
    }

    // The signed platform GetRandom path uses Boost uniform_int to map a
    // 32-bit engine word into a small inclusive range. A high tail is retried.
    // This matches its range reduction, not its NV-seeded engine stream.
    public static class OriginalRngRange
    {
        public static int MapRaw(uint raw, int exclusive)
        {
            if (exclusive <= 0) throw new ArgumentOutOfRangeException("exclusive");
            ulong quantum = 0x100000000UL / (uint)exclusive;
            ulong mapped = raw / quantum;
            return mapped < (uint)exclusive ? (int)mapped : -1;
        }
    }

    // PC-only ordinary gamemgr path. Its seed is local because the cabinet's
    // persisted NV state is unavailable; Android/SBox must supply its own authority.
    public sealed class PcOutcomeSource : IRouletteOutcomeSource, IDisposable
    {
        private sealed class PreparedOperation
        {
            public PcRngOperationContext context;
            public string digest;
            public Func<int, int> resolver;
            public int ownerThread;
        }

        private readonly object sync = new object();
        private readonly OriginalPlatformRngCore core;
        private readonly Thread mixThread;
        private readonly string statePath;
        private readonly Action<string, int> diagnosticStep;
        private volatile bool disposed;
        private volatile bool faulted;
        private Exception fault;
        private int stateVersion = 2;
        private PcRngDrawReceipt receipt;
        private bool receiptAcknowledged;
        private PreparedOperation prepared;
        private bool receiptDrawInFlight;
        private int rawDrawsInFlight;
        private Action<string, PcRngDrawReceipt> receiptCheckpoint;

        public PcOutcomeSource()
        {
            statePath = PcRngStateStore.PathForCurrentProcess;
            PcRngStoredState stored = PcRngStateStore.LoadStrict(statePath);
            if (stored != null)
            {
                core = OriginalPlatformRngCore.FromSnapshot(stored.Snapshot);
                stateVersion = stored.Version;
                receipt = stored.Receipt;
                Debug.Log("ROULETTE_PC_RNG_STATE: restored");
            }
            else
            {
                var bytes = new byte[4];
                uint seed;
                using (var seedSource = RandomNumberGenerator.Create())
                {
                    do
                    {
                        seedSource.GetBytes(bytes);
                        seed = BitConverter.ToUInt32(bytes, 0);
                    } while (seed == 0);
                }
                core = new OriginalPlatformRngCore(seed);
                PcRngStateStore.InitializeNew(statePath, core.ExportSnapshot());
                Debug.Log("ROULETTE_PC_RNG_STATE: initialized");
            }
            mixThread = new Thread(MixLoop) { IsBackground = true,
                Name = "Roulette PC RNG mix" };
            mixThread.Start();
        }

        // Compiled diagnostics use the actual draw/save and mix lock paths,
        // with explicit barriers and an isolated state file. Ordinary play
        // never supplies this observer or suppresses its background thread.
        internal PcOutcomeSource(uint[] snapshot, string path, Action<string, int> step)
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-rouletteRngScheduleSmoke") < 0 ||
                step == null ||
                !System.IO.Path.GetFileName(path).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal) ||
                !string.Equals(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path)),
                    System.IO.Path.GetFullPath(Application.persistentDataPath), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("RNG schedule diagnostics require an isolated state path and flag");
            core = OriginalPlatformRngCore.FromSnapshot(snapshot);
            statePath = path;
            diagnosticStep = step;
        }

        private PcOutcomeSource(PcRngStoredState stored, string path)
        {
            core = OriginalPlatformRngCore.FromSnapshot(stored.Snapshot);
            statePath = path;
            stateVersion = stored.Version;
            receipt = stored.Receipt;
        }

        // A fixed engine state and no live mix make the explicit receipt
        // fixture reproducible without substituting its actual draw or writer.
        internal static PcOutcomeSource CreateReceiptDiagnostic(uint[] snapshot, string path)
        {
            ValidateReceiptDiagnosticPath(path);
            PcRngStoredState stored = PcRngStateStore.LoadStrict(path);
            if (snapshot != null)
            {
                if (stored != null)
                    throw new InvalidOperationException("Fresh RNG receipt diagnostics cannot overwrite existing state");
                PcRngStateStore.InitializeNew(path, snapshot);
                stored = PcRngStateStore.LoadStrict(path);
                Debug.Log("ROULETTE_PC_RNG_STATE: initialized");
            }
            else
            {
                if (stored == null)
                    throw new InvalidOperationException("Cold RNG receipt diagnostics require existing state");
                Debug.Log("ROULETTE_PC_RNG_STATE: restored");
            }
            return new PcOutcomeSource(stored, path);
        }

        private static void ValidateReceiptDiagnosticPath(string path)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (string.IsNullOrEmpty(path) ||
                (Array.IndexOf(args, "-rouletteRngReceiptSmoke") < 0 &&
                    Array.IndexOf(args, "-rouletteAutoCompleteSmoke") < 0 &&
                    Array.IndexOf(args, "-rouletteGameSuspensionSmoke") < 0 &&
                    Array.IndexOf(args, "-rouletteGamePaymentSmoke") < 0 &&
                    Array.IndexOf(args, "-rouletteAutoCompleteLegacySmoke") < 0) ||
                Array.IndexOf(args, "-rouletteRngStateTestKey") < 0 ||
                !System.IO.Path.GetFileName(path).StartsWith("roulette_platform_rng_test_", StringComparison.Ordinal) ||
                !string.Equals(System.IO.Path.GetFullPath(path),
                    System.IO.Path.GetFullPath(PcRngStateStore.PathForCurrentProcess), StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("RNG receipt diagnostics require an isolated state path and flag");
        }

        public static string ContextDigest(PcRngOperationContext context)
        {
            return PcRngDrawReceipt.ContextDigest(context);
        }

        public PcRngDrawReceipt Receipt
        {
            get { lock (sync) return PcRngDrawReceipt.Clone(receipt); }
        }

        public bool HasUnacknowledgedReceipt
        {
            get { lock (sync) return receipt != null && !receiptAcknowledged; }
        }

        public void Prepare(PcRngOperationContext context, Func<int, int> effectiveResolver)
        {
            if (effectiveResolver == null) throw new ArgumentNullException("effectiveResolver");
            PcRngOperationContext copy = PcRngDrawReceipt.CloneContext(context);
            string digest = ContextDigest(copy);
            int owner = Thread.CurrentThread.ManagedThreadId;
            lock (sync)
            {
                EnsureAvailable();
                if (receiptDrawInFlight || rawDrawsInFlight != 0)
                    throw new InvalidOperationException("PC RNG has an in-flight draw");
                if (prepared != null)
                {
                    if (prepared.ownerThread == owner && prepared.digest == digest &&
                        prepared.context.operationId == copy.operationId) return;
                    throw new InvalidOperationException("PC RNG already has a prepared operation");
                }
                if (receipt != null)
                {
                    if (receipt.context.operationId == copy.operationId)
                    {
                        if (receipt.contextSha256 != digest)
                            throw new InvalidOperationException("PC RNG operation context conflicts with its receipt");
                    }
                    else if (!receiptAcknowledged)
                        throw new InvalidOperationException("PC RNG receipt has not been acknowledged");
                }
                prepared = new PreparedOperation {
                    context = copy, digest = digest, resolver = effectiveResolver, ownerThread = owner
                };
            }
        }

        // The caller must first verify its durable Pending/Archive consumer.
        // Keep the saved receipt until a subsequent operation commits; taking
        // a new snapshot here would persist unrelated background mix words.
        public void Acknowledge(string operationId, string contextSha256)
        {
            lock (sync)
            {
                EnsureAvailable();
                if (prepared != null || receiptDrawInFlight || rawDrawsInFlight != 0 ||
                    receipt == null || receipt.context.operationId != operationId ||
                    receipt.contextSha256 != contextSha256)
                    throw new InvalidOperationException("PC RNG receipt acknowledgement does not match");
                receiptAcknowledged = true;
            }
        }

        internal void ConfigureReceiptDiagnostics(Action<string, PcRngDrawReceipt> checkpoint)
        {
            if (checkpoint == null) throw new ArgumentNullException("checkpoint");
            ValidateReceiptDiagnosticPath(statePath);
            lock (sync)
            {
                EnsureAvailable();
                if (prepared != null || receiptDrawInFlight || rawDrawsInFlight != 0 || receiptCheckpoint != null)
                    throw new InvalidOperationException("RNG receipt diagnostics are already active");
                receiptCheckpoint = checkpoint;
            }
        }

        private void ReceiptCheckpoint(string name, PcRngDrawReceipt value)
        {
            if (receiptCheckpoint != null) receiptCheckpoint(name, PcRngDrawReceipt.Clone(value));
        }

        private void EnsureAvailable()
        {
            if (disposed) throw new ObjectDisposedException("PcOutcomeSource");
            if (faulted) throw new InvalidOperationException("PC RNG stopped after a result persistence failure", fault);
        }

        private void DiagnosticStep(string name, int value)
        {
            if (diagnosticStep != null) diagnosticStep(name, value);
        }

        internal void MixForDiagnostic()
        {
            if (diagnosticStep == null || mixThread != null)
                throw new InvalidOperationException("Manual RNG mix is diagnostic only");
            MixBatch();
        }

        private void MixBatch()
        {
            int advance;
            lock (sync)
            {
                if (disposed || faulted) return;
                // RngMixMasterThread draws its 0..5 count under one lock.
                advance = 10 + core.NextInclusive(0, 5);
            }
            DiagnosticStep("mix-count", advance);
            // The source releases the mutex between each of the 10..15
            // subsequent engine words. Game requests may interleave here.
            for (int i = 0; i < advance && !disposed && !faulted; ++i)
            {
                DiagnosticStep("mix-before-word", i + 1);
                lock (sync)
                {
                    if (disposed || faulted) return;
                    core.NextRaw();
                }
                DiagnosticStep("mix-word", i + 1);
            }
        }

        private void MixLoop()
        {
            while (!disposed && !faulted)
            {
                MixBatch();
                Thread.Sleep(100);
            }
        }

        private int Draw(int exclusive)
        {
            int result = -1;
            bool engineTouched = false;
            bool ownsRawDraw = false;
            bool ownsReceiptDraw = false;
            PcRngDrawReceipt candidate = null;
            DiagnosticStep("draw-before-lock", exclusive);
            try
            {
                lock (sync)
                {
                    EnsureAvailable();
                    PreparedOperation operation = prepared;
                    if (operation == null)
                    {
                        if (receipt != null || receiptDrawInFlight)
                            throw new InvalidOperationException("PC RNG draw requires a prepared operation");
                        // Preserve the existing raw-only concurrent diagnostic
                        // channel. It can never overwrite a retained receipt.
                        ++rawDrawsInFlight;
                        ownsRawDraw = true;
                    }
                    else
                    {
                        if (operation.ownerThread != Thread.CurrentThread.ManagedThreadId ||
                            receiptDrawInFlight || rawDrawsInFlight != 0 ||
                            (operation.context.kind == 1 ? 37 : 4) != exclusive)
                            throw new InvalidOperationException("PC RNG draw does not match its prepared operation");
                        if (receipt != null && receipt.context.operationId == operation.context.operationId)
                        {
                            if (receipt.contextSha256 != operation.digest ||
                                operation.resolver(receipt.rawResult) != receipt.effectiveResult)
                                throw new InvalidOperationException("PC RNG replay conflicts with its saved result");
                            prepared = null;
                            DiagnosticStep("replayed", receipt.rawResult);
                            return receipt.rawResult;
                        }
                        if (receipt != null && !receiptAcknowledged)
                            throw new InvalidOperationException("PC RNG receipt has not been acknowledged");
                        receiptDrawInFlight = true;
                        ownsReceiptDraw = true;
                    }
                    engineTouched = true;
                    result = core.NextInclusive(0, exclusive - 1);
                    if (operation != null)
                    {
                        candidate = new PcRngDrawReceipt {
                            context = operation.context, contextSha256 = operation.digest,
                            rawResult = result, effectiveResult = operation.resolver(result)
                        };
                        PcRngDrawReceipt.Validate(candidate);
                    }
                }
                DiagnosticStep("draw", result);
                // Preserve the source unlock/relock interval: mix can run
                // here. A receipt operation remains reserved without holding
                // the engine mutex, so another draw cannot overwrite it.
                lock (sync)
                {
                    EnsureAvailable();
                    uint[] snapshot = core.ExportSnapshot();
                    // This outer engine lock includes serialization, candidate
                    // flush and replacement, as in GetRandArray/StoreRngData.
                    DiagnosticStep("snapshot", (int)snapshot[1]);
                    if (candidate != null)
                    {
                        PcRngStateStore.SaveWithReceipt(statePath, snapshot, candidate,
                            receiptCheckpoint == null ? (Action)null :
                                () => ReceiptCheckpoint("before-replace", candidate));
                        receipt = PcRngDrawReceipt.Clone(candidate);
                        receiptAcknowledged = false;
                        stateVersion = 3;
                        prepared = null;
                        receiptDrawInFlight = false;
                        ownsReceiptDraw = false;
                        ReceiptCheckpoint("saved", receipt);
                    }
                    else
                    {
                        if (stateVersion >= 3) PcRngStateStore.SaveWithReceipt(statePath, snapshot, null);
                        else PcRngStateStore.Save(statePath, snapshot);
                        --rawDrawsInFlight;
                        ownsRawDraw = false;
                        stateVersion = Math.Max(stateVersion, 2);
                    }
                }
                DiagnosticStep("saved", result);
                return result;
            }
            catch (Exception error)
            {
                if (engineTouched)
                    lock (sync)
                    {
                        fault = error;
                        faulted = true;
                        if (ownsRawDraw) --rawDrawsInFlight;
                        if (ownsReceiptDraw) receiptDrawInFlight = false;
                    }
                throw;
            }
        }

        public int DrawPocket() { return Draw(37); }
        public int DrawDoubleSuit() { return Draw(4); }
        public void Dispose()
        {
            disposed = true;
            if (mixThread != null) mixThread.Join(200);
        }
    }

    public sealed class WinningPayRecord
    {
        public int BetId;
        public long Amount;
        public int WinLevel;
    }

    public sealed class HistoryPayOverride
    {
        // GameHistory stores an index into RouletteResult's sorted pay array.
        public int PayIndex;
        public long Amount;
        public int ProgressiveLevel;
        public int Kind;
    }

    public sealed class RoundResult
    {
        public int Pocket;
        public long Wager;
        public long Returned;
        public int Category;
        public bool WinLimitReached;
        public List<WinningPayRecord> WinningPays = new List<WinningPayRecord>();
        public List<HistoryPayOverride> HistoryPayOverrides =
            new List<HistoryPayOverride>();
    }

    // A future SBox session implements the whole contract; Unity's screen never settles bets itself.
    public interface IRouletteSession
    {
        RouletteProfile Profile { get; }
        Dictionary<int, long> Bets { get; }
        Dictionary<int, List<long>> BetChipStacks { get; }
        List<RoundResult> History { get; }
        long Credit { get; }
        long Denomination { get; }
        long Chip { get; }
        long Wagered { get; }
        long Returned { get; }
        long Rounds { get; }
        long TotalBet { get; }
        bool CanUndo { get; }
        int LastUndoSourceSpot { get; }
        int LastUndoRemovalSoundCount { get; }
        bool LastUndoRequestsBetState { get; }
        int Wins { get; }
        RoundResult LastResult { get; }
        bool SpinPending { get; }
        int PendingPocket { get; }
        bool ResultPhase { get; }
        bool DoubleEnabled { get; set; }
        bool DoubleSuitMode { get; }
        bool DoubleAvailable { get; }
        long DoubleStake { get; }
        int DoubleRounds { get; }
        int DoubleMaximumRounds { get; }
        long DoubleMaximumWin { get; }
        long BaseMaximumWin { get; }
        bool InDoubleUp { get; }
        int LastDoubleCard { get; }
        void SetDenomination(long value);
        void SetChip(long value);
        void PlaceBet(int id, long amount);
        void PlaceNeighbor(int number);
        bool CanPlaceNeighbor(int number);
        BetRejectionReason? NeighborRejectionReason(int number);
        void PlaceSector(RouletteSector sector);
        void Undo();
        void Clear();
        void Repeat();
        int BeginSpin();
        RoundResult CompleteSpin();
        void CancelSpin();
        RoundResult Spin();
        void NextRound();
        void StartDouble();
        bool GuessDouble(int choice);
        void CollectDouble();
    }

    public enum RouletteSector { Orphelins, Tiers, Voisins }

    // BetSpot IDs from the original filedefs.xml. Repeated IDs place another
    // chip on the same spot; Voisins deliberately doubles 155 and 145.
    public static class OriginalSectorBets
    {
        private static readonly int[] Orphelins = { 1, 101, 109, 112, 126 };
        private static readonly int[] Tiers = { 100, 78, 108, 87, 122, 128 };
        private static readonly int[] Voisins = { 155, 155, 99, 107, 113, 114, 145, 145, 127 };

        public static int[] Spots(RouletteSector sector)
        {
            switch (sector)
            {
                case RouletteSector.Orphelins: return (int[])Orphelins.Clone();
                case RouletteSector.Tiers: return (int[])Tiers.Clone();
                case RouletteSector.Voisins: return (int[])Voisins.Clone();
                default: throw new ArgumentOutOfRangeException("sector");
            }
        }
    }

    public sealed class RouletteSession : IRouletteSession
    {
        private static readonly int[] WheelOrder = {
            0,32,15,19,4,21,2,25,17,34,6,27,13,36,11,30,8,23,10,
            5,24,16,33,1,20,14,31,9,22,18,29,7,28,12,35,3,26
        };
        public RouletteProfile Profile { get; private set; }
        public Dictionary<int, long> Bets { get; private set; } = new Dictionary<int, long>();
        public Dictionary<int, List<long>> BetChipStacks { get; private set; } =
            new Dictionary<int, List<long>>();
        public List<RoundResult> History { get; private set; } = new List<RoundResult>();
        private readonly IRouletteOutcomeSource outcome;
        internal IRouletteOutcomeSource OutcomeSource { get { return outcome; } }
        private readonly long doubleMaximumWin;
        private readonly long baseMaximumWin;
        private readonly int doubleMaximumRounds;
        private readonly bool doubleSuitMode;
        private Dictionary<int, long> previous = new Dictionary<int, long>();
        private Dictionary<int, List<long>> previousStacks = new Dictionary<int, List<long>>();
        private List<int> betEvaluationOrder = new List<int>();
        private List<int> previousEvaluationOrder = new List<int>();
        private sealed class BettingSnapshot
        {
            public Dictionary<int, long> Bets;
            public Dictionary<int, List<long>> Stacks;
            public List<int> Order;
            public int[] GroupRemovalOrder;
            public int LastTouched;
            public int SourceSpotPlusOne;
            public bool PlaysRemovalSound;
        }
        public sealed class SavedUndoState
        {
            public Dictionary<int, List<long>> Stacks;
            public int[] Order;
            public int[] GroupRemovalOrder;
            public int LastTouched;
            public int SourceSpotPlusOne;
            public bool PlaysRemovalSound;
        }
        private readonly List<BettingSnapshot> undoHistory = new List<BettingSnapshot>();
        private bool groupingBet;
        private int lastTouched = -1;
        public bool CanUndo { get { return undoHistory.Count > 0; } }
        public int LastUndoSourceSpot { get {
            return undoHistory.Count == 0 ? -1 :
                undoHistory[undoHistory.Count - 1].SourceSpotPlusOne - 1;
        } }
        public int LastTouchedBet { get { return lastTouched; } }
        public int[] BetEvaluationOrder { get { return betEvaluationOrder.ToArray(); } }
        public int LastUndoRemovalSoundCount { get; private set; }
        public bool LastUndoRequestsBetState { get; private set; }
        public long Credit { get; private set; }
        public long Denomination { get; private set; }
        public long MoneyRemainderCents { get; private set; }
        public long MoneyCents { get { return checked(Credit * Denomination + MoneyRemainderCents); } }
        public long Chip { get; private set; }
        public long Wagered { get; private set; }
        public long Returned { get; private set; }
        public long Rounds { get; private set; }
        public int Wins { get; private set; }
        public RoundResult LastResult { get; private set; }
        public bool SpinPending { get; private set; }
        public int PendingPocket { get; private set; } = -1;
        public bool ResultPhase { get; private set; }
        public bool DoubleEnabled { get; set; }
        public bool DoubleSuitMode { get { return doubleSuitMode; } }
        public bool DoubleAvailable { get; private set; }
        public long DoubleStake { get; private set; }
        public int DoubleRounds { get; private set; }
        public int DoubleMaximumRounds { get { return doubleMaximumRounds; } }
        public long DoubleMaximumWin { get { return doubleMaximumWin; } }
        public long BaseMaximumWin { get { return baseMaximumWin; } }
        public bool InDoubleUp { get; private set; }
        // New tracked PC rounds keep their evaluated award unpaid until the
        // Game payout boundary. Old archives retain their paid-base meaning,
        // even when this opt-in is enabled for the next newly played round.
        public bool BaseSettlementDeferred { get; private set; }
        public bool RoundPayoutPending { get; private set; }
        // Native FeatureComplete prepares new PC feature awards; the later
        // Game payout boundary applies them. Historical model callers keep
        // their immediate settlement semantics.
        public bool DoubleSettlementDeferred { get; private set; }
        public bool DoubleChoiceTerminal { get; private set; }
        public int LastDoubleCard { get; private set; } = -1;

        public RouletteSession(RouletteProfile profile, long initialCredit,
            IRouletteOutcomeSource outcomeSource, long doubleMaximumWin = long.MaxValue,
            int doubleMaximumRounds = 16, bool doubleSuitMode = false,
            long baseMaximumWin = 0)
        {
            if (outcomeSource == null) throw new ArgumentNullException("outcomeSource");
            if (doubleMaximumWin < 1) throw new ArgumentOutOfRangeException("doubleMaximumWin");
            if (baseMaximumWin < 0) throw new ArgumentOutOfRangeException("baseMaximumWin");
            if (doubleMaximumRounds < 1 || doubleMaximumRounds > 16)
                throw new ArgumentOutOfRangeException("doubleMaximumRounds");
            Profile = profile;
            outcome = outcomeSource;
            // StateEvaluate uses the resolved GetWinLimit (zero means no
            // ordinary cap). StateBonus passes the same cap to Double Up,
            // substituting the source 2,000,000,000 fallback when it is zero.
            // An explicit doubleMaximumWin remains a PC diagnostic override.
            this.baseMaximumWin = baseMaximumWin;
            this.doubleMaximumWin = baseMaximumWin > 0 ?
                Math.Min(baseMaximumWin, doubleMaximumWin) :
                doubleMaximumWin == long.MaxValue ? 2000000000L : doubleMaximumWin;
            this.doubleMaximumRounds = doubleMaximumRounds;
            this.doubleSuitMode = doubleSuitMode;
            Credit = initialCredit;
            Denomination = 1;
            Chip = 1;
        }

        public long TotalBet { get { return Bets.Values.Sum(); } }

        // Only the explicitly enabled PC attendant demo path uses this.
        // The original platform credit meter is outside the session model.
        public void AddDemoCredits(long units)
        {
            if (units <= 0) throw new ArgumentOutOfRangeException("units");
            if (SpinPending || InDoubleUp || RoundPayoutPending)
                throw new InvalidOperationException("Demo credits require an idle game");
            long newCredit = checked(Credit + units);
            checked { long totalCents = newCredit * Denomination + MoneyRemainderCents; }
            Credit = newCredit;
        }

        // Credit comes from the cabinet platform, independently of a bet or
        // Double Up settlement. The PC platform adapter uses the same entry
        // point after recording its simulated credit-change event.
        public void ApplyPlatformCredit(long absoluteCredit)
        {
            if (absoluteCredit < 0) throw new ArgumentOutOfRangeException("absoluteCredit");
            if (SpinPending || InDoubleUp || RoundPayoutPending)
                throw new InvalidOperationException("Platform credit changed during a game transaction");
            checked { long totalCents = absoluteCredit * Denomination + MoneyRemainderCents; }
            Credit = absoluteCredit;
        }

        // PC journal replay and platform credit notifications commit all
        // three monetary fields together. A remainder is never a playable
        // credit, but remains part of the money available for cashout.
        public void ApplyPlatformMoneyState(long credits, long denomination,
            long remainderCents)
        {
            if (credits < 0 || denomination < 1 ||
                denomination > int.MaxValue ||
                !Profile.Denominations.Contains((int)denomination) ||
                remainderCents < 0 || remainderCents >= denomination)
                throw new ArgumentOutOfRangeException("Invalid platform money state");
            if (SpinPending || InDoubleUp || RoundPayoutPending)
                throw new InvalidOperationException("Platform money changed during a game transaction");
            checked { long totalCents = credits * denomination + remainderCents; }
            Credit = credits;
            Denomination = denomination;
            MoneyRemainderCents = remainderCents;
        }

        private void PushUndo(Dictionary<int, long> bets,
            Dictionary<int, List<long>> stacks, int touched,
            bool playsRemovalSound = true, List<int> order = null,
            int[] groupRemovalOrder = null, int sourceSpotPlusOne = 0)
        {
            if (undoHistory.Count == 10) undoHistory.RemoveAt(0);
            undoHistory.Add(new BettingSnapshot {
                Bets = new Dictionary<int, long>(bets),
                Stacks = CopyStacks(stacks),
                Order = new List<int>(order ?? betEvaluationOrder),
                GroupRemovalOrder = groupRemovalOrder == null ? null :
                    (int[])groupRemovalOrder.Clone(),
                LastTouched = touched,
                SourceSpotPlusOne = sourceSpotPlusOne,
                PlaysRemovalSound = playsRemovalSound
            });
        }

        public SavedUndoState[] ExportUndoHistory()
        {
            return undoHistory.Select(state => new SavedUndoState {
                Stacks = CopyStacks(state.Stacks),
                Order = state.Order.ToArray(),
                GroupRemovalOrder = state.GroupRemovalOrder == null ? null :
                    (int[])state.GroupRemovalOrder.Clone(),
                LastTouched = state.LastTouched,
                SourceSpotPlusOne = state.SourceSpotPlusOne,
                PlaysRemovalSound = state.PlaysRemovalSound
            }).ToArray();
        }

        public void SetDenomination(long value)
        {
            if (SpinPending || InDoubleUp || ResultPhase || TotalBet != 0)
                throw new InvalidOperationException("Denomination change requires an idle empty table");
            if (value < 1 || value > int.MaxValue ||
                !Profile.Denominations.Contains((int)value))
                throw new ArgumentException("Unsupported denomination");
            long totalCents = MoneyCents;
            Denomination = value;
            Credit = totalCents / value;
            MoneyRemainderCents = totalCents % value;
            Chip = 1;
            undoHistory.Clear();
            previous.Clear();
            previousStacks.Clear();
            previousEvaluationOrder.Clear();
        }

        public void SetChip(long value)
        {
            if (SpinPending) throw new InvalidOperationException("Wheel is spinning");
            if (!IsSelectableChip(value)) throw new ArgumentException("Unsupported chip");
            Chip = value;
        }

        private static bool IsSelectableChip(long value)
        {
            return value == 1 || value == 2 || value == 5 || value == 10;
        }

        private static List<int> RestoreBetOrder(int[] saved,
            Dictionary<int, long> bets)
        {
            if (saved == null)
                return bets.Keys.OrderBy(id => id).ToList();
            if (saved.Length != bets.Count || saved.Distinct().Count() != saved.Length ||
                saved.Any(id => !bets.ContainsKey(id)))
                throw new InvalidOperationException("Invalid saved bet evaluation order");
            return new List<int>(saved);
        }

        private void AssertBetting()
        {
            if (SpinPending || ResultPhase || InDoubleUp)
                throw new InvalidOperationException("Begin the next round first");
        }

        public void PlaceBet(int id, long amount)
        {
            if (SpinPending || InDoubleUp) AssertBetting();
            WinRecord record;
            if (!Profile.Records.TryGetValue(id, out record)) throw new ArgumentException("Unknown bet");
            long prior = Bets.ContainsKey(id) ? Bets[id] : 0;
            if (amount <= 0) throw new InvalidOperationException("Invalid bet amount");
            long next = checked(prior + amount);
            if (next < record.MinimumBet || next % record.BetMultiple != 0)
                throw new InvalidOperationException("Invalid bet amount");
            long total = TotalBet;
            if (amount > Profile.MaximumTotalBet - total)
                throw new BetRejectedException(BetRejectionReason.TotalLimit);
            if (amount > Credit - total)
                throw new BetRejectedException(BetRejectionReason.InsufficientCredit);
            if (next > Profile.MaximumSpotBet)
                throw new BetRejectedException(BetRejectionReason.SpotLimit);
            if (prior == 0 && Bets.Count >= Profile.Records.Count) throw new InvalidOperationException("Too many bet spots");
            // A rejected touch leaves the settled result and Double Up offer
            // intact. Enter the next betting round only after validation.
            if (ResultPhase) NextRound();
            if (!groupingBet) PushUndo(Bets, BetChipStacks, lastTouched,
                sourceSpotPlusOne: id - 99);
            Bets[id] = next;
            // Direct table touches unhook/reappend an active spot. The
            // original sector and neighbor group routines instead hook only
            // spots whose state was inactive; covered active spots keep their
            // existing position even when another chip is added.
            if (prior == 0) betEvaluationOrder.Add(id);
            else if (!groupingBet)
            {
                betEvaluationOrder.Remove(id);
                betEvaluationOrder.Add(id);
            }
            if (!BetChipStacks.ContainsKey(id)) BetChipStacks[id] = new List<long>();
            BetChipStacks[id].Add(amount);
            lastTouched = id;
        }

        public void PlaceNeighbor(int number)
        {
            PlaceGroup(NeighborSpots(number), number);
        }

        public bool CanPlaceNeighbor(int number)
        {
            return ValidateNeighbor(number, out _);
        }

        public BetRejectionReason? NeighborRejectionReason(int number)
        {
            BetRejectionReason? reason;
            ValidateNeighbor(number, out reason);
            return reason;
        }

        private bool ValidateNeighbor(int number, out BetRejectionReason? reason)
        {
            reason = null;
            if (SpinPending || InDoubleUp) return false;
            int[] spots;
            try { spots = NeighborSpots(number); }
            catch (ArgumentException) { return false; }
            long added = Chip * spots.Length;
            if (TotalBet + added > Credit)
            {
                reason = BetRejectionReason.InsufficientCredit;
                return false;
            }
            if (TotalBet + added > Profile.MaximumTotalBet)
            {
                reason = BetRejectionReason.TotalLimit;
                return false;
            }
            if (Bets.Count + spots.Count(x => !Bets.ContainsKey(100 + x)) >
                Profile.Records.Count)
                return false;
            foreach (int spot in spots)
            {
                int id = 100 + spot;
                WinRecord record = Profile.Records[id];
                long next = (Bets.ContainsKey(id) ? Bets[id] : 0) + Chip;
                if (next > Profile.MaximumSpotBet)
                {
                    reason = BetRejectionReason.SpotLimit;
                    return false;
                }
                if (next < record.MinimumBet || next % record.BetMultiple != 0)
                    return false;
            }
            return true;
        }

        private static int[] NeighborSpots(int number)
        {
            int index = Array.IndexOf(WheelOrder, number);
            if (index < 0) throw new ArgumentException("Invalid pocket");
            var spots = new List<int>(4);
            for (int offset = -2; offset <= 2; ++offset)
            {
                int pocket = WheelOrder[(index + offset + WheelOrder.Length) % WheelOrder.Length];
                // The source neighbor list contains the centre too, but the
                // original PlaceBetsForNeighborsOf skips it.
                if (pocket != number) spots.Add(pocket);
            }
            return spots.ToArray();
        }

        public void PlaceSector(RouletteSector sector)
        {
            int sourceSpot = sector == RouletteSector.Orphelins ? 117 :
                sector == RouletteSector.Tiers ? 125 : 133;
            PlaceGroup(OriginalSectorBets.Spots(sector), sourceSpot);
        }

        private void PlaceGroup(IEnumerable<int> spotIds, int sourceSpot)
        {
            if (SpinPending || InDoubleUp) AssertBetting();
            int[] spots = spotIds.ToArray();
            long added = checked(Chip * spots.LongLength);
            // NeighborsGadget::ValidateBets checks the whole group against
            // available credit, then the table limit, before any spot check.
            if (TotalBet + added > Credit)
                throw new BetRejectedException(BetRejectionReason.InsufficientCredit);
            if (TotalBet + added > Profile.MaximumTotalBet)
                throw new BetRejectedException(BetRejectionReason.TotalLimit);
            // Validate the entire group, including repeated sector spots,
            // before leaving a completed result or changing any bet.
            var projected = new Dictionary<int, long>(Bets);
            foreach (int spot in spots)
            {
                int id = 100 + spot;
                WinRecord record = Profile.Records[id];
                long prior = projected.ContainsKey(id) ? projected[id] : 0;
                long next = checked(prior + Chip);
                if (next < record.MinimumBet || next % record.BetMultiple != 0)
                    throw new InvalidOperationException("Invalid bet amount");
                if (next > Profile.MaximumSpotBet)
                    throw new BetRejectedException(BetRejectionReason.SpotLimit);
                projected[id] = next;
            }
            if (projected.Count > Profile.Records.Count)
                throw new InvalidOperationException("Too many bet spots");
            if (ResultPhase) NextRound();
            var snapshot = new Dictionary<int, long>(Bets);
            var stackSnapshot = CopyStacks(BetChipStacks);
            var orderSnapshot = new List<int>(betEvaluationOrder);
            int oldLast = lastTouched;
            try
            {
                groupingBet = true;
                foreach (int spot in spots)
                    PlaceBet(100 + spot, Chip);
            }
            catch
            {
                Bets.Clear();
                foreach (var pair in snapshot) Bets.Add(pair.Key, pair.Value);
                BetChipStacks = stackSnapshot;
                betEvaluationOrder = orderSnapshot;
                lastTouched = oldLast;
                throw;
            }
            finally { groupingBet = false; }
            // Source group-removal functions do not call the 0x172 remove sound.
            PushUndo(snapshot, stackSnapshot, oldLast, false, orderSnapshot,
                spots.Select(spot => 100 + spot).ToArray(), sourceSpot + 1);
        }

        public void Undo()
        {
            AssertBetting();
            if (undoHistory.Count == 0)
                throw new InvalidOperationException("Nothing to undo");
            int latest = undoHistory.Count - 1;
            BettingSnapshot prior = undoHistory[latest];
            int removedSpot = lastTouched;
            List<int> restoredOrder = new List<int>(prior.Order);
            if (prior.PlaysRemovalSound)
            {
                // Source RemoveBet acts on the currently restored list. An NV
                // restart rebuilt that list by ID, so an old snapshot order
                // cannot be used for unrelated surviving spots.
                restoredOrder = new List<int>(betEvaluationOrder);
                restoredOrder.Remove(removedSpot);
                if (prior.Bets.ContainsKey(removedSpot))
                    restoredOrder.Add(removedSpot);
            }
            else if (prior.GroupRemovalOrder != null)
            {
                // Original sector/neighbor removal visits the source spot list
                // in order. Each surviving spot is unhooked and rehooked at
                // the evaluation-list tail, including repeated sector IDs.
                restoredOrder = new List<int>(betEvaluationOrder);
                foreach (int id in prior.GroupRemovalOrder)
                {
                    restoredOrder.Remove(id);
                    if (prior.Bets.ContainsKey(id)) restoredOrder.Add(id);
                }
            }
            if (restoredOrder.Count != prior.Bets.Count ||
                restoredOrder.Distinct().Count() != restoredOrder.Count ||
                restoredOrder.Any(id => !prior.Bets.ContainsKey(id)))
                throw new InvalidOperationException("Invalid undo evaluation order");
            undoHistory.RemoveAt(latest);
            LastUndoRemovalSoundCount = prior.PlaysRemovalSound ? 1 : 0;
            LastUndoRequestsBetState = prior.GroupRemovalOrder != null;
            Bets.Clear();
            foreach (var pair in prior.Bets) Bets.Add(pair.Key, pair.Value);
            BetChipStacks = CopyStacks(prior.Stacks);
            betEvaluationOrder = restoredOrder;
            lastTouched = prior.LastTouched;
        }

        public void Clear()
        {
            AssertBetting();
            undoHistory.Clear();
            LastUndoRemovalSoundCount = 0;
            Bets.Clear();
            BetChipStacks.Clear();
            betEvaluationOrder.Clear();
            lastTouched = -1;
        }

        public void Repeat()
        {
            AssertBetting();
            if (previous.Count == 0) return;
            long total = previous.Values.Sum();
            if (total > Credit) throw new InvalidOperationException("Insufficient credit");
            PushUndo(Bets, BetChipStacks, lastTouched, false);
            Bets.Clear();
            foreach (var pair in previous) Bets.Add(pair.Key, pair.Value);
            BetChipStacks = CopyStacks(previousStacks);
            betEvaluationOrder = new List<int>(previousEvaluationOrder);
        }

        public int BeginSpin()
        {
            AssertBetting();
            if (Bets.Count == 0) throw new InvalidOperationException("Place a bet first");
            if (TotalBet > Credit) throw new InvalidOperationException("Insufficient credit for current bets");
            long wager = TotalBet;
            // Source StateStartGame reports and commits the wager before
            // StateDrawStops obtains a result. This is the local PC ledger.
            Credit = checked(Credit - wager);
            try
            {
                int pocket = outcome.DrawPocket();
                if (pocket < 0 || pocket > 36)
                    throw new InvalidOperationException("Outcome source failed");
                PendingPocket = pocket;
                SpinPending = true;
                DoubleAvailable = false;
                return pocket;
            }
            catch
            {
                Credit = checked(Credit + wager);
                throw;
            }
        }

        public RoundResult EvaluatePendingSpin()
        {
            if (!SpinPending) throw new InvalidOperationException("No spin is pending");
            return EvaluateBaseSpin(Profile, PendingPocket, Bets, betEvaluationOrder, baseMaximumWin);
        }

        // Pure calculation used by pending play and independent archive validation.
        internal static RoundResult EvaluateBaseSpin(RouletteProfile profile, int pocket,
            IDictionary<int,long> bets, IList<int> evaluationOrder, long maximumWin)
        {
            if (profile == null || bets == null || evaluationOrder == null ||
                pocket < 0 || pocket > 36 || maximumWin < 0 || bets.Count < 1 ||
                bets.Any(pair => pair.Value < 1 || !profile.Records.ContainsKey(pair.Key)))
                throw new InvalidOperationException("Invalid base evaluation inputs");
            if (evaluationOrder.Count != bets.Count ||
                evaluationOrder.Distinct().Count() != bets.Count ||
                evaluationOrder.Any(id => !bets.ContainsKey(id)))
                throw new InvalidOperationException("Roulette bet evaluation order is invalid");
            
            long wager = bets.Values.Sum();
            long payout = 0;
            bool winLimitReached = false;
            var winningPays = new List<WinningPayRecord>();
            foreach (int id in evaluationOrder)
                if (profile.Records[id].Covers(pocket))
                {
                    long amount = checked(bets[id] * profile.Records[id].ReturnMultiple);
                    payout = AddBasePay(payout, amount, maximumWin, ref winLimitReached);
                    winningPays.Add(new WinningPayRecord {
                        BetId = id, Amount = amount,
                        WinLevel = profile.Records[id].WinLevel });
                    SortSourceWinningPays(winningPays);
                }
            // Ordinary StateEvaluate calls PayOnRouletteBet with mode 2;
            // that calls RouletteResult::SortPays after each added pay.
            // The source compares signed 64-bit amounts and swaps each
            // larger later record with the current record. A third larger
            // pay can therefore reverse the order of two earlier equal pays.
            var result = new RoundResult { Pocket = pocket, Wager = wager,
                Returned = payout, Category = profile.WagerCategory(bets),
                WinLimitReached = winLimitReached, WinningPays = winningPays };
            return result;
        }

        internal static void SortSourceWinningPays(List<WinningPayRecord> pays)
        {
            // RouletteResult::SortPays, 0x0824de10. Ordinary AddPay creates
            // type-zero records; the source type-two skip is a feature path.
            for (int i = 0; i < pays.Count - 1; ++i)
                for (int j = i + 1; j < pays.Count; ++j)
                    if (pays[i].Amount < pays[j].Amount)
                    {
                        WinningPayRecord swap = pays[i];
                        pays[i] = pays[j];
                        pays[j] = swap;
                    }
        }

        public RoulettePendingSettlementCandidate BuildPendingSettlementCandidate(
            RouletteDeluxe.SasDraftV1.SasDecision sasDecision = null)
        {
            RoundResult basis = EvaluatePendingSpin();
            long award = sasDecision == null ? basis.Returned : sasDecision.PostMultiplierAward;
            return new RoulettePendingSettlementCandidate(this, basis, sasDecision, CanOfferDouble(award));
        }

        // Coordinator entry after durable V4 validation. Installs the exact
        // candidate; it does not draw, capture configuration, or save an archive.
        internal RoundResult InstallSasSettlementCandidate(RoulettePendingSettlementCandidate candidate)
        {
            if(candidate==null||candidate.Sas==null||!candidate.Deferred)
                throw new InvalidOperationException("SAS installation requires deferred candidate");
            candidate.RequireCurrentPendingBaseline(this);
            var result=candidate.CopyResult();
            var nextPrevious=candidate.CopyBets();
            var nextPreviousStacks=candidate.CopyChipStacks();
            var nextPreviousOrder=new List<int>(candidate.CopyEvaluationOrder());
            var nextHistory=new List<RoundResult>(History);nextHistory.Insert(0,result);
            if(nextHistory.Count>50)nextHistory.RemoveAt(50);
            Credit=candidate.CreditAtPublication;Wagered=candidate.WageredAfter;Returned=candidate.ReturnedAfter;
            Rounds=candidate.RoundsAfter;Wins=candidate.WinsAfter;
            previous=nextPrevious;previousStacks=nextPreviousStacks;previousEvaluationOrder=nextPreviousOrder;
            LastResult=result;History=nextHistory;ResultPhase=true;DoubleStake=result.Returned;
            DoubleAvailable=candidate.DoubleAvailable;DoubleRounds=0;LastDoubleCard=-1;
            RoundPayoutPending=true;SpinPending=false;PendingPocket=-1;return result;
        }

        public RoundResult CompleteSpin()
        {
            RoundResult result = EvaluatePendingSpin();
            long wager = result.Wager;
            long payout = result.Returned;
            // Validate every monetary/statistical intermediate before publication,
            // including the eventual credit addition for deferred settlement.
            long payoutCredit = checked(Credit + payout);
            long nextWagered = checked(Wagered + wager);
            long nextReturned = checked(Returned + payout);
            long nextRounds = checked(Rounds + 1);
            int nextWins = checked(Wins + (payout > 0 ? 1 : 0));
            var nextPrevious = new Dictionary<int, long>(Bets);
            var nextPreviousStacks = CopyStacks(BetChipStacks);
            var nextPreviousOrder = new List<int>(betEvaluationOrder);
            bool nextDoubleAvailable = CanOfferDouble(payout);
            if (!BaseSettlementDeferred) Credit = payoutCredit;
            Wagered = nextWagered;
            Returned = nextReturned;
            Rounds = nextRounds;
            Wins = nextWins;
            previous = nextPrevious;
            previousStacks = nextPreviousStacks;
            previousEvaluationOrder = nextPreviousOrder;
            LastResult = result;
            History.Insert(0, result);
            if (History.Count > 50) History.RemoveAt(50);
            ResultPhase = true;
            DoubleStake = payout;
            DoubleAvailable = nextDoubleAvailable;
            DoubleRounds = 0;
            LastDoubleCard = -1;
            RoundPayoutPending = BaseSettlementDeferred;
            SpinPending = false;
            PendingPocket = -1;
            return result;
        }

        public void CancelSpin()
        {
            if (!SpinPending) return;
            Credit = checked(Credit + TotalBet);
            SpinPending = false;
            PendingPocket = -1;
        }

        // Resume a committed PC wager with its already drawn pocket. The
        // outcome source must not be called again for this round.
        public void RestorePendingSpin(int pocket, long creditBefore,
            long denomination, long selectedChip,
            Dictionary<int, List<long>> chipStacks, int[] savedOrder = null,
            long moneyRemainderCents = 0)
        {
            if (SpinPending || InDoubleUp || pocket < 0 || pocket > 36 ||
                creditBefore != Credit || creditBefore < 1 ||
                denomination < 1 || denomination > int.MaxValue ||
                !Profile.Denominations.Contains((int)denomination) ||
                moneyRemainderCents < 0 || moneyRemainderCents >= denomination ||
                !IsSelectableChip(selectedChip) || chipStacks == null ||
                chipStacks.Count < 1 || chipStacks.Count > Profile.Records.Count)
                throw new InvalidOperationException("Invalid pending roulette spin");
            var bets = new Dictionary<int, long>();
            long wager = 0;
            foreach (var pair in chipStacks)
            {
                WinRecord record;
                if (!Profile.Records.TryGetValue(pair.Key, out record) ||
                    pair.Value == null || pair.Value.Count == 0)
                    throw new InvalidOperationException("Invalid pending roulette bet");
                long amount = 0;
                foreach (long chip in pair.Value)
                {
                    if (chip < 1) throw new InvalidOperationException("Invalid pending chip");
                    amount = checked(amount + chip);
                }
                if (amount < record.MinimumBet || amount > Profile.MaximumSpotBet ||
                    amount % record.BetMultiple != 0)
                    throw new InvalidOperationException("Pending bet exceeds limits");
                bets.Add(pair.Key, amount);
                wager = checked(wager + amount);
            }
            if (wager > creditBefore || wager > Profile.MaximumTotalBet)
                throw new InvalidOperationException("Pending wager exceeds credit or limit");
            // RestoreFromNV rebuilds the table through RestoreTableBets:
            // it scans the 161 stored spot slots in ascending ID order and
            // SetBet appends each nonzero spot to the evaluation list.
            RestoreBetOrder(savedOrder, bets);
            List<int> restoredOrder = bets.Keys.OrderBy(id => id).ToList();
            if (ResultPhase) NextRound();
            Bets = bets;
            BetChipStacks = CopyStacks(chipStacks);
            betEvaluationOrder = restoredOrder;
            Credit = creditBefore - wager;
            Denomination = denomination;
            MoneyRemainderCents = moneyRemainderCents;
            Chip = selectedChip;
            PendingPocket = pocket;
            SpinPending = true;
            DoubleAvailable = false;
        }

        // Restore an uncommitted table. Source SaveTableBets writes the table
        // after accepted edits; the wager is not debited until StartGame.
        public void RestoreStagedBets(long credit, long denomination,
            long selectedChip, Dictionary<int, List<long>> chipStacks,
            int touched, SavedUndoState[] savedUndo, int[] savedOrder = null,
            bool validatedExternalCredit = false, long moneyRemainderCents = 0)
        {
            if (SpinPending || InDoubleUp || credit < 0 ||
                (credit != Credit && !validatedExternalCredit) ||
                denomination < 1 || denomination > int.MaxValue ||
                !Profile.Denominations.Contains((int)denomination) ||
                moneyRemainderCents < 0 || moneyRemainderCents >= denomination ||
                !IsSelectableChip(selectedChip) || chipStacks == null ||
                savedUndo == null || savedUndo.Length > 10)
                throw new InvalidOperationException("Invalid staged roulette table");
            // A retained table can exceed the remaining balance after a
            // losing round or platform credit change. This restores no debit;
            // new bets and BeginSpin enforce affordability at their boundary.
            Dictionary<int, long> bets = ValidateStagedStacks(chipStacks, long.MaxValue);
            // The PC journal retains edit order for validation, but the
            // cabinet's NV table restore reconstructs the active list by ID.
            RestoreBetOrder(savedOrder, bets);
            List<int> restoredOrder = bets.Keys.OrderBy(id => id).ToList();
            List<BettingSnapshot> restoredUndo = ValidateTableUndoState(
                touched, savedUndo, bets, long.MaxValue);
            if (ResultPhase) NextRound();
            Bets = bets;
            BetChipStacks = CopyStacks(chipStacks);
            betEvaluationOrder = restoredOrder;
            Denomination = denomination;
            MoneyRemainderCents = moneyRemainderCents;
            Chip = selectedChip;
            undoHistory.Clear();
            undoHistory.AddRange(restoredUndo);
            lastTouched = touched;
        }

        // RouletteData persists the Undo queue across a game and feature.
        // Restore only that table metadata after the independent settlement
        // or pending-wager recovery. Historical bets can exceed today's
        // remaining credit, so validate their table limits without imposing
        // the current balance. Validate everything before changing the queue.
        public void RestoreTableUndoState(int touched, SavedUndoState[] savedUndo)
        {
            List<BettingSnapshot> restoredUndo = ValidateTableUndoState(
                touched, savedUndo, Bets, long.MaxValue);
            undoHistory.Clear();
            undoHistory.AddRange(restoredUndo);
            lastTouched = touched;
            LastUndoRemovalSoundCount = 0;
        }

        private List<BettingSnapshot> ValidateTableUndoState(int touched,
            SavedUndoState[] savedUndo, Dictionary<int, long> currentBets,
            long creditBound)
        {
            if (savedUndo == null || savedUndo.Length > 10 ||
                (touched != -1 && !currentBets.ContainsKey(touched)))
                throw new InvalidOperationException("Invalid saved table Undo state");
            var restoredUndo = new List<BettingSnapshot>(savedUndo.Length);
            foreach (SavedUndoState saved in savedUndo)
            {
                if (saved == null || saved.Stacks == null)
                    throw new InvalidOperationException("Invalid staged undo entry");
                Dictionary<int, long> prior = ValidateStagedStacks(saved.Stacks, creditBound);
                if (saved.LastTouched != -1 && !prior.ContainsKey(saved.LastTouched))
                    throw new InvalidOperationException("Invalid staged undo spot");
                if (saved.SourceSpotPlusOne < 0 ||
                    saved.SourceSpotPlusOne > Profile.Records.Count)
                    throw new InvalidOperationException("Invalid staged undo source spot");
                if (saved.GroupRemovalOrder != null &&
                    saved.GroupRemovalOrder.Length > 0 &&
                    (saved.GroupRemovalOrder.Length > 9 ||
                     saved.GroupRemovalOrder.Any(id => !Profile.Records.ContainsKey(id))))
                    throw new InvalidOperationException("Invalid staged group undo order");
                restoredUndo.Add(new BettingSnapshot {
                    Bets = prior, Stacks = CopyStacks(saved.Stacks),
                    Order = RestoreBetOrder(saved.Order, prior),
                    GroupRemovalOrder = saved.GroupRemovalOrder == null ||
                        saved.GroupRemovalOrder.Length == 0 ? null :
                        (int[])saved.GroupRemovalOrder.Clone(),
                    LastTouched = saved.LastTouched,
                    SourceSpotPlusOne = saved.SourceSpotPlusOne,
                    PlaysRemovalSound = saved.PlaysRemovalSound
                });
            }
            return restoredUndo;
        }

        private Dictionary<int, long> ValidateStagedStacks(
            Dictionary<int, List<long>> chipStacks, long credit)
        {
            if (chipStacks == null || chipStacks.Count > Profile.Records.Count)
                throw new InvalidOperationException("Invalid staged roulette spots");
            var bets = new Dictionary<int, long>();
            long wager = 0;
            foreach (var pair in chipStacks)
            {
                WinRecord record;
                if (!Profile.Records.TryGetValue(pair.Key, out record) ||
                    pair.Value == null || pair.Value.Count == 0)
                    throw new InvalidOperationException("Invalid staged roulette bet");
                long amount = 0;
                foreach (long chip in pair.Value)
                {
                    if (chip < 1) throw new InvalidOperationException("Invalid staged chip");
                    amount = checked(amount + chip);
                }
                if (amount < record.MinimumBet || amount > Profile.MaximumSpotBet ||
                    amount % record.BetMultiple != 0)
                    throw new InvalidOperationException("Staged bet exceeds limits");
                bets.Add(pair.Key, amount);
                wager = checked(wager + amount);
            }
            if (wager > credit || wager > Profile.MaximumTotalBet)
                throw new InvalidOperationException("Staged wager exceeds credit or limit");
            return bets;
        }

        public RoundResult Spin()
        {
            BeginSpin();
            return CompleteSpin();
        }

        public void NextRound()
        {
            if (SpinPending) throw new InvalidOperationException("Wheel is spinning");
            if (InDoubleUp) throw new InvalidOperationException("Collect double win first");
            if (RoundPayoutPending) throw new InvalidOperationException("Complete the current round payout first");
            ResultPhase = false;
            DoubleAvailable = false;
            DoubleStake = 0;
        }

        // Roulette::StatePlay decides whether to enter the feature before
        // StateDisplayResults purges input. Skipping it is final for this
        // result even if that purge subsequently clears configured AutoPlay.
        public void DeclineDoubleOffer()
        {
            if (!ResultPhase || SpinPending || InDoubleUp)
                throw new InvalidOperationException("No idle Double Up offer to decline");
            DoubleAvailable = false;
            DoubleStake = 0;
        }

        public void EnableDeferredBaseSettlement()
        {
            if (SpinPending || ResultPhase || InDoubleUp || RoundPayoutPending)
                throw new InvalidOperationException("Base settlement mode must precede the round or restore");
            BaseSettlementDeferred = true;
        }

        // This applies model money once. Request replies, credit notification,
        // PayComplete, durable history and End remain separate Game concerns.
        public void CompleteBaseGamePayout(long expectedAward)
        {
            if (!BaseSettlementDeferred || !RoundPayoutPending || !ResultPhase ||
                SpinPending || InDoubleUp || DoubleAvailable || LastResult == null ||
                expectedAward < 0 || expectedAward != LastResult.Returned)
                throw new InvalidOperationException("Base Game payout differs from its pending result");
            long finalCredit = checked(Credit + expectedAward);
            checked { long totalCents = finalCredit * Denomination + MoneyRemainderCents; }
            Credit = finalCredit;
            RoundPayoutPending = false;
        }

        // Explicit unpaid accounting basis for new PC archives. The supplied
        // wallet is already after the wager; neither base nor feature entry
        // subtracts the evaluated award from it. No outcome source is called.
        public void RestoreUnpaidRound(RoundResult baseResult, long creditBeforePayout,
            long denomination, long selectedChip,
            Dictionary<int, List<long>> chipStacks, bool offerAvailable, bool featureStarted,
            int featureRounds, long featureStake, int lastDealer,
            int[] savedOrder = null, long moneyRemainderCents = 0)
        {
            if (!BaseSettlementDeferred || LastResult != null || SpinPending || ResultPhase ||
                InDoubleUp || RoundPayoutPending || Bets.Count != 0 || Rounds != 0 ||
                baseResult == null || baseResult.Returned < 0 || creditBeforePayout < 0 ||
                denomination < 1 || denomination > int.MaxValue ||
                !Profile.Denominations.Contains((int)denomination) ||
                moneyRemainderCents < 0 || moneyRemainderCents >= denomination ||
                !IsSelectableChip(selectedChip) || featureRounds < 0 ||
                featureRounds > doubleMaximumRounds || featureStake < 0)
                throw new InvalidOperationException("Invalid unpaid roulette round");
            if (featureStarted)
            {
                if (offerAvailable || !DoubleSettlementDeferred || !CanOfferDouble(baseResult.Returned) ||
                    featureStake > doubleMaximumWin ||
                    (featureRounds == 0 ? featureStake != baseResult.Returned || lastDealer != -1 :
                        lastDealer < 0 || lastDealer > 3))
                    throw new InvalidOperationException("Invalid unpaid Double state");
            }
            else if (featureRounds != 0 || lastDealer != -1 ||
                (offerAvailable ? !CanOfferDouble(baseResult.Returned) ||
                    featureStake != baseResult.Returned :
                    featureStake != 0 && featureStake != baseResult.Returned))
                throw new InvalidOperationException("Invalid unpaid result offer");
            checked { long totalCents = creditBeforePayout * denomination + moneyRemainderCents; }

            // Validate a copy first: an invalid order or bet must not mutate
            // either the caller's result or the model before rejection.
            var validatedResult = new RoundResult { Pocket = baseResult.Pocket,
                Wager = baseResult.Wager, Returned = baseResult.Returned,
                Category = baseResult.Category, WinLimitReached = baseResult.WinLimitReached };
            Dictionary<int, long> restoredBets = ValidateSavedRoundBets(validatedResult, chipStacks);
            List<int> restoredOrder = RestoreBetOrder(savedOrder, restoredBets);
            List<WinningPayRecord> restoredPays = RebuildWinningPays(restoredBets,
                restoredOrder, validatedResult.Pocket);
            Dictionary<int, List<long>> restoredStacks = CopyStacks(chipStacks);
            var restoredPrevious = new Dictionary<int, long>(restoredBets);
            Dictionary<int, List<long>> restoredPreviousStacks = CopyStacks(chipStacks);
            var restoredPreviousOrder = new List<int>(restoredOrder);

            baseResult.WinLimitReached = validatedResult.WinLimitReached;
            baseResult.WinningPays = restoredPays;
            Bets = restoredBets;
            BetChipStacks = restoredStacks;
            betEvaluationOrder = restoredOrder;
            previous = restoredPrevious;
            previousStacks = restoredPreviousStacks;
            previousEvaluationOrder = restoredPreviousOrder;
            Credit = creditBeforePayout;
            Denomination = denomination;
            MoneyRemainderCents = moneyRemainderCents;
            Chip = selectedChip;
            Wagered = baseResult.Wager;
            Returned = baseResult.Returned;
            Rounds = 1;
            Wins = baseResult.Returned > 0 ? 1 : 0;
            LastResult = baseResult;
            History.Insert(0, baseResult);
            ResultPhase = true;
            DoubleStake = featureStake;
            DoubleRounds = featureRounds;
            LastDoubleCard = lastDealer;
            DoubleAvailable = offerAvailable;
            InDoubleUp = featureStarted;
            DoubleChoiceTerminal = featureStarted && featureRounds > 0 &&
                (featureStake == 0 || featureRounds == doubleMaximumRounds ||
                    featureStake > doubleMaximumWin / (doubleSuitMode ? 4 : 2));
            RoundPayoutPending = true;
        }

        // Rebuild an already settled round from the PC's validated archive.
        // The platform/SBox implementation owns its own NV and credit state.
        public void RestorePendingRound(RoundResult result, long creditAfterBase,
            long denomination, long selectedChip,
            Dictionary<int, List<long>> chipStacks, bool featureStarted,
            int featureRounds, long featureStake, int lastDealer,
            int[] savedOrder = null, long moneyRemainderCents = 0)
        {
            if (LastResult != null || SpinPending || ResultPhase || InDoubleUp ||
                Bets.Count != 0 || Rounds != 0 || result == null ||
                result.Pocket < 0 || result.Pocket > 36 || result.Wager < 1 ||
                result.Returned <= 0 || creditAfterBase < result.Returned ||
                chipStacks == null || chipStacks.Count < 1 || chipStacks.Count > Profile.Records.Count ||
                denomination < 1 || denomination > int.MaxValue ||
                !Profile.Denominations.Contains((int)denomination) ||
                moneyRemainderCents < 0 || moneyRemainderCents >= denomination ||
                !IsSelectableChip(selectedChip) || !CanOfferDouble(result.Returned) ||
                featureRounds < 0 || featureRounds > doubleMaximumRounds ||
                featureStake < 0 || (featureStake == 0 && (!DoubleSettlementDeferred || featureRounds == 0)) ||
                (featureStarted && featureRounds >= doubleMaximumRounds && !DoubleSettlementDeferred) ||
                (!featureStarted && (featureRounds != 0 ||
                    featureStake != result.Returned || lastDealer != -1)) ||
                (featureRounds == 0 ? lastDealer != -1 : lastDealer < 0 || lastDealer > 3))
                throw new InvalidOperationException("Invalid saved roulette round");
            Dictionary<int, long> restoredBets = ValidateSavedRoundBets(result, chipStacks);
            List<int> restoredOrder = RestoreBetOrder(savedOrder, restoredBets);
            result.WinningPays = RebuildWinningPays(restoredBets,
                restoredOrder, result.Pocket);
            Bets = restoredBets;
            BetChipStacks = CopyStacks(chipStacks);
            betEvaluationOrder = restoredOrder;
            previous = new Dictionary<int, long>(restoredBets);
            previousStacks = CopyStacks(chipStacks);
            previousEvaluationOrder = new List<int>(betEvaluationOrder);
            Credit = featureStarted ? creditAfterBase - result.Returned : creditAfterBase;
            Denomination = denomination;
            MoneyRemainderCents = moneyRemainderCents;
            Chip = selectedChip;
            Wagered = result.Wager;
            Returned = result.Returned;
            Rounds = 1;
            Wins = 1;
            LastResult = result;
            History.Insert(0, result);
            ResultPhase = true;
            DoubleStake = featureStake;
            DoubleRounds = featureRounds;
            LastDoubleCard = lastDealer;
            DoubleAvailable = !featureStarted;
            InDoubleUp = featureStarted;
            RoundPayoutPending = false;
            DoubleChoiceTerminal = DoubleSettlementDeferred && featureStarted &&
                featureRounds > 0 && (featureStake == 0 || featureRounds == doubleMaximumRounds ||
                    featureStake > doubleMaximumWin / (doubleSuitMode ? 4 : 2));
        }

        internal void RequireSameSasIdleSnapshot(RouletteSession expected,int[] savedOrder)
        {
            // Warm edited tables retain their saved active order; native NV
            // restoration sorts by ID. Both compare with the same admitted
            // table and full immutable accounting. Never reorder the live model.
            if(expected!=null&&savedOrder!=null&&betEvaluationOrder.SequenceEqual(savedOrder))
                expected.betEvaluationOrder=RestoreBetOrder(savedOrder,expected.Bets);
            RequireSameSasBaseSnapshot(expected);
            if(lastTouched!=expected.lastTouched||undoHistory.Count!=expected.undoHistory.Count)
                throw new InvalidOperationException("Live SAS Idle Undo queue differs from durable owner");
            for(int i=0;i<undoHistory.Count;i++)
            {
                var actual=undoHistory[i];var saved=expected.undoHistory[i];
                if(actual.LastTouched!=saved.LastTouched||actual.SourceSpotPlusOne!=saved.SourceSpotPlusOne||
                    actual.PlaysRemovalSound!=saved.PlaysRemovalSound||!actual.Order.SequenceEqual(saved.Order)||
                    !(actual.GroupRemovalOrder??new int[0]).SequenceEqual(saved.GroupRemovalOrder??new int[0])||
                    actual.Stacks.Count!=saved.Stacks.Count||saved.Stacks.Any(p=>!actual.Stacks.ContainsKey(p.Key)||!actual.Stacks[p.Key].SequenceEqual(p.Value)))
                    throw new InvalidOperationException("Live SAS Idle Undo entry differs from durable owner");
            }
        }
        internal void RequireSameSasBaseSnapshot(RouletteSession expected)
        {
            if(expected==null||!RoulettePendingSettlementCandidate.SameProfile(Profile,expected.Profile)||
                Profile.PaytableResourceSha256!=expected.Profile.PaytableResourceSha256||Profile.GameConfigResourceSha256!=expected.Profile.GameConfigResourceSha256||
                !ReferenceEquals(outcome,expected.outcome)||Credit!=expected.Credit||Wagered!=expected.Wagered||Returned!=expected.Returned||Rounds!=expected.Rounds||Wins!=expected.Wins||
                Denomination!=expected.Denomination||MoneyRemainderCents!=expected.MoneyRemainderCents||Chip!=expected.Chip||
                SpinPending!=expected.SpinPending||PendingPocket!=expected.PendingPocket||ResultPhase!=expected.ResultPhase||InDoubleUp!=expected.InDoubleUp||
                RoundPayoutPending!=expected.RoundPayoutPending||BaseSettlementDeferred!=expected.BaseSettlementDeferred||DoubleEnabled!=expected.DoubleEnabled||
                DoubleAvailable!=expected.DoubleAvailable||DoubleStake!=expected.DoubleStake||DoubleRounds!=expected.DoubleRounds||LastDoubleCard!=expected.LastDoubleCard||
                baseMaximumWin!=expected.baseMaximumWin||doubleMaximumWin!=expected.doubleMaximumWin||doubleSuitMode!=expected.doubleSuitMode||doubleMaximumRounds!=expected.doubleMaximumRounds||
                DoubleSettlementDeferred!=expected.DoubleSettlementDeferred||DoubleChoiceTerminal!=expected.DoubleChoiceTerminal||
                !RoulettePendingSettlementCandidate.SameResult(LastResult,expected.LastResult)||History.Count!=expected.History.Count||
                !History.Zip(expected.History,RoulettePendingSettlementCandidate.SameResult).All(equal=>equal)||
                Bets.Count!=expected.Bets.Count||BetChipStacks.Count!=expected.BetChipStacks.Count||!betEvaluationOrder.SequenceEqual(expected.betEvaluationOrder))
                throw new InvalidOperationException("Live SAS Base snapshot differs from durable owner");
            foreach(var pair in expected.Bets){long value;if(!Bets.TryGetValue(pair.Key,out value)||value!=pair.Value)throw new InvalidOperationException("Live SAS Base bet differs");}
            foreach(var pair in expected.BetChipStacks){List<long> value;if(!BetChipStacks.TryGetValue(pair.Key,out value)||value==null||!value.SequenceEqual(pair.Value))throw new InvalidOperationException("Live SAS Base chips differ");}
        }

        // Coordinator supplies resource-verified SAS decision and independently
        // admitted prior history. No draw, payout callback or wallet addition.
        internal void RestoreSasBaseSnapshot(RoundResult basis, RouletteDeluxe.SasDraftV1.SasDecision decision,
            long creditBefore, bool moneyApplied, long denomination, long remainder, long selectedChip,
            Dictionary<int,List<long>> stacks, int[] order, long priorWagered, long priorReturned,
            long priorRounds, int priorWins, RoundResult[] priorHistory, bool offerAvailable)
        {
            if(LastResult!=null||SpinPending||ResultPhase||InDoubleUp||Bets.Count!=0||Rounds!=0||Credit!=creditBefore||
                basis==null||decision==null||decision.Qualification.EvaluatedAward!=basis.Returned||decision.Qualification.Wager!=basis.Wager||
                creditBefore<0||priorWagered<0||priorReturned<0||priorRounds<0||priorWins<0||priorWins>priorRounds||
                denomination<1||denomination>int.MaxValue||!Profile.Denominations.Contains((int)denomination)||remainder<0||remainder>=denomination||
                !IsSelectableChip(selectedChip)||priorHistory==null||priorHistory.Length>50||priorHistory.Take(49).Any(r=>r==null)||
                (offerAvailable&&!CanOfferDouble(decision.PostMultiplierAward)))
                throw new InvalidOperationException("Invalid SAS Base restore baseline");
            var restoredBets=ValidateSavedRoundBets(basis,stacks);var restoredOrder=RestoreBetOrder(order,restoredBets);
            long award=decision.PostMultiplierAward;long wallet=checked(creditBefore+(moneyApplied?award:0));
            long nextWagered=checked(priorWagered+basis.Wager),nextReturned=checked(priorReturned+award),nextRounds=checked(priorRounds+1);
            int nextWins=checked(priorWins+(basis.Returned>0?1:0));checked{long money=wallet*denomination+remainder;}
            var result=CopySasRestoreResult(basis);result.Returned=award;
            var restoredStacks=CopyStacks(stacks);var previousBets=new Dictionary<int,long>(restoredBets);var previousChips=CopyStacks(stacks);var previousOrder=new List<int>(restoredOrder);
            var history=priorHistory.Take(49).Select(CopySasRestoreResult).ToList();history.Insert(0,result);
            Bets=restoredBets;BetChipStacks=restoredStacks;betEvaluationOrder=restoredOrder;previous=previousBets;previousStacks=previousChips;previousEvaluationOrder=previousOrder;
            Credit=wallet;Denomination=denomination;MoneyRemainderCents=remainder;Chip=selectedChip;Wagered=nextWagered;Returned=nextReturned;Rounds=nextRounds;Wins=nextWins;
            LastResult=result;History=history;ResultPhase=true;DoubleAvailable=offerAvailable;DoubleStake=award;DoubleRounds=0;LastDoubleCard=-1;
            InDoubleUp=false;RoundPayoutPending=!moneyApplied;SpinPending=false;PendingPocket=-1;BaseSettlementDeferred=true;
        }
        private static RoundResult CopySasRestoreResult(RoundResult r)
        {
            return new RoundResult{Pocket=r.Pocket,Wager=r.Wager,Returned=r.Returned,Category=r.Category,WinLimitReached=r.WinLimitReached,
                WinningPays=r.WinningPays.Select(p=>new WinningPayRecord{BetId=p.BetId,Amount=p.Amount,WinLevel=p.WinLevel}).ToList(),
                HistoryPayOverrides=r.HistoryPayOverrides.Select(p=>new HistoryPayOverride{PayIndex=p.PayIndex,Amount=p.Amount,ProgressiveLevel=p.ProgressiveLevel,Kind=p.Kind}).ToList()};
        }

        public void RestoreCompletedRound(RoundResult finalResult, long basePayout,
            long creditAfter, long denomination, long selectedChip,
            Dictionary<int, List<long>> chipStacks, bool featureCompleted,
            int[] savedOrder = null, long moneyRemainderCents = 0)
        {
            if (LastResult != null || SpinPending || ResultPhase || InDoubleUp ||
                Bets.Count != 0 || Rounds != 0 || finalResult == null ||
                finalResult.Returned < 0 || basePayout < 0 || creditAfter < 0 ||
                (featureCompleted ? basePayout <= 0 : finalResult.Returned != basePayout) ||
                denomination < 1 || denomination > int.MaxValue ||
                !Profile.Denominations.Contains((int)denomination) ||
                moneyRemainderCents < 0 || moneyRemainderCents >= denomination ||
                !IsSelectableChip(selectedChip))
                throw new InvalidOperationException("Invalid completed roulette round");
            var baseResult = new RoundResult { Pocket = finalResult.Pocket,
                Wager = finalResult.Wager, Returned = basePayout,
                Category = finalResult.Category,
                WinLimitReached = finalResult.WinLimitReached };
            Dictionary<int, long> restoredBets = ValidateSavedRoundBets(baseResult, chipStacks);
            List<int> restoredOrder = RestoreBetOrder(savedOrder, restoredBets);
            finalResult.WinLimitReached = baseResult.WinLimitReached;
            finalResult.WinningPays = RebuildWinningPays(restoredBets,
                restoredOrder, finalResult.Pocket);
            Bets = restoredBets;
            BetChipStacks = CopyStacks(chipStacks);
            betEvaluationOrder = restoredOrder;
            previous = new Dictionary<int, long>(restoredBets);
            previousStacks = CopyStacks(chipStacks);
            previousEvaluationOrder = new List<int>(betEvaluationOrder);
            Credit = creditAfter;
            Denomination = denomination;
            MoneyRemainderCents = moneyRemainderCents;
            Chip = selectedChip;
            Wagered = finalResult.Wager;
            Returned = finalResult.Returned;
            Rounds = 1;
            // This counter tracks the base roulette hit. A losing Double Up
            // changes its paid amount, but does not undo the earlier hit.
            Wins = basePayout > 0 ? 1 : 0;
            LastResult = finalResult;
            History.Insert(0, finalResult);
            ResultPhase = true;
            DoubleAvailable = false;
            DoubleStake = 0;
            DoubleRounds = 0;
            LastDoubleCard = -1;
            InDoubleUp = false;
            RoundPayoutPending = false;
        }

        // The most recent round restores the playable state; these archived
        // totals preserve the preceding PC session across a process restart.
        public void RestoreCounters(long wagered, long returned, long rounds,
            int wins)
        {
            if (LastResult == null || SpinPending || wagered < Wagered ||
                returned < Returned || rounds < Rounds || wins < Wins ||
                wins > rounds)
                throw new InvalidOperationException("Invalid saved roulette totals");
            Wagered = wagered;
            Returned = returned;
            Rounds = rounds;
            Wins = wins;
        }

        private List<WinningPayRecord> RebuildWinningPays(
            Dictionary<int, long> bets, List<int> order, int pocket)
        {
            // The PC archive keeps the saved table and its evaluation order.
            // Re-evaluate source pay metadata after restart. This does not
            // start a visual result cycle; its restart lifetime is separate.
            var pays = new List<WinningPayRecord>();
            foreach (int id in order)
            {
                WinRecord record = Profile.Records[id];
                if (!record.Covers(pocket)) continue;
                pays.Add(new WinningPayRecord {
                    BetId = id, Amount = checked(bets[id] * record.ReturnMultiple),
                    WinLevel = record.WinLevel
                });
                SortSourceWinningPays(pays);
            }
            return pays;
        }

        private Dictionary<int, long> ValidateSavedRoundBets(RoundResult result,
            Dictionary<int, List<long>> chipStacks)
        {
            if (result == null || result.Pocket < 0 || result.Pocket > 36 ||
                result.Wager < 1 || result.Returned < 0 || chipStacks == null ||
                chipStacks.Count < 1 || chipStacks.Count > Profile.Records.Count)
                throw new InvalidOperationException("Invalid saved roulette round");
            long wager = 0;
            long payout = 0;
            bool winLimitReached = false;
            var restoredBets = new Dictionary<int, long>();
            foreach (var pair in chipStacks)
            {
                WinRecord record;
                if (!Profile.Records.TryGetValue(pair.Key, out record) ||
                    pair.Value == null || pair.Value.Count == 0)
                    throw new InvalidOperationException("Invalid saved roulette bet");
                long amount = 0;
                foreach (long chip in pair.Value)
                {
                    if (chip < 1) throw new InvalidOperationException("Invalid saved chip");
                    amount = checked(amount + chip);
                }
                if (amount < record.MinimumBet || amount > Profile.MaximumSpotBet ||
                    amount % record.BetMultiple != 0)
                    throw new InvalidOperationException("Saved bet exceeds limits");
                restoredBets.Add(pair.Key, amount);
                wager = checked(wager + amount);
                if (record.Covers(result.Pocket))
                    payout = AddBasePay(payout,
                        checked(amount * record.ReturnMultiple), ref winLimitReached);
            }
            if (wager != result.Wager || wager > Profile.MaximumTotalBet ||
                payout != result.Returned ||
                result.Category != Profile.WagerCategory(restoredBets))
                throw new InvalidOperationException("Saved roulette payout disagrees with paytable");
            result.WinLimitReached = winLimitReached;
            return restoredBets;
        }

        // StateEvaluate checks the cumulative return after each bet spot.
        // Keep its per-spot limit flag and avoid adding beyond that limit.
        private long AddBasePay(long current, long amount, ref bool limitReached)
        { return AddBasePay(current, amount, baseMaximumWin, ref limitReached); }

        private static long AddBasePay(long current, long amount, long maximumWin, ref bool limitReached)
        {
            if (maximumWin != 0 && amount > maximumWin - current)
            {
                limitReached = true;
                return maximumWin;
            }
            return checked(current + amount);
        }

        private static Dictionary<int, List<long>> CopyStacks(
            Dictionary<int, List<long>> source)
        {
            var copy = new Dictionary<int, List<long>>();
            foreach (var pair in source) copy[pair.Key] = new List<long>(pair.Value);
            return copy;
        }

        public void StartDouble()
        {
            if (!ResultPhase || !DoubleAvailable || !CanOfferDouble(DoubleStake))
                throw new InvalidOperationException("Double Up unavailable");
            if (RoundPayoutPending)
            {
                if (!DoubleSettlementDeferred)
                    throw new InvalidOperationException("Unpaid base awards require deferred Double settlement");
            }
            else
            {
                if (DoubleStake > Credit) throw new InvalidOperationException("Double Up stake exceeds credit");
                Credit -= DoubleStake;
            }
            InDoubleUp = true;
            DoubleAvailable = false;
            LastDoubleCard = -1;
            DoubleChoiceTerminal = false;
        }

        public void EnableDeferredDoubleSettlement()
        {
            if (InDoubleUp || SpinPending)
                throw new InvalidOperationException("Double settlement mode must precede feature entry or restore");
            DoubleSettlementDeferred = true;
        }

        private bool CanOfferDouble(long stake)
        {
            if (!DoubleEnabled || stake <= 0) return false;
            // Game::CanDoubleUpResults shifts the configured cap by one
            // (red/black) or two (suit) and rejects a stake at the boundary.
            // long.MaxValue is this PC build's explicit unbounded fallback.
            if (doubleMaximumWin == long.MaxValue) return true;
            return stake < (doubleMaximumWin >> (doubleSuitMode ? 2 : 1));
        }

        // choice: -1 red, -2 black, 0 hearts, 1 diamonds, 2 clubs, 3 spades.
        public bool GuessDouble(int choice)
        {
            ValidateDoubleChoice(choice);
            return ReplayDoubleChoice(choice, outcome.DrawDoubleSuit());
        }

        private void ValidateDoubleChoice(int choice)
        {
            if (!InDoubleUp || DoubleChoiceTerminal || DoubleRounds >= doubleMaximumRounds)
                throw new InvalidOperationException("Double Up unavailable");
            if (choice < -2 || choice > 3) throw new ArgumentException("Invalid Double Up guess");
            if (choice >= 0 && !doubleSuitMode)
                throw new InvalidOperationException("Suit guesses are unavailable in red/black mode");
        }

        // The PC durable result receipt supplies the committed suit on cold
        // restore. It uses exactly the same settlement as a live draw.
        internal bool ReplayDoubleChoice(int choice, int originalSuit)
        {
            ValidateDoubleChoice(choice);
            if (originalSuit < 0 || originalSuit > 3)
                throw new InvalidOperationException("Double Up suit outcome failed");
            LastDoubleCard = originalSuit;
            bool matched = choice == -1 ? OriginalDoubleSuit.IsRed(originalSuit) :
                choice == -2 ? !OriginalDoubleSuit.IsRed(originalSuit) :
                OriginalDoubleSuit.ViewIndex(originalSuit) == choice;
            if (!matched)
            {
                DoubleRounds++;
                DoubleStake = 0;
                DoubleChoiceTerminal = true;
                if (!DoubleSettlementDeferred)
                {
                    InDoubleUp = false;
                    SetFinalDoublePayout(0);
                }
                return false;
            }
            long nextStake = checked(DoubleStake * (choice >= 0 ? 4 : 2));
            if (nextStake > doubleMaximumWin)
                throw new InvalidOperationException("Double Up choice exceeds maximum award");
            bool maximumAward = DoubleRounds + 1 == doubleMaximumRounds ||
                nextStake > doubleMaximumWin / (doubleSuitMode ? 4 : 2);
            if (maximumAward) checked { long finalCredit = Credit + nextStake; }
            DoubleRounds++;
            DoubleStake = nextStake;
            if (maximumAward)
            {
                DoubleChoiceTerminal = true;
                if (!DoubleSettlementDeferred)
                {
                    Credit = checked(Credit + DoubleStake);
                    InDoubleUp = false;
                    SetFinalDoublePayout(DoubleStake);
                }
            }
            return true;
        }

        public void CollectDouble()
        {
            if (DoubleSettlementDeferred)
                throw new InvalidOperationException("Deferred Double requires native FeatureComplete");
            if (!InDoubleUp) throw new InvalidOperationException("No Double Up win to collect");
            Credit = checked(Credit + DoubleStake);
            SetFinalDoublePayout(DoubleStake);
            DoubleStake = 0;
            InDoubleUp = false;
        }

        public void CompleteDoubleFeature(long expectedAward)
        {
            if (!DoubleSettlementDeferred || !InDoubleUp || expectedAward != DoubleStake)
                throw new InvalidOperationException("Double FeatureComplete award differs from its pending result");
            long finalCredit = checked(Credit + expectedAward);
            long finalReturned = checked(Returned - LastResult.Returned + expectedAward);
            if (RoundPayoutPending)
                checked { long totalCents = finalCredit * Denomination + MoneyRemainderCents; }
            Credit = finalCredit;
            Returned = finalReturned;
            LastResult.Returned = expectedAward;
            InDoubleUp = false;
            DoubleAvailable = false;
            RoundPayoutPending = false;
        }

        private void SetFinalDoublePayout(long amount)
        {
            if (LastResult == null)
                throw new InvalidOperationException("No round result for Double Up");
            Returned = checked(Returned - LastResult.Returned + amount);
            LastResult.Returned = amount;
        }
    }
}
