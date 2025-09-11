namespace BlogApp.Application.Roles.Queries;

public class GetRoleClaimByIdQueryHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<GetRoleClaimByIdQuery, ApiResponse<RoleClaimDto>>
{
    public async Task<ApiResponse<RoleClaimDto>> Handle(GetRoleClaimByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.RoleId);
            if (role == null) return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleNotFound", request.RoleId));

            // Get claims for the role
            var claims = await roleManager.GetClaimsAsync(role);

            // Find the specific claim by index
            // The ID from the frontend corresponds to the index of the claim in the list
            if (request.Id < 0 || request.Id >= claims.Count) return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimNotFound", request.Id.ToString()));

            var claim = claims[request.Id];

            // Convert to DTO
            var roleClaimDto = new RoleClaimDto
            {
                Id = request.Id,
                RoleId = role.Id,
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                CreatedAt = DateTime.UtcNow
            };

            return ApiResponse<RoleClaimDto>.Success(roleClaimDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoleClaimDto>.Failure(messageService.GetMessage("RoleClaimRetrievalError", ex.Message));
        }
    }
}