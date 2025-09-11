namespace BlogApp.Application.FirebaseNotifications.Commands;

public class UnsubscribeFromTopicCommandHandler(
    IFirebaseNotificationService firebaseNotificationService,
    IMessageService messageService) : IRequestHandler<UnsubscribeFromTopicCommand, ApiResponse<bool>>
{
    public async Task<ApiResponse<bool>> Handle(UnsubscribeFromTopicCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await firebaseNotificationService.UnsubscribeFromTopicAsync(request.Token, request.Topic);

            if (result)
                return ApiResponse<bool>.Success(result);

            return ApiResponse<bool>.Failure(messageService.GetMessage("FailedToUnsubscribeFromTopicMessage"));
        }
        catch (Exception)
        {
            return ApiResponse<bool>.Failure(messageService.GetMessage("InternalServerError"));
        }
    }
}