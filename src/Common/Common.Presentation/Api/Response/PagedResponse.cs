namespace Common.Presentation.Api.Response;

/// <summary>
/// Paginated list payload for HTTP success responses.
/// </summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);