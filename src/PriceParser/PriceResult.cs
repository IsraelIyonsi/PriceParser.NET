namespace PriceParsing;

/// <summary>The outcome of parsing a price out of free text.</summary>
/// <param name="Amount">The numeric value, or <see langword="null"/> when no amount was found.</param>
/// <param name="Currency">The currency token exactly as it appeared in the text (for example <c>$</c> or <c>EUR</c>), never resolved to an ISO code; <see langword="null"/> when none was found.</param>
/// <param name="AmountText">The raw substring the amount was parsed from, or <see langword="null"/> when no amount was found.</param>
public readonly record struct PriceResult(decimal? Amount, string? Currency, string? AmountText);
