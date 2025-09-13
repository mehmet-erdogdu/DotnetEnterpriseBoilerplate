namespace BlogApp.UnitTests.Application.Users.Commands;

public class DeleteUserCommandHandlerTests : BaseApplicationTest
{
    private readonly DeleteUserCommandHandler _handler;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public DeleteUserCommandHandlerTests()
    {
        // Setup UserManager mock
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();

        _handler = new DeleteUserCommandHandler(
            _mockUserManager.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteUserAndReturnSuccess()
    {
        // Arrange
        var command = new DeleteUserCommand
        {
            Id = "test-user-id"
        };

        var user = new ApplicationUser
        {
            Id = command.Id,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "John",
            LastName = "Doe"
        };

        // Setup mocks
        _mockUserManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();

        _mockUserManager.Verify(x => x.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteUserCommand
        {
            Id = "non-existent-user-id"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync((ApplicationUser)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        _mockUserManager.Verify(x => x.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserDeletionFails_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteUserCommand
        {
            Id = "test-user-id"
        };

        var user = new ApplicationUser
        {
            Id = command.Id,
            Email = "test@example.com",
            UserName = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        var identityError = new IdentityError { Code = "DeletionError", Description = "User deletion failed" };
        var identityResult = IdentityResult.Failed(identityError);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.Id))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(identityResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
    }
}