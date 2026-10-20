using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.Observability;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Diagnostics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors;

/// <summary>
/// Command metrics, optional tracing and slow-query logging, all driven by EF's own eventData.Duration.
/// </summary>
internal sealed partial class CommandObservabilityInterceptor(EfMetrics metrics,
                                                              ActivitySource activitySource,
                                                              ILogger<CommandObservabilityInterceptor> logger,
                                                              IOptionsMonitor<SlowQueryInterceptorOptions> options) : DbCommandInterceptor
{
    private const int MaxLoggedCommandLength = 2000;

    // ---------- Reader ----------

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
    {
        OnSuccess(command, eventData);
        return result;
    }

    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        OnSuccess(command, eventData);
        return ValueTask.FromResult(result);
    }

    // ---------- NonQuery ----------

    public override int NonQueryExecuted(
        DbCommand command, CommandExecutedEventData eventData, int result)
    {
        OnSuccess(command, eventData);
        return result;
    }

    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        OnSuccess(command, eventData);
        return ValueTask.FromResult(result);
    }

    // ---------- Scalar ----------

    public override object? ScalarExecuted(
        DbCommand command, CommandExecutedEventData eventData, object? result)
    {
        OnSuccess(command, eventData);
        return result;
    }

    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command, CommandExecutedEventData eventData, object? result,
        CancellationToken cancellationToken = default)
    {
        OnSuccess(command, eventData);
        return ValueTask.FromResult(result);
    }

    // ---------- Failure / Cancel ----------

    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        => OnFailure(command, eventData);

    public override Task CommandFailedAsync(
        DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        OnFailure(command, eventData);
        return Task.CompletedTask;
    }

    public override void CommandCanceled(DbCommand command, CommandEndEventData eventData)
        => OnCanceled(command, eventData);

    public override Task CommandCanceledAsync(
        DbCommand command, CommandEndEventData eventData, CancellationToken cancellationToken = default)
    {
        OnCanceled(command, eventData);
        return Task.CompletedTask;
    }

    // ---------- Handlers ----------

    private void OnSuccess(DbCommand command, CommandExecutedEventData e)
    {
        var operation = EfMetrics.GetOperation(command.CommandText);
        var target = EfTarget.From(command.Connection, e.Context);

        metrics.RecordCommand(e.Duration, target, operation);
        Trace(e, target, operation, errorType: null, isError: false);
        LogIfSlow(command, target, e.Duration);
    }

    private void OnFailure(DbCommand command, CommandErrorEventData e)
    {
        var operation = EfMetrics.GetOperation(command.CommandText);
        var target = EfTarget.From(command.Connection, e.Context);
        var errorType = e.Exception.GetType().Name;

        metrics.RecordCommand(e.Duration, target, operation, errorType);
        Trace(e, target, operation, errorType, isError: true);
    }

    private void OnCanceled(DbCommand command, CommandEndEventData e)
    {
        var operation = EfMetrics.GetOperation(command.CommandText);
        var target = EfTarget.From(command.Connection, e.Context);

        metrics.RecordCommand(e.Duration, target, operation, EfMetrics.Canceled);
        Trace(e, target, operation, EfMetrics.Canceled, isError: false);
    }

    private void Trace(CommandEndEventData e, EfTarget target, string operation, string? errorType, bool isError)
    {
        if (!options.CurrentValue.EmitCommandSpans)
            return;

        using var activity = activitySource.StartCompleted(operation, e.StartTime, e.Duration);
        if (activity is not { IsAllDataRequested: true })
            return;

        activity.SetTag("db.system.name", EfTracing.DbSystem(e.Context));
        activity.SetTag("db.namespace", target.Database);
        activity.SetTag("db.operation.name", operation);

        if (errorType is null)
            return;

        activity.SetTag("error.type", errorType);
        if (isError)
            activity.SetStatus(ActivityStatusCode.Error);
    }

    private void LogIfSlow(DbCommand command, EfTarget target, TimeSpan elapsed)
    {
        var o = options.CurrentValue;
        if (elapsed < o.WarningThreshold)
            return;

        var level = elapsed >= o.CriticalThreshold ? LogLevel.Critical
                  : elapsed >= o.ErrorThreshold ? LogLevel.Error
                  : LogLevel.Warning;

        if (!logger.IsEnabled(level))
            return;

        var text = command.CommandText;
        if (text.Length > MaxLoggedCommandLength)
            text = string.Concat(text.AsSpan(0, MaxLoggedCommandLength), "…");

        LogSlowCommand(logger, level, (long)elapsed.TotalMilliseconds, target.Database, text);
    }

    [LoggerMessage(Message = "EF command exceeded duration threshold: {ElapsedMs}ms. Database: {Database}. Command: {CommandText}")]
    private static partial void LogSlowCommand(ILogger logger, LogLevel level, long elapsedMs, string database, string commandText);
}
