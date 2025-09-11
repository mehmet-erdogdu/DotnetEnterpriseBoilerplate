namespace BlogApp.Application.Roles.Queries;

public class GetRoleByIdQueryHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<GetRoleByIdQuery, ApiResponse<RoleDto>>
{
    public async Task<ApiResponse<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.Id);
            if (role == null) return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleNotFound", request.Id));

            // Convert to DTO
            var roleDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                NormalizedName = role.NormalizedName!
            };

            return ApiResponse<RoleDto>.Success(roleDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleRetrievalError", ex.Message));
        }
    }
}