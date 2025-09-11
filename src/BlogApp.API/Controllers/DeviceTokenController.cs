namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DeviceTokenController(
    IMediator mediator,
    IMessageService messageService,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    ///     Save device token for current user
    /// </summary>
    [HttpPost("save")]
    public async Task<ApiResponse<bool>> SaveDeviceToken(
        [FromBody] SaveDeviceTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<bool>(ModelState);

        var command = new SaveDeviceTokenCommand
        {
            Token = request.Token,
            Platform = request.Platform,
            UserId = currentUserService.UserId
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Remove device token for the current user
    /// </summary>
    [HttpDelete("remove")]
    public async Task<ApiResponse<bool>> RemoveDeviceToken(
        [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
            return ApiResponse<bool>.Failure(messageService.GetMessage("TokenRequired"));

        var command = new RemoveDeviceTokenCommand
        {
            Token = token,
            UserId = currentUserService.UserId
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Get device tokens for current user
    /// </summary>
    [HttpGet]
    public async Task<ApiResponse<List<string>>> GetDeviceTokens()
    {
        var query = new GetDeviceTokensQuery { UserId = currentUserService.UserId };
        return await mediator.Send(query);
    }

    /// <summary>
    ///     Validate device token
    /// </summary>
    [HttpPost("validate")]
    public async Task<ApiResponse<bool>> ValidateToken(
        [FromBody] ValidateTokenRequestDto request)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<bool>(ModelState);

        var command = new ValidateTokenCommand
        {
            Token = request.Token,
            UserId = currentUserService.UserId
        };

        return await mediator.Send(command);
    }
}