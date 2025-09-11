namespace BlogApp.UnitTests.Application.Auth.Commands;

public class RevokeTokenCommandHandlerTests : BaseTestClass
{
    private readonly RevokeTokenCommandHandler _handler;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;

    public RevokeTokenCommandHandlerTests()
    {
        _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        _mockMessageService = new Mock<IMessageService>();

        _handler = new RevokeTokenCommandHandler(
            _mockRefreshTokenService.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidTokenAndAuthorizedUser_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new RevokeTokenCommand
        {
            RefreshToken = "valid-refresh-token",
            CurrentUserId = "test-user-id"
        };

        _mockRefreshTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(command.CurrentUserId);

        _mockRefreshTokenService.Setup(x => x.RevokeRefreshTokenAsync(command.RefreshToken, command.CurrentUserId, "User revoked"))
            .Returns(Task.CompletedTask);

        _mockMessageService.Setup(x => x.GetMessage("TokenRevokedSuccessfully"))
            .Returns("Token revoked successfully");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().Be("Token revoked successfully");

        _mockRefreshTokenService.Verify(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken), Times.Once);
        _mockRefreshTokenService.Verify(x => x.RevokeRefreshTokenAsync(command.RefreshToken, command.CurrentUserId, "User revoked"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RevokeTokenCommand
        {
            RefreshToken = "invalid-refresh-token",
            CurrentUserId = "test-user-id"
        };

        _mockRefreshTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((string?)null);

        _mockMessageService.Setup(x => x.GetMessage("InvalidRefreshTokenMessage"))
            .Returns("Invalid refresh token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid refresh token");

        _mockRefreshTokenService.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RevokeTokenCommand
        {
            RefreshToken = "valid-refresh-token",
            CurrentUserId = "different-user-id"
        };

        var tokenOwnerId = "token-owner-id";

        _mockRefreshTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(tokenOwnerId);

        _mockMessageService.Setup(x => x.GetMessage("ForbiddenAccess"))
            .Returns("Forbidden access");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Forbidden access");

        _mockRefreshTokenService.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}