using BlogApp.Application.Constants;

namespace BlogApp.API.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestBody = string.Empty;
        var responseBody = string.Empty;

        // Capture request body for logging (excluding sensitive endpoints)
        if (ShouldLogRequestBody(context.Request) && context.Request.Body.CanSeek)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        // Capture response body (skip for large responses)
        var originalBodyStream = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await next(context);
        }
        finally
        {
            // Capture response body
            memoryStream.Position = 0;
            if (memoryStream.Length < 1024 * 1024) // 1MB guard
                responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
        }

        var duration = DateTime.UtcNow - startTime;

        // Log request and response (excluding sensitive data)
        var logMessage = new
        {
            Timestamp = startTime,
            context.Request.Method,
            context.Request.Path,
            QueryString = SanitizeQueryString(context.Request.QueryString.ToString()),
            context.Response.StatusCode,
            Duration = duration.TotalMilliseconds,
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            IPAddress = GetClientIPAddress(context),
            RequestBody = SanitizeRequestBody(requestBody, context.Request.Path),
            ResponseBody = SanitizeResponseBody(responseBody, context.Request.Path)
        };

        if (context.Response.StatusCode >= 400)
            logger.LogWarning("HTTP {Method} {Path} => {StatusCode} in {Duration}ms",
                logMessage.Method, logMessage.Path, logMessage.StatusCode, logMessage.Duration);
        else
            logger.LogInformation("HTTP {Method} {Path} => {StatusCode} in {Duration}ms",
                logMessage.Method, logMessage.Path, logMessage.StatusCode, logMessage.Duration);
    }

    private static bool ShouldLogRequestBody(HttpRequest request)
    {
        // Don't log sensitive endpoints
        var sensitiveEndpoints = new[] { "/api/auth/login", "/api/auth/register" };
        return !sensitiveEndpoints.Any(endpoint => request.Path.StartsWithSegments(endpoint));
    }

    private static string SanitizeQueryString(string queryString)
    {
        if (string.IsNullOrEmpty(queryString)) return queryString;

        try
        {
            // Remove sensitive query parameters
            var sensitiveParams = new[] { "password", "token", "secret", "key" };
            var sanitized = queryString;

            foreach (var param in sensitiveParams)
            {
                var pattern = $@"{param}=[^&]*";
                sanitized = Regex.Replace(sanitized, pattern, $"{param}=***",
                    FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);
            }

            return sanitized;
        }
        catch (RegexMatchTimeoutException)
        {
            // If regex times out, return redacted for security
            return "***REDACTED***";
        }
    }

    private static string SanitizeRequestBody(string body, PathString path)
    {
        var sensitiveFields = new[] { "password", "token", "secret", "key" };
        return SanitizeJsonBody(body, path, sensitiveFields);
    }

    private static string SanitizeResponseBody(string body, PathString path)
    {
        var sensitiveFields = new[] { "token", "secret", "key", "password" };
        return SanitizeJsonBody(body, path, sensitiveFields);
    }

    private static string SanitizeJsonBody(string body, PathString path, string[] sensitiveFields)
    {
        if (string.IsNullOrEmpty(body)) return body;

        // Don't log sensitive endpoints
        if (path.StartsWithSegments("/api/auth"))
            return "***REDACTED***";

        try
        {
            // Remove sensitive fields from JSON body
            var sanitized = body;

            foreach (var field in sensitiveFields)
            {
                var pattern = $@"""{field}""\s*:\s*""[^""]*""";
                sanitized = Regex.Replace(sanitized, pattern, $"\"{field}\": \"***\"",
                    FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);
            }

            return sanitized;
        }
        catch (RegexMatchTimeoutException)
        {
            // If regex times out, return redacted for security
            return "***REDACTED***";
        }
    }

    private static string GetClientIPAddress(HttpContext context)
    {
        // Get the real IP address considering proxies
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader)) return forwardedHeader.Split(',')[0].Trim();

        var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader)) return realIpHeader;

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static void UseRequestLogging(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}