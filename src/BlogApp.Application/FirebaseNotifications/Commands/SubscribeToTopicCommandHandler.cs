namespace BlogApp.Application.FirebaseNotifications.Commands;

public class SubscribeToTopicCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<SubscribeToTopicCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(SubscribeToTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await firebaseNotificationService.SubscribeToTopicAsync(request.Token, request.Topic);

            if (result)
                return ApiResponse<bool>.Success(result);

            return ApiResponse<bool>.Failure(messageService.GetMessage("FailedToSubscribeToTopicMessage"));
        }
        catch (Exception)
        {
            return ApiResponse<bool>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}