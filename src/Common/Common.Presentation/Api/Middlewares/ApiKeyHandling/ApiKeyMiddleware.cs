using Common.Presentation.Api.Configuration.Options;
using Common.Presentation.Api.Extensions;
using Common.Presentation.Api.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Common.Presentation.Api.Middlewares.ApiKeyHandling;
public sealed partial class ApiKeyMiddleware(ApiKeyValidator validator,
                                             ILogger<ApiKeyMiddleware> logger,
                                             IOptionsMonitor<ApiKeyOptions> optionsMonitor) : IMiddleware
{
    public const string AuthenticationType = "ApiKey";
    public const string ClientNameClaimType = "client_name";
    public const string ClientNameItemKey = "ApiKeyClientName";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var options = optionsMonitor.CurrentValue;

        if (PathMatcher.IsExcluded(context.Request.Path, options.ExcludedPaths))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(options.HeaderName, out var values) || values.Count == 0)
        {
            await RejectAsync(context, "missing_key");
            return;
        }

        if (values.Count > 1)
        {
            await RejectAsync(context, "multiple_keys");
            return;
        }

        if (!validator.TryValidate(options, values[0], out var clientName))
        {
            await RejectAsync(context, "invalid_key");
            return;
        }

        context.Items[ClientNameItemKey] = clientName;
        context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClientNameClaimType, clientName)], AuthenticationType, nameType: ClientNameClaimType, roleType: null));

        // Every log written further down the pipeline carries ClientName.
        using (logger.BeginScope(new Dictionary<string, object> { ["ClientName"] = clientName }))
        {
            await next(context);
        }
    }

    private Task RejectAsync(HttpContext context, string reason)
    {
        logger.LogWarning("API key rejected ({Reason}) for {Method} {Path} from {RemoteIp}.",
                          reason, context.Request.Method, context.Request.Path.Value,
                          context.Connection.RemoteIpAddress?.ToString());

        context.Response.Headers.WWWAuthenticate = "ApiKey realm=\"api\"";

        return context.WriteProblemAsync(new ApiProblemDetails
        {
            Type = "urn:error:unauthorized",
            Title = "Unauthorized",
            Status = StatusCodes.Status401Unauthorized,
            Detail = "A valid API key is required.",
            Instance = context.Request.Path.Value
        });
    }
}