namespace BlogApp.Application.Files.Queries;

public class DownloadFileQuery : IRequest<DownloadFileResult>
{
    public Guid FileId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class DownloadFileResult
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}