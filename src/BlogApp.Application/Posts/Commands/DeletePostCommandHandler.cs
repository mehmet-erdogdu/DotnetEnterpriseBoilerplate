namespace BlogApp.Application.Posts.Commands;

public class DeletePostCommandHandler(
    IUnitOfWork unitOfWork,
    ICacheInvalidationService cacheInvalidationService,
    ILogger<DeletePostCommandHandler> logger)
    : IRequestHandler<DeletePostCommand, bool>
{
    public async Task<bool> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync();
        try
        {
            var post = await unitOfWork.Posts.GetByIdAsync(request.Id);
            if (post == null)
                return false;

            unitOfWork.Posts.Remove(post);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            // Invalidate cache after successful deletion
            await cacheInvalidationService.InvalidatePostCacheAsync(request.Id);
            logger.LogInformation("Deleted post and invalidated cache: {PostId}", request.Id);

            return true;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}