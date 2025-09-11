namespace BlogApp.Application.Roles.Commands;

public class AddRoleClaimCommand : IRequest<ApiResponse<RoleClaimDto>>
{
    public string RoleId { get; set; } = string.Empty;
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
}