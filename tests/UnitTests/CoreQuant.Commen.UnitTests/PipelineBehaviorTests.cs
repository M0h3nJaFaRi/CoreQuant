using Common.Application;
using Common.Application.Authorization;
using Common.Application.Caching;
using Common.Application.Cqrs.Abstractions;
using Common.Application.Cqrs.Defaults;
using Common.Application.Cqrs.Pipeline;
using Common.Application.Exceptions;
using Common.Application.Idempotency;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace CoreQuant.Commen.UnitTests;

public sealed class PipelineBehaviorTests
{
    [Fact]
    public async Task ValidationBehavior_Stops_On_Invalid_Command()
    {
        var services = CreateBaseServices();
        services.AddTransient<IValidator<ValidatedCommand>, ValidatedCommandValidator>();
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        var ex = await Assert.ThrowsAsync<Common.Application.Exceptions.ValidationException>(
            () => bus.SendAsync(new ValidatedCommand("")));

        Assert.NotEmpty(ex.Failures);
        Assert.Equal(0, ValidatedCommandHandler.CallCount);
    }

    [Fact]
    public async Task CachingBehavior_Returns_Cached_Value_On_Hit()
    {
        var services = CreateBaseServices();
        await using var provider = services.BuildServiceProvider();
        var queryBus = provider.GetRequiredService<IQueryBus>();

        var first = await queryBus.SendAsync(new CachedQuery("k1"));
        var second = await queryBus.SendAsync(new CachedQuery("k1"));

        Assert.Equal("k1:1", first);
        Assert.Equal("k1:1", second);
        Assert.Equal(1, CachedQueryHandler.CallCount);
    }

    [Fact]
    public async Task CachingBehavior_Misses_For_Different_Keys()
    {
        CachedQueryHandler.CallCount = 0;
        var services = CreateBaseServices();
        await using var provider = services.BuildServiceProvider();
        var queryBus = provider.GetRequiredService<IQueryBus>();

        await queryBus.SendAsync(new CachedQuery("a"));
        await queryBus.SendAsync(new CachedQuery("b"));

        Assert.Equal(2, CachedQueryHandler.CallCount);
    }

    [Fact]
    public async Task IdempotencyBehavior_Skips_Handler_On_Duplicate_Key()
    {
        IdempotentCommandHandler.CallCount = 0;
        var services = CreateBaseServices();
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        var first = await bus.SendAsync(new IdempotentCommand("idem-1", "payload"));
        var second = await bus.SendAsync(new IdempotentCommand("idem-1", "payload"));

        Assert.Equal("payload", first);
        Assert.Equal("payload", second);
        Assert.Equal(1, IdempotentCommandHandler.CallCount);
    }

    [Fact]
    public async Task IdempotencyBehavior_Runs_When_Marker_Absent()
    {
        PlainCommandHandler.CallCount = 0;
        var services = CreateBaseServices();
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        await bus.SendAsync(new PlainCommand("x"));
        await bus.SendAsync(new PlainCommand("x"));

        Assert.Equal(2, PlainCommandHandler.CallCount);
    }

    [Fact]
    public async Task AuthorizationBehavior_Throws_When_Denied()
    {
        var services = CreateBaseServices();
        services.RemoveAll<IAuthorizationService>();
        services.AddSingleton<IAuthorizationService, DenyAllAuthorizationService>();
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        await Assert.ThrowsAsync<ForbiddenException>(() => bus.SendAsync(new PlainCommand("x")));
    }

    [Fact]
    public async Task Pipeline_Executes_Behaviors_In_Deterministic_Order()
    {
        var trace = new List<int>();
        var services = CreateBaseServices();
        services.RemoveAll(typeof(IPipelineBehavior<,>));
        services.AddTransient<IPipelineBehavior<OrderProbeCommand, string>>(_ => new OrderProbeBehavior(10, trace));
        services.AddTransient<IPipelineBehavior<OrderProbeCommand, string>>(_ => new OrderProbeBehavior(30, trace));
        services.AddTransient<IPipelineBehavior<OrderProbeCommand, string>>(_ => new OrderProbeBehavior(20, trace));
        await using var provider = services.BuildServiceProvider();
        var bus = provider.GetRequiredService<ICommandBus>();

        await bus.SendAsync(new OrderProbeCommand());

        Assert.Equal([10, 20, 30], trace);
    }

    private static ServiceCollection CreateBaseServices()
    {
        ValidatedCommandHandler.CallCount = 0;
        CachedQueryHandler.CallCount = 0;
        IdempotentCommandHandler.CallCount = 0;
        PlainCommandHandler.CallCount = 0;

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new System.Diagnostics.Metrics.Meter("tests"));
        services.AddCommonApplication(typeof(PipelineBehaviorTests).Assembly);
        // Unit tests only — production registers Redis via AddCommonInfrastructure.
        services.AddSingleton<IRequestCache, InMemoryRequestCache>();
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
        return services;
    }

    public sealed record ValidatedCommand(string Name) : ICommand<string>;

    public sealed class ValidatedCommandValidator : AbstractValidator<ValidatedCommand>
    {
        public ValidatedCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public sealed class ValidatedCommandHandler : ICommandHandler<ValidatedCommand, string>
    {
        public static int CallCount { get; set; }

        public Task<string> HandleAsync(ValidatedCommand command, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(command.Name);
        }
    }

    public sealed record CachedQuery(string Key) : IQuery<string>, ICacheableQuery<string>
    {
        public string CacheKey => Key;
        public TimeSpan? CacheDuration => TimeSpan.FromMinutes(5);
    }

    public sealed class CachedQueryHandler : IQueryHandler<CachedQuery, string>
    {
        public static int CallCount { get; set; }

        public Task<string> HandleAsync(CachedQuery query, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult($"{query.Key}:{CallCount}");
        }
    }

    public sealed record IdempotentCommand(string IdempotencyKey, string Payload)
        : ICommand<string>, IIdempotentCommand<string>;

    public sealed class IdempotentCommandHandler : ICommandHandler<IdempotentCommand, string>
    {
        public static int CallCount { get; set; }

        public Task<string> HandleAsync(IdempotentCommand command, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(command.Payload);
        }
    }

    public sealed record PlainCommand(string Value) : ICommand<string>;

    public sealed class PlainCommandHandler : ICommandHandler<PlainCommand, string>
    {
        public static int CallCount { get; set; }

        public Task<string> HandleAsync(PlainCommand command, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult(command.Value);
        }
    }

    public sealed record OrderProbeCommand : ICommand<string>;

    public sealed class OrderProbeCommandHandler : ICommandHandler<OrderProbeCommand, string>
    {
        public Task<string> HandleAsync(OrderProbeCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult("ok");
    }

    private sealed class OrderProbeBehavior : IPipelineBehavior<OrderProbeCommand, string>
    {
        private readonly List<int> _trace;

        public OrderProbeBehavior(int order, List<int> trace)
        {
            Order = order;
            _trace = trace;
        }

        public int Order { get; }

        public async Task<string> Handle(
            OrderProbeCommand request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            _trace.Add(Order);
            return await next().ConfigureAwait(false);
        }
    }

    private sealed class DenyAllAuthorizationService : IAuthorizationService
    {
        public Task<bool> AuthorizeAsync(Type requestType, CancellationToken cancellationToken = default)
            => Task.FromResult(false);
    }
}
