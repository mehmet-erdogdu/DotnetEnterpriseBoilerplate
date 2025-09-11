namespace BlogApp.API.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "Development", StringComparison.OrdinalIgnoreCase);
        // Security Headers
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers.XFrameOptions = "DENY";
        context.Response.Headers.XXSSProtection = "1; mode=block";
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=(), payment=(), usb=()";

        // Enhanced Content Security Policy
        context.Response.Headers.ContentSecurityPolicy = isDevelopment
            ? "default-src 'self' 'unsafe-inline' 'unsafe-eval' data: blob:; connect-src 'self' https: http: ws: wss:; img-src 'self' data: https:; frame-ancestors 'none'; base-uri 'self'; form-action 'self';"
            : "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data: https:; font-src 'self' https:; connect-src 'self' https:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'; upgrade-insecure-requests; require-trusted-types-for 'script';";

        // Additional security headers
        context.Response.Headers.StrictTransportSecurity = "max-age=31536000; includeSubDomains; preload";
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
        context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
        context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

        // Additional security headers for enhanced protection
        context.Response.Headers["X-Download-Options"] = "noopen";
        context.Response.Headers["X-DNS-Prefetch-Control"] = "off";
        context.Response.Headers.XContentTypeOptions = "nosniff";
        context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static void UseSecurityHeaders(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}