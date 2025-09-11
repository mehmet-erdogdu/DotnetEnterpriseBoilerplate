namespace BlogApp.Application.Users.Commands;

public class DeleteUserCommand : IRequest<ApiResponse<string>>
{
    public string Id { get; set; } = string.Empty;
}