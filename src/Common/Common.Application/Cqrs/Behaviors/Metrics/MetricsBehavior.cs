using System.Diagnostics;
using Common.Application.Cqrs.Pipeline;

namespace Common.Application.Cqrs.Behaviors.Metrics;

/// <summary>
/// Records low-cardinality CQRS metrics.
/// </summary>
internal sealed class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly CqrsMetrics _metrics;

    /// <summary>
    /// Creates the behavior.
    /// </summary>
    public MetricsBehavior(CqrsMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        _metrics = metrics;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Metrics;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestType = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();
        try
        {
            var response = await next().ConfigureAwait(false);
            Record(requestType, "success", started);
            return response;
        }
        catch
        {
            Record(requestType, "failed", started);
            _metrics.Failed.Add(1, new KeyValuePair<string, object?>("request_type", requestType));
            throw;
        }
    }

    private void Record(string requestType, string status, long started)
    {
        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        _metrics.Total.Add(
            1,
            new KeyValuePair<string, object?>("request_type", requestType),
            new KeyValuePair<string, object?>("status", status));
        _metrics.DurationMs.Record(
            elapsedMs,
            new KeyValuePair<string, object?>("request_type", requestType),
            new KeyValuePair<string, object?>("status", status));
    }
}
