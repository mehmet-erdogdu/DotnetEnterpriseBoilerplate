namespace BlogApp.Application.Roles.Queries;

public class GetRoleClaimByIdQuery : IRequest<ApiResponse<RoleClaimDto>>
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
}