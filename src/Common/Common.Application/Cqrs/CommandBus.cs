using Common.Application.Cqrs.Pipeline;
using Common.Application.Cqrs.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Common.Application.Cqrs.Abstractions.Commands;
using Common.Application.Cqrs.Abstractions.Commands.Bus;
using Common.Application.Cqrs.Abstractions.Commands.Handler;

namespace Common.Application.Cqrs;

/// <summary>
/// Default <see cref="ICommandBus"/> resolving handlers and ordered pipeline behaviors from DI.
/// </summary>
internal sealed class CommandBus(IServiceProvider serviceProvider) : ICommandBus
{
    /// <inheritdoc />
    public Task SendAsync(ICommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();

        var method = typeof(CommandBus).GetMethod(nameof(SendVoidAsync), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                                       .MakeGenericMethod(commandType);

        return (Task)method.Invoke(this, [command, cancellationToken])!;
    }

    /// <inheritdoc />
    public Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var commandType = command.GetType();

        var method = typeof(CommandBus).GetMethod(nameof(SendWithResponseAsync), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                                       .MakeGenericMethod(commandType, typeof(TResponse));

        return (Task<TResponse>)method.Invoke(this, [command, cancellationToken])!;
    }

    
    private Task SendVoidAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
    where TCommand : ICommand
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        
        RequestHandlerDelegate<Unit> next = async () =>
        {
            await handler.HandleAsync(command, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        };

        return BuildPipeline(command, next, cancellationToken);
    }
    private Task<TResponse> SendWithResponseAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
    where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        RequestHandlerDelegate<TResponse> next = () => handler.HandleAsync(command, cancellationToken);
        return BuildPipeline(command, next, cancellationToken);
    }
    private Task<TResponse> BuildPipeline<TRequest, TResponse>(TRequest request, RequestHandlerDelegate<TResponse> handler, CancellationToken cancellationToken)
    {
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>()
                                       .OrderByDescending(b => b.Order);

        var next = handler;

        foreach (var behavior in behaviors)
        {
            var captured = behavior;
            var current = next;
            next = () => captured.Handle(request, current, cancellationToken);
        }

        return next();
    }
}