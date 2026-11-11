using System.Collections.Frozen;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Common.Domain.Exceptions.Abstraction;
using Common.Framework.Exceptions;
using Common.Infrastructure.Exceptions.Abstraction;
using Common.Presentation.Api.Response;
using Microsoft.AspNetCore.Http;

namespace Common.Presentation.Api.Extensions;

/// <summary>
/// Maps categorized exceptions to RFC 7807 problem responses and HTTP status codes.
/// </summary>
public static class ExceptionErrorMapper
{
    private const string ApplicationExceptionsNamespace = "Common.Application.Exceptions";

    public static int GetStatusCode(Exception exception) => exception switch
    {
        DomainException domain => (int)DomainStatusMap.GetValueOrDefault(domain.Code, HttpStatusCode.UnprocessableEntity),
        IntegrationException integration => (int)IntegrationStatusMap.GetValueOrDefault(integration.Code, HttpStatusCode.BadGateway),
        InfrastructureException infrastructure => (int)InfrastructureStatusMap.GetValueOrDefault(infrastructure.Code, HttpStatusCode.ServiceUnavailable),
        IHasErrorCode coded when IsApplicationException(exception) =>
            (int)ApplicationStatusMap.GetValueOrDefault(coded.Code, HttpStatusCode.UnprocessableEntity),
        _ => StatusCodes.Status500InternalServerError
    };

    public static bool IsClientError(Exception exception) => GetStatusCode(exception) < StatusCodes.Status500InternalServerError;

    public static ApiProblemDetails? TryMap(Exception exception, string? instance = null)
    {
        return exception switch
        {
            DomainException domain when IsClientError(domain) => ToDomainProblem(domain, instance),
            IntegrationException integration when IsClientError(integration) => ToIntegrationProblem(integration, instance),
            InfrastructureException infrastructure when IsClientError(infrastructure) => ToInfrastructureProblem(infrastructure, instance),
            _ when exception is IHasErrorCode coded && IsApplicationException(exception) && IsClientError(exception) =>
                ToApplicationProblem(exception, coded, instance),
            _ => null
        };
    }

    public static string GetErrorType(Exception exception) =>
        exception is IHasErrorCode coded ? coded.Code : exception.GetType().Name;

    public static string Humanize(string code)
    {
        if (code.Contains('_', StringComparison.Ordinal))
        {
            var words = code.Split('_', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(' ', words.Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
        }

        return string.Join(' ', Regex.Split(code, "(?<!^)(?=[A-Z])").Where(p => p.Length > 0));
    }

    private static bool IsApplicationException(Exception exception) =>
        exception.GetType().Namespace?.StartsWith(ApplicationExceptionsNamespace, StringComparison.Ordinal) == true;

    private static ApiProblemDetails ToDomainProblem(DomainException exception, string? instance)
    {
        var problem = new ApiProblemDetails
        {
            Type = $"urn:domain-error:{exception.Code}",
            Title = Humanize(exception.Code),
            Status = GetStatusCode(exception),
            Detail = exception.Message,
            Instance = instance
        };

        problem.Extensions["code"] = exception.Code;

        if (exception.EntityName is not null)
            problem.Extensions["entityName"] = exception.EntityName;

        if (exception.AggregateName is not null)
            problem.Extensions["aggregateName"] = exception.AggregateName;

        if (exception.Value is not null)
            problem.Extensions["value"] = exception.Value;

        return problem;
    }

    private static ApiProblemDetails ToApplicationProblem(Exception exception, IHasErrorCode coded, string? instance)
    {
        var problem = new ApiProblemDetails
        {
            Type = $"urn:application-error:{coded.Code}",
            Title = Humanize(coded.Code),
            Status = GetStatusCode(exception),
            Detail = exception.Message,
            Instance = instance
        };

        problem.Extensions["code"] = coded.Code;

        if (string.Equals(exception.GetType().Name, "ValidationException", StringComparison.Ordinal))
        {
            var errors = exception.GetType().GetProperty("Errors", BindingFlags.Instance | BindingFlags.Public)?.GetValue(exception);
            if (errors is not null)
                problem.Extensions["errors"] = errors;
        }

        var retryAfter = exception.GetType().GetProperty("RetryAfter", BindingFlags.Instance | BindingFlags.Public)?.GetValue(exception);
        if (retryAfter is TimeSpan retry)
            problem.Extensions["retryAfter"] = retry.TotalSeconds;

        var reason = exception.GetType().GetProperty("Reason", BindingFlags.Instance | BindingFlags.Public)?.GetValue(exception) as string;
        if (!string.IsNullOrWhiteSpace(reason))
            problem.Extensions["reason"] = reason;

        return problem;
    }

    private static ApiProblemDetails ToIntegrationProblem(IntegrationException exception, string? instance)
    {
        var problem = new ApiProblemDetails
        {
            Type = $"urn:integration-error:{exception.Code}",
            Title = Humanize(exception.Code),
            Status = GetStatusCode(exception),
            Detail = exception.Message,
            Instance = instance
        };

        problem.Extensions["code"] = exception.Code;
        problem.Extensions["provider"] = exception.Provider;

        if (exception.ProviderStatusCode is not null)
            problem.Extensions["providerStatusCode"] = exception.ProviderStatusCode;

        return problem;
    }

    private static ApiProblemDetails ToInfrastructureProblem(InfrastructureException exception, string? instance)
    {
        var problem = new ApiProblemDetails
        {
            Type = $"urn:infrastructure-error:{exception.Code}",
            Title = Humanize(exception.Code),
            Status = GetStatusCode(exception),
            Detail = exception.Message,
            Instance = instance
        };

        problem.Extensions["code"] = exception.Code;
        return problem;
    }

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
