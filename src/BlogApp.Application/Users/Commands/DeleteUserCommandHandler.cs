namespace BlogApp.Application.Users.Commands;

public class DeleteUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    IMessageService messageService) : IRequestHandler<DeleteUserCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Find user by ID
            var user = await userManager.FindByIdAsync(request.Id);
            if (user == null) return ApiResponse<string>.Failure(messageService.GetMessage("UserNotFound", request.Id));

            // Delete user
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ApiResponse<string>.Failure(messageService.GetMessage("UserDeletionFailed", errors));
            }

            return ApiResponse<string>.Success(messageService.GetMessage("UserDeletedSuccessfully"));
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Failure(messageService.GetMessage("UserDeletionError", ex.Message));
        }
    }
}