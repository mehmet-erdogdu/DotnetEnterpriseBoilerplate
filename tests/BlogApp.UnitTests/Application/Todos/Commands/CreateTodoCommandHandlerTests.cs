namespace BlogApp.UnitTests.Application.Todos.Commands;

public class CreateTodoCommandHandlerTests : BaseApplicationTest
{
    private readonly CreateTodoCommandHandler _handler;

    public CreateTodoCommandHandlerTests()
    {
        _handler = new CreateTodoCommandHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTodoAndReturnDto()
    {
        // Arrange
        var command = new CreateTodoCommand
        {
            Title = "Test Todo",
            Description = "Test Description",
            UserId = "test-user-id"
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        var todoWithUser = TestHelper.TestData.CreateTestTodo(command.UserId);
        todoWithUser.User = TestHelper.TestData.CreateTestUser(command.UserId);

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.AddAsync(It.IsAny<Todo>()))
            .Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(todoWithUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.Description.Should().Be(command.Description);
        result.UserId.Should().Be(command.UserId);
        result.UserName.Should().Be($"{todoWithUser.User.FirstName} {todoWithUser.User.LastName}");
        result.IsCompleted.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _mockUnitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Once);
        mockTodoRepository.Verify(x => x.AddAsync(It.IsAny<Todo>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_ShouldRollbackAndThrow()
    {
        // Arrange
        var command = new CreateTodoCommand
        {
            Title = "Test Todo",
            Description = "Test Description",
            UserId = "test-user-id"
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Database error"));
        _mockUnitOfWork.Setup(x => x.RollbackAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.AddAsync(It.IsAny<Todo>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _handler.Handle(command, CancellationToken.None));

        _mockUnitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Never);
        mockTodoRepository.Verify(x => x.GetTodoWithUserAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldCreateTodoWithNullUserName()
    {
        // Arrange
        var command = new CreateTodoCommand
        {
            Title = "Test Todo",
            Description = "Test Description",
            UserId = "test-user-id"
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        var todoWithoutUser = TestHelper.TestData.CreateTestTodo(command.UserId);
        todoWithoutUser.User = null;

        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);
        _mockUnitOfWork.Setup(x => x.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);
        _mockUnitOfWork.Setup(x => x.CommitAsync()).Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.AddAsync(It.IsAny<Todo>()))
            .Returns(Task.CompletedTask);

        mockTodoRepository.Setup(x => x.GetTodoWithUserAsync(It.IsAny<Guid>()))
            .ReturnsAsync(todoWithoutUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(command.Title);
        result.UserName.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.CommitAsync(), Times.Once);
    }
}