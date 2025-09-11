namespace BlogApp.UnitTests.Domain.Entities;

public class ApplicationUserTests : BaseTestClass
{
    [Fact]
    public void ApplicationUser_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = "test-user-id";
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@example.com";
        var userName = "johndoe";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var user = new ApplicationUser
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            UserName = userName,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        user.Id.Should().Be(id);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.UserName.Should().Be(userName);
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(updatedAt);
        user.IsDeleted.Should().BeFalse(); // Default value
        user.DeletedAt.Should().BeNull(); // Default value
        user.DeletedById.Should().BeNull(); // Default value
    }

    [Fact]
    public void ApplicationUser_WithRequiredProperties_ShouldBeValid()
    {
        // Act
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Posts.Should().NotBeNull().And.BeEmpty(); // Default collection
        user.Todos.Should().NotBeNull().And.BeEmpty(); // Default collection
        user.PasswordHistory.Should().NotBeNull().And.BeEmpty(); // Default collection
        user.RefreshTokens.Should().NotBeNull().And.BeEmpty(); // Default collection
        user.IsDeleted.Should().BeFalse(); // Default value
        user.DeletedAt.Should().BeNull(); // Default value
        user.DeletedById.Should().BeNull(); // Default value
    }

    [Fact]
    public void ApplicationUser_WhenMarkedAsDeleted_ShouldSetDeletedProperties()
    {
        // Arrange
        var user = new ApplicationUser
        {
            FirstName = "John",
            LastName = "Doe"
        };
        var deletedAt = DateTime.UtcNow;
        var deletedById = "admin-user-id";

        // Act
        user.IsDeleted = true;
        user.DeletedAt = deletedAt;
        user.DeletedById = deletedById;

        // Assert
        user.IsDeleted.Should().BeTrue();
        user.DeletedAt.Should().Be(deletedAt);
        user.DeletedById.Should().Be(deletedById);
    }
}