namespace BlogApp.Domain.Entities;

public class FileEntity : AuditableEntity
{
    public Guid Id { get; set; }
    public required string FileName { get; set; }
    public required string OriginalFileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public required string FilePath { get; set; }
    public string? Description { get; set; }
    public string? UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }
}