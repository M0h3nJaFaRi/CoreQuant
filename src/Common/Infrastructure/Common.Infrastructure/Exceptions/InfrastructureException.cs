using Common.Framework.Exceptions;

namespace Common.Infrastructure.Exceptions;

/// <summary>
/// Base for infrastructure failures (database, cache, messaging, storage).
/// </summary>
public abstract class InfrastructureException : Exception, IHasErrorCode
{
    protected InfrastructureException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code.Trim();
    }

    public string Code { get; }
}
