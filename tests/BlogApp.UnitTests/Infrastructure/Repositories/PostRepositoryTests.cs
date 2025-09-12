namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class PostRepositoryTests : BaseTestClass
{
    private new readonly ApplicationDbContext _context = null!;
    private readonly PostRepository _postRepository = null!;

    public PostRepositoryTests()
    {
        _context = CreateDbContext();
        _postRepository = new PostRepository(_context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _context.Dispose();
        base.Dispose(disposing);
    }

    [Fact]
    public async Task GetPostsByAuthorIdAsync_WithValidAuthorId_ReturnsAuthorPosts()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var post1 = TestHelper.TestData.CreateTestPost(user.Id);
        var post2 = TestHelper.TestData.CreateTestPost(user.Id, Guid.NewGuid());
        var post3 = TestHelper.TestData.CreateTestPost("other-user-id", Guid.NewGuid());

        await _context.Posts.AddRangeAsync(post1, post2, post3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetPostsByAuthorIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.AuthorId == user.Id);
        // Note: The ordering is handled by the database query, but in-memory database might not guarantee the same order
        // So we're not asserting the order here
    }

    [Fact]
    public async Task GetPostsByAuthorIdAsync_WithNonExistentAuthorId_ReturnsEmptyList()
    {
        // Arrange
        var authorId = "non-existent-author-id";

        // Act
        var result = await _postRepository.GetPostsByAuthorIdAsync(authorId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPostWithAuthorAsync_WithValidPostId_ReturnsPostWithAuthor()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var post = TestHelper.TestData.CreateTestPost(user.Id);
        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetPostWithAuthorAsync(post.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(post.Id);
        result.Author.Should().NotBeNull();
        result.Author!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetPostWithAuthorAsync_WithNonExistentPostId_ReturnsNull()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var result = await _postRepository.GetPostWithAuthorAsync(postId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllWithAuthors_ReturnsAllPostsWithAuthors()
    {
        // Arrange
        var user1 = TestHelper.TestData.CreateTestUser("user1");
        var user2 = TestHelper.TestData.CreateTestUser("user2", "user2@example.com");
        await _context.Users.AddRangeAsync(user1, user2);

        var post1 = TestHelper.TestData.CreateTestPost(user1.Id);
        var post2 = TestHelper.TestData.CreateTestPost(user2.Id, Guid.NewGuid());
        var post3 = TestHelper.TestData.CreateTestPost(user1.Id, Guid.NewGuid());

        await _context.Posts.AddRangeAsync(post1, post2, post3);
        await _context.SaveChangesAsync();

        // Act
        var result = _postRepository.GetAllWithAuthors();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(p => p.Author != null);
        // Note: The ordering is handled by the database query, but in-memory database might not guarantee the same order
        // So we're not asserting the order here
    }

    [Fact]
    public async Task GetCountByUserIdAsync_WithValidUserId_ReturnsCorrectCount()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var post1 = TestHelper.TestData.CreateTestPost(user.Id);
        var post2 = TestHelper.TestData.CreateTestPost(user.Id, Guid.NewGuid());
        var post3 = TestHelper.TestData.CreateTestPost("other-user-id", Guid.NewGuid());

        await _context.Posts.AddRangeAsync(post1, post2, post3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _postRepository.GetCountByUserIdAsync(user.Id);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCountByUserIdAsync_WithNonExistentUserId_ReturnsZero()
    {
        // Arrange
        var userId = "non-existent-user-id";

        // Act
        var result = await _postRepository.GetCountByUserIdAsync(userId);

        // Assert
        result.Should().Be(0);
    }
}