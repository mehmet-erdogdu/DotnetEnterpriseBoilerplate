namespace BlogApp.UnitTests.Application.FirebaseNotifications.Commands;

public class SaveDeviceTokenCommandHandlerTests : BaseTestClass
{
    private readonly SaveDeviceTokenCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public SaveDeviceTokenCommandHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new SaveDeviceTokenCommandHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new SaveDeviceTokenCommand
        {
            UserId = "test-user-id",
            Token = "device-token-123",
            Platform = "iOS"
        };

        _mockFirebaseNotificationService.Setup(x => x.SaveDeviceTokenAsync(It.Is<DeviceTokenDto>(dt =>
                dt.UserId == command.UserId &&
                dt.Token == command.Token &&
                dt.Platform == command.Platform)))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockFirebaseNotificationService.Verify(x => x.SaveDeviceTokenAsync(It.Is<DeviceTokenDto>(dt =>
            dt.UserId == command.UserId &&
            dt.Token == command.Token &&
            dt.Platform == command.Platform)), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsFalse_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SaveDeviceTokenCommand
        {
            UserId = "test-user-id",
            Token = "invalid-token",
            Platform = "Android"
        };

        _mockFirebaseNotificationService.Setup(x => x.SaveDeviceTokenAsync(It.IsAny<DeviceTokenDto>()))
            .ReturnsAsync(false);

        _mockErrorMessageService.Setup(x => x.GetMessage("FailedToSaveDeviceTokenMessage"))
            .Returns("Failed to save device token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Failed to save device token");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SaveDeviceTokenCommand
        {
            UserId = "test-user-id",
            Token = "device-token-123",
            Platform = "iOS"
        };

        _mockFirebaseNotificationService.Setup(x => x.SaveDeviceTokenAsync(It.IsAny<DeviceTokenDto>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}