namespace Common.Api.Response;

public class ApiResponse
{
    public bool Success { get; init; }

    public ApiError? Error { get; init; }

    public static ApiResponse Ok() => new() { Success = true };

    public static ApiResponse Fail(ApiError error) => new() { Success = false, Error = error };
}

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
}