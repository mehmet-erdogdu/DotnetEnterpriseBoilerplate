namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SaveDeviceTokenCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<SaveDeviceTokenCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(SaveDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deviceToken = new DeviceTokenDto
            {
                UserId = request.UserId,
                Token = request.Token,
                Platform = request.Platform
            };

            var result = await firebaseNotificationService.SaveDeviceTokenAsync(deviceToken);

            if (result)
                return ApiResponse<bool>.Success(result);

            return ApiResponse<bool>.Failure(messageService.GetMessage("FailedToSaveDeviceTokenMessage"));
        }
        catch (Exception)
        {
            return ApiResponse<bool>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}