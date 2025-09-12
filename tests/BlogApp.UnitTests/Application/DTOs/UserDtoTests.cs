namespace BlogApp.UnitTests.Application.DTOs;

public class UserDtoTests
{
    [Fact]
    public void UserDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = "test-id";
        var email = "test@example.com";
        var firstName = "Test";
        var lastName = "User";
        var emailConfirmed = true;
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);
        var roles = new List<string> { "User", "Admin" };

        // Act
        var userDto = new UserDto
        {
            Id = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = emailConfirmed,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Roles = roles
        };

        // Assert
        Assert.Equal(id, userDto.Id);
        Assert.Equal(email, userDto.Email);
        Assert.Equal(firstName, userDto.FirstName);
        Assert.Equal(lastName, userDto.LastName);
        Assert.Equal(emailConfirmed, userDto.EmailConfirmed);
        Assert.Equal(createdAt, userDto.CreatedAt);
        Assert.Equal(updatedAt, userDto.UpdatedAt);
        Assert.Equal(roles, userDto.Roles);
    }

    [Fact]
    public void UserDto_Should_Have_Default_Values()
    {
        // Act
        var userDto = new UserDto();

        // Assert
        Assert.Equal(string.Empty, userDto.Id);
        Assert.Equal(string.Empty, userDto.Email);
        Assert.Equal(string.Empty, userDto.FirstName);
        Assert.Equal(string.Empty, userDto.LastName);
        Assert.False(userDto.EmailConfirmed);
        Assert.Equal(default, userDto.CreatedAt);
        Assert.Null(userDto.UpdatedAt);
        Assert.NotNull(userDto.Roles);
        Assert.Empty(userDto.Roles);
    }

    [Fact]
    public void UserDto_Roles_Should_Be_Modifiable()
    {
        // Arrange
        var userDto = new UserDto();
        var roles = new List<string> { "User" };

        // Act
        userDto.Roles = roles;

        // Assert
        Assert.Same(roles, userDto.Roles);
    }
}