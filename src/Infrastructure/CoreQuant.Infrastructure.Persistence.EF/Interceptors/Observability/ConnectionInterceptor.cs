using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

/// <summary>
/// Traces and counts EF connection open/close via Framework.Observability instruments.
/// </summary>
internal sealed class ConnectionInterceptor : DbConnectionInterceptor
{
    private readonly ActivitySource _activitySource;
    private readonly EfPersistenceMetrics _metrics;
    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();

    public ConnectionInterceptor(ActivitySource activitySource, EfPersistenceMetrics metrics)
    {
        ArgumentNullException.ThrowIfNull(activitySource);
        ArgumentNullException.ThrowIfNull(metrics);
        _activitySource = activitySource;
        _metrics = metrics;
    }

    /// <inheritdoc />
    public override InterceptionResult ConnectionOpening(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result)
    {
        Start(eventData.ConnectionId);
        return base.ConnectionOpening(connection, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection,
        ConnectionEventData eventData,
        InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        Start(eventData.ConnectionId);
        return base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
    {
        Stop(eventData.ConnectionId);
        _metrics.ConnectionsOpened.Add(1);
        base.ConnectionOpened(connection, eventData);
    }

    /// <inheritdoc />
    public override Task ConnectionOpenedAsync(
        DbConnection connection,
        ConnectionEndEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Stop(eventData.ConnectionId);
        _metrics.ConnectionsOpened.Add(1);
        return base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
    }

    /// <inheritdoc />
    public override void ConnectionClosed(DbConnection connection, ConnectionEndEventData eventData)
    {
        _metrics.ConnectionsClosed.Add(1);
        base.ConnectionClosed(connection, eventData);
    }

    /// <inheritdoc />
    public override Task ConnectionClosedAsync(DbConnection connection, ConnectionEndEventData eventData)
    {
        _metrics.ConnectionsClosed.Add(1);
        return base.ConnectionClosedAsync(connection, eventData);
    }

    /// <inheritdoc />
    public override void ConnectionFailed(DbConnection connection, ConnectionErrorEventData eventData)
    {
        if (_activities.TryRemove(eventData.ConnectionId, out var activity))
        {
            activity.SetStatus(ActivityStatusCode.Error);
            activity.Dispose();
        }

        base.ConnectionFailed(connection, eventData);
    }

    /// <inheritdoc />
    public override Task ConnectionFailedAsync(
        DbConnection connection,
        ConnectionErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        if (_activities.TryRemove(eventData.ConnectionId, out var activity))
        {
            activity.SetStatus(ActivityStatusCode.Error);
            activity.Dispose();
        }

        return base.ConnectionFailedAsync(connection, eventData, cancellationToken);
    }

    private void Start(Guid connectionId)
    {
        var activity = _activitySource.StartActivity("ef.connection.open", ActivityKind.Client);
        if (activity is not null)
        {
            _activities[connectionId] = activity;
        }
    }

    private void Stop(Guid connectionId)
    {
        if (_activities.TryRemove(connectionId, out var activity))
        {
            activity.Dispose();
        }
    }
}
