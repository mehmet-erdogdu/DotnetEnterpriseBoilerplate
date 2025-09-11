namespace BlogApp.Application.Auth.Commands;

public class RevokeTokenCommand : IRequest<ApiResponse<string>>
{
    public string RefreshToken { get; set; } = string.Empty;
    public string CurrentUserId { get; set; } = string.Empty;
}