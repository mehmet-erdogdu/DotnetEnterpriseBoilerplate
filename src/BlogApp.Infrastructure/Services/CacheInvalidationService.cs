namespace BlogApp.Infrastructure.Services;

public class CacheInvalidationService(ICacheService cacheService, ILogger<CacheInvalidationService> logger) : ICacheInvalidationService
{
    public async Task InvalidatePostCacheAsync(Guid? postId = null)
    {
        try
        {
            if (postId.HasValue)
            {
                // Remove specific post cache
                var postKey = CacheConstants.Keys.PostById(postId.Value);
                await cacheService.RemoveAsync(postKey);
                logger.LogInformation("Invalidated cache for specific post: {PostId}", postId.Value);
            }
            else
            {
                // Remove all post-related cache
                await cacheService.RemoveByPatternAsync($"{CacheConstants.PostsPrefix}:*");
                logger.LogInformation("Invalidated all post cache entries");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating post cache for postId: {PostId}", postId);
        }
    }

    public async Task InvalidateTodoCacheAsync(Guid? todoId = null)
    {
        try
        {
            if (todoId.HasValue)
            {
                var todoKey = CacheConstants.Keys.TodoById(todoId.Value);
                await cacheService.RemoveAsync(todoKey);
                logger.LogInformation("Invalidated cache for specific todo: {TodoId}", todoId.Value);
            }
            else
            {
                await cacheService.RemoveByPatternAsync($"{CacheConstants.TodosPrefix}:*");
                logger.LogInformation("Invalidated all todo cache entries");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating todo cache for todoId: {TodoId}", todoId);
        }
    }

    public async Task InvalidateUserCacheAsync(string? userId = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(userId))
            {
                // Remove specific user cache
                var userKey = CacheConstants.Keys.UserById(userId);
                var userPostsKey = CacheConstants.Keys.UserPosts(userId);
                await cacheService.RemoveAsync(userKey);
                await cacheService.RemoveAsync(userPostsKey);
                logger.LogInformation("Invalidated cache for specific user: {UserId}", userId);
            }
            else
            {
                // Remove all user-related cache
                await cacheService.RemoveByPatternAsync($"{CacheConstants.UserPrefix}:*");
                logger.LogInformation("Invalidated all user cache entries");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating user cache for userId: {UserId}", userId);
        }
    }

    public async Task InvalidateAllCacheAsync()
    {
        try
        {
            // Remove all cache entries
            await cacheService.RemoveByPatternAsync("*");
            logger.LogInformation("Invalidated all cache entries");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating all cache entries");
        }
    }
}