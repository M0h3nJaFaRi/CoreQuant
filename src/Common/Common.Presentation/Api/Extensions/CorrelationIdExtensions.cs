using Microsoft.AspNetCore.Http;

namespace Common.Presentation.Api.Extensions;
public static class CorrelationIdExtensions
{
    // Private object key: can't collide with any other Items entry.
    private static readonly object ItemKey = new();

    /// <summary>The current request's correlation id, or null if CorrelationIdMiddleware hasn't run.</summary>
    internal static void SetCorrelationId(this HttpContext context, string correlationId) => context.Items[ItemKey] = correlationId;
    public static string? GetCorrelationId(this HttpContext context) => context.Items.TryGetValue(ItemKey, out var value) ? value as string : null;
}