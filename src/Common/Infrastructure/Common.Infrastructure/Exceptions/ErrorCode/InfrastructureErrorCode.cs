namespace Common.Infrastructure.Exceptions.ErrorCode;

internal static class InfrastructureErrorCode
{
    public const string DatabaseFailure = "DatabaseFailure";
    public const string DatabaseUnavailable = "DatabaseUnavailable";
    public const string CacheFailure = "CacheFailure";
    public const string CacheUnavailable = "CacheUnavailable";
    public const string StorageFailure = "StorageFailure";
    public const string MessageBrokerFailure = "MessageBrokerFailure";
    public const string UnexpectedInfrastructureFailure = "UnexpectedInfrastructureFailure";
}
