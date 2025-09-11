namespace BlogApp.Application.Roles.Commands;

public class UpdateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}