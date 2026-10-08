namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Handles a command that returns <typeparamref name="TResponse"/>.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
