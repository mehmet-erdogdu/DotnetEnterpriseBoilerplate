namespace BlogApp.UnitTests.Application.DTOs;

public class FirebaseNotificationDtoTests
{
    [Fact]
    public void FirebaseNotificationDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var title = "Test Notification";
        var body = "This is a test notification";
        var imageUrl = "https://example.com/image.jpg";
        var data = new Dictionary<string, string> { { "key", "value" } };
        var clickAction = "OPEN_ACTIVITY";
        var sound = "default";
        var priority = true;
        var timeToLive = 3600;

        // Act
        var notificationDto = new FirebaseNotificationDto
        {
            Title = title,
            Body = body,
            ImageUrl = imageUrl,
            Data = data,
            ClickAction = clickAction,
            Sound = sound,
            Priority = priority,
            TimeToLive = timeToLive
        };

        // Assert
        Assert.Equal(title, notificationDto.Title);
        Assert.Equal(body, notificationDto.Body);
        Assert.Equal(imageUrl, notificationDto.ImageUrl);
        Assert.Same(data, notificationDto.Data);
        Assert.Equal(clickAction, notificationDto.ClickAction);
        Assert.Equal(sound, notificationDto.Sound);
        Assert.Equal(priority, notificationDto.Priority);
        Assert.Equal(timeToLive, notificationDto.TimeToLive);
    }

    [Fact]
    public void FirebaseNotificationDto_Should_Have_Default_Values()
    {
        // Act
        var notificationDto = new FirebaseNotificationDto();

        // Assert
        Assert.Equal(string.Empty, notificationDto.Title);
        Assert.Equal(string.Empty, notificationDto.Body);
        Assert.Null(notificationDto.ImageUrl);
        Assert.Null(notificationDto.Data);
        Assert.Null(notificationDto.ClickAction);
        Assert.Equal("default", notificationDto.Sound);
        Assert.True(notificationDto.Priority);
        Assert.Null(notificationDto.TimeToLive);
    }
}

public class FirebaseNotificationRequestDtoTests
{
    [Fact]
    public void FirebaseNotificationRequestDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var tokenIds = new List<string> { "token1", "token2" };
        var notification = new FirebaseNotificationDto { Title = "Test", Body = "Test body" };
        var topic = "test-topic";
        var data = new Dictionary<string, string> { { "key", "value" } };

        // Act
        var requestDto = new FirebaseNotificationRequestDto
        {
            TokenIds = tokenIds,
            Notification = notification,
            Topic = topic,
            Data = data
        };

        // Assert
        Assert.Same(tokenIds, requestDto.TokenIds);
        Assert.Same(notification, requestDto.Notification);
        Assert.Equal(topic, requestDto.Topic);
        Assert.Same(data, requestDto.Data);
    }

    [Fact]
    public void FirebaseNotificationRequestDto_Should_Have_Default_Values()
    {
        // Act
        var requestDto = new FirebaseNotificationRequestDto();

        // Assert
        Assert.NotNull(requestDto.TokenIds);
        Assert.Empty(requestDto.TokenIds);
        Assert.NotNull(requestDto.Notification);
        Assert.Null(requestDto.Topic);
        Assert.Null(requestDto.Data);
    }
}

public class FirebaseNotificationResponseDtoTests
{
    [Fact]
    public void FirebaseNotificationResponseDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var success = true;
        var message = "Notification sent successfully";
        var successCount = 1;
        var failureCount = 0;
        var failedTokens = new List<string> { "failed-token" };

        // Act
        var responseDto = new FirebaseNotificationResponseDto
        {
            Success = success,
            Message = message,
            SuccessCount = successCount,
            FailureCount = failureCount,
            FailedTokens = failedTokens
        };

        // Assert
        Assert.Equal(success, responseDto.Success);
        Assert.Equal(message, responseDto.Message);
        Assert.Equal(successCount, responseDto.SuccessCount);
        Assert.Equal(failureCount, responseDto.FailureCount);
        Assert.Same(failedTokens, responseDto.FailedTokens);
    }

    [Fact]
    public void FirebaseNotificationResponseDto_Should_Have_Default_Values()
    {
        // Act
        var responseDto = new FirebaseNotificationResponseDto();

        // Assert
        Assert.False(responseDto.Success);
        Assert.Equal(string.Empty, responseDto.Message);
        Assert.Equal(0, responseDto.SuccessCount);
        Assert.Equal(0, responseDto.FailureCount);
        Assert.NotNull(responseDto.FailedTokens);
        Assert.Empty(responseDto.FailedTokens);
    }
}

public class DeviceTokenDtoTests
{
    [Fact]
    public void DeviceTokenDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var userId = "test-user-id";
        var token = "test-token";
        var platform = "android";
        var createdAt = DateTime.UtcNow;
        var lastUsedAt = DateTime.UtcNow.AddHours(1);

        // Act
        var deviceTokenDto = new DeviceTokenDto
        {
            UserId = userId,
            Token = token,
            Platform = platform,
            CreatedAt = createdAt,
            LastUsedAt = lastUsedAt
        };

