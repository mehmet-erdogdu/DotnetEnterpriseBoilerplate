namespace BlogApp.Application.Todos.Commands;

public class DeleteTodoCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<DeleteTodoCommand, bool>
{
    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync();

            var todo = await unitOfWork.Todos.GetByIdAsync(request.Id);
            if (todo == null)
                return false;

            unitOfWork.Todos.Remove(todo);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitAsync();

            return true;
        }
        catch
        {
            await unitOfWork.RollbackAsync();
            throw;
        }
    }
}