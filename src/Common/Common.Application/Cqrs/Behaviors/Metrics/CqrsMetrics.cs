using Common.Presentation.Contract;
using System.Diagnostics;
using System.Diagnostics.Metrics;

/// <summary>
/// Shared CQRS meter instruments (created once per process).
/// </summary>
internal sealed class CqrsMetrics(Meter meter, ServiceIdentity identity)
{
    /// <summary>Total requests, tagged by component / request_type / kind / outcome.</summary>
    private Counter<long> Requests { get; } = meter.CreateCounter<long>(name: $"{identity.Name}_cqrs_requests_total",
                                                                        description: "CQRS requests by outcome");

    /// <summary>Request duration in milliseconds.</summary>
    private Histogram<double> DurationMs { get; } = meter.CreateHistogram<double>(name: $"{identity.Name}_cqrs_requests_duration_ms",
                                                                                  unit: "ms",
                                                                                  description: "CQRS request duration");

    /// <summary>Currently executing requests.</summary>
    private UpDownCounter<long> InFlight { get; } = meter.CreateUpDownCounter<long>(name: $"{identity.Name}_cqrs_requests_in_flight", 
                                                                                    description: "Currently executing CQRS requests");

    /// <summary>Number of validation errors per failed request.</summary>
    private Histogram<int> ValidationErrorCount { get; } = meter.CreateHistogram<int>(name: $"{identity.Name}_cqrs_validation_errors", 
                                                                                      description: "Number of validation errors per failed request");

    /// <summary>
    /// Increments the in-flight gauge for the given request. Must be paired with a matching
    /// <see cref="DecrementInFlight"/> call, typically in a <c>finally</c> block.
    /// </summary>
    /// <param name="requestType">The CLR type name of the request (e.g. <c>typeof(TRequest).Name</c>).</param>
    /// <param name="kind">The request kind, e.g. "command" or "query".</param>
    public void IncrementInFlight(string requestType, string kind) => InFlight.Add(1, BuildKindTags(requestType, kind));

    /// <summary>
    /// Decrements the in-flight gauge for the given request. Should mirror a prior
    /// <see cref="IncrementInFlight"/> call for the same request.
    /// </summary>
    /// <param name="requestType">The CLR type name of the request (e.g. <c>typeof(TRequest).Name</c>).</param>
    /// <param name="kind">The request kind, e.g. "command" or "query".</param>
    public void DecrementInFlight(string requestType, string kind) => InFlight.Add(-1, BuildKindTags(requestType, kind));

    /// <summary>
    /// Records a successfully completed request, including its total duration.
    /// </summary>
    /// <param name="requestType">The CLR type name of the request.</param>
    /// <param name="kind">The request kind, e.g. "command" or "query".</param>
    /// <param name="started">The <see cref="Stopwatch.GetTimestamp"/> value captured when the request began.</param>
    /// <param name="servedFrom">
    /// Optional source that short-circuited the pipeline (e.g. "cache" or "idempotency").
    /// When set, the recorded outcome becomes <c>success_{servedFrom}</c> instead of plain
    /// <c>success</c>, so cache/idempotency hits can be distinguished from a full handler execution.
    /// </param>
    public void RecordSuccess(string requestType, string kind, long started, string? servedFrom = null)
    {
        var outcome = servedFrom is null ? "success" : $"success_{servedFrom}";
        var tags = BuildTags(requestType, kind, outcome);
        Requests.Add(1, tags);
        DurationMs.Record(ElapsedMs(started), tags);
    }

    /// <summary>
    /// Records a failed request, including its total duration.
    /// </summary>
    /// <param name="requestType">The CLR type name of the request.</param>
    /// <param name="kind">The request kind, e.g. "command" or "query".</param>
    /// <param name="outcome">
    /// A small, bounded outcome label (e.g. "forbidden", "rate_limited", "validation",
    /// "cancelled", "unhandled"). Must not be a free-form string, to keep tag cardinality safe.
    /// </param>
    /// <param name="started">The <see cref="Stopwatch.GetTimestamp"/> value captured when the request began.</param>
    /// <param name="exceptionType">
    /// Optional <c>exception.GetType().Name</c>, intended only for the catch-all/unhandled path
    /// where the outcome isn't already categorized.
    /// </param>
    /// <param name="reason">
    /// Optional bounded reason code (e.g. an authorization denial reason) providing more context
    /// for the outcome without using a free-form string.
    /// </param>
    public void RecordFailure(string requestType, 
                              string kind, 
                              string outcome, 
                              long started, 
                              string? exceptionType = null, 
                              string? reason = null)
    {
        var tags = BuildFailureTags(requestType, kind, outcome, exceptionType, reason);
        Requests.Add(1, tags);
        DurationMs.Record(ElapsedMs(started), tags);
    }

    /// <summary>
    /// Records a validation failure and the number of individual validation errors it contained.
    /// </summary>
    /// <param name="requestType">The CLR type name of the request.</param>
    /// <param name="kind">The request kind, e.g. "command" or "query".</param>
    /// <param name="started">The <see cref="Stopwatch.GetTimestamp"/> value captured when the request began.</param>
    /// <param name="errorCount">The number of validation failures that caused this request to be rejected.</param>
    public void RecordValidationFailure(string requestType, 
                                        string kind, 
                                        long started, 
                                        int errorCount)
    {
        RecordFailure(requestType, kind, "validation", started);
        ValidationErrorCount.Record(errorCount, BuildKindTags(requestType, kind));
    }

    /// <summary>
    /// Computes elapsed milliseconds since <paramref name="started"/>.
    /// </summary>
    /// <param name="started">
    /// The <see cref="Stopwatch.GetTimestamp"/> value captured when the request began.
    /// </param>
    private static double ElapsedMs(long started) => Stopwatch.GetElapsedTime(started).TotalMilliseconds;

    /// <summary>
    /// Builds the tag set shared by the in-flight gauge and the validation error histogram.
    /// </summary>
    private KeyValuePair<string, object?>[] BuildKindTags(string requestType, string kind) =>
    [
        new("component", identity.Component),
        new("request_type", requestType),
        new("kind", kind)
    ];

    /// <summary>
    /// Builds the base tag set (component, request type, kind, outcome) used for request/duration instruments.
    /// </summary>
    private KeyValuePair<string, object?>[] BuildTags(string requestType, string kind, string outcome) =>
    [
        new("component", identity.Component),
        new("request_type", requestType),
        new("kind", kind),
        new("outcome", outcome)
    ];

    /// <summary>
    /// Builds the tag set for a failed request, optionally including the exception type and/or
    /// reason tags when they are provided.
    /// </summary>
    private KeyValuePair<string, object?>[] BuildFailureTags(string requestType, 
                                                             string kind, 
                                                             string outcome, 
                                                             string? exceptionType, 
                                                             string? reason)
    {
        var tags = new List<KeyValuePair<string, object?>>
        {
            new("component", identity.Component),
            new("request_type", requestType),
            new("kind", kind),
            new("outcome", outcome)
        };

        if (exceptionType is not null)
            tags.Add(new("exception_type", exceptionType));

        if (reason is not null)
            tags.Add(new("reason", reason));

        return [.. tags];
    }
}