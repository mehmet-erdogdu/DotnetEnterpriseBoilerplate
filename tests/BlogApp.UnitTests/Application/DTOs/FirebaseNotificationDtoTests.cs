namespace BlogApp.UnitTests.Application.DTOs;

public class FirebaseNotificationDtoTests
{
    [Fact]
    public void FirebaseNotificationDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new FirebaseNotificationDto();

        // Assert
        dto.Title.Should().Be(string.Empty);
        dto.Body.Should().Be(string.Empty);
        dto.ImageUrl.Should().BeNull();
        dto.Data.Should().BeNull();
        dto.ClickAction.Should().BeNull();
        dto.Sound.Should().Be("default");
        dto.Priority.Should().BeTrue();
        dto.TimeToLive.Should().BeNull();
    }

    [Fact]
    public void FirebaseNotificationDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var data = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var dto = new FirebaseNotificationDto
        {
            Title = "Test Title",
            Body = "Test Body",
            ImageUrl = "https://example.com/image.jpg",
            Data = data,
            ClickAction = "OPEN_APP",
            Sound = "custom_sound",
            Priority = false,
            TimeToLive = 3600
        };

        // Assert
        dto.Title.Should().Be("Test Title");
        dto.Body.Should().Be("Test Body");
        dto.ImageUrl.Should().Be("https://example.com/image.jpg");
        dto.Data.Should().BeEquivalentTo(data);
        dto.ClickAction.Should().Be("OPEN_APP");
        dto.Sound.Should().Be("custom_sound");
        dto.Priority.Should().BeFalse();
        dto.TimeToLive.Should().Be(3600);
    }

    [Fact]
    public void FirebaseNotificationRequestDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new FirebaseNotificationRequestDto();

        // Assert
        dto.TokenIds.Should().NotBeNull();
        dto.TokenIds.Should().BeEmpty();
        dto.Notification.Should().NotBeNull();
        dto.Notification.Should().BeOfType<FirebaseNotificationDto>();
        dto.Topic.Should().BeNull();
        dto.Data.Should().BeNull();
    }

    [Fact]
    public void FirebaseNotificationRequestDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var tokenIds = new List<string> { "token1", "token2", "token3" };
        var notification = new FirebaseNotificationDto
        {
            Title = "Test Title",
            Body = "Test Body"
        };
        var data = new Dictionary<string, string>
        {
            { "custom_key", "custom_value" }
        };

        // Act
        var dto = new FirebaseNotificationRequestDto
        {
            TokenIds = tokenIds,
            Notification = notification,
            Topic = "news",
            Data = data
        };

        // Assert
        dto.TokenIds.Should().BeEquivalentTo(tokenIds);
        dto.Notification.Should().Be(notification);
        dto.Topic.Should().Be("news");
        dto.Data.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void FirebaseNotificationResponseDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new FirebaseNotificationResponseDto();

        // Assert
        dto.Success.Should().BeFalse();
        dto.Message.Should().Be(string.Empty);
        dto.SuccessCount.Should().Be(0);
        dto.FailureCount.Should().Be(0);
        dto.FailedTokens.Should().NotBeNull();
        dto.FailedTokens.Should().BeEmpty();
    }

    [Fact]
    public void FirebaseNotificationResponseDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var failedTokens = new List<string> { "failed_token1", "failed_token2" };

        // Act
        var dto = new FirebaseNotificationResponseDto
        {
            Success = true,
            Message = "Notifications sent successfully",
            SuccessCount = 5,
            FailureCount = 2,
            FailedTokens = failedTokens
        };

        // Assert
        dto.Success.Should().BeTrue();
        dto.Message.Should().Be("Notifications sent successfully");
        dto.SuccessCount.Should().Be(5);
        dto.FailureCount.Should().Be(2);
        dto.FailedTokens.Should().BeEquivalentTo(failedTokens);
    }

    [Fact]
    public void DeviceTokenDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new DeviceTokenDto();

        // Assert
        dto.UserId.Should().Be(string.Empty);
        dto.Token.Should().Be(string.Empty);
        dto.Platform.Should().Be(string.Empty);
        dto.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        dto.LastUsedAt.Should().BeNull();
    }

    [Fact]
    public void DeviceTokenDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var createdAt = DateTime.UtcNow.AddHours(-1);
        var lastUsedAt = DateTime.UtcNow;

        // Act
        var dto = new DeviceTokenDto
        {
            UserId = "user123",
            Token = "device_token_abc123",
            Platform = "android",
            CreatedAt = createdAt,
            LastUsedAt = lastUsedAt
        };

        // Assert
        dto.UserId.Should().Be("user123");
        dto.Token.Should().Be("device_token_abc123");
        dto.Platform.Should().Be("android");
        dto.CreatedAt.Should().Be(createdAt);
        dto.LastUsedAt.Should().Be(lastUsedAt);
    }

    [Fact]
    public void TopicSubscriptionDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new TopicSubscriptionDto();

        // Assert
        dto.UserId.Should().Be(string.Empty);
        dto.Topic.Should().Be(string.Empty);
        dto.SubscribedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void TopicSubscriptionDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var subscribedAt = DateTime.UtcNow.AddDays(-7);

        // Act
        var dto = new TopicSubscriptionDto
        {
            UserId = "user456",
            Topic = "breaking_news",
            SubscribedAt = subscribedAt
        };

        // Assert
        dto.UserId.Should().Be("user456");
        dto.Topic.Should().Be("breaking_news");
        dto.SubscribedAt.Should().Be(subscribedAt);
    }

    [Fact]
    public void TopicNotificationRequestDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new TopicNotificationRequestDto();

        // Assert
        dto.Topic.Should().Be(string.Empty);
        dto.Notification.Should().NotBeNull();
        dto.Notification.Should().BeOfType<FirebaseNotificationDto>();
        dto.Data.Should().BeNull();
    }

    [Fact]
    public void TopicNotificationRequestDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var notification = new FirebaseNotificationDto
        {
            Title = "Topic Title",
            Body = "Topic Body"
        };
        var data = new Dictionary<string, string>
        {
            { "topic_key", "topic_value" }
        };

        // Act
        var dto = new TopicNotificationRequestDto
        {
            Topic = "sports",
            Notification = notification,
            Data = data
        };

        // Assert
        dto.Topic.Should().Be("sports");
        dto.Notification.Should().Be(notification);
        dto.Data.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void TopicSubscriptionRequestDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new TopicSubscriptionRequestDto();

        // Assert
        dto.Token.Should().Be(string.Empty);
        dto.Topic.Should().Be(string.Empty);
    }

    [Fact]
    public void TopicSubscriptionRequestDto_ShouldAllowSettingAllProperties()
    {
        // Act
        var dto = new TopicSubscriptionRequestDto
        {
            Token = "subscription_token_xyz789",
            Topic = "technology"
        };

        // Assert
        dto.Token.Should().Be("subscription_token_xyz789");
        dto.Topic.Should().Be("technology");
    }

    [Fact]
    public void SaveDeviceTokenRequestDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new SaveDeviceTokenRequestDto();

        // Assert
        dto.Token.Should().Be(string.Empty);
        dto.Platform.Should().Be(string.Empty);
    }

    [Fact]
    public void SaveDeviceTokenRequestDto_ShouldAllowSettingAllProperties()
    {
        // Act
        var dto = new SaveDeviceTokenRequestDto
        {
            Token = "new_device_token_123",
            Platform = "ios"
        };

        // Assert
        dto.Token.Should().Be("new_device_token_123");
        dto.Platform.Should().Be("ios");
    }

    [Fact]
    public void ValidateTokenRequestDto_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new ValidateTokenRequestDto();

        // Assert
        dto.Token.Should().Be(string.Empty);
    }

    [Fact]
    public void ValidateTokenRequestDto_ShouldAllowSettingToken()
    {
        // Act
        var dto = new ValidateTokenRequestDto
        {
            Token = "token_to_validate_456"
        };

        // Assert
        dto.Token.Should().Be("token_to_validate_456");
    }

    [Theory]
    [InlineData("android")]
    [InlineData("ios")]
    [InlineData("web")]
    public void SaveDeviceTokenRequestDto_ShouldAcceptValidPlatforms(string platform)
    {
        // Act
        var dto = new SaveDeviceTokenRequestDto
        {
            Token = "test_token",
            Platform = platform
        };

        // Assert
        dto.Platform.Should().Be(platform);
    }

    [Fact]
    public void FirebaseNotificationDto_WithNullData_ShouldHandleGracefully()
    {
        // Act
        var dto = new FirebaseNotificationDto
        {
            Title = "Test",
            Body = "Test Body",
            Data = null
        };

        // Assert
        dto.Data.Should().BeNull();
        dto.Title.Should().Be("Test");
        dto.Body.Should().Be("Test Body");
    }

    [Fact]
    public void FirebaseNotificationDto_WithEmptyData_ShouldHandleGracefully()
    {
        // Act
        var dto = new FirebaseNotificationDto
        {
            Title = "Test",
            Body = "Test Body",
            Data = new Dictionary<string, string>()
        };

        // Assert
        dto.Data.Should().NotBeNull();
        dto.Data.Should().BeEmpty();
    }
}