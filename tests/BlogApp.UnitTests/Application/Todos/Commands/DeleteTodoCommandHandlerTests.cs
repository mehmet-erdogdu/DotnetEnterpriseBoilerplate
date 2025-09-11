namespace BlogApp.UnitTests.Application.Todos.Commands;

public class DeleteTodoCommandHandlerTests : BaseApplicationTest
{
    private readonly DeleteTodoCommandHandler _handler;

    public DeleteTodoCommandHandlerTests()
    {
        _handler = new DeleteTodoCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidTodoId_DeletesTodoAndReturnsTrue()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand { Id = todoId };
        var mockTodo = new Todo { Id = todoId, Title = "Test Todo", UserId = "test-user" };

        _mockUnitOfWork.Setup(x => x.Todos.GetByIdAsync(todoId))
            .ReturnsAsync(mockTodo);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.Todos.Remove(mockTodo), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidTodoId_ReturnsFalse()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand { Id = todoId };

        _mockUnitOfWork.Setup(x => x.Todos.GetByIdAsync(todoId))
            .ReturnsAsync((Todo?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.Todos.Remove(It.IsAny<Todo>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_RollsBackTransaction()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new DeleteTodoCommand { Id = todoId };
        var mockTodo = new Todo { Id = todoId, Title = "Test Todo", UserId = "test-user" };

        _mockUnitOfWork.Setup(x => x.Todos.GetByIdAsync(todoId))
            .ReturnsAsync(mockTodo);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }
}