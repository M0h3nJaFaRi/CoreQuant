namespace Common.Application.Cqrs.Abstractions.Queries;

/// <summary>
/// Marker for a read-only query that returns <typeparamref name="TResponse"/>.
/// </summary>
public interface IQuery<TResponse>;