namespace BlogApp.Application.Users.Queries;

public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    public string Id { get; set; } = string.Empty;
}