namespace Framework.Observability.Correlation;

/// <summary>
/// Well-known correlation identifiers for logs, traces, and HTTP headers.
/// </summary>
public static class CorrelationKeys
{
    /// <summary>
    /// OpenTelemetry / logging baggage key for the correlation identifier.
    /// </summary>
    public const string CorrelationId = "correlation.id";

    /// <summary>
    /// Default HTTP header that carries the correlation identifier.
    /// </summary>
    public const string HeaderName = "X-Correlation-ID";
}
