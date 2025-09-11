namespace BlogApp.UnitTests.Application.Todos.Queries;

public class GetTodoByIdQueryHandlerTests : BaseApplicationTest
{
    private readonly GetTodoByIdQueryHandler _handler;

    public GetTodoByIdQueryHandlerTests()
    {
        _handler = new GetTodoByIdQueryHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithExistingTodo_ShouldReturnTodoDto()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var query = new GetTodoByIdQuery { Id = todoId };
        var todo = TestHelper.TestData.CreateTestTodo(id: todoId);
        todo.User = TestHelper.TestData.CreateTestUser();

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync(todo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todoId);
        result.Title.Should().Be(todo.Title);
        result.Description.Should().Be(todo.Description);
        result.UserId.Should().Be(todo.UserId);
        result.UserName.Should().Be($"{todo.User.FirstName} {todo.User.LastName}");
        result.IsCompleted.Should().Be(todo.IsCompleted);
        result.CreatedAt.Should().Be(todo.CreatedAt);

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentTodo_ShouldReturnNull()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var query = new GetTodoByIdQuery { Id = todoId };

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync((Todo?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(todoId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTodoWithoutUser_ShouldReturnTodoDtoWithNullUserName()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var query = new GetTodoByIdQuery { Id = todoId };
        var todo = TestHelper.TestData.CreateTestTodo(id: todoId);
        todo.User = null;

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync(todo);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UserName.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(todoId), Times.Once);
    }
}