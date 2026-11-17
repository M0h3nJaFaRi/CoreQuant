using Common.Domain.Exceptions.ErrorCode;
using Common.Domain.Exceptions.Abstraction;

namespace Common.Domain.Exceptions.Adapters;

/// <summary>
/// Thrown when an operation targets an entity that was already processed.
/// </summary>
public sealed class AlreadyProcessedException(string entityName,
                                              object? key = null,
                                              string? aggregateName = null,
                                              Exception? innerException = null) : 
                    DomainLayerException(errorCode: DomainErrorCode.AlreadyProcessed,
                                         message: key is null ? $"{entityName} has already been processed." : $"{entityName} '{key}' has already been processed.",
                                         value: key,
                                         entityName: entityName,
                                         aggregateName: aggregateName,
                                         innerException: innerException);