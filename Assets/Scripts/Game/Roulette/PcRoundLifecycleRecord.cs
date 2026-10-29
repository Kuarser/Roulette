using System;

namespace RouletteDeluxe
{
    public enum PcRoundStage
    {
        RoundOpen = 1, AwaitEnd = 2, EndPrepared = 3, Ended = 4
    }

    // This single receipt belongs to the latest paid archive page. Financial
    // values remain in that page; this record only controls its continuation.
    [Serializable]
    public sealed class PcRoundLifecycleRecord
    {
        public string roundArchiveId;
        public string profile;
        public string spinJournalId;
        public PcRoundStage roundStage;
        public PcRoundEndIntent endIntent;
        public PcIdleEntry idleEntry;

        public static bool IsCanonicalId(string value)
        {
            Guid parsed;
            return value != null && value.Length == 32 &&
                Guid.TryParseExact(value, "N", out parsed) && parsed != Guid.Empty &&
                string.Equals(value, parsed.ToString("N"), StringComparison.Ordinal);
        }

        public void ValidateIdentity()
        {
            if (!IsCanonicalId(roundArchiveId) || !IsCanonicalId(spinJournalId) ||
                string.IsNullOrEmpty(profile) ||
                roundStage < PcRoundStage.RoundOpen || roundStage > PcRoundStage.Ended)
                throw new PcRoundLifecycleDataException("Round lifecycle identity or stage is invalid");
        }
    }

    // The legacy archive loader catches ArgumentException and substitutes
    // placeholder pages. A claimed lifecycle record must never take that path.
    public sealed class PcRoundLifecycleDataException : Exception
    {
        public PcRoundLifecycleDataException(string message) : base(message) { }
        public PcRoundLifecycleDataException(string message, Exception inner)
            : base(message, inner) { }
    }
}
