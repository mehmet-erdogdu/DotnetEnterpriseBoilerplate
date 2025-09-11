namespace BlogApp.Application.Auth.Commands;

public class RevokeTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    IMessageService messageService) : IRequestHandler<RevokeTokenCommand, ApiResponse<string>>
{
    public async Task<ApiResponse<string>> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = await refreshTokenService.GetUserIdFromRefreshTokenAsync(request.RefreshToken);
        if (userId == null)
            return ApiResponse<string>.Failure(messageService.GetMessage("InvalidRefreshTokenMessage"));

        // Ensure user can only revoke their own tokens
        if (request.CurrentUserId != userId)
            return ApiResponse<string>.Failure(messageService.GetMessage("ForbiddenAccess"));

        await refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken, request.CurrentUserId, "User revoked");
        return ApiResponse<string>.Success(messageService.GetMessage("TokenRevokedSuccessfully"));
    }
}