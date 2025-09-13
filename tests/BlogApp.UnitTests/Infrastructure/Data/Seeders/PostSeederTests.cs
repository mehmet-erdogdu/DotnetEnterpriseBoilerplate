using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class PostSeederTests : BaseTestClass
{
    public PostSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoPostsExistAndUsersExist_ShouldCreateSamplePosts()
    {
        // Arrange
        var author = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "author@example.com",
            Email = "author@example.com",
            FirstName = "Author",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        _context!.Users.Add(author);
        await _context.SaveChangesAsync();

        var seeder = new PostSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var posts = await _context.Posts.ToListAsync();
        posts.Should().HaveCount(3);

        var firstPost = posts.FirstOrDefault(p => p.Title == "Welcome to Our Blog");
        firstPost.Should().NotBeNull();
        firstPost!.Content.Should().NotBeNullOrEmpty();
        firstPost.AuthorId.Should().Be(author.Id);
    }

    [Fact]
    public async Task SeedAsync_WhenPostsAlreadyExist_ShouldNotCreateNewPosts()
    {
        // Arrange
        var existingPost = new Post
        {
            Id = Guid.NewGuid(),
            Title = "Existing Post",
            Content = "This is an existing post",
            AuthorId = "author-id",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "author-id"
        };

        _context!.Posts.Add(existingPost);
        await _context.SaveChangesAsync();

        var seeder = new PostSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var posts = await _context.Posts.ToListAsync();
        posts.Should().HaveCount(1); // Should still be only 1 post
        posts[0].Title.Should().Be("Existing Post");
    }

    [Fact]
    public async Task SeedAsync_WhenNoUsersExist_ShouldNotCreatePosts()
    {
        // Arrange
        var seeder = new PostSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var posts = await _context!.Posts.ToListAsync();
        posts.Should().BeEmpty();
    }
}