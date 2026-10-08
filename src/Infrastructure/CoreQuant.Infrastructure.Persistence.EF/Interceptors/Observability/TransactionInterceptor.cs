using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

/// <summary>
/// Traces and counts EF transactions via Framework.Observability instruments.
/// </summary>
internal sealed class TransactionInterceptor : DbTransactionInterceptor
{
    private readonly ActivitySource _activitySource;
    private readonly EfPersistenceMetrics _metrics;
    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();

    public TransactionInterceptor(ActivitySource activitySource, EfPersistenceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(activitySource);
        ArgumentNullException.ThrowIfNull(metrics);
        _activitySource = activitySource;
        _metrics = metrics;
    }

    /// <inheritdoc />
    public override InterceptionResult<DbTransaction> TransactionStarting(
        DbConnection connection,
        TransactionStartingEventData eventData,
        InterceptionResult<DbTransaction> result)
    {
        Start(eventData.TransactionId);
        _metrics.TransactionsTotal.Add(1);
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
        _metrics.TransactionsTotal.Add(1);
        return base.TransactionStartingAsync(connection, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData)
    {
        _metrics.TransactionsCommitted.Add(1);
        Stop(eventData.TransactionId, error: false, result: "committed");
        base.TransactionCommitted(transaction, eventData);
    }

    /// <inheritdoc />
    public override Task TransactionCommittedAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _metrics.TransactionsCommitted.Add(1);
        Stop(eventData.TransactionId, error: false, result: "committed");
        return base.TransactionCommittedAsync(transaction, eventData, cancellationToken);
    }

    /// <inheritdoc />
    public override void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
    {
        _metrics.TransactionsRolledBack.Add(1);
        Stop(eventData.TransactionId, error: true, result: "rolled_back");
        base.TransactionRolledBack(transaction, eventData);
    }

    /// <inheritdoc />
    public override Task TransactionRolledBackAsync(
        DbTransaction transaction,
        TransactionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _metrics.TransactionsRolledBack.Add(1);
        Stop(eventData.TransactionId, error: true, result: "rolled_back");
        return base.TransactionRolledBackAsync(transaction, eventData, cancellationToken);
    }

    private void Start(Guid transactionId)
    {
        var activity = _activitySource.StartActivity("ef.transaction", ActivityKind.Client);
        if (activity is not null)
        {
            _activities[transactionId] = activity;
        }
    }

    private void Stop(Guid transactionId, bool error, string result)
    {
        if (!_activities.TryRemove(transactionId, out var activity))
        {
            return;
        }

        activity.SetTag("ef.transaction.result", result);
        if (error)
        {
            activity.SetStatus(ActivityStatusCode.Error);
        }

        activity.Dispose();
    }
}
