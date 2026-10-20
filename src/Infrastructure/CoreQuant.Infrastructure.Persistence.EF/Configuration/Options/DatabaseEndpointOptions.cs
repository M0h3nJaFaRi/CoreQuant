using Framework.Configurations.Options;

namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

/// <summary>
/// Connection inittings for a single MySQL database endpoint.
/// </summary>
public record DatabaseEndpointOptions
{
    /// <summary>
    /// Database server host.
    /// </summary>
    public string Host { get; init; } = default!;

    /// <summary>
    /// Database server port.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Database user name.
    /// </summary>
    public string Username { get; init; } = default!;

    /// <summary>
    /// Database password.
    /// </summary>
    public string Password { get; init; } = default!;

    /// <summary>
    /// Database (schema) name.
    /// </summary>
    public string Name { get; init; } = default!;

    /// <summary>
    /// Optional command timeout.
    /// </summary>
    public TimeSpan? CommandTimeout { get; init; }

    /// <summary>
    /// Health check settings for this database.
    /// </summary>
    public DependencyHealthOptions Health { get; init; } = new();

    /// <summary>
    /// True when host and database name are present.
    /// </summary>
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Command timeout in whole seconds for providers that require an <see cref="int"/>.
    /// </summary>
    public int? CommandTimeoutSeconds => CommandTimeout is { } timeout ? (int)Math.Ceiling(timeout.TotalSeconds) : null;

    /// <summary>
    /// Builds a MySQL ADO.NET connection string from the endpoint fields.
    /// </summary>
    public string BuildMySqlConnectionString()
    {
        if (!IsConfigured) return string.Empty;

        return $"Server={Host};Port={Port};Database={Name};User={Username};Password={Password};";
    }
}
