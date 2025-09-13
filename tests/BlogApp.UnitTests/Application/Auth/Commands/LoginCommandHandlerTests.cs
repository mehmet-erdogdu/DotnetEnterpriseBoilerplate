namespace BlogApp.UnitTests.Application.Auth.Commands;

public class LoginCommandHandlerTests : BaseApplicationTest
{
    private readonly LoginCommandHandler _handler;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public LoginCommandHandlerTests()
    {
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();
        _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        _mockConfiguration = new Mock<IConfiguration>();

        SetupConfiguration();

        _handler = new LoginCommandHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockRefreshTokenService.Object,
            _mockMessageService.Object,
            _mockConfiguration.Object);
    }

    private void SetupConfiguration()
    {
        _mockConfiguration.Setup(x => x["JWT:Secret"]).Returns(new string('x', 64));
        _mockConfiguration.Setup(x => x["JWT:ValidIssuer"]).Returns("test-issuer");
        _mockConfiguration.Setup(x => x["JWT:ValidAudience"]).Returns("test-audience");
        _mockConfiguration.Setup(x => x["JWT:TokenExpirationMinutes"]).Returns("30");
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!"
        };

        var user = TestHelper.TestData.CreateTestUser(email: command.Email);
        var refreshToken = "refresh_token_123";
        var userRoles = new List<string> { "User" };
        var role = new IdentityRole("User");

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(true);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(userRoles);

        _mockRoleManager.Setup(x => x.FindByNameAsync("User"))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(new List<Claim>());

        _mockRefreshTokenService.Setup(x => x.GenerateRefreshTokenAsync(user.Id))
            .ReturnsAsync(refreshToken);

        _mockMessageService.Setup(x => x.GetMessage("InvalidCredentials"))
            .Returns("Invalid credentials");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.AccessToken.Should().NotBeNullOrEmpty();
        result.Data.RefreshToken.Should().Be(refreshToken);
        result.Data.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromMinutes(1));

        _mockUserManager.Verify(x => x.FindByEmailAsync(command.Email), Times.Once);
        _mockUserManager.Verify(x => x.CheckPasswordAsync(user, command.Password), Times.Once);
        _mockUserManager.Verify(x => x.GetRolesAsync(user), Times.Once);
        _mockRoleManager.Verify(x => x.FindByNameAsync("User"), Times.Once);
        _mockRefreshTokenService.Verify(x => x.GenerateRefreshTokenAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "P@ssw0rd123!"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _mockMessageService.Setup(x => x.GetMessage("InvalidCredentials"))
            .Returns("Invalid credentials");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid credentials");

        _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockRefreshTokenService.Verify(x => x.GenerateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = TestHelper.TestData.CreateTestUser(email: command.Email);

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.Password))
            .ReturnsAsync(false);

        _mockMessageService.Setup(x => x.GetMessage("InvalidCredentials"))
            .Returns("Invalid credentials");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Invalid credentials");

        _mockRefreshTokenService.Verify(x => x.GenerateRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }
}