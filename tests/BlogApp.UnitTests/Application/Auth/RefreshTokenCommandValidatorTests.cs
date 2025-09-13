using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.Auth;

public class RefreshTokenCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly RefreshTokenCommandValidator _validator;

    public RefreshTokenCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new RefreshTokenCommandValidator(_mockMessageService.Object);
    }

    [Fact]
    public void RefreshTokenCommandValidator_Should_Have_Error_When_RefreshToken_Is_Empty()
    {
        // Arrange
        var model = new RefreshTokenCommand
        {
            RefreshToken = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Error: RefreshTokenRequired");
    }

    [Fact]
    public void RefreshTokenCommandValidator_Should_Have_Error_When_RefreshToken_Is_Null()
    {
        // Arrange
        var model = new RefreshTokenCommand
        {
            RefreshToken = null!
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Error: RefreshTokenRequired");
    }

    [Fact]
    public void RefreshTokenCommandValidator_Should_Have_Error_When_RefreshToken_Exceeds_Max_Length()
    {
        // Arrange
        var longRefreshToken = new string('a', 513); // 513 characters
        var model = new RefreshTokenCommand
        {
            RefreshToken = longRefreshToken
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken)
            .WithErrorMessage("Error: RefreshTokenMax");
    }

    [Fact]
    public void RefreshTokenCommandValidator_Should_Not_Have_Error_When_RefreshToken_Is_Valid()
    {
        // Arrange
        var model = new RefreshTokenCommand
        {
            RefreshToken = "valid-refresh-token"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }

    [Fact]
    public void RefreshTokenCommandValidator_Should_Not_Have_Error_When_RefreshToken_Is_Valid_And_At_Max_Length()
    {
        // Arrange
        var maxRefreshToken = new string('a', 512); // 512 characters (max)
        var model = new RefreshTokenCommand
        {
            RefreshToken = maxRefreshToken
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RefreshToken);
    }
}