namespace BlogApp.Application.Todos.Commands;

public class CreateTodoCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateTodoCommand, TodoDto>
{
    public async Task<TodoDto> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var todo = new Todo
            {
                Title = request.Title,
                Description = request.Description,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            await unitOfWork.Todos.AddAsync(todo);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            var todoWithUser = await unitOfWork.Todos.GetTodoWithUserAsync(todo.Id);
            return new TodoDto
            {
                Id = todo.Id,
                Title = todo.Title,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                CreatedAt = todo.CreatedAt,
                CompletedAt = todo.CompletedAt,
                UserId = todo.UserId,
                UserName = todoWithUser?.User != null ? $"{todoWithUser.User.FirstName} {todoWithUser.User.LastName}" : null
            };
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}