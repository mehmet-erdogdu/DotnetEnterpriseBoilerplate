using FluentValidation.TestHelper;
using BlogApp.Application.DTOs;

namespace BlogApp.UnitTests.Application.DTOs;

public class RegisterDtoValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly RegisterDtoValidator _validator;

    public RegisterDtoValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new RegisterDtoValidator(_mockMessageService.Object);
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Email_Is_Null()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = null!,
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Email_Exceeds_Max_Length()
    {
        // Arrange
        var longEmail = new string('a', 101) + "@example.com"; // 115 characters
        var model = new RegisterDto
        {
            Email = longEmail,
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailMax");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Email_Is_Invalid_Format()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "invalid-email",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Error: EmailInvalid");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Not_Have_Error_When_Email_Is_Valid()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Is_Null()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = null!,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Is_Too_Short()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "Short1!", // Less than 12 characters
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Exceeds_Max_Length()
    {
        // Arrange
        var longPassword = new string('a', 129); // 129 characters
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = longPassword,
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordLength");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Lacks_Complexity_Lowercase()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "NOPASSWORD123!", // Missing lowercase
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Lacks_Complexity_Uppercase()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "nopassword123!", // Missing uppercase
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Lacks_Complexity_Digit()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "NoPassword!", // Missing digit
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_Password_Lacks_Complexity_SpecialChar()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "NoPassword123", // Missing special character
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Error: PasswordComplexity");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Not_Have_Error_When_Password_Is_Valid()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_FirstName_Is_Empty()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("Error: FirstNameRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_FirstName_Is_Null()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = null!,
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("Error: FirstNameRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_FirstName_Is_Too_Short()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "J", // Less than 2 characters
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("Error: FirstNameLength");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_FirstName_Exceeds_Max_Length()
    {
        // Arrange
        var longFirstName = new string('a', 51); // 51 characters
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = longFirstName,
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("Error: FirstNameLength");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_FirstName_Contains_Invalid_Characters()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John123", // Contains numbers
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("Error: FirstNameInvalid");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Not_Have_Error_When_FirstName_Is_Valid()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_LastName_Is_Empty()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Error: LastNameRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_LastName_Is_Null()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = null!
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Error: LastNameRequired");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_LastName_Is_Too_Short()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "D" // Less than 2 characters
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Error: LastNameLength");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_LastName_Exceeds_Max_Length()
    {
        // Arrange
        var longLastName = new string('a', 51); // 51 characters
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = longLastName
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Error: LastNameLength");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Have_Error_When_LastName_Contains_Invalid_Characters()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe123" // Contains numbers
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("Error: LastNameInvalid");
    }

    [Fact]
    public void RegisterDtoValidator_Should_Not_Have_Error_When_LastName_Is_Valid()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void RegisterDtoValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new RegisterDto
        {
            Email = "valid@example.com",
            Password = "ValidPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}