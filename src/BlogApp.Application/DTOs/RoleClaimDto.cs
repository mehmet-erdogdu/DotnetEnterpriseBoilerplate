namespace BlogApp.Application.DTOs;

public class RoleClaimDto
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}