namespace BlogApp.UnitTests.Application.Todos.Commands;

public class UpdateTodoCommandHandlerTests : BaseApplicationTest
{
    private readonly UpdateTodoCommandHandler _handler;

    public UpdateTodoCommandHandlerTests()
    {
        _handler = new UpdateTodoCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateTodoAndReturnDto()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new UpdateTodoCommand
        {
            Id = todoId,
            Title = "Updated Todo Title",
            Description = "Updated Description",
            IsCompleted = false
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        var existingTodo = TestHelper.TestData.CreateTestTodo(id: todoId);
        existingTodo.User = TestHelper.TestData.CreateTestUser();

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync(existingTodo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todoId);
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.IsCompleted.Should().Be(command.IsCompleted);
        result.UserId.Should().Be(existingTodo.UserId);
        result.UserName.Should().Be($"{existingTodo.User.FirstName} {existingTodo.User.LastName}");

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(todoId), Times.Once);
        mockTodoRepository.Verify(x => x.Update(It.IsAny<Todo>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTodoNotFound_ShouldReturnNull()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new UpdateTodoCommand
        {
            Id = todoId,
            Title = "Updated Todo Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        var mockTodoRepository = new Mock<ITodoRepository>();

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync((Todo?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(todoId), Times.Once);
        mockTodoRepository.Verify(x => x.Update(It.IsAny<Todo>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMarkingAsCompleted_ShouldSetCompletedAt()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new UpdateTodoCommand
        {
            Id = todoId,
            Title = "Updated Todo Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        var existingTodo = TestHelper.TestData.CreateTestTodo(id: todoId);
        existingTodo.IsCompleted = false;
        existingTodo.CompletedAt = null;
        existingTodo.User = TestHelper.TestData.CreateTestUser();

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync(existingTodo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenMarkingAsNotCompleted_ShouldClearCompletedAt()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new UpdateTodoCommand
        {
            Id = todoId,
            Title = "Updated Todo Title",
            Description = "Updated Description",
            IsCompleted = false
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        var existingTodo = TestHelper.TestData.CreateTestTodo(id: todoId);
        existingTodo.IsCompleted = true;
        existingTodo.CompletedAt = DateTime.UtcNow.AddDays(-1);
        existingTodo.User = TestHelper.TestData.CreateTestUser();

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync(existingTodo);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeFalse();
        result.CompletedAt.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_ShouldRollbackAndThrow()
    {
        // Arrange
        var todoId = Guid.NewGuid();
        var command = new UpdateTodoCommand
        {
            Id = todoId,
            Title = "Updated Todo Title",
            Description = "Updated Description",
            IsCompleted = true
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        var existingTodo = TestHelper.TestData.CreateTestTodo(id: todoId);

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));
        _mockUnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(todoId))
            .ReturnsAsync(existingTodo);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        mockTodoRepository.Verify(x => x.Update(It.IsAny<Todo>()), Times.Once);
    }
}