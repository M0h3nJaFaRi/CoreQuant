using Common.Api.Attributes;

namespace Common.Api.VersionController.V1;

/// <summary>
/// Base for public HTTP API version 1. Resource controllers inherit this type
/// instead of mixing v1 actions into a multi-version controller.
/// </summary>
[InheritedApiVersion("1.0")]
public abstract class ControllerV1 : MainController;