namespace BlogApp.UnitTests.Application.DTOs;

public class TodoDtoTests
{
    [Fact]
    public void TodoDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Todo";
        var description = "This is a test todo description";
        var isCompleted = false;
        var createdAt = DateTime.UtcNow;
        var completedAt = DateTime.UtcNow.AddHours(1);
        var userId = "test-user-id";
        var userName = "Test User";

        // Act
        var todoDto = new TodoDto
        {
            Id = id,
            Title = title,
            Description = description,
            IsCompleted = isCompleted,
            CreatedAt = createdAt,
            CompletedAt = completedAt,
            UserId = userId,
            UserName = userName
        };

        // Assert
        Assert.Equal(id, todoDto.Id);
        Assert.Equal(title, todoDto.Title);
        Assert.Equal(description, todoDto.Description);
        Assert.Equal(isCompleted, todoDto.IsCompleted);
        Assert.Equal(createdAt, todoDto.CreatedAt);
        Assert.Equal(completedAt, todoDto.CompletedAt);
        Assert.Equal(userId, todoDto.UserId);
        Assert.Equal(userName, todoDto.UserName);
    }

    [Fact]
    public void TodoDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var todoDto = new TodoDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Todo",
            Description = null,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = null,
            UserId = "test-user-id",
            UserName = null
        };

        // Assert
        Assert.Null(todoDto.Description);
        Assert.Null(todoDto.CompletedAt);
        Assert.Null(todoDto.UserName);
    }
}