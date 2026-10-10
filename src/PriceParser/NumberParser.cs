using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PriceParsing;

internal static class NumberParser
{
    private const char Euro = '\u20AC';
    private const char Dot = '.';
    private const char Comma = ',';
    private const string Space = " ";

    private static readonly Regex DecimalSeparator = new(
        @"\d*([.,\u20AC])(?:\d{1,2}?|\d{4}\d*?)$",
        RegexOptions.CultureInvariant,
        RegexLimits.Timeout);

    internal static decimal? Parse(string number, char? forcedSeparator)
    {
        if (number.Length == 0)
        {
            return null;
        }

        number = number.Trim().Replace(Space, string.Empty, StringComparison.Ordinal);
        char? separator = forcedSeparator ?? GuessSeparator(number);

        number = separator switch
        {
            null => Strip(number),
            Dot => Remove(number, Comma),
            Comma => Remove(number, Dot).Replace(Comma, Dot),
            Euro => Strip(number).Replace(Euro, Dot),
            _ => number,
        };

        return decimal.TryParse(ToAsciiDigits(number), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal value)
            ? value
            : null;
    }

    private static char? GuessSeparator(string number)
    {
        Match m = DecimalSeparator.Match(number);
        return m.Success ? m.Groups[1].Value[0] : null;
    }

    private static string Remove(string number, char c) =>
        number.Replace(c.ToString(), string.Empty, StringComparison.Ordinal);

    private static string Strip(string number) => Remove(Remove(number, Dot), Comma);

    private static string ToAsciiDigits(string number)
    {
        var sb = new StringBuilder(number.Length);
        foreach (char c in number)
        {
            int digit = char.IsAscii(c) ? -1 : CharUnicodeInfo.GetDecimalDigitValue(c);
            sb.Append(digit >= 0 ? (char)('0' + digit) : c);
        }

        return sb.ToString();
    }
}
