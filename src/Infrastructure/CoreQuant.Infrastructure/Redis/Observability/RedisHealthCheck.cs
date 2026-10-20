using Common.Application.Cqrs.Abstractions;
using CoreQuant.Infrastructure.Redis.Configuration.Options;
using CoreQuant.Infrastructure.Redis.Core;
using Framework.Configurations.Options;
using Framework.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace CoreQuant.Infrastructure.Redis.Observability;

/// <summary>
/// Readiness probe that verifies Redis responds to <c>PING</c>.
/// </summary>
internal sealed class RedisHealthCheck(IRedisCore redis, DependencyHealthOptions options) : DependencyHealthCheck(options)
{
    protected override Task ProbeAsync(CancellationToken cancellationToken) => redis.PingAsync((uint)Random.Shared.Next(0, 16), cancellationToken);
}