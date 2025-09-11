namespace BlogApp.UnitTests.Domain.Entities;

public class TodoTests : BaseTestClass
{
    [Fact]
    public void Todo_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Todo";
        var description = "Test Description";
        var userId = "test-user-id";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var todo = new Todo
        {
            Id = id,
            Title = title,
            Description = description,
            UserId = userId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        todo.Id.Should().Be(id);
        todo.Title.Should().Be(title);
        todo.Description.Should().Be(description);
        todo.UserId.Should().Be(userId);
        todo.CreatedAt.Should().Be(createdAt);
        todo.UpdatedAt.Should().Be(updatedAt);
        todo.IsCompleted.Should().BeFalse(); // Default value
        todo.CompletedAt.Should().BeNull(); // Default value
    }

    [Fact]
    public void Todo_WithRequiredProperties_ShouldBeValid()
    {
        // Act
        var todo = new Todo
        {
            Title = "Test Todo",
            UserId = "test-user-id"
        };

        // Assert
        todo.Title.Should().Be("Test Todo");
        todo.UserId.Should().Be("test-user-id");
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Todo_WhenMarkedAsCompleted_ShouldSetCompletedAt()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test Todo",
            UserId = "test-user-id"
        };
        var completionTime = DateTime.UtcNow;

        // Act
        todo.IsCompleted = true;
        todo.CompletedAt = completionTime;

        // Assert
        todo.IsCompleted.Should().BeTrue();
        todo.CompletedAt.Should().Be(completionTime);
    }

    [Fact]
    public void Todo_WhenUnmarkedAsCompleted_ShouldClearCompletedAt()
    {
        // Arrange
        var todo = new Todo
        {
            Title = "Test Todo",
            UserId = "test-user-id",
            IsCompleted = true,
            CompletedAt = DateTime.UtcNow
        };

        // Act
        todo.IsCompleted = false;
        todo.CompletedAt = null;

        // Assert
        todo.IsCompleted.Should().BeFalse();
        todo.CompletedAt.Should().BeNull();
    }
}