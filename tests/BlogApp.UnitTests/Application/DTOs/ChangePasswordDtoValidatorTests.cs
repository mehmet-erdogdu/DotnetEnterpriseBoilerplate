using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.DTOs;

public class ChangePasswordDtoValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly ChangePasswordDtoValidator _validator;

    public ChangePasswordDtoValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new ChangePasswordDtoValidator(_mockMessageService.Object);
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_CurrentPassword_Is_Empty()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "ValidPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage("Error: CurrentPasswordRequired");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_CurrentPassword_Is_Null()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = null!,
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "ValidPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword)
            .WithErrorMessage("Error: CurrentPasswordRequired");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Not_Have_Error_When_CurrentPassword_Is_Valid()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "ValidPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Is_Empty()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "",
            ConfirmNewPassword = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Is_Null()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = null!,
            ConfirmNewPassword = null!
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Is_Too_Short()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "Short1!", // Less than 12 characters
            ConfirmNewPassword = "Short1!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Exceeds_Max_Length()
    {
        // Arrange
        var longPassword = new string('a', 129); // 129 characters
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = longPassword,
            ConfirmNewPassword = longPassword
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_Lowercase()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NOPASSWORD123!", // Missing lowercase
            ConfirmNewPassword = "NOPASSWORD123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_Uppercase()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "nopassword123!", // Missing uppercase
            ConfirmNewPassword = "nopassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_Digit()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NoPassword!", // Missing digit
            ConfirmNewPassword = "NoPassword!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_NewPassword_Lacks_Complexity_SpecialChar()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NoPassword123", // Missing special character
            ConfirmNewPassword = "NoPassword123"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewPassword)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Not_Have_Error_When_NewPassword_Is_Valid()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "ValidPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Have_Error_When_ConfirmNewPassword_Does_Not_Match_NewPassword()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "DifferentPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmNewPassword)
            .WithErrorMessage("Error: PasswordsDoNotMatch");
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Not_Have_Error_When_ConfirmNewPassword_Matches_NewPassword()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "ValidPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ConfirmNewPassword);
    }

    [Fact]
    public void ChangePasswordDtoValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "ValidPassword123!",
            ConfirmNewPassword = "ValidPassword123!"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}