using Common.Application.Exceptions.ErrorCode;
using Common.Application.Exceptions.Abstraction.Business;

namespace Common.Application.Exceptions.Adapters;

/// <summary>
/// Thrown when the caller is not allowed to execute a use case.
/// </summary>
internal sealed class ForbiddenException(
    string message = "Forbidden.",
    string? reason = null,
    string? handler = null,
    string? requestType = null)
    : BusinessException(BusinessErrorCode.OperationNotAllowed, message, handler, requestType)
{
    public string? Reason { get; } = reason;
}