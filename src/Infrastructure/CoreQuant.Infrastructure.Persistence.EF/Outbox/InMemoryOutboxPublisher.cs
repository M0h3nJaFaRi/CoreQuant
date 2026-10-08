using System.Threading.Channels;
using Common.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace CoreQuant.Infrastructure.Persistence.EF.Outbox;

/// <summary>
/// Process-local outbox transport stand-in until a real broker adapter is wired.
/// Successful write to the channel counts as published (at-least-once consumers still apply later).
/// </summary>
internal sealed class InMemoryOutboxPublisher : IOutboxPublisher
{
    private readonly Channel<OutboxPublishRequest> _channel;
    private readonly ILogger<InMemoryOutboxPublisher> _logger;

    public InMemoryOutboxPublisher(ILogger<InMemoryOutboxPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
        _channel = Channel.CreateUnbounded<OutboxPublishRequest>(new UnboundedChannelOptions
        {
            SingleReader = false,
            SingleWriter = false,
        });
    }

    /// <summary>
    /// Reader for diagnostics and future in-process consumers.
    /// </summary>
    public ChannelReader<OutboxPublishRequest> Reader => _channel.Reader;

    /// <inheritdoc />
    public async Task PublishAsync(OutboxPublishRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await _channel.Writer.WriteAsync(request, cancellationToken).ConfigureAwait(false);

        _logger.LogDebug(
            "InMemory outbox published MessageId={MessageId} Type={MessageType}",
            request.MessageId,
            request.MessageType);
    }
}
