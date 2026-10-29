using System;
using System.IO;
using System.Globalization;
using UnityEngine;

namespace RouletteDeluxe.Validation
{
    public static class OriginalMonetaryLocaleValidation
    {
        [Serializable] private sealed class ReferenceCase { public string locale, minorUnits, expected; }
        [Serializable] private sealed class Reference { public int schema; public ReferenceCase[] cases; }
        private static void Require(bool ok, string message)
        { if (!ok) throw new Exception("Original monetary validation: " + message); }
        public static int Validate()
        {
            int checks = 0;
            var names = OriginalMonetaryLocales.Names(); Require(names.Length == 136, "catalog count"); checks++;
            foreach (string name in names)
            {
                var locale = OriginalMonetaryLocales.Load(name);
                Require(locale.ResourceSha256.Length == 64, "resource hash " + name); checks++;
                Require(locale.FormatMinorUnits(100000).Length > 0, "source format " + name); checks++;
            }
            var rm = OriginalMonetaryLocales.Load("ms_MY.utf8");
            Require(rm.FormatMinorUnits(0) == "RM0.00" && rm.FormatMinorUnits(99) == "RM0.99" &&
                rm.FormatMinorUnits(100000001) == "RM1,000,000.01", "ringgit cents and grouping"); checks++;
            Require(rm.FormatMinorUnits(-995) == "(RM9.95)", "ringgit negative parentheses"); checks++;
            Require(rm.FormatMinorUnits(long.MaxValue) == "RM92,233,720,368,547,760.00", "x87 division and double store at Int64 boundary"); checks++;
            Require(rm.FormatPaymentCredits(0.29, 100) == "RM0.29" && rm.PaymentMinorUnits(1.999, 100) == 199,
                "native epsilon and truncation"); checks++;
            Require(rm.PaymentMinorUnits(-1, 100) == -99, "native signed truncation after epsilon"); checks++;
            Require(OriginalMonetaryLocales.Load("en_AU.utf8").FormatMinorUnits(995) == "$9.95", "Australian currency"); checks++;
            Require(OriginalMonetaryLocales.Load("ja_JP.utf8").FormatMinorUnits(995) == "¥9", "zero-digit currency truncates cents"); checks++;
            bool rejected = false; try { OriginalMonetaryLocales.Load("en_MY.utf8"); } catch (InvalidDataException) { rejected = true; }
            Require(rejected, "absent locale is not substituted"); checks++;
            byte[] original = Resources.Load<TextAsset>("Original/Locale/ms_MY.utf8/LC_MONETARY").bytes;
            foreach (int kind in new[] { 0, 1, 2, 3 })
            {
                var changed = (byte[])original.Clone();
                if (kind == 0) changed[0] ^= 1;
                if (kind == 1) { changed[8] = 0; changed[9] = 0; changed[10] = 0; changed[11] = 0; }
                int at = (int)((uint)changed[40] | (uint)changed[41] << 8 | (uint)changed[42] << 16 | (uint)changed[43] << 24);
                if (kind == 2) changed[at] = 1; // fractional digits: not in supplied resources
                if (kind == 3) changed[197] = 0xff; // invalid UTF-8 currency symbol
                rejected = false;
                try { OriginalMonetaryLocale.Read(changed); } catch (ArgumentException) { rejected = true; } catch (InvalidDataException) { rejected = true; }
                Require(rejected, "malformed locale " + kind); checks++;
            }
            return checks;
        }
        public static int ValidateReference(string json)
        {
            var reference = JsonUtility.FromJson<Reference>(json);
            Require(reference != null && reference.schema == 1 && reference.cases != null && reference.cases.Length == 1496, "reference corpus shape");
            foreach (var item in reference.cases)
            {
                long amount = long.Parse(item.minorUnits, CultureInfo.InvariantCulture);
                string actual = OriginalMonetaryLocales.Load(item.locale).FormatMinorUnits(amount);
                Require(actual == item.expected, "reference " + item.locale + " " + item.minorUnits);
            }
            return reference.cases.Length;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RunStandalone()
        {
            if (Application.isEditor) return;
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-rouletteMonetaryValidation") < 0) return;
            try
            {
                int at = Array.IndexOf(args, "-rouletteMonetaryReference");
                Require(at >= 0 && at + 1 < args.Length, "reference path missing");
                int checks = Validate() + ValidateReference(File.ReadAllText(args[at + 1]));
                checks += OriginalPaymentMessageStateReference.Validate(OriginalMonetaryLocales.Load("ms_MY.utf8"));
                checks += OriginalFlashingLabelReference.Validate();
                checks += PcVirtualPaymentPresentationReceiptReference.Validate();
                Debug.Log("ROULETTE_MONETARY_PLAYER_PASSED checks=" + checks + " catalog=" + OriginalMonetaryLocales.ManifestSha256);
                Application.Quit(0);
            }
            catch (Exception error)
            { Debug.LogError("ROULETTE_MONETARY_PLAYER_FAILED " + error); Application.Quit(9); }
        }
    }
}
