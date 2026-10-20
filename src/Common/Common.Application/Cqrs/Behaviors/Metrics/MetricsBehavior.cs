using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Abstractions.Commands;
using Common.Application.Cqrs.Abstractions.Queries;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Cqrs.Pipeline.Context;
using Common.Application.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Common.Application.Cqrs.Behaviors.Metrics;

/// <summary>
/// Records low-cardinality CQRS metrics.
/// </summary>
internal sealed class MetricsBehavior<TRequest, TResponse>(CqrsMetrics metrics,
                                                           IRequestExecutionContext executionContext) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.Metrics;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var kind = ResolveKind(request);
        var requestType = typeof(TRequest).Name;
        var started = Stopwatch.GetTimestamp();

        metrics.IncrementInFlight(requestType, kind);
        try
        {
            var response = await next().ConfigureAwait(false);
            metrics.RecordSuccess(requestType, kind, started, executionContext.ServedFrom);
            return response;
        }
        catch (Exception ex)
        {
            RecordFailure(ex, requestType, kind, started, cancellationToken);
            throw;
        }
        finally
        {
            metrics.DecrementInFlight(requestType, kind);
        }
    }

    /// <summary>
    /// Maps a thrown exception to a bounded metrics outcome and records it. Known,
    /// already-categorized exceptions map to their specific outcome; anything else
    /// falls through to "unhandled" with the exception's type name attached.
    /// </summary>
    private void RecordFailure(Exception ex, string requestType, string kind, long started, CancellationToken cancellationToken)
    {
        switch (ex)
        {
            case ForbiddenException forbidden:
                metrics.RecordFailure(requestType, kind, "forbidden", started, reason: forbidden.Reason);
                break;

            case RateLimitExceededException:
                metrics.RecordFailure(requestType, kind, "rate_limited", started);
                break;

            case ValidationException validation:
                metrics.RecordValidationFailure(requestType, kind, started, validation.Failures.Count);
                break;

            case OperationCanceledException when cancellationToken.IsCancellationRequested:
                metrics.RecordFailure(requestType, kind, "cancelled", started);
                break;

            default:
                metrics.RecordFailure(requestType, kind, "unhandled", started, exceptionType: ex.GetType().Name);
                break;
        }
    }

    /// <summary>
    /// Resolves the broad, low-cardinality request kind. Does NOT reflect optional
    /// capabilities (idempotent, cacheable, rate-limited) — those are orthogonal and
    /// already surfaced through their own behaviors/instruments (e.g. cache/idempotency
    /// hits via <see cref="IRequestExecutionContext.ServedFrom"/>), to avoid exploding
    /// the cardinality of this core "kind" tag.
    /// </summary>
    private static string ResolveKind(TRequest request) => request switch
    {
        ICommand => "command",
        IQuery<TResponse> => "query",
        _ => "unknown"
    };
}