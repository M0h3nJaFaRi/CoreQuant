namespace CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;

/// <summary>
/// Connection settings for a single read or write database endpoint.
/// </summary>
public sealed class DatabaseEndpointOptions
{
    /// <summary>
    /// Database server host.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Database server port.
    /// </summary>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// Database user name.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Database password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Database (schema) name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Optional command timeout.
    /// </summary>
    public TimeSpan? CommandTimeout { get; set; }

    /// <summary>
    /// True when host and database name are present.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Command timeout in whole seconds for providers that require an <see cref="int"/>.
    /// </summary>
    public int? CommandTimeoutSeconds =>
        CommandTimeout is { } timeout
            ? (int)Math.Ceiling(timeout.TotalSeconds)
            : null;

    /// <summary>
    /// Builds a MySQL ADO.NET connection string from the endpoint fields.
    /// </summary>
    public string BuildMySqlConnectionString()
    {
        if (!IsConfigured)
        {
            return string.Empty;
        }

        return $"Server={Host};Port={Port};Database={Name};User={Username};Password={Password};";
    }

    /// <summary>
    /// Builds a SQL Server ADO.NET connection string from the endpoint fields.
    /// </summary>
    public string BuildSqlServerConnectionString()
    {
        if (!IsConfigured)
        {
            return string.Empty;
        }

        return $"Server={Host},{Port};Database={Name};User Id={Username};Password={Password};";
    }

    /// <summary>
    /// Builds a PostgreSQL (Npgsql) connection string from the endpoint fields.
    /// </summary>
    public string BuildNpgsqlConnectionString()
    {
        if (!IsConfigured)
        {
            return string.Empty;
        }

        return $"Host={Host};Port={Port};Database={Name};Username={Username};Password={Password}";
    }

    /// <summary>
    /// Copies field values from another endpoint.
    /// </summary>
    public void CopyFrom(DatabaseEndpointOptions source)
    {
        ArgumentNullException.ThrowIfNull(source);

        Host = source.Host;
        Port = source.Port;
        Username = source.Username;
        Password = source.Password;
        Name = source.Name;
        CommandTimeout = source.CommandTimeout;
    }
}
