using System.Collections.Frozen;
using System.Net;
using System.Reflection;
using Common.Domain.Exceptions;
using Common.Framework.Exceptions;
using Common.Infrastructure.Exceptions;
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
        var words = code.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(' ', words.Select(w => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant()));
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
        [DomainErrorCode.NotFound] = HttpStatusCode.NotFound,
        [DomainErrorCode.AlreadyExists] = HttpStatusCode.Conflict,
        [DomainErrorCode.Duplicate] = HttpStatusCode.Conflict,
        [DomainErrorCode.InvalidState] = HttpStatusCode.UnprocessableEntity,
        [DomainErrorCode.InvalidStateTransition] = HttpStatusCode.UnprocessableEntity,
        [DomainErrorCode.AlreadyProcessed] = HttpStatusCode.Conflict,
        [DomainErrorCode.Expired] = HttpStatusCode.UnprocessableEntity,
        [DomainErrorCode.InvariantViolation] = HttpStatusCode.UnprocessableEntity,
        [DomainErrorCode.ReferenceNotFound] = HttpStatusCode.UnprocessableEntity,
        [DomainErrorCode.IntegrityViolation] = HttpStatusCode.Conflict,
        [DomainErrorCode.ConcurrencyConflict] = HttpStatusCode.Conflict,
        [DomainErrorCode.StaleVersion] = HttpStatusCode.Conflict
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, HttpStatusCode> ApplicationStatusMap = new Dictionary<string, HttpStatusCode>
    {
        ["VALIDATION_FAILED"] = HttpStatusCode.BadRequest,
        ["OPERATION_NOT_ALLOWED"] = HttpStatusCode.Forbidden,
        ["LIMIT_EXCEEDED"] = HttpStatusCode.TooManyRequests,
        ["INSUFFICIENT_BALANCE"] = HttpStatusCode.UnprocessableEntity,
        ["INSUFFICIENT_QUANTITY"] = HttpStatusCode.UnprocessableEntity,
        ["BUSINESS_RULE_VIOLATION"] = HttpStatusCode.UnprocessableEntity
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, HttpStatusCode> IntegrationStatusMap = new Dictionary<string, HttpStatusCode>
    {
        [IntegrationErrorCode.ProviderRejected] = HttpStatusCode.BadGateway,
        [IntegrationErrorCode.ProviderTimeout] = HttpStatusCode.GatewayTimeout,
        [IntegrationErrorCode.ProviderUnavailable] = HttpStatusCode.BadGateway,
        [IntegrationErrorCode.ProviderCommunicationFailure] = HttpStatusCode.BadGateway,
        [IntegrationErrorCode.UnexpectedProviderResponse] = HttpStatusCode.BadGateway
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, HttpStatusCode> InfrastructureStatusMap = new Dictionary<string, HttpStatusCode>
    {
        [InfrastructureErrorCode.DatabaseFailure] = HttpStatusCode.ServiceUnavailable,
        [InfrastructureErrorCode.DatabaseUnavailable] = HttpStatusCode.ServiceUnavailable,
        [InfrastructureErrorCode.CacheFailure] = HttpStatusCode.ServiceUnavailable,
        [InfrastructureErrorCode.CacheUnavailable] = HttpStatusCode.ServiceUnavailable,
        [InfrastructureErrorCode.StorageFailure] = HttpStatusCode.ServiceUnavailable,
        [InfrastructureErrorCode.MessageBrokerFailure] = HttpStatusCode.ServiceUnavailable,
        [InfrastructureErrorCode.UnexpectedInfrastructureFailure] = HttpStatusCode.ServiceUnavailable
    }.ToFrozenDictionary();
}
