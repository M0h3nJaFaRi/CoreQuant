using System.Diagnostics;
using Framework.Configurations.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Framework.HealthChecks;

/// <summary>
/// Template for dependency probes: internal timeout, Degraded on slowness,
/// the registration's FailureStatus on failure, and a short result cache that
/// concurrent probes share. Subclasses only implement the probe itself.
/// </summary>
public abstract class DependencyHealthCheck(DependencyHealthOptions options) : IHealthCheck
{
    private volatile CachedResult? _cached;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly TimeSpan FailureCacheDuration = TimeSpan.FromSeconds(1);
    private sealed record CachedResult(HealthCheckResult Result, long Timestamp, TimeSpan Ttl);

    protected abstract Task ProbeAsync(CancellationToken cancellationToken);

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (TryGetFresh(out var cached))
            return cached;

        // probe های همزمان یک اجرای واقعی رو share می‌کنن
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (TryGetFresh(out cached))
                return cached;

            var result = await RunAsync(context, cancellationToken).ConfigureAwait(false);

            var ttl = result.Status == HealthStatus.Healthy ? options.CacheDuration : FailureCacheDuration;
            _cached = new CachedResult(result, Stopwatch.GetTimestamp(), ttl);

            return result;
        }
        finally
        {
            _gate.Release();
        }
    }
    private async Task<HealthCheckResult> RunAsync(HealthCheckContext context, CancellationToken ct)
    {
        var name = context.Registration.Name;
        var started = Stopwatch.GetTimestamp();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(options.Timeout);

        try
        {
            await ProbeAsync(cts.Token).WaitAsync(options.Timeout, ct)
                                       .ConfigureAwait(false);

            var elapsed = Stopwatch.GetElapsedTime(started);
            var data = new Dictionary<string, object> { ["elapsed_ms"] = (long)elapsed.TotalMilliseconds };

            return elapsed > options.DegradedAfter ? HealthCheckResult.Degraded($"'{name}' responded slowly.", data: data)
                                                   : HealthCheckResult.Healthy($"'{name}' is reachable.", data);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex) when (ex is OperationCanceledException or TimeoutException)
        {
            return Failure(context, $"'{name}' timed out.", exception: null);
        }
        catch (Exception ex)
        {
            return Failure(context, $"'{name}' is unreachable.", ex);
        }
    }

    private bool TryGetFresh(out HealthCheckResult result)
    {
        var c = _cached;
        if (c is not null && Stopwatch.GetElapsedTime(c.Timestamp) < c.Ttl)
        {
            result = c.Result;
            return true;
        }

        result = default;
        return false;
    }
    private static HealthCheckResult Failure(HealthCheckContext context, string description, Exception? exception) 
        => new(context.Registration.FailureStatus, description, exception);
}