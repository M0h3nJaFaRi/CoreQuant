namespace Common.Infrastructure.Exceptions;

/// <summary>
/// Standard integration adapter error codes.
/// </summary>
public static class IntegrationErrorCode
{
    public const string ProviderRejected = "PROVIDER_REJECTED";
    public const string ProviderTimeout = "PROVIDER_TIMEOUT";
    public const string ProviderUnavailable = "PROVIDER_UNAVAILABLE";
    public const string ProviderCommunicationFailure = "PROVIDER_COMMUNICATION_FAILURE";
    public const string UnexpectedProviderResponse = "UNEXPECTED_PROVIDER_RESPONSE";
}
