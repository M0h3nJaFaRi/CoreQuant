using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Persistence;

namespace Common.Application.Cqrs.Behaviors.Transaction;

/// <summary>
/// Opens an <see cref="IUnitOfWork"/> for commands. Queries pass through unchanged.
/// Outbox writers participate in the same UoW before commit.
/// </summary>
internal sealed class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Creates the behavior.
    /// </summary>
    public TransactionBehavior(IUnitOfWork unitOfWork)
    {
        ArgumentNullException.ThrowIfNull(unitOfWork);
        _unitOfWork = unitOfWork;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.Transaction;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand(request))
        {
            return await next().ConfigureAwait(false);
        }

        await _unitOfWork.BeginAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var response = await next().ConfigureAwait(false);
            await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
            return response;
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static bool IsCommand(TRequest request)
    {
        if (request is ICommand)
        {
            return true;
        }

        var type = typeof(TRequest);
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
