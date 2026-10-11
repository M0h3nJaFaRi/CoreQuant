using System.ComponentModel;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Candles.Enums;

/// <summary>
/// Candle timeframe codes aligned with lookup rows and wire contract.
/// </summary>
public enum CandleTimeframeCode : byte
{
    [Description("یک دقیقه")]
    M1 = 1,

    [Description("پنج دقیقه")]
    M5 = 2,

    [Description("پانزده دقیقه")]
    M15 = 3,

    [Description("سی دقیقه")]
    M30 = 4,

    [Description("یک ساعت")]
    H1 = 5,

    [Description("چهار ساعت")]
    H4 = 6,

    [Description("یک روز")]
    D1 = 7,

    [Description("یک هفته")]
    W1 = 8,

    [Description("یک ماه")]
    MN1 = 9,
}