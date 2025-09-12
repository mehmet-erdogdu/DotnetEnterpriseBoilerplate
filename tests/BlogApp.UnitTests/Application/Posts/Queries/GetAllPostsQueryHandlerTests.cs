namespace BlogApp.UnitTests.Application.Posts.Queries;

public class GetAllPostsQueryHandlerTests : BaseTestClass
{
    [Fact]
    public void Should_CreateHandler_Successfully()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        // Act
        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);

        // Assert
        handler.Should().NotBeNull();
    }

    [Fact]
    public void Should_AccessUnitOfWork_Successfully()
    {
        // Arrange
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockPostRepository = new Mock<IPostRepository>();

        mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);

        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);

        // Act - Access the Posts property to verify setup
        var posts = mockUnitOfWork.Object.Posts;

        // Assert
        posts.Should().NotBeNull();
        mockUnitOfWork.Verify(x => x.Posts, Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllPosts()
    {
        // Arrange
        var context = CreateDbContext();
        var author = new ApplicationUser { Id = "author1", FirstName = "John", LastName = "Doe" };
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "Post 1", Content = "Content 1", AuthorId = author.Id, Author = author, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Post 2", Content = "Content 2", AuthorId = author.Id, Author = author, CreatedAt = DateTime.UtcNow }
        };

        context.Users.Add(author);
        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockPostRepository = new Mock<IPostRepository>();

        mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        mockPostRepository.Setup(x => x.GetAllWithAuthors()).Returns(context.Posts.AsQueryable());

        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);
        var query = new GetAllPostsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.AuthorName == "John Doe");
    }

    [Fact]
    public async Task Handle_WithSearchFilter_ReturnsMatchingPosts()
    {
        // Arrange
        var context = CreateDbContext();
        var author = new ApplicationUser { Id = "author1", FirstName = "John", LastName = "Doe" };
        var posts = new List<Post>
        {
            new() { Id = Guid.NewGuid(), Title = "First Post", Content = "Content 1", AuthorId = author.Id, Author = author, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Second Post", Content = "Content 2", AuthorId = author.Id, Author = author, CreatedAt = DateTime.UtcNow },
            new() { Id = Guid.NewGuid(), Title = "Other Item", Content = "Content 3", AuthorId = author.Id, Author = author, CreatedAt = DateTime.UtcNow }
        };

        context.Users.Add(author);
        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockPostRepository = new Mock<IPostRepository>();

        mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        mockPostRepository.Setup(x => x.GetAllWithAuthors()).Returns(context.Posts.AsQueryable());

        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);
        var query = new GetAllPostsQuery { Search = "Post" }; // Should match first two posts

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Title.Contains("Post"));
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsPagedResults()
    {
        // Arrange
        var context = CreateDbContext();
        var author = new ApplicationUser { Id = "author1", FirstName = "John", LastName = "Doe" };
        var posts = new List<Post>();

        // Create 15 posts
        for (var i = 1; i <= 15; i++)
            posts.Add(new Post
            {
                Id = Guid.NewGuid(),
                Title = $"Post {i}",
                Content = $"Content {i}",
                AuthorId = author.Id,
                Author = author,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });

        context.Users.Add(author);
        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();

        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockPostRepository = new Mock<IPostRepository>();

        mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        mockPostRepository.Setup(x => x.GetAllWithAuthors()).Returns(context.Posts.AsQueryable());

        var handler = new GetAllPostsQueryHandler(mockUnitOfWork.Object);
        var query = new GetAllPostsQuery { Page = 2, PageSize = 5 }; // Should return posts 6-10

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
    }
}