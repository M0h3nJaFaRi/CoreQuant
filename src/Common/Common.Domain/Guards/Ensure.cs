using Common.Domain.Exceptions;

namespace Common.Domain.Guards;

/// <summary>
/// Domain invariant guards. Prefer over ad-hoc exception throws inside aggregates.
/// </summary>
public static class Ensure
{
    /// <summary>
    /// Throws <see cref="DomainException"/> when <paramref name="condition"/> is false.
    /// </summary>
    public static void That(bool condition, string message, string code)
    {
        if (!condition) throw DomainException.Guard(message, code);
    }

    /// <summary>
    /// Throws <see cref="DomainException"/> when <paramref name="value"/> is null.
    /// </summary>
    public static T NotNull<T>(T? value, string message, string code = "not_null")
    where T : class
    {
        if (value is null)
            throw DomainException.Guard(message, code);

        return value;
    }

    /// <summary>
    /// Throws <see cref="DomainException"/> when <paramref name="value"/> is null or whitespace.
    /// </summary>
    public static string NotEmpty(string? value, string message, string code = "not_empty")
    {
        if (string.IsNullOrWhiteSpace(value))
            throw DomainException.Guard(message, code);

        return value;
    }

    /// <summary>
    /// Throws <see cref="NotFoundException"/> when <paramref name="value"/> is null.
    /// </summary>
    public static T Found<T>(T? value, string message, string code = "not_found")
    where T : class
    {
        if (value is null)
            throw DomainException.Guard(message, code);

        return value;
    }
}