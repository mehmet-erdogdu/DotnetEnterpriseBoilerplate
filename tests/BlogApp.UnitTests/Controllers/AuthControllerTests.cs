namespace BlogApp.UnitTests.Controllers;

public class AuthControllerTests : BaseControllerTest
{
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_mockMediator.Object, _mockCurrentUserService.Object);
    }

    #region Private Helper Methods

    private void SetupMockMediator<TRequest, TResponse>(TResponse response) where TRequest : class
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var expectedResponse = ApiResponse<string>.Success("User registered successfully");
        _mockMediator.Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().Be("User registered successfully");

        _mockMediator.Verify(x => x.Send(It.Is<RegisterCommand>(cmd =>
            cmd.Email == registerDto.Email &&
            cmd.Password == registerDto.Password &&
            cmd.FirstName == registerDto.FirstName &&
            cmd.LastName == registerDto.LastName
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "invalid-email",
            Password = "weak",
            FirstName = "",
            LastName = ""
        };

        _controller.ModelState.AddModelError("Email", "Invalid email format");
        _controller.ModelState.AddModelError("Password", "Password too weak");

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenMediatorReturnsFailure_ShouldReturnFailureResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var expectedResponse = ApiResponse<string>.Failure("Email already exists");
        _mockMediator.Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Email already exists");
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokenResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!"
        };

        var loginResponse = new LoginResponseDto
        {
            AccessToken = "jwt-token",
            RefreshToken = "refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        var expectedResponse = ApiResponse<LoginResponseDto>.Success(loginResponse);
        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("jwt-token");
        result.Data.RefreshToken.Should().Be("refresh-token");

        _mockMediator.Verify(x => x.Send(It.Is<LoginCommand>(cmd =>
            cmd.Email == loginDto.Email &&
            cmd.Password == loginDto.Password
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnFailureResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        var expectedResponse = ApiResponse<LoginResponseDto>.Failure("Invalid credentials");
        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid credentials");
    }

    [Fact]
    public async Task Login_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "",
            Password = ""
        };

        _controller.ModelState.AddModelError("Email", "Email is required");
        _controller.ModelState.AddModelError("Password", "Password is required");

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);

        _mockMediator.Verify(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task Refresh_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "valid-refresh-token"
        };

        var loginResponse = new LoginResponseDto
        {
            AccessToken = "new-jwt-token",
            RefreshToken = "new-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddMinutes(30)
        };

        var expectedResponse = ApiResponse<LoginResponseDto>.Success(loginResponse);
        _mockMediator.Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Refresh(refreshTokenDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().Be("new-jwt-token");
        result.Data.RefreshToken.Should().Be("new-refresh-token");

        _mockMediator.Verify(x => x.Send(It.Is<RefreshTokenCommand>(cmd =>
            cmd.RefreshToken == refreshTokenDto.RefreshToken
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Refresh_WithInvalidRefreshToken_ShouldReturnFailureResponse()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-refresh-token"
        };

        var expectedResponse = ApiResponse<LoginResponseDto>.Failure("Invalid refresh token");
        _mockMediator.Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Refresh(refreshTokenDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid refresh token");
    }

    [Fact]
    public async Task Refresh_WithEmptyRefreshToken_ShouldReturnValidationErrors()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = ""
        };

        _controller.ModelState.AddModelError("RefreshToken", "Refresh token is required");

        // Act
        var result = await _controller.Refresh(refreshTokenDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);

        _mockMediator.Verify(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Revoke Token Tests

    [Fact]
    public async Task Revoke_WithValidRefreshToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "valid-refresh-token"
        };

        var expectedResponse = ApiResponse<string>.Success("Token revoked successfully");
        _mockMediator.Setup(x => x.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        SetupAuthenticatedUser(_controller);

        // Act
        var result = await _controller.Revoke(refreshTokenDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().Be("Token revoked successfully");

        _mockMediator.Verify(x => x.Send(It.Is<RevokeTokenCommand>(cmd =>
            cmd.RefreshToken == refreshTokenDto.RefreshToken &&
            cmd.CurrentUserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Revoke_WithInvalidRefreshToken_ShouldReturnFailureResponse()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            RefreshToken = "invalid-refresh-token"
        };

        var expectedResponse = ApiResponse<string>.Failure("Invalid refresh token");
        _mockMediator.Setup(x => x.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        SetupAuthenticatedUser(_controller);

        // Act
        var result = await _controller.Revoke(refreshTokenDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid refresh token");
    }

    #endregion

    #region Change Password Tests

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldReturnSuccessResponse()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldP@ssw0rd123!",
            NewPassword = "NewP@ssw0rd123!"
        };

        var expectedResponse = ApiResponse<string>.Success("Password changed successfully");
        _mockMediator.Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        SetupAuthenticatedUser(_controller);

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().Be("Password changed successfully");

        _mockMediator.Verify(x => x.Send(It.Is<ChangePasswordCommand>(cmd =>
            cmd.CurrentPassword == changePasswordDto.CurrentPassword &&
            cmd.NewPassword == changePasswordDto.NewPassword &&
            cmd.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangePassword_WithIncorrectCurrentPassword_ShouldReturnFailureResponse()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewP@ssw0rd123!"
        };

        var expectedResponse = ApiResponse<string>.Failure("Current password is incorrect");
        _mockMediator.Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        SetupAuthenticatedUser(_controller);

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Current password is incorrect");
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_ShouldReturnFailureResponse()
    {
        // Arrange
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "OldP@ssw0rd123!",
            NewPassword = "weak"
        };

        var expectedResponse = ApiResponse<string>.Failure("Password does not meet complexity requirements");
        _mockMediator.Setup(x => x.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        SetupAuthenticatedUser(_controller);

        // Act
        var result = await _controller.ChangePassword(changePasswordDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Password does not meet complexity requirements");
    }

    #endregion
}