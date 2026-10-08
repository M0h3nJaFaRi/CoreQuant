using System.Diagnostics.Metrics;

namespace CoreQuant.Infrastructure.Persistence.EF.Interceptors.Observability;

/// <summary>
/// EF persistence instruments created on the shared Framework.Observability <see cref="Meter"/>.
/// </summary>
internal sealed class EfPersistenceMetrics
{
    public EfPersistenceMetrics(Meter meter)
    {
        ArgumentNullException.ThrowIfNull(meter);

        CommandsTotal = meter.CreateCounter<long>(
            "ef.commands.total",
            description: "EF database commands executed");

        CommandsFailed = meter.CreateCounter<long>(
            "ef.commands.failed",
            description: "EF database commands that failed");

        CommandDurationMs = meter.CreateHistogram<double>(
            "ef.commands.duration",
            unit: "ms",
            description: "EF database command duration");

        ConnectionsOpened = meter.CreateCounter<long>(
            "ef.connections.opened",
            description: "EF database connections opened");

        ConnectionsClosed = meter.CreateCounter<long>(
            "ef.connections.closed",
            description: "EF database connections closed");

        TransactionsTotal = meter.CreateCounter<long>(
            "ef.transactions.total",
            description: "EF transactions started");

        TransactionsCommitted = meter.CreateCounter<long>(
            "ef.transactions.committed",
            description: "EF transactions committed");

        TransactionsRolledBack = meter.CreateCounter<long>(
            "ef.transactions.rolled_back",
            description: "EF transactions rolled back");
    }

    public Counter<long> CommandsTotal { get; }

    public Counter<long> CommandsFailed { get; }

    public Histogram<double> CommandDurationMs { get; }

    public Counter<long> ConnectionsOpened { get; }

    public Counter<long> ConnectionsClosed { get; }

    public Counter<long> TransactionsTotal { get; }

    public Counter<long> TransactionsCommitted { get; }

    public Counter<long> TransactionsRolledBack { get; }
}
