namespace BlogApp.Application.Users.Queries;

public class GetAllUsersQuery : IRequest<ApiResponse<IEnumerable<UserDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
}