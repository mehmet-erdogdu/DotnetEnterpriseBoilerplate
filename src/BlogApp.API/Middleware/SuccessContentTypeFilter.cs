namespace BlogApp.API.Middleware;

public class SuccessContentTypeFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var statusCode = context.HttpContext.Response.StatusCode;
        // If the action produced a specific ObjectResult status code, prefer that
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode.HasValue)
            statusCode = objectResult.StatusCode.Value;

        if (statusCode >= 200 && statusCode < 300)
        {
            // Keep the original response shape; only standardize content-type for JSON-like results
            if (context.Result is FileResult || context.Result is EmptyResult || context.Result is NoContentResult)
            {
                await next();
                return;
            }

            if (context.Result is ObjectResult || context.Result is JsonResult)
                context.HttpContext.Response.ContentType = "application/json";
        }

        await next();
    }
}