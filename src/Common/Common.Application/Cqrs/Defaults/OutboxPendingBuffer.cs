using Common.Application.Outbox;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// Scoped in-memory pending outbox buffer for the current request / unit of work.
/// </summary>
internal sealed class OutboxPendingBuffer : IOutboxPendingBuffer
{
    private readonly List<OutboxPublishRequest> _pending = [];

    /// <inheritdoc />
    public void Enqueue(OutboxPublishRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (_pending.Any(x => x.MessageId == request.MessageId))
        {
            return;
        }

        _pending.Add(request);
    }

    /// <inheritdoc />
    public IReadOnlyList<OutboxPublishRequest> Drain()
    {
        if (_pending.Count == 0)
        {
            return [];
        }

        var snapshot = _pending.ToArray();
        _pending.Clear();
        return snapshot;
    }
}
