using System.Data.Common;
using Common.Application;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoreQuant.Infrastructure.Persistence.EF;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Core;

namespace CoreQuant.Api.IntegrationTests;

/// <summary>
/// Shared real-MySQL database for persistence integration tests.
/// Enabled only when <c>SERVICEFOUNDATION_TEST_MYSQL</c> is set to a connection string;
/// otherwise the MySQL tests skip.
/// </summary>
public sealed class MySqlFixture : IAsyncLifetime
{
    public const string ConnectionStringVariable = "SERVICEFOUNDATION_TEST_MYSQL";

    private const string DefaultConnectionString =
        "Server=127.0.0.1;Port=3306;Database=core_quant_tests;User=root;Password=changeme;Connection Timeout=10;";

    public ServiceProvider? Provider { get; private set; }

    /// <summary>
    /// True when a MySQL connection string was provided and tests should run.
    /// </summary>
    public bool Enabled { get; } =
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(ConnectionStringVariable));

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable(ConnectionStringVariable) ?? DefaultConnectionString;

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        if (!Enabled)
        {
            return;
        }

        var endpoint = ParseConnectionString(ConnectionString) with { CommandTimeout = TimeSpan.FromSeconds(30) };
        var configuration = BuildConfiguration(endpoint);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCommonApplication();
        services.AddCoreQuantPersistenceEf(configuration);

        Provider = services.BuildServiceProvider();
        await using var scope = Provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CoreQuantDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        if (Provider is not null)
        {
            await using var scope = Provider.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CoreQuantDbContext>();
            await db.Database.EnsureDeletedAsync();
            await Provider.DisposeAsync();
        }
    }

    private static IConfiguration BuildConfiguration(DatabaseEndpointOptions endpoint)
    {
        var keys = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var name in new[] { "Core", "News", "Forex", "Index", "Metal", "Stock", "Crypto", "Commodity" })
        {
            keys[$"{DatabaseOptions.SectionName}:{name}:Host"] = endpoint.Host;
            keys[$"{DatabaseOptions.SectionName}:{name}:Port"] = endpoint.Port.ToString();
            keys[$"{DatabaseOptions.SectionName}:{name}:Name"] = $"{endpoint.Name}_{name.ToLowerInvariant()}";
            keys[$"{DatabaseOptions.SectionName}:{name}:Username"] = endpoint.Username;
            keys[$"{DatabaseOptions.SectionName}:{name}:Password"] = endpoint.Password;
            keys[$"{DatabaseOptions.SectionName}:{name}:CommandTimeout"] = endpoint.CommandTimeout?.ToString();
        }

        keys["Infrastructure:Persistence:SlowQuery:WarningThreshold"] = "00:00:00.250";
        keys["Infrastructure:Persistence:SlowQuery:ErrorThreshold"] = "00:00:00.500";
        keys["Infrastructure:Persistence:SlowQuery:CriticalThreshold"] = "00:00:01";

        return new ConfigurationBuilder()
            .AddInMemoryCollection(keys)
            .Build();
    }

    private static DatabaseEndpointOptions ParseConnectionString(string connectionString)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        return new DatabaseEndpointOptions
        {
            Host = Get(builder, "Server", "Host", "Data Source") ?? "127.0.0.1",
            Port = int.TryParse(Get(builder, "Port"), out var port) ? port : 3306,
            Name = Get(builder, "Database", "Initial Catalog") ?? string.Empty,
            Username = Get(builder, "User", "Uid", "User ID", "Username") ?? string.Empty,
            Password = Get(builder, "Password", "Pwd") ?? string.Empty,
        };
    }

    private static string? Get(DbConnectionStringBuilder builder, params string[] keys)
    {
        foreach (var key in keys)
        {
            if (builder.TryGetValue(key, out var value) && value is not null)
            {
                var text = Convert.ToString(value);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }
        }

        return null;
    }
}

[CollectionDefinition("mysql")]
public sealed class MySqlCollection : ICollectionFixture<MySqlFixture>;
