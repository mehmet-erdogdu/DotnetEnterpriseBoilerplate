using Microsoft.AspNetCore.Http;

namespace BlogApp.Application.Services;

public interface ICurrentUserService
{
    string UserId { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public string? IpAddress
    {
        get
        {
            if (httpContextAccessor.HttpContext == null) return null;

            // Try to get IP from X-Forwarded-For header
            var forwardedFor = httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor)) return forwardedFor.Split(',')[0].Trim();

            // Otherwise get it from the remote IP address
            return httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
        }
    }

    public string? UserAgent => httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}