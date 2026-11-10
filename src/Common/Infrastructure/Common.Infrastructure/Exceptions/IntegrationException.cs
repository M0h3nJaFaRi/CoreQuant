using Common.Framework.Exceptions;

namespace Common.Infrastructure.Exceptions;

/// <summary>
/// Thrown when an external provider or integration adapter fails.
/// </summary>
public sealed class IntegrationException : Exception, IHasErrorCode
{
    public IntegrationException(
        string provider,
        string code,
        string message,
        string? providerStatusCode = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Provider = provider;
        Code = code.Trim();
        ProviderStatusCode = providerStatusCode;
    }

    public string Provider { get; }

    public string Code { get; }

    public string? ProviderStatusCode { get; }
}
