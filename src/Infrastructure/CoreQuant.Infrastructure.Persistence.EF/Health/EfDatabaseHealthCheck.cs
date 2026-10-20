using Framework.Configurations.Options;
using Framework.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoreQuant.Infrastructure.Persistence.EF.Health;

/// <summary>
/// Opens a raw ADO.NET connection from the context and runs a probe query.
/// Going through DbConnection directly bypasses EF interceptors, so probes
/// never show up in EF metrics, slow-query logs or EF traces.
/// </summary>
internal sealed class EfDatabaseHealthCheck(IServiceScopeFactory scopeFactory, Type contextType, DependencyHealthOptions options) : DependencyHealthCheck(options)
{
    private const string ProbeSql = "SELECT 1";
    protected override async Task ProbeAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);
        var connection = context.Database.GetDbConnection();

        try
        {
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = ProbeSql;
            await command.ExecuteScalarAsync(cancellationToken);
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}