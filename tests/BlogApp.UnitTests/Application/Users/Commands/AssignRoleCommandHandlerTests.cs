using BlogApp.Application.Users.Commands;

namespace BlogApp.UnitTests.Application.Users.Commands;

public class AssignRoleCommandHandlerTests : BaseApplicationTest
{
    private readonly AssignRoleCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public AssignRoleCommandHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new AssignRoleCommandHandler(
            _mockUserManager.Object,
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAssignRoleAndReturnSuccess()
    {
        // Arrange
        var command = new AssignRoleCommand
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
            .ReturnsAsync(new List<string>()); // User has no roles initially

        _mockUserManager.Setup(x => x.AddToRoleAsync(user, command.RoleName))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockUserManager.Verify(x => x.AddToRoleAsync(user, command.RoleName), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignRoleCommand
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
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignRoleCommand
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
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyInRole_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignRoleCommand
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
            .ReturnsAsync(new List<string> { "Admin" }); // User already has the role

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleAssignmentFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new AssignRoleCommand
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

        var identityError = new IdentityError { Code = "RoleAssignmentError", Description = "Failed to assign role" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.RoleName))
            .ReturnsAsync(role);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string>()); // User has no roles initially

        _mockUserManager.Setup(x => x.AddToRoleAsync(user, command.RoleName))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}