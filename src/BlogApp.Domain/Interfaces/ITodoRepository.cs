namespace BlogApp.Domain.Interfaces;

public interface ITodoRepository : IGenericRepository<Todo>
{
    Task<IEnumerable<Todo>> GetTodosByUserIdAsync(string userId);
    Task<Todo?> GetTodoWithUserAsync(Guid todoId);
    Task<IEnumerable<Todo>> GetAllWithUsersAsync();
    Task<int> GetCountByUserIdAsync(string userId);
}