namespace Common.Infrastructure.Exceptions;

/// <summary>
/// Standard infrastructure-layer error codes.
/// </summary>
public static class InfrastructureErrorCode
{
    public const string DatabaseFailure = "DATABASE_FAILURE";
    public const string DatabaseUnavailable = "DATABASE_UNAVAILABLE";
    public const string CacheFailure = "CACHE_FAILURE";
    public const string CacheUnavailable = "CACHE_UNAVAILABLE";
    public const string StorageFailure = "STORAGE_FAILURE";
    public const string MessageBrokerFailure = "MESSAGE_BROKER_FAILURE";
    public const string UnexpectedInfrastructureFailure = "UNEXPECTED_INFRASTRUCTURE_FAILURE";
}
