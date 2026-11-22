namespace Common.Application.Exceptions.Abstraction.Validation;

/// <summary>
/// Base for command/query validation failures.
/// </summary>
public abstract class ApplicationValidationException(string errorCode, string message, string? handler = null, string? requestType = null, Exception? innerException = null) : 
                      ApplicationLayerException(errorCode, message, handler, requestType, innerException)
{
    public abstract IReadOnlyDictionary<string, string[]> Errors { get; }
}