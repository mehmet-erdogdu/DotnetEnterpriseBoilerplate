namespace BlogApp.UnitTests.Application.DTOs;

public class PostDtoTests
{
    [Fact]
    public void PostDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Post";
        var content = "This is a test post content";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddHours(1);
        var authorId = "test-author-id";
        var authorName = "Test Author";

        // Act
        var postDto = new PostDto
        {
            Id = id,
            Title = title,
            Content = content,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            AuthorId = authorId,
            AuthorName = authorName
        };

        // Assert
        Assert.Equal(id, postDto.Id);
        Assert.Equal(title, postDto.Title);
        Assert.Equal(content, postDto.Content);
        Assert.Equal(createdAt, postDto.CreatedAt);
        Assert.Equal(updatedAt, postDto.UpdatedAt);
        Assert.Equal(authorId, postDto.AuthorId);
        Assert.Equal(authorName, postDto.AuthorName);
    }

    [Fact]
    public void PostDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var postDto = new PostDto
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            Content = "This is a test post content",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            AuthorId = "test-author-id",
            AuthorName = null
        };

        // Assert
        Assert.Null(postDto.UpdatedAt);
        Assert.Null(postDto.AuthorName);
    }
}