namespace BlogApp.UnitTests.Application.Posts.Commands;

public class CreatePostCommandHandlerTests : BaseApplicationTest
{
    private readonly CreatePostCommandHandler _handler;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ILogger<CreatePostCommandHandler>> _mockSpecificLogger;

    public CreatePostCommandHandlerTests()
    {
        _mockSpecificLogger = new Mock<ILogger<CreatePostCommandHandler>>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns("test-user-id");

        _handler = new CreatePostCommandHandler(
            _mockUnitOfWork.Object,
            _mockCacheInvalidationService.Object,
            _mockSpecificLogger.Object,
            _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePostAndReturnDto()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            Title = "Test Post",
            Content = "Test Content"
        };

        var mockPostRepository = new Mock<IPostRepository>();
        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.AddAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);

        _mockCacheInvalidationService.Setup(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.Content.Should().Be(command.Content);
        result.AuthorId.Should().Be("test-user-id");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        mockPostRepository.Verify(x => x.AddAsync(It.IsAny<Post>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_ShouldRollbackAndThrow()
    {
        // Arrange
        var command = new CreatePostCommand
        {
            Title = "Test Post",
            Content = "Test Content"
        };

        var mockPostRepository = new Mock<IPostRepository>();
        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));
        _mockUnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.AddAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);

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
        var command = new CreatePostCommand
        {
            Title = "Test Post",
            Content = "Test Content"
        };

        var mockPostRepository = new Mock<IPostRepository>();
        _mockUnitOfWork.Setup(x => x.Posts).Returns(mockPostRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockPostRepository.Setup(x => x.AddAsync(It.IsAny<Post>()))
            .Returns(Task.CompletedTask);

        _mockCacheInvalidationService.Setup(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()))
            .ThrowsAsync(new Exception("Cache error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));
        exception.Message.Should().Be("Cache error");

        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        _mockCacheInvalidationService.Verify(x => x.InvalidatePostCacheAsync(It.IsAny<Guid?>()), Times.Once);
    }
}