using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Abstractions.Commands;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Cqrs.Pipeline.Context;
using Common.Application.Idempotency;
using Microsoft.Extensions.Options;

namespace Common.Application.Cqrs.Behaviors.Idempotency;

/// <summary>
/// Short-circuits duplicate command executions when the request implements <see cref="IIdempotentCommand{TResponse}"/>.
/// </summary>
internal sealed class IdempotencyBehavior<TRequest, TResponse>(IIdempotencyStore store,
                                                               IRequestExecutionContext executionContext, 
                                                               IOptions<IdempotencyBehaviorOptions> options) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.Idempotency;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand<TResponse> idempotent)
            return await next().ConfigureAwait(false);

        var key = idempotent.IdempotencyKey;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (found, cached) = await store.TryGetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);

        if (found)
        {
            executionContext.MarkServedFrom("idempotency");
            return cached!;
        }

        var response = await next().ConfigureAwait(false);
        await store.SetAsync(key, response, options.Value.DefaultTtl, cancellationToken).ConfigureAwait(false);

        return response;
    }
}