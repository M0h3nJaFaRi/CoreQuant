using Common.Application.Configuration.Options;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Idempotency;
using Microsoft.Extensions.Options;

namespace Common.Application.Cqrs.Behaviors.Idempotency;

/// <summary>
/// Short-circuits duplicate command executions when the request implements <see cref="IIdempotentCommand{TResponse}"/>.
/// </summary>
internal sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IIdempotencyStore _store;
    private readonly IdempotencyBehaviorOptions _options;

    public IdempotencyBehavior(IIdempotencyStore store, IOptions<IdempotencyBehaviorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(options);
        _store = store;
        _options = options.Value;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Idempotency;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand<TResponse> idempotent)
        {
            return await next().ConfigureAwait(false);
        }

        var key = idempotent.IdempotencyKey;
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var (found, cached) = await _store.TryGetAsync<TResponse>(key, cancellationToken).ConfigureAwait(false);
        if (found)
        {
            return cached!;
        }

        var response = await next().ConfigureAwait(false);
        await _store.SetAsync(key, response, _options.DefaultTtl, cancellationToken).ConfigureAwait(false);
        return response;
    }
}
