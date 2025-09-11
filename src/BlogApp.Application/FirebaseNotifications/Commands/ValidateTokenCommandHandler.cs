namespace BlogApp.Application.FirebaseNotifications.Commands;

public class ValidateTokenCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<ValidateTokenCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(ValidateTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await firebaseNotificationService.IsTokenValidAsync(request.Token);

            if (result)
                return ApiResponse<bool>.Success(result);

            return ApiResponse<bool>.Failure(messageService.GetMessage("InvalidDeviceTokenMessage"));
        }
        catch (Exception)
        {
            return ApiResponse<bool>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}