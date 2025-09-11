using BlogApp.Application.Users.Queries;

namespace BlogApp.UnitTests.Application.Users.Queries;

public class GetAllUsersQueryHandlerTests : BaseApplicationTest
{
    private readonly GetAllUsersQueryHandler _handler;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public GetAllUsersQueryHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = "user-1",
                Email = "user1@example.com",
                UserName = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "user-2",
                Email = "user2@example.com",
                UserName = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            }
        }.AsQueryable();

        _mockUserManager.Setup(x => x.Users)
            .Returns(users);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        _handler = new GetAllUsersQueryHandler(
            _mockUserManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery
        {
            Page = 1,
            PageSize = 10
        };

        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = "user-1",
                Email = "user1@example.com",
                UserName = "user1@example.com",
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = "user-2",
                Email = "user2@example.com",
                UserName = "user2@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        // Setup mocks with async support
        var mockUsers = users.AsQueryable();
        _mockUserManager.Setup(x => x.Users).Returns(mockUsers);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        // Setup message service to avoid exception messages
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) =>
            {
                if (key == "UsersRetrievalError")
                    return $"Error retrieving users: {args.FirstOrDefault() ?? "Unknown error"}";
                return "Operation successful";
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Debug: Print the error message if there is one
        if (!result.IsSuccess) Console.WriteLine($"Error message: {result.Error?.Message}");

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);

        var userList = result.Data!.ToList();
        userList[0].Email.Should().Be("user1@example.com");
        userList[0].FirstName.Should().Be("John");
        userList[0].LastName.Should().Be("Doe");
        userList[0].EmailConfirmed.Should().BeTrue();
        userList[0].Roles.Should().Contain("User");

        userList[1].Email.Should().Be("user2@example.com");
        userList[1].FirstName.Should().Be("Jane");
        userList[1].LastName.Should().Be("Smith");
        userList[1].EmailConfirmed.Should().BeFalse();
        userList[1].Roles.Should().Contain("User");
    }

    [Fact]
    public async Task Handle_WithSearchQuery_ShouldReturnFilteredUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery
        {
            Page = 1,
            PageSize = 10,
            Search = "John"
        };

        var users = new List<ApplicationUser>
        {
            new()
            {
                Id = "user-1",
                Email = "john.doe@example.com",
                UserName = "john.doe@example.com",
                FirstName = "John",
                LastName = "Doe",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Setup mocks with async support
        var mockUsers = users.AsQueryable();
        _mockUserManager.Setup(x => x.Users).Returns(mockUsers);

        _mockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });

        // Setup message service to avoid exception messages
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) =>
            {
                if (key == "UsersRetrievalError")
                    return $"Error retrieving users: {args.FirstOrDefault() ?? "Unknown error"}";
                return "Operation successful";
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Debug: Print the error message if there is one
        if (!result.IsSuccess) Console.WriteLine($"Error message: {result.Error?.Message}");

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetAllUsersQuery();

        // Setup mocks to throw exception
        _mockUserManager.Setup(x => x.Users)
            .Throws(new Exception("Database error"));

        // Setup message service to return proper error message
        _mockMessageService.Setup(x => x.GetMessage("UsersRetrievalError", It.IsAny<object[]>()))
            .Returns("Database error occurred");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}