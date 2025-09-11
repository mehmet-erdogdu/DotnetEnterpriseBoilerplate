namespace BlogApp.Application.Users.Commands;

public class UpdateUserCommand : IRequest<ApiResponse<UserDto>>
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public ICollection<string> Roles { get; set; } = new List<string>();
}