using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Common.Presentation.Api.Extensions;

namespace Common.Presentation.Api.Middlewares.ExceptionHandling;
public sealed class GlobalExceptionHandlingMiddleware(IHostEnvironment environment, ILogger<GlobalExceptionHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation("Request cancelled by client. {Method} {Path}",
                context.Request.Method, context.Request.Path.Value);

            if (!context.Response.HasStarted)
                context.Response.StatusCode = 499;
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var path = context.Request.Path.Value ?? "/";

        if (context.Response.HasStarted)
        {
            logger.LogError("""
                                Exception after response started. 
                                Path: {Path},
                                Method: {Method},
                                Message: {Message}.
                            """, context.Request.Method, path, ex.Message);

            context.Abort();

            return;
        }

        context.Features.Set<IExceptionHandlerFeature>(new ExceptionHandlerFeature { Error = ex, Path = path });

        var problem = ApiProblemFactory.Create(ex, path, environment.IsDevelopment());

        if (problem.Status >= 500)
            logger.LogError("""
                                Unhandled exception on {Method} {Path}. 
                                Type: {type},
                                Code: {Code}
                                Title: {title},
                                Status: {Status},
                                Detail: {detail},
                                Instance: {instance},
                                Message: {message},
                                InnerMessage: {innerMessage}
                            """,
                            context.Request.Method, path, problem.Type,
                            problem.Extensions.GetValueOrDefault("code"), problem.Title,
                            problem.Status, problem.Detail, problem.Instance, ex.Message, ex.InnerException?.Message);
        else
            logger.LogWarning("""
                                  Request failed on {Method} {Path}. 
                                  Type: {type},
                                  Code: {Code}
                                  Title: {title},
                                  Status: {Status},
                                  Detail: {detail},
                                  Instance: {instance},
                                  Message: {message},
                                  InnerMessage: {innerMessage}
                              """,
                            context.Request.Method, path, problem.Type,
                            problem.Extensions.GetValueOrDefault("code"), problem.Title,
                            problem.Status, problem.Detail, problem.Instance, ex.Message, ex.InnerException?.Message);

        await context.WriteProblemAsync(problem);
    }
}