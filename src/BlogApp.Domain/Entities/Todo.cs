namespace BlogApp.Domain.Entities;

public class Todo : AuditableEntity
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public required string UserId { get; set; }
    public ApplicationUser? User { get; set; }
}