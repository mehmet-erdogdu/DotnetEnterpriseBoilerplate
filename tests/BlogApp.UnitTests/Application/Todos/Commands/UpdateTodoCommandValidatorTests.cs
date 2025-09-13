using FluentValidation.TestHelper;
using Moq;
using BlogApp.Application.Todos.Commands;
using BlogApp.Application.Services;

namespace BlogApp.UnitTests.Application.Todos.Commands;

public class UpdateTodoCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly UpdateTodoCommandValidator _validator;

    public UpdateTodoCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new UpdateTodoCommandValidator(_mockMessageService.Object);
    }

    #region Id Tests

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.Empty,
            Title = "Valid Title",
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Error: IdRequired");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Id_Is_Valid()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    #endregion

    #region Title Tests

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "",
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Title_Is_Null()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = null!,
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "AB", // Less than 3 characters
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleLength");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Title_Exceeds_Max_Length()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = longTitle,
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleLength");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Title_Contains_Invalid_Characters()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Invalid Title @#$%", // Contains special characters not in the allowed pattern
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleInvalid");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Title_Is_Valid()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Title_Contains_Allowed_Special_Characters()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title with -_.,!?() and Turkish chars ğüşıöçĞÜŞİÖÇ0-9",
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Description Tests

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_Null()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = null
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_Empty()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Error_When_Description_Exceeds_Max_Length()
    {
        // Arrange
        var longDescription = new string('A', 1001); // 1001 characters
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = longDescription
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Error: DescriptionMax");
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_Valid()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "This is a valid description with less than 1000 characters"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_At_Max_Length()
    {
        // Arrange
        var maxDescription = new string('A', 1000); // 1000 characters
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = maxDescription
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region Combined Tests

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid description for the todo"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Not_Have_Error_When_All_Required_Fields_Are_Valid_And_Description_Is_Null()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = null
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void UpdateTodoCommandValidator_Should_Have_Errors_When_Multiple_Fields_Are_Invalid()
    {
        // Arrange
        var model = new UpdateTodoCommand
        {
            Id = Guid.Empty, // Empty ID
            Title = "", // Empty title
            Description = new string('A', 1001) // Too long description
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Error: IdRequired");
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Error: DescriptionMax");
    }

    #endregion
}