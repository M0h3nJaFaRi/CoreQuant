using Microsoft.AspNetCore.Mvc;

namespace Common.Api.Attributes;

/// <summary>
/// Pins a versioned REST resource path shared by the Command/Query controller pair
/// for the same aggregate. Keeps URLs resource-oriented and independent of
/// controller type names.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ApiRouteAttribute(string resourcePath) : RouteAttribute($"api/v{{version:apiVersion}}/{resourcePath.Trim('/')}");