using Common.Application.Outbox;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No-op completion until Persistence registers an EF implementation.
/// </summary>
internal sealed class NullOutboxCompletionService : IOutboxCompletionService
{
    /// <inheritdoc />
    public Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
