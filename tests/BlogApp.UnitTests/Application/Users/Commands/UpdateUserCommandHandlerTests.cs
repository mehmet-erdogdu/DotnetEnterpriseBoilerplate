namespace BlogApp.UnitTests.Application.Users.Commands;

public class UpdateUserCommandHandlerTests : BaseApplicationTest
{
    private readonly UpdateUserCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public UpdateUserCommandHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new UpdateUserCommandHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateUserAndReturnSuccess()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            Id = "test-user-id",
            Email = "updated@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            EmailConfirmed = true,
            Roles = new List<string> { "User", "Admin" }
        };

        var user = new ApplicationUser
        {
            Id = command.Id,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Setup mocks
        _mockUserManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        _mockRoleManager.Setup(x => x.FindByNameAsync("Admin"))
            .ReturnsAsync(new IdentityRole("Admin"));

        _mockRoleManager.Setup(x => x.FindByNameAsync("User"))
            .ReturnsAsync(new IdentityRole("User"));

        // Mock the AddToRoleAsync method
        _mockUserManager.Setup(x => x.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Mock the RemoveFromRolesAsync method - no roles to remove in this case
        _mockUserManager.Setup(x => x.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        // Mock GetRolesAsync to return updated roles after the operation
        _mockUserManager.SetupSequence(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" }) // Before update
            .ReturnsAsync(new List<string> { "User", "Admin" }); // After update

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email);
        result.Data.FirstName.Should().Be(command.FirstName);
        result.Data.LastName.Should().Be(command.LastName);
        result.Data.EmailConfirmed.Should().Be(command.EmailConfirmed);
        result.Data.Roles.Should().Contain("User");
        result.Data.Roles.Should().Contain("Admin");

        _mockUserManager.Verify(x => x.UpdateAsync(user), Times.Once);
        _mockUserManager.Verify(x => x.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            Id = "non-existent-user-id",
            Email = "updated@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            EmailConfirmed = true
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            Id = "test-user-id",
            Email = "updated@example.com",
            FirstName = "Jane",
            LastName = "Smith",
            EmailConfirmed = true
        };

        var user = new ApplicationUser
        {
            Id = command.Id,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        var identityError = new IdentityError { Code = "InvalidEmail", Description = "Email is invalid" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}