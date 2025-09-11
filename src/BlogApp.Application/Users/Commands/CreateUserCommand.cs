namespace BlogApp.Application.Users.Commands;

public class CreateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public ICollection<string> Roles { get; set; } = new List<string>();
}