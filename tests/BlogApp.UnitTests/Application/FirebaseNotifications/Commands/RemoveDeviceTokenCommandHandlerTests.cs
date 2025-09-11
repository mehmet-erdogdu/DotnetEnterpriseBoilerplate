namespace BlogApp.UnitTests.Application.FirebaseNotifications.Commands;

public class RemoveDeviceTokenCommandHandlerTests : BaseTestClass
{
    private readonly RemoveDeviceTokenCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public RemoveDeviceTokenCommandHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new RemoveDeviceTokenCommandHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new RemoveDeviceTokenCommand
        {
            UserId = "test-user-id",
            Token = "device-token-123"
        };

        _mockFirebaseNotificationService.Setup(x => x.RemoveDeviceTokenAsync(command.UserId, command.Token))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockFirebaseNotificationService.Verify(x => x.RemoveDeviceTokenAsync(command.UserId, command.Token), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsFalse_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RemoveDeviceTokenCommand
        {
            UserId = "test-user-id",
            Token = "invalid-token"
        };

        _mockFirebaseNotificationService.Setup(x => x.RemoveDeviceTokenAsync(command.UserId, command.Token))
            .ReturnsAsync(false);

        _mockErrorMessageService.Setup(x => x.GetMessage("FailedToRemoveDeviceTokenMessage"))
            .Returns("Failed to remove device token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Failed to remove device token");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RemoveDeviceTokenCommand
        {
            UserId = "test-user-id",
            Token = "device-token-123"
        };

        _mockFirebaseNotificationService.Setup(x => x.RemoveDeviceTokenAsync(command.UserId, command.Token))
            .ThrowsAsync(new Exception("Database connection failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}