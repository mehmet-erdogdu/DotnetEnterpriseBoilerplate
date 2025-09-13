using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.Auth;

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
            UserId = "user123"
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
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage("Error: CurrentPasswordRequired");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Not_Have_Error_When_CurrentPassword_Is_Valid()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Is_Empty()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "",
            UserId = "user123"
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
            UserId = "user123"
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
            UserId = "user123"
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
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_Lowercase()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NOPASSWORD123!", // Missing lowercase
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_Uppercase()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "nopassword123!", // Missing uppercase
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_Digit()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NoPassword!", // Missing digit
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_SpecialChar()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NoPassword123", // Missing special character
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Not_Have_Error_When_NewPassword_Is_Valid()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
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
    public void ChangePasswordCommandValidator_Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void ChangePasswordCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new ChangePasswordCommand
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            UserId = "user123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}