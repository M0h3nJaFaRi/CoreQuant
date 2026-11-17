namespace Common.Framework.Exceptions.Abstraction;

/// <summary>
/// Root for service-layer categorized exceptions across domain, application, and infrastructure.
/// </summary>
public abstract class ServiceException(string errorCode, string message, Exception? innerException = null) : Exception(message, innerException)
{
    public string ErrorCode { get; } = errorCode.Trim();
}