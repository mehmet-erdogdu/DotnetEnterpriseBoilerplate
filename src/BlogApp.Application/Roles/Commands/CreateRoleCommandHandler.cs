namespace BlogApp.Application.Roles.Commands;

public class CreateRoleCommandHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<CreateRoleCommand, ApiResponse<RoleDto>>
{
    public async Task<ApiResponse<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if role already exists
            var existingRole = await roleManager.FindByNameAsync(request.Name);
            if (existingRole != null) return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleAlreadyExists", request.Name));

            // Create new role
            var role = new IdentityRole(request.Name);
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleCreationFailed", errors));
            }

            // Return role DTO
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
            return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleCreationError", ex.Message));
        }
    }
}