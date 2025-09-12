namespace BlogApp.UnitTests.Application.Posts.Queries;

public class GetPostsCountQueryHandlerTests
{
    private readonly GetPostsCountQueryHandler _handler;
    private readonly Mock<ILogger<GetPostsCountQueryHandler>> _mockLogger;
    private readonly Mock<IPostRepository> _mockPostRepository;

    public GetPostsCountQueryHandlerTests()
    {
        _mockPostRepository = new Mock<IPostRepository>();
        _mockLogger = new Mock<ILogger<GetPostsCountQueryHandler>>();

        // Create handler with mocked dependencies
        _handler = new GetPostsCountQueryHandler(_mockPostRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnPostCount()
    {
        // Arrange
        var userId = "test-user-id";
        var expectedCount = 5;
        var query = new GetPostsCountQuery(userId);

        _mockPostRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedCount);
        _mockPostRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroPosts_ShouldReturnZero()
    {
        // Arrange
        var userId = "test-user-id";
        var expectedCount = 0;
        var query = new GetPostsCountQuery(userId);

        _mockPostRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedCount);
        _mockPostRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDifferentUserIds_ShouldCallRepositoryWithCorrectUserId()
    {
        // Arrange
        var userId1 = "user-1";
        var userId2 = "user-2";
        var query1 = new GetPostsCountQuery(userId1);
        var query2 = new GetPostsCountQuery(userId2);

        _mockPostRepository.Setup(x => x.GetCountByUserIdAsync(userId1))
            .ReturnsAsync(3);
        _mockPostRepository.Setup(x => x.GetCountByUserIdAsync(userId2))
            .ReturnsAsync(7);

        // Act
        var result1 = await _handler.Handle(query1, CancellationToken.None);
        var result2 = await _handler.Handle(query2, CancellationToken.None);

        // Assert
        result1.Should().Be(3);
        result2.Should().Be(7);
        _mockPostRepository.Verify(x => x.GetCountByUserIdAsync(userId1), Times.Once);
        _mockPostRepository.Verify(x => x.GetCountByUserIdAsync(userId2), Times.Once);
    }
}