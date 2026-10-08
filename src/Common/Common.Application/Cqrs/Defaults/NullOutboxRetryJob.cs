using Common.Application.Outbox;

namespace Common.Application.Cqrs.Defaults;

/// <summary>
/// No-op outbox retry job until EF persistence registers a real implementation.
/// </summary>
internal sealed class NullOutboxRetryJob : IOutboxRetryJob
{
    /// <inheritdoc />
    public Task<OutboxRetryJobResult> ExecuteAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(new OutboxRetryJobResult(
            SkippedNoLease: false,
            Attempted: 0,
            Published: 0,
            Failed: 0));
}
