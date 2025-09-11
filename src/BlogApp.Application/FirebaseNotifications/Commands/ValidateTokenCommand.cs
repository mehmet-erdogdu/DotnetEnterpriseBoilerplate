namespace BlogApp.Application.FirebaseNotifications.Commands;

public class ValidateTokenCommand : IRequest<ApiResponse<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}