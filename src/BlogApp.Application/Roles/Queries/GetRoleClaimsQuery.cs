namespace BlogApp.Application.Roles.Queries;

public class GetRoleClaimsQuery : IRequest<ApiResponse<IEnumerable<RoleClaimDto>>>
{
    public string RoleId { get; set; } = string.Empty;
}