using Common.Application.Outbox;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No-op outbox writer until persistence outbox is wired.
/// </summary>
internal sealed class NullOutboxWriter : IOutboxWriter
{
    /// <inheritdoc />
    public Task WriteAsync(OutboxMessageWrite message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
