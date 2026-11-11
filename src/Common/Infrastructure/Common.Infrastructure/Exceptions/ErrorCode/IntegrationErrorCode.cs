namespace Common.Infrastructure.Exceptions.ErrorCode;

internal static class IntegrationErrorCode
{
    public const string ProviderRejected = "ProviderRejected";
    public const string ProviderTimeout = "ProviderTimeout";
    public const string ProviderUnavailable = "ProviderUnavailable";
    public const string ProviderCommunicationFailure = "ProviderCommunicationFailure";
    public const string UnexpectedProviderResponse = "UnexpectedProviderResponse";
}
