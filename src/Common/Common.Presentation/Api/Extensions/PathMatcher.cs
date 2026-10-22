using Microsoft.AspNetCore.Http;

namespace Common.Presentation.Api.Extensions;
public static class PathMatcher
{
    /// <summary>
    /// True when <paramref name="path"/> equals or is nested under any of the excluded paths
    /// (segment-aware, case-insensitive: "/health" matches "/health" and "/health/ready", not "/healthy").
    /// Invalid entries are skipped instead of throwing, so a bad config value can't break every request.
    /// </summary>
    public static bool IsExcluded(PathString path, string[] excludedPaths)
    {
        foreach (var excluded in excludedPaths)
            if (IsValidPath(excluded) && path.StartsWithSegments(excluded, StringComparison.OrdinalIgnoreCase)) return true;

        return false;
    }

    /// <summary>Must start with '/' and be more than just "/" (which would exclude everything).</summary>
    public static bool IsValidPath(string? path) => !string.IsNullOrWhiteSpace(path) && path.Length > 1 && path[0] == '/';
}