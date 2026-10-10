using System.Globalization;

namespace PriceParsing.Tests;

public sealed class SpecTests
{
    private static decimal? D(string? s) => s is null ? null : decimal.Parse(s, NumberStyles.Number, CultureInfo.InvariantCulture);

    [Theory]
    [InlineData("$10", "10", "$", "10")]
    [InlineData("\u20AC10,99", "10.99", "\u20AC", "10,99")]
    [InlineData("\u00A31,234.56", "1234.56", "\u00A3", "1,234.56")]
    [InlineData("\u20B9 1,00,000", "100000", "\u20B9", "1,00,000")]
    [InlineData("1,234.56 USD", "1234.56", "USD", "1,234.56")]
    [InlineData("EUR 1.234,56", "1234.56", "EUR", "1.234,56")]
    [InlineData("1 234,56 EUR", "1234.56", "EUR", "1 234,56")]
    [InlineData("1234", "1234", null, "1234")]
    [InlineData("10,99", "10.99", null, "10,99")]
    [InlineData("1,000", "1000", null, "1,000")]
    [InlineData("9.99", "9.99", null, "9.99")]
    [InlineData("1.000", "1000", null, "1.000")]
    [InlineData("$10-$20", "10", "$", "10")]
    [InlineData("$10 - 20", "10", "$", "10")]
    [InlineData("1,00 .-", "1.00", null, "1,00")]
    [InlineData("1234,--", "1234", null, "1234")]
    [InlineData("iPhone 11", "11", null, "11")]
    [InlineData("Model 3", "3", null, "3")]
    [InlineData("Free", "0", null, "0")]
    [InlineData("US$:12.99", "12.99", "US$", "12.99")]
    [InlineData("NZD $123", "123", "NZD", "123")]
    public void Parse_Table(string text, string? amount, string? currency, string? amountText)
    {
        PriceResult r = PriceParser.Parse(text);
        Assert.Equal(D(amount), r.Amount);
        Assert.Equal(currency, r.Currency);
        Assert.Equal(amountText, r.AmountText);
    }

    [Theory]
    [InlineData("$")]
    [InlineData("USD")]
    [InlineData("no price here")]
    [InlineData("50% OFF")]
    [InlineData("")]
    [InlineData("   ")]
    public void NoPrice_AmountIsNull(string text)
    {
        Assert.Null(PriceParser.Parse(text).Amount);
        Assert.False(PriceParser.TryParse(text, out PriceResult r));
        Assert.Null(r.Amount);
    }

    [Fact]
    public void TryParse_Null_DoesNotThrow()
    {
        Assert.False(PriceParser.TryParse(null, out PriceResult r));
        Assert.Equal(default, r);
    }

    [Fact]
    public void Parse_Null_Throws() => Assert.Throws<ArgumentNullException>(() => PriceParser.Parse(null!));

    [Theory]
    [InlineData("$10", "10", "$")]
    [InlineData("Price: 7.50 GBP", "7.50", "GBP")]
    public void TryParse_ReturnsTrueWithValues(string text, string amount, string currency)
    {
        Assert.True(PriceParser.TryParse(text, out PriceResult r));
        Assert.Equal(D(amount), r.Amount);
        Assert.Equal(currency, r.Currency);
    }

    [Theory]
    [InlineData("$", "$")]
    [InlineData("US$", "US$")]
    [InlineData("A$", "A$")]
    [InlineData("USD", "USD")]
    public void Currency_IsRawToken_NeverResolved(string symbol, string expected)
    {
        Assert.Equal(expected, PriceParser.Parse(symbol + "5").Currency);
        Assert.NotEqual("USD", PriceParser.Parse("$5").Currency);
    }

    [Theory]
    [InlineData("$10", "10", "$11", "11")]
    [InlineData("1.000", "1000", "1.00", "1.00")]
    [InlineData("1.000", "1000", "1.0000", "1.0000")]
    [InlineData("1,234.56", "1234.56", "1,234.57", "1234.57")]
    [InlineData("EUR 5", "5", "GBP 5", "5")]
    public void Mutation_ChangesResult(string original, string originalAmount, string mutated, string mutatedAmount)
    {
        Assert.Equal(D(originalAmount), PriceParser.Parse(original).Amount);
        Assert.Equal(D(mutatedAmount), PriceParser.Parse(mutated).Amount);
        Assert.NotEqual(PriceParser.Parse(original), PriceParser.Parse(mutated));
    }

    [Theory]
    [InlineData("140.000", '.', "140.000")]
    [InlineData("140.000", ',', "140000")]
    [InlineData("1,234", '.', "1234")]
    public void ForcedDecimalSeparator(string text, char separator, string amount) =>
        Assert.Equal(D(amount), PriceParser.Parse(text, null, separator).Amount);

    [Theory]
    [InlineData("1.234", '.', "1234")]
    [InlineData("1,234", ',', "1234")]
    public void DigitGroupSeparator_IsRemoved(string text, char group, string amount) =>
        Assert.Equal(D(amount), PriceParser.Parse(text, null, null, group).Amount);

    [Fact]
    public void CurrencyHint_UsedWhenTextHasNone()
    {
        PriceResult r = PriceParser.Parse("12.50", "GBP");
        Assert.Equal(D("12.50"), r.Amount);
        Assert.Equal("GBP", r.Currency);
    }
}
