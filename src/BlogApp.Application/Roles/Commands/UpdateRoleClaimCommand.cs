namespace BlogApp.Application.Roles.Commands;

public class UpdateRoleClaimCommand : IRequest<ApiResponse<RoleClaimDto>>
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}