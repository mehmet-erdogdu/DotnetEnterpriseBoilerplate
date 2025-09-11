namespace BlogApp.Application.Posts.Commands;

public class UpdatePostCommandHandler(
    IUnitOfWork unitOfWork,
    ICacheInvalidationService cacheInvalidationService,
    ILogger<UpdatePostCommandHandler> logger)
    : IRequestHandler<UpdatePostCommand, PostDto?>
{
    public async Task<PostDto?> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var post = await unitOfWork.Posts.GetPostWithAuthorAsync(request.Id);
            if (post == null) return null;

            post.Title = request.Title;
            post.Content = request.Content;

            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            // Invalidate cache after successful update
            await cacheInvalidationService.InvalidatePostCacheAsync(request.Id);
            logger.LogInformation("Updated post and invalidated cache: {PostId}", request.Id);

            return new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorId = post.AuthorId,
                AuthorName = post.Author != null ? $"{post.Author.FirstName} {post.Author.LastName}" : null
            };
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}