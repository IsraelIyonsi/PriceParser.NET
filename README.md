# PriceParser.Net

Zero-dependency price extraction for .NET. Give it free text, get back a `decimal` amount and the currency token exactly as it was written.

[![NuGet](https://img.shields.io/nuget/v/PriceParser.Net.svg)](https://www.nuget.org/packages/PriceParser.Net) &nbsp; MIT &nbsp; Zero dependencies &nbsp; Native AOT clean

## Inspiration

This is a .NET port of the behavior of [`price-parser`](https://github.com/scrapinghub/price-parser) by Scrapinghub (Zyte). The expected values in this package's test suite are taken from that project's own test corpus (about a thousand text-to-price cases) and every case passes, so the heuristics match the reference implementation.

## Install

```
dotnet add package PriceParser.Net
```

## Use

```csharp
using PriceParsing;

PriceResult r = PriceParser.Parse("Now only $1,299.00!");
// r.Amount     == 1299.00m
// r.Currency   == "$"
// r.AmountText == "1,299.00"

if (PriceParser.TryParse("EUR 1.234,56", out var eu))
{
    // eu.Amount == 1234.56m, eu.Currency == "EUR"
}
```

`Parse` throws `ArgumentNullException` for `null` and otherwise returns a result whose `Amount` is `null` when the text holds no price. `TryParse` never throws and returns `true` only when an amount was found.

Optional arguments: `currencyHint` (text from a neighbouring element that may hold the currency), `decimalSeparator` (force `.`, `,` or the euro sign), and `digitGroupSeparator`.

## Design: the raw currency token, no ISO resolution

`Currency` is the token exactly as found in the text: `$`, `US$`, `A$`, `EUR`, a symbol like the rupee or pound sign, or an Arabic or Cyrillic abbreviation. It is never mapped to an ISO 4217 code. `$` stays `$` because it could be USD, CAD, AUD, MXN and more, and guessing silently corrupts money. Resolve it yourself with the context you have (site locale, store settings).

## Heuristics

- `1,234.56` and `1.234,56`: the decimal separator is the last separator followed by one or two digits (or four or more); a separator followed by exactly three digits is a thousands separator. So `1.000` is `1000`, `1,000` is `1000`, `10,99` is `10.99`, `9.99` is `9.99`.
- Space, apostrophe and non-breaking-space groups: `1 234,56`, `CHF 1'049,95`.
- Ranges (`$10-$20`, `$10 - 20`) return the first value.
- Trailing notation such as `1,00 .-` and `1234,--` carries no fraction beyond what is written.
- Bare numbers return an amount with a `null` currency, as the reference does. That includes numbers glued to words (`iPhone 11` gives `11`); if you scrape text where that matters, require a currency.
- `Free` returns `0`.

## Notes and limitations

- Amounts that need more than the 28-29 significant digits of `decimal` return `null` (Python's `Decimal` is arbitrary precision).
- Only the first price-looking substring in the text is parsed.
- Regex matching has a 2 second timeout; `Parse` surfaces `RegexMatchTimeoutException` and `TryParse` returns `false`.

## License

MIT. Copyright Israel Iyonsi.
