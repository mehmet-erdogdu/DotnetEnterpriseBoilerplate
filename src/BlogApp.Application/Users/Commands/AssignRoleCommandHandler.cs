namespace BlogApp.Application.Users.Commands;

public class AssignRoleCommandHandler(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<AssignRoleCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await userManager.FindByIdAsync(request.UserId);
            if (user == null) return ApiResponse<string>.Failure(messageService.GetMessage("UserNotFound", request.UserId));

            // Find role by name
            var role = await roleManager.FindByNameAsync(request.RoleName);
            if (role == null) return ApiResponse<string>.Failure(messageService.GetMessage("RoleNotFound", request.RoleName));

            // Check if user already has this role
            var userRoles = await userManager.GetRolesAsync(user);
            if (userRoles.Contains(request.RoleName)) return ApiResponse<string>.Failure(messageService.GetMessage("UserAlreadyInRole", user.Email!, request.RoleName));

            // Assign role to user
            var result = await userManager.AddToRoleAsync(user, request.RoleName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<string>.Failure(messageService.GetMessage("RoleAssignmentFailed", errors));
            }

            return ApiResponse<string>.Success(messageService.GetMessage("RoleAssignedSuccessfully", user.Email!, request.RoleName));
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure(messageService.GetMessage("RoleAssignmentError", ex.Message));
        }
    }
}