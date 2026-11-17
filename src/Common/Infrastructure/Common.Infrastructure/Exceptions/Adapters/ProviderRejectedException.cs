using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when an external provider rejects a request.
/// </summary>
public sealed class ProviderRejectedException(
    string provider,
    string message,
    string? providerStatusCode = null,
    Exception? innerException = null)
    : IntegrationLayerException(provider, IntegrationErrorCode.ProviderRejected, message, providerStatusCode, innerException);
