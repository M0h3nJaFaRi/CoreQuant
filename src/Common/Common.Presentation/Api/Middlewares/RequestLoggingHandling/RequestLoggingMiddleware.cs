using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Common.Presentation.Api.Extensions;
using Common.Presentation.Api.Configuration.Options;

namespace Common.Presentation.Api.Middlewares.RequestLoggingHandling;

/// <summary>
/// One summary line per request (no request/response bodies). Must sit OUTSIDE ExceptionHandlingMiddleware
/// so the status code it reads is the final one. CorrelationId comes from the log scope that
/// CorrelationIdMiddleware opens, so it is not repeated as a parameter.
/// </summary>
public sealed partial class RequestLoggingMiddleware(ILogger<RequestLoggingMiddleware> logger,
                                                     IOptionsMonitor<RequestLoggingOptions> optionsMonitor) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Exception? escaped = null;
        var startTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            escaped = ex;
            throw;
        }
        finally
        {
            LogRequest(context, startTimestamp, escaped);
        }
    }

    private void LogRequest(HttpContext context, long startTimestamp, Exception? escaped)
    {
        var statusCode = escaped is null ?
                         context.Response.StatusCode : escaped is OperationCanceledException && context.RequestAborted.IsCancellationRequested ?
                                                       StatusCodes.Status499ClientClosedRequest : StatusCodes.Status500InternalServerError;

        var path = context.Request.Path;
        var level = statusCode < StatusCodes.Status400BadRequest && PathMatcher.IsExcluded(path, optionsMonitor.CurrentValue.QuietPaths) ? LogLevel.Debug : GetLevel(statusCode);

        if (!logger.IsEnabled(level)) return;

        var elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        logger.Log(level, "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms. TraceId: {TraceId}",
                   context.Request.Method, path.ToUriComponent(), statusCode, elapsedMs, traceId);
    }
    private static LogLevel GetLevel(int statusCode) => statusCode switch
    {
        StatusCodes.Status499ClientClosedRequest => LogLevel.Information,
        StatusCodes.Status503ServiceUnavailable => LogLevel.Warning,
        >= 500 => LogLevel.Error,
        _ => LogLevel.Information
    };
}