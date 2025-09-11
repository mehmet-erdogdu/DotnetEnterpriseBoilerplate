namespace BlogApp.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = default!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = default!; // Create, Update, Delete
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public string UserId { get; set; } = default!;
    public string? UserIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}