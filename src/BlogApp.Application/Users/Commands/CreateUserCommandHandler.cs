namespace BlogApp.Application.Users.Commands;

public class CreateUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<CreateUserCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(request.Email);
            if (existingUser != null) return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserAlreadyExists", request.Email));

            // Create new user
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserCreationFailed", errors));
            }

            // Assign roles if provided
            if (request.Roles.Count > 0)
                foreach (var roleName in request.Roles)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role != null) await userManager.AddToRoleAsync(user, roleName);
                }

            // Get user roles
            var userRoles = await userManager.GetRolesAsync(user);

            // Return user DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                Roles = userRoles.ToList()
            };

            return ApiResponse<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserCreationError", ex.Message));
        }
    }
}