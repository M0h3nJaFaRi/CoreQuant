namespace Common.Application.Outbox;

/// <summary>
/// Marks an outbox message as published after a successful <see cref="IOutboxPublisher"/> call.
/// </summary>
public interface IOutboxCompletionService
{
    /// <summary>
    /// Sets <c>ProcessedOnUtc</c> for the message on the write database.
    /// </summary>
    Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default);
}
