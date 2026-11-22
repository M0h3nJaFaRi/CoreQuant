namespace Common.Framework.Contract;

/// <summary>
/// Stable identity of a running service process (namespace, name, component, version).
/// </summary>
public sealed record ServiceIdentity(string Namespace, string Name, string Component, string Version);