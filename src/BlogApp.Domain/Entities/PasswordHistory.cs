namespace BlogApp.Domain.Entities;

public class PasswordHistory : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public DateTime ChangedAt { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}