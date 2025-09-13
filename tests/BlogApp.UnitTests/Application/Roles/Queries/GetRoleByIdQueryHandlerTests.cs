namespace BlogApp.UnitTests.Application.Roles.Queries;

public class GetRoleByIdQueryHandlerTests : BaseApplicationTest
{
    private readonly GetRoleByIdQueryHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public GetRoleByIdQueryHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        _handler = new GetRoleByIdQueryHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnRole()
    {
        // Arrange
        var query = new GetRoleByIdQuery
        {
            Id = "role-id"
        };

        var role = new IdentityRole("Admin")
        {
            Id = query.Id,
            Name = "Admin",
            NormalizedName = "ADMIN"
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(query.Id))
            .ReturnsAsync(role);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(query.Id);
        result.Data.Name.Should().Be("Admin");
        result.Data.NormalizedName.Should().Be("ADMIN");
    }

    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetRoleByIdQuery
        {
            Id = "non-existent-role-id"
        };

        // Setup mocks
        _mockRoleManager.Setup(x => x.FindByIdAsync(query.Id))
            .ReturnsAsync((IdentityRole)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetRoleByIdQuery
        {
            Id = "role-id"
        };

        // Setup mocks to throw exception
        _mockRoleManager.Setup(x => x.FindByIdAsync(query.Id))
            .Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}