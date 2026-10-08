using Common.Application.Outbox;
using Microsoft.Extensions.Options;
using Common.Application.Configuration.Options;

namespace CoreQuant.Scheduler;

/// <summary>
/// Periodically invokes <see cref="IOutboxRetryJob"/> (Scheduler trigger — not an outbox BackgroundService dispatcher).
/// </summary>
internal sealed class OutboxRetrySchedulerHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxRetrySchedulerHostedService> logger) : BackgroundService
{
    private readonly OutboxOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.SchedulerIntervalSeconds));
        using var timer = new PeriodicTimer(interval);

        logger.LogInformation(
            "Outbox retry scheduler started. Interval={IntervalSeconds}s",
            interval.TotalSeconds);

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var job = scope.ServiceProvider.GetRequiredService<IOutboxRetryJob>();
                var result = await job.ExecuteAsync(stoppingToken).ConfigureAwait(false);

                if (!result.SkippedNoLease && result.Attempted > 0)
                    logger.LogInformation(
                        "Outbox retry tick Attempted={Attempted} Published={Published} Failed={Failed}",
                        result.Attempted,
                        result.Published,
                        result.Failed);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox retry scheduler tick failed.");
            }
        }
    }
}
