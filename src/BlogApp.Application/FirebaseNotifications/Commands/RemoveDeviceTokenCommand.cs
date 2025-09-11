namespace BlogApp.Application.FirebaseNotifications.Commands;

public class RemoveDeviceTokenCommand : IRequest<ApiResponse<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}