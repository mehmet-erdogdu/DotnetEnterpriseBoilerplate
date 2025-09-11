namespace BlogApp.UnitTests.Application.Files.Queries;

public class GetFilesCountQueryHandlerTests : BaseApplicationTest
{
    private readonly GetFilesCountQueryHandler _handler;
    private readonly Mock<IFileRepository> _mockFileRepository;

    public GetFilesCountQueryHandlerTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _handler = new GetFilesCountQueryHandler(_mockFileRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnFileCount()
    {
        // Arrange
        var userId = "test-user-id";
        var query = new GetFilesCountQuery(userId);
        var expectedCount = 5;

        _mockFileRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedCount);

        _mockFileRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroFiles_ShouldReturnZero()
    {
        // Arrange
        var userId = "test-user-id";
        var query = new GetFilesCountQuery(userId);

        _mockFileRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);

        _mockFileRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnZero()
    {
        // Arrange
        var userId = "non-existent-user-id";
        var query = new GetFilesCountQuery(userId);

        _mockFileRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);

        _mockFileRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }
}