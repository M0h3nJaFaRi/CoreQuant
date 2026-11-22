using Common.Application.Exceptions.Abstraction;

namespace Common.Application.Exceptions.Abstraction.Business;

/// <summary>
/// Base for application policy and business-rule violations (not domain invariants).
/// </summary>
public abstract class BusinessException(string errorCode, string message, string? handler = null, string? requestType = null, Exception? innerException = null) : 
                      ApplicationLayerException(errorCode, message, handler, requestType, innerException);