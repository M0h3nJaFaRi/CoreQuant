using System.Diagnostics;
using Common.Application.Authorization;
using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.Logging;

namespace Common.Application.Cqrs.Behaviors.Logging;

/// <summary>
/// Logs CQRS request lifecycle without dumping the full request payload.
/// </summary>
internal sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserAccessor _currentUser;

    /// <summary>
    /// Creates the behavior.
    /// </summary>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserAccessor currentUser)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(currentUser);
        _logger = logger;
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Logging;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var traceId = Activity.Current?.TraceId.ToString() ?? "none";
        var requestId = Activity.Current?.Id ?? Guid.NewGuid().ToString("N");
        var userId = _currentUser.UserId;

        _logger.LogInformation(
            "CQRS start {RequestType} TraceId={TraceId} RequestId={RequestId} UserId={UserId}",
            requestName,
            traceId,
            requestId,
            userId);

        var started = Stopwatch.GetTimestamp();
        try
        {
            var response = await next().ConfigureAwait(false);
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            _logger.LogInformation(
                "CQRS success {RequestType} TraceId={TraceId} RequestId={RequestId} DurationMs={DurationMs}",
                requestName,
                traceId,
                requestId,
                elapsedMs);
            return response;
        }
        catch (Exception ex)
        {
            var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
            _logger.LogError(
                ex,
                "CQRS failure {RequestType} TraceId={TraceId} RequestId={RequestId} DurationMs={DurationMs}",
                requestName,
                traceId,
                requestId,
                elapsedMs);
            throw;
        }
    }
}
