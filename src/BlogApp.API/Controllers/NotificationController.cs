namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationController(
    IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Send notification to specific device tokens
    /// </summary>
    [HttpPost("send")]
    public async Task<ApiResponse<FirebaseNotificationResponseDto>> SendNotification(
        [FromBody] FirebaseNotificationRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<FirebaseNotificationResponseDto>(ModelState);

        var command = new SendNotificationCommand { Request = request };
        return await mediator.Send(command);
    }

    /// <summary>
    ///     Send notification to a topic
    /// </summary>
    [HttpPost("send-topic")]
    public async Task<ApiResponse<FirebaseNotificationResponseDto>> SendTopicNotification(
        [FromBody] TopicNotificationRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<FirebaseNotificationResponseDto>(ModelState);

        var command = new SendTopicNotificationCommand
        {
            Topic = request.Topic,
            Notification = request.Notification,
            Data = request.Data
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Subscribe device token to a topic
    /// </summary>
    [HttpPost("subscribe-topic")]
    public async Task<ApiResponse<bool>> SubscribeToTopic(
        [FromBody] TopicSubscriptionRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<bool>(ModelState);

        var command = new SubscribeToTopicCommand
        {
            Token = request.Token,
            Topic = request.Topic
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Unsubscribe device token from a topic
    /// </summary>
    [HttpPost("unsubscribe-topic")]
    public async Task<ApiResponse<bool>> UnsubscribeFromTopic(
        [FromBody] TopicSubscriptionRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<bool>(ModelState);

        var command = new UnsubscribeFromTopicCommand
        {
            Token = request.Token,
            Topic = request.Topic
        };

        return await mediator.Send(command);
    }
}