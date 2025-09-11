namespace BlogApp.UnitTests.Application.Todos.Queries;

public class GetTodosCountQueryHandlerTests : BaseApplicationTest
{
    private readonly GetTodosCountQueryHandler _handler;
    private readonly Mock<ITodoRepository> _mockTodoRepository;

    public GetTodosCountQueryHandlerTests()
    {
        _mockTodoRepository = new Mock<ITodoRepository>();
        _handler = new GetTodosCountQueryHandler(_mockTodoRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnTodoCount()
    {
        // Arrange
        var userId = "test-user-id";
        var query = new GetTodosCountQuery(userId);
        var expectedCount = 5;

        _mockTodoRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedCount);

        _mockTodoRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroTodos_ShouldReturnZero()
    {
        // Arrange
        var userId = "test-user-id";
        var query = new GetTodosCountQuery(userId);

        _mockTodoRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);

        _mockTodoRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnZero()
    {
        // Arrange
        var userId = "non-existent-user-id";
        var query = new GetTodosCountQuery(userId);

        _mockTodoRepository.Setup(x => x.GetCountByUserIdAsync(userId))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(0);

        _mockTodoRepository.Verify(x => x.GetCountByUserIdAsync(userId), Times.Once);
    }
}