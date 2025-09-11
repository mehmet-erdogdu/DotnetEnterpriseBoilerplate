namespace BlogApp.Application.Users.Commands;

public class RemoveRoleCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<RemoveRoleCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return ApiResponse<string>.Failure(messageService.GetMessage("UserNotFound", request.UserId));

            // Find role by name
            var role = await roleManager.FindByNameAsync(request.RoleName);
            if (role == null) return ApiResponse<string>.Failure(messageService.GetMessage("RoleNotFound", request.RoleName));

            // Check if user has this role
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Contains(request.RoleName)) return ApiResponse<string>.Failure(messageService.GetMessage("UserNotInRole", user.Email!, request.RoleName));

            // Remove role from user
            var result = await userManager.RemoveFromRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<string>.Failure(messageService.GetMessage("RoleRemovalFailed", errors));
            }

            return ApiResponse<string>.Success(messageService.GetMessage("RoleRemovedSuccessfully", user.Email!, request.RoleName));
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure(messageService.GetMessage("RoleRemovalError", ex.Message));
        }
    }
}