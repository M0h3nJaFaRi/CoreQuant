using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Common.Presentation.Api.Response;

namespace Common.Presentation.Api.VersionController;

/// <summary>
/// Version-agnostic HTTP adapter base for the Presentation layer.
/// Maps Application results onto the existing <see cref="ApiResponse"/> envelope
/// and standard success status codes. Error mapping stays in
/// <c>GlobalExceptionHandlerMiddleware</c>.
/// </summary>
[ApiController]
public abstract class MainController : ControllerBase
{
    /// <summary>
    /// 200 OK with the success envelope and no payload.
    /// </summary>
    protected IActionResult OkResponse() 
                         => Ok(ApiResponse.Ok());

    /// <summary>
    /// 200 OK with the success envelope.
    /// </summary>
    protected IActionResult OkResponse<T>(T data) 
                         => Ok(ApiResponse<T>.Ok(data));

    /// <summary>
    /// 201 Created with the success envelope and no Location header.
    /// </summary>
    protected IActionResult CreatedResponse<T>(T data)
                         => StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(data));

    /// <summary>
    /// 201 Created with the success envelope and a Location header.
    /// </summary>
    protected IActionResult CreatedResponse<T>(string uri, T data)
                         => Created(uri, ApiResponse<T>.Ok(data));

    /// <summary>
    /// 201 Created with the success envelope and a Location derived from an action.
    /// </summary>
    protected IActionResult CreatedAtActionResponse<T>(string actionName, object? routeValues, T data)
                         => CreatedAtAction(actionName, routeValues, ApiResponse<T>.Ok(data));

    /// <summary>
    /// 202 Accepted with the success envelope.
    /// </summary>
    protected IActionResult AcceptedResponse<T>(T data)
                         => Accepted(ApiResponse<T>.Ok(data));

    /// <summary>
    /// 204 No Content. The body is omitted by HTTP convention.
    /// </summary>
    protected IActionResult NoContentResponse() 
                         => NoContent();

    protected IActionResult Page<T>(int page, int pageSize) 
                         => OkResponse(new PagedResponse<T>([], 0, page, pageSize));

    protected IActionResult EmptyList<T>(object query) 
                         => OkResponse<IReadOnlyList<T>>([]);

}