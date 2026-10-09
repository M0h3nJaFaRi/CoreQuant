using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Observability;

/// <summary>
/// Readiness probe that verifies Redis responds to <c>PING</c>.
/// </summary>
internal sealed class RedisHealthCheck(IRedisCore redis, IOptionsMonitor<CoreQuantRedisOptions> options) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var health = options.CurrentValue.Health;
        
        if (!health.Enabled)
            return HealthCheckResult.Healthy("Redis health check is disabled.");

        var pingDatabase = health.PingDatabase ?? options.CurrentValue.Cache.Database;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        timeout.CancelAfter(health.Timeout);

        try
        {
            await redis.PingAsync(pingDatabase, timeout.Token).ConfigureAwait(false);
            return HealthCheckResult.Healthy("Redis ping succeeded.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return HealthCheckResult.Unhealthy("Redis ping timed out.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis ping failed.", ex);
        }
    }
}
