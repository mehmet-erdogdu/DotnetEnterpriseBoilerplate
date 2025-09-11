namespace BlogApp.Application.Posts.Commands;

public class CreatePostCommandHandler(
    IUnitOfWork unitOfWork,
    ICacheInvalidationService cacheInvalidationService,
    ILogger<CreatePostCommandHandler> logger,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreatePostCommand, PostDto>
{
    public async Task<PostDto> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var post = new Post
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = currentUserService.UserId,
                CreatedAt = DateTime.UtcNow
            };

            await unitOfWork.Posts.AddAsync(post);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            // Invalidate cache after successful creation
            await cacheInvalidationService.InvalidatePostCacheAsync();
            logger.LogInformation("Created new post and invalidated cache: {PostId}", post.Id);

            return new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                AuthorId = post.AuthorId
            };
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}