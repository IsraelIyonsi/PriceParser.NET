using System.Text.RegularExpressions;

namespace PriceParsing;

internal static class AmountText
{
    private const string Euro = "\u20AC";
    private const string FreeWord = "free";
    private const string FreeAmount = "0";
    private const string Apostrophe = "'";
    private const string Space = " ";
    private const char Dot = '.';
    private const char Comma = ',';

    private static readonly Regex Whitespace = new(@"\s+", RegexOptions.CultureInvariant, RegexLimits.Timeout);

    private static readonly Regex EuroDecimal = new(
        @"[\d\s.,']*?\d\s*?\u20AC(\s*?)?\d(?(1)\d|\d*?)(?:$|[^\d])",
        RegexOptions.CultureInvariant,
        RegexLimits.Timeout);

    private static readonly Regex Number = new(
        @"([.]?\d[\d\s.,']*)\s*?(?:[^%\d]|$)",
        RegexOptions.CultureInvariant,
        RegexLimits.Timeout);

    internal static string? Extract(string price)
    {
        price = Whitespace.Replace(price, Space);

        if (CountOf(price, Euro) == 1)
        {
            Match euro = EuroDecimal.Match(price);
            if (euro.Success)
            {
                return euro.Value.Replace(Space, string.Empty, StringComparison.Ordinal);
            }
        }

        Match m = Number.Match(price);
        if (m.Success)
        {
            string text = m.Groups[1].Value.TrimEnd(Comma, Dot).Replace(Apostrophe, string.Empty, StringComparison.Ordinal);
            return text.Count(c => c == Dot) == 1 ? text.Trim() : text.TrimStart(Comma, Dot).Trim();
        }

        return price.ToLowerInvariant().Contains(FreeWord, StringComparison.Ordinal) ? FreeAmount : null;
    }

    private static int CountOf(string text, string token)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
    }
}
