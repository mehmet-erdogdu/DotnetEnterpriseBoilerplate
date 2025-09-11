namespace BlogApp.Application.Posts.Queries;

public class GetAllPostsQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllPostsQuery, IEnumerable<PostDto>>
{
    private const int DefaultPageSize = 10;

    public async Task<IEnumerable<PostDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {
        var allPosts = unitOfWork.Posts.GetAllWithAuthors();

        // Apply search filter if requested
        if (!string.IsNullOrWhiteSpace(request.Search))
            allPosts = allPosts.Where(post => post.Title.Contains(request.Search));

        // Apply pagination if requested
        if (request.Page.HasValue && request.PageSize.HasValue)
        {
            var pageSize = request.PageSize.Value == 0 ? DefaultPageSize : request.PageSize.Value;
            var skip = (request.Page.Value - 1) * pageSize;
            allPosts = allPosts.Skip(skip).Take(pageSize);
        }

        return await allPosts.Select(post => new PostDto
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            AuthorId = post.AuthorId,
            AuthorName = post.Author != null ? $"{post.Author.FirstName} {post.Author.LastName}" : null
        }).ToListAsync(cancellationToken);
    }
}