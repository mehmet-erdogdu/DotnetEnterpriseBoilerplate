namespace BlogApp.Application.Roles.Commands;

public class DeleteRoleCommand : IRequest<ApiResponse<string>>
{
    public string Id { get; set; } = string.Empty;
}