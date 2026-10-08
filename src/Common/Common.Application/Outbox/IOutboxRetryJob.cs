namespace Common.Application.Outbox;

/// <summary>
/// Worker/Scheduler job that publishes pending outbox rows left after a failed post-commit publish.
/// </summary>
public interface IOutboxRetryJob
{
    /// <summary>
    /// Acquires a work lease, loads a pending batch from master, publishes, and marks completed.
    /// </summary>
    Task<OutboxRetryJobResult> ExecuteAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Outcome of one <see cref="IOutboxRetryJob"/> run.
/// </summary>
/// <param name="SkippedNoLease">True when another worker held the lease.</param>
/// <param name="Attempted">Messages attempted in this run.</param>
/// <param name="Published">Successfully published and marked processed.</param>
/// <param name="Failed">Publish failures left pending for a later run.</param>
public sealed record OutboxRetryJobResult(
    bool SkippedNoLease,
    int Attempted,
    int Published,
    int Failed);
