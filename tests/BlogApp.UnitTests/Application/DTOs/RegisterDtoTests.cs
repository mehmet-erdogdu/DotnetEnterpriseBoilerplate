using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class RegisterDtoTests
{
    [Fact]
    public void RegisterDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "TestPassword123!";
        var firstName = "Test";
        var lastName = "User";

        // Act
        var registerDto = new RegisterDto
        {
            Email = email,
            Password = password,
            FirstName = firstName,
            LastName = lastName
        };

        // Assert
        Assert.Equal(email, registerDto.Email);
        Assert.Equal(password, registerDto.Password);
        Assert.Equal(firstName, registerDto.FirstName);
        Assert.Equal(lastName, registerDto.LastName);
    }

    [Fact]
    public void RegisterDto_Should_Allow_Empty_Values()
    {
        // Act
        var registerDto = new RegisterDto
        {
            Email = string.Empty,
            Password = string.Empty,
            FirstName = string.Empty,
            LastName = string.Empty
        };

        // Assert
        Assert.Equal(string.Empty, registerDto.Email);
        Assert.Equal(string.Empty, registerDto.Password);
        Assert.Equal(string.Empty, registerDto.FirstName);
        Assert.Equal(string.Empty, registerDto.LastName);
    }

    [Fact]
    public void RegisterDto_Should_Allow_Null_Values()
    {
        // Act
        var registerDto = new RegisterDto
        {
            Email = null!,
            Password = null!,
            FirstName = null!,
            LastName = null!
        };

        // Assert
        Assert.Null(registerDto.Email);
        Assert.Null(registerDto.Password);
        Assert.Null(registerDto.FirstName);
        Assert.Null(registerDto.LastName);
    }
}