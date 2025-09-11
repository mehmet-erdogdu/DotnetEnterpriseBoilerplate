using BlogApp.Application.Roles.Commands;

namespace BlogApp.UnitTests.Application.Roles.Commands;

public class AddRoleClaimCommandHandlerTests : BaseApplicationTest
{
    private readonly AddRoleClaimCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public AddRoleClaimCommandHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new AddRoleClaimCommandHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddRoleClaimAndReturnSuccess()
    {
        // Arrange
        var command = new AddRoleClaimCommand
        {
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var claims = new List<Claim>
        {
            new(command.ClaimType, command.ClaimValue)
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.AddClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(claims);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.RoleId.Should().Be(command.RoleId);
        result.Data.ClaimType.Should().Be(command.ClaimType);
        result.Data.ClaimValue.Should().Be(command.ClaimValue);

        _mockRoleManager.Verify(x => x.AddClaimAsync(role, It.IsAny<Claim>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddRoleClaimCommand
        {
            RoleId = "non-existent-role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.AddClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleClaimAdditionFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddRoleClaimCommand
        {
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var identityError = new IdentityError { Code = "ClaimAdditionError", Description = "Failed to add claim" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.AddClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}