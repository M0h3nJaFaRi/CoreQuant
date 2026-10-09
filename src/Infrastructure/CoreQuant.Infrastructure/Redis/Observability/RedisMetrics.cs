using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CoreQuant.Infrastructure.Redis.Observability;

/// <summary>
/// Redis instruments on the shared application <see cref="Meter"/>.
/// </summary>
internal sealed class RedisMetrics
{
    public RedisMetrics(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);

        OperationsTotal = meter.CreateCounter<long>("redis.operations.total", description: "Redis total operations");
        OperationsSuccess = meter.CreateCounter<long>("redis.operations.total", description: "Redis operations completed successfully");
        OperationsFailed = meter.CreateCounter<long>("redis.operations.failed", description: "Redis operations that failed");
        OperationDurationMs = meter.CreateHistogram<double>("redis.operation.duration", unit: "ms", description: "Redis operation duration");
        ConnectionFailed = meter.CreateCounter<long>("redis.connection.failed", description: "Redis connection failures reported by the multiplexer");
        ConnectionRestored = meter.CreateCounter<long>("redis.connection.restored", description: "Redis connection restores reported by the multiplexer");
    }

    public Counter<long> OperationsTotal { get; }
    public Counter<long> OperationsFailed { get; }
    public Counter<long> OperationsSuccess { get; }
    public Counter<long> ConnectionFailed { get; }
    public Counter<long> ConnectionRestored { get; }
    public Histogram<double> OperationDurationMs { get; }

    public void RecordSuccess(string operation, uint dbIndex, string feature, double durationMs, string? script = null)
    {
        var tags = BuildTags(operation, dbIndex, feature, script);
        
        OperationsTotal.Add(1, tags);
        OperationsSuccess.Add(1, tags);
        OperationDurationMs.Record(durationMs, tags);
    }

    public void RecordFailure(string operation, uint dbIndex, string feature, double durationMs, string? script = null)
    {
        var tags = BuildTags(operation, dbIndex, feature, script);
        
        OperationsTotal.Add(1, tags);
        OperationsFailed.Add(1, tags);
        OperationDurationMs.Record(durationMs, tags);
    }

    public void RecordConnectionFailed(string? endpoint) => ConnectionFailed.Add(1, new KeyValuePair<string, object?>("endpoint", endpoint ?? "unknown"));

    public void RecordConnectionRestored(string? endpoint) => ConnectionRestored.Add(1, new KeyValuePair<string, object?>("endpoint", endpoint ?? "unknown"));

    private static TagList BuildTags(string operation, uint dbIndex, string feature, string? script)
    {
        var tags = new TagList
        {
            { "operation", operation },
            { "db", dbIndex },
            { "feature", feature }
        };

        if (!string.IsNullOrWhiteSpace(script))
            tags.Add("script", script);

        return tags;
    }
}