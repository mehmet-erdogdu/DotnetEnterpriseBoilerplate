namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SendTopicNotificationCommand : IRequest<ApiResponse<FirebaseNotificationResponseDto>>
{
    public string Topic { get; set; } = string.Empty;
    public FirebaseNotificationDto Notification { get; set; } = new();
    public Dictionary<string, string>? Data { get; set; }
}