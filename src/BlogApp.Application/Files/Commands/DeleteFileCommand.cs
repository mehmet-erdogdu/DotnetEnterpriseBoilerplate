namespace BlogApp.Application.Files.Commands;

public class DeleteFileCommand : IRequest<ApiResponse<bool>>
{
    public Guid FileId { get; set; }
    public string UserId { get; set; } = string.Empty;
}