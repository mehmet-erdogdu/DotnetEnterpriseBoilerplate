namespace BlogApp.API.Middleware;

public class EnhancedSecurityMiddleware(RequestDelegate next, ILogger<EnhancedSecurityMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var messageService = context.RequestServices.GetRequiredService<IMessageService>();
        // Block requests with suspicious user agents
        var userAgent = context.Request.Headers.UserAgent.ToString();
        if (IsSuspiciousUserAgent(userAgent))
        {
            logger.LogWarning("Blocked request with suspicious User-Agent: {UserAgent}", userAgent);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("AccessDenied"));
            apiResponse.Error!.Code = "SUSPICIOUS_USER_AGENT";
            apiResponse.Error.Details = new Dictionary<string, object>
            {
                ["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString(),
                ["traceId"] = context.TraceIdentifier,
                ["userAgent"] = userAgent
            };

            var result = JsonSerializer.Serialize(apiResponse);
            await context.Response.WriteAsync(result);
            return;
        }

        // Block requests with suspicious IP patterns (basic implementation)
        var clientIp = GetClientIpAddress(context);
        if (IsSuspiciousIpAddress(clientIp))
        {
            logger.LogWarning("Blocked request from suspicious IP: {IP}", clientIp);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var apiResponse = ApiResponse<object>.Failure(messageService.GetMessage("AccessDenied"));
            apiResponse.Error!.Code = "SUSPICIOUS_IP_ADDRESS";
            apiResponse.Error.Details = new Dictionary<string, object>
            {
                ["correlationId"] = context.Response.Headers["X-Correlation-ID"].ToString(),
                ["traceId"] = context.TraceIdentifier,
                ["ipAddress"] = clientIp
            };

            var result = JsonSerializer.Serialize(apiResponse);
            await context.Response.WriteAsync(result);
            return;
        }

        // Add security context to request
        context.Items["ClientIP"] = clientIp;
        context.Items["UserAgent"] = userAgent;

        await next(context);
    }

    private static string GetClientIpAddress(HttpContext context)
    {
        // Get the real IP address considering proxies
        var forwardedHeader = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedHeader)) return forwardedHeader.Split(',')[0].Trim();

        var realIpHeader = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader)) return realIpHeader;

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private static bool IsSuspiciousUserAgent(string userAgent)
    {
        if (string.IsNullOrEmpty(userAgent)) return true;

        var suspiciousPatterns = new[]
        {
            "bot", "crawler", "spider", "scraper", "curl", "wget", "python", "java",
            "perl", "ruby", "php", "go-http-client", "okhttp", "apache-httpclient"
        };

        return suspiciousPatterns.Any(pattern =>
            userAgent.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsSuspiciousIpAddress(string ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress) || ipAddress == "Unknown") return false;

        // Simple IP range check (for production, use a proper IP address library)
        if (ipAddress.StartsWith("10.")
            || ipAddress.StartsWith("172.")
            || ipAddress.StartsWith("192.168.")
            || ipAddress.StartsWith("127."))
            return true;

        return false;
    }
}

public static class EnhancedSecurityMiddlewareExtensions
{
    public static void UseEnhancedSecurity(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<EnhancedSecurityMiddleware>();
    }
}