namespace BlogApp.Application.Todos.Commands;

public class UpdateTodoCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<UpdateTodoCommand, TodoDto?>
{
    public async Task<TodoDto?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var todo = await unitOfWork.Todos.GetTodoWithUserAsync(request.Id);
            if (todo == null)
                return null;

            todo.Title = request.Title;
            todo.Description = request.Description;

            if (!todo.IsCompleted && request.IsCompleted)
            {
                todo.IsCompleted = true;
                todo.CompletedAt = DateTime.UtcNow;
            }
            else if (todo.IsCompleted && !request.IsCompleted)
            {
                todo.IsCompleted = false;
                todo.CompletedAt = null;
            }

            unitOfWork.Todos.Update(todo);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

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
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}