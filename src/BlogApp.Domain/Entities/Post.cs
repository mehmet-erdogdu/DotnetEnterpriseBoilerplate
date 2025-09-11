namespace BlogApp.Domain.Entities;

public class Post : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string AuthorId { get; set; }
    public ApplicationUser? Author { get; set; }
    public Guid? BannerImageFileId { get; set; }
    public FileEntity? BannerImageFile { get; set; }
}