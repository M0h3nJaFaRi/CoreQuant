namespace Common.Application.Cqrs.Pipeline;

/// <summary>
/// Continuation that invokes the next pipeline behavior or the handler.
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();