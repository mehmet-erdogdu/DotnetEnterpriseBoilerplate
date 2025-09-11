namespace BlogApp.Application.Files.Queries;

public class GetUserFilesQuery : IRequest<ApiResponse<IEnumerable<FileDto>>>
{
    public string UserId { get; set; } = string.Empty;
}