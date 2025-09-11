namespace BlogApp.Application.Users.Commands;

public class UpdateUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<UpdateUserCommand, ApiResponse<UserDto>>
{
    public async Task<ApiResponse<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await userManager.FindByIdAsync(request.Id);
            if (user == null) return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserNotFound", request.Id));

            // Update user properties
            user.Email = request.Email;
            user.UserName = request.Email;
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.EmailConfirmed = request.EmailConfirmed;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserUpdateFailed", errors));
            }

            // Update roles if provided
            if (request.Roles != null)
            {
                // Get current roles
                var currentRoles = await userManager.GetRolesAsync(user);

                // Remove roles that are not in the new list
                var rolesToRemove = currentRoles.Except(request.Roles);
                if (rolesToRemove.Any()) await userManager.RemoveFromRolesAsync(user, rolesToRemove);

                // Add roles that are not in the current list
                var rolesToAdd = request.Roles.Except(currentRoles);
                foreach (var roleName in rolesToAdd)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role != null) await userManager.AddToRoleAsync(user, roleName);
                }
            }

            // Get updated user roles
            var userRoles = await userManager.GetRolesAsync(user);

            // Return updated user DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailConfirmed = user.EmailConfirmed,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                Roles = userRoles.ToList()
            };

            return ApiResponse<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserDto>.Failure(messageService.GetMessage("UserUpdateError", ex.Message));
        }
    }
}