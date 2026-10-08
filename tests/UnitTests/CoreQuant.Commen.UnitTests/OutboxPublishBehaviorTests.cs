using Common.Application;
using Common.Application.Caching;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Defaults;
using Common.Application.Idempotency;
using Common.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreQuant.Commen.UnitTests;

/// <summary>
/// Proves <c>OutboxPublishBehavior</c> publishes buffered messages after a successful command
/// and never fails the command when publishing fails (rows stay pending for Scheduler/Worker retry).
/// </summary>
public sealed class OutboxPublishBehaviorTests
{
    [Fact]
    public async Task Publishes_Buffered_Messages_And_Marks_Them_Completed()
    {
        var publisher = new RecordingOutboxPublisher();
        var completion = new RecordingOutboxCompletionService();
        await using var provider = CreateServices(publisher, completion).BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        var response = await bus.SendAsync(new BufferedCommand("ok"));

        Assert.Equal("ok", response);
        var published = Assert.Single(publisher.Published);
        Assert.Equal(BufferedCommandHandler.MessageId, published.MessageId);
        Assert.Equal([BufferedCommandHandler.MessageId], completion.Completed);
        Assert.Empty(provider.GetRequiredService<IOutboxPendingBuffer>().Drain());
    }

    [Fact]
    public async Task Publish_Failure_Does_Not_Fail_The_Command_And_Skips_Completion()
    {
        var publisher = new RecordingOutboxPublisher { ThrowOnPublish = true };
        var completion = new RecordingOutboxCompletionService();
        await using var provider = CreateServices(publisher, completion).BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        var response = await bus.SendAsync(new BufferedCommand("ok"));

        Assert.Equal("ok", response);
        Assert.Empty(publisher.Published);
        Assert.Empty(completion.Completed);
    }

    private static ServiceCollection CreateServices(
        RecordingOutboxPublisher publisher,
        RecordingOutboxCompletionService completion)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new System.Diagnostics.Metrics.Meter("tests"));
        services.AddCommonApplication(typeof(OutboxPublishBehaviorTests).Assembly);
        // Unit tests only — production registers Redis via AddCommonInfrastructure.
        services.AddSingleton<IRequestCache, InMemoryRequestCache>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        services.RemoveAll<IOutboxPublisher>();
        services.AddSingleton<IOutboxPublisher>(publisher);
        services.RemoveAll<IOutboxCompletionService>();
        services.AddSingleton<IOutboxCompletionService>(completion);
        return services;
    }

    public sealed record BufferedCommand(string Value) : ICommand<string>;

    public sealed class BufferedCommandHandler : ICommandHandler<BufferedCommand, string>
    {
        public static readonly Guid MessageId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private readonly IOutboxPendingBuffer _pendingBuffer;

        public BufferedCommandHandler(IOutboxPendingBuffer pendingBuffer)
        {
            ArgumentNullException.ThrowIfNull(pendingBuffer);
            _pendingBuffer = pendingBuffer;
        }

        public Task<string> HandleAsync(BufferedCommand command, CancellationToken cancellationToken = default)
        {
            _pendingBuffer.Enqueue(new OutboxPublishRequest
            {
                MessageId = MessageId,
                MessageType = typeof(BufferedCommand).FullName!,
                Payload = "{}",
                OccurredOnUtc = DateTimeOffset.UtcNow,
            });
            return Task.FromResult(command.Value);
        }
    }

    private sealed class RecordingOutboxPublisher : IOutboxPublisher
    {
        public List<OutboxPublishRequest> Published { get; } = [];
        public bool ThrowOnPublish { get; set; }

        public Task PublishAsync(OutboxPublishRequest request, CancellationToken cancellationToken = default)
        {
            if (ThrowOnPublish)
            {
                throw new InvalidOperationException("Simulated broker outage.");
            }

            Published.Add(request);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingOutboxCompletionService : IOutboxCompletionService
    {
        public List<Guid> Completed { get; } = [];

        public Task MarkPublishedAsync(Guid messageId, CancellationToken cancellationToken = default)
        {
            Completed.Add(messageId);
            return Task.CompletedTask;
        }
    }
}
