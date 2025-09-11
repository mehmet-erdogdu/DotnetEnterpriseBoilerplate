using BlogApp.Application.Roles.Commands;

namespace BlogApp.UnitTests.Application.Roles.Commands;

public class CreateRoleCommandHandlerTests : BaseApplicationTest
{
    private readonly CreateRoleCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public CreateRoleCommandHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new CreateRoleCommandHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateRoleAndReturnSuccess()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "Admin"
        };

        var role = new IdentityRole(command.Name);

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((IdentityRole)null!);

        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);

        _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleAlreadyExists_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "Admin"
        };

        var existingRole = new IdentityRole(command.Name);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync(existingRole);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleCreationFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "Admin"
        };

        var identityError = new IdentityError { Code = "DuplicateRole", Description = "Role already exists" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockRoleManager.Setup(x => x.FindByNameAsync(command.Name))
            .ReturnsAsync((IdentityRole)null!);

        _mockRoleManager.Setup(x => x.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}