using Common.Application.Authorization.Abstraction;
using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Common.Application.Cqrs.Behaviors.Logging;

/// <summary>
/// Owns all CQRS request logging: lifecycle (start/success/failure), outcome severity
/// mapping, and slow-request warnings. No other behavior in the pipeline should call
/// <see cref="ILogger"/> directly — this keeps logging decisions in one place.
/// </summary>
internal sealed class LoggingBehavior<TRequest, TResponse>(ICurrentUserAccessor currentUser,
                                                           IOptions<PerformanceBehaviorOptions> perfOptions,
                                                           ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.Logging;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        var requestType = typeof(TRequest).Name;
        var traceId = Activity.Current?.TraceId.ToString() ?? "none";
        var spanId = Activity.Current?.SpanId.ToString() ?? Guid.NewGuid().ToString("N");

        logger.LogInformation("CQRS start {RequestType} TraceId={TraceId} SpanId={SpanId} UserId={UserId}",
                              requestType, traceId, spanId, userId);

        var started = Stopwatch.GetTimestamp();
        try
        {
            var response = await next().ConfigureAwait(false);
            LogSuccess(requestType, traceId, spanId, started);
            return response;
        }
        catch (Exception ex)
        {
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            LogFailure(ex, requestType, traceId, spanId, elapsedMs, cancellationToken);
            throw;
        }
    }

    private void LogSuccess(string requestType, string traceId, string spanId, long started)
    {
        var elapsed = Stopwatch.GetElapsedTime(started);

        if (elapsed > perfOptions.Value.SlowRequestThreshold)
            logger.LogWarning("CQRS success but slow {RequestType} TraceId={TraceId} SpanId={SpanId} DurationMs={DurationMs} ThresholdMs={ThresholdMs}",
                               requestType, traceId, spanId, elapsed.TotalMilliseconds, perfOptions.Value.SlowRequestThreshold.TotalMilliseconds);
        else
            logger.LogInformation("CQRS success {RequestType} TraceId={TraceId} SpanId={SpanId} DurationMs={DurationMs}",
                                  requestType, traceId, spanId, elapsed.TotalMilliseconds);
    }
    private void LogFailure(Exception ex, string requestType, string traceId, string spanId, double elapsedMs, CancellationToken cancellationToken)
    {
        switch (ex)
        {
            case OperationCanceledException when cancellationToken.IsCancellationRequested:
                logger.LogInformation("CQRS cancelled {RequestType} TraceId={TraceId} SpanId={SpanId} DurationMs={DurationMs}",
                                       requestType, traceId, spanId, elapsedMs);
                break;

            case ForbiddenException or RateLimitExceededException or ValidationException:
                logger.LogWarning(ex, "CQRS rejected {RequestType} TraceId={TraceId} SpanId={SpanId} DurationMs={DurationMs} Reason={ExceptionType}",
                                  requestType, traceId, spanId, elapsedMs, ex.GetType().Name);
                break;

            default:
                logger.LogError(ex, "CQRS failure {RequestType} TraceId={TraceId} SpanId={SpanId} DurationMs={DurationMs}",
                                requestType, traceId, spanId, elapsedMs);
                break;
        }
    }
}