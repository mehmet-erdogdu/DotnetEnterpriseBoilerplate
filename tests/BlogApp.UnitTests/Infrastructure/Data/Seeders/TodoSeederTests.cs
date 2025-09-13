using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class TodoSeederTests : BaseTestClass
{
    public TodoSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoTodosExistAndUsersExist_ShouldCreateSampleTodos()
    {
        // Arrange
        var user1 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user1@example.com",
            Email = "user1@example.com",
            FirstName = "User",
            LastName = "One",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        var user2 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user2@example.com",
            Email = "user2@example.com",
            FirstName = "User",
            LastName = "Two",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        _context!.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var seeder = new TodoSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var todos = await _context.Todos.ToListAsync();
        todos.Should().HaveCount(3);

        var completedTodo = todos.FirstOrDefault(t => t.Title == "Complete project setup");
        completedTodo.Should().NotBeNull();
        completedTodo!.IsCompleted.Should().BeTrue();
        completedTodo.UserId.Should().Be(user1.Id);

        var pendingTodo = todos.FirstOrDefault(t => t.Title == "Write documentation");
        pendingTodo.Should().NotBeNull();
        pendingTodo!.IsCompleted.Should().BeFalse();
        pendingTodo.UserId.Should().Be(user1.Id);
    }

    [Fact]
    public async Task SeedAsync_WhenTodosAlreadyExist_ShouldNotCreateNewTodos()
    {
        // Arrange
        var existingTodo = new Todo
        {
            Id = Guid.NewGuid(),
            Title = "Existing Todo",
            Description = "This is an existing todo",
            IsCompleted = false,
            UserId = "user-id",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "user-id"
        };

        _context!.Todos.Add(existingTodo);
        await _context.SaveChangesAsync();

        var seeder = new TodoSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var todos = await _context.Todos.ToListAsync();
        todos.Should().HaveCount(1); // Should still be only 1 todo
        todos[0].Title.Should().Be("Existing Todo");
    }

    [Fact]
    public async Task SeedAsync_WhenNoUsersExist_ShouldNotCreateTodos()
    {
        // Arrange
        var seeder = new TodoSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var todos = await _context!.Todos.ToListAsync();
        todos.Should().BeEmpty();
    }
}