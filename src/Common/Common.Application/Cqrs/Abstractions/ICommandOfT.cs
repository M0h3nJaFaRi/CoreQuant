namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Marker for a command that returns <typeparamref name="TResponse"/>.
/// </summary>
public interface ICommand<TResponse>;
