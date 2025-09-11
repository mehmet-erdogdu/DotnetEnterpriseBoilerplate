namespace BlogApp.UnitTests.Controllers;

public class DeviceTokenControllerTests : BaseControllerTest
{
    private const string TestUserId = "test-user-id";
    private readonly DeviceTokenController _controller;

    public DeviceTokenControllerTests()
    {
        _controller = new DeviceTokenController(_mockMediator.Object, _mockMessageService.Object, _mockCurrentUserService.Object);
        SetupAuthenticatedUser(_controller);
    }

    #region Private Helper Methods

    private void SetupMockMediator<TRequest, TResponse>(TResponse response) where TRequest : class
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    #endregion

    #region GetDeviceTokens Tests

    [Fact]
    public async Task GetDeviceTokens_WithValidUser_ShouldReturnTokenList()
    {
        // Arrange
        var tokens = new List<string> { "token1", "token2", "token3" };
        var expectedResponse = ApiResponse<List<string>>.Success(tokens);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetDeviceTokensQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetDeviceTokens();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data.Should().Contain("token1");
        result.Data.Should().Contain("token2");
        result.Data.Should().Contain("token3");

        _mockMediator.Verify(x => x.Send(It.Is<GetDeviceTokensQuery>(query =>
            query.UserId == TestUserId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region SaveDeviceToken Tests

    [Fact]
    public async Task SaveDeviceToken_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestDto = new SaveDeviceTokenRequestDto
        {
            Token = "device-token-123",
            Platform = "iOS"
        };

        var expectedResponse = ApiResponse<bool>.Success(true);
        _mockMediator.Setup(x => x.Send(It.IsAny<SaveDeviceTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SaveDeviceToken(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockMediator.Verify(x => x.Send(It.Is<SaveDeviceTokenCommand>(cmd =>
            cmd.Token == requestDto.Token &&
            cmd.Platform == requestDto.Platform &&
            cmd.UserId == TestUserId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SaveDeviceToken_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var requestDto = new SaveDeviceTokenRequestDto
        {
            Token = "",
            Platform = ""
        };

        _controller.ModelState.AddModelError("Token", "Token is required");
        _controller.ModelState.AddModelError("Platform", "Platform is required");

        // Act
        var result = await _controller.SaveDeviceToken(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<SaveDeviceTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region RemoveDeviceToken Tests

    [Fact]
    public async Task RemoveDeviceToken_WithValidToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var token = "device-token-123";
        var expectedResponse = ApiResponse<bool>.Success(true);
        _mockMediator.Setup(x => x.Send(It.IsAny<RemoveDeviceTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RemoveDeviceToken(token);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockMediator.Verify(x => x.Send(It.Is<RemoveDeviceTokenCommand>(cmd =>
            cmd.Token == token &&
            cmd.UserId == TestUserId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveDeviceToken_WithEmptyToken_ShouldReturnFailureResponse()
    {
        // Arrange
        var token = "";
        _mockMessageService.Setup(x => x.GetMessage("TokenRequired"))
            .Returns("Token is required");

        // Act
        var result = await _controller.RemoveDeviceToken(token);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Token is required");

        _mockMediator.Verify(x => x.Send(It.IsAny<RemoveDeviceTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region ValidateToken Tests

    [Fact]
    public async Task ValidateToken_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestDto = new ValidateTokenRequestDto
        {
            Token = "device-token-123"
        };

        var expectedResponse = ApiResponse<bool>.Success(true);
        _mockMediator.Setup(x => x.Send(It.IsAny<ValidateTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ValidateToken(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockMediator.Verify(x => x.Send(It.Is<ValidateTokenCommand>(cmd =>
            cmd.Token == requestDto.Token &&
            cmd.UserId == TestUserId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidateToken_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var requestDto = new ValidateTokenRequestDto
        {
            Token = ""
        };

        _controller.ModelState.AddModelError("Token", "Token is required");

        // Act
        var result = await _controller.ValidateToken(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<ValidateTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}