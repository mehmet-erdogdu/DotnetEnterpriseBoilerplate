using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.DTOs;

public class LoginDtoValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly LoginDtoValidator _validator;

    public LoginDtoValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new LoginDtoValidator(_mockMessageService.Object);
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var model = new LoginDto { Email = "", Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailRequired");
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Email_Is_Null()
    {
        // Arrange
        var model = new LoginDto { Email = null!, Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailRequired");
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Email_Is_Invalid_Format()
    {
        // Arrange
        var model = new LoginDto { Email = "invalid-email", Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailInvalid");
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Email_Exceeds_Max_Length()
    {
        // Arrange
        var longEmail = new string('a', 101) + "@example.com"; // 115 characters
        var model = new LoginDto { Email = longEmail, Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailMax");
    }

    [Fact]
    public void LoginDtoValidator_Should_Not_Have_Error_When_Email_Is_Valid()
    {
        // Arrange
        var model = new LoginDto { Email = "valid@example.com", Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var model = new LoginDto { Email = "valid@example.com", Password = "" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Password_Is_Null()
    {
        // Arrange
        var model = new LoginDto { Email = "valid@example.com", Password = null! };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var model = new LoginDto { Email = "valid@example.com", Password = "Short1!" }; // Less than 12 characters

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void LoginDtoValidator_Should_Have_Error_When_Password_Exceeds_Max_Length()
    {
        // Arrange
        var longPassword = new string('a', 129); // 129 characters
        var model = new LoginDto { Email = "valid@example.com", Password = longPassword };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void LoginDtoValidator_Should_Not_Have_Error_When_Password_Is_Valid()
    {
        // Arrange
        var model = new LoginDto { Email = "valid@example.com", Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void LoginDtoValidator_Should_Not_Have_Error_When_Both_Email_And_Password_Are_Valid()
    {
        // Arrange
        var model = new LoginDto { Email = "valid@example.com", Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}