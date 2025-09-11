namespace BlogApp.UnitTests.Application.FirebaseNotifications.Commands;

public class ValidateTokenCommandHandlerTests : BaseTestClass
{
    private readonly ValidateTokenCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public ValidateTokenCommandHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new ValidateTokenCommandHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new ValidateTokenCommand
        {
            UserId = "test-user-id",
            Token = "valid-device-token-123"
        };

        _mockFirebaseNotificationService.Setup(x => x.IsTokenValidAsync(command.Token))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockFirebaseNotificationService.Verify(x => x.IsTokenValidAsync(command.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidToken_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ValidateTokenCommand
        {
            UserId = "test-user-id",
            Token = "invalid-token"
        };

        _mockFirebaseNotificationService.Setup(x => x.IsTokenValidAsync(command.Token))
            .ReturnsAsync(false);

        _mockErrorMessageService.Setup(x => x.GetMessage("InvalidDeviceTokenMessage"))
            .Returns("Invalid device token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid device token");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ValidateTokenCommand
        {
            UserId = "test-user-id",
            Token = "device-token-123"
        };

        _mockFirebaseNotificationService.Setup(x => x.IsTokenValidAsync(command.Token))
            .ThrowsAsync(new Exception("Service unavailable"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}