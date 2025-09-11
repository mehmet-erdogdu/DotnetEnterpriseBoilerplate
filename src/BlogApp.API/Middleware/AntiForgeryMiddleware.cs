namespace BlogApp.API.Middleware;

public class AntiForgeryMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip CSRF check for GET requests
        if (context.Request.Method == "GET")
        {
            await next(context);
            return;
        }

        // Validate CSRF for state-changing operations
        if (IsStateChangingRequest(context.Request.Method))
        {
            var isValid = await ValidateCsrfAsync(context);
            if (!isValid) return; // Response already written in ValidateCsrfAsync
        }

        await next(context);
    }

    private static bool IsStateChangingRequest(string method)
    {
        return method is "POST" or "PUT" or "DELETE" or "PATCH";
    }

    private static async Task<bool> ValidateCsrfAsync(HttpContext context)
    {
        var messageService = context.RequestServices.GetRequiredService<IMessageService>();
        var configuration = context.RequestServices.GetRequiredService<IConfiguration>();

        var origin = context.Request.Headers.Origin;
        var referer = context.Request.Headers.Referer;

        var allowedOrigins = GetAllowedOrigins(configuration);
        if (allowedOrigins == null) return true; // No configuration available, allow request

        if (string.IsNullOrEmpty(origin) || string.IsNullOrEmpty(referer)) return true; // No origin/referer headers, allow request

        if (!IsOriginAllowed(origin!, referer!, allowedOrigins))
        {
            await WriteErrorResponseAsync(context, messageService);
            return false;
        }

        return true;
    }

    private static string[]? GetAllowedOrigins(IConfiguration configuration)
    {
        var frontendUrls = configuration["FrontendUrls"];
        var apiUrl = configuration["APIUrl"];

        if (string.IsNullOrEmpty(frontendUrls) && string.IsNullOrEmpty(apiUrl)) return null;

        return !string.IsNullOrWhiteSpace(frontendUrls)
            ? frontendUrls.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : new[] { apiUrl! };
    }

    private static bool IsOriginAllowed(string origin, string referer, string[] allowedOrigins)
    {
        return allowedOrigins.Any(allowedOrigin =>
            origin.StartsWith(allowedOrigin, StringComparison.OrdinalIgnoreCase) &&
            referer.StartsWith(allowedOrigin, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, IMessageService messageService)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("CSRFInvalidOriginOrReferer"));
        apiResponse.Error!.Code = "CSRF_VALIDATION_FAILED";

        // Add correlation and trace information
        apiResponse.Error.Details = new Dictionary<string, object>
        {
            ["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString(),
            ["traceId"] = context.TraceIdentifier
        };

        var result = JsonSerializer.Serialize(apiResponse);
        await context.Response.WriteAsync(result);
    }
}

public static class AntiForgeryMiddlewareExtensions
{
    public static void UseAntiForgery(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<AntiForgeryMiddleware>();
    }
}