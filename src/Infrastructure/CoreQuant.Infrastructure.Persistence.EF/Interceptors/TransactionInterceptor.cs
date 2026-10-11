using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using CoreQuant.Infrastructure.Persistence.EF.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Traces and counts EF transactions via Framework.Observability instruments.
/// </summary>
internal sealed class TransactionInterceptor(ActivitySource activitySource, EfPersistenceMetrics metrics) : DbTransactionInterceptor
{
    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();

    /// <inheritdoc />
    public override InterceptionResult<DbTransaction> TransactionStarting(
    DbConnection connection,
    TransactionStartingEventData eventData,
    InterceptionResult<DbTransaction> result)
    {
        Start(eventData.TransactionId);
        metrics.TransactionsTotal.Add(1);
        return base.TransactionStarting(connection, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbTransaction>> TransactionStartingAsync(
    DbConnection connection,
    TransactionStartingEventData eventData,
    InterceptionResult<DbTransaction> result,
    CancellationToken cancellationToken = default)
    {
        Start(eventData.TransactionId);
        metrics.TransactionsTotal.Add(1);
        return base.TransactionStartingAsync(connection, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        metrics.TransactionsCommitted.Add(1);
        Stop(eventData.TransactionId, error: false, result: "committed");
        base.TransactionCommitted(transaction, eventData);
    }

    /// <inheritdoc />
    public override Task TransactionCommittedAsync(
    DbTransaction transaction,
    TransactionEndEventData eventData,
    CancellationToken cancellationToken = default)
    {
        metrics.TransactionsCommitted.Add(1);
        Stop(eventData.TransactionId, error: false, result: "committed");
        return base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
    }

    /// <inheritdoc />
    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        metrics.TransactionsRolledBack.Add(1);
        Stop(eventData.TransactionId, error: true, result: "rolled_back");
        base.TransactionRolledBack(transaction, eventData);
    }

    /// <inheritdoc />
    public override Task TransactionRolledBackAsync(
    DbTransaction transaction,
    TransactionEndEventData eventData,
    CancellationToken cancellationToken = default)
    {
        metrics.TransactionsRolledBack.Add(1);
        Stop(eventData.TransactionId, error: true, result: "rolled_back");
        return base.TransactionRolledBackAsync(transaction, eventData, cancellationToken);
    }

    private void Start(Guid transactionId)
    {
        var activity = activitySource.StartActivity("ef.transaction", ActivityKind.Client);
 
        if (activity is not null)
            _activities[transactionId] = activity;
    }

    private void Stop(Guid transactionId, bool error, string result)
    {
        if (!_activities.TryRemove(transactionId, out var activity))
            return;

        activity.SetTag("ef.transaction.result", result);

        if (error)
            activity.SetStatus(ActivityStatusCode.Error);

        activity.Dispose();
    }
}