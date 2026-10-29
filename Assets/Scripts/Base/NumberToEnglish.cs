using System;
using System.Collections.Generic;
public static class NumberToEnglish
{
    private static readonly string[] Ones =
    {
        "Zero", "One", "Two", "Three", "Four", "Five", "Six",
        "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve",
        "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen",
        "Eighteen", "Nineteen"
    };

    private static readonly string[] Tens =
    {
        "", "", "Twenty", "Thirty", "Forty", "Fifty",
        "Sixty", "Seventy", "Eighty", "Ninety"
    };

    private static readonly string[] Scales =
    {
        "", "Thousand", "Million", "Billion"
    };

    public static string Convert(long number)
    {
        if (number == 0)
            return "Zero";

        bool isNegative = number < 0;
        // 用 ulong 处理 long.MinValue，避免取负溢出
        ulong value = isNegative ? (ulong)(-(number + 1)) + 1 : (ulong)number;

        var parts = new List<string>();
        int scaleIndex = 0;

        while (value > 0)
        {
            int group = (int)(value % 1000);

            if (group > 0)
            {
                string groupText = ConvertUnderThousand(group);
                string scale = Scales[scaleIndex];

                parts.Insert(0, string.IsNullOrEmpty(scale)
                    ? groupText
                    : $"{groupText} {scale}");
            }

            value /= 1000;
            scaleIndex++;
        }

        string result = string.Join(" ", parts);
        return isNegative ? $"Negative {result}" : result;
    }

    private static string ConvertUnderThousand(int number)
    {
        var parts = new List<string>();

        if (number >= 100)
        {
            parts.Add(Ones[number / 100]);
            parts.Add("Hundred");
            number %= 100;
        }

        if (number >= 20)
        {
            string tens = Tens[number / 10];
            int ones = number % 10;

            parts.Add(ones == 0 ? tens : $"{tens}-{Ones[ones]}");
        }
        else if (number > 0)
        {
            parts.Add(Ones[number]);
        }

        return string.Join(" ", parts);
    }
}