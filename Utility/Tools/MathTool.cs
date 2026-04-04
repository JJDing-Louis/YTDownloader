using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utility.Tools
{
    public class MathTool
    {
        public static decimal ParsePercentage(string percentageString)
        {
            if (string.IsNullOrWhiteSpace(percentageString))
                throw new ArgumentException("Input string cannot be null or empty.");

            // Remove any leading or trailing whitespace and the percentage sign
            percentageString = percentageString.Trim().TrimEnd('%');

            // Try to parse the remaining string as a double
            if (decimal.TryParse(percentageString, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                // Convert the parsed number to a percentage
                return result / 100m;
            }
            else
            {
                throw new FormatException("The input string is not a valid percentage format.");
            }
        }
        public static IEnumerable<int> Range(int from, int to, int increase = 1)
        {
            for (var i = from; i <= to; i += increase)
            {
                yield return i;
            }
        }
        /// <summary>
        /// value > 0 return 1
        /// value = 0 return 0
        /// value < 0 return -1
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("From .NET 7,8,9 ,please use .NET Decimal.Sign")]
        public static int Sign(decimal value)
        {
            switch (value)
            {
                case (< 0):
                    return -1;
                case > 0:
                    return 1;
                default:
                    return 0;
            }
        }

        private const int Precision = 28;

        public static decimal Round(decimal value, int NoDecimal)
        {
            var _temp = Math.Min(NoDecimal + 6, Precision);
            //decimal 二次平滑處理(針對IEEE754 rounding error)
            value = Math.Round(value, _temp, MidpointRounding.AwayFromZero);



            return Math.Round(value, NoDecimal, MidpointRounding.AwayFromZero);
        }

        public static decimal RoundUp(decimal value, int NoDecimal)
        {
            var _temp = Math.Min(NoDecimal + 6, Precision);
            //decimal 二次平滑處理(針對IEEE754 rounding error)
            value = Math.Round(value, _temp, MidpointRounding.AwayFromZero);

            var fstr = value.ToString("F30");
            var pos = fstr.IndexOf(".", StringComparison.Ordinal);
            var len = Math.Min(fstr.Length, pos + NoDecimal + 1);
            var data = fstr.Substring(0, len);
            var outValue = decimal.Parse(data);
            if (fstr.Length > len)
            {
                var rem = fstr.Substring(len, fstr.Length - len);
                if (decimal.TryParse(rem, out var remValue))
                {
                    if (remValue > 0)
                    {
                        var scale = 1m / (decimal)Math.Pow(10, NoDecimal);
                        outValue += Sign(value) * scale;
                    }
                }


            }

            return outValue;

        }

        public static decimal RoundDown(decimal value, int NoDecimal)
        {

            var _temp = Math.Min(NoDecimal + 6, Precision);
            //decimal 二次平滑處理(針對IEEE754 rounding error)
            value = Math.Round(value, _temp, MidpointRounding.AwayFromZero);

            var fstr = value.ToString("F30");
            var pos = fstr.IndexOf(".", StringComparison.Ordinal);
            var len = Math.Min(fstr.Length, pos + NoDecimal + 1);
            var data = fstr.Substring(0, len);
            var outValue = decimal.Parse(data);
            return outValue;
        }

        public static int ConvertNumber(string num)
        {
            try
            {
                return int.Parse(num);
            }
            catch (Exception e)
            {
                return ConvChineseNumber(num);
            }
        }

        public static int ConvChineseNumber(string cnum)
        {
            cnum = cnum.Replace("一十", "十");

            cnum = cnum.TrimEnd();
            var cnums = cnum.ToCharArray();
            int sum = 0, sectionUnit = 0, sectionsum = 0;
            foreach (var c in cnums)
            {
                var arab = mapCnumLetters(c);
                if (isMultiplier(c))
                {
                    if (isSegmentDelimeter(c)) // 萬/億
                    {
                        sectionsum = sum * arab;
                        sum = sectionsum;
                        if (sum < 0)
                            throw new Exception("輸入的字串無法解析!");
                        sectionsum = 0;
                    }
                    else // 十/百/千
                    {
                        if (sectionUnit == 0)
                        {
                            sectionsum = 10; // 特別處理 "十萬", "十一萬" 之類的狀況
                        }
                        else
                        {
                            sectionsum -= sectionUnit;
                            sum -= sectionUnit;
                            sectionsum = sectionUnit * arab;
                        }

                        sum += sectionsum;
                        if (sum < 0)
                            throw new Exception("輸入的字串無法解析!");
                    }
                }
                else
                {
                    sectionUnit = arab;
                    sum += arab;
                    if (sum < 0)
                        throw new Exception("輸入的字串無法解析!");
                    sectionsum += arab;
                }
            }

            return sum;
        }

        public static bool equals(double a, double b, decimal tolerance = 0.0001m)
        {
            var temp = (decimal)Math.Abs(a - b);
            return temp <= Math.Abs(tolerance);
        }

        public static bool equals(float a, float b, decimal tolerance = 0.0001m)
        {
            return equals(a, (double)b, tolerance);
        }

        private static bool isSegmentDelimeter(char cnum)
        {
            switch (cnum)
            {
                case '萬':
                    return true;
                case '億':
                    return true;
                default:
                    return false;
            }
        }

        private static bool isMultiplier(char cnum)
        {
            switch (cnum)
            {
                case '十':
                    return true;
                case '拾':
                    return true;
                case '百':
                    return true;
                case '佰':
                    return true;
                case '千':
                    return true;
                case '仟':
                    return true;
                case '萬':
                    return true;
                case '億':
                    return true;
                default:
                    return false;
            }
        }

        private static int mapCnumLetters(char cnum)
        {
            switch (cnum)
            {
                case '零':
                    return 0;
                case '一':
                    return 1;
                case '壹':
                    return 1;
                case '二':
                    return 2;
                case '貳':
                    return 2;
                case '三':
                    return 3;
                case '參':
                    return 3;
                case '四':
                    return 4;
                case '肆':
                    return 4;
                case '五':
                    return 5;
                case '伍':
                    return 5;
                case '六':
                    return 6;
                case '陸':
                    return 6;
                case '七':
                    return 7;
                case '柒':
                    return 7;
                case '八':
                    return 8;
                case '捌':
                    return 8;
                case '九':
                    return 9;
                case '玖':
                    return 9;
                case '十':
                    return 10;
                case '拾':
                    return 10;
                case '廿':
                    return 20;
                case '丗':
                    return 30;
                case '百':
                    return 100;
                case '佰':
                    return 100;
                case '千':
                    return 1000;
                case '仟':
                    return 1000;
                case '萬':
                    return 10000;
                case '億':
                    return 100000000;
                default:
                    return 0;
            }
        }
    }
}
