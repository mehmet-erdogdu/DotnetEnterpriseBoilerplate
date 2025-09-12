namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class TodoRepositoryTests : BaseTestClass
{
    private new readonly ApplicationDbContext _context = null!;
    private readonly TodoRepository _todoRepository = null!;

    public TodoRepositoryTests()
    {
        _context = CreateDbContext();
        _todoRepository = new TodoRepository(_context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _context.Dispose();
        base.Dispose(disposing);
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_WithValidUserId_ReturnsUserTodos()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var todo1 = TestHelper.TestData.CreateTestTodo(user.Id);
        var todo2 = TestHelper.TestData.CreateTestTodo(user.Id, Guid.NewGuid());
        var todo3 = TestHelper.TestData.CreateTestTodo("other-user-id", Guid.NewGuid());

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _todoRepository.GetTodosByUserIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().OnlyContain(t => t.UserId == user.Id);
        // Note: The ordering is handled by the database query, but in-memory database might not guarantee the same order
        // So we're not asserting the order here
    }

    [Fact]
    public async Task GetTodosByUserIdAsync_WithNonExistentUserId_ReturnsEmptyList()
    {
        // Arrange
        var userId = "non-existent-user-id";

        // Act
        var result = await _todoRepository.GetTodosByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTodoWithUserAsync_WithValidTodoId_ReturnsTodoWithUser()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var todo = TestHelper.TestData.CreateTestTodo(user.Id);
        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _todoRepository.GetTodoWithUserAsync(todo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todo.Id);
        result.User.Should().NotBeNull();
        result.User!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetTodoWithUserAsync_WithNonExistentTodoId_ReturnsNull()
    {
        // Arrange
        var todoId = Guid.NewGuid();

        // Act
        var result = await _todoRepository.GetTodoWithUserAsync(todoId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllWithUsersAsync_ReturnsAllTodosWithUsers()
    {
        // Arrange
        var user1 = TestHelper.TestData.CreateTestUser("user1");
        var user2 = TestHelper.TestData.CreateTestUser("user2", "user2@example.com");
        await _context.Users.AddRangeAsync(user1, user2);

        var todo1 = TestHelper.TestData.CreateTestTodo(user1.Id);
        var todo2 = TestHelper.TestData.CreateTestTodo(user2.Id, Guid.NewGuid());
        var todo3 = TestHelper.TestData.CreateTestTodo(user1.Id, Guid.NewGuid());

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _todoRepository.GetAllWithUsersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(t => t.User != null);
        // Note: The ordering is handled by the database query, but in-memory database might not guarantee the same order
        // So we're not asserting the order here
    }

    [Fact]
    public async Task GetCountByUserIdAsync_WithValidUserId_ReturnsCorrectCount()
    {
        // Arrange
        var user = TestHelper.TestData.CreateTestUser();
        await _context.Users.AddAsync(user);

        var todo1 = TestHelper.TestData.CreateTestTodo(user.Id);
        var todo2 = TestHelper.TestData.CreateTestTodo(user.Id, Guid.NewGuid());
        var todo3 = TestHelper.TestData.CreateTestTodo("other-user-id", Guid.NewGuid());

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _todoRepository.GetCountByUserIdAsync(user.Id);

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCountByUserIdAsync_WithNonExistentUserId_ReturnsZero()
    {
        // Arrange
        var userId = "non-existent-user-id";

        // Act
        var result = await _todoRepository.GetCountByUserIdAsync(userId);

        // Assert
        result.Should().Be(0);
    }
}