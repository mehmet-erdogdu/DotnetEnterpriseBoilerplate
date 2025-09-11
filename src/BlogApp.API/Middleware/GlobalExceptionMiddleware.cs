namespace BlogApp.API.Middleware;

public class GlobalExceptionMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var messageService = context.RequestServices.GetRequiredService<IMessageService>();
            var errorResponse = HandleExceptionAsync(error, messageService);
            response.StatusCode = (int)errorResponse.statusCode;

            var apiResponse = ApiResponse<object>.Failure(errorResponse.error.Message);
            apiResponse.Error!.Code = errorResponse.error.Code;

            // Combine original details with correlation/trace info
            var enhancedDetails = new Dictionary<string, object>();

            // Add correlation and trace information
            enhancedDetails["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString();
            enhancedDetails["traceId"] = context.TraceIdentifier;

            // Add original error details if they exist
            if (errorResponse.error.Details != null) enhancedDetails["errorDetails"] = errorResponse.error.Details;

            apiResponse.Error.Details = enhancedDetails;

            var result = JsonSerializer.Serialize(apiResponse);
            await response.WriteAsync(result);
        }
    }

    private (HttpStatusCode statusCode, ErrorResponse error) HandleExceptionAsync(Exception exception, IMessageService messageService)
    {
        logger.LogError(exception, "An unhandled exception occurred.");

        HttpStatusCode statusCode;
        string message;
        string? code = null;
        object? details = null;

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = messageService.GetMessage("UnauthorizedAccess");
                code = "UNAUTHORIZED";
                break;

            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                message = argEx.Message;
                code = "INVALID_ARGUMENT";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = messageService.GetMessage("ResourceNotFound");
                code = "NOT_FOUND";
                break;

            case ValidationException ve:
                statusCode = HttpStatusCode.BadRequest;
                message = messageService.GetMessage("ValidationError");
                code = "VALIDATION_ERROR";
                details = ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => messageService.GetMessage(e.ErrorMessage)).Distinct().ToArray()
                    );
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = messageService.GetMessage("UnexpectedErrorOccurred");
                code = "INTERNAL_SERVER_ERROR";

                // Sadece development ortamında detayları göster
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                    details = new
                    {
                        exception.Message,
                        exception.StackTrace
                    };
                break;
        }

        var error = new ErrorResponse(message, code, details);
        return (statusCode, error);
    }
}

// Extension method for easy middleware registration
public static class GlobalExceptionMiddlewareExtensions
{
    public static void UseGlobalExceptionMiddleware(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}