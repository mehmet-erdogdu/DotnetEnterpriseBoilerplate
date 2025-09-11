namespace BlogApp.Application.Roles.Commands;

public class UpdateRoleClaimCommandHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<UpdateRoleClaimCommand, ApiResponse<RoleClaimDto>>
{
    public async Task<ApiResponse<RoleClaimDto>> Handle(UpdateRoleClaimCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleNotFound", request.RoleId));

            // Get existing claims for the role
            var existingClaims = await roleManager.GetClaimsAsync(role);

            // Find the specific claim to update (using a combination of type and value as identifier)
            // In a real implementation, you might want to use a more robust way to identify claims
            var existingClaim = existingClaims.FirstOrDefault(c => c.Type == request.ClaimType);

            if (existingClaim == null) return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimNotFound", request.ClaimType));

            // Remove the existing claim
            var removeResult = await roleManager.RemoveClaimAsync(role, existingClaim);
            if (!removeResult.Succeeded)
            {
                var errors = string.Join(", ", removeResult.Errors.Select(e => e.Description));
                return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimRemovalFailed", errors));
            }

            // Add the updated claim
            var newClaim = new Claim(request.ClaimType, request.ClaimValue);
            var addResult = await roleManager.AddClaimAsync(role, newClaim);
            if (!addResult.Succeeded)
            {
                var errors = string.Join(", ", addResult.Errors.Select(e => e.Description));
                return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimAdditionFailed", errors));
            }

            // Return updated role claim DTO
            var roleClaimDto = new RoleClaimDto
            {
                Id = request.Id,
                RoleId = role.Id,
                ClaimType = newClaim.Type,
                ClaimValue = newClaim.Value,
                UpdatedAt = DateTime.UtcNow
            };

            return ApiResponse<RoleClaimDto>.Success(roleClaimDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimUpdateError", ex.Message));
        }
    }
}