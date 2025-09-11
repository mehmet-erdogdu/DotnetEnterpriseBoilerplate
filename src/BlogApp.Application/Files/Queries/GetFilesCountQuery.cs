namespace BlogApp.Application.Files.Queries;

public record GetFilesCountQuery(string UserId) : IRequest<int>;