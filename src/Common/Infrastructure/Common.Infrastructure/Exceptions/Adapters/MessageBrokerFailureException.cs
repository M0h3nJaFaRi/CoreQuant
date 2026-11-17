using Common.Infrastructure.Exceptions.Abstraction;
using Common.Infrastructure.Exceptions.ErrorCode;

namespace Common.Infrastructure.Exceptions.Adapters;

/// <summary>
/// Thrown when a message broker operation fails.
/// </summary>
public sealed class MessageBrokerFailureException(string message, Exception? innerException = null)
    : InfrastructureLayerException(InfrastructureErrorCode.MessageBrokerFailure, message, innerException);
