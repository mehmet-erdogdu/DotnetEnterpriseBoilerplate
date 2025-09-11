namespace BlogApp.Application.FirebaseNotifications.Commands;

public class RemoveDeviceTokenCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<RemoveDeviceTokenCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(RemoveDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await firebaseNotificationService.RemoveDeviceTokenAsync(request.UserId, request.Token);

            if (result)
                return ApiResponse<bool>.Success(result);

            return ApiResponse<bool>.Failure(messageService.GetMessage("FailedToRemoveDeviceTokenMessage"));
        }
        catch (Exception)
        {
            return ApiResponse<bool>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}