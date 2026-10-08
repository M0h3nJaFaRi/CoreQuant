namespace Common.Application.Cqrs.Abstractions;

/// <summary>
/// Empty response for void command pipeline composition.
/// </summary>
public readonly struct Unit : IEquatable<Unit>
{
    /// <summary>
    /// Singleton unit value.
    /// </summary>
    public static Unit Value => default;

    /// <inheritdoc />
    public bool Equals(Unit other) => true;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is Unit;

    /// <inheritdoc />
    public override int GetHashCode() => 0;

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
