using Common.Application;
using Common.Application.Caching;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Idempotency;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreQuant.Commen.UnitTests;

public sealed class CommandBusTests
{
    [Fact]
    public async Task SendAsync_Dispatches_Void_Command()
    {
        PingCommandHandler.WasCalled = false;
        await using var provider = BuildProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        await bus.SendAsync(new PingCommand());

        Assert.True(PingCommandHandler.WasCalled);
    }

    [Fact]
    public async Task SendAsync_Returns_Command_Response()
    {
        await using var provider = BuildProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        var result = await bus.SendAsync(new EchoCommand("hi"));

        Assert.Equal("hi", result);
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new System.Diagnostics.Metrics.Meter("tests"));
        services.AddCommonApplication(typeof(CommandBusTests).Assembly);
        services.AddSingleton<IRequestCache, InMemoryRequestCache>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        return services.BuildServiceProvider();
    }

    public sealed record PingCommand : ICommand;

    public sealed class PingCommandHandler : ICommandHandler<PingCommand>
    {
        public static bool WasCalled { get; set; }

        public Task HandleAsync(PingCommand command, CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    public sealed record EchoCommand(string Text) : ICommand<string>;

    public sealed class EchoCommandHandler : ICommandHandler<EchoCommand, string>
    {
        public Task<string> HandleAsync(EchoCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(command.Text);
    }
}
