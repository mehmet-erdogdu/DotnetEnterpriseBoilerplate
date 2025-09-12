using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.Auth.Commands;

public class ChangePasswordCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly ChangePasswordCommandValidator _validator;

    public ChangePasswordCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new ChangePasswordCommandValidator(_mockMessageService.Object);
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_CurrentPassword_Is_Empty()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "",
            NewPassword = "ValidPassword123!",
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage("Error: CurrentPasswordRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_CurrentPassword_Is_Null()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = null!,
            NewPassword = "ValidPassword123!",
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage("Error: CurrentPasswordRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Is_Empty()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "",
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Is_Null()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = null!,
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Is_Too_Short()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "Short1!", // Less than 12 characters
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Exceeds_Max_Length()
    {
        // Arrange
        var longPassword = new string('a', 129); // 129 characters
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = longPassword,
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Does_Not_Match_Complexity_Requirements()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "simplepassword", // Missing uppercase, number, and special character
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_UserId_Is_Null()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = null!
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = "test-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}