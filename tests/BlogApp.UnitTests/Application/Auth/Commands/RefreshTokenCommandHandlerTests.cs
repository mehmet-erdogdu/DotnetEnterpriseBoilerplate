namespace BlogApp.UnitTests.Application.Auth.Commands;

public class RefreshTokenCommandHandlerTests : BaseTestClass
{
    private readonly RefreshTokenCommandHandler _handler;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public RefreshTokenCommandHandlerTests()
    {
        _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();
        _mockMessageService = new Mock<IMessageService>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup JWT configuration
        _mockConfiguration.Setup(x => x["JWT:Secret"]).Returns("this-is-a-very-long-secret-key-for-jwt-token-generation-that-is-at-least-32-characters");
        _mockConfiguration.Setup(x => x["JWT:ValidIssuer"]).Returns("test-issuer");
        _mockConfiguration.Setup(x => x["JWT:ValidAudience"]).Returns("test-audience");
        _mockConfiguration.Setup(x => x["JWT:TokenExpirationMinutes"]).Returns("30");

        _handler = new RefreshTokenCommandHandler(
            _mockRefreshTokenService.Object,
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMessageService.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        var userId = "test-user-id";
        var user = TestHelper.TestData.CreateTestUser(userId);
        var newRefreshToken = "new-refresh-token";
        var userRoles = new List<string> { "User" };
        var role = new IdentityRole("User");

        _mockRefreshTokenService.Setup(x => x.ValidateRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(true);

        _mockRefreshTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(userId);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(userRoles);

        _mockRoleManager.Setup(x => x.FindByNameAsync("User"))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim>());

        _mockRefreshTokenService.Setup(x => x.RevokeRefreshTokenAsync(command.RefreshToken, userId, "Refreshed"))
            .Returns(Task.CompletedTask);

        _mockRefreshTokenService.Setup(x => x.GenerateRefreshTokenAsync(userId))
            .ReturnsAsync(newRefreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().Be(newRefreshToken);
        result.Data.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));

        _mockRefreshTokenService.Verify(x => x.ValidateRefreshTokenAsync(command.RefreshToken), Times.Once);
        _mockRefreshTokenService.Verify(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken), Times.Once);
        _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        _mockRoleManager.Verify(x => x.FindByNameAsync("User"), Times.Once);
        _mockRefreshTokenService.Verify(x => x.RevokeRefreshTokenAsync(command.RefreshToken, userId, "Refreshed"), Times.Once);
        _mockRefreshTokenService.Verify(x => x.GenerateRefreshTokenAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "invalid-refresh-token"
        };

        _mockRefreshTokenService.Setup(x => x.ValidateRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(false);

        _mockMessageService.Setup(x => x.GetMessage("InvalidOrExpiredRefreshToken"))
            .Returns("Invalid or expired refresh token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid or expired refresh token");

        _mockRefreshTokenService.Verify(x => x.GetUserIdFromRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidTokenButInvalidUserId_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        _mockRefreshTokenService.Setup(x => x.ValidateRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(true);

        _mockRefreshTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((string?)null);

        _mockMessageService.Setup(x => x.GetMessage("InvalidRefreshToken"))
            .Returns("Invalid refresh token");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid refresh token");

        _mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _mockRefreshTokenService.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidTokenButNonExistentUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        var userId = "non-existent-user-id";

        _mockRefreshTokenService.Setup(x => x.ValidateRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(true);

        _mockRefreshTokenService.Setup(x => x.GetUserIdFromRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(userId);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        _mockMessageService.Setup(x => x.GetMessage("UserNotFound"))
            .Returns("User not found");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "User not found");

        _mockRefreshTokenService.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockRefreshTokenService.Verify(x => x.GenerateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }
}