using Common.Domain.Exceptions.ErrorCode;
using Common.Domain.Exceptions.Abstraction;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when creating a resource that already exists.
/// </summary>
public sealed class AlreadyExistsException(string entityName, object? key = null, string? aggregateName = null, Exception? innerException = null) : 
                    DomainLayerException(value: key, 
                                         entityName: entityName, 
                                         aggregateName: aggregateName,
                                         innerException: innerException,
                                         errorCode: DomainErrorCode.AlreadyExists,
                                         message: key is null ? $"{entityName} already exists." : $"{entityName} '{key}' already exists.");