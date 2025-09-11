namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SendNotificationCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<SendNotificationCommand, ApiResponse<FirebaseNotificationResponseDto>>
{
    public async Task<ApiResponse<FirebaseNotificationResponseDto>> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await firebaseNotificationService.SendNotificationAsync(request.Request);

            if (response.Success)
                return ApiResponse<FirebaseNotificationResponseDto>.Success(response);

            return ApiResponse<FirebaseNotificationResponseDto>.Failure(response.Message);
        }
        catch (Exception)
        {
            return ApiResponse<FirebaseNotificationResponseDto>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}