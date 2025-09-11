namespace BlogApp.Application.Roles.Commands;

public class DeleteRoleCommandHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<DeleteRoleCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.Id);
            if (role == null) return ApiResponse<string>.Failure(messageService.GetMessage("RoleNotFound", request.Id));

            // Delete role
            var result = await roleManager.DeleteAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<string>.Failure(messageService.GetMessage("RoleDeletionFailed", errors));
            }

            return ApiResponse<string>.Success(messageService.GetMessage("RoleDeletedSuccessfully"));
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure(messageService.GetMessage("RoleDeletionError", ex.Message));
        }
    }
}