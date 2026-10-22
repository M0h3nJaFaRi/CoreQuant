namespace Common.Presentation.Api.Response;

public class ApiResponse
{
    public bool Success { get; init; }

    public ApiProblemDetails? Error { get; init; }

    public static ApiResponse Ok() => new() { Success = true };

    public static ApiResponse Fail(ApiProblemDetails problemDetails) => new() { Success = false, Error = problemDetails };
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
}