using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when an external provider is unavailable.
/// </summary>
public sealed class ProviderUnavailableException(
    string provider,
    string message,
    string? providerStatusCode = null,
    Exception? innerException = null)
    : IntegrationLayerException(provider, IntegrationErrorCode.ProviderUnavailable, message, providerStatusCode, innerException);
