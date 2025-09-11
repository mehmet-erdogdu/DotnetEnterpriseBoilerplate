namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SendTopicNotificationCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<SendTopicNotificationCommand, ApiResponse<FirebaseNotificationResponseDto>>
{
    public async Task<ApiResponse<FirebaseNotificationResponseDto>> Handle(SendTopicNotificationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await firebaseNotificationService.SendNotificationToTopicAsync(
                request.Topic,
                request.Notification,
                request.Data);

            if (result.Success)
                return ApiResponse<FirebaseNotificationResponseDto>.Success(result);

            return ApiResponse<FirebaseNotificationResponseDto>.Failure(messageService.GetMessage("FailedToSendTopicNotificationMessage"));
        }
        catch (Exception)
        {
            return ApiResponse<FirebaseNotificationResponseDto>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}