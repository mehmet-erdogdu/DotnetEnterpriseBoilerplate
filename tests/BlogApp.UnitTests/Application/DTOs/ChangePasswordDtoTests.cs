using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class ChangePasswordDtoTests
{
    [Fact]
    public void ChangePasswordDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var currentPassword = "currentPassword123!";
        var newPassword = "newPassword123!";
        var confirmNewPassword = "newPassword123!";

        // Act
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmNewPassword
        };

        // Assert
        Assert.Equal(currentPassword, changePasswordDto.CurrentPassword);
        Assert.Equal(newPassword, changePasswordDto.NewPassword);
        Assert.Equal(confirmNewPassword, changePasswordDto.ConfirmNewPassword);
    }

    [Fact]
    public void ChangePasswordDto_Should_Have_Default_Values()
    {
        // Act
        var changePasswordDto = new ChangePasswordDto();

        // Assert
        Assert.Equal(string.Empty, changePasswordDto.CurrentPassword);
        Assert.Equal(string.Empty, changePasswordDto.NewPassword);
        Assert.Equal(string.Empty, changePasswordDto.ConfirmNewPassword);
    }

    [Fact]
    public void ChangePasswordDto_Should_Allow_Empty_Values()
    {
        // Act
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = string.Empty,
            NewPassword = string.Empty,
            ConfirmNewPassword = string.Empty
        };

        // Assert
        Assert.Equal(string.Empty, changePasswordDto.CurrentPassword);
        Assert.Equal(string.Empty, changePasswordDto.NewPassword);
        Assert.Equal(string.Empty, changePasswordDto.ConfirmNewPassword);
    }
}