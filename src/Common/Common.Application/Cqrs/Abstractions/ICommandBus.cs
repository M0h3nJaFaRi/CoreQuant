namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Dispatches commands through the CQRS pipeline.
/// </summary>
public interface ICommandBus
{
    /// <summary>
    /// Sends a command with no response.
    /// </summary>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a command and returns <typeparamref name="TResponse"/>.
    /// </summary>
    Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default);
}
