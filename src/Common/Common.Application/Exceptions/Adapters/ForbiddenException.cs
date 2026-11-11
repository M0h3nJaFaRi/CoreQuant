using Common.Application.Exceptions.Abstraction;
using Common.Application.Exceptions.ErrorCode;

namespace Common.Application.Exceptions.Adapters;

/// <summary>
/// Thrown when the caller is not allowed to execute a use case.
/// </summary>
internal sealed class ForbiddenException(string message = "Forbidden.", string? reason = null)
    : BusinessException(BusinessErrorCode.OperationNotAllowed, message)
{
    public string? Reason { get; } = reason;
}
