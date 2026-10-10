using System.Globalization;

namespace PriceParsing.Tests;

internal static class CorpusAssert
{
    internal static void Check(string? hint, string? price, string? currency, string? amountText, string? amount, string? decimalSeparator)
    {
        char? separator = decimalSeparator is null ? null : decimalSeparator[0];
        PriceResult actual = PriceParser.Parse(price ?? string.Empty, hint, separator);

        decimal? expected = amount is null ? null : decimal.Parse(amount, NumberStyles.Number, CultureInfo.InvariantCulture);
        Assert.Equal(expected, actual.Amount);
        Assert.Equal(currency, actual.Currency);
        Assert.Equal(amountText, actual.AmountText);
    }
}
