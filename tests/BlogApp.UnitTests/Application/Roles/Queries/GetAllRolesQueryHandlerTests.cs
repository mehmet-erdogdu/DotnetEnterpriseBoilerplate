namespace BlogApp.UnitTests.Application.Roles.Queries;

public class GetAllRolesQueryHandlerTests : BaseApplicationTest
{
    private readonly GetAllRolesQueryHandler _handler;
    private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

    public GetAllRolesQueryHandlerTests()
    {
        // Setup RoleManager mock
        _mockRoleManager = TestHelper.MockSetups.CreateMockRoleManager();

        var mockRoles = new List<IdentityRole>
        {
            new("Admin") { Id = "role-1", NormalizedName = "ADMIN" },
            new("User") { Id = "role-2", NormalizedName = "USER" },
            new("Moderator") { Id = "role-3", NormalizedName = "MODERATOR" }
        }.AsQueryable();

        _mockRoleManager.Setup(x => x.Roles)
            .Returns(mockRoles);

        _handler = new GetAllRolesQueryHandler(
            _mockRoleManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnRoles()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        var roles = new List<IdentityRole>
        {
            new("Admin") { Id = "role-1", NormalizedName = "ADMIN" },
            new("User") { Id = "role-2", NormalizedName = "USER" },
            new("Moderator") { Id = "role-3", NormalizedName = "MODERATOR" }
        };

        // Setup mocks with async support
        var mockRoles = roles.AsQueryable();
        _mockRoleManager.Setup(x => x.Roles).Returns(mockRoles);

        // Setup message service to return success message
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) =>
            {
                if (key == "RolesRetrievalError")
                    return $"Error retrieving roles: {args.FirstOrDefault() ?? "Unknown error"}";
                return "Operation successful";
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Debug: Print the error message if there is one
        if (!result.IsSuccess) Console.WriteLine($"Error message: {result.Error?.Message}");

        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(3);

        var roleList = result.Data!.ToList();
        roleList[0].Name.Should().Be("Admin");
        roleList[1].Name.Should().Be("User");
        roleList[2].Name.Should().Be("Moderator");
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllRolesQuery();

        // Setup mocks to throw exception
        _mockRoleManager.Setup(x => x.Roles)
            .Throws(new Exception("Database error"));

        // Setup message service to return proper error message
        _mockMessageService.Setup(x => x.GetMessage("RolesRetrievalError", It.IsAny<object[]>()))
            .Returns("Database error occurred");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}