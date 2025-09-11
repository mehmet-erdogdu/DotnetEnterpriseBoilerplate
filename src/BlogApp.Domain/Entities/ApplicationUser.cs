namespace BlogApp.Domain.Entities;

public class ApplicationUser : IdentityUser, IAuditableEntity
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
    public ICollection<PasswordHistory> PasswordHistory { get; set; } = new List<PasswordHistory>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // ISoftDeletable implementation
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedById { get; set; }

    // IAuditableEntity implementation
    public string CreatedById { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public string? UpdatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public string? UpdatedByIp { get; set; }
    public string? CreatedByUserAgent { get; set; }
    public string? UpdatedByUserAgent { get; set; }
}