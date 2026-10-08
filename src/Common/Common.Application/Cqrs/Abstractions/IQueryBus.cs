namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Dispatches queries through the CQRS pipeline.
/// </summary>
public interface IQueryBus
{
    /// <summary>
    /// Sends a query and returns <typeparamref name="TResponse"/>.
    /// </summary>
    Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default);
}
