using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using UnityEngine;

namespace RouletteDeluxe
{
    public sealed partial class RouletteGame
    {
        [Serializable] private sealed class PaymentPresentationEntry
        { public string kind, credit, receipt; }
        [Serializable] private sealed class PaymentPresentationJournal
        { public int version = 1; public string owner, digest; public List<PaymentPresentationEntry> entries = new List<PaymentPresentationEntry>(); }
        private PaymentPresentationJournal pcPaymentPresentationJournal;
        private OriginalPaymentMessageState pcPaymentMessageState = new OriginalPaymentMessageState();
        private OriginalFlashingLabel pcPaymentFlashingLabel;
        private Dictionary<int, string> pcPaymentPhrases;
        private bool pcPaymentMessagesActive;
        private string PcPaymentPresentationOwner
        { get { return PaymentTextHash(recordedHistoryKey + "|" + session.Profile.Id + "|AU|" + OriginalMonetaryLocales.PcVirtualLocaleName + "|" + PcVirtualPaymentPresentationReceipt.Provider); } }
        private string PcPaymentPresentationKey { get { return PcCreditKey + ".Presentation." + PcPaymentPresentationOwner; } }
        private static string PaymentTextHash(string text)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", ""); }
        private string PaymentIdentity(PcCreditEvent item)
        {
            var copy = JsonUtility.FromJson<PcCreditEvent>(JsonUtility.ToJson(item));
            copy.phase = 1;
            return PaymentTextHash(PcPaymentPresentationOwner + "|" + PcCreditDigest(copy));
        }
        private bool PcPaymentPresentationEnabled
        {
            get {
#if UNITY_ANDROID && !UNITY_EDITOR
                return false;
#else
                return auNationalStandardsEnabled && !temporaryHistory && session != null;
#endif
            }
        }
        private void LoadPcPaymentPresentation(bool publish = true)
        {
            if (!PcPaymentPresentationEnabled) return;
            string raw = PlayerPrefs.GetString(PcPaymentPresentationKey, "");
            var journal = raw.Length == 0 ? new PaymentPresentationJournal { owner = PcPaymentPresentationOwner } :
                JsonUtility.FromJson<PaymentPresentationJournal>(raw);
            if (journal == null || journal.version != 1 || journal.owner != PcPaymentPresentationOwner || journal.entries == null)
                throw new InvalidDataException("Payment presentation owner");
            if (raw.Length != 0)
            {
                string digest = journal.digest; journal.digest = "";
                if (digest != PaymentTextHash(JsonUtility.ToJson(journal))) throw new InvalidDataException("Payment presentation digest");
                journal.digest = digest;
            }
            var state = new OriginalPaymentMessageState();
            var ids = new HashSet<string>();
            foreach (var entry in journal.entries)
            {
                if (entry == null) throw new InvalidDataException("Payment presentation entry");
                if (entry.kind == "start" && entry.credit == "" && entry.receipt == "") { state = state.AdmittedStartGame(); continue; }
                if (entry.kind != "collect") throw new InvalidDataException("Payment presentation kind");
                var credit = ParseValidatedPcCreditEvent(entry.credit);
                if (credit.version != 2 || credit.kind != "collect" || credit.phase < 2 || credit.profile != session.Profile.Id ||
                    credit.anchor != PcCreditAnchor || !ids.Add(credit.id)) throw new InvalidDataException("Payment presentation source receipt");
                var receipt = PcVirtualPaymentPresentationReceipt.Decode(entry.receipt, PaymentIdentity(credit), true,
                    credit.collectedCents, credit.beforeDenomination, state);
                if (receipt.Completed != (credit.phase == 3)) throw new InvalidDataException("Payment presentation completion");
                state = receipt.After;
            }
            pcPaymentPresentationJournal = journal; pcPaymentMessageState = state;
            if (publish) PublishPcPaymentMessages();
        }
        private void CommitPcPaymentPresentation()
        {
            pcPaymentPresentationJournal.digest = "";
            pcPaymentPresentationJournal.digest = PaymentTextHash(JsonUtility.ToJson(pcPaymentPresentationJournal));
            string raw = JsonUtility.ToJson(pcPaymentPresentationJournal);
            PlayerPrefs.SetString(PcPaymentPresentationKey, raw); PlayerPrefs.Save();
            if (PlayerPrefs.GetString(PcPaymentPresentationKey, "") != raw) throw new InvalidDataException("Payment presentation save changed");
            // Publication follows the durable candidate, including cold recovery.
            LoadPcPaymentPresentation();
        }
        private void SavePcPaymentPresentationForCredit(PcCreditEvent item)
        {
            if (!PcPaymentPresentationEnabled) return;
            var admitted = ReadPcCreditEvent();
            if (admitted == null || admitted.digest != item.digest || admitted.id != item.id || admitted.anchor != PcCreditAnchor)
                throw new InvalidDataException("Payment presentation current transaction");
            LoadPcPaymentPresentation();
            var entries = pcPaymentPresentationJournal.entries;
            var before = new OriginalPaymentMessageState();
            PcVirtualPaymentPresentationReceipt receipt = null;
            PaymentPresentationEntry target = null;
            foreach (var entry in entries)
            {
                if (entry.kind == "start") { before = before.AdmittedStartGame(); continue; }
                var source = ParseValidatedPcCreditEvent(entry.credit);
                var prior = PcVirtualPaymentPresentationReceipt.Decode(entry.receipt, PaymentIdentity(source), true,
                    source.collectedCents, source.beforeDenomination, before);
                if (source.id == item.id)
                {
                    if (!ReferenceEquals(entry, entries[entries.Count - 1]) || PaymentIdentity(source) != PaymentIdentity(item) ||
                        source.phase > item.phase) throw new InvalidDataException("Payment presentation replay order");
                    receipt = prior; target = entry; break;
                }
                before = prior.After;
            }
            if (receipt == null)
            {
                receipt = PcVirtualPaymentPresentationReceipt.Begin(PaymentIdentity(item), true, item.collectedCents,
                    item.beforeDenomination, before);
                target = new PaymentPresentationEntry { kind = "collect" }; entries.Add(target);
            }
            if (item.phase == 3) receipt = receipt.Complete();
            target.credit = JsonUtility.ToJson(item); target.receipt = receipt.Encode();
            CommitPcPaymentPresentation();
        }
        private void SavePcPaymentPresentationAdmittedStart()
        {
            if (!PcPaymentPresentationEnabled) return;
            LoadPcPaymentPresentation();
            // With no pending byte, admitted StartGame has an independently
            // known all-zero presentation baseline. Retire its old event chain.
            if (!pcPaymentMessageState.CollectPending) pcPaymentPresentationJournal.entries.Clear();
            pcPaymentPresentationJournal.entries.Add(new PaymentPresentationEntry { kind = "start", credit = "", receipt = "" });
            CommitPcPaymentPresentation();
            pcPaymentMessagesActive = false;
            if (pcPaymentFlashingLabel != null) pcPaymentFlashingLabel.SetState(0, System.Diagnostics.Stopwatch.GetTimestamp());
        }
        private string PcPaymentPhrase(OriginalPaymentMessageSlot slot)
        {
            if (pcPaymentPhrases == null)
            {
                string path = Path.Combine(Application.streamingAssetsPath, "Original/definitions/BAL_246490A/RouletteDeluxeSZ/Translations/LangTransEntry.xml");
                byte[] bytes = File.ReadAllBytes(path);
                using (var sha = SHA256.Create())
                    if (BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "") != "7437FBB347C9315346806128B33CC616D0D3CC8C9A3529CAFFD0ACC8EFD204A3")
                        throw new InvalidDataException("Original payment phrases changed");
                var doc = new XmlDocument();
                using (var stream = new MemoryStream(bytes))
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null })) doc.Load(reader);
                var phrases = doc.SelectNodes("//TranslationEntryObj");
                pcPaymentPhrases = new Dictionary<int, string>();
                foreach (int id in new[] { 58, 198, 199, 397, 398, 399, 400 })
                    pcPaymentPhrases.Add(id, phrases[id - 1].SelectSingleNode("Phrase").Attributes["value"].Value);
            }
            string value = pcPaymentPhrases[slot.PhraseId];
            foreach (string argument in new[] { slot.Amount, slot.TotalPaid })
            {
                if (argument == null) continue;
                int at = value.IndexOf("%s", StringComparison.Ordinal);
                if (at < 0) throw new InvalidDataException("Original payment phrase arguments");
                value = value.Substring(0, at) + argument + value.Substring(at + 2);
            }
            if (value.Contains("%s")) throw new InvalidDataException("Original payment phrase is incomplete");
            return value;
        }
        private void PublishPcPaymentMessages()
        {
            var update = pcPaymentMessageState.Messages(true, OriginalMonetaryLocales.Load(OriginalMonetaryLocales.PcVirtualLocaleName), session.Denomination);
            if (update == null) { pcPaymentMessagesActive = false; return; }
            if (pcPaymentFlashingLabel == null)
            {
                pcPaymentFlashingLabel = new OriginalFlashingLabel(System.Diagnostics.Stopwatch.Frequency);
                pcPaymentFlashingLabel.SetMessage(2, 0, PcPaymentPhrase(new OriginalPaymentMessageSlot(58, null, null)), "MessageFont-uni", 2500, 250);
            }
            pcPaymentFlashingLabel.ApplyPaymentUpdate(update, PcPaymentPhrase, "MessageFont-uni", System.Diagnostics.Stopwatch.GetTimestamp());
            pcPaymentMessagesActive = true;
        }
        private void ServicePcPaymentMessages()
        {
            if (pcPaymentMessagesActive && pcPaymentFlashingLabel != null)
                pcPaymentFlashingLabel.Service(System.Diagnostics.Stopwatch.GetTimestamp(), PcGameAppSuspended,
                    null, System.Diagnostics.Stopwatch.GetTimestamp);
        }
        private void CompleteRestoredPcPaymentPresentation(PcCreditEvent item)
        {
            if (!PcPaymentPresentationEnabled || item.kind != "collect" || item.version != 2 || item.phase != 3 ||
                pcPaymentPresentationJournal == null || pcPaymentPresentationJournal.entries.Count == 0) return;
            var last = pcPaymentPresentationJournal.entries[pcPaymentPresentationJournal.entries.Count - 1];
            if (last.kind != "collect") return;
            var prior = ParseValidatedPcCreditEvent(last.credit);
            if (prior.id == item.id && prior.phase == 2) SavePcPaymentPresentationForCredit(item);
        }
    }
}
