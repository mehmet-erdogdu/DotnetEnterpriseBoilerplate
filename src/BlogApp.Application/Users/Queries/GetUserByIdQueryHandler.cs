namespace BlogApp.Application.Users.Queries;

public class GetUserByIdQueryHandler(
    UserManager<ApplicationUser> userManager,
    IMessageService messageService) : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await userManager.FindByIdAsync(request.Id);
            if (user == null) return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserNotFound", request.Id));

            // Get user roles
            var roles = await userManager.GetRolesAsync(user);

            // Convert to DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = roles.ToList()
            };

            return ApiResponse<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserRetrievalError", ex.Message));
        }
    }
}