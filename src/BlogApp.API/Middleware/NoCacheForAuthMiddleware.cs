namespace BlogApp.API.Middleware;

public class NoCacheForAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/auth"))
        {
            // Set immediately so tests and downstream code can observe headers even before response starts
            context.Response.Headers["Cache-Control"] = "no-store";
            context.Response.Headers["Pragma"] = "no-cache";
            context.Response.Headers["Expires"] = "0";

            // Also enforce via OnStarting to ensure headers are present when response is sent
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state!;
                httpContext.Response.Headers["Cache-Control"] = "no-store";
                httpContext.Response.Headers["Pragma"] = "no-cache";
                httpContext.Response.Headers["Expires"] = "0";
                return Task.CompletedTask;
            }, context);
        }

        await next(context);
    }
}

public static class NoCacheForAuthMiddlewareExtensions
{
    public static void UseNoCacheForAuth(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<NoCacheForAuthMiddleware>();
    }
}