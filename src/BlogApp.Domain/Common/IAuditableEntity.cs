namespace BlogApp.Domain.Common;

public interface IAuditableEntity
{
    string CreatedById { get; set; }
    DateTime CreatedAt { get; set; }
    string? UpdatedById { get; set; }
    DateTime? UpdatedAt { get; set; }
    string? CreatedByIp { get; set; }
    string? UpdatedByIp { get; set; }
    string? CreatedByUserAgent { get; set; }
    string? UpdatedByUserAgent { get; set; }
}