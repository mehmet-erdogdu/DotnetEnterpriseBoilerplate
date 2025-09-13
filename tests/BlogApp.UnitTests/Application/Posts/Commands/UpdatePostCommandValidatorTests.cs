using FluentValidation.TestHelper;
using Moq;
using BlogApp.Application.Posts.Commands;
using BlogApp.Application.Services;

namespace BlogApp.UnitTests.Application.Posts.Commands;

public class UpdatePostCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly UpdatePostCommandValidator _validator;

    public UpdatePostCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new UpdatePostCommandValidator(_mockMessageService.Object);
    }

    #region Id Tests

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.Empty,
            Title = "Valid Title",
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Error: IdRequired");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Not_Have_Error_When_Id_Is_Valid()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    #endregion

    #region Title Tests

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "",
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Title_Is_Null()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = null!,
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "AB", // Less than 3 characters
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleLength");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Title_Exceeds_Max_Length()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = longTitle,
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleLength");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Title_Contains_Invalid_Characters()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Invalid Title @#$%", // Contains special characters not in the allowed pattern
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleInvalid");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Not_Have_Error_When_Title_Is_Valid()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Not_Have_Error_When_Title_Contains_Allowed_Special_Characters()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title with -_.,!?() and Turkish chars ğüşıöçĞÜŞİÖÇ0-9",
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Content Tests

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Content_Is_Empty()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Error: ContentRequired");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Content_Is_Null()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = null!
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Error: ContentRequired");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Content_Is_Too_Short()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = "Short" // Less than 10 characters
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Error: ContentLength");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Error_When_Content_Exceeds_Max_Length()
    {
        // Arrange
        var longContent = new string('A', 10001); // 10001 characters
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = longContent
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Error: ContentLength");
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Not_Have_Error_When_Content_Is_Valid()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = "This is valid content with more than 10 characters"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Content);
    }

    #endregion

    #region Combined Tests

    [Fact]
    public void UpdatePostCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Content = "This is valid content with more than 10 characters"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdatePostCommandValidator_Should_Have_Errors_When_Multiple_Fields_Are_Invalid()
    {
        // Arrange
        var model = new UpdatePostCommand
        {
            Id = Guid.Empty, // Empty ID
            Title = "", // Empty title
            Content = "Short" // Too short content
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Error: IdRequired");
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Error: ContentLength");
    }

    #endregion
}