using Microsoft.AspNetCore.Http;
using System.Diagnostics.Metrics;

namespace Framework.Web.Middleware;

/// <summary>
/// Records basic HTTP request metrics (count and duration).
/// </summary>
public sealed class HttpMetricsMiddleware(RequestDelegate next, Meter meter)
{
    private readonly Counter<long> _requestCounter = meter.CreateCounter<long>(
        "http.server.requests",
        description: "HTTP requests processed");

    private readonly Histogram<double> _requestDurationMs = meter.CreateHistogram<double>(
        "http.server.request.duration",
        unit: "ms",
        description: "HTTP request duration in milliseconds");

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var start = TimeProvider.System.GetTimestamp();
        try
        {
            await next(context).ConfigureAwait(false);
        }
        finally
        {
            var elapsedMs = TimeProvider.System.GetElapsedTime(start).TotalMilliseconds;
            var status = context.Response.StatusCode;
            var method = context.Request.Method;

            _requestCounter.Add(1, new KeyValuePair<string, object?>("http.method", method),
                                   new KeyValuePair<string, object?>("http.status_code", status));

            _requestDurationMs.Record(elapsedMs, new KeyValuePair<string, object?>("http.method", method),
                                                 new KeyValuePair<string, object?>("http.status_code", status));
        }
    }
}
