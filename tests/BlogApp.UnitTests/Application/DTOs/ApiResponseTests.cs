using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class ApiResponseTests
{
    [Fact]
    public void ApiResponse_Success_Should_Create_Successful_Response()
    {
        // Arrange
        var testData = "test data";

        // Act
        var response = ApiResponse<string>.Success(testData);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess);
        Assert.Equal(testData, response.Data);
        Assert.Null(response.Error);
    }

    [Fact]
    public void ApiResponse_Failure_Should_Create_Failed_Response()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var response = ApiResponse<string>.Failure(errorMessage);

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotNull(response.Error);
        Assert.Equal(errorMessage, response.Error.Message);
        Assert.Null(response.Error.Code);
        Assert.Null(response.Error.Details);
    }

    [Fact]
    public void ApiResponse_Failure_With_Code_And_Details_Should_Create_Failed_Response()
    {
        // Arrange
        var errorMessage = "Test error message";
        var errorCode = "TEST_ERROR";
        var errorDetails = new { Property = "Value" };

        // Act
        var response = new ApiResponse<string>
        {
            IsSuccess = false,
            Error = new ErrorResponse(errorMessage, errorCode, errorDetails)
        };

        // Assert
        Assert.NotNull(response);
        Assert.False(response.IsSuccess);
        Assert.Null(response.Data);
        Assert.NotNull(response.Error);
        Assert.Equal(errorMessage, response.Error.Message);
        Assert.Equal(errorCode, response.Error.Code);
        Assert.Equal(errorDetails, response.Error.Details);
    }

    [Fact]
    public void ErrorResponse_Constructor_Should_Set_Properties()
    {
        // Arrange
        var errorMessage = "Test error message";
        var errorCode = "TEST_ERROR";
        var errorDetails = new { Property = "Value" };

        // Act
        var error = new ErrorResponse(errorMessage, errorCode, errorDetails);

        // Assert
        Assert.Equal(errorMessage, error.Message);
        Assert.Equal(errorCode, error.Code);
        Assert.Equal(errorDetails, error.Details);
    }

    [Fact]
    public void ErrorResponse_Constructor_With_Only_Message_Should_Set_Properties()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var error = new ErrorResponse(errorMessage);

        // Assert
        Assert.Equal(errorMessage, error.Message);
        Assert.Null(error.Code);
        Assert.Null(error.Details);
    }
}