using BlogApp.Application.Users.Queries;

namespace BlogApp.UnitTests.Application.Users.Queries;

public class GetUserByIdQueryHandlerTests : BaseApplicationTest
{
    private readonly GetUserByIdQueryHandler _handler;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public GetUserByIdQueryHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        _handler = new GetUserByIdQueryHandler(
            _mockUserManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var query = new GetUserByIdQuery
        {
            Id = "test-user-id"
        };

        var user = new ApplicationUser
        {
            Id = query.Id,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        // Setup mocks
        _mockUserManager.Setup(x => x.FindByIdAsync(query.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User", "Admin" });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(query.Id);
        result.Data.Email.Should().Be("test@example.com");
        result.Data.FirstName.Should().Be("John");
        result.Data.LastName.Should().Be("Doe");
        result.Data.EmailConfirmed.Should().BeTrue();
        result.Data.Roles.Should().Contain("User");
        result.Data.Roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetUserByIdQuery
        {
            Id = "non-existent-user-id"
        };

        // Setup mocks
        _mockUserManager.Setup(x => x.FindByIdAsync(query.Id))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetUserByIdQuery
        {
            Id = "test-user-id"
        };

        // Setup mocks to throw exception
        _mockUserManager.Setup(x => x.FindByIdAsync(query.Id))
            .Throws(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}