using BlogApp.Application.Roles.Commands;

namespace BlogApp.UnitTests.Application.Roles.Commands;

public class UpdateRoleCommandHandlerTests : BaseApplicationTest
{
    private readonly UpdateRoleCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public UpdateRoleCommandHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new UpdateRoleCommandHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateRoleAndReturnSuccess()
    {
        // Arrange
        var command = new UpdateRoleCommand
        {
            Id = "role-id",
            Name = "UpdatedAdmin"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.Id
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((IdentityRole)null!); // No role with the new name exists

        _mockRoleManager.Setup(x => x.UpdateAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);

        _mockRoleManager.Verify(x => x.UpdateAsync(role), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleCommand
        {
            Id = "non-existent-role-id",
            Name = "UpdatedAdmin"
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.UpdateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleNameAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleCommand
        {
            Id = "role-id",
            Name = "Admin"
        };

        var role = new IdentityRole("User")
        {
            Id = command.Id
        };

        var existingRole = new IdentityRole("Admin");

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync(existingRole); // Role with the new name already exists

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.UpdateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleUpdateFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleCommand
        {
            Id = "role-id",
            Name = "UpdatedAdmin"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.Id
        };

        var identityError = new IdentityError { Code = "UpdateError", Description = "Role update failed" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((IdentityRole)null!); // No role with the new name exists

        _mockRoleManager.Setup(x => x.UpdateAsync(role))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}