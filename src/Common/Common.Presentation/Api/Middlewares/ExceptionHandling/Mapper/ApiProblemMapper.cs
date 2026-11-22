using System.Net;
using System.Collections.Frozen;

namespace Common.Presentation.Api.Middlewares.ExceptionHandling.Mapper;
internal static class ApiProblemMapper
{
    public static int ResolveDomainStatusCode(string errorCode) => (int)DomainStatusMap.GetValueOrDefault(errorCode, HttpStatusCode.UnprocessableEntity);
    public static int ResolveIntegrationStatusCode(string errorCode) => (int)IntegrationStatusMap.GetValueOrDefault(errorCode, HttpStatusCode.BadGateway);
    public static int ResolveApplicationStatusCode(string errorCode) => (int)ApplicationStatusMap.GetValueOrDefault(errorCode, HttpStatusCode.UnprocessableEntity);
    public static int ResolveInfrastructureStatusCode(string errorCode) => (int)InfrastructureStatusMap.GetValueOrDefault(errorCode, HttpStatusCode.ServiceUnavailable);

    private static readonly FrozenDictionary<string, HttpStatusCode> DomainStatusMap = new Dictionary<string, HttpStatusCode>
    {
        ["NotFound"] = HttpStatusCode.NotFound,
        ["AlreadyExists"] = HttpStatusCode.Conflict,
        ["Duplicate"] = HttpStatusCode.Conflict,
        ["InvalidState"] = HttpStatusCode.UnprocessableEntity,
        ["InvalidStateTransition"] = HttpStatusCode.UnprocessableEntity,
        ["AlreadyProcessed"] = HttpStatusCode.Conflict,
        ["Expired"] = HttpStatusCode.UnprocessableEntity,
        ["InvariantViolation"] = HttpStatusCode.UnprocessableEntity,
        ["ReferenceNotFound"] = HttpStatusCode.UnprocessableEntity,
        ["IntegrityViolation"] = HttpStatusCode.Conflict,
        ["ConcurrencyConflict"] = HttpStatusCode.Conflict,
        ["StaleVersion"] = HttpStatusCode.Conflict
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, HttpStatusCode> ApplicationStatusMap = new Dictionary<string, HttpStatusCode>
    {
        ["ValidationFailed"] = HttpStatusCode.BadRequest,
        ["OperationNotAllowed"] = HttpStatusCode.Forbidden,
        ["LimitExceeded"] = HttpStatusCode.TooManyRequests,
        ["InsufficientBalance"] = HttpStatusCode.UnprocessableEntity,
        ["InsufficientQuantity"] = HttpStatusCode.UnprocessableEntity,
        ["BusinessRuleViolation"] = HttpStatusCode.UnprocessableEntity
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, HttpStatusCode> IntegrationStatusMap = new Dictionary<string, HttpStatusCode>
    {
        ["ProviderRejected"] = HttpStatusCode.BadGateway,
        ["ProviderTimeout"] = HttpStatusCode.GatewayTimeout,
        ["ProviderUnavailable"] = HttpStatusCode.BadGateway,
        ["ProviderCommunicationFailure"] = HttpStatusCode.BadGateway,
        ["UnexpectedProviderResponse"] = HttpStatusCode.BadGateway
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, HttpStatusCode> InfrastructureStatusMap = new Dictionary<string, HttpStatusCode>
    {
        ["DatabaseFailure"] = HttpStatusCode.ServiceUnavailable,
        ["DatabaseUnavailable"] = HttpStatusCode.ServiceUnavailable,
        ["CacheFailure"] = HttpStatusCode.ServiceUnavailable,
        ["CacheUnavailable"] = HttpStatusCode.ServiceUnavailable,
        ["StorageFailure"] = HttpStatusCode.ServiceUnavailable,
        ["MessageBrokerFailure"] = HttpStatusCode.ServiceUnavailable,
        ["UnexpectedInfrastructureFailure"] = HttpStatusCode.ServiceUnavailable
    }.ToFrozenDictionary();
}