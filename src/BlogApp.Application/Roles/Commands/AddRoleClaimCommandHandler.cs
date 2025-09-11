namespace BlogApp.Application.Roles.Commands;

public class AddRoleClaimCommandHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<AddRoleClaimCommand, ApiResponse<RoleClaimDto>>
{
    public async Task<ApiResponse<RoleClaimDto>> Handle(AddRoleClaimCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleNotFound", request.RoleId));

            // Create claim
            var claim = new Claim(request.ClaimType, request.ClaimValue);

            // Add claim to role
            var result = await roleManager.AddClaimAsync(role, claim);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimAdditionFailed", errors));
            }

            // Get the added claim to return in response
            var roleClaims = await roleManager.GetClaimsAsync(role);
            var addedClaim = roleClaims.FirstOrDefault(c => c.Type == request.ClaimType && c.Value == request.ClaimValue);

            if (addedClaim == null) return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimNotFoundAfterAddition"));

            // Return role claim DTO
            var roleClaimDto = new RoleClaimDto
            {
                RoleId = role.Id,
                ClaimType = addedClaim.Type,
                ClaimValue = addedClaim.Value,
                CreatedAt = DateTime.UtcNow
            };

            return ApiResponse<RoleClaimDto>.Success(roleClaimDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimAdditionError", ex.Message));
        }
    }
}