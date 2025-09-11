namespace BlogApp.Domain.Common;

public abstract class AuditableEntity : IAuditableEntity, ISoftDeletable
{
    public string CreatedById { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UpdatedByIp { get; set; }
    public string? CreatedByUserAgent { get; set; }
    public string? UpdatedByUserAgent { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedById { get; set; }
}