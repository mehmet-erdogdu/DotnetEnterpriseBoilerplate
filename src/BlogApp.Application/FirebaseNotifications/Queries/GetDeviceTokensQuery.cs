namespace BlogApp.Application.FirebaseNotifications.Queries;

public class GetDeviceTokensQuery : IRequest<ApiResponse<List<string>>>
{
    public string UserId { get; set; } = string.Empty;
}