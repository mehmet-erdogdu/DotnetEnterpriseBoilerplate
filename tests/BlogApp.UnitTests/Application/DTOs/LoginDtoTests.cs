using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class LoginDtoTests
{
    [Fact]
    public void LoginDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var email = "test@example.com";
        var password = "testpassword123";

        // Act
        var loginDto = new LoginDto
        {
            Email = email,
            Password = password
        };

        // Assert
        Assert.Equal(email, loginDto.Email);
        Assert.Equal(password, loginDto.Password);
    }

    [Fact]
    public void LoginDto_Should_Allow_Empty_Values()
    {
        // Act
        var loginDto = new LoginDto
        {
            Email = string.Empty,
            Password = string.Empty
        };

        // Assert
        Assert.Equal(string.Empty, loginDto.Email);
        Assert.Equal(string.Empty, loginDto.Password);
    }

    [Fact]
    public void LoginDto_Should_Allow_Null_Values()
    {
        // Act
        var loginDto = new LoginDto
        {
            Email = null!,
            Password = null!
        };

        // Assert
        Assert.Null(loginDto.Email);
        Assert.Null(loginDto.Password);
    }
}