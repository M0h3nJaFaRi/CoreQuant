using System.Diagnostics;
using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Common.Application.Cqrs.Behaviors.Performance;

/// <summary>
/// Measures CQRS execution duration and warns when exceeding a configured threshold.
/// </summary>
internal sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly PerformanceBehaviorOptions _options;

    /// <summary>
    /// Creates the behavior.
    /// </summary>
    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IOptions<PerformanceBehaviorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Performance;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            return await next().ConfigureAwait(false);
        }
        finally
        {
            var elapsed = Stopwatch.GetElapsedTime(started);
            var requestType = typeof(TRequest).Name;
            if (elapsed > _options.SlowRequestThreshold)
            {
                _logger.LogWarning(
                    "CQRS slow {RequestType} DurationMs={DurationMs} ThresholdMs={ThresholdMs}",
                    requestType,
                    elapsed.TotalMilliseconds,
                    _options.SlowRequestThreshold.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug(
                    "CQRS performance {RequestType} DurationMs={DurationMs}",
                    requestType,
                    elapsed.TotalMilliseconds);
            }
        }
    }
}
