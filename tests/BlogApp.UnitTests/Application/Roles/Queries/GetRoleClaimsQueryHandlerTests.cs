using BlogApp.Application.Roles.Queries;

namespace BlogApp.UnitTests.Application.Roles.Queries;

public class GetRoleClaimsQueryHandlerTests : BaseApplicationTest
{
    private readonly GetRoleClaimsQueryHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public GetRoleClaimsQueryHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new GetRoleClaimsQueryHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnRoleClaims()
    {
        // Arrange
        var query = new GetRoleClaimsQuery
        {
            RoleId = "role-id"
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
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetRoleClaimsQuery
        {
            RoleId = "non-existent-role-id"
        };

        _mockRoleManager.Setup(x => x.FindByIdAsync(query.RoleId))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}