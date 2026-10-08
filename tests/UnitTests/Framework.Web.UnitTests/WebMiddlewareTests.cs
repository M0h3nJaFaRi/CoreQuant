using System.Diagnostics.Metrics;
using Framework.Observability;
using Framework.Observability.Correlation;
using Framework.Web.Middleware;
using Microsoft.AspNetCore.Http;

namespace Framework.Web.UnitTests;

public sealed class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Uses_Incoming_Correlation_Header()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationKeys.HeaderName] = "abc123";
        context.Response.Body = new MemoryStream();

        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);
        await context.Response.Body.FlushAsync();

        Assert.Equal("abc123", context.Items[CorrelationKeys.CorrelationId]);
    }

    [Fact]
    public async Task InvokeAsync_Generates_Correlation_Id_When_Missing()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);
        await middleware.InvokeAsync(context);

        var id = Assert.IsType<string>(context.Items[CorrelationKeys.CorrelationId]);
        Assert.False(string.IsNullOrWhiteSpace(id));
    }
}

public sealed class HttpMetricsMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Completes_Request()
    {
        var context = new DefaultHttpContext();
        using var meter = new Meter("test-http-metrics");
        var called = false;

        var middleware = new HttpMetricsMiddleware(_ =>
        {
            called = true;
            return Task.CompletedTask;
        }, meter);

        await middleware.InvokeAsync(context);

        Assert.True(called);
    }
}
