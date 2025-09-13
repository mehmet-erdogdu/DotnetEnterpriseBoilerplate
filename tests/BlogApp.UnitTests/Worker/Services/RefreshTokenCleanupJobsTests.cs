using BlogApp.Worker.Services;

namespace BlogApp.UnitTests.Worker.Services;

public class RefreshTokenCleanupJobsTests
{
    private readonly Mock<ILogger<RefreshTokenCleanupJobs>> _mockLogger;
    private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
    private readonly RefreshTokenCleanupJobs _refreshTokenCleanupJobs;

    public RefreshTokenCleanupJobsTests()
    {
        _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        _mockLogger = new Mock<ILogger<RefreshTokenCleanupJobs>>();

        _refreshTokenCleanupJobs = new RefreshTokenCleanupJobs(
            _mockRefreshTokenService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Run_ShouldCallCleanupExpiredTokensAsync()
    {
        // Arrange
        _mockRefreshTokenService.Setup(x => x.CleanupExpiredTokensAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _refreshTokenCleanupJobs.Run();

        // Assert
        _mockRefreshTokenService.Verify(x => x.CleanupExpiredTokensAsync(), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("TickerQ cleanup executed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenCleanupThrowsException_ShouldLogError()
    {
        // Arrange
        var exception = new Exception("Cleanup failed");
        _mockRefreshTokenService.Setup(x => x.CleanupExpiredTokensAsync())
            .ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _refreshTokenCleanupJobs.Run());

        _mockRefreshTokenService.Verify(x => x.CleanupExpiredTokensAsync(), Times.Once);
    }
}