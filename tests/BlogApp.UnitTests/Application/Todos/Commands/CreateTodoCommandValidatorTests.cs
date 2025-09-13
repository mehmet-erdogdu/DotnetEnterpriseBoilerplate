using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.Todos.Commands;

public class CreateTodoCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly CreateTodoCommandValidator _validator;

    public CreateTodoCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new CreateTodoCommandValidator(_mockMessageService.Object);
    }

    #region Title Tests

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "",
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_Title_Is_Null()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = null!,
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_Title_Is_Too_Short()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "AB", // Less than 3 characters
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleLength");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_Title_Exceeds_Max_Length()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var model = new CreateTodoCommand
        {
            Title = longTitle,
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleLength");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_Title_Contains_Invalid_Characters()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Invalid Title @#$%", // Contains special characters not in the allowed pattern
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleInvalid");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_Title_Is_Valid()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_Title_Contains_Allowed_Special_Characters()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title with -_.,!?() and Turkish chars ğüşıöçĞÜŞİÖÇ0-9",
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Description Tests

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_Null()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = null,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_Empty()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_Description_Exceeds_Max_Length()
    {
        // Arrange
        var longDescription = new string('A', 1001); // 1001 characters
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = longDescription,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Error: DescriptionMax");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_Valid()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "This is a valid description with less than 1000 characters",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_Description_Is_At_Max_Length()
    {
        // Arrange
        var maxDescription = new string('A', 1000); // 1000 characters
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = maxDescription,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    #endregion

    #region UserId Tests

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "Valid description for the todo",
            UserId = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Error_When_UserId_Is_Null()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "Valid description for the todo",
            UserId = null!
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    #endregion

    #region Combined Tests

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = "Valid description for the todo",
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Not_Have_Error_When_All_Required_Fields_Are_Valid_And_Description_Is_Null()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "Valid Title",
            Description = null,
            UserId = "valid-user-id"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateTodoCommandValidator_Should_Have_Errors_When_Multiple_Fields_Are_Invalid()
    {
        // Arrange
        var model = new CreateTodoCommand
        {
            Title = "", // Empty title
            Description = new string('A', 1001), // Too long description
            UserId = "" // Empty user ID
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Error: TitleRequired");
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Error: DescriptionMax");
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    #endregion
}