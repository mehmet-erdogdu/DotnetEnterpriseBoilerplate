namespace BlogApp.UnitTests.Infrastructure.Repositories;

public class GenericRepositoryTests : BaseTestClass
{
    private new readonly ApplicationDbContext _context = null!;
    private readonly GenericRepository<Todo> _repository = null!;

    public GenericRepositoryTests()
    {
        _context = CreateDbContext();
        _repository = new GenericRepository<Todo>(_context);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _context.Dispose();
        base.Dispose(disposing);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var todo = TestHelper.TestData.CreateTestTodo();
        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(todo.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(todo.Id);
        result.Title.Should().Be(todo.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var todo1 = TestHelper.TestData.CreateTestTodo();
        var todo2 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        var todo3 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task FindAsync_WithMatchingExpression_ReturnsFilteredEntities()
    {
        // Arrange
        var user1Todos = new List<Todo>();
        var user2Todos = new List<Todo>();

        for (var i = 0; i < 3; i++)
        {
            var todo = TestHelper.TestData.CreateTestTodo("user1", Guid.NewGuid());
            user1Todos.Add(todo);
            await _context.Todos.AddAsync(todo);
        }

        for (var i = 0; i < 2; i++)
        {
            var todo = TestHelper.TestData.CreateTestTodo("user2", Guid.NewGuid());
            user2Todos.Add(todo);
            await _context.Todos.AddAsync(todo);
        }

        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(t => t.UserId == "user1");

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().OnlyContain(t => t.UserId == "user1");
    }

    [Fact]
    public async Task GetCountAsync_WithoutExpression_ReturnsTotalCount()
    {
        // Arrange
        var todo1 = TestHelper.TestData.CreateTestTodo();
        var todo2 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        var todo3 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetCountAsync_WithExpression_ReturnsFilteredCount()
    {
        // Arrange
        var completedTodo = TestHelper.TestData.CreateTestTodo();
        completedTodo.IsCompleted = true;

        var incompleteTodo1 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        incompleteTodo1.IsCompleted = false;

        var incompleteTodo2 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        incompleteTodo2.IsCompleted = false;

        await _context.Todos.AddRangeAsync(completedTodo, incompleteTodo1, incompleteTodo2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetCountAsync(t => t.IsCompleted);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_AddsEntityToContext()
    {
        // Arrange
        var todo = TestHelper.TestData.CreateTestTodo();

        // Act
        await _repository.AddAsync(todo);

        // Assert
        _context.Todos.Local.Should().Contain(todo);
    }

    [Fact]
    public async Task AddRangeAsync_AddsMultipleEntitiesToContext()
    {
        // Arrange
        var todos = new List<Todo>
        {
            TestHelper.TestData.CreateTestTodo(),
            TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid()),
            TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid())
        };

        // Act
        await _repository.AddRangeAsync(todos);

        // Assert
        _context.Todos.Local.Should().HaveCount(3);
        _context.Todos.Local.Should().Contain(todos);
    }

    [Fact]
    public async Task Update_UpdatesEntityInContext()
    {
        // Arrange
        var todo = TestHelper.TestData.CreateTestTodo();
        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        var newTitle = "Updated Title";
        todo.Title = newTitle;

        // Act
        _repository.Update(todo);

        // Assert
        var updatedTodo = await _context.Todos.FindAsync(todo.Id);
        updatedTodo.Should().NotBeNull();
        updatedTodo!.Title.Should().Be(newTitle);
    }

    [Fact]
    public async Task Remove_MarksEntityForDeletion()
    {
        // Arrange
        var todo = TestHelper.TestData.CreateTestTodo();
        await _context.Todos.AddAsync(todo);
        await _context.SaveChangesAsync();

        // Get the tracked entity
        var trackedTodo = await _context.Todos.FindAsync(todo.Id);
        trackedTodo.Should().NotBeNull();

        // Act
        _repository.Remove(trackedTodo!);

        // Assert
        // After calling Remove, the entity should be marked for deletion
        // We can verify this by checking that SaveChangesAsync would process a deletion
        var result = await _context.SaveChangesAsync();
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RemoveRange_MarksEntitiesForDeletion()
    {
        // Arrange
        var todo1 = TestHelper.TestData.CreateTestTodo();
        var todo2 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());
        var todo3 = TestHelper.TestData.CreateTestTodo(id: Guid.NewGuid());

        await _context.Todos.AddRangeAsync(todo1, todo2, todo3);
        await _context.SaveChangesAsync();

        // Get tracked entities
        var trackedTodo1 = await _context.Todos.FindAsync(todo1.Id);
        var trackedTodo2 = await _context.Todos.FindAsync(todo2.Id);

        trackedTodo1.Should().NotBeNull();
        trackedTodo2.Should().NotBeNull();

        var todosToRemove = new List<Todo> { trackedTodo1!, trackedTodo2! };

        // Act
        _repository.RemoveRange(todosToRemove);

        // Assert
        // After calling RemoveRange, the entities should be marked for deletion
        // We can verify this by checking that SaveChangesAsync would process deletions
        var result = await _context.SaveChangesAsync();
        result.Should().BeGreaterThan(0);

        // Verify the entities were actually removed
        var remainingTodo = await _context.Todos.FindAsync(todo3.Id);
        remainingTodo.Should().NotBeNull(); // This one should still exist
    }
}