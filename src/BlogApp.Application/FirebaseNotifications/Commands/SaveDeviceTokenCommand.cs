namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SaveDeviceTokenCommand : IRequest<ApiResponse<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}