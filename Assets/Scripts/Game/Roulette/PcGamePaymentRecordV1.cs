using System;

namespace RouletteDeluxe
{
    public enum PcGamePaymentOwner { Base = 1, Double = 2 }
    public enum PcGamePaymentOffer { None = 0, Open = 1, Take = 2, Enter = 3, Transferred = 4 }

    // PC adapter receipt, not a native packet or a second wallet. Its identity,
    // original money values and priorHistoryCount are immutable after creation.
    // The archive owner commits Applied and the one authoritative wallet in the
    // same envelope. A completion callback never adds money on its own.
    [Serializable]
    public sealed class PcGamePaymentRecordV1
    {
        public int version = 1;
        public string roundArchiveId;
        public string spinJournalId;
        public string profile;
        public string operationId;
        public int priorHistoryCount;
        public long creditBefore;
        public long denomination;
        public long remainderCents;
        public long baseAward;
        public PcGamePaymentOwner owner = PcGamePaymentOwner.Base;
        public PcGamePaymentOffer baseOffer;
        public string featureId;
        public bool prepared;
        public long finalAward;
        public int replyValue;
        public bool moneyApplied;
        public bool creditNotified;
        public bool completionReceived;
        public bool zeroResolved;
        public bool historyPublished;

        public bool IsResolved
        {
            get
            {
                return prepared && moneyApplied && (finalAward == 0 ? zeroResolved :
                    replyValue != 0 && creditNotified && completionReceived);
            }
        }

        public PcGamePaymentRecordV1 Clone()
        {
            return (PcGamePaymentRecordV1)MemberwiseClone();
        }

        public void Validate()
        {
            if (version != 1 || !PcRoundLifecycleRecord.IsCanonicalId(roundArchiveId) ||
                !PcRoundLifecycleRecord.IsCanonicalId(spinJournalId) || roundArchiveId == spinJournalId ||
                string.IsNullOrWhiteSpace(profile) ||
                !string.Equals(operationId, roundArchiveId + ":game-payout", StringComparison.Ordinal) ||
                priorHistoryCount < 0 || priorHistoryCount > 50 || creditBefore < 0 ||
                denomination < 1 || remainderCents < 0 || remainderCents >= denomination ||
                baseAward < 0 || finalAward < 0)
                throw new PcRoundLifecycleDataException("Game payment identity or amount is invalid");

            if (baseOffer < PcGamePaymentOffer.None || baseOffer > PcGamePaymentOffer.Transferred ||
                (baseAward == 0 && baseOffer != PcGamePaymentOffer.None))
                throw new PcRoundLifecycleDataException("Game payment offer is invalid");

            if (owner == PcGamePaymentOwner.Base)
            {
                if (!string.IsNullOrEmpty(featureId) || baseOffer == PcGamePaymentOffer.Transferred ||
                    (prepared && finalAward != baseAward))
                    throw new PcRoundLifecycleDataException("Base payment ownership is invalid");
            }
            else if (owner == PcGamePaymentOwner.Double)
            {
                if (baseOffer != PcGamePaymentOffer.Transferred || baseAward == 0 ||
                    !PcRoundLifecycleRecord.IsCanonicalId(featureId) || featureId == roundArchiveId ||
                    featureId == spinJournalId)
                    throw new PcRoundLifecycleDataException("Double payment ownership is invalid");
            }
            else throw new PcRoundLifecycleDataException("Game payment owner is unsupported");

            if (!prepared && (finalAward != 0 || replyValue != 0 || moneyApplied || creditNotified ||
                completionReceived || zeroResolved || historyPublished))
                throw new PcRoundLifecycleDataException("Unprepared game payment has effects");
            if (prepared && (baseOffer == PcGamePaymentOffer.Open || baseOffer == PcGamePaymentOffer.Enter))
                throw new PcRoundLifecycleDataException("Game payment was prepared before its offer decision");
            if (finalAward == 0 && (replyValue != 0 || creditNotified || completionReceived))
                throw new PcRoundLifecycleDataException("Zero game payment has positive-award notifications");
            if (zeroResolved && (!prepared || finalAward != 0 || !moneyApplied))
                throw new PcRoundLifecycleDataException("Zero game payment resolution is out of order");
            if (creditNotified && (!moneyApplied || finalAward == 0))
                throw new PcRoundLifecycleDataException("Game credit notification precedes its money effect");
            if (completionReceived && !creditNotified)
                throw new PcRoundLifecycleDataException("Game completion precedes its credit notification");
            if (historyPublished && !IsResolved)
                throw new PcRoundLifecycleDataException("Game history precedes resolved payment");

            try
            {
                // Amounts are denomination units; the one wallet also has an
                // integer cents representation, including its unchanged remainder.
                checked
                {
                    long beforeCents = creditBefore * denomination + remainderCents;
                    long baseCents = baseAward * denomination;
                    long projectedBaseCents = (creditBefore + baseAward) * denomination + remainderCents;
                    long settledCents = (creditBefore + finalAward) * denomination + remainderCents;
                    if (beforeCents < 0 || baseCents < 0 || projectedBaseCents < 0 || settledCents < 0)
                        throw new OverflowException();
                }
            }
            catch (OverflowException error)
            {
                throw new PcRoundLifecycleDataException("Game payment money representation overflows", error);
            }
        }
    }

    // Every operation is a pure candidate transformation. None persists state,
    // updates a wallet, sends IPC, changes feature NV, or publishes a history page.
    public static class PcGamePaymentTransitions
    {
        private static PcGamePaymentRecordV1 Candidate(PcGamePaymentRecordV1 value)
        {
            if (value == null) throw new PcRoundLifecycleDataException("Game payment record is missing");
            value.Validate();
            return value.Clone();
        }

        private static PcGamePaymentRecordV1 Checked(PcGamePaymentRecordV1 value)
        {
            value.Validate();
            return value;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new PcRoundLifecycleDataException(message);
        }

        private static void RequirePositive(PcGamePaymentRecordV1 value)
        {
            Require(value.prepared && value.finalAward > 0, "Game payment needs a prepared positive award");
        }

        public static PcGamePaymentRecordV1 OfferOpened(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            if (next.baseOffer == PcGamePaymentOffer.Open) return next;
            Require(next.owner == PcGamePaymentOwner.Base && !next.prepared && next.baseAward > 0 &&
                next.baseOffer == PcGamePaymentOffer.None, "Game offer cannot open in this payment state");
            next.baseOffer = PcGamePaymentOffer.Open;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 OfferTaken(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            if (next.baseOffer == PcGamePaymentOffer.Take) return next;
            Require(next.owner == PcGamePaymentOwner.Base && !next.prepared &&
                (next.baseOffer == PcGamePaymentOffer.Open || next.baseOffer == PcGamePaymentOffer.Enter),
                "Game offer cannot be taken in this payment state");
            next.baseOffer = PcGamePaymentOffer.Take;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 OfferEntered(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            if (next.baseOffer == PcGamePaymentOffer.Enter) return next;
            Require(next.owner == PcGamePaymentOwner.Base && !next.prepared &&
                next.baseOffer == PcGamePaymentOffer.Open, "Game offer cannot enter in this payment state");
            next.baseOffer = PcGamePaymentOffer.Enter;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 TransferToDouble(PcGamePaymentRecordV1 value, string featureId)
        {
            var next = Candidate(value);
            if (next.owner == PcGamePaymentOwner.Double)
            {
                Require(string.Equals(next.featureId, featureId, StringComparison.Ordinal),
                    "Game payment was transferred to another feature");
                return next;
            }
            Require(!next.prepared && next.baseOffer == PcGamePaymentOffer.Enter,
                "Game payment needs an entered offer before feature transfer");
            next.owner = PcGamePaymentOwner.Double;
            next.baseOffer = PcGamePaymentOffer.Transferred;
            next.featureId = featureId;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 Prepare(PcGamePaymentRecordV1 value, long finalAward)
        {
            var next = Candidate(value);
            if (next.prepared)
            {
                Require(next.finalAward == finalAward, "Game payment already has another final award");
                return next;
            }
            next.prepared = true;
            next.finalAward = finalAward;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 Reply(PcGamePaymentRecordV1 value, int reply)
        {
            var next = Candidate(value);
            RequirePositive(next);
            Require(next.replyValue == 0 || next.replyValue == reply,
                "Game payment accepted reply cannot be changed");
            next.replyValue = reply;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 Applied(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            Require(next.prepared, "Game payment must be prepared before its money effect");
            next.moneyApplied = true;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 CreditNotified(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            RequirePositive(next);
            Require(next.moneyApplied, "Game credit notification precedes its money effect");
            next.creditNotified = true;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 Completed(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            RequirePositive(next);
            Require(next.creditNotified, "Game completion precedes its credit notification");
            next.completionReceived = true;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 ResolveZero(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            Require(next.prepared && next.finalAward == 0 && next.moneyApplied,
                "Zero game payment needs a prepared no-op money effect");
            next.zeroResolved = true;
            return Checked(next);
        }

        public static PcGamePaymentRecordV1 Published(PcGamePaymentRecordV1 value)
        {
            var next = Candidate(value);
            Require(next.IsResolved, "Game history requires resolved payment");
            next.historyPublished = true;
            return Checked(next);
        }
    }
}
