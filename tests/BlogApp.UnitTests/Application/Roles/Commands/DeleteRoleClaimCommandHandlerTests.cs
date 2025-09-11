using BlogApp.Application.Roles.Commands;

namespace BlogApp.UnitTests.Application.Roles.Commands;

public class DeleteRoleClaimCommandHandlerTests : BaseApplicationTest
{
    private readonly DeleteRoleClaimCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public DeleteRoleClaimCommandHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new DeleteRoleClaimCommandHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteRoleClaimAndReturnSuccess()
    {
        // Arrange
        var command = new DeleteRoleClaimCommand
        {
            Id = 0, // Index of the claim in the list
            RoleId = "role-id"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanManageUsers")
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, existingClaims[0]))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockRoleManager.Verify(x => x.RemoveClaimAsync(role, existingClaims[0]), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteRoleClaimCommand
        {
            Id = 0,
            RoleId = "non-existent-role-id"
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.RemoveClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleClaimNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteRoleClaimCommand
        {
            Id = 5, // Index out of range
            RoleId = "role-id"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanManageUsers"),
            new("Permission", "CanViewReports")
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockRoleManager.Verify(x => x.RemoveClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleClaimDeletionFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteRoleClaimCommand
        {
            Id = 0,
            RoleId = "role-id"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanManageUsers")
        };

        var identityError = new IdentityError { Code = "ClaimDeletionError", Description = "Failed to delete claim" };
        var identityResult = IdentityResult.Failed(identityError);

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, existingClaims[0]))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}