namespace BlogApp.UnitTests.Application.Auth.Commands;

public class ChangePasswordCommandHandlerTests : BaseTestClass
{
    private readonly ChangePasswordCommandHandler _handler;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;

    public ChangePasswordCommandHandlerTests()
    {
        _mockUserManager = TestHelper.MockSetups.CreateMockUserManager();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        _mockMessageService = new Mock<IMessageService>();

        _handler = new ChangePasswordCommandHandler(
            _mockUserManager.Object,
            _mockPasswordService.Object,
            _mockRefreshTokenService.Object,
            _mockMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidPasswordChange_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = "test-user-id",
            CurrentPassword = "Current@123",
            NewPassword = "NewPassword@456"
        };

        var user = TestHelper.TestData.CreateTestUser(command.UserId);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.CurrentPassword))
            .ReturnsAsync(true);

        _mockPasswordService.Setup(x => x.IsPasswordRecentlyUsedAsync(command.UserId, It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Success);

        _mockPasswordService.Setup(x => x.TrackPasswordChangeAsync(command.UserId, It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _mockRefreshTokenService.Setup(x => x.RevokeAllUserTokensAsync(command.UserId, command.UserId, "Password changed"))
            .Returns(Task.CompletedTask);

        _mockMessageService.Setup(x => x.GetMessage("PasswordChangedSuccessfully"))
            .Returns("Password changed successfully");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().Be("Password changed successfully");

        _mockUserManager.Verify(x => x.FindByIdAsync(command.UserId), Times.Once);
        _mockUserManager.Verify(x => x.CheckPasswordAsync(user, command.CurrentPassword), Times.Once);
        _mockPasswordService.Verify(x => x.IsPasswordRecentlyUsedAsync(command.UserId, It.IsAny<string>()), Times.Once);
        _mockUserManager.Verify(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword), Times.Once);
        _mockPasswordService.Verify(x => x.TrackPasswordChangeAsync(command.UserId, It.IsAny<string>()), Times.Once);
        _mockRefreshTokenService.Verify(x => x.RevokeAllUserTokensAsync(command.UserId, command.UserId, "Password changed"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = "non-existent-user",
            CurrentPassword = "Current@123",
            NewPassword = "NewPassword@456"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync((ApplicationUser?)null);

        _mockMessageService.Setup(x => x.GetMessage("UserNotFound"))
            .Returns("User not found");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "User not found");

        _mockUserManager.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        _mockPasswordService.Verify(x => x.IsPasswordRecentlyUsedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncorrectCurrentPassword_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = "test-user-id",
            CurrentPassword = "WrongPassword@123",
            NewPassword = "NewPassword@456"
        };

        var user = TestHelper.TestData.CreateTestUser(command.UserId);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.CurrentPassword))
            .ReturnsAsync(false);

        _mockMessageService.Setup(x => x.GetMessage("CurrentPasswordIncorrect"))
            .Returns("Current password is incorrect");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Current password is incorrect");

        _mockPasswordService.Verify(x => x.IsPasswordRecentlyUsedAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockUserManager.Verify(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithRecentlyUsedPassword_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = "test-user-id",
            CurrentPassword = "Current@123",
            NewPassword = "RecentlyUsed@456"
        };

        var user = TestHelper.TestData.CreateTestUser(command.UserId);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.CurrentPassword))
            .ReturnsAsync(true);

        _mockPasswordService.Setup(x => x.IsPasswordRecentlyUsedAsync(command.UserId, It.IsAny<string>()))
            .ReturnsAsync(true);

        _mockMessageService.Setup(x => x.GetMessage("PasswordRecentlyUsed"))
            .Returns("Password was recently used");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Password was recently used");

        _mockUserManager.Verify(x => x.ChangePasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockPasswordService.Verify(x => x.TrackPasswordChangeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithFailedPasswordChange_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new ChangePasswordCommand
        {
            UserId = "test-user-id",
            CurrentPassword = "Current@123",
            NewPassword = "weak"
        };

        var user = TestHelper.TestData.CreateTestUser(command.UserId);

        _mockUserManager.Setup(x => x.FindByIdAsync(command.UserId))
            .ReturnsAsync(user);

        _mockUserManager.Setup(x => x.CheckPasswordAsync(user, command.CurrentPassword))
            .ReturnsAsync(true);

        _mockPasswordService.Setup(x => x.IsPasswordRecentlyUsedAsync(command.UserId, It.IsAny<string>()))
            .ReturnsAsync(false);

        var identityErrors = new List<IdentityError>
        {
            new() { Code = "PasswordTooShort", Description = "Password is too short" }
        };

        _mockUserManager.Setup(x => x.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword))
            .ReturnsAsync(IdentityResult.Failed(identityErrors.ToArray()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result);
        result.Error!.Message.Should().Contain("Password is too short");

        _mockPasswordService.Verify(x => x.TrackPasswordChangeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockRefreshTokenService.Verify(x => x.RevokeAllUserTokensAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}