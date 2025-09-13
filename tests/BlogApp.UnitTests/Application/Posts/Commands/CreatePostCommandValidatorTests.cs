using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.Posts.Commands;

public class CreatePostCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly CreatePostCommandValidator _validator;

    public CreatePostCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new CreatePostCommandValidator(_mockMessageService.Object);
    }

    #region Title Tests

    [Fact]
    public void CreatePostCommandValidator_Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Title_Is_Null()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Title_Exceeds_Max_Length()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Title_Contains_Invalid_Characters()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Not_Have_Error_When_Title_Is_Valid()
    {
        // Arrange
        var model = new CreatePostCommand
        {
            Title = "Valid Title",
            Content = "Valid content for the post"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreatePostCommandValidator_Should_Not_Have_Error_When_Title_Contains_Allowed_Special_Characters()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Content_Is_Empty()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Content_Is_Null()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Content_Is_Too_Short()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Have_Error_When_Content_Exceeds_Max_Length()
    {
        // Arrange
        var longContent = new string('A', 10001); // 10001 characters
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Not_Have_Error_When_Content_Is_Valid()
    {
        // Arrange
        var model = new CreatePostCommand
        {
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
    public void CreatePostCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new CreatePostCommand
        {
            Title = "Valid Title",
            Content = "This is valid content with more than 10 characters"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreatePostCommandValidator_Should_Have_Errors_When_Multiple_Fields_Are_Invalid()
    {
        // Arrange
        var model = new CreatePostCommand
        {
            Title = "", // Empty title
            Content = "Short" // Too short content
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Error: ContentLength");
    }

    #endregion
}