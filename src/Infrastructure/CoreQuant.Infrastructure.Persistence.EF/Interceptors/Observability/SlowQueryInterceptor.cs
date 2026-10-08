using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

/// <summary>
/// Logs EF commands that exceed configured duration thresholds (warning / error / critical).
/// </summary>
internal sealed class SlowQueryInterceptor : DbCommandInterceptor
{
    private readonly ILogger<SlowQueryInterceptor> _logger;
    private readonly SlowQueryInterceptorOptions _options;
    private readonly ConcurrentDictionary<Guid, long> _timestamps = new();

    public SlowQueryInterceptor(
        ILogger<SlowQueryInterceptor> logger,
        IOptions<SlowQueryInterceptorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        _logger = logger;
        _options = options.Value;
    }

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
        End(command, eventData.CommandId);
        return base.ReaderExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<DbDataReader> ReaderExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        DbDataReader result,
        CancellationToken cancellationToken = default)
    {
        End(command, eventData.CommandId);
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
        End(command, eventData.CommandId);
        return base.NonQueryExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<int> NonQueryExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        End(command, eventData.CommandId);
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
        End(command, eventData.CommandId);
        return base.ScalarExecuted(command, eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<object?> ScalarExecutedAsync(
        DbCommand command,
        CommandExecutedEventData eventData,
        object? result,
        CancellationToken cancellationToken = default)
    {
        End(command, eventData.CommandId);
        return base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
    }

    /// <inheritdoc />
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    {
        _timestamps.TryRemove(eventData.CommandId, out _);
        base.CommandFailed(command, eventData);
    }

    /// <inheritdoc />
    public override Task CommandFailedAsync(
        DbCommand command,
        CommandErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        _timestamps.TryRemove(eventData.CommandId, out _);
        return base.CommandFailedAsync(command, eventData, cancellationToken);
    }

    private void Begin(Guid commandId) =>
        _timestamps[commandId] = Stopwatch.GetTimestamp();

    private void End(DbCommand command, Guid commandId)
    {
        if (!_timestamps.TryRemove(commandId, out var started))
        {
            return;
        }

        var elapsed = Stopwatch.GetElapsedTime(started);
        if (elapsed < _options.WarningThreshold)
        {
            return;
        }

        var ms = elapsed.TotalMilliseconds;
        var text = command.CommandText;

        if (elapsed >= _options.CriticalThreshold)
        {
            _logger.LogCritical(
                "EF command critical duration {ElapsedMs}ms. Command: {CommandText}",
                ms,
                text);
            return;
        }

        if (elapsed >= _options.ErrorThreshold)
        {
            _logger.LogError(
                "EF command slow duration {ElapsedMs}ms. Command: {CommandText}",
                ms,
                text);
            return;
        }

        _logger.LogWarning(
            "EF command elevated duration {ElapsedMs}ms. Command: {CommandText}",
            ms,
            text);
    }
}
