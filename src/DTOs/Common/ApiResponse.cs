namespace Api.DTOs.Common;

public class ApiResponse<T>
{
    public string Status { get; init; } = "success";
    public T? Data { get; init; }
    public string? Message { get; init; }
    public object? Meta { get; init; }

    public static ApiResponse<T> Success(T data, object? meta = null) =>
        new() { Status = "success", Data = data, Meta = meta };

    public static ApiResponse<T> Fail(string message) =>
        new() { Status = "fail", Message = message };

    public static ApiResponse<T> Error(string message) =>
        new() { Status = "error", Message = message };
}

public class PaginationMeta
{
    public int Total { get; init; }
    public int Page { get; init; }
    public int Limit { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
