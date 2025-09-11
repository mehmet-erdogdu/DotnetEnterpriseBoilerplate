namespace BlogApp.Application.Auth.Commands;

public class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IPasswordService passwordService,
    IRefreshTokenService refreshTokenService,
    IMessageService messageService) : IRequestHandler<ChangePasswordCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return ApiResponse<string>.Failure(messageService.GetMessage("UserNotFound"));

        // Verify the current password
        var isCurrentPasswordValid = await userManager.CheckPasswordAsync(user, request.CurrentPassword);
        if (!isCurrentPasswordValid)
            return ApiResponse<string>.Failure(messageService.GetMessage("CurrentPasswordIncorrect"));

        // Check if a new password was recently used
        var newPasswordHash = userManager.PasswordHasher.HashPassword(user, request.NewPassword);
        if (await passwordService.IsPasswordRecentlyUsedAsync(request.UserId, newPasswordHash))
            return ApiResponse<string>.Failure(messageService.GetMessage("PasswordRecentlyUsed"));

        // Change password
        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return ApiResponse<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Track password change
        await passwordService.TrackPasswordChangeAsync(request.UserId, newPasswordHash);

        // Revoke all refresh tokens for security
        await refreshTokenService.RevokeAllUserTokensAsync(request.UserId, request.UserId, "Password changed");

        return ApiResponse<string>.Success(messageService.GetMessage("PasswordChangedSuccessfully"));
    }
}