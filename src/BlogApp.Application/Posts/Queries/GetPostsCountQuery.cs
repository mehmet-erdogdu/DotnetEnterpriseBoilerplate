namespace BlogApp.Application.Posts.Queries;

public record GetPostsCountQuery(string UserId) : IRequest<int>;