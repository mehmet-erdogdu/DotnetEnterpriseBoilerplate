namespace BlogApp.Domain.Entities;

public class RefreshToken : AuditableEntity
{
    public Guid Id { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokedBy { get; set; }

    public string? RevokedReason { get; set; }

    // Navigation property
    public ApplicationUser User { get; set; } = null!;
}