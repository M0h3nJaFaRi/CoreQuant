using System.Buffers;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Common.Presentation.Api.Extensions;

namespace Common.Presentation.Api.Middlewares.CorrelationHandling;
public sealed class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger) : IMiddleware
{
    private const int MaxLength = 64;
    public const string HeaderName = "X-Correlation-ID";
    private static readonly SearchValues<char> AllowedChars = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_");

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var incoming = context.Request.Headers[HeaderName].FirstOrDefault();

        // Keep the caller's id when it is safe, otherwise generate one.
        var correlationId = IsValid(incoming) ? incoming! : Guid.NewGuid().ToString("N");

        context.SetCorrelationId(correlationId);
        context.Response.Headers[HeaderName] = correlationId;

        // Tag the request's server Activity (not baggage: baggage would be propagated to downstream services).
        Activity.Current?.SetTag("correlation_id", correlationId);

        // Every log written while the request is in flight carries CorrelationId.
        using var scope = logger.BeginScope(new[]
        {
            new KeyValuePair<string, object>("CorrelationId", correlationId)
        });

        await next(context);
    }

    // 1..64 chars of [A-Za-z0-9-_]: protects logs and headers from injection (CR/LF, huge values, markup).
    private static bool IsValid(string? value) => !string.IsNullOrEmpty(value) && value.Length <= MaxLength && value.AsSpan().IndexOfAnyExcept(AllowedChars) < 0;
}