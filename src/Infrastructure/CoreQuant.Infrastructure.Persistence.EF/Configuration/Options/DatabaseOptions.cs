namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

/// <summary>
/// Paired write (primary) and read (replica) connection settings.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// Configuration section name under the host tree.
    /// </summary>
    public const string SectionName = "Infrastructure:Databases";

    /// <summary>
    /// Primary / write database.
    /// </summary>
    public DatabaseEndpointOptions Write { get; set; } = new();

    /// <summary>
    /// Read replica / read database. Falls back to <see cref="Write"/> when not configured.
    /// </summary>
    public DatabaseEndpointOptions Read { get; set; } = new();

    /// <summary>
    /// Endpoint used for read operations (replica when configured, otherwise write).
    /// </summary>
    public DatabaseEndpointOptions ResolveReadEndpoint()
    {
        if (!Read.IsConfigured)
        {
            return Write;
        }

        if (Read.CommandTimeout is null && Write.CommandTimeout is not null)
        {
            var merged = new DatabaseEndpointOptions();
            merged.CopyFrom(Read);
            merged.CommandTimeout = Write.CommandTimeout;
            return merged;
        }

        return Read;
    }
}
