using System;
using System.Globalization;
using System.Text;

namespace LascheApp.Padeye
{
    internal static class ReportNumberFormatter
    {
        public static string Format(double value, int decimals)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                return value.ToString(CultureInfo.CurrentCulture);

            double absoluteValue = Math.Abs(value);
            bool useScientificNotation =
                absoluteValue >= 10000.0 ||
                (absoluteValue > 0.0 && absoluteValue < 0.001);

            string fixedFormat = decimals > 0
                ? "0." + new string('0', decimals)
                : "0";

            if (!useScientificNotation)
                return value.ToString(fixedFormat, CultureInfo.CurrentCulture);

            int exponent = (int)Math.Floor(Math.Log10(absoluteValue));
            double mantissa = value / Math.Pow(10.0, exponent);
            mantissa = Math.Round(mantissa, decimals, MidpointRounding.AwayFromZero);
            if (Math.Abs(mantissa) >= 10.0)
            {
                mantissa /= 10.0;
                exponent++;
            }
            return mantissa.ToString(fixedFormat, CultureInfo.CurrentCulture) +
                   " · 10" + ToSuperscript(exponent);
        }

        private static string ToSuperscript(int exponent)
        {
            const string superscriptDigits = "⁰¹²³⁴⁵⁶⁷⁸⁹";
            StringBuilder result = new();
            if (exponent < 0)
            {
                result.Append('⁻');
                exponent = -exponent;
            }

            foreach (char digit in exponent.ToString(CultureInfo.InvariantCulture))
                result.Append(superscriptDigits[digit - '0']);
            return result.ToString();
        }
    }
}
