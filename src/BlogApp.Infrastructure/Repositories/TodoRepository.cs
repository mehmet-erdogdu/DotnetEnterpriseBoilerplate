namespace BlogApp.Infrastructure.Repositories;

public class TodoRepository(ApplicationDbContext context) : GenericRepository<Todo>(context), ITodoRepository
{
    public async Task<IEnumerable<Todo>> GetTodosByUserIdAsync(string userId)
    {
        return await context.Todos
            .Include(t => t.User)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Todo?> GetTodoWithUserAsync(Guid todoId)
    {
        return await context.Todos
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == todoId);
    }

    public async Task<IEnumerable<Todo>> GetAllWithUsersAsync()
    {
        return await context.Todos
            .Include(t => t.User)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(string userId)
    {
        return await context.Todos
            .CountAsync(t => t.UserId == userId);
    }
}