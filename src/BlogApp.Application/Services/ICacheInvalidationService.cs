namespace BlogApp.Application.Services;

public interface ICacheInvalidationService
{
    Task InvalidatePostCacheAsync(Guid? postId = null);
    Task InvalidateTodoCacheAsync(Guid? todoId = null);
    Task InvalidateUserCacheAsync(string? userId = null);
    Task InvalidateAllCacheAsync();
}