namespace BlogApp.UnitTests.Infrastructure.Services;

public class CacheInvalidationServiceTests : BaseInfrastructureTest
{
    private readonly CacheInvalidationService _cacheInvalidationService;
    private readonly Mock<ICacheService> _mockCacheService;
    private new readonly Mock<ILogger<CacheInvalidationService>> _mockLogger;

    public CacheInvalidationServiceTests()
    {
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<CacheInvalidationService>>();
        _cacheInvalidationService = new CacheInvalidationService(_mockCacheService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task InvalidatePostCacheAsync_WithPostId_InvalidatesSpecificPostCache()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postKey = $"posts:{postId}";

        _mockCacheService.Setup(x => x.RemoveAsync(postKey)).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidatePostCacheAsync(postId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveAsync(postKey), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated cache for specific post")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidatePostCacheAsync_WithoutPostId_InvalidatesAllPostCache()
    {
        // Arrange
        _mockCacheService.Setup(x => x.RemoveByPatternAsync("posts:*")).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidatePostCacheAsync();

        // Assert
        _mockCacheService.Verify(x => x.RemoveByPatternAsync("posts:*"), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated all post cache entries")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidatePostCacheAsync_WithException_LogsError()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var exception = new Exception("Cache error");
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>())).ThrowsAsync(exception);

        // Act
        await _cacheInvalidationService.InvalidatePostCacheAsync(postId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error invalidating post cache")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateTodoCacheAsync_WithTodoId_InvalidatesSpecificTodoCache()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var todoKey = $"todos:{todoId}";

        _mockCacheService.Setup(x => x.RemoveAsync(todoKey)).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidateTodoCacheAsync(todoId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveAsync(todoKey), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated cache for specific todo")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateTodoCacheAsync_WithoutTodoId_InvalidatesAllTodoCache()
    {
        // Arrange
        _mockCacheService.Setup(x => x.RemoveByPatternAsync("todos:*")).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidateTodoCacheAsync();

        // Assert
        _mockCacheService.Verify(x => x.RemoveByPatternAsync("todos:*"), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated all todo cache entries")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateTodoCacheAsync_WithException_LogsError()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var exception = new Exception("Cache error");
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>())).ThrowsAsync(exception);

        // Act
        await _cacheInvalidationService.InvalidateTodoCacheAsync(todoId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error invalidating todo cache")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_WithUserId_InvalidatesSpecificUserCache()
    {
        // Arrange
        var userId = "test-user-id";
        var userKey = $"user:{userId}";
        var userPostsKey = $"user:{userId}:posts";

        _mockCacheService.Setup(x => x.RemoveAsync(userKey)).Returns(Task.CompletedTask);
        _mockCacheService.Setup(x => x.RemoveAsync(userPostsKey)).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidateUserCacheAsync(userId);

        // Assert
        _mockCacheService.Verify(x => x.RemoveAsync(userKey), Times.Once);
        _mockCacheService.Verify(x => x.RemoveAsync(userPostsKey), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated cache for specific user")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_WithoutUserId_InvalidatesAllUserCache()
    {
        // Arrange
        _mockCacheService.Setup(x => x.RemoveByPatternAsync("user:*")).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidateUserCacheAsync();

        // Assert
        _mockCacheService.Verify(x => x.RemoveByPatternAsync("user:*"), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated all user cache entries")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateUserCacheAsync_WithException_LogsError()
    {
        // Arrange
        var userId = "test-user-id";
        var exception = new Exception("Cache error");
        _mockCacheService.Setup(x => x.RemoveAsync(It.IsAny<string>())).ThrowsAsync(exception);

        // Act
        await _cacheInvalidationService.InvalidateUserCacheAsync(userId);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error invalidating user cache")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateAllCacheAsync_InvalidatesAllCacheEntries()
    {
        // Arrange
        _mockCacheService.Setup(x => x.RemoveByPatternAsync("*")).Returns(Task.CompletedTask);

        // Act
        await _cacheInvalidationService.InvalidateAllCacheAsync();

        // Assert
        _mockCacheService.Verify(x => x.RemoveByPatternAsync("*"), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalidated all cache entries")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateAllCacheAsync_WithException_LogsError()
    {
        // Arrange
        var exception = new Exception("Cache error");
        _mockCacheService.Setup(x => x.RemoveByPatternAsync("*")).ThrowsAsync(exception);

        // Act
        await _cacheInvalidationService.InvalidateAllCacheAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error invalidating all cache entries")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}