using System.ComponentModel;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Candles.Enums;

/// <summary>
/// Origin of a persisted candle row.
/// </summary>
public enum CandleSourceCode : byte
{
    [Description("نرخ MT5")]
    Mt5Rates = 1,

    [Description("مشتق از تیک")]
    DerivedTicks = 2,
}
