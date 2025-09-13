namespace BlogApp.UnitTests.Application.Users.Commands;

public class RemoveRoleCommandHandlerTests : BaseApplicationTest
{
    private readonly RemoveRoleCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public RemoveRoleCommandHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new RemoveRoleCommandHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveRoleAndReturnSuccess()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "test-user-id",
            RoleName = "Admin"
        };

        var user = new ApplicationUser
        {
            Id = command.UserId,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var role = new IdentityRole("Admin");

        // Setup mocks
        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.RoleName))
            .ReturnsAsync(role);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" }); // User has the role

        _mockUserManager.Setup(x => x.RemoveFromRoleAsync(user, command.RoleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockUserManager.Verify(x => x.RemoveFromRoleAsync(user, command.RoleName), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "non-existent-user-id",
            RoleName = "Admin"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "test-user-id",
            RoleName = "NonExistentRole"
        };

        var user = new ApplicationUser
        {
            Id = command.UserId,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.RoleName))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotInRole_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "test-user-id",
            RoleName = "Admin"
        };

        var user = new ApplicationUser
        {
            Id = command.UserId,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var role = new IdentityRole("Admin");

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.RoleName))
            .ReturnsAsync(role);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>()); // User doesn't have the role

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleRemovalFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "test-user-id",
            RoleName = "Admin"
        };

        var user = new ApplicationUser
        {
            Id = command.UserId,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var role = new IdentityRole("Admin");

        var identityError = new IdentityError { Code = "RoleRemovalError", Description = "Failed to remove role" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.RoleName))
            .ReturnsAsync(role);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "Admin" }); // User has the role

        _mockUserManager.Setup(x => x.RemoveFromRoleAsync(user, command.RoleName))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}