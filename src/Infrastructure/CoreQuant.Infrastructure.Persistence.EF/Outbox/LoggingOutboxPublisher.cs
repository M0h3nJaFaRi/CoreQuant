using Common.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace CoreQuant.Infrastructure.Persistence.EF.Outbox;

/// <summary>
/// Development/default publisher that logs the outbox message. Replace with a broker adapter in production.
/// </summary>
internal sealed class LoggingOutboxPublisher : IOutboxPublisher
{
    private readonly ILogger<LoggingOutboxPublisher> _logger;

    public LoggingOutboxPublisher(ILogger<LoggingOutboxPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <inheritdoc />
    public Task PublishAsync(OutboxPublishRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Outbox publish MessageId={MessageId} Type={MessageType} Schema={SchemaVersion} CorrelationId={CorrelationId}",
            request.MessageId,
            request.MessageType,
            request.SchemaVersion,
            request.CorrelationId);

        return Task.CompletedTask;
    }
}
