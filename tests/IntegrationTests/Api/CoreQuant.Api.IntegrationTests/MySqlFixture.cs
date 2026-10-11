using System.Data.Common;
using Common.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoreQuant.Infrastructure.Persistence.EF;
using CoreQuant.Infrastructure.Persistence.EF.Configuration.Options;
using CoreQuant.Infrastructure.Persistence.EF.DbContexts.Core;

namespace CoreQuant.Api.IntegrationTests;

/// <summary>
/// Shared real-MySQL database for persistence integration tests.
/// Enabled only when <c>SERVICEFOUNDATION_TEST_MYSQL</c> is set to a connection string;
/// otherwise the MySQL tests skip (Sqlite wiring coverage lives in
/// <c>CoreQuant.Infrastructure.Persistence.EF.UnitTests.NotePersistenceSqliteTests</c>).
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

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddCommonApplication();
        services.AddCoreQuantPersistenceEf2();
        services.AddMySqlDbContext<CoreQuantDbContext>(endpoint =>
        {
            ApplyConnectionString(endpoint, ConnectionString);
            endpoint.CommandTimeout = TimeSpan.FromSeconds(30);
        });

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

    private static void ApplyConnectionString(DatabaseEndpointOptions endpoint, string connectionString)
    {
        var builder = new DbConnectionStringBuilder { ConnectionString = connectionString };

        endpoint.Host = Get(builder, "Server", "Host", "Data Source") ?? "127.0.0.1";
        endpoint.Port = int.TryParse(Get(builder, "Port"), out var port) ? port : 3306;
        endpoint.Name = Get(builder, "Database", "Initial Catalog") ?? string.Empty;
        endpoint.Username = Get(builder, "User", "Uid", "User ID", "Username") ?? string.Empty;
        endpoint.Password = Get(builder, "Password", "Pwd") ?? string.Empty;
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
