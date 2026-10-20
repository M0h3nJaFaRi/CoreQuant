using Common.Presentation.Contract;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace CoreQuant.Infrastructure.Persistence.EF.Observability;

/// <summary>
/// EF persistence instruments created on the shared Framework.Observability <see cref="Meter"/>.
/// </summary>
internal sealed class EfMetrics(Meter meter, ServiceIdentity identity)
{
    public const string Failed = "failed";
    public const string Canceled = "canceled";
    public const string Committed = "committed";
    public const string RolledBack = "rolled_back";

    private static readonly SearchValues<char> Whitespace = SearchValues.Create(" \n\r\t");
    private static readonly double[] DurationBuckets = [0.001, 0.005, 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1, 2.5, 5, 10];

    private readonly Histogram<double> _commandDuration = meter.CreateHistogram(
        name: $"{identity.Name}_ef_command_duration", unit: "s",
        description: "EF command duration. Tags: db.namespace, db.context, db.operation.name, error.type (absent on success)",
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = DurationBuckets });

    private readonly Histogram<double> _saveChangesDuration = meter.CreateHistogram(
        name: $"{identity.Name}_ef_save_changes_duration", unit: "s",
        description: "SaveChanges duration. Tags: db.namespace, db.context, error.type (absent on success)",
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = DurationBuckets });

    private readonly Histogram<long> _saveChangesEntities = meter.CreateHistogram<long>(
        name: $"{identity.Name}_ef_save_changes_entities", unit: "{entity}",
        description: "Entities written per successful SaveChanges. Tags: db.namespace, db.context");

    private readonly Histogram<double> _connectionOpenDuration = meter.CreateHistogram(
        name: $"{identity.Name}_ef_connection_open_duration", unit: "s",
        description: "Time to obtain an open connection (pool wait included). Tags: db.namespace, db.context, error.type (absent on success)",
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = DurationBuckets });

    private readonly UpDownCounter<long> _activeConnections = meter.CreateUpDownCounter<long>(
        name: $"{identity.Name}_ef_connection_active", unit: "{connection}",
        description: "Currently open EF connections. Tags: db.namespace, db.context");

    private readonly Histogram<double> _transactionDuration = meter.CreateHistogram(
        name: $"{identity.Name}_ef_transaction_duration", unit: "s",
        description: "Transaction lifetime. Tags: db.namespace, db.context, outcome (committed | rolled_back | failed)",
        advice: new InstrumentAdvice<double> { HistogramBucketBoundaries = DurationBuckets });

    private readonly Counter<long> _concurrencyConflicts = meter.CreateCounter<long>(
        name: $"{identity.Name}_ef_concurrency_conflicts", unit: "{conflict}",
        description: "DbUpdateConcurrencyException occurrences. Tags: db.namespace, db.context");

    // ---------- command ----------

    public void RecordCommand(TimeSpan elapsed, EfTarget target, string operation, string? errorType = null)
    {
        var tags = new TagList { { "db.operation.name", operation } };
        target.AddTo(ref tags);
        if (errorType is not null)
            tags.Add("error.type", errorType);

        _commandDuration.Record(elapsed.TotalSeconds, tags);
    }

    // ---------- save changes ----------

    public void RecordSaveChanges(TimeSpan elapsed, EfTarget target, int entitiesWritten, string? errorType = null)
    {
        var tags = new TagList();
        target.AddTo(ref tags);
        if (errorType is not null)
            tags.Add("error.type", errorType);

        _saveChangesDuration.Record(elapsed.TotalSeconds, tags);

        // تعداد entity فقط برای SaveChanges موفق معنی داره
        if (errorType is null)
        {
            var entityTags = new TagList();
            target.AddTo(ref entityTags);
            _saveChangesEntities.Record(entitiesWritten, entityTags);
        }
    }

    public void RecordConcurrencyConflict(EfTarget target)
    {
        var tags = new TagList();
        target.AddTo(ref tags);
        _concurrencyConflicts.Add(1, tags);
    }

    // ---------- connection ----------

    public void RecordConnectionOpened(TimeSpan openDuration, EfTarget target)
    {
        var tags = new TagList();
        target.AddTo(ref tags);

        _connectionOpenDuration.Record(openDuration.TotalSeconds, tags);
        _activeConnections.Add(1, tags);
    }

    public void RecordConnectionOpenFailed(TimeSpan elapsed, EfTarget target, string errorType)
    {
        var tags = new TagList { { "error.type", errorType } };
        target.AddTo(ref tags);

        _connectionOpenDuration.Record(elapsed.TotalSeconds, tags);
    }

    public void RecordConnectionClosed(EfTarget target)
    {
        var tags = new TagList();
        target.AddTo(ref tags);
        _activeConnections.Add(-1, tags);
    }

    // ---------- transaction ----------

    public void RecordTransaction(TimeSpan elapsed, EfTarget target, string outcome)
    {
        var tags = new TagList { { "outcome", outcome } };
        target.AddTo(ref tags);

        _transactionDuration.Record(elapsed.TotalSeconds, tags);
    }

    // ---------- helpers ----------

    /// فقط نوع عملیات SQL (cardinality ثابت). کامنت‌های اول (TagWith) رد میشن.
    public static string GetOperation(string commandText)
    {
        var span = commandText.AsSpan().TrimStart();

        while (span.StartsWith("--", StringComparison.Ordinal))
        {
            var newLine = span.IndexOf('\n');
            if (newLine < 0)
                return "OTHER";

            span = span[(newLine + 1)..].TrimStart();
        }

        var end = span.IndexOfAny(Whitespace);
        var word = end < 0 ? span : span[..end];

        if (word.Length is 0 or > 6)
            return "OTHER";

        if (word.Equals("SELECT", StringComparison.OrdinalIgnoreCase)) return "SELECT";
        if (word.Equals("INSERT", StringComparison.OrdinalIgnoreCase)) return "INSERT";
        if (word.Equals("UPDATE", StringComparison.OrdinalIgnoreCase)) return "UPDATE";
        if (word.Equals("DELETE", StringComparison.OrdinalIgnoreCase)) return "DELETE";
        if (word.Equals("MERGE", StringComparison.OrdinalIgnoreCase)) return "MERGE";

        return "OTHER";
    }
}