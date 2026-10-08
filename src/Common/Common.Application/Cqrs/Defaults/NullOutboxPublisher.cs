using Common.Application.Outbox;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No-op outbox publisher until a broker adapter is registered.
/// </summary>
internal sealed class NullOutboxPublisher : IOutboxPublisher
{
    /// <inheritdoc />
    public Task PublishAsync(OutboxPublishRequest request, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
