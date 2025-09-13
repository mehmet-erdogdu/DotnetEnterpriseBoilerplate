namespace BlogApp.UnitTests.Application.Users.Commands;

public class CreateUserCommandHandlerTests : BaseApplicationTest
{
    private readonly CreateUserCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public CreateUserCommandHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new CreateUserCommandHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserAndReturnSuccess()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe",
            Roles = new List<string> { "User" }
        };

        var user = new ApplicationUser
        {
            Id = "test-user-id",
            Email = command.Email,
            UserName = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Setup mocks
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _mockRoleManager.Setup(x => x.FindByNameAsync("User"))
            .ReturnsAsync(new IdentityRole("User"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email);
        result.Data.FirstName.Should().Be(command.FirstName);
        result.Data.LastName.Should().Be(command.LastName);
        result.Data.Roles.Should().Contain("User");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), "User"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var existingUser = new ApplicationUser
        {
            Id = "existing-user-id",
            Email = command.Email,
            UserName = command.Email,
            FirstName = "Existing",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "John",
            LastName = "Doe"
        };

        var identityError = new IdentityError { Code = "DuplicateEmail", Description = "Email is already taken" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser)null!);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}