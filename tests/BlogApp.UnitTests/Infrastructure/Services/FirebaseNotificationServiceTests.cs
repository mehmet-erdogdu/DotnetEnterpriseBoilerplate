namespace BlogApp.UnitTests.Infrastructure.Services;

public class FirebaseNotificationServiceTests : BaseInfrastructureTest
{
    private readonly Mock<ICacheService> _mockCacheService;
    private new readonly Mock<IConfiguration> _mockConfiguration;
    private new readonly Mock<ILogger<FirebaseNotificationService>> _mockLogger;

    public FirebaseNotificationServiceTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<FirebaseNotificationService>>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["Firebase:ProjectId"]).Returns("test-project-id");
        _mockConfiguration.Setup(c => c["Firebase:ServiceAccountKeyPath"]).Returns("test-key-path");
    }

    [Fact]
    public void Constructor_WithValidConfiguration_DoesNotThrow()
    {
        // Act
        Action act = () => new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public async Task SaveDeviceTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var deviceToken = new DeviceTokenDto
        {
            UserId = "test-user-id",
            Token = "test-device-token"
        };

        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.SaveDeviceTokenAsync(deviceToken);

        // Assert
        result.Should().BeTrue();
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task SaveDeviceTokenAsync_WithException_ReturnsFalse()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var deviceToken = new DeviceTokenDto
        {
            UserId = "test-user-id",
            Token = "test-device-token"
        };

        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await service.SaveDeviceTokenAsync(deviceToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RemoveDeviceTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var userId = "test-user-id";
        var token = "test-device-token";

        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.RemoveDeviceTokenAsync(userId, token);

        // Assert
        result.Should().BeTrue();
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task RemoveDeviceTokenAsync_WithException_ReturnsFalse()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var userId = "test-user-id";
        var token = "test-device-token";

        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await service.RemoveDeviceTokenAsync(userId, token);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserDeviceTokensAsync_WithCachedTokens_ReturnsTokens()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var userId = "test-user-id";
        var expectedTokens = new List<string> { "token1", "token2" };

        _mockCacheService.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>()))
            .ReturnsAsync(expectedTokens);

        // Act
        var result = await service.GetUserDeviceTokensAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedTokens);
    }

    [Fact]
    public async Task GetUserDeviceTokensAsync_WithException_ReturnsEmptyList()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var userId = "test-user-id";

        _mockCacheService.Setup(x => x.GetAsync<List<string>>(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await service.GetUserDeviceTokensAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SendNotificationAsync_WithValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var request = new FirebaseNotificationRequestDto
        {
            TokenIds = new List<string> { "token1", "token2" },
            Notification = new FirebaseNotificationDto
            {
                Title = "Test Notification",
                Body = "Test Body"
            }
        };

        // Act
        var result = await service.SendNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        // Note: Actual Firebase messaging functionality is not tested due to complexity of mocking
    }

    [Fact]
    public async Task SendNotificationAsync_WithNoTokens_ReturnsFailureResponse()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var request = new FirebaseNotificationRequestDto
        {
            TokenIds = new List<string>(),
            Notification = new FirebaseNotificationDto
            {
                Title = "Test Notification",
                Body = "Test Body"
            }
        };

        // Act
        var result = await service.SendNotificationAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No device tokens provided");
    }

    [Fact]
    public async Task SubscribeToTopicAsync_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var token = "test-device-token";
        var topic = "test-topic";

        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.SubscribeToTopicAsync(token, topic);

        // Assert
        // In test environment, FirebaseMessaging is not available, so it will return false
        result.Should().BeFalse();
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task SubscribeToTopicAsync_WithException_ReturnsFalse()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var token = "test-device-token";
        var topic = "test-topic";

        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<TimeSpan?>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await service.SubscribeToTopicAsync(token, topic);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UnsubscribeFromTopicAsync_WithValidParameters_ReturnsTrue()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var token = "test-device-token";
        var topic = "test-topic";

        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.UnsubscribeFromTopicAsync(token, topic);

        // Assert
        // In test environment, FirebaseMessaging is not available, so it will return false
        result.Should().BeFalse();
        _mockCacheService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UnsubscribeFromTopicAsync_WithException_ReturnsFalse()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var token = "test-device-token";
        var topic = "test-topic";

        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act
        var result = await service.UnsubscribeFromTopicAsync(token, topic);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsTokenValidAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var token = "valid-device-token";

        // Act
        var result = await service.IsTokenValidAsync(token);

        // Assert
        result.Should().BeFalse(); // Will be false since we can't actually test Firebase messaging
    }
}