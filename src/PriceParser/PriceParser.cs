using System.Text.RegularExpressions;

namespace PriceParsing;

/// <summary>Extracts a price amount and its raw currency token from free text.</summary>
public static class PriceParser
{
    /// <summary>Parses a price out of <paramref name="text"/>.</summary>
    /// <param name="text">Text that contains a price, such as <c>"Now only $1,299.00!"</c>.</param>
    /// <param name="currencyHint">Optional text from a neighbouring element that may hold the currency; the currency inside <paramref name="text"/> is preferred.</param>
    /// <param name="decimalSeparator">Forces the decimal separator (<c>.</c>, <c>,</c> or the euro sign) instead of guessing it.</param>
    /// <param name="digitGroupSeparator">Removes this digit group separator from <paramref name="text"/> before parsing.</param>
    /// <returns>A result whose <see cref="PriceResult.Amount"/> is <see langword="null"/> when the text holds no price.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    /// <exception cref="RegexMatchTimeoutException">The input is pathologically long and matching exceeded the time limit.</exception>
    public static PriceResult Parse(string text, string? currencyHint = null, char? decimalSeparator = null, char? digitGroupSeparator = null)
    {
        ArgumentNullException.ThrowIfNull(text);
        return Extract(text, currencyHint, decimalSeparator, digitGroupSeparator);
    }

    /// <summary>Parses a price without throwing.</summary>
    /// <param name="text">Text that may contain a price; <see langword="null"/>, empty and whitespace are accepted.</param>
    /// <param name="result">The parsed result; its fields are <see langword="null"/> when nothing was found.</param>
    /// <returns><see langword="true"/> when an amount was found; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string? text, out PriceResult result) => TryParse(text, null, null, null, out result);

    /// <summary>Parses a price without throwing, with the full set of options.</summary>
    /// <param name="text">Text that may contain a price.</param>
    /// <param name="currencyHint">Optional text that may hold the currency.</param>
    /// <param name="decimalSeparator">Forces the decimal separator instead of guessing it.</param>
    /// <param name="digitGroupSeparator">Removes this digit group separator before parsing.</param>
    /// <param name="result">The parsed result; its fields are <see langword="null"/> when nothing was found.</param>
    /// <returns><see langword="true"/> when an amount was found; otherwise <see langword="false"/>.</returns>
    public static bool TryParse(string? text, string? currencyHint, char? decimalSeparator, char? digitGroupSeparator, out PriceResult result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        try
        {
            result = Extract(text, currencyHint, decimalSeparator, digitGroupSeparator);
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }

        return result.Amount.HasValue;
    }

    private static PriceResult Extract(string text, string? currencyHint, char? decimalSeparator, char? digitGroupSeparator)
    {
        string? currency = CurrencyFinder.Find(text, currencyHint)?.Trim();
        if (digitGroupSeparator is { } group)
        {
            text = text.Replace(group.ToString(), string.Empty, StringComparison.Ordinal);
        }

        string? amountText = AmountText.Extract(text);
        decimal? amount = amountText is null ? null : NumberParser.Parse(amountText, decimalSeparator);
        return new PriceResult(amount, currency, amountText);
    }
}
