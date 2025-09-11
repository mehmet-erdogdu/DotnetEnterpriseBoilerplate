using BlogApp.Application.Roles.Commands;
using BlogApp.Application.Roles.Queries;

namespace BlogApp.UnitTests.Controllers;

public class RolesControllerTests : BaseControllerTest
{
    private readonly RolesController _controller;

    public RolesControllerTests()
    {
        _controller = new RolesController(_mockMediator.Object);
        SetupAuthenticatedUser(_controller, "admin-user-id");
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidQuery_ShouldReturnRoles()
    {
        // Arrange
        var roles = new List<RoleDto>
        {
            new() { Id = "role-1", Name = "Admin" },
            new() { Id = "role-2", Name = "User" }
        };
        var expectedResponse = ApiResponse<IEnumerable<RoleDto>>.Success(roles);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetAllRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAll();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteRole()
    {
        // Arrange
        var roleId = "test-role-id";
        var expectedResponse = ApiResponse<string>.Success("Role deleted successfully");

        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Delete(roleId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.Is<DeleteRoleCommand>(cmd => cmd.Id == roleId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetRoleClaims Tests

    [Fact]
    public async Task GetRoleClaims_WithValidRoleId_ShouldReturnRoleClaims()
    {
        // Arrange
        var roleId = "test-role-id";
        var roleClaims = new List<RoleClaimDto>
        {
            new() { Id = 1, RoleId = roleId, ClaimType = "Permission", ClaimValue = "CanManageUsers" },
            new() { Id = 2, RoleId = roleId, ClaimType = "Permission", ClaimValue = "CanViewReports" }
        };
        var expectedResponse = ApiResponse<IEnumerable<RoleClaimDto>>.Success(roleClaims);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetRoleClaimsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetRoleClaims(roleId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    #endregion

    #region GetRoleClaimById Tests

    [Fact]
    public async Task GetRoleClaimById_WithValidIds_ShouldReturnRoleClaim()
    {
        // Arrange
        var roleId = "test-role-id";
        var claimId = 1;
        var roleClaim = new RoleClaimDto
        {
            Id = claimId,
            RoleId = roleId,
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };
        var expectedResponse = ApiResponse<RoleClaimDto>.Success(roleClaim);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetRoleClaimByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetRoleClaimById(roleId, claimId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(claimId);
        result.Data.RoleId.Should().Be(roleId);
    }

    #endregion

    #region DeleteRoleClaim Tests

    [Fact]
    public async Task DeleteRoleClaim_WithValidIds_ShouldDeleteRoleClaim()
    {
        // Arrange
        var roleId = "test-role-id";
        var claimId = 1;
        var expectedResponse = ApiResponse<string>.Success("Role claim deleted successfully");

        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteRoleClaimCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DeleteRoleClaim(roleId, claimId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.Is<DeleteRoleClaimCommand>(cmd => cmd.RoleId == roleId && cmd.Id == claimId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnRole()
    {
        // Arrange
        var roleId = "test-role-id";
        var role = new RoleDto
        {
            Id = roleId,
            Name = "Admin"
        };
        var expectedResponse = ApiResponse<RoleDto>.Success(role);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetById(roleId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(roleId);
        result.Data.Name.Should().Be("Admin");
    }

    [Fact]
    public async Task GetById_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var roleId = "non-existent-role-id";
        var expectedResponse = ApiResponse<RoleDto>.Failure("Role not found");

        _mockMediator.Setup(x => x.Send(It.IsAny<GetRoleByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetById(roleId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidModel_ShouldCreateRole()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = "NewRole"
        };

        var role = new RoleDto
        {
            Id = "new-role-id",
            Name = "NewRole"
        };
        var expectedResponse = ApiResponse<RoleDto>.Success(role);

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Create(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = new CreateRoleCommand
        {
            Name = ""
        };

        _controller.ModelState.AddModelError("Name", "Role name is required");

        // Act
        var result = await _controller.Create(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidModel_ShouldUpdateRole()
    {
        // Arrange
        var roleId = "test-role-id";
        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = "UpdatedRole"
        };

        var role = new RoleDto
        {
            Id = roleId,
            Name = "UpdatedRole"
        };
        var expectedResponse = ApiResponse<RoleDto>.Success(role);

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Update(roleId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(command.Name);

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var roleId = "test-role-id";
        var command = new UpdateRoleCommand
        {
            Id = roleId,
            Name = ""
        };

        _controller.ModelState.AddModelError("Name", "Role name is required");

        // Act
        var result = await _controller.Update(roleId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ShouldReturnFailure()
    {
        // Arrange
        var roleId = "test-role-id";
        var command = new UpdateRoleCommand
        {
            Id = "different-id",
            Name = "TestRole"
        };

        // Act
        var result = await _controller.Update(roleId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region AddRoleClaim Tests

    [Fact]
    public async Task AddRoleClaim_WithValidModel_ShouldAddRoleClaim()
    {
        // Arrange
        var roleId = "test-role-id";
        var command = new AddRoleClaimCommand
        {
            RoleId = roleId,
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var roleClaim = new RoleClaimDto
        {
            Id = 1,
            RoleId = roleId,
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };
        var expectedResponse = ApiResponse<RoleClaimDto>.Success(roleClaim);

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AddRoleClaim(roleId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.RoleId.Should().Be(command.RoleId);
        result.Data.ClaimType.Should().Be(command.ClaimType);
        result.Data.ClaimValue.Should().Be(command.ClaimValue);

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRoleClaim_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var roleId = "test-role-id";
        var command = new AddRoleClaimCommand
        {
            RoleId = roleId,
            ClaimType = "",
            ClaimValue = ""
        };

        _controller.ModelState.AddModelError("ClaimType", "Claim type is required");

        // Act
        var result = await _controller.AddRoleClaim(roleId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<AddRoleClaimCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddRoleClaim_WithMismatchedRoleIds_ShouldReturnFailure()
    {
        // Arrange
        var roleId = "test-role-id";
        var command = new AddRoleClaimCommand
        {
            RoleId = "different-role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        // Act
        var result = await _controller.AddRoleClaim(roleId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<AddRoleClaimCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region UpdateRoleClaim Tests

    [Fact]
    public async Task UpdateRoleClaim_WithValidModel_ShouldUpdateRoleClaim()
    {
        // Arrange
        var roleId = "test-role-id";
        var claimId = 1;
        var command = new UpdateRoleClaimCommand
        {
            Id = claimId,
            RoleId = roleId,
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        var roleClaim = new RoleClaimDto
        {
            Id = claimId,
            RoleId = roleId,
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };
        var expectedResponse = ApiResponse<RoleClaimDto>.Success(roleClaim);

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateRoleClaim(roleId, claimId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(command.Id);
        result.Data.RoleId.Should().Be(command.RoleId);
        result.Data.ClaimType.Should().Be(command.ClaimType);
        result.Data.ClaimValue.Should().Be(command.ClaimValue);

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleClaim_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var roleId = "test-role-id";
        var claimId = 1;
        var command = new UpdateRoleClaimCommand
        {
            Id = claimId,
            RoleId = roleId,
            ClaimType = "",
            ClaimValue = ""
        };

        _controller.ModelState.AddModelError("ClaimType", "Claim type is required");

        // Act
        var result = await _controller.UpdateRoleClaim(roleId, claimId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateRoleClaimCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleClaim_WithMismatchedRoleIds_ShouldReturnFailure()
    {
        // Arrange
        var roleId = "test-role-id";
        var claimId = 1;
        var command = new UpdateRoleClaimCommand
        {
            Id = claimId,
            RoleId = "different-role-id",
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        // Act
        var result = await _controller.UpdateRoleClaim(roleId, claimId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateRoleClaimCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRoleClaim_WithMismatchedClaimIds_ShouldReturnFailure()
    {
        // Arrange
        var roleId = "test-role-id";
        var claimId = 1;
        var command = new UpdateRoleClaimCommand
        {
            Id = 2, // Different claim ID
            RoleId = roleId,
            ClaimType = "Permission",
            ClaimValue = "CanManageUsers"
        };

        // Act
        var result = await _controller.UpdateRoleClaim(roleId, claimId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateRoleClaimCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}