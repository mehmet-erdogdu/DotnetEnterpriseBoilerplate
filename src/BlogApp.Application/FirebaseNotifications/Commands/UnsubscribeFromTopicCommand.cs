namespace BlogApp.Application.FirebaseNotifications.Commands;

public class UnsubscribeFromTopicCommand : IRequest<ApiResponse<bool>>
{
    public string Token { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
}