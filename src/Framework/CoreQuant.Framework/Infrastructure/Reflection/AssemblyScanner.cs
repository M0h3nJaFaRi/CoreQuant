using System.Reflection;

namespace Framework.Infrastructure.Reflection;

/// <summary>
/// Discovers concrete types in an assembly that assign to a given base or interface type.
/// </summary>
public static class AssemblyScanner
{
    /// <summary>
    /// Returns non-abstract, non-generic concrete types from <paramref name="assembly"/>
    /// that are assignable to <typeparamref name="T"/>.
    /// </summary>
    public static IReadOnlyList<Type> GetTypesAssignableTo<T>(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        return assembly
            .GetTypes()
            .Where(static t =>
                t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                && typeof(T).IsAssignableFrom(t))
            .ToArray();
    }

    /// <summary>
    /// Returns non-abstract, non-generic concrete types from <paramref name="assembly"/>
    /// that are assignable to <paramref name="baseType"/>.
    /// </summary>
    public static IReadOnlyList<Type> GetTypesAssignableTo(Assembly assembly, Type baseType)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(baseType);

        return assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                && baseType.IsAssignableFrom(t))
            .ToArray();
    }
}
