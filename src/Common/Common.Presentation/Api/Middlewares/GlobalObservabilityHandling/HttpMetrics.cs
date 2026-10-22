using Common.Presentation.Contract;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Common.Presentation.Api.Middlewares.GlobalObservabilityHandling;
public sealed class HttpMetrics(Meter meter, ServiceIdentity identity)
{
    // Aligned with the request-timeout policies (3s / 10s / 30s / 60s) so timeouts are visible as bucket edges.
    private static readonly double[] DurationBuckets = [0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2, 3, 5, 10, 30, 60];
    private static readonly FrozenSet<string> KnownMethods = new[] { "GET", "HEAD", "POST", "PUT", "DELETE", "CONNECT", "OPTIONS", "TRACE", "PATCH" }.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    private readonly Histogram<double> _requestDuration = meter.CreateHistogram(name: $"{identity.Name}_http_server_request_duration_seconds",
                                                                                unit: "s",
                                                                                description: "Duration of HTTP server requests.",
                                                                                advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = DurationBuckets });
    private readonly UpDownCounter<long> _activeRequests = meter.CreateUpDownCounter<long>(name: $"{identity.Name}_http_server_active_requests",
                                                                                           unit: "{request}",
                                                                                           description: "Number of HTTP requests currently in flight.");
    private readonly Counter<long> _exceptions = meter.CreateCounter<long>(name: $"{identity.Name}_http_server_exceptions",
                                                                           unit: "{exception}",
                                                                           description: "Exceptions raised while handling HTTP requests (client aborts excluded).");



    public void RequestFinished(string method) => _activeRequests.Add(-1, new KeyValuePair<string, object?>("method", method));
    public void RequestStarted(string method) => _activeRequests.Add(1, new KeyValuePair<string, object?>("method", method));
    public static string NormalizeMethod(string method) => KnownMethods.TryGetValue(method, out var known) ? known : "_OTHER";

    public void RecordRequest(string method, string route, int statusCode, double durationSeconds)
    {
        var tags = new TagList
        {
            { "method", method },
            { "route", route },
            { "status_code", statusCode }
        };

        _requestDuration.Record(durationSeconds, tags);
    }
    public void RecordException(string method, string route, string errorType, string errorKind)
    {
        var tags = new TagList
        {
            { "method", method },
            { "route", route },
            { "error_type", errorType },
            { "error_kind", errorKind }
        };

        _exceptions.Add(1, tags);
    }
}