namespace Common.Application.Outbox;

/// <summary>
/// Writes outbox messages within the same persistence transaction as aggregate changes.
/// </summary>
public interface IOutboxWriter
{
    /// <summary>
    /// Enqueues a message for reliable publication after commit.
    /// </summary>
    Task WriteAsync(OutboxMessageWrite message, CancellationToken cancellationToken = default);
}
