namespace BlogApp.UnitTests.Application.Roles.Queries;

public class GetRoleClaimByIdQueryHandlerTests : BaseApplicationTest
{
    private readonly GetRoleClaimByIdQueryHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public GetRoleClaimByIdQueryHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new GetRoleClaimByIdQueryHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnRoleClaim()
    {
        // Arrange
        var query = new GetRoleClaimByIdQuery
        {
            RoleId = "role-id",
            Id = 0 // Index of the claim in the list
        };

        var role = new IdentityRole("Admin")
        {
            Id = query.RoleId,
            Name = "Admin"
        };

        var claims = new List<Claim>
        {
            new("Permission", "CanManageUsers")
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(query.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(claims);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(query.Id);
        result.Data.RoleId.Should().Be(query.RoleId);
        result.Data.ClaimType.Should().Be("Permission");
        result.Data.ClaimValue.Should().Be("CanManageUsers");
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetRoleClaimByIdQuery
        {
            RoleId = "non-existent-role-id",
            Id = 0
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(query.RoleId))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }

    [Fact]
    public async Task Handle_WhenRoleClaimNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetRoleClaimByIdQuery
        {
            RoleId = "role-id",
            Id = 5 // Index out of range
        };

        var role = new IdentityRole("Admin")
        {
            Id = query.RoleId,
            Name = "Admin"
        };

        var claims = new List<Claim>
        {
            new("Permission", "CanManageUsers"),
            new("Permission", "CanViewReports")
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(query.RoleId))
            .ReturnsAsync(role);

        _mockRoleManager.Setup(x => x.GetClaimsAsync(role))
            .ReturnsAsync(claims);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}