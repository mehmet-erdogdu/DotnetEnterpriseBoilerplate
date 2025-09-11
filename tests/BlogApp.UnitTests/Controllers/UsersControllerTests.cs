using BlogApp.Application.Users.Commands;
using BlogApp.Application.Users.Queries;

namespace BlogApp.UnitTests.Controllers;

public class UsersControllerTests : BaseControllerTest
{
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _controller = new UsersController(_mockMediator.Object);
        SetupAuthenticatedUser(_controller, "admin-user-id");
    }

    #region GetAll Tests

    [Fact]
    public async Task GetAll_WithValidQuery_ShouldReturnUsers()
    {
        // Arrange
        var query = new GetAllUsersQuery { Page = 1, PageSize = 10 };
        var users = new List<UserDto>
        {
            new() { Id = "user-1", Email = "user1@example.com", FirstName = "John", LastName = "Doe" },
            new() { Id = "user-2", Email = "user2@example.com", FirstName = "Jane", LastName = "Smith" }
        };
        var expectedResponse = ApiResponse<IEnumerable<UserDto>>.Success(users);

        _mockMediator.Setup(x => x.Send(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(2);
    }

    #endregion

    #region GetById Tests

    [Fact]
    public async Task GetById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = "test-user-id";
        var user = new UserDto
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var expectedResponse = ApiResponse<UserDto>.Success(user);

        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetById(userId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(userId);
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Delete_WithValidId_ShouldDeleteUser()
    {
        // Arrange
        var userId = "test-user-id";
        var expectedResponse = ApiResponse<string>.Success("User deleted successfully");

        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(It.Is<DeleteUserCommand>(cmd => cmd.Id == userId), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task Create_WithValidModel_ShouldCreateUser()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "New",
            LastName = "User"
        };

        var user = new UserDto
        {
            Id = "new-user-id",
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName
        };

        var expectedResponse = ApiResponse<UserDto>.Success(user);

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Create(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email);

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "invalid-email",
            Password = "weak",
            FirstName = "",
            LastName = ""
        };

        _controller.ModelState.AddModelError("Email", "Invalid email format");
        _controller.ModelState.AddModelError("Password", "Password too weak");

        // Act
        var result = await _controller.Create(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task Update_WithValidModel_ShouldUpdateUser()
    {
        // Arrange
        var userId = "test-user-id";
        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "updated@example.com",
            FirstName = "Updated",
            LastName = "User",
            EmailConfirmed = true
        };

        var user = new UserDto
        {
            Id = userId,
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            EmailConfirmed = command.EmailConfirmed
        };

        var expectedResponse = ApiResponse<UserDto>.Success(user);

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Update(userId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(command.Email);

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var userId = "test-user-id";
        var command = new UpdateUserCommand
        {
            Id = userId,
            Email = "",
            FirstName = "",
            LastName = ""
        };

        _controller.ModelState.AddModelError("Email", "Email is required");
        _controller.ModelState.AddModelError("FirstName", "First name is required");

        // Act
        var result = await _controller.Update(userId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Update_WithMismatchedIds_ShouldReturnFailure()
    {
        // Arrange
        var userId = "test-user-id";
        var command = new UpdateUserCommand
        {
            Id = "different-id",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Act
        var result = await _controller.Update(userId, command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region AssignRole Tests

    [Fact]
    public async Task AssignRole_WithValidModel_ShouldAssignRole()
    {
        // Arrange
        var command = new AssignRoleCommand
        {
            UserId = "test-user-id",
            RoleName = "Admin"
        };

        var expectedResponse = ApiResponse<string>.Success("Role assigned successfully");

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AssignRole(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRole_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = new AssignRoleCommand
        {
            UserId = "",
            RoleName = ""
        };

        _controller.ModelState.AddModelError("UserId", "User ID is required");
        _controller.ModelState.AddModelError("RoleName", "Role name is required");

        // Act
        var result = await _controller.AssignRole(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<AssignRoleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region RemoveRole Tests

    [Fact]
    public async Task RemoveRole_WithValidModel_ShouldRemoveRole()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "test-user-id",
            RoleName = "Admin"
        };

        var expectedResponse = ApiResponse<string>.Success("Role removed successfully");

        _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.RemoveRole(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockMediator.Verify(x => x.Send(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveRole_WithInvalidModel_ShouldReturnValidationErrors()
    {
        // Arrange
        var command = new RemoveRoleCommand
        {
            UserId = "",
            RoleName = ""
        };

        _controller.ModelState.AddModelError("UserId", "User ID is required");
        _controller.ModelState.AddModelError("RoleName", "Role name is required");

        // Act
        var result = await _controller.RemoveRole(command);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockMediator.Verify(x => x.Send(It.IsAny<RemoveRoleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}