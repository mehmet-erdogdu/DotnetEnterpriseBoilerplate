namespace BlogApp.UnitTests.Application.Services;

public class RefreshTokenServiceTests : BaseApplicationTest
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly RefreshTokenService _refreshTokenService;

    public RefreshTokenServiceTests()
    {
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockUnitOfWork.Setup(x => x.RefreshTokens).Returns(_mockRefreshTokenRepository.Object);
        _refreshTokenService = new RefreshTokenService(_mockRefreshTokenRepository.Object, _mockUnitOfWork.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task GenerateRefreshTokenAsync_WithValidData_ShouldGenerateAndSaveToken()
    {
        // Arrange
        var userId = "test-user-id";
        _mockConfiguration.Setup(x => x["JWT:RefreshTokenExpirationDays"]).Returns("7");
        _mockRefreshTokenRepository.Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _refreshTokenService.GenerateRefreshTokenAsync(userId);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Length.Should().BeGreaterThan(64); // Base64 encoded 64 bytes should be longer than 64 characters

        _mockRefreshTokenRepository.Verify(x => x.AddAsync(It.Is<RefreshToken>(rt =>
            rt.UserId == userId &&
            rt.Token == result &&
            rt.ExpiresAt <= DateTime.UtcNow.AddDays(7).AddSeconds(1) &&
            rt.ExpiresAt >= DateTime.UtcNow.AddDays(7).AddSeconds(-1))), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithValidActiveToken_ShouldReturnTrue()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = "test-user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(refreshTokenEntity);

        // Act
        var result = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithRevokedToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = "test-user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = true
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(refreshTokenEntity);

        // Act
        var result = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithExpiredToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = "test-user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            IsRevoked = false
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(refreshTokenEntity);

        // Act
        var result = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Arrange
        var refreshToken = "non-existent-token";
        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _refreshTokenService.ValidateRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserIdFromRefreshTokenAsync_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        var userId = "test-user-id";
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync(refreshTokenEntity);

        // Act
        var result = await _refreshTokenService.GetUserIdFromRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserIdFromRefreshTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Arrange
        var refreshToken = "non-existent-token";
        _mockRefreshTokenRepository.Setup(x => x.GetByTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _refreshTokenService.GetUserIdFromRefreshTokenAsync(refreshToken);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        var refreshToken = "test-refresh-token";
        var revokedBy = "test-user";
        var reason = "testing";

        _mockRefreshTokenRepository.Setup(x => x.RevokeTokenAsync(refreshToken, revokedBy, reason))
            .Returns(Task.CompletedTask);

        // Act
        await _refreshTokenService.RevokeRefreshTokenAsync(refreshToken, revokedBy, reason);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.RevokeTokenAsync(refreshToken, revokedBy, reason), Times.Once);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        var userId = "test-user-id";
        var revokedBy = "test-user";
        var reason = "testing";

        _mockRefreshTokenRepository.Setup(x => x.RevokeAllUserTokensAsync(userId, revokedBy, reason))
            .Returns(Task.CompletedTask);

        // Act
        await _refreshTokenService.RevokeAllUserTokensAsync(userId, revokedBy, reason);

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.RevokeAllUserTokensAsync(userId, revokedBy, reason), Times.Once);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        _mockRefreshTokenRepository.Setup(x => x.CleanupExpiredTokensAsync())
            .Returns(Task.CompletedTask);

        // Act
        await _refreshTokenService.CleanupExpiredTokensAsync();

        // Assert
        _mockRefreshTokenRepository.Verify(x => x.CleanupExpiredTokensAsync(), Times.Once);
    }
}