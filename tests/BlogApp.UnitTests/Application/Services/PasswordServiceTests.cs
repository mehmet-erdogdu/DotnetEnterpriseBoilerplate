namespace BlogApp.UnitTests.Application.Services;

public class PasswordServiceTests : BaseApplicationTest
{
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly PasswordService _passwordService;

    public PasswordServiceTests()
    {
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _mockUnitOfWork.Setup(x => x.PasswordHistory).Returns(_mockPasswordHistoryRepository.Object);
        _passwordService = new PasswordService(_mockPasswordHistoryRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task TrackPasswordChangeAsync_WithValidData_ShouldAddPasswordHistory()
    {
        // Arrange
        var userId = "test-user-id";
        var passwordHash = "hashed-password";
        var passwordHistory = new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            ChangedAt = DateTime.UtcNow
        };

        _mockPasswordHistoryRepository.Setup(x => x.AddAsync(It.IsAny<PasswordHistory>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        await _passwordService.TrackPasswordChangeAsync(userId, passwordHash);

        // Assert
        _mockPasswordHistoryRepository.Verify(x => x.AddAsync(It.Is<PasswordHistory>(ph =>
            ph.UserId == userId &&
            ph.PasswordHash == passwordHash)), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task IsPasswordRecentlyUsedAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        var userId = "test-user-id";
        var passwordHash = "hashed-password";
        var expectedResult = true;

        _mockPasswordHistoryRepository.Setup(x => x.IsPasswordRecentlyUsedAsync(userId, passwordHash))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _passwordService.IsPasswordRecentlyUsedAsync(userId, passwordHash);

        // Assert
        result.Should().Be(expectedResult);
        _mockPasswordHistoryRepository.Verify(x => x.IsPasswordRecentlyUsedAsync(userId, passwordHash), Times.Once);
    }

    [Fact]
    public async Task GetPasswordHistoryCountAsync_ShouldCallRepositoryMethod()
    {
        // Arrange
        var userId = "test-user-id";
        var expectedResult = 5;

        _mockPasswordHistoryRepository.Setup(x => x.GetPasswordHistoryCountAsync(userId))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _passwordService.GetPasswordHistoryCountAsync(userId);

        // Assert
        result.Should().Be(expectedResult);
        _mockPasswordHistoryRepository.Verify(x => x.GetPasswordHistoryCountAsync(userId), Times.Once);
    }
}