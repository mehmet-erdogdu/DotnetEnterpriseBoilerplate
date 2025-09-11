namespace BlogApp.Application.Roles.Queries;

public class GetRoleByIdQuery : IRequest<ApiResponse<RoleDto>>
{
    public string Id { get; set; } = string.Empty;
}