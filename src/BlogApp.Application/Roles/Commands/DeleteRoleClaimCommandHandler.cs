namespace BlogApp.Application.Roles.Commands;

public class DeleteRoleClaimCommandHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<DeleteRoleClaimCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(DeleteRoleClaimCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return ApiResponse<string>.Failure(messageService.GetMessage("RoleNotFound", request.RoleId));

            // Get existing claims for the role
            var existingClaims = await roleManager.GetClaimsAsync(role);

            // Find the specific claim to delete by index
            // The ID from the frontend corresponds to the index of the claim in the list
            if (request.Id < 0 || request.Id >= existingClaims.Count) return ApiResponse<string>.Failure(messageService.GetMessage("RoleClaimNotFound", request.Id.ToString()));

            var claimToDelete = existingClaims[request.Id];

            // Remove the claim
            var result = await roleManager.RemoveClaimAsync(role, claimToDelete);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<string>.Failure(messageService.GetMessage("RoleClaimDeletionFailed", errors));
            }

            return ApiResponse<string>.Success(messageService.GetMessage("RoleClaimDeletedSuccessfully"));
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure(messageService.GetMessage("RoleClaimDeletionError", ex.Message));
        }
    }
}