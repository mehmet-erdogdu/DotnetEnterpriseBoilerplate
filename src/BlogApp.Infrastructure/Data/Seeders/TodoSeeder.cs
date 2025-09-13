namespace BlogApp.Infrastructure.Data.Seeders;

public class TodoSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if todos already exist
        if (await context.Todos.AnyAsync())
            return;

        // Get users
        var users = await context.Users.Take(2).ToListAsync();
        if (users.Count == 0)
            return;

        var todos = new List<Todo>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Complete project setup",
                Description = "Finish setting up the development environment and project structure",
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow.AddDays(-5),
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                CreatedById = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Write documentation",
                Description = "Create comprehensive documentation for the application",
                IsCompleted = false,
                UserId = users[0].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                CreatedById = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Review code",
                Description = "Review the codebase for potential improvements",
                IsCompleted = false,
                UserId = users[1].Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedById = users[1].Id
            }
        };

        context.Todos.AddRange(todos);
        await context.SaveChangesAsync();
    }
}