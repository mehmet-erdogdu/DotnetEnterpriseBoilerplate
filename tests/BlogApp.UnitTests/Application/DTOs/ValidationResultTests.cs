using ValidationResult = BlogApp.Application.DTOs.ValidationResult;

namespace BlogApp.UnitTests.Application.DTOs;

public class ValidationResultTests
{
    [Fact]
    public void ValidationResult_Should_Have_Default_Values()
    {
        // Act
        var validationResult = new ValidationResult();

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.NotNull(validationResult.Errors);
        Assert.Empty(validationResult.Errors);
        Assert.NotNull(validationResult.Warnings);
        Assert.Empty(validationResult.Warnings);
    }

    [Fact]
    public void ValidationResult_Success_Should_Create_Successful_Result()
    {
        // Act
        var validationResult = ValidationResult.Success();

        // Assert
        Assert.True(validationResult.IsValid);
        Assert.NotNull(validationResult.Errors);
        Assert.Empty(validationResult.Errors);
        Assert.NotNull(validationResult.Warnings);
        Assert.Empty(validationResult.Warnings);
    }

    [Fact]
    public void ValidationResult_Failure_With_Params_Should_Create_Failed_Result()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };

        // Act
        var validationResult = ValidationResult.Failure(errors);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.NotNull(validationResult.Errors);
        Assert.Equal(errors, validationResult.Errors);
        Assert.NotNull(validationResult.Warnings);
        Assert.Empty(validationResult.Warnings);
    }

    [Fact]
    public void ValidationResult_Failure_With_Enumerable_Should_Create_Failed_Result()
    {
        // Arrange
        var errors = new List<string> { "Error 1", "Error 2" };

        // Act
        var validationResult = ValidationResult.Failure(errors);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.NotNull(validationResult.Errors);
        Assert.Equal(errors, validationResult.Errors);
        Assert.NotNull(validationResult.Warnings);
        Assert.Empty(validationResult.Warnings);
    }

    [Fact]
    public void ValidationResult_AddError_Should_Add_Error_And_Make_Invalid()
    {
        // Arrange
        var validationResult = ValidationResult.Success();
        var error = "Test error";

        // Act
        var result = validationResult.AddError(error);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(error, result.Errors);
        Assert.Single(result.Errors);
        Assert.NotNull(result.Warnings);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void ValidationResult_Combine_Should_Combine_Two_Results()
    {
        // Arrange
        var result1 = ValidationResult.Failure("Error 1");
        result1 = result1 with { Warnings = new List<string> { "Warning 1" } };

        var result2 = ValidationResult.Failure("Error 2");
        result2 = result2 with { Warnings = new List<string> { "Warning 2" } };

        // Act
        var combinedResult = result1.Combine(result2);

        // Assert
        Assert.False(combinedResult.IsValid);
        Assert.NotNull(combinedResult.Errors);
        Assert.Contains("Error 1", combinedResult.Errors);
        Assert.Contains("Error 2", combinedResult.Errors);
        Assert.Equal(2, combinedResult.Errors.Count);
        Assert.NotNull(combinedResult.Warnings);
        Assert.Contains("Warning 1", combinedResult.Warnings);
        Assert.Contains("Warning 2", combinedResult.Warnings);
        Assert.Equal(2, combinedResult.Warnings.Count);
    }

    [Fact]
    public void ValidationResult_Combine_With_Success_Result_Should_Preserve_Valid_Status()
    {
        // Arrange
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        // Act
        var combinedResult = result1.Combine(result2);

        // Assert
        Assert.True(combinedResult.IsValid);
        Assert.NotNull(combinedResult.Errors);
        Assert.Empty(combinedResult.Errors);
        Assert.NotNull(combinedResult.Warnings);
        Assert.Empty(combinedResult.Warnings);
    }

    [Fact]
    public void ValidationResult_Combine_With_One_Failure_Should_Make_Combined_Result_Invalid()
    {
        // Arrange
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Failure("Error");

        // Act
        var combinedResult = result1.Combine(result2);

        // Assert
        Assert.False(combinedResult.IsValid);
        Assert.NotNull(combinedResult.Errors);
        Assert.Contains("Error", combinedResult.Errors);
        Assert.Single(combinedResult.Errors);
        Assert.NotNull(combinedResult.Warnings);
        Assert.Empty(combinedResult.Warnings);
    }
}