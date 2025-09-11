namespace BlogApp.Domain.Interfaces;

public interface IPostRepository : IGenericRepository<Post>
{
    Task<IEnumerable<Post>> GetPostsByAuthorIdAsync(string authorId);
    Task<Post?> GetPostWithAuthorAsync(Guid postId);
    IQueryable<Post> GetAllWithAuthors();
    Task<int> GetCountByUserIdAsync(string userId);
}