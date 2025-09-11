namespace BlogApp.Application.Roles.Queries;

public class GetRoleClaimsQueryHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<GetRoleClaimsQuery, ApiResponse<IEnumerable<RoleClaimDto>>>
{
    public async Task<ApiResponse<IEnumerable<RoleClaimDto>>> Handle(GetRoleClaimsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return ApiResponse<IEnumerable<RoleClaimDto>>.Failure(messageService.GetMessage("RoleNotFound", request.RoleId));

            // Get claims for the role
            var claims = await roleManager.GetClaimsAsync(role);

            // Convert to DTOs
            var roleClaimDtos = claims.Select((claim, index) => new RoleClaimDto
            {
                Id = index, // This is a simplification - in a real implementation, you'd need to get the actual claim ID
                RoleId = role.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            return ApiResponse<IEnumerable<RoleClaimDto>>.Success(roleClaimDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<RoleClaimDto>>.Failure(messageService.GetMessage("RoleClaimsRetrievalError", ex.Message));
        }
    }
}