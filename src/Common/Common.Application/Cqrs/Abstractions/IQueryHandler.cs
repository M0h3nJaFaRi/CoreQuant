namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Handles a query.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Executes the query.
    /// </summary>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
