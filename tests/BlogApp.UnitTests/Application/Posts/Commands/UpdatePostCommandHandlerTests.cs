namespace BlogApp.UnitTests.Application.Posts.Commands;

public class UpdatePostCommandHandlerTests : BaseApplicationTest
{
    private readonly UpdatePostCommandHandler _handler;
    private readonly Mock<ILogger<UpdatePostCommandHandler>> _mockSpecificLogger;

    public UpdatePostCommandHandlerTests()
    {
        _mockSpecificLogger = new Mock<ILogger<UpdatePostCommandHandler>>();
        
        _handler = new UpdatePostCommandHandler(
            _mockUnitOfWork.Object,
            _mockCacheInvalidationService.Object,
            _mockSpecificLogger.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePostAndReturnDto()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new UpdatePostCommand
        {
            Id = postId,
            Title = "Updated Post Title",
            Content = "Updated post content"
        };

        var mockPostRepository = new Mock<IPostRepository>();
        var existingPost = TestHelper.TestData.CreateTestPost(id: postId);
        existingPost.Author = TestHelper.TestData.CreateTestUser();

        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.GetPostWithAuthorAsync(postId))
            .ReturnsAsync(existingPost);

        _mockCacheInvalidationService.Setup(x => x.InvalidatePostCacheAsync(postId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(postId);
        result.Title.Should().Be(command.Title);
        result.Content.Should().Be(command.Content);
        result.AuthorId.Should().Be(existingPost.AuthorId);
        result.AuthorName.Should().Be($"{existingPost.Author.FirstName} {existingPost.Author.LastName}");

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        mockPostRepository.Verify(x => x.GetPostWithAuthorAsync(postId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(postId), Times.Once);
        _mockSpecificLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Updated post and invalidated cache")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenPostNotFound_ShouldReturnNull()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new UpdatePostCommand
        {
            Id = postId,
            Title = "Updated Post Title",
            Content = "Updated post content"
        };

        var mockPostRepository = new Mock<IPostRepository>();

        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.GetPostWithAuthorAsync(postId))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        mockPostRepository.Verify(x => x.GetPostWithAuthorAsync(postId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_ShouldRollbackAndThrow()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new UpdatePostCommand
        {
            Id = postId,
            Title = "Updated Post Title",
            Content = "Updated post content"
        };

        var mockPostRepository = new Mock<IPostRepository>();
        var existingPost = TestHelper.TestData.CreateTestPost(id: postId);

        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));
        _mockUnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.GetPostWithAuthorAsync(postId))
            .ReturnsAsync(existingPost);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenCacheInvalidationFails_ShouldThrowException()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command = new UpdatePostCommand
        {
            Id = postId,
            Title = "Updated Post Title",
            Content = "Updated post content"
        };

        var mockPostRepository = new Mock<IPostRepository>();
        var existingPost = TestHelper.TestData.CreateTestPost(id: postId);
        existingPost.Author = TestHelper.TestData.CreateTestUser();

        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.GetPostWithAuthorAsync(postId))
            .ReturnsAsync(existingPost);

        _mockCacheInvalidationService.Setup(x => x.InvalidatePostCacheAsync(postId))
            .ThrowsAsync(new Exception("Cache error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Cache error");

        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(postId), Times.Once);
    }
}