namespace Common.Presentation.Api.Response;

public record ApiError(string Code, string Message, IReadOnlyDictionary<string, string[]>? Details = null);
