using CoreQuant.Infrastructure.Persistence.EF.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;
internal sealed class SaveChangesMetricsInterceptor(EfMetrics metrics) : SaveChangesInterceptor
{
    private readonly ConditionalWeakTable<DbContext, StrongBox<long>> _started = [];

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Begin(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Begin(eventData.Context);
        return ValueTask.FromResult(result);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        Record(eventData.Context, eventData.EntitiesSavedCount, errorType: null);
        return result;
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        Record(eventData.Context, eventData.EntitiesSavedCount, errorType: null);
        return ValueTask.FromResult(result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
        => OnFailed(eventData);

    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        OnFailed(eventData);
        return Task.CompletedTask;
    }

    public override void SaveChangesCanceled(DbContextEventData eventData)
        => Record(eventData.Context, 0, EfMetrics.Canceled);

    public override Task SaveChangesCanceledAsync(
        DbContextEventData eventData, CancellationToken cancellationToken = default)
    {
        Record(eventData.Context, 0, EfMetrics.Canceled);
        return Task.CompletedTask;
    }

    private void Begin(DbContext? context)
    {
        if (context is null) return;
        _started.GetValue(context, _ => new StrongBox<long>()).Value = Stopwatch.GetTimestamp();
    }

    private void Record(DbContext? context, int entities, string? errorType)
    {
        if (context is null || !_started.TryGetValue(context, out var box))
            return;

        metrics.RecordSaveChanges(Stopwatch.GetElapsedTime(box.Value), TargetOf(context), entities, errorType);
    }

    private void OnFailed(DbContextErrorEventData e)
    {
        Record(e.Context, 0, e.Exception.GetType().Name);

        if (e.Exception is DbUpdateConcurrencyException && e.Context is { } context)
            metrics.RecordConcurrencyConflict(TargetOf(context));
    }

    private static EfTarget TargetOf(DbContext context)
        => EfTarget.From(context.Database.GetDbConnection(), context);
}