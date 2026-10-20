using FluentValidation;
using Common.Application.Exceptions;
using Common.Application.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Application.Cqrs.Behaviors.Validation;

/// <summary>
/// Runs FluentValidation validators for the request before the rest of the pipeline.
/// </summary>
internal sealed class ValidationBehavior<TRequest, TResponse>(IServiceProvider serviceProvider) : IPipelineBehavior<TRequest, TResponse>
{
    /// <inheritdoc />
    public int Order => PipelineOrders.Validation;

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var validators = serviceProvider.GetServices<IValidator<TRequest>>().ToArray();
        
        if (validators.Length == 0)
            return await next().ConfigureAwait(false);

        var context = new ValidationContext<TRequest>(request);
        
        var validationResults = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken))).ConfigureAwait(false);

        var failures = validationResults.SelectMany(r => r.Errors)
                                        .Where(f => f is not null)
                                        .Select(f => new ValidationFailure(f.PropertyName, f.ErrorMessage, f.ErrorCode))
                                        .ToArray();

        if (failures.Length > 0)
            throw new Exceptions.ValidationException(failures);

        return await next().ConfigureAwait(false);
    }
}