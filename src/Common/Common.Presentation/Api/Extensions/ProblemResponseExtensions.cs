using System.Text.Json;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Common.Presentation.Api.Response;

namespace Common.Presentation.Api.Extensions;
public static class ProblemResponseExtensions
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Single place that writes application/problem+json. Adds traceId and correlationId,
    /// so every middleware (Exception, Maintenance, ApiKey, ...) produces the same shape.
    /// </summary>
    public static Task WriteProblemAsync(this HttpContext context, ApiProblemDetails problem)
    {
        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        
        if (context.GetCorrelationId() is { } correlationId)
            problem.Extensions["correlationId"] = correlationId;

        context.Response.StatusCode = problem.Status;
        
        return context.Response.WriteAsJsonAsync(ApiResponse.Fail(problem), Json, "application/problem+json");
    }
}