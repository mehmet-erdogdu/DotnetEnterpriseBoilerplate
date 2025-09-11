namespace BlogApp.UnitTests.Controllers;

public class NotificationControllerTests : BaseControllerTest
{
    private readonly NotificationController _controller;

    public NotificationControllerTests()
    {
        _controller = new NotificationController(_mockMediator.Object);
        SetupAuthenticatedUser(_controller);
    }

    #region Private Helper Methods

    private void SetupMockMediator<TRequest, TResponse>(TResponse response) where TRequest : class
    {
        _mockMediator.Setup(x => x.Send(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
    }

    #endregion

    #region SendNotification Tests

    [Fact]
    public async Task SendNotification_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestDto = new FirebaseNotificationRequestDto
        {
            TokenIds = new List<string> { "token1", "token2" },
            Notification = new FirebaseNotificationDto
            {
                Title = "Test Notification",
                Body = "This is a test notification"
            }
        };

        var responseDto = new FirebaseNotificationResponseDto
        {
            Success = true,
            Message = "Notification sent successfully",
            SuccessCount = 2,
            FailureCount = 0
        };

        var expectedResponse = ApiResponse<FirebaseNotificationResponseDto>.Success(responseDto);
        _mockMediator.Setup(x => x.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SendNotification(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Success.Should().BeTrue();
        result.Data.SuccessCount.Should().Be(2);

        _mockMediator.Verify(x => x.Send(It.Is<SendNotificationCommand>(cmd =>
            cmd.Request.TokenIds.Count == 2 &&
            cmd.Request.Notification.Title == "Test Notification"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendNotification_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var requestDto = new FirebaseNotificationRequestDto
        {
            TokenIds = new List<string>(),
            Notification = new FirebaseNotificationDto
            {
                Title = "",
                Body = ""
            }
        };

        _controller.ModelState.AddModelError("Notification.Title", "Title is required");
        _controller.ModelState.AddModelError("TokenIds", "At least one token is required");

        // Act
        var result = await _controller.SendNotification(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<SendNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region SendTopicNotification Tests

    [Fact]
    public async Task SendTopicNotification_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestDto = new TopicNotificationRequestDto
        {
            Topic = "news",
            Notification = new FirebaseNotificationDto
            {
                Title = "News Update",
                Body = "Latest news update"
            },
            Data = new Dictionary<string, string> { { "category", "news" } }
        };

        var responseDto = new FirebaseNotificationResponseDto
        {
            Success = true,
            Message = "Topic notification sent successfully",
            SuccessCount = 10,
            FailureCount = 0
        };

        var expectedResponse = ApiResponse<FirebaseNotificationResponseDto>.Success(responseDto);
        _mockMediator.Setup(x => x.Send(It.IsAny<SendTopicNotificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SendTopicNotification(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Success.Should().BeTrue();
        result.Data.SuccessCount.Should().Be(10);

        _mockMediator.Verify(x => x.Send(It.Is<SendTopicNotificationCommand>(cmd =>
            cmd.Topic == requestDto.Topic &&
            cmd.Notification.Title == requestDto.Notification.Title &&
            cmd.Data != null && cmd.Data.ContainsKey("category")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendTopicNotification_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var requestDto = new TopicNotificationRequestDto
        {
            Topic = "",
            Notification = new FirebaseNotificationDto
            {
                Title = "",
                Body = ""
            }
        };

        _controller.ModelState.AddModelError("Topic", "Topic is required");
        _controller.ModelState.AddModelError("Notification.Title", "Title is required");

        // Act
        var result = await _controller.SendTopicNotification(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<SendTopicNotificationCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region SubscribeToTopic Tests

    [Fact]
    public async Task SubscribeToTopic_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestDto = new TopicSubscriptionRequestDto
        {
            Token = "device-token-123",
            Topic = "news"
        };

        var expectedResponse = ApiResponse<bool>.Success(true);
        _mockMediator.Setup(x => x.Send(It.IsAny<SubscribeToTopicCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.SubscribeToTopic(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockMediator.Verify(x => x.Send(It.Is<SubscribeToTopicCommand>(cmd =>
            cmd.Token == requestDto.Token &&
            cmd.Topic == requestDto.Topic
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubscribeToTopic_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var requestDto = new TopicSubscriptionRequestDto
        {
            Token = "",
            Topic = ""
        };

        _controller.ModelState.AddModelError("Token", "Token is required");
        _controller.ModelState.AddModelError("Topic", "Topic is required");

        // Act
        var result = await _controller.SubscribeToTopic(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<SubscribeToTopicCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UnsubscribeFromTopic Tests

    [Fact]
    public async Task UnsubscribeFromTopic_WithValidModel_ShouldReturnSuccessResponse()
    {
        // Arrange
        var requestDto = new TopicSubscriptionRequestDto
        {
            Token = "device-token-123",
            Topic = "news"
        };

        var expectedResponse = ApiResponse<bool>.Success(true);
        _mockMediator.Setup(x => x.Send(It.IsAny<UnsubscribeFromTopicCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UnsubscribeFromTopic(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockMediator.Verify(x => x.Send(It.Is<UnsubscribeFromTopicCommand>(cmd =>
            cmd.Token == requestDto.Token &&
            cmd.Topic == requestDto.Topic
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnsubscribeFromTopic_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var requestDto = new TopicSubscriptionRequestDto
        {
            Token = "",
            Topic = ""
        };

        _controller.ModelState.AddModelError("Token", "Token is required");
        _controller.ModelState.AddModelError("Topic", "Topic is required");

        // Act
        var result = await _controller.UnsubscribeFromTopic(requestDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.IsAny<UnsubscribeFromTopicCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}