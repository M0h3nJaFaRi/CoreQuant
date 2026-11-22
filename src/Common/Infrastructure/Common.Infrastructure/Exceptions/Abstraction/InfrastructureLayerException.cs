using Common.Framework.Exceptions.Abstraction;

namespace Common.Infrastructure.Exceptions.Abstraction;

/// <summary>
/// Base for infrastructure failures (database, cache, messaging, storage).
/// </summary>
public abstract class InfrastructureLayerException(string errorCode, string message, Exception? innerException = null) : 
                      ServiceException(errorCode: errorCode, message: message, innerException: innerException);