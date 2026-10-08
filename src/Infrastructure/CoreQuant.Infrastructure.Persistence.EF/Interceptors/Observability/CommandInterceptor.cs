using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

/// <summary>
/// Creates an <see cref="Activity"/> for each EF database command via Framework.Observability <see cref="ActivitySource"/>.
/// </summary>
internal sealed class CommandInterceptor : DbCommandInterceptor
{
    private readonly ActivitySource _activitySource;
    private readonly ConcurrentDictionary<Guid, Activity> _activities = new();

    public CommandInterceptor(ActivitySource activitySource)
    {
        ArgumentNullException.ThrowIfNull(activitySource);
        _activitySource = activitySource;
    }

    /// <inheritdoc />
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        Start(command, eventData);
        return base.ReaderExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Start(command, eventData);
        return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override DbDataReader ReaderExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result)
    {
        Stop(eventData.CommandId, error: false);
        return base.ReaderExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        Stop(eventData.CommandId, error: false);
        return base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        Start(command, eventData);
        return base.NonQueryExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Start(command, eventData);
        return base.NonQueryExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override int NonQueryExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result)
    {
        Stop(eventData.CommandId, error: false);
        return base.NonQueryExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        Stop(eventData.CommandId, error: false);
        return base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        Start(command, eventData);
        return base.ScalarExecuting(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Start(command, eventData);
        return base.ScalarExecutingAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override object? ScalarExecuted(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result)
    {
        Stop(eventData.CommandId, error: false);
        return base.ScalarExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        Stop(eventData.CommandId, error: false);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        Stop(eventData.CommandId, error: true);
        base.CommandFailed(command, eventData);
    }

    /// <inheritdoc />
    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        Stop(eventData.CommandId, error: true);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void Start(DbCommand command, CommandEventData eventData)
    {
        var activity = _activitySource.StartActivity("ef.command", ActivityKind.Client);
        if (activity is null)
        {
            return;
        }

        activity.SetTag("db.system", "relational");
        activity.SetTag("db.operation", eventData.ExecuteMethod.ToString());
        activity.SetTag("db.command_text", command.CommandText);
        _activities[eventData.CommandId] = activity;
    }

    private void Stop(Guid commandId, bool error)
    {
        if (!_activities.TryRemove(commandId, out var activity))
        {
            return;
        }

        if (error)
        {
            activity.SetStatus(ActivityStatusCode.Error);
        }

        activity.Dispose();
    }
}
