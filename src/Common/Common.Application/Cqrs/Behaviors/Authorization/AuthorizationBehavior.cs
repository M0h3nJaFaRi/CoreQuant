using Common.Application.Authorization.Abstraction;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Exceptions;

namespace Common.Application.Cqrs.Behaviors.Authorization;

/// <summary>
/// Authorizes the current caller for the CQRS use case.
/// </summary>
internal sealed class AuthorizationBehavior<TRequest, TResponse>(IAuthorizationService authorizationService) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.Authorization;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var result = await authorizationService.AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);

        if (!result.Allowed)
            throw new ForbiddenException($"Caller is not authorized for '{typeof(TRequest).Name}'.", result.Reason);

        return await next().ConfigureAwait(false);
    }
}