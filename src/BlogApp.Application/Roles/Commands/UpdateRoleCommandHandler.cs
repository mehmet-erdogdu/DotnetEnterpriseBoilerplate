namespace BlogApp.Application.Roles.Commands;

public class UpdateRoleCommandHandler(
    RoleManager<IdentityRole> roleManager,
    IMessageService messageService) : IRequestHandler<UpdateRoleCommand, ApiResponse<RoleDto>>
{
    public async Task<ApiResponse<RoleDto>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find role by ID
            var role = await roleManager.FindByIdAsync(request.Id);
            if (role == null) return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleNotFound", request.Id));

            // Check if another role with the same name already exists
            var existingRole = await roleManager.FindByNameAsync(request.Name);
            if (existingRole != null && existingRole.Id != request.Id) return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleNameAlreadyExists", request.Name));

            // Update role properties
            role.Name = request.Name;
            role.NormalizedName = request.Name.ToUpperInvariant();

            var result = await roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleUpdateFailed", errors));
            }

            // Return updated role DTO
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
            return ApiResponse<RoleDto>.Failure(messageService.GetMessage("RoleUpdateError", ex.Message));
        }
    }
}