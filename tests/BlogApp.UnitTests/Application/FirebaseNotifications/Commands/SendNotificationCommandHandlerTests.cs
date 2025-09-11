namespace BlogApp.UnitTests.Application.FirebaseNotifications.Commands;

public class SendNotificationCommandHandlerTests : BaseTestClass
{
    private readonly SendNotificationCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public SendNotificationCommandHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new SendNotificationCommandHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithSuccessfulNotification_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new SendNotificationCommand
        {
            Request = new FirebaseNotificationRequestDto
            {
                TokenIds = new List<string> { "token1", "token2" },
                Notification = new FirebaseNotificationDto
                {
                    Title = "Test Notification",
                    Body = "This is a test notification"
                }
            }
        };

        var expectedResponse = new FirebaseNotificationResponseDto
        {
            Success = true,
            Message = "Notifications sent successfully",
            SuccessCount = 2,
            FailureCount = 0,
            FailedTokens = new List<string>()
        };

        _mockFirebaseNotificationService.Setup(x => x.SendNotificationAsync(command.Request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Success.Should().BeTrue();
        result.Data.SuccessCount.Should().Be(2);
        result.Data.FailureCount.Should().Be(0);

        _mockFirebaseNotificationService.Verify(x => x.SendNotificationAsync(command.Request), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPartialFailure_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SendNotificationCommand
        {
            Request = new FirebaseNotificationRequestDto
            {
                TokenIds = new List<string> { "valid-token", "invalid-token" },
                Notification = new FirebaseNotificationDto
                {
                    Title = "Test Notification",
                    Body = "This is a test notification"
                }
            }
        };

        var responseWithFailure = new FirebaseNotificationResponseDto
        {
            Success = false,
            Message = "Some notifications failed",
            SuccessCount = 1,
            FailureCount = 1,
            FailedTokens = new List<string> { "invalid-token" }
        };

        _mockFirebaseNotificationService.Setup(x => x.SendNotificationAsync(command.Request))
            .ReturnsAsync(responseWithFailure);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Some notifications failed");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new SendNotificationCommand
        {
            Request = new FirebaseNotificationRequestDto
            {
                TokenIds = new List<string> { "token1" },
                Notification = new FirebaseNotificationDto
                {
                    Title = "Test Notification",
                    Body = "This is a test notification"
                }
            }
        };

        _mockFirebaseNotificationService.Setup(x => x.SendNotificationAsync(command.Request))
            .ThrowsAsync(new Exception("Firebase service error"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}