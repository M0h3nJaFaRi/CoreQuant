using CoreQuant.Domain.Markets.Commodity.Aggregates.Candles.Enums;

namespace CoreQuant.Domain.Markets.Commodity.Aggregates.Candles.Extensions;

/// <summary>
/// Parsing and metadata helpers for <see cref="CandleTimeframeCode"/>.
/// </summary>
public static class CandleTimeframeCodeExtensions
{
    /// <summary>
    /// Returns the wire code (M1, H1, …).
    /// </summary>
    public static string ToCode(this CandleTimeframeCode timeframe) => timeframe switch
    {
        CandleTimeframeCode.M1 => "M1",
        CandleTimeframeCode.M5 => "M5",
        CandleTimeframeCode.M15 => "M15",
        CandleTimeframeCode.M30 => "M30",
        CandleTimeframeCode.H1 => "H1",
        CandleTimeframeCode.H4 => "H4",
        CandleTimeframeCode.D1 => "D1",
        CandleTimeframeCode.W1 => "W1",
        CandleTimeframeCode.MN1 => "MN1",
        _ => throw new ArgumentOutOfRangeException(nameof(timeframe), timeframe, null),
    };

    /// <summary>
    /// Approximate bar length in seconds.
    /// </summary>
    public static int ToApproximateSeconds(this CandleTimeframeCode timeframe) => timeframe switch
    {
        CandleTimeframeCode.M1 => 60,
        CandleTimeframeCode.M5 => 300,
        CandleTimeframeCode.M15 => 900,
        CandleTimeframeCode.M30 => 1_800,
        CandleTimeframeCode.H1 => 3_600,
        CandleTimeframeCode.H4 => 14_400,
        CandleTimeframeCode.D1 => 86_400,
        CandleTimeframeCode.W1 => 604_800,
        CandleTimeframeCode.MN1 => 2_592_000,
        _ => 60,
    };

    /// <summary>
    /// Parses a wire timeframe code.
    /// </summary>
    public static bool TryParse(string? value, out CandleTimeframeCode timeframe)
    {
        timeframe = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "M1" => Assign(CandleTimeframeCode.M1, out timeframe),
            "M5" => Assign(CandleTimeframeCode.M5, out timeframe),
            "M15" => Assign(CandleTimeframeCode.M15, out timeframe),
            "M30" => Assign(CandleTimeframeCode.M30, out timeframe),
            "H1" => Assign(CandleTimeframeCode.H1, out timeframe),
            "H4" => Assign(CandleTimeframeCode.H4, out timeframe),
            "D1" => Assign(CandleTimeframeCode.D1, out timeframe),
            "W1" => Assign(CandleTimeframeCode.W1, out timeframe),
            "MN1" => Assign(CandleTimeframeCode.MN1, out timeframe),
            _ => false,
        };
    }

    private static bool Assign(CandleTimeframeCode value, out CandleTimeframeCode timeframe)
    {
        timeframe = value;
        return true;
    }
}
