using Common.Framework.Exceptions;

namespace Common.Infrastructure.Exceptions.Abstraction;

/// <summary>
/// Base for infrastructure failures (database, cache, messaging, storage).
/// </summary>
public abstract class InfrastructureException(string code, string message, Exception? innerException = null)
    : Exception(message, innerException), IHasErrorCode
{
    public string Code { get; } = code.Trim();
}
