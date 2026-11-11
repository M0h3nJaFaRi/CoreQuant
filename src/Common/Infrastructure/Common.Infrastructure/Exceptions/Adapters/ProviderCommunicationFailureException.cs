using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when communication with an external provider fails.
/// </summary>
public sealed class ProviderCommunicationFailureException(
    string provider,
    string message,
    string? providerStatusCode = null,
    Exception? innerException = null)
    : IntegrationException(provider, IntegrationErrorCode.ProviderCommunicationFailure, message, providerStatusCode, innerException);
