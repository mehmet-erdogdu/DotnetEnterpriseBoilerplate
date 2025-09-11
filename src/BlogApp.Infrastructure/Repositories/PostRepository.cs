namespace BlogApp.Infrastructure.Repositories;

public class PostRepository(ApplicationDbContext context) : GenericRepository<Post>(context), IPostRepository
{
    public async Task<IEnumerable<Post>> GetPostsByAuthorIdAsync(string authorId)
    {
        return await context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task<Post?> GetPostWithAuthorAsync(Guid postId)
    {
        return await context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public IQueryable<Post> GetAllWithAuthors()
    {
        return context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.CreatedAt)
            .AsQueryable();
    }

    public async Task<int> GetCountByUserIdAsync(string userId)
    {
        return await context.Posts
            .CountAsync(p => p.AuthorId == userId);
    }
}