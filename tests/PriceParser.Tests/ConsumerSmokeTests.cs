namespace PriceParser.Tests.ConsumerSmoke;

using PriceParsing;

public sealed class ConsumerSmokeTests
{
    [Fact]
    public void EntryClass_IsReachableFromNamespaceNamedLikeTheAssembly()
    {
        var r = PriceParser.Parse("$10");
        Assert.Equal(10m, r.Amount);
        Assert.Equal("$", r.Currency);
        Assert.Equal("10", r.AmountText);
    }
}
