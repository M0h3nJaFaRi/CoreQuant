using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using Common.Presentation.Api.Response;
using Microsoft.AspNetCore.WebUtilities;
using Common.Domain.Exceptions.Abstraction;
using Common.Framework.Exceptions.Abstraction;
using Common.Application.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.Abstraction;
using Common.Presentation.Api.Middlewares.ExceptionHandling.Mapper;

namespace Common.Presentation.Api.Middlewares.ExceptionHandling;
internal static partial class ApiProblemFactory
{
    private const string UnexpectedDetail = "An unexpected error occurred.";
    private const string InternalDetail = "A required service is temporarily unavailable.";

    public static ApiProblemDetails Create(Exception ex, string path, bool isDevelopment) => ex switch
    {
        DomainLayerException e => Domain(e, path),
        ApplicationLayerException e => Application(e, path),
        IntegrationLayerException e => Integration(e, path, isDevelopment),
        InfrastructureLayerException e => Infrastructure(e, path, isDevelopment),
        ServiceException e => Build("service", e.ErrorCode, StatusCodes.Status422UnprocessableEntity, e.Message, path),
        BadHttpRequestException e => Transport(e.StatusCode, "The request is malformed.", path),

        _ => Unhandled(ex, path, isDevelopment)
    };

    private static ApiProblemDetails Domain(DomainLayerException e, string path)
    {
        return Build("domain", e.ErrorCode, ApiProblemMapper.ResolveDomainStatusCode(e.ErrorCode), e.Message, path).Add("value", e.Value)
                                                                                                                   .Add("entityName", e.EntityName)
                                                                                                                   .Add("aggregateName", e.AggregateName);
    }
    private static ApiProblemDetails Application(ApplicationLayerException e, string path)
    {
        return Build("application", e.ErrorCode, ApiProblemMapper.ResolveApplicationStatusCode(e.ErrorCode), e.Message, path);
    }
    private static ApiProblemDetails Integration(IntegrationLayerException e, string path, bool isDev)
    {
        var p = Build("integration", e.ErrorCode, ApiProblemMapper.ResolveIntegrationStatusCode(e.ErrorCode), isDev ? e.Message : InternalDetail, path);

        if (isDev)
        {
            p.Add("provider", e.Provider)
             .Add("providerStatusCode", e.ProviderStatusCode);
        }

        return p;
    }
    private static ApiProblemDetails Infrastructure(InfrastructureLayerException e, string path, bool isDev)
    {
        return Build("infrastructure", e.ErrorCode, ApiProblemMapper.ResolveInfrastructureStatusCode(e.ErrorCode), isDev ? e.Message : InternalDetail, path);
    }
    private static ApiProblemDetails Transport(int status, string detail, string path) => new()
    {
        Type = $"urn:problem:http:{status}",
        Title = ReasonPhrases.GetReasonPhrase(status),
        Status = status,
        Detail = detail,
        Instance = path
    };
    private static ApiProblemDetails Unhandled(Exception ex, string path, bool isDev) => new()
    {
        Type = "urn:problem:internal:unexpected",
        Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError),
        Status = StatusCodes.Status500InternalServerError,
        Detail = isDev ? ex.ToString() : UnexpectedDetail,
        Instance = path
    };

    private static ApiProblemDetails Build(string category, string code, int status, string? detail, string path)
    {
        var words = SplitWords(code);

        var problem = new ApiProblemDetails
        {
            Type = $"urn:problem:{category}:{string.Join('-', words.Select(w => w.ToLowerInvariant()))}",
            Title = string.Join(' ', words.Select(Capitalize)),
            Status = status,
            Detail = detail,
            Instance = path
        };

        problem.Extensions["code"] = code;
        return problem;
    }
    private static ApiProblemDetails Add(this ApiProblemDetails p, string key, object? value)
    {
        if (value is not null)
            p.Extensions[key] = value;

        return p;
    }

    private static string[] SplitWords(string code) => code.Contains('_', StringComparison.Ordinal) ?
                                                       code.Split('_', StringSplitOptions.RemoveEmptyEntries) :
                                                       [.. PascalBoundary().Split(code).Where(s => s.Length > 0)];

    private static string Capitalize(string w) => char.ToUpperInvariant(w[0]) + w[1..].ToLowerInvariant();

    [GeneratedRegex("(?<!^)(?=[A-Z])")]
    private static partial Regex PascalBoundary();
}