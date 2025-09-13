namespace BlogApp.API.Middleware;

public class InputValidationMiddleware(RequestDelegate next, ILogger<InputValidationMiddleware> logger)
{
    private const string ApplicationJsonContentType = "application/json";

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var messageService = context.RequestServices.GetRequiredService<IMessageService>();
            // Validate and sanitize query parameters
            if (context.Request.QueryString.HasValue)
            {
                var sanitizedQuery = SanitizeQueryString(context.Request.QueryString.Value);
                if (sanitizedQuery != context.Request.QueryString.Value)
                    logger.LogWarning("Query string sanitized from {Original} to {Sanitized}",
                        context.Request.QueryString.Value, sanitizedQuery);
            }

            // Validate Content-Type for POST/PUT/PATCH requests
            if (context.Request.Method is "POST" or "PUT" or "PATCH")
            {
                var contentType = context.Request.ContentType;
                if (string.IsNullOrEmpty(contentType) ||
                    (!contentType.Contains(ApplicationJsonContentType) &&
                     !contentType.Contains("application/x-www-form-urlencoded") &&
                     !contentType.Contains("multipart/form-data")))
                {
                    var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("InvalidContentType"));
                    apiResponse.Error!.Code = "INVALID_CONTENT_TYPE";
                    apiResponse.Error.Details = new Dictionary<string, object>
                    {
                        ["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString(),
                        ["traceId"] = context.TraceIdentifier,
                        ["providedContentType"] = contentType ?? "null"
                    };

                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    context.Response.ContentType = ApplicationJsonContentType;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
                    return;
                }

                // Check content length to prevent large payload attacks
                if (context.Request.ContentLength > 1048576) // 1MB limit
                {
                    var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("RequestPayloadTooLarge"));
                    apiResponse.Error!.Code = "PAYLOAD_TOO_LARGE";
                    apiResponse.Error.Details = new Dictionary<string, object>
                    {
                        ["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString(),
                        ["traceId"] = context.TraceIdentifier,
                        ["contentLength"] = context.Request.ContentLength ?? 0,
                        ["maxAllowedSize"] = 1048576
                    };

                    context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                    context.Response.ContentType = ApplicationJsonContentType;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
                    return;
                }
            }

            // Validate request headers
            if (!ValidateHeaders(context.Request.Headers))
            {
                var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("InvalidRequestHeaders"));
                apiResponse.Error!.Code = "INVALID_HEADERS";
                apiResponse.Error.Details = new Dictionary<string, object>
                {
                    ["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString(),
                    ["traceId"] = context.TraceIdentifier
                };

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = ApplicationJsonContentType;
                await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
                return;
            }

            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in InputValidationMiddleware while processing request {Method} {Path}",
                context?.Request?.Method, context?.Request?.Path);
            // Rethrow with additional context
            throw new InvalidOperationException($"Input validation failed for {context?.Request?.Method} {context?.Request?.Path}", ex);
        }
    }

    private static string SanitizeQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString)) return queryString;

        try
        {
            // Remove potentially dangerous characters
            var sanitized = Regex.Replace(queryString, @"[<>""'&]", "",
                FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);

            // Remove SQL injection patterns
            sanitized = Regex.Replace(sanitized,
                @"(?i)(union|select|insert|update|delete|drop|create|alter|exec|execute|script|javascript|vbscript|expression|onload|onerror|onclick)",
                "", FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);

            // Remove XSS patterns
            sanitized = Regex.Replace(sanitized, @"(?i)(<script|javascript:|vbscript:|expression|onload|onerror|onclick|onmouseover|onfocus|onblur)", "",
                FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);

            // Remove path traversal patterns
            sanitized = Regex.Replace(sanitized, @"\.\./|\.\.\\|%2e%2e%2f|%2e%2e%5c", "",
                FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);

            return sanitized;
        }
        catch (RegexMatchTimeoutException)
        {
            // If regex times out, return empty string for security
            return string.Empty;
        }
    }

    private static bool ValidateHeaders(IHeaderDictionary headers)
    {
        // Check for suspicious headers
        var suspiciousHeaders = new[] { "X-Forwarded-For", "X-Real-IP", "X-Forwarded-Host" };

        foreach (var header in suspiciousHeaders.Where(headers.ContainsKey))
        {
            headers.TryGetValue(header, out var value);
            if (IsSuspiciousHeaderValue(value.ToString())) return false;
        }

        return true;
    }

    private static bool IsSuspiciousHeaderValue(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;

        // Check for suspicious patterns in header values
        var suspiciousPatterns = new[]
        {
            @"<script",
            @"javascript:",
            @"vbscript:",
            @"expression\(",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*=",
            @"\.\./",
            @"\.\.\\"
        };

        return suspiciousPatterns.Any(pattern =>
        {
            try
            {
                return Regex.IsMatch(value, pattern, FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                // If regex times out, assume it's suspicious for security
                return true;
            }
        });
    }
}

public static class InputValidationMiddlewareExtensions
{
    public static void UseInputValidation(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<InputValidationMiddleware>();
    }
}