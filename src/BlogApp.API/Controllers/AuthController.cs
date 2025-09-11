namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IMediator mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    /// <summary>
    ///     Registers a new user
    /// </summary>
    /// <param name="model">User registration information</param>
    /// <returns>Success message if registration is successful, or validation errors in ApiResponse format</returns>
    /// <response code="200">Returns a success message if registration is successful, or validation errors in the error field</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<ApiResponse<string>> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<string>(ModelState);
        var command = new RegisterCommand
        {
            Email = model.Email,
            Password = model.Password,
            FirstName = model.FirstName,
            LastName = model.LastName
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Authenticates a user and returns a JWT token
    /// </summary>
    /// <param name="model">User login credentials</param>
    /// <returns>JWT token and refresh token if authentication is successful</returns>
    /// <response code="200">Returns the JWT token and refresh token if authentication is successful</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<ApiResponse<LoginResponseDto>> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<LoginResponseDto>(ModelState);
        var command = new LoginCommand
        {
            Email = model.Email,
            Password = model.Password
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Refreshes an access token using a refresh token
    /// </summary>
    /// <param name="model">Refresh token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Returns new tokens if refresh is successful</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<ApiResponse<LoginResponseDto>> Refresh([FromBody] RefreshTokenDto model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<LoginResponseDto>(ModelState);
        var command = new RefreshTokenCommand
        {
            RefreshToken = model.RefreshToken
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Revokes a refresh token
    /// </summary>
    /// <param name="model">Refresh token to revoke</param>
    /// <returns>>The success message if revocation is successful</returns>
    /// <response code="200">Returns the success message if revocation is successful</response>
    [HttpPost("revoke")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize]
    public async Task<ApiResponse<string>> Revoke([FromBody] RefreshTokenDto model)
    {
        var command = new RevokeTokenCommand
        {
            RefreshToken = model.RefreshToken,
            CurrentUserId = currentUserService.UserId
        };

        return await mediator.Send(command);
    }

    /// <summary>
    ///     Changes user password
    /// </summary>
    /// <param name="model">Password change information</param>
    /// <returns>Success message if the password change is successful</returns>
    /// <response code="200">Returns the success message if the password change is successful</response>
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize]
    [EnableRateLimiting("AuthLimiter")]
    public async Task<ApiResponse<string>> ChangePassword([FromBody] ChangePasswordDto model)
    {
        var command = new ChangePasswordCommand
        {
            CurrentPassword = model.CurrentPassword,
            NewPassword = model.NewPassword,
            UserId = currentUserService.UserId
        };

        return await mediator.Send(command);
    }
}