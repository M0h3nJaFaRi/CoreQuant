using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when an external provider call times out.
/// </summary>
public sealed class ProviderTimeoutException(
    string provider,
    string message,
    string? providerStatusCode = null,
    Exception? innerException = null)
    : IntegrationException(provider, IntegrationErrorCode.ProviderTimeout, message, providerStatusCode, innerException);
