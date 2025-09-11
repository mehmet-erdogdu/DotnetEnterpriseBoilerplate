namespace BlogApp.Application.Users.Commands;

public class RemoveRoleCommand : IRequest<ApiResponse<string>>
{
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}