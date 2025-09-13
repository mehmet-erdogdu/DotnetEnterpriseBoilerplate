namespace BlogApp.UnitTests.Application.Roles.Commands;

public class UpdateRoleClaimCommandHandlerTests : BaseApplicationTest
{
    private readonly UpdateRoleClaimCommandHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public UpdateRoleClaimCommandHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new UpdateRoleClaimCommandHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateRoleClaimAndReturnSuccess()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanViewUsers")
        };

        var newClaim = new Claim(command.ClaimType, command.ClaimValue);

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockRoleManager.Setup(x => x.AddClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(command.Id);
        result.Data.RoleId.Should().Be(command.RoleId);
        result.Data.ClaimType.Should().Be(command.ClaimType);
        result.Data.ClaimValue.Should().Be(command.ClaimValue);

        _mockRoleManager.Verify(x => x.RemoveClaimAsync(role, It.IsAny<Claim>()), Times.Once);
        _mockRoleManager.Verify(x => x.AddClaimAsync(role, It.IsAny<Claim>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
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
        _mockRoleManager.Verify(x => x.RemoveClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
        _mockRoleManager.Verify(x => x.AddClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleClaimNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "NonExistentPermission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanViewUsers")
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
        _mockRoleManager.Verify(x => x.AddClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRoleClaimRemovalFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanViewUsers")
        };

        var identityError = new IdentityError { Code = "ClaimRemovalError", Description = "Failed to remove claim" };
        var identityResult = IdentityResult.Failed(identityError);

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(identityResult);

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
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanViewUsers")
        };

        var newClaim = new Claim(command.ClaimType, command.ClaimValue);
        var identityError = new IdentityError { Code = "ClaimAdditionError", Description = "Failed to add claim" };
        var identityResult = IdentityResult.Failed(identityError);

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockRoleManager.Setup(x => x.AddClaimAsync(role, newClaim))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldReturnFailureWithErrorMessage()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error!.Message.Should().Contain("RoleClaimUpdateError");
    }

    [Fact]
    public async Task Handle_WithMultipleExistingClaims_ShouldFindCorrectClaimToUpdate()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanViewUsers"),
            new("Permission", "CanEditUsers"),
            new("Role", "Administrator")
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "CanViewUsers")))
            .ReturnsAsync(IdentityResult.Success);

        _mockRoleManager.Setup(x => x.AddClaimAsync(role, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "CanManageUsers")))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.ClaimType.Should().Be("Permission");
        result.Data.ClaimValue.Should().Be("CanManageUsers");

        // Verify that the first matching claim was removed
        _mockRoleManager.Verify(x => x.RemoveClaimAsync(role, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "CanViewUsers")), Times.Once);
        _mockRoleManager.Verify(x => x.AddClaimAsync(role, It.Is<Claim>(c => c.Type == "Permission" && c.Value == "CanManageUsers")), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyExistingClaims_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>();

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
        _mockRoleManager.Verify(x => x.AddClaimAsync(It.IsAny<IdentityRole>(), It.IsAny<Claim>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSetUpdatedAtTimestamp()
    {
        // Arrange
        var command = new UpdateRoleClaimCommand
        {
            Id = 1,
            RoleId = "role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var role = new IdentityRole("Admin")
        {
            Id = command.RoleId,
            Name = "Admin"
        };

        var existingClaims = new List<Claim>
        {
            new("Permission", "CanViewUsers")
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(command.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(existingClaims);

        _mockRoleManager.Setup(x => x.RemoveClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockRoleManager.Setup(x => x.AddClaimAsync(role, It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}