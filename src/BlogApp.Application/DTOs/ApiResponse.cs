namespace BlogApp.Application.DTOs;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public ErrorResponse? Error { get; set; }

    public static ApiResponse<T> Success(T data)
    {
        return new ApiResponse<T>
        {
            Data = data,
            IsSuccess = true
        };
    }

    public static ApiResponse<T> Failure(string error)
    {
        return new ApiResponse<T>
        {
            Error = new ErrorResponse(error),
            IsSuccess = false
        };
    }
}

public class ErrorResponse(string message, string? code = null, object? details = null)
{
    public string Message { get; set; } = message;
    public string? Code { get; set; } = code;
    public object? Details { get; set; } = details;
}