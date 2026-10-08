using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace Common.Application.Cqrs.Behaviors.Outbox;

/// <summary>
/// After a successful command transaction commit, publishes buffered outbox messages.
/// Publish failures do not fail the command — rows stay pending for Scheduler/Worker Job retry.
/// </summary>
internal sealed class OutboxPublishBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IOutboxPendingBuffer _pendingBuffer;
    private readonly IOutboxPublisher _publisher;
    private readonly IOutboxCompletionService _completion;
    private readonly ILogger<OutboxPublishBehavior<TRequest, TResponse>> _logger;

    public OutboxPublishBehavior(
        IOutboxPendingBuffer pendingBuffer,
        IOutboxPublisher publisher,
        IOutboxCompletionService completion,
        ILogger<OutboxPublishBehavior<TRequest, TResponse>> logger)
    {
        ArgumentNullException.ThrowIfNull(pendingBuffer);
        ArgumentNullException.ThrowIfNull(publisher);
        ArgumentNullException.ThrowIfNull(completion);
        ArgumentNullException.ThrowIfNull(logger);

        _pendingBuffer = pendingBuffer;
        _publisher = publisher;
        _completion = completion;
        _logger = logger;
    }

    /// <inheritdoc />
    public int Order => PipelineOrders.OutboxPublish;

    /// <inheritdoc />
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!IsCommand(request))
        {
            return await next().ConfigureAwait(false);
        }

        var response = await next().ConfigureAwait(false);

        var pending = _pendingBuffer.Drain();
        if (pending.Count == 0)
        {
            return response;
        }

        foreach (var message in pending)
        {
            try
            {
                await _publisher.PublishAsync(message, cancellationToken).ConfigureAwait(false);
                await _completion.MarkPublishedAsync(message.MessageId, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Outbox publish failed for {MessageId} ({MessageType}). Left pending for Scheduler retry.",
                    message.MessageId,
                    message.MessageType);
            }
        }

        return response;
    }

    private static bool IsCommand(TRequest request)
    {
        if (request is ICommand)
        {
            return true;
        }

        var type = typeof(TRequest);
        return type.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }
}
