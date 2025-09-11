namespace BlogApp.UnitTests.Application.Posts.Queries;

public class GetPostByIdQueryHandlerTests : BaseTestClass
{
    private readonly GetPostByIdQueryHandler _handler;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<GetPostByIdQueryHandler>> _mockLogger;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public GetPostByIdQueryHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<GetPostByIdQueryHandler>>();

        _handler = new GetPostByIdQueryHandler(
            _mockUnitOfWork.Object,
            _mockCacheService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Handle_WithCachedPost_ShouldReturnCachedResult()
    {
        // Arrange
        var query = new GetPostByIdQuery { Id = Guid.NewGuid() };
        var cachedPost = TestHelper.TestData.CreateTestPostDto(id: query.Id);

        _mockCacheService.Setup(x => x.GetAsync<PostDto>(It.IsAny<string>()))
            .ReturnsAsync(cachedPost);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(query.Id);

        _mockCacheService.Verify(x => x.GetAsync<PostDto>(It.IsAny<string>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.Posts.GetPostWithAuthorAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCacheMissAndExistingPost_ShouldReturnPostFromDatabase()
    {
        // Arrange
        var query = new GetPostByIdQuery { Id = Guid.NewGuid() };
        var post = TestHelper.TestData.CreateTestPost(id: query.Id);

        _mockCacheService.Setup(x => x.GetAsync<PostDto>(It.IsAny<string>()))
            .ReturnsAsync((PostDto?)null);

        _mockUnitOfWork.Setup(x => x.Posts.GetPostWithAuthorAsync(query.Id))
            .ReturnsAsync(post);

        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PostDto>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(query.Id);
        result.Title.Should().Be(post.Title);
        result.Content.Should().Be(post.Content);

        _mockUnitOfWork.Verify(x => x.Posts.GetPostWithAuthorAsync(query.Id), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PostDto>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCacheMissAndNonExistentPost_ShouldReturnNull()
    {
        // Arrange
        var query = new GetPostByIdQuery { Id = Guid.NewGuid() };

        _mockCacheService.Setup(x => x.GetAsync<PostDto>(It.IsAny<string>()))
            .ReturnsAsync((PostDto?)null);

        _mockUnitOfWork.Setup(x => x.Posts.GetPostWithAuthorAsync(query.Id))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.Posts.GetPostWithAuthorAsync(query.Id), Times.Once);
        _mockCacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCacheException_ShouldContinueWithDatabase()
    {
        // Arrange
        var query = new GetPostByIdQuery { Id = Guid.NewGuid() };
        var post = TestHelper.TestData.CreateTestPost(id: query.Id);

        _mockCacheService.Setup(x => x.GetAsync<PostDto>(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Cache connection failed"));

        _mockUnitOfWork.Setup(x => x.Posts.GetPostWithAuthorAsync(query.Id))
            .ReturnsAsync(post);

        _mockCacheService.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<PostDto>(), It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(query.Id);

        _mockUnitOfWork.Verify(x => x.Posts.GetPostWithAuthorAsync(query.Id), Times.Once);
    }
}