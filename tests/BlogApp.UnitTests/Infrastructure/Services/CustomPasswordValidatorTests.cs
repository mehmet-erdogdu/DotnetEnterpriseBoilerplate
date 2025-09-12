namespace BlogApp.UnitTests.Infrastructure.Services;

// Test helper class
public class TestUser : IdentityUser
{
    public new string Id { get; set; } = string.Empty;
}

public class CustomPasswordValidatorTests : BaseInfrastructureTest
{
    private readonly Mock<IPasswordHistoryRepository> _mockPasswordHistoryRepository;
    private readonly CustomPasswordValidator<TestUser> _passwordValidator;

    public CustomPasswordValidatorTests()
    {
        _mockPasswordHistoryRepository = new Mock<IPasswordHistoryRepository>();
        _passwordValidator = new CustomPasswordValidator<TestUser>(_mockPasswordHistoryRepository.Object);
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordNotRecentlyUsed_ReturnsSuccess()
    {
        // Arrange
        var user = new TestUser { Id = "test-user-id" };
        var password = "NewPassword123!";

        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync(user.Id);

        var mockPasswordHasher = new Mock<IPasswordHasher<TestUser>>();
        mockPasswordHasher.Setup(x => x.HashPassword(user, password))
            .Returns("hashed-password");
        mockUserManager.Object.PasswordHasher = mockPasswordHasher.Object;

        _mockPasswordHistoryRepository.Setup(x => x.IsPasswordRecentlyUsedAsync(user.Id, "hashed-password"))
            .ReturnsAsync(false);

        // Act
        var result = await _passwordValidator.ValidateAsync(mockUserManager.Object, user, password);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WhenPasswordRecentlyUsed_ReturnsFailure()
    {
        // Arrange
        var user = new TestUser { Id = "test-user-id" };
        var password = "OldPassword123!";

        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync(user.Id);

        var mockPasswordHasher = new Mock<IPasswordHasher<TestUser>>();
        mockPasswordHasher.Setup(x => x.HashPassword(user, password))
            .Returns("hashed-password");
        mockUserManager.Object.PasswordHasher = mockPasswordHasher.Object;

        _mockPasswordHistoryRepository.Setup(x => x.IsPasswordRecentlyUsedAsync(user.Id, "hashed-password"))
            .ReturnsAsync(true);

        // Act
        var result = await _passwordValidator.ValidateAsync(mockUserManager.Object, user, password);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Code.Should().Be("PasswordRecentlyUsed");
        result.Errors.First().Description.Should().Contain("recently used");
    }

    [Fact]
    public async Task ValidateAsync_WhenRepositoryThrowsException_ThrowsException()
    {
        // Arrange
        var user = new TestUser { Id = "test-user-id" };
        var password = "NewPassword123!";

        var mockUserManager = CreateMockUserManager();
        mockUserManager.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync(user.Id);

        var mockPasswordHasher = new Mock<IPasswordHasher<TestUser>>();
        mockPasswordHasher.Setup(x => x.HashPassword(user, password))
            .Returns("hashed-password");
        mockUserManager.Object.PasswordHasher = mockPasswordHasher.Object;

        _mockPasswordHistoryRepository.Setup(x => x.IsPasswordRecentlyUsedAsync(user.Id, "hashed-password"))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        Func<Task> act = async () => await _passwordValidator.ValidateAsync(mockUserManager.Object, user, password);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }

    private Mock<UserManager<TestUser>> CreateMockUserManager()
    {
        var userStore = new Mock<IUserStore<TestUser>>();
        var mockUserManager = new Mock<UserManager<TestUser>>(
            userStore.Object,
            null!, null!, null!, null!, null!, null!, null!, null!);
        return mockUserManager;
    }
}