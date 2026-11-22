using Common.Framework.Exceptions.Abstraction;

namespace Common.Application.Exceptions.Abstraction;

/// <summary>
/// Root for all application-layer categorized exceptions.
/// </summary>
public abstract class ApplicationLayerException(string errorCode, string message, string? handler = null, string? requestType = null, Exception? innerException = null) : 
                      ServiceException(errorCode, message, innerException)
{
    public string? Handler { get; } = handler;
    public string? RequestType { get; } = requestType;
}