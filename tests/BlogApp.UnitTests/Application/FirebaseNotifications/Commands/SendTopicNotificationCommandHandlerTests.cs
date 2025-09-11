namespace BlogApp.UnitTests.Application.FirebaseNotifications.Commands;

public class SendTopicNotificationCommandHandlerTests : BaseTestClass
{
    private readonly SendTopicNotificationCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public SendTopicNotificationCommandHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new SendTopicNotificationCommandHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new SendTopicNotificationCommand
        {
            Topic = "news",
            Notification = new FirebaseNotificationDto
            {
                Title = "Test Notification",
                Body = "This is a test notification"
            },
            Data = new Dictionary<string, string> { { "key1", "value1" } }
        };

        var expectedResponse = new FirebaseNotificationResponseDto
        {
            Success = true,
            Message = "Notification sent successfully",
            SuccessCount = 1,
            FailureCount = 0,
            FailedTokens = new List<string>()
        };

        _mockFirebaseNotificationService.Setup(x => x.SendNotificationToTopicAsync(
                command.Topic,
                command.Notification,
                command.Data))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Success.Should().BeTrue();
        result.Data.SuccessCount.Should().Be(1);

        _mockFirebaseNotificationService.Verify(x => x.SendNotificationToTopicAsync(
            command.Topic,
            command.Notification,
            command.Data), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceReturnsFailure_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SendTopicNotificationCommand
        {
            Topic = "news",
            Notification = new FirebaseNotificationDto
            {
                Title = "Test Notification",
                Body = "This is a test notification"
            }
        };

        var failedResponse = new FirebaseNotificationResponseDto
        {
            Success = false,
            Message = "Failed to send notification",
            SuccessCount = 0,
            FailureCount = 1,
            FailedTokens = new List<string>()
        };

        _mockFirebaseNotificationService.Setup(x => x.SendNotificationToTopicAsync(
                command.Topic,
                command.Notification,
                command.Data))
            .ReturnsAsync(failedResponse);

        _mockErrorMessageService.Setup(x => x.GetMessage("FailedToSendTopicNotificationMessage"))
            .Returns("Failed to send topic notification");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Failed to send topic notification");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SendTopicNotificationCommand
        {
            Topic = "news",
            Notification = new FirebaseNotificationDto
            {
                Title = "Test Notification",
                Body = "This is a test notification"
            }
        };

        _mockFirebaseNotificationService.Setup(x => x.SendNotificationToTopicAsync(
                command.Topic,
                command.Notification,
                command.Data))
            .ThrowsAsync(new Exception("Firebase service error"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}