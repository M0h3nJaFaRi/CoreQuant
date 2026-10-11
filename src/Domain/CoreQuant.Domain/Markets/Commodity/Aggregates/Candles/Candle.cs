using Common.Domain.Core;
using Common.Domain.Guards;
using CoreQuant.Domain.Markets.Commodity.Aggregates.Candles.Enums;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Candles;

/// <summary>
/// Closed OHLC candle bar enriched with optional tick snapshot fields.
/// </summary>
public sealed class Candle : AggregateRoot<ulong>
{
    /// <summary>
    /// EF Core materialization constructor.
    /// </summary>
    private Candle() { }

    /// <summary>
    /// FK to <see cref="Symbols.Symbol"/>.
    /// </summary>
    public ulong SymbolId { get; private set; }

    /// <summary>
    /// FK to timeframe lookup.
    /// </summary>
    public CandleTimeframeCode TimeframeId { get; private set; }

    /// <summary>
    /// Bar open time (UTC).
    /// </summary>
    public DateTimeOffset OpenTime { get; private set; }

    /// <summary>
    /// Bar close time (UTC).
    /// </summary>
    public DateTimeOffset CloseTime { get; private set; }

    /// <summary>
    /// Open price.
    /// </summary>
    public decimal Open { get; private set; }

    /// <summary>
    /// High price.
    /// </summary>
    public decimal High { get; private set; }

    /// <summary>
    /// Low price.
    /// </summary>
    public decimal Low { get; private set; }

    /// <summary>
    /// Close price.
    /// </summary>
    public decimal Close { get; private set; }

    /// <summary>
    /// Tick/print count in the bar.
    /// </summary>
    public ulong TickVolume { get; private set; }

    /// <summary>
    /// Real/exchange volume when available.
    /// </summary>
    public decimal RealVolume { get; private set; }

    /// <summary>
    /// Spread in points at bar close.
    /// </summary>
    public uint SpreadPoints { get; private set; }

    /// <summary>
    /// Bid at enrichment tick.
    /// </summary>
    public decimal? CloseBid { get; private set; }

    /// <summary>
    /// Ask at enrichment tick.
    /// </summary>
    public decimal? CloseAsk { get; private set; }

    /// <summary>
    /// Last at enrichment tick.
    /// </summary>
    public decimal? CloseLast { get; private set; }

    /// <summary>
    /// Flags from enrichment tick.
    /// </summary>
    public uint? TickFlags { get; private set; }

    /// <summary>
    /// Timestamp of enrichment tick.
    /// </summary>
    public DateTimeOffset? TickTs { get; private set; }

    /// <summary>
    /// Candle origin lookup id.
    /// </summary>
    public CandleSourceCode SourceId { get; private set; }

    /// <summary>
    /// Records a closed OHLC bar.
    /// </summary>
    public static Candle RecordClosed(
        ulong symbolId,
        CandleTimeframeCode timeframeId,
        DateTimeOffset openTime,
        DateTimeOffset closeTime,
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        ulong tickVolume,
        decimal realVolume,
        uint spreadPoints,
        CandleSourceCode sourceId,
        decimal? closeBid = null,
        decimal? closeAsk = null,
        decimal? closeLast = null,
        uint? tickFlags = null,
        DateTimeOffset? tickTs = null)
    {
        Ensure.That(symbolId > 0, "Symbol id is required.", "candle_symbol_required");
        Ensure.That(closeTime >= openTime, "Close time must be on or after open time.", "candle_time_range_invalid");
        Ensure.That(high >= open && high >= close && high >= low, "High must cover the bar range.", "candle_high_invalid");
        Ensure.That(low <= open && low <= close && low <= high, "Low must cover the bar range.", "candle_low_invalid");
        Ensure.That(tickVolume >= 0, "Tick volume cannot be negative.", "candle_tick_volume_invalid");
        Ensure.That(realVolume >= 0, "Real volume cannot be negative.", "candle_real_volume_invalid");

        var candle = new Candle
        {
            SymbolId = symbolId,
            TimeframeId = timeframeId,
            OpenTime = openTime,
            CloseTime = closeTime,
            Open = open,
            High = high,
            Low = low,
            Close = close,
            TickVolume = tickVolume,
            RealVolume = realVolume,
            SpreadPoints = spreadPoints,
            CloseBid = closeBid,
            CloseAsk = closeAsk,
            CloseLast = closeLast,
            TickFlags = tickFlags,
            TickTs = tickTs,
            SourceId = sourceId
        };

        candle.StampCreated();

        return candle;
    }
}
