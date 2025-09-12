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
    public async Task GetUserDeviceTokensAsync_WithCachedTokens_ReturnsTokens()
    {
        // Arrange
        var service = new FirebaseNotificationService(
            _mockLogger.Object,
            _mockCacheService.Object,
            _mockConfiguration.Object);

        var userId = "test-user-id";
        var expectedTokens = new List<string> { "token1", "token2" };
        var serializedTokens = JsonSerializer.Serialize(expectedTokens);

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
}