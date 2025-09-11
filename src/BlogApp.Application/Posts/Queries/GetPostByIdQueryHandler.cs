namespace BlogApp.Application.Posts.Queries;

public class GetPostByIdQueryHandler(
    IUnitOfWork unitOfWork,
    ICacheService cacheService,
    ILogger<GetPostByIdQueryHandler> logger)
    : IRequestHandler<GetPostByIdQuery, PostDto?>
{
    public async Task<PostDto?> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheConstants.Keys.PostById(request.Id);

        // Try to get from cache first
        try
        {
            var cachedResult = await cacheService.GetAsync<PostDto>(cacheKey);
            if (cachedResult != null)
            {
                logger.LogInformation("Retrieved post from cache for key: {CacheKey}", cacheKey);
                return cachedResult;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting post from cache for key: {CacheKey}", cacheKey);
        }

        // If not in cache, get from database
        logger.LogInformation("Cache miss for post {PostId}, fetching from database", request.Id);
        var post = await unitOfWork.Posts.GetPostWithAuthorAsync(request.Id);

        if (post == null)
        {
            logger.LogWarning("Post not found in database: {PostId}", request.Id);
            return null;
        }

        // Convert to DTO
        var postDto = new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author != null ? $"{post.Author.FirstName} {post.Author.LastName}" : null
        };

        // Cache the result
        try
        {
            await cacheService.SetAsync(cacheKey, postDto, CacheConstants.Expiration.Posts);
            logger.LogInformation("Cached post result for key: {CacheKey}", cacheKey);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error caching post for key: {CacheKey}", cacheKey);
        }

        return postDto;
    }
}