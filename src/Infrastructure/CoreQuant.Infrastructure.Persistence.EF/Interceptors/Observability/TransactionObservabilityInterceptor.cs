using CoreQuant.Infrastructure.Persistence.EF.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

internal sealed class TransactionObservabilityInterceptor(EfMetrics metrics, ActivitySource activitySource) : DbTransactionInterceptor
{
    private readonly ConcurrentDictionary<Guid, (long Timestamp, DateTimeOffset Start, EfTarget Target)> _open = new();

    public override DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
    {
        Begin(connection, eventData);
        return result;
    }

    public override ValueTask<DbTransaction> TransactionStartedAsync(
        DbConnection connection, TransactionEndEventData eventData, DbTransaction result,
        CancellationToken cancellationToken = default)
    {
        Begin(connection, eventData);
        return ValueTask.FromResult(result);
    }

    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
        => End(eventData, EfMetrics.Committed, errorType: null);

    public override Task TransactionCommittedAsync(
        DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        End(eventData, EfMetrics.Committed, errorType: null);
        return Task.CompletedTask;
    }

    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
        => End(eventData, EfMetrics.RolledBack, errorType: null);

    public override Task TransactionRolledBackAsync(
        DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        End(eventData, EfMetrics.RolledBack, errorType: null);
        return Task.CompletedTask;
    }

    public override void TransactionFailed(DbTransaction transaction, TransactionErrorEventData eventData)
        => End(eventData, EfMetrics.Failed, eventData.Exception.GetType().Name);

    public override Task TransactionFailedAsync(
        DbTransaction transaction, TransactionErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        End(eventData, EfMetrics.Failed, eventData.Exception.GetType().Name);
        return Task.CompletedTask;
    }

    private void Begin(DbConnection connection, TransactionEndEventData e)
        => _open[e.TransactionId] = (Stopwatch.GetTimestamp(), e.StartTime, EfTarget.From(connection, e.Context));

    private void End(TransactionEndEventData e, string outcome, string? errorType)
    {
        if (!_open.TryRemove(e.TransactionId, out var state))
            return;

        var elapsed = Stopwatch.GetElapsedTime(state.Timestamp);
        metrics.RecordTransaction(elapsed, state.Target, outcome);

        using var activity = activitySource.StartCompleted("transaction", state.Start, elapsed);
        if (activity is not { IsAllDataRequested: true })
            return;

        activity.SetTag("db.system.name", EfTracing.DbSystem(e.Context));
        activity.SetTag("db.namespace", state.Target.Database);
        activity.SetTag("outcome", outcome);

        // rollback عادی خطا نیست؛ فقط شکست واقعی Error میشه
        if (errorType is not null)
        {
            activity.SetTag("error.type", errorType);
            activity.SetStatus(ActivityStatusCode.Error);
        }
    }
}
