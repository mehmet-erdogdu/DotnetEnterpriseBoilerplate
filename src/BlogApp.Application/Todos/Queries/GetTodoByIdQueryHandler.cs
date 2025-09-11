namespace BlogApp.Application.Todos.Queries;

public class GetTodoByIdQueryHandler(IUnitOfWork unitOfWork) : IRequestHandler<GetTodoByIdQuery, TodoDto?>
{
    public async Task<TodoDto?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await unitOfWork.Todos.GetTodoWithUserAsync(request.Id);

        if (todo == null)
            return null;

        return new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt,
            CompletedAt = todo.CompletedAt,
            UserId = todo.UserId,
            UserName = todo.User != null ? $"{todo.User.FirstName} {todo.User.LastName}" : null
        };
    }
}