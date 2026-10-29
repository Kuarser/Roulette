using System;

namespace RouletteDeluxe
{
    // Saved core state is deliberately separate from the live event state.
    // Completion fields journal the PC adapter effect, not native SafeRam.
    [Serializable]
    public sealed class PcDoubleFeatureNvV1
    {
        public int version = 1;
        public string featureId;
        public string roundArchiveId;
        public string spinJournalId;
        public string profile;
        public int savedState;
        public bool autoRestoreMarker;
        public bool suitMode;
        public int stepCount;
        public string resultDigest;
        public int completionStage;
        public long completionAward;

        public PcDoubleFeatureNvV1 Clone()
        {
            return (PcDoubleFeatureNvV1)MemberwiseClone();
        }

        public void Validate()
        {
            if (version != 1 || !PcRoundLifecycleRecord.IsCanonicalId(featureId) ||
                !PcRoundLifecycleRecord.IsCanonicalId(roundArchiveId) ||
                !PcRoundLifecycleRecord.IsCanonicalId(spinJournalId) ||
                featureId == roundArchiveId || featureId == spinJournalId ||
                string.IsNullOrEmpty(profile) || stepCount < 0 || stepCount > 16 ||
                !IsDigest(resultDigest) || completionStage < 0 || completionStage > 2 ||
                completionAward < 0 || (completionStage == 0 && completionAward != 0))
                throw new PcRoundLifecycleDataException("Double Feature NV identity or completion is invalid");
            if (savedState != 0 && savedState != 2 && savedState != 3 &&
                savedState != 5 && savedState != 6 && savedState != 7)
                throw new PcRoundLifecycleDataException("Double Feature NV state is unsupported");
            // Native Feature completion precedes ordinary Game payout. A
            // Prepared/native0 record remains unpaid until that owner applies
            // the stable completion intent; native0 without an intent is invalid.
            if ((savedState == 0 && completionStage == 0) ||
                (savedState == 3 && stepCount == 0))
                throw new PcRoundLifecycleDataException("Double Feature NV state conflicts with its result");
        }

        public static bool IsDigest(string value)
        {
            if (value == null || value.Length != 64) return false;
            foreach (char c in value)
                if (!(c >= '0' && c <= '9') && !(c >= 'A' && c <= 'F')) return false;
            return true;
        }
    }
}
