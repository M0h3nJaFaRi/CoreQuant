using System.Data.Common;
using System.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Diagnostics;
using CoreQuant.Infrastructure.Persistence.EF.Observability;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Records EF command counts and durations on the shared Framework.Observability <see cref="System.Diagnostics.Metrics.Meter"/>.
/// </summary>
internal sealed class MetricsInterceptor(EfPersistenceMetrics metrics) : DbCommandInterceptor
{
    private readonly ConcurrentDictionary<Guid, long> _timestamps = new();
   
    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<DbDataReader> result)
    {
        Begin(eventData.CommandId);
        return base.ReaderExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<DbDataReader> result,
    CancellationToken cancellationToken = default)
    {
        Begin(eventData.CommandId);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override DbDataReader ReaderExecuted(
    DbCommand command,
    CommandExecutedEventData eventData,
    DbDataReader result)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: false);
        return base.ReaderExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<DbDataReader> ReaderExecutedAsync(
    DbCommand command,
    CommandExecutedEventData eventData,
    DbDataReader result,
    CancellationToken cancellationToken = default)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: false);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> NonQueryExecuting(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<int> result)
    {
        Begin(eventData.CommandId);
        return base.NonQueryExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<int> result,
    CancellationToken cancellationToken = default)
    {
        Begin(eventData.CommandId);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override int NonQueryExecuted(
    DbCommand command,
    CommandExecutedEventData eventData,
    int result)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: false);
        return base.NonQueryExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<int> NonQueryExecutedAsync(
    DbCommand command,
    CommandExecutedEventData eventData,
    int result,
    CancellationToken cancellationToken = default)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: false);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<object> ScalarExecuting(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<object> result)
    {
        Begin(eventData.CommandId);
        return base.ScalarExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
    DbCommand command,
    CommandEventData eventData,
    InterceptionResult<object> result,
    CancellationToken cancellationToken = default)
    {
        Begin(eventData.CommandId);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override object? ScalarExecuted(
    DbCommand command,
    CommandExecutedEventData eventData,
    object? result)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: false);
        return base.ScalarExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<object?> ScalarExecutedAsync(
    DbCommand command,
    CommandExecutedEventData eventData,
    object? result,
    CancellationToken cancellationToken = default)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: false);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: true);
        base.CommandFailed(command, eventData);
    }

    /// <inheritdoc />
    public override Task CommandFailedAsync(
    DbCommand command,
    CommandErrorEventData eventData,
    CancellationToken cancellationToken = default)
    {
        End(eventData.CommandId, eventData.ExecuteMethod, failed: true);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void Begin(Guid commandId) => _timestamps[commandId] = Stopwatch.GetTimestamp();
    private void End(Guid commandId, DbCommandMethod method, bool failed)
    {
        if (!_timestamps.TryRemove(commandId, out var started))
            return;

        var elapsedMs = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        var tag = new KeyValuePair<string, object?>("db.operation", method.ToString());

        metrics.CommandsTotal.Add(1, tag);
        metrics.CommandDurationMs.Record(elapsedMs, tag);

        if (failed)
            metrics.CommandsFailed.Add(1, tag);
    }
}