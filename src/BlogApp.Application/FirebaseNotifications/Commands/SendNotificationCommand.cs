namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SendNotificationCommand : IRequest<ApiResponse<FirebaseNotificationResponseDto>>
{
    public FirebaseNotificationRequestDto Request { get; set; } = new();
}