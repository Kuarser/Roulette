using System;
using System.IO;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable] private sealed class PcCollectMeterRecord
        {
            public int version = 1;
            public string owner, credit = "", clearedAtFinancialId = "", digest;
            public bool cleared;
        }
        private PcCollectMeterRecord pcCollectMeterRecord;
        private double pcCollectMeterCredits;
        private string PcCollectMeterOwner
        { get { return PaymentTextHash(recordedHistoryKey + "|" + session.Profile.Id + "|" + PcVirtualPaymentPresentationReceipt.Provider + "|CollectMeter.v1"); } }
        private string PcCollectMeterKey { get { return PcCreditKey + ".CollectMeter." + PcCollectMeterOwner; } }
        private bool PcCollectMeterEnabled
        {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
                return false;
#else
                return !temporaryHistory && session is RouletteSession;
#endif
            }
        }
        private bool PcCollectMeterVisible
        { get { return PcCollectMeterEnabled && !auNationalStandardsEnabled && pcCollectMeterRecord != null &&
            !pcCollectMeterRecord.cleared && pcCollectMeterCredits > 0 && !spinning &&
            (sourceGameplayState == SourceGameplayState.Idle || sourceGameplayState == SourceGameplayState.Bet); } }
        private void LoadPcCollectMeter()
        {
            if (!PcCollectMeterEnabled) return;
            string raw = PlayerPrefs.GetString(PcCollectMeterKey, "");
            if (raw.Length == 0) { pcCollectMeterRecord = null; pcCollectMeterCredits = 0; return; }
            var record = JsonUtility.FromJson<PcCollectMeterRecord>(raw);
            if (record == null || record.version != 1 || record.owner != PcCollectMeterOwner ||
                record.credit == null || record.clearedAtFinancialId == null)
                throw new InvalidDataException("Collect meter owner");
            string digest = record.digest; record.digest = "";
            if (digest != PaymentTextHash(JsonUtility.ToJson(record))) throw new InvalidDataException("Collect meter digest");
            record.digest = digest;
            double amount = 0;
            if (record.credit.Length != 0)
            {
                var source = ParseValidatedPcCreditEvent(record.credit);
                if (source.version != 2 || source.kind != "collect" || source.phase != 2 || source.profile != session.Profile.Id ||
                    (!record.cleared && source.anchor != PcCreditAnchor)) throw new InvalidDataException("Collect meter source receipt");
                var current = ReadPcCreditEvent();
                if (!record.cleared && (current == null || current.profile != source.profile || current.anchor != source.anchor ||
                    current.baseCredit != source.baseCredit || current.baseDenomination != source.baseDenomination ||
                    current.baseRemainderCents != source.baseRemainderCents)) throw new InvalidDataException("Collect meter financial baseline");
                if (!record.cleared) amount = OriginalBinary80.Divide(OriginalBinary80.Integer64(source.collectedCents),
                    OriginalBinary80.Integer64(source.beforeDenomination)).ToDouble();
            }
            else if (!record.cleared) throw new InvalidDataException("Collect meter empty active source");
            pcCollectMeterRecord = record; pcCollectMeterCredits = amount;
        }
        private void CommitPcCollectMeter()
        {
            pcCollectMeterRecord.digest = "";
            pcCollectMeterRecord.digest = PaymentTextHash(JsonUtility.ToJson(pcCollectMeterRecord));
            string raw = JsonUtility.ToJson(pcCollectMeterRecord);
            PlayerPrefs.SetString(PcCollectMeterKey, raw); PlayerPrefs.Save();
            if (PlayerPrefs.GetString(PcCollectMeterKey) != raw) throw new InvalidDataException("Collect meter save changed");
            LoadPcCollectMeter();
        }
        private void SavePcCollectMeterCredit(PcCreditEvent item)
        {
            if (!PcCollectMeterEnabled) return;
            var current = ReadPcCreditEvent();
            if (current == null || current.id != item.id || current.digest != item.digest || item.version != 2 ||
                item.kind != "collect" || item.phase < 2 || item.anchor != PcCreditAnchor || item.profile != session.Profile.Id)
                throw new InvalidDataException("Collect meter current transaction");
            LoadPcCollectMeter();
            var copy = JsonUtility.FromJson<PcCreditEvent>(JsonUtility.ToJson(item));
            copy.phase = 2; copy.digest = PcCreditDigest(copy);
            pcCollectMeterRecord = new PcCollectMeterRecord { owner = PcCollectMeterOwner, credit = JsonUtility.ToJson(copy) };
            CommitPcCollectMeter();
            // Game::UpdateCollectDisplay disables gameplay animations. AU
            // still owns its win-only meter and independent payment message.
            if (!auNationalStandardsEnabled) footerMoviePhase = FooterMoviePhase.Hidden;
        }
        private void RestorePcCollectMeterBeforeCredit(PcCreditEvent item)
        {
            LoadPcCollectMeter();
            // Art can start its initial Intro before LoadProfile restores
            // the native collect value. This applies to a later deposit too.
            if (PcCollectMeterVisible) footerMoviePhase = FooterMoviePhase.Hidden;
            if (!PcCollectMeterEnabled || item == null || item.version != 2 || item.kind != "collect" || item.phase < 2 ||
                item.anchor != PcCreditAnchor || item.profile != session.Profile.Id) return;
            // A later ClearPaid marker must survive a cold final completion.
            if (pcCollectMeterRecord != null && pcCollectMeterRecord.cleared && pcCollectMeterRecord.clearedAtFinancialId == item.id) return;
            if (pcCollectMeterRecord != null && !pcCollectMeterRecord.cleared &&
                ParseValidatedPcCreditEvent(pcCollectMeterRecord.credit).id == item.id)
            {
                if (!auNationalStandardsEnabled) footerMoviePhase = FooterMoviePhase.Hidden;
                return;
            }
            // A durable phase 2/3 financial fact independently reconstructs
            // this single value if the display write was interrupted.
            SavePcCollectMeterCredit(item);
        }
        private void ClearPcCollectMeter()
        {
            if (!PcCollectMeterEnabled) return;
            LoadPcCollectMeter();
            var current = ReadPcCreditEvent();
            if (pcCollectMeterRecord != null && pcCollectMeterRecord.cleared &&
                pcCollectMeterRecord.clearedAtFinancialId == (current == null ? "" : current.id)) return;
            if (pcCollectMeterRecord == null) pcCollectMeterRecord = new PcCollectMeterRecord { owner = PcCollectMeterOwner };
            pcCollectMeterRecord.cleared = true;
            pcCollectMeterRecord.clearedAtFinancialId = current == null ? "" : current.id;
            CommitPcCollectMeter();
        }
    }
}
