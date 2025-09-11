namespace BlogApp.Application.Posts.Queries;

public class GetPostsCountQueryHandler(IPostRepository postRepository) : IRequestHandler<GetPostsCountQuery, int>
{
    public async Task<int> Handle(GetPostsCountQuery request, CancellationToken cancellationToken)
    {
        return await postRepository.GetCountByUserIdAsync(request.UserId);
    }
}