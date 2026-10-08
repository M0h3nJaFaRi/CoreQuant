namespace Framework.Resilience.Backoff;

/// <summary>
/// Calculates exponential backoff delays for retries.
/// </summary>
public static class ExponentialBackoff
{
    /// <summary>
    /// Returns the delay for the given 1-based <paramref name="attempt"/>:
    /// <c>baseDelay * factor^(attempt - 1)</c>, capped by <paramref name="maxDelay"/> when provided.
    /// </summary>
    public static TimeSpan Calculate(
        int attempt,
        TimeSpan baseDelay,
        TimeSpan? maxDelay = null,
        double factor = 2.0)
    {
        if (attempt < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(attempt), attempt, "Attempt must be at least 1.");
        }

        if (baseDelay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(baseDelay), baseDelay, "Base delay cannot be negative.");
        }

        if (factor < 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), factor, "Factor must be at least 1.0.");
        }

        var milliseconds = baseDelay.TotalMilliseconds * Math.Pow(factor, attempt - 1);
        var delay = TimeSpan.FromMilliseconds(milliseconds);

        if (maxDelay is { } cap && delay > cap)
        {
            return cap;
        }

        return delay;
    }
}
