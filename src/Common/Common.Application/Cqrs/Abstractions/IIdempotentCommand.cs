namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Opt-in marker for commands that require idempotent handling.
/// </summary>
public interface IIdempotentCommand<TResponse> : ICommand<TResponse>
{
    /// <summary>
    /// Client-supplied key uniquely identifying this command attempt.
    /// </summary>
    string IdempotencyKey { get; }
}
