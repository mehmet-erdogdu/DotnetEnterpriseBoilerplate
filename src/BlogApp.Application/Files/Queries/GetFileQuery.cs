namespace BlogApp.Application.Files.Queries;

public class GetFileQuery : IRequest<ApiResponse<FileDto>>
{
    public Guid FileId { get; set; }
}