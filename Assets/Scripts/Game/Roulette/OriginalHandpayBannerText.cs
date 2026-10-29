using System;
using System.Text;

namespace RouletteDeluxe
{
    // CreditOutMgr::DisplayHandPayScreen, 081ef270. This preserves the
    // raw keys passed to DisplayTextScreen; translation is a separate step.
    public static class OriginalHandpayBannerText
    {
        public sealed class Content
        {
            public byte[] Title, Body, Amount;
            public bool AlternateScreen, TranslateFirstTwoLines;
        }
        public static Content Create(long amount, bool jurisdiction3a8, bool jurisdiction448,
            bool manager3aa, bool selected1bd, Func<long, byte[]> formatMoney)
        {
            if (formatMoney == null) throw new ArgumentNullException(nameof(formatMoney));
            byte[] formatted = formatMoney(amount);
            if (formatted == null) throw new InvalidOperationException("Native amount format missing");
            int length = Array.IndexOf(formatted, (byte)0); if (length < 0) length = formatted.Length;
            byte[] money = new byte[length]; Array.Copy(formatted, money, length);
            bool alternate = manager3aa && selected1bd;
            bool keyed = alternate || jurisdiction448;
            string prefix = jurisdiction3a8 ? "VOUCHER" : "HAND";
            byte[] body;
            if (keyed) body = Encoding.ASCII.GetBytes(prefix + "_PAY");
            else
            {
                byte[] lead = Encoding.ASCII.GetBytes(prefix + " PAY ");
                body = new byte[lead.Length + money.Length]; Array.Copy(lead, body, lead.Length); Array.Copy(money, 0, body, lead.Length, money.Length);
            }
            return new Content { Title = Encoding.ASCII.GetBytes(keyed ? "Call_Attendant" : "Call Attendant"),
                Body = body, Amount = keyed ? money : null, AlternateScreen = alternate, TranslateFirstTwoLines = jurisdiction448 };
        }
        public static byte[] Translate(byte[] key, OriginalLanguageTranslator translator, string language, Func<string, byte[]> encode)
        {
            if (key == null || translator == null || encode == null) throw new ArgumentNullException();
            return encode(translator.Translate(Encoding.ASCII.GetString(key), language, false,
                new string[0], null, null, null));
        }
    }
    public static class OriginalHandpayBannerTextValidation
    {
        public static int Validate(Func<string, byte[]> read, OriginalPlatformMonetaryLocale monetary)
        {
            int checks = 0;
            Action<bool> check = b => { if (!b) throw new Exception("Handpay banner text regression " + checks); ++checks; };
            var translator = OriginalLanguageTranslator.Load(read);
            for (int flags = 0; flags < 16; ++flags)
            {
                bool voucher = (flags & 1) != 0, translate = (flags & 2) != 0, manager = (flags & 4) != 0, selected = (flags & 8) != 0;
                int calls = 0;
                var text = OriginalHandpayBannerText.Create(995, voucher, translate, manager, selected,
                    value => { check(value == 995); ++calls; return Encoding.ASCII.GetBytes("RM9.95\0unused"); });
                bool keyed = translate || (manager && selected);
                check(calls == 1 && text.AlternateScreen == (manager && selected) && text.TranslateFirstTwoLines == translate);
                check(Encoding.ASCII.GetString(text.Title) == (keyed ? "Call_Attendant" : "Call Attendant"));
                check(Encoding.ASCII.GetString(text.Body) == ((voucher ? "VOUCHER" : "HAND") + (keyed ? "_PAY" : " PAY RM9.95")));
                check(keyed ? Encoding.ASCII.GetString(text.Amount) == "RM9.95" : text.Amount == null);
            }
            var money = OriginalHandpayBannerText.Create(995, false, false, false, false,
                value => Encoding.UTF8.GetBytes(monetary.FormatText(value, 100, false, 0)));
            check(Encoding.UTF8.GetString(money.Body) == "HAND PAY RM9.95");
            string[] languages = { "en", "zh", "es_CL", "fr_CA" };
            string[] keys = { "Call_Attendant", "HAND_PAY", "VOUCHER_PAY" };
            string[][] expected = {
                new[] { "Call Attendant", "HAND PAY", "VOUCHER PAY" },
                new[] { "召唤服务员", "人手派彩", "积分券派彩" },
                new[] { "LLAMAR EL ASISTENTE", "PAGO DE MANO", "PAGO DE VALE" },
                new[] { "Appeler préposé", "Paiement manuel", "VOUCHER PAY" } };
            for (int language = 0; language < languages.Length; ++language)
                for (int key = 0; key < keys.Length; ++key)
                    check(Encoding.UTF8.GetString(OriginalHandpayBannerText.Translate(Encoding.ASCII.GetBytes(keys[key]), translator,
                        languages[language], Encoding.UTF8.GetBytes)) == expected[language][key]);
            byte[] fixture = { 0xff, 0x80, 0, 1 };
            var raw = OriginalHandpayBannerText.Create(long.MinValue, false, true, false, false, value => { check(value == long.MinValue); return fixture; });
            fixture[0] = 0; check(raw.Amount[0] == 255 && raw.Amount[1] == 128 && raw.Amount.Length == 2);
            return checks;
        }
    }
}
