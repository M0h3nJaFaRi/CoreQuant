using System.Text.Json;
using System.Diagnostics;
using Common.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Diagnostics;
using Common.Presentation.Api.Response;
using Microsoft.AspNetCore.WebUtilities;
using Common.Presentation.Api.Extensions;

namespace Common.Presentation.Api.Middlewares.ExceptionHandling;
public sealed class ExceptionHandlingMiddleware(IHostEnvironment environment, ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected: not a server error, nothing to write.
            context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        var path = context.Request.Path.Value ?? "/";

        // Lets outer middlewares (observability / request logging) see the exception that produced the status.
        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature { Error = ex, Path = path });

        if (context.Response.HasStarted)
        {
            logger.LogError(ex, "Exception after response started. TraceId: {TraceId}", traceId);
            context.Abort();
            return;
        }

        var problem = MapExpected(ex, path);

        if (problem is not null)
        {
            logger.LogInformation(
                "Handled {ErrorType} ({Status}) on {Method} {Path}. TraceId: {TraceId}",
                ex is DomainException d ? d.Code : ex.GetType().Name,
                problem.Status, context.Request.Method, path, traceId);
        }
        else
        {
            var status = ex is DomainException domain
                ? DomainErrorMapper.GetStatusCode(domain)   // 5xx domain errors (unexpected, not_implemented, ...)
                : StatusCodes.Status500InternalServerError;

            problem = new ApiProblemDetails
            {
                Type = "urn:error:internal",
                Title = ReasonPhrases.GetReasonPhrase(status),
                Status = status,
                Detail = environment.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.",
                Instance = path
            };

            logger.LogError(ex, "Unhandled exception on {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method, path, traceId);
        }

        await context.WriteProblemAsync(problem);
    }
    private static ApiProblemDetails? MapExpected(Exception ex, string path) => ex switch
    {
        BadHttpRequestException b => Simple(b.StatusCode, "The request is malformed.", path),
        JsonException => Simple(StatusCodes.Status400BadRequest, "The request body is not valid JSON.", path),
        DomainException d when DomainErrorMapper.GetStatusCode(d) < StatusCodes.Status500InternalServerError => DomainErrorMapper.ToProblemDetails(d, path),
        _ => null
    };
    private static ApiProblemDetails Simple(int status, string detail, string path) => new()
    {
        Type = $"urn:http-error:{status}",
        Title = ReasonPhrases.GetReasonPhrase(status),
        Status = status,
        Detail = detail,
        Instance = path
    };
}