using Microsoft.International.Formatters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utility.Tools
{
    public enum TextAlign
    {
        Right,
        Left
    }

    public static class StringTool
    {
        static StringTool()
        {
            try
            {
                var e = Encoding.GetEncoding("big5");

            }
            catch (Exception)
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
        }

        public static bool? ParseBoolean(string str, bool? defaultValue = null)
        {
            if (str == null)
                return defaultValue;
            var _str = str.Trim().ToLower();
            switch (_str)
            {
                case "t":
                case "true":
                case "y":
                case "1":
                case "yes":
                    return true;
                case "f":
                case "false":
                case "n":
                case "0":
                case "no":
                case "":
                    return false;
            }

            if (bool.TryParse(str, out var val))
                return val;


            return defaultValue;
        }

        public static decimal? ParseDecimal(string str)
        {
            decimal val;
            if (decimal.TryParse(str, out val))
                return val;
            return null;
        }

        public static DateTime? ParseISO8601String(string str)
        {
            if (ParseBoolean(str) == null && ParseDecimal(str) == null)
            {
                DateTime dt;
                if (DateTime.TryParse(str, out dt))
                    return dt;
            }


            return null;
        }


        public static string IntToLetters(int value)
        {
            var result = string.Empty;
            value++;
            while (--value >= 0)
            {
                result = (char)('A' + value % 26) + result;
                value /= 26;
            }

            return result;
        }


        public static int ChineseToInt(string s)
        {
            if (string.IsNullOrEmpty(s))
                return 0; // Recursion termination when input is divisible by 10 (no unit digit character)
            if (s[0] == '零')
                s = s.Substring(1); // Ignore '零'
            if (s[0] == '十')
                s = "一" + s; // Normalize 10-19 by padding with "一"
            var t = " 一二三四五六七八九"; // Index lookup
            return s.Length < 2
                ? t.IndexOf(s[0]) // 0-9
                : t.IndexOf(s[0]) * (s[1] == '百' ? 100 : 10) + ChineseToInt(s.Substring(2));
            // Get the first character's digit, multiply by 100 or 10 depends on 2nd character, and recursively process from 3rd character onwards
        }

        /// <summary>
        ///     將字串的數字部分轉為中文大寫
        /// </summary>
        /// <param name="s">原始字串</param>
        /// <param name="moneyChar">
        ///     是否使用金額大寫，true時使用"壹貳參肆...", false時則為"一二三四..."
        /// </param>
        /// <returns>轉換後字串</returns>
        public static string FormatChineseNumber(string s, bool moneyChar)
        {
            return Regex.Replace(s, "\\d+", m =>
            {
                var n = int.Parse(m.Value);
                return FormatChineseNumber(n, moneyChar);
            });
        }

        /// <summary>
        ///     修正EastAsiaNumericFormatter.FormatWithCulture出現"三百十"之問題，
        ///     本函數會將其修正為三百一十的慣用寫法
        ///     2015-04-12更新，增加拾萬改為壹拾萬邏輯
        /// </summary>
        /// <param name="n">要轉換的數字</param>
        /// <param name="moneyChar">
        ///     是否使用金額大寫，true時使用"壹貳參肆...", false時則為"一二三四..."
        /// </param>
        /// <returns>轉為中文大寫的數字</returns>
        public static string FormatChineseNumber(decimal n, bool moneyChar)
        {
            //"L"-大寫，壹貳參... "Ln"-一二三... "Lc"-貨幣，同L
            var t =
                EastAsiaNumericFormatter.FormatWithCulture(
                    moneyChar ? "L" : "Ln", n,
                    null, new CultureInfo("zh-TW"));
            var pattern = moneyChar ? "[^壹貳參肆伍陸柒捌玖]拾" : "[^一二三四五六七八九]十";
            var one = moneyChar ? "壹" : "一";
            var res = Regex.Replace(t, pattern, m =>
            {
                return m.Value.Substring(0, 1) + one +
                       m.Value.Substring(1);
            });
            //拾萬需補為壹拾萬
            if (moneyChar && res.StartsWith("拾")) res = "壹" + res;
            return res;
        }

        /// <summary>
        /// 字串相似度比較
        /// 計算兩個字串之間的 Levenshtein 編輯距離。
        /// 編輯距離表示將一個字串轉換為另一個字串所需的最少單字元操作次數，
        /// 操作包含：插入 (Insertion)、刪除 (Deletion) 與取代 (Substitution)。
        /// </summary>
        /// <param name="source1">第一個要比較的字串。</param>
        /// <param name="source2">第二個要比較的字串。</param>
        /// <returns>
        /// 兩個字串之間的編輯距離值。
        /// 數值越小表示兩個字串越相似，0 代表兩字串完全相同。
        /// </returns>
        public static int CalculateDistance(this string s1, string s2)
        {
            if (s1 == null) throw new ArgumentNullException(nameof(s1));
            if (s2 == null) throw new ArgumentNullException(nameof(s2));

            var n = s1.Length;
            var m = s2.Length;

            if (n == 0) return m;
            if (m == 0) return n;

            var matrix = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++)
                matrix[i, 0] = i;

            for (int j = 0; j <= m; j++)
                matrix[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[n, m];
        }


        public static string ReplaceAt(this string str, int index, int length, string replace)
        {

            return str.Remove(index, Math.Min(length, str.Length - index))
                .Insert(index, replace);
        }



        public static string Repeat(this string str, int count, string delim = "")
        {
            var k = new StringBuilder();
            for (var i = 0; i < count; i++)
                k.Append(i != 0 ? delim : "").Append(str);
            return k.ToString();
        }



        public static string PadOrTrimBig5(this string value, int length, TextAlign align = TextAlign.Left, char padChar = ' ')
        {
            byte[] outputBuffer = new byte[length];

            if (value == null)
                value = "";
            var encoder = Encoding.GetEncoding("big5");

            var defaultChar = encoder.GetBytes(padChar.ToString())[0];
            for (int i = 0; i < outputBuffer.Length; i++)
                outputBuffer[i] = defaultChar;

            if (align == TextAlign.Left)
            {
                //左靠
                // Array.Copy(source, 0, outputBuffer, 0, fillByteLength);

                var rem = outputBuffer.Length;
                if (rem > 0)
                {
                    var idx = 0;
                    for (var i = 0; i < value.Length; i++)
                    {
                        var sub = ToBig5Char(value, i);
                        if (rem >= sub.Length)
                        {
                            for (var j = 0; j < sub.Length; j++)
                            {
                                outputBuffer[idx++] = sub[j];
                                rem--;
                            }
                        }
                        else
                            break;


                    }
                }


            }
            else
            {
                //右靠

                var rem = outputBuffer.Length;
                if (rem > 0)
                {
                    var idx = outputBuffer.Length - 1;
                    for (var i = value.Length - 1; i >= 0; i--)
                    {
                        var sub = ToBig5Char(value, i);
                        if (rem >= sub.Length)
                        {
                            for (var j = sub.Length - 1; j >= 0; j--)
                            {

                                outputBuffer[idx--] = sub[j];
                                rem--;
                            }
                        }
                        else
                            break;


                    }
                }
            }

            //ReplaceInvalidBig5Bytes(outputBuffer,defaultChar);
            return encoder.GetString(outputBuffer);
        }
        public static string PadOrTrimTaiwanFormat(this DateTime? value, int length = 7, TextAlign align = TextAlign.Left)
        {
            if (value == null)
                return PadOrTrimBig5("", length, align);
            return PadOrTrimTaiwanFormat(value.Value, length, align);

        }
        public static string PadOrTrimTaiwanFormat(this DateTime value, int length = 7, TextAlign align = TextAlign.Left)
        {
            var num = (value.Year - 1911) * 10000 + value.Month * 100 + value.Day;
            var str = num.ToString("0000000");
            return PadOrTrimBig5(str, length, align);
        }
        public static string PadOrTrimBig5(this decimal? value, int integerDigital, int fractionalDigital = 0, char positiveSign = '0')
        {
            if (value == null)
                return PadOrTrimBig5(0m, integerDigital, fractionalDigital, positiveSign);
            return PadOrTrimBig5(value.Value, integerDigital, fractionalDigital, positiveSign);

        }
        public static string PadOrTrimBig5(this decimal value, int integerDigital, int fractionalDigital = 0, char integerPadChar = '0', char fractionalPadChar = '0')
        {
            if (value == null)
                value = 0;

            byte[] data = new byte[integerDigital + fractionalDigital];
            var encoder = Encoding.GetEncoding("big5");
            var t = encoder.GetBytes("0")[0];
            for (int i = 0; i < data.Length; i++)
                data[i] = t;
            bool IsNegative = value < 0;

            value = Math.Abs(value);
            var part1 = MathTool.RoundDown(value, 0);
            var scale = (decimal)Math.Pow(10, fractionalDigital);
            var fractional = value - part1;
            var part2 = MathTool.RoundDown(fractional * scale, 0);
            var part1_str = part1.ToString("F0");
            var part2_str = part2.ToString("F0");

            if (part2_str.Length < fractionalDigital)
                part2_str = Repeat("0", fractionalDigital - part2_str.Length) + part2_str;

            var s1 = PadOrTrimBig5(part1_str, integerDigital, TextAlign.Right, integerPadChar);
            var s2 = PadOrTrimBig5(part2_str, fractionalDigital, TextAlign.Left, fractionalPadChar);
            // if (s2.Length>fractionalDigital)
            //     s2=s2.Substring(0,fractionalDigital);
            var s = s1 + s2;
            if (IsNegative)
                s = s.ReplaceAt(0, 1, "-");
            return s;

        }
        public static byte[] ToBig5Char(this string str, int idx, int length = 1)
        {
            var _str = str.Substring(idx, length);
            var encoder = Encoding.GetEncoding("big5");

            return encoder.GetBytes(_str);
        }
    }
}
