using Common.Presentation.Api.Attributes;

namespace Common.Presentation.Api.VersionController.V2;

/// <summary>
/// Base for public HTTP API version 2. Resource controllers inherit this type
/// instead of mixing v2 actions into a multi-version controller.
/// </summary>
[InheritedApiVersion("2.0")]
public abstract class ControllerV2 : MainController;
