using Common.Domain.Exceptions;
using Common.Framework.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Diagnostics;

namespace Common.Presentation.Api.Middlewares.GlobalObservabilityHandling;

public sealed class GlobalObservabilityMiddleware(HttpMetrics metrics) : IMiddleware
{
    private const string UnmatchedRoute = "unmatched";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        Exception? escaped = null;
        var startTimestamp = Stopwatch.GetTimestamp();
        var method = HttpMetrics.NormalizeMethod(context.Request.Method);

        metrics.RequestStarted(method);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            escaped = ex;
            throw;
        }
        finally
        {
            metrics.RequestFinished(method);
            Complete(context, method, startTimestamp, escaped);
        }
    }

    private void Complete(HttpContext context, string method, long startTimestamp, Exception? escaped)
    {
        var exception = escaped ?? context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var clientAborted = exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested;

        var statusCode = escaped is null ? context.Response.StatusCode : clientAborted ? 499 : StatusCodes.Status500InternalServerError;

        var route = GetRoute(context);
        var elapsedSeconds = Stopwatch.GetElapsedTime(startTimestamp).TotalSeconds;
        metrics.RecordRequest(method, route, statusCode, elapsedSeconds);
        var activity = Activity.Current;

        if (exception is not null && !clientAborted)
        {
            var (errorType, errorKind) = exception switch
            {
                DomainException domain => (domain.Code, "domain"),
                IHasErrorCode coded => (coded.Code, "expected"),
                _ => (exception.GetType().Name, "unexpected")
            };

            metrics.RecordException(method, route, errorType, errorKind);
            activity?.AddException(exception);
        }

        if (statusCode >= StatusCodes.Status500InternalServerError)
            activity?.SetStatus(ActivityStatusCode.Error);
    }

    private static string GetRoute(HttpContext context) => (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? UnmatchedRoute;
}
