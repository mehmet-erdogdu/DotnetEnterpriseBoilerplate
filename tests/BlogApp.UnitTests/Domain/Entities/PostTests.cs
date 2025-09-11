namespace BlogApp.UnitTests.Domain.Entities;

public class PostTests : BaseTestClass
{
    [Fact]
    public void Post_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Test Post";
        var content = "Test Content";
        var authorId = "test-author-id";
        var bannerImageFileId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var post = new Post
        {
            Id = id,
            Title = title,
            Content = content,
            AuthorId = authorId,
            BannerImageFileId = bannerImageFileId,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        post.Id.Should().Be(id);
        post.Title.Should().Be(title);
        post.Content.Should().Be(content);
        post.AuthorId.Should().Be(authorId);
        post.BannerImageFileId.Should().Be(bannerImageFileId);
        post.CreatedAt.Should().Be(createdAt);
        post.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void Post_WithRequiredProperties_ShouldBeValid()
    {
        // Act
        var post = new Post
        {
            Title = "Test Post",
            Content = "Test Content",
            AuthorId = "test-author-id"
        };

        // Assert
        post.Title.Should().Be("Test Post");
        post.Content.Should().Be("Test Content");
        post.AuthorId.Should().Be("test-author-id");
        post.BannerImageFileId.Should().BeNull(); // Default value
    }

    [Fact]
    public void Post_WithNullBannerImageFileId_ShouldBeValid()
    {
        // Act
        var post = new Post
        {
            Title = "Test Post",
            Content = "Test Content",
            AuthorId = "test-author-id",
            BannerImageFileId = null
        };

        // Assert
        post.BannerImageFileId.Should().BeNull();
    }
}