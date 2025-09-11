namespace BlogApp.UnitTests.Application.FirebaseNotifications.Commands;

public class SubscribeToTopicCommandHandlerTests : BaseTestClass
{
    private readonly SubscribeToTopicCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public SubscribeToTopicCommandHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new SubscribeToTopicCommandHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidTokenAndTopic_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new SubscribeToTopicCommand
        {
            Token = "device-token-123",
            Topic = "news"
        };

        _mockFirebaseNotificationService.Setup(x => x.SubscribeToTopicAsync(command.Token, command.Topic))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockFirebaseNotificationService.Verify(x => x.SubscribeToTopicAsync(command.Token, command.Topic), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsFalse_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SubscribeToTopicCommand
        {
            Token = "device-token-123",
            Topic = "news"
        };

        _mockFirebaseNotificationService.Setup(x => x.SubscribeToTopicAsync(command.Token, command.Topic))
            .ReturnsAsync(false);

        _mockErrorMessageService.Setup(x => x.GetMessage("FailedToSubscribeToTopicMessage"))
            .Returns("Failed to subscribe to topic");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Failed to subscribe to topic");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SubscribeToTopicCommand
        {
            Token = "device-token-123",
            Topic = "news"
        };

        _mockFirebaseNotificationService.Setup(x => x.SubscribeToTopicAsync(command.Token, command.Topic))
            .ThrowsAsync(new Exception("Firebase service error"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}