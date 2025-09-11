namespace BlogApp.API.Middleware;

public class ProblemDetailsResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Only wrap non-success results and those not already ApiResponse
        if (context.Result is ObjectResult objectResult)
        {
            var statusCode = objectResult.StatusCode ?? context.HttpContext.Response.StatusCode;
            if (statusCode >= 400 && objectResult.Value is not ApiResponse<object>)
            {
                var message = ExtractMessage(objectResult.Value);
                var apiResponse = ApiResponse<object>.Failure(message ?? "An error occurred");
                apiResponse.Error!.Code = MapCode(statusCode);

                context.Result = new ObjectResult(apiResponse)
                {
                    StatusCode = statusCode,
                    DeclaredType = typeof(ApiResponse<object>)
                };
                context.HttpContext.Response.ContentType = "application/json";
            }
        }
        else if (context.Result is StatusCodeResult sc && sc.StatusCode >= 400)
        {
            var apiResponse = ApiResponse<object>.Failure(MapDefaultMessage(sc.StatusCode));
            apiResponse.Error!.Code = MapCode(sc.StatusCode);

            context.Result = new ObjectResult(apiResponse) { StatusCode = sc.StatusCode };
            context.HttpContext.Response.ContentType = "application/json";
        }

        await next();
    }

    private static string MapCode(int status)
    {
        return status switch
        {
            StatusCodes.Status400BadRequest => "BAD_REQUEST",
            StatusCodes.Status401Unauthorized => "UNAUTHORIZED",
            StatusCodes.Status403Forbidden => "FORBIDDEN",
            StatusCodes.Status404NotFound => "NOT_FOUND",
            StatusCodes.Status409Conflict => "CONFLICT",
            StatusCodes.Status413PayloadTooLarge => "PAYLOAD_TOO_LARGE",
            _ => "ERROR"
        };
    }

    private static string MapDefaultMessage(int status)
    {
        return status switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status409Conflict => "Conflict",
            StatusCodes.Status413PayloadTooLarge => "Payload too large",
            _ => "An error occurred"
        };
    }

    private static string? ExtractMessage(object? value)
    {
        if (value is null) return null;
        if (value is string s) return s;
        var prop = value.GetType().GetProperty("message") ?? value.GetType().GetProperty("Message");
        return prop?.GetValue(value)?.ToString();
    }
}