namespace BlogApp.Application.Roles.Commands;

public class DeleteRoleClaimCommand : IRequest<ApiResponse<string>>
{
    public int Id { get; set; }
    public string RoleId { get; set; } = string.Empty;
}