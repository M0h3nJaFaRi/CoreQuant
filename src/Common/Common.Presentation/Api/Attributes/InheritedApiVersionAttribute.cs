using Asp.Versioning;

namespace Common.Presentation.Api.Attributes;

/// <summary>
/// API version metadata that applies to derived controllers.
/// Built-in <see cref="ApiVersionAttribute"/> is not inherited, so version
/// base controllers use this attribute instead of repeating
/// <c>[ApiVersion]</c> on every resource controller.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class InheritedApiVersionAttribute(string version) : ApiVersionAttribute(version);