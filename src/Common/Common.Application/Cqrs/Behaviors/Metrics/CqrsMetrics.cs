using System.Diagnostics.Metrics;

namespace Common.Application.Cqrs.Behaviors.Metrics;

/// <summary>
/// Shared CQRS meter instruments (created once per process).
/// </summary>
internal sealed class CqrsMetrics
{
    /// <summary>
    /// Creates instruments on <paramref name="meter"/>.
    /// </summary>
    public CqrsMetrics(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);
        Total = meter.CreateCounter<long>("cqrs.requests.total", description: "CQRS requests");
        Failed = meter.CreateCounter<long>("cqrs.requests.failed", description: "Failed CQRS requests");
        DurationMs = meter.CreateHistogram<double>(
            "cqrs.requests.duration",
            unit: "ms",
            description: "CQRS request duration");
    }

    /// <summary>Total requests.</summary>
    public Counter<long> Total { get; }

    /// <summary>Failed requests.</summary>
    public Counter<long> Failed { get; }

    /// <summary>Request duration in milliseconds.</summary>
    public Histogram<double> DurationMs { get; }
}
