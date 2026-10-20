namespace Common.Application.Cqrs.Pipeline;

/// <summary>
/// Cross-cutting behavior around command/query handling.
/// Lower <see cref="Order"/> values run farther from the handler (outer).
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Execution order. Lower values wrap outer stages of the pipeline.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Invokes the next behavior or the handler.
    /// </summary>
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}