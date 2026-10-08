namespace Common.Application.Outbox;

/// <summary>
/// Scoped buffer of outbox messages written in the current unit of work, drained after commit for publish.
/// </summary>
public interface IOutboxPendingBuffer
{
    /// <summary>
    /// Records a message that was persisted in the current transaction.
    /// </summary>
    void Enqueue(OutboxPublishRequest request);

    /// <summary>
    /// Returns and clears buffered messages for the current scope.
    /// </summary>
    IReadOnlyList<OutboxPublishRequest> Drain();
}
