namespace BlogApp.Application.Roles.Commands;

public class CreateRoleCommand : IRequest<ApiResponse<RoleDto>>
{
    public string Name { get; set; } = string.Empty;
}