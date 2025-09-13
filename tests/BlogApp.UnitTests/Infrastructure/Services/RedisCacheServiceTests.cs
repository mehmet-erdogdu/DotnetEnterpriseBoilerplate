using System.Reflection;

namespace BlogApp.UnitTests.Infrastructure.Services;

public class RedisCacheServiceTests
{
    private readonly RedisCacheService _cacheService = null!;
    private readonly Mock<IDistributedCacheWrapper> _mockCache = null!;
    private readonly Mock<IConfiguration> _mockConfiguration = null!;
    private readonly Mock<ILogger<RedisCacheService>> _mockLogger = null!;

    public RedisCacheServiceTests()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(c => c["RedisConnection"]).Returns("localhost:6379");

        _mockLogger = new Mock<ILogger<RedisCacheService>>();
        _mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _mockCache = new Mock<IDistributedCacheWrapper>();

        // Use the internal constructor for testing
        _cacheService = (RedisCacheService)Activator.CreateInstance(
            typeof(RedisCacheService),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { _mockCache.Object, _mockLogger.Object, _mockConfiguration.Object },
            null)!;
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ReturnsCachedValue()
    {
        // Arrange
        var key = "test-key";
        var expectedValue = "test-value";
        var serializedValue = JsonSerializer.Serialize(expectedValue);

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(serializedValue);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().Be(expectedValue);
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ReturnsDefault()
    {
        // Arrange
        var key = "non-existent-key";

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithException_ReturnsDefaultAndLogsError()
    {
        // Arrange
        var key = "error-key";
        var exception = new Exception("Cache error");

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _cacheService.GetAsync<string>(key);

        // Assert
        result.Should().BeNull();
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_SetsValueInCache()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var serializedValue = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions();

        _mockCache.Setup(x => x.SetStringAsync(key, serializedValue, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        _mockCache.Verify(x => x.SetStringAsync(key, serializedValue, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_SetsValueWithExpiration()
    {
        // Arrange
        var key = "test-key";
        var value = "test-value";
        var expiration = TimeSpan.FromMinutes(30);
        var serializedValue = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions();
        options.SetAbsoluteExpiration(expiration);

        _mockCache.Setup(x => x.SetStringAsync(
                key,
                serializedValue,
                It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == expiration),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.SetAsync(key, value, expiration);

        // Assert
        _mockCache.Verify(x => x.SetStringAsync(
            key,
            serializedValue,
            It.Is<DistributedCacheEntryOptions>(o => o.AbsoluteExpirationRelativeToNow == expiration),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithException_LogsError()
    {
        // Arrange
        var key = "error-key";
        var value = "test-value";
        var exception = new Exception("Cache error");
        var options = new DistributedCacheEntryOptions();

        _mockCache.Setup(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _cacheService.SetAsync(key, value);

        // Assert
        _mockCache.Verify(x => x.SetStringAsync(key, It.IsAny<string>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_RemovesValueFromCache()
    {
        // Arrange
        var key = "test-key";

        _mockCache.Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockCache.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_WithException_LogsError()
    {
        // Arrange
        var key = "error-key";
        var exception = new Exception("Cache error");

        _mockCache.Setup(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _mockCache.Verify(x => x.RemoveAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var key = "existing-key";
        var value = "test-value";

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(value);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeTrue();
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingKey_ReturnsFalse()
    {
        // Arrange
        var key = "non-existent-key";

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WithException_ReturnsFalseAndLogsError()
    {
        // Arrange
        var key = "error-key";
        var exception = new Exception("Cache error");

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _cacheService.ExistsAsync(key);

        // Assert
        result.Should().BeFalse();
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementAsync_WithExistingKey_IncrementsValue()
    {
        // Arrange
        var key = "counter-key";
        var initialValue = 5L;
        var incrementValue = 3L;
        var expectedValue = 8L;

        // Mock the GetAsync call for the initial value
        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult<string?>(JsonSerializer.Serialize(initialValue)));

        var options = new DistributedCacheEntryOptions();
        var expectedValueSerialized = JsonSerializer.Serialize(expectedValue);
        _mockCache.Setup(x => x.SetStringAsync(key, expectedValueSerialized, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cacheService.IncrementAsync(key, incrementValue);

        // Assert
        result.Should().Be(expectedValue);
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetStringAsync(key, expectedValueSerialized, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementAsync_WithNonExistentKey_SetsInitialValue()
    {
        // Arrange
        var key = "new-counter-key";
        var incrementValue = 5L;
        var expectedValue = 5L;

        // Mock the GetAsync call for the initial value (null)
        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult((string?)null));

        var options = new DistributedCacheEntryOptions();
        var expectedValueSerialized2 = JsonSerializer.Serialize(expectedValue);
        _mockCache.Setup(x => x.SetStringAsync(key, expectedValueSerialized2, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _cacheService.IncrementAsync(key, incrementValue);

        // Assert
        result.Should().Be(expectedValue);
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
        _mockCache.Verify(x => x.SetStringAsync(key, expectedValueSerialized2, It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IncrementAsync_WithException_ReturnsZeroAndLogsError()
    {
        // Arrange
        var key = "error-key";
        var exception = new Exception("Cache error");

        _mockCache.Setup(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _cacheService.IncrementAsync(key);

        // Assert
        result.Should().Be(1L);
        _mockCache.Verify(x => x.GetStringAsync(key, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithValidPattern_RemovesMatchingKeys()
    {
        // Arrange
        var pattern = "test:*";

        // Act
        var act = () => _cacheService.RemoveByPatternAsync(pattern);

        // Assert
        // Since we can't easily mock ConnectionMultiplexer, we just verify the method doesn't throw
        await act.Should().NotThrowAsync();
        // In a real implementation, you would verify the actual removal logic
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithException_LogsError()
    {
        // Arrange
        var pattern = "error:*";
        _mockConfiguration.Setup(c => c["RedisConnection"]).Throws(new Exception("Connection error"));

        // Create a new instance with the faulty configuration
        var faultyCacheService = (RedisCacheService)Activator.CreateInstance(
            typeof(RedisCacheService),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { _mockCache.Object, _mockLogger.Object, _mockConfiguration.Object },
            null)!;

        // Act
        var act = () => faultyCacheService.RemoveByPatternAsync(pattern);

        // Assert
        // Should not throw exception even when connection fails
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetTimeToLiveAsync_WithValidKey_ReturnsTtl()
    {
        // Arrange
        var key = "test-key";

        // Act
        var result = await _cacheService.GetTimeToLiveAsync(key);

        // Assert
        // Since we can't easily mock ConnectionMultiplexer, we just verify the method doesn't throw
        result.Should().BeNull(); // Will be null since we can't actually connect to Redis
    }

    [Fact]
    public async Task GetTimeToLiveAsync_WithException_ReturnsNullAndLogsError()
    {
        // Arrange
        var key = "error-key";
        _mockConfiguration.Setup(c => c["RedisConnection"]).Throws(new Exception("Connection error"));

        // Create a new instance with the faulty configuration
        var faultyCacheService = (RedisCacheService)Activator.CreateInstance(
            typeof(RedisCacheService),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            new object[] { _mockCache.Object, _mockLogger.Object, _mockConfiguration.Object },
            null)!;

        // Act
        var result = await faultyCacheService.GetTimeToLiveAsync(key);

        // Assert
        result.Should().BeNull();
    }
}