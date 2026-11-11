using Common.Framework.Exceptions;

namespace Common.Infrastructure.Exceptions.Abstraction;

/// <summary>
/// Base for integration adapter failures.
/// </summary>
public abstract class IntegrationException(
    string provider,
    string code,
    string message,
    string? providerStatusCode = null,
    Exception? innerException = null)
    : Exception(message, innerException), IHasErrorCode
{
    public string Provider { get; } = provider;

    public string Code { get; } = code.Trim();

    public string? ProviderStatusCode { get; } = providerStatusCode;
}
