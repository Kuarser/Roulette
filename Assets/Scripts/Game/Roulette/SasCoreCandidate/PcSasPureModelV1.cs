using System;

// Design draft only. No Unity, persistence, RNG, IPC, UI, or wallet mutation.
namespace RouletteDeluxe.SasDraftV1
{
    public static class SasLittleEndian
    {
        public static long Read64(byte[] bytes, int offset)
        {
            ulong value = 0;
            for (int i = 0; i < 8; ++i) value |= (ulong)bytes[offset + i] << (8 * i);
            return unchecked((long)value);
        }
        public static void Write64(byte[] bytes, int offset, long value)
        {
            ulong bits = unchecked((ulong)value);
            for (int i = 0; i < 8; ++i) bytes[offset + i] = (byte)((bits >> (8 * i)) & 0xff);
        }
        public static uint Read32(byte[] bytes, int offset)
        {
            return unchecked((uint)(bytes[offset] | bytes[offset + 1] << 8 |
                bytes[offset + 2] << 16 | bytes[offset + 3] << 24));
        }
        public static void Write32(byte[] bytes, int offset, uint value)
        {
            for (int i = 0; i < 4; ++i) bytes[offset + i] = (byte)((value >> (8 * i)) & 0xff);
        }
    }

    public sealed class SasPayload
    {
        private readonly byte[] raw;
        public SasPayload(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 32 || bytes[0] > 1)
                throw new ArgumentException("Invalid native SAS payload");
            raw = (byte[])bytes.Clone();
        }
        public static SasPayload Disabled() { return new SasPayload(new byte[32]); }
        public byte[] CopyBytes() { return (byte[])raw.Clone(); }
        public bool PendingEnable { get { return raw[0] != 0; } }
        public long Minimum { get { return SasLittleEndian.Read64(raw, 1); } }
        public long Maximum { get { return SasLittleEndian.Read64(raw, 9); } }
        public byte Multiplier { get { return raw[17]; } }
        public byte Mode { get { return raw[18]; } }
        public bool Available { get { return !PendingEnable && Multiplier > 1; } }
    }

    public sealed class SasHostMessage
    {
        public readonly bool Enabled;
        public readonly long Minimum, Maximum;
        public readonly byte Multiplier, Mode;
        private SasHostMessage(bool enabled, long min, long max, byte multiplier, byte mode)
        { Enabled = enabled; Minimum = min; Maximum = max; Multiplier = multiplier; Mode = mode; }
        public static SasHostMessage Decode(byte[] wire)
        {
            if (wire == null || wire.Length != 32 || SasLittleEndian.Read32(wire, 0) != 0x11b ||
                SasLittleEndian.Read32(wire, 4) != 32 || wire[8] > 1)
                throw new ArgumentException("Invalid host multiplier message");
            return new SasHostMessage(wire[8] != 0, SasLittleEndian.Read64(wire, 12),
                SasLittleEndian.Read64(wire, 20), wire[28], wire[29]);
        }
        public static byte[] Encode(bool enabled, long min, long max, byte multiplier, byte mode)
        {
            byte[] bytes = new byte[32];
            SasLittleEndian.Write32(bytes, 0, 0x11b);
            SasLittleEndian.Write32(bytes, 4, 32);
            if (enabled)
            {
                bytes[8] = 1;
                SasLittleEndian.Write64(bytes, 12, min);
                SasLittleEndian.Write64(bytes, 20, max);
                bytes[28] = multiplier; bytes[29] = mode;
            }
            return bytes;
        }
    }

    public sealed class SasConfigTransition
    {
        public readonly SasPayload Payload;
        public readonly bool RuntimePendingDisable, SaveRequired;
        public SasConfigTransition(SasPayload payload, bool disable, bool save)
        { Payload = payload; RuntimePendingDisable = disable; SaveRequired = save; }
    }

    public static class SasConfigRules
    {
        public static SasConfigTransition Receive(SasPayload prior, bool pendingDisable,
            int nativeMainState, SasHostMessage message)
        {
            if (prior == null || message == null) throw new ArgumentNullException();
            bool idle = nativeMainState == 1;
            if (!message.Enabled && !idle)
                return new SasConfigTransition(prior, true, false);
            byte[] raw = prior.CopyBytes();
            if (message.Enabled)
            {
                if (!idle) raw[0] = 1; // Idle enable does not clear an existing pending bit.
                SasLittleEndian.Write64(raw, 1, message.Minimum);
                SasLittleEndian.Write64(raw, 9, message.Maximum);
                raw[17] = message.Multiplier; raw[18] = message.Mode;
            }
            else
            {
                Array.Clear(raw, 0, 19); // Original stores stop at Game+7f2e.
                pendingDisable = false;
            }
            return new SasConfigTransition(new SasPayload(raw), pendingDisable, true);
        }
        public static SasConfigTransition ActivateAtIdle(SasPayload prior, bool pendingDisable,
            int nativeMainState)
        {
            if (prior == null) throw new ArgumentNullException();
            if (nativeMainState != 1) throw new InvalidOperationException("Activation requires main state 1");
            byte[] raw = prior.CopyBytes(); raw[0] = 0;
            return new SasConfigTransition(new SasPayload(raw), pendingDisable, true);
        }
        public static SasConfigTransition ColdLoad(byte[] saved32)
        {
            return new SasConfigTransition(new SasPayload(saved32), false, false);
        }
    }

    public enum SasObservationPoint { Qualification, Display, Recorded, Arithmetic, Route, Packet }
    public sealed class SasObservation
    {
        public readonly SasObservationPoint Point;
        public readonly long Generation;
        public readonly SasPayload Payload;
        // Historical diagnostic only. This does not restore the runtime disable bit.
        public readonly bool RuntimePendingDisableObserved;
        public SasObservation(SasObservationPoint point, long generation, SasPayload payload, bool disable)
        {
            if (payload == null || generation < 0 || !Enum.IsDefined(typeof(SasObservationPoint), point))
                throw new ArgumentException("Invalid configuration observation");
            Point = point; Generation = generation; Payload = payload;
            RuntimePendingDisableObserved = disable;
        }
        internal static void Require(SasObservation value, SasObservationPoint point)
        {
            if (value == null || value.Point != point) throw new ArgumentException("Wrong observation point");
        }
    }

    public enum SasQualificationFailure { None, Unavailable, Mode, ZeroAward, OutsideRange }
    public sealed class SasQualification
    {
        public readonly SasObservation Observation;
        public readonly long EvaluatedAward, Wager, EffectiveMaximumWager;
        public readonly SasQualificationFailure Failure;
        public bool Eligible { get { return Failure == SasQualificationFailure.None; } }
        public SasQualification(SasObservation observation, long award, long wager, long maxWager)
        {
            SasObservation.Require(observation, SasObservationPoint.Qualification);
            if (award < 0 || wager < 1 || maxWager < 1) throw new ArgumentOutOfRangeException();
            Observation = observation; EvaluatedAward = award; Wager = wager; EffectiveMaximumWager = maxWager;
            SasPayload config = observation.Payload;
            Failure = !config.Available ? SasQualificationFailure.Unavailable :
                config.Mode != 0 && (config.Mode != 1 || wager != maxWager) ? SasQualificationFailure.Mode :
                award == 0 ? SasQualificationFailure.ZeroAward :
                award < config.Minimum || award > config.Maximum ? SasQualificationFailure.OutsideRange :
                SasQualificationFailure.None;
        }
    }

    public sealed class SasDecision
    {
        public readonly SasQualification Qualification;
        public readonly SasObservation Display, Recorded, Arithmetic;
        public readonly long PostMultiplierAward;
        public readonly bool Applied;
        public readonly byte RecordedMultiplier;
        public SasDecision(SasQualification qualification, SasObservation display,
            SasObservation recorded, SasObservation arithmetic)
        {
            if (qualification == null) throw new ArgumentNullException();
            Qualification = qualification;
            if (!qualification.Eligible)
            {
                if (display != null || recorded != null || arithmetic != null)
                    throw new ArgumentException("Unqualified result has application observations");
                RecordedMultiplier = 1; PostMultiplierAward = qualification.EvaluatedAward;
                return;
            }
            SasObservation.Require(display, SasObservationPoint.Display);
            SasObservation.Require(recorded, SasObservationPoint.Recorded);
            SasObservation.Require(arithmetic, SasObservationPoint.Arithmetic);
            Display = display; Recorded = recorded; Arithmetic = arithmetic;
            RecordedMultiplier = recorded.Payload.Multiplier;
            PostMultiplierAward = checked(qualification.EvaluatedAward * arithmetic.Payload.Multiplier);
            Applied = true; // M=0/1 at this later observation is not requalified.
        }
    }

    public enum SasPaymentRoute { Ordinary, Sas }
    public sealed class SasSpecialPacket
    {
        public readonly long LineWins, TotalWin;
        public readonly byte Multiplier;
        public readonly SasObservation Observation;
        private readonly byte[] raw;
        internal SasSpecialPacket(long lineWins, byte multiplier, long totalWin, SasObservation observation)
        {
            LineWins = lineWins; Multiplier = multiplier; TotalWin = totalWin; Observation = observation;
            raw = new byte[28];
            SasLittleEndian.Write32(raw, 0, 0x335000); SasLittleEndian.Write32(raw, 4, 28);
            SasLittleEndian.Write64(raw, 8, lineWins); raw[16] = multiplier;
            SasLittleEndian.Write64(raw, 20, totalWin);
        }
        public byte[] CopyBytes() { return (byte[])raw.Clone(); }
    }
    public static class SasPaymentPlanning
    {
        public static SasPaymentRoute SelectRoute(SasObservation route)
        {
            SasObservation.Require(route, SasObservationPoint.Route);
            return route.Payload.Available ? SasPaymentRoute.Sas : SasPaymentRoute.Ordinary;
        }
        private static long Cents(long finalAward, long denomination)
        {
            if (finalAward < 0 || denomination < 1) throw new ArgumentOutOfRangeException();
            return checked(finalAward * denomination);
        }
        public static byte[] Query(long finalAward, long denomination)
        {
            long amount = Cents(finalAward, denomination);
            byte[] raw = new byte[16];
            SasLittleEndian.Write32(raw, 0, 0x336000); SasLittleEndian.Write32(raw, 4, 16);
            SasLittleEndian.Write64(raw, 8, amount);
            return raw;
        }
        public static bool ClassifyQueryResponse(int response) { return response > 0; }
        public static SasSpecialPacket Packet(SasDecision decision, long finalAward,
            long denomination, SasObservation packet)
        {
            if (decision == null) throw new ArgumentNullException();
            SasObservation.Require(packet, SasObservationPoint.Packet);
            if (finalAward == 0) throw new InvalidOperationException("Zero award has no payment packet");
            long total = Cents(finalAward, denomination);
            byte multiplier = packet.Payload.Multiplier;
            if (multiplier == 0) throw new InvalidOperationException("Packet multiplier is zero");
            return new SasSpecialPacket(decision.Applied ? total / multiplier : total,
                multiplier, decision.Applied ? total : 0, packet);
        }
    }

    public sealed class SasSettlementNumbers
    {
        public readonly long CreditBefore, ProjectedBaseCredit, ProjectedFinalCredit;
        public readonly long Wagered, Returned, Rounds;
        public readonly int Wins;
        // This creates numbers only. Applying them requires a durable archive candidate.
        public SasSettlementNumbers(SasDecision decision, long finalAward, long creditBefore,
            long denomination, long remainder, long priorWagered, long priorReturned,
            long priorRounds, int priorWins)
        {
            if (decision == null) throw new ArgumentNullException();
            if (finalAward < 0 || creditBefore < 0 || denomination < 1 || remainder < 0 ||
                remainder >= denomination || priorWagered < 0 || priorReturned < 0 ||
                priorRounds < 0 || priorWins < 0 || priorWins > priorRounds)
                throw new ArgumentOutOfRangeException();
            checked
            {
                CreditBefore = creditBefore;
                ProjectedBaseCredit = creditBefore + decision.PostMultiplierAward;
                ProjectedFinalCredit = creditBefore + finalAward;
                long beforeCents = creditBefore * denomination + remainder;
                long baseCents = ProjectedBaseCredit * denomination + remainder;
                long finalCents = ProjectedFinalCredit * denomination + remainder;
                if (beforeCents < 0 || baseCents < 0 || finalCents < 0) throw new OverflowException();
                Wagered = priorWagered + decision.Qualification.Wager;
                Returned = priorReturned + decision.PostMultiplierAward;
                Rounds = priorRounds + 1;
                // Explicit PC policy: preserve evaluation-stage win counting, including A>0/B=0.
                Wins = priorWins + (decision.Qualification.EvaluatedAward > 0 ? 1 : 0);
            }
        }
    }
}
