namespace BlogApp.UnitTests.Application.Todos.Queries;

public class GetAllTodosQueryHandlerTests : BaseApplicationTest
{
    private readonly GetAllTodosQueryHandler _handler;

    public GetAllTodosQueryHandlerTests()
    {
        _handler = new GetAllTodosQueryHandler(_mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithNoPaging_ShouldReturnAllTodos()
    {
        // Arrange
        var query = new GetAllTodosQuery();
        var todos = new List<Todo>
        {
            TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid()),
            TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid())
        };

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetAllWithUsersAsync())
            .ReturnsAsync(todos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetAllWithUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPaging_ShouldReturnPagedTodos()
    {
        // Arrange
        var query = new GetAllTodosQuery
        {
            Page = 1,
            PageSize = 2
        };

        var todos = new List<Todo>();
        for (int i = 0; i < 5; i++)
        {
            todos.Add(TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid()));
        }

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetAllWithUsersAsync())
            .ReturnsAsync(todos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetAllWithUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithZeroPageSize_ShouldUseDefaultPageSize()
    {
        // Arrange
        var query = new GetAllTodosQuery
        {
            Page = 1,
            PageSize = 0
        };

        var todos = new List<Todo>();
        for (int i = 0; i < 15; i++)
        {
            todos.Add(TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid()));
        }

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetAllWithUsersAsync())
            .ReturnsAsync(todos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(10); // Default page size is 10

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetAllWithUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTodosHavingUsers_ShouldMapUserNames()
    {
        // Arrange
        var query = new GetAllTodosQuery();
        var user = TestHelper.TestData.CreateTestUser();
        var todo = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        todo.User = user;

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetAllWithUsersAsync())
            .ReturnsAsync(new List<Todo> { todo });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var todoDto = result.First();

        // Assert
        todoDto.UserName.Should().Be($"{user.FirstName} {user.LastName}");

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetAllWithUsersAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithTodosWithoutUsers_ShouldHaveNullUserName()
    {
        // Arrange
        var query = new GetAllTodosQuery();
        var todo = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        todo.User = null;

        var mockTodoRepository = new Mock<ITodoRepository>();
        _mockUnitOfWork.Setup(x => x.Todos).Returns(mockTodoRepository.Object);

        mockTodoRepository.Setup(x => x.GetAllWithUsersAsync())
            .ReturnsAsync(new List<Todo> { todo });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);
        var todoDto = result.First();

        // Assert
        todoDto.UserName.Should().BeNull();

        _mockUnitOfWork.Verify(x => x.Todos, Times.Once);
        mockTodoRepository.Verify(x => x.GetAllWithUsersAsync(), Times.Once);
    }
}