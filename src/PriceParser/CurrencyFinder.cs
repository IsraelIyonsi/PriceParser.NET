using System.Text.RegularExpressions;

namespace PriceParsing;

internal static class CurrencyFinder
{
    private const string DollarSign = "$";
    private const string DollarCodePrefix = @"\b(?:";
    private const string DollarCodeSuffix = @")(?=\$?(?:[\W\d]|$))";

    private static readonly Regex SafeSymbols = FromAlternatives(CurrencyTable.Safe);
    private static readonly Regex OtherSymbols = FromAlternatives(CurrencyTable.Other);
    private static readonly Regex DollarCodes = new(
        DollarCodePrefix + string.Join('|', CurrencyTable.DollarCodes.Select(Regex.Escape)) + DollarCodeSuffix,
        RegexOptions.CultureInvariant,
        RegexLimits.Timeout);

    internal static string? Find(string? price, string? hint)
    {
        string? dollarCode = null;
        if (hint is not null && hint.Contains(DollarSign, StringComparison.Ordinal))
        {
            dollarCode = Match(DollarCodes, hint);
        }

        if (price is not null && price.Contains(DollarSign, StringComparison.Ordinal))
        {
            dollarCode = Match(DollarCodes, price) ?? dollarCode;
        }

        return dollarCode
            ?? Match(SafeSymbols, price)
            ?? Match(SafeSymbols, hint)
            ?? Match(OtherSymbols, price)
            ?? Match(OtherSymbols, hint);
    }

    private static string? Match(Regex regex, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Match m = regex.Match(value);
        return m.Success ? m.Value : null;
    }

    private static Regex FromAlternatives(string[] symbols) =>
        new(string.Join('|', symbols.Select(Regex.Escape)), RegexOptions.CultureInvariant, RegexLimits.Timeout);
}
