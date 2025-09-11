namespace BlogApp.Application.FirebaseNotifications.Queries;

public class GetDeviceTokensQueryHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<GetDeviceTokensQuery, ApiResponse<List<string>>>
{
    public async Task<ApiResponse<List<string>>> Handle(GetDeviceTokensQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var tokens = await firebaseNotificationService.GetUserDeviceTokensAsync(request.UserId);
            return ApiResponse<List<string>>.Success(tokens);
        }
        catch (Exception)
        {
            return ApiResponse<List<string>>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}