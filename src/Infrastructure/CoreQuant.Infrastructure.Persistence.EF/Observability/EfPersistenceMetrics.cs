using System.Diagnostics.Metrics;

namespace CoreQuant.Infrastructure.Persistence.EF.Observability;

/// <summary>
/// EF persistence instruments created on the shared Framework.Observability <see cref="Meter"/>.
/// </summary>
internal sealed class EfPersistenceMetrics(Meter meter)
{
    public Counter<long> CommandsTotal { get; } = meter.CreateCounter<long>("ef.commands.total", description: "EF database commands executed");
    public Counter<long> CommandsFailed { get; } = meter.CreateCounter<long>("ef.commands.failed", description: "EF database commands that failed");
    public Histogram<double> CommandDurationMs { get; } = meter.CreateHistogram<double>("ef.commands.duration", unit: "ms", description: "EF database command duration");
    public Counter<long> ConnectionsOpened { get; } = meter.CreateCounter<long>("ef.connections.opened", description: "EF database connections opened");
    public Counter<long> ConnectionsClosed { get; } = meter.CreateCounter<long>("ef.connections.closed", description: "EF database connections closed");
    public Counter<long> TransactionsTotal { get; } = meter.CreateCounter<long>("ef.transactions.total", description: "EF transactions started");
    public Counter<long> TransactionsCommitted { get; } = meter.CreateCounter<long>("ef.transactions.committed", description: "EF transactions committed");
    public Counter<long> TransactionsRolledBack { get; } = meter.CreateCounter<long>("ef.transactions.rolled_back", description: "EF transactions rolled back");
}