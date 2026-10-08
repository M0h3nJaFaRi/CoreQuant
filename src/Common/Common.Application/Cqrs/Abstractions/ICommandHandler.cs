namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Handles a command with no response.
/// </summary>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    Task HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
