namespace BlogApp.UnitTests.Application.Auth.Commands;

public class RegisterCommandHandlerTests : BaseTestClass
{
    private readonly RegisterCommandHandler _handler;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public RegisterCommandHandlerTests()
    {
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();
        _mockMessageService = new Mock<IMessageService>();
        _mockPasswordService = new Mock<IPasswordService>();

        _handler = new RegisterCommandHandler(
            _mockUserManager.Object,
            _mockMessageService.Object,
            _mockPasswordService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Success);

        _mockPasswordService.Setup(x => x.TrackPasswordChangeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockMessageService.Setup(x => x.GetMessage("UserCreatedSuccessfully"))
            .Returns("User created successfully");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().Be("User created successfully");

        _mockUserManager.Verify(x => x.FindByEmailAsync(command.Email), Times.Once);
        _mockUserManager.Verify(x => x.CreateAsync(It.Is<ApplicationUser>(u =>
            u.Email == command.Email &&
            u.UserName == command.Email &&
            u.FirstName == command.FirstName &&
            u.LastName == command.LastName
        ), command.Password), Times.Once);
        _mockPasswordService.Verify(x => x.TrackPasswordChangeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            Password = "P@ssw0rd123!",
            FirstName = "Test",
            LastName = "User"
        };

        var existingUser = TestHelper.TestData.CreateTestUser(email: command.Email);
        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync(existingUser);

        _mockMessageService.Setup(x => x.GetMessage("UserAlreadyExists"))
            .Returns("User already exists");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "User already exists");

        _mockUserManager.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockPasswordService.Verify(x => x.TrackPasswordChangeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithFailedUserCreation_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "test@example.com",
            Password = "weak",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserManager.Setup(x => x.FindByEmailAsync(command.Email))
            .ReturnsAsync((ApplicationUser?)null);

        var identityErrors = new List<IdentityError>
        {
            new() { Code = "PasswordTooShort", Description = "Password is too short" },
            new() { Code = "PasswordRequiresNonAlphanumeric", Description = "Password requires special character" }
        };

        _mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error!.Message.Should().Contain("Password is too short");
        result.Error.Message.Should().Contain("Password requires special character");

        _mockPasswordService.Verify(x => x.TrackPasswordChangeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}