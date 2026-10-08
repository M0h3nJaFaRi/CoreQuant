using Common.Application.Authorization;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Exceptions;

namespace Common.Application.Cqrs.Behaviors.Authorization;

/// <summary>
/// Authorizes the current caller for the CQRS use case.
/// </summary>
internal sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Creates the behavior.
    /// </summary>
    public AuthorizationBehavior(IAuthorizationService authorizationService)
    {
        ArgumentNullException.ThrowIfNull(authorizationService);
        _authorizationService = authorizationService;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Authorization;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var allowed = await _authorizationService
            .AuthorizeAsync(typeof(TRequest), cancellationToken)
            .ConfigureAwait(false);

        if (!allowed)
        {
            throw new ForbiddenException($"Caller is not authorized for '{typeof(TRequest).Name}'.");
        }

        return await next().ConfigureAwait(false);
    }
}
