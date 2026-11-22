using Common.Framework.Exceptions.Abstraction;

namespace Common.Infrastructure.Exceptions.Abstraction;

/// <summary>
/// Base for integration adapter failures.
/// </summary>
public abstract class IntegrationLayerException(string provider, string errorCode, string message, string? providerStatusCode = null, Exception? innerException = null) : 
                      ServiceException(errorCode: errorCode, message: message, innerException: innerException)
{
    public string Provider { get; } = provider;
    public string? ProviderStatusCode { get; } = providerStatusCode;
}