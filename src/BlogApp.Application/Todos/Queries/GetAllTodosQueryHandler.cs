namespace BlogApp.Application.Todos.Queries;

public class GetAllTodosQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetAllTodosQuery, IEnumerable<TodoDto>>
{
    private const int DefaultPageSize = 10;

    public async Task<IEnumerable<TodoDto>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await unitOfWork.Todos.GetAllWithUsersAsync();
        var allTodos = todos.ToList();

        if (request.Page.HasValue && request.PageSize.HasValue)
        {
            var pageSize = request.PageSize.Value == 0 ? DefaultPageSize : request.PageSize.Value;
            var skip = (request.Page.Value - 1) * pageSize;
            allTodos = allTodos.Skip(skip).Take(pageSize).ToList();
        }

        return allTodos.Select(todo => new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            CompletedAt = todo.CompletedAt,
            UserId = todo.UserId,
            UserName = todo.User != null ? $"{todo.User.FirstName} {todo.User.LastName}" : null
        });
    }
}