        // Assert
        Assert.Equal(userId, deviceTokenDto.UserId);
        Assert.Equal(token, deviceTokenDto.Token);
        Assert.Equal(platform, deviceTokenDto.Platform);
        Assert.Equal(createdAt, deviceTokenDto.CreatedAt);
        Assert.Equal(lastUsedAt, deviceTokenDto.LastUsedAt);
    }

    [Fact]
    public void DeviceTokenDto_Should_Have_Default_Values()
    {
        // Act
        var deviceTokenDto = new DeviceTokenDto();

        // Assert
        Assert.Equal(string.Empty, deviceTokenDto.UserId);
        Assert.Equal(string.Empty, deviceTokenDto.Token);
        Assert.Equal(string.Empty, deviceTokenDto.Platform);
        Assert.True(deviceTokenDto.CreatedAt <= DateTime.UtcNow);
        Assert.Null(deviceTokenDto.LastUsedAt);
    }
}

public class TopicSubscriptionDtoTests
{
    [Fact]
    public void TopicSubscriptionDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var userId = "test-user-id";
        var topic = "test-topic";
        var subscribedAt = DateTime.UtcNow;

        // Act
        var topicSubscriptionDto = new TopicSubscriptionDto
        {
            UserId = userId,
            Topic = topic,
            SubscribedAt = subscribedAt
        };

        // Assert
        Assert.Equal(userId, topicSubscriptionDto.UserId);
        Assert.Equal(topic, topicSubscriptionDto.Topic);
        Assert.Equal(subscribedAt, topicSubscriptionDto.SubscribedAt);
    }

    [Fact]
    public void TopicSubscriptionDto_Should_Have_Default_Values()
    {
        // Act
        var topicSubscriptionDto = new TopicSubscriptionDto();

        // Assert
        Assert.Equal(string.Empty, topicSubscriptionDto.UserId);
        Assert.Equal(string.Empty, topicSubscriptionDto.Topic);
        Assert.True(topicSubscriptionDto.SubscribedAt <= DateTime.UtcNow);
    }
}

public class TopicNotificationRequestDtoTests
{
    [Fact]
    public void TopicNotificationRequestDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var topic = "test-topic";
        var notification = new FirebaseNotificationDto { Title = "Test", Body = "Test body" };
        var data = new Dictionary<string, string> { { "key", "value" } };

        // Act
        var requestDto = new TopicNotificationRequestDto
        {
            Topic = topic,
            Notification = notification,
            Data = data
        };

        // Assert
        Assert.Equal(topic, requestDto.Topic);
        Assert.Same(notification, requestDto.Notification);
        Assert.Same(data, requestDto.Data);
    }

    [Fact]
    public void TopicNotificationRequestDto_Should_Have_Default_Values()
    {
        // Act
        var requestDto = new TopicNotificationRequestDto();

        // Assert
        Assert.Equal(string.Empty, requestDto.Topic);
        Assert.NotNull(requestDto.Notification);
        Assert.Null(requestDto.Data);
    }
}

public class TopicSubscriptionRequestDtoTests
{
    [Fact]
    public void TopicSubscriptionRequestDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var token = "test-token";
        var topic = "test-topic";

        // Act
        var requestDto = new TopicSubscriptionRequestDto
        {
            Token = token,
            Topic = topic
        };

        // Assert
        Assert.Equal(token, requestDto.Token);
        Assert.Equal(topic, requestDto.Topic);
    }

    [Fact]
    public void TopicSubscriptionRequestDto_Should_Have_Default_Values()
    {
        // Act
        var requestDto = new TopicSubscriptionRequestDto();

        // Assert
        Assert.Equal(string.Empty, requestDto.Token);
        Assert.Equal(string.Empty, requestDto.Topic);
    }
}

public class SaveDeviceTokenRequestDtoTests
{
    [Fact]
    public void SaveDeviceTokenRequestDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var token = "test-token";
        var platform = "android";

        // Act
        var requestDto = new SaveDeviceTokenRequestDto
        {
            Token = token,
            Platform = platform
        };

        // Assert
        Assert.Equal(token, requestDto.Token);
        Assert.Equal(platform, requestDto.Platform);
    }

    [Fact]
    public void SaveDeviceTokenRequestDto_Should_Have_Default_Values()
    {
        // Act
        var requestDto = new SaveDeviceTokenRequestDto();

        // Assert
        Assert.Equal(string.Empty, requestDto.Token);
        Assert.Equal(string.Empty, requestDto.Platform);
    }
}

public class ValidateTokenRequestDtoTests
{
    [Fact]
    public void ValidateTokenRequestDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var token = "test-token";

        // Act
        var requestDto = new ValidateTokenRequestDto
        {
            Token = token
        };

        // Assert
        Assert.Equal(token, requestDto.Token);
    }

    [Fact]
    public void ValidateTokenRequestDto_Should_Have_Default_Values()
    {
        // Act
        var requestDto = new ValidateTokenRequestDto();

        // Assert
        Assert.Equal(string.Empty, requestDto.Token);
    }
}