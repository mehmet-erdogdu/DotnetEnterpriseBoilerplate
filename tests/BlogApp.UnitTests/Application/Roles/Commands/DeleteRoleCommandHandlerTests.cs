namespace BlogApp.UnitTests.Application.Roles.Commands;

public class DeleteRoleCommandHandlerTests : BaseApplicationTest
{
    private readonly DeleteRoleCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public DeleteRoleCommandHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new DeleteRoleCommandHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteRoleAndReturnSuccess()
    {
        // Arrange
        var command = new DeleteRoleCommand
        {
            Id = "role-id"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.Id,
            Name = "Admin"
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockRoleManager.Verify(x => x.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteRoleCommand
        {
            Id = "non-existent-role-id"
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.DeleteAsync(It.IsAny<IdentityRole>()), Times.Never);
    }


    [Fact]
    public async Task Handle_WhenRoleDeletionFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteRoleCommand
        {
            Id = "role-id"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.Id,
            Name = "Admin"
        };

        var identityError = new IdentityError { Code = "DeletionError", Description = "Role deletion failed" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.DeleteAsync(role))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}