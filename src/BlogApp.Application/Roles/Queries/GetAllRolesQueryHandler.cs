namespace BlogApp.Application.Roles.Queries;

public class GetAllRolesQueryHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<GetAllRolesQuery, ApiResponse<IEnumerable<RoleDto>>>
{
    public Task<ApiResponse<IEnumerable<RoleDto>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all roles
            var roles = roleManager.Roles;

            // Convert to DTOs
            var roleDtos = roles.Select(role => new RoleDto
            {
                Id = role.Id,
                Name = role.Name!,
                NormalizedName = role.NormalizedName!
            }).ToList();

            return Task.FromResult(ApiResponse<IEnumerable<RoleDto>>.Success(roleDtos));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ApiResponse<IEnumerable<RoleDto>>.Failure(messageService.GetMessage("RolesRetrievalError", ex.Message)));
        }
    }
}