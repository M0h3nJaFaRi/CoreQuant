using CoreQuant.Infrastructure.Persistence.EF.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

internal sealed class ConnectionObservabilityInterceptor(EfMetrics metrics, ActivitySource activitySource) : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => OnOpened(connection, eventData);

    public override Task ConnectionOpenedAsync(
        DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        OnOpened(connection, eventData);
        return Task.CompletedTask;
    }

    public override void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData)
        => OnFailed(connection, eventData);

    public override Task ConnectionFailedAsync(
        DbConnection connection, ConnectionErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        OnFailed(connection, eventData);
        return Task.CompletedTask;
    }

    public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
        => metrics.RecordConnectionClosed(EfTarget.From(connection, eventData.Context));

    public override Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
    {
        metrics.RecordConnectionClosed(EfTarget.From(connection, eventData.Context));
        return Task.CompletedTask;
    }

    private void OnOpened(DbConnection connection, ConnectionEndEventData e)
    {
        var target = EfTarget.From(connection, e.Context);
        metrics.RecordConnectionOpened(e.Duration, target);

        using var activity = activitySource.StartCompleted("connection open", e.StartTime, e.Duration);
        if (activity is not { IsAllDataRequested: true })
            return;

        activity.SetTag("db.system.name", EfTracing.DbSystem(e.Context));
        activity.SetTag("db.namespace", target.Database);
    }

    private void OnFailed(DbConnection connection, ConnectionErrorEventData e)
    {
        var target = EfTarget.From(connection, e.Context);
        var errorType = e.Exception.GetType().Name;
        metrics.RecordConnectionOpenFailed(e.Duration, target, errorType);

        using var activity = activitySource.StartCompleted("connection open", e.StartTime, e.Duration);
        if (activity is not { IsAllDataRequested: true })
            return;

        activity.SetTag("db.system.name", EfTracing.DbSystem(e.Context));
        activity.SetTag("db.namespace", target.Database);
        activity.SetTag("error.type", errorType);
        activity.SetStatus(ActivityStatusCode.Error);
    }
}
