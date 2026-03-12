namespace BlogApp.Application.Users.Queries;

public class GetAllUsersQueryHandler(
    UserManager<ApplicationUser> userManager,
    IMessageService messageService) : IRequestHandler<GetAllUsersQuery, ApiResponse<IEnumerable<UserDto>>>
{
    public async Task<ApiResponse<IEnumerable<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get users with pagination
            var users = userManager.Users;

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.Search))
                users = users.Where(u =>
                    u.Email!.Contains(request.Search) ||
                    u.FirstName.Contains(request.Search) ||
                    u.LastName.Contains(request.Search));

            // Apply pagination
            var data = await users
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Convert to DTOs
            var userDtos = new List<UserDto>();
            foreach (var user in data)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailConfirmed = user.EmailConfirmed,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    Roles = roles.ToList()
                });
            }

            return ApiResponse<IEnumerable<UserDto>>.Success(userDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<UserDto>>.Failure(messageService.GetMessage("UsersRetrievalError", ex.Message));
        }
    }
}