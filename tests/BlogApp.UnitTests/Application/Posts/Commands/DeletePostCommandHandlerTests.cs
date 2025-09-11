namespace BlogApp.UnitTests.Application.Posts.Commands;

public class DeletePostCommandHandlerTests : BaseApplicationTest
{
    private readonly DeletePostCommandHandler _handler;
    private readonly Mock<ILogger<DeletePostCommandHandler>> _mockPostLogger;

    public DeletePostCommandHandlerTests()
    {
        _mockPostLogger = new Mock<ILogger<DeletePostCommandHandler>>();
        _handler = new DeletePostCommandHandler(
            _mockUnitOfWork.Object,
            _mockCacheInvalidationService.Object,
            _mockPostLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidPostId_DeletesPostAndReturnsTrue()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new DeletePostCommand { Id = postId };
        var mockPost = new Post { Id = postId, Title = "Test Post", Content = "Test Content", AuthorId = "test-author" };

        _mockUnitOfWork.Setup(x => x.Posts.GetByIdAsync(postId))
            .ReturnsAsync(mockPost);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.Posts.Remove(mockPost), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(postId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidPostId_ReturnsFalse()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new DeletePostCommand { Id = postId };

        _mockUnitOfWork.Setup(x => x.Posts.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.Posts.Remove(It.IsAny<Post>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_RollsBackTransaction()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new DeletePostCommand { Id = postId };
        var mockPost = new Post { Id = postId, Title = "Test Post", Content = "Test Content", AuthorId = "test-author" };

        _mockUnitOfWork.Setup(x => x.Posts.GetByIdAsync(postId))
            .ReturnsAsync(mockPost);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }
}