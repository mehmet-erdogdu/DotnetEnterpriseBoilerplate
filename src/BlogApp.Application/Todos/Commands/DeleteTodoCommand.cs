namespace BlogApp.Application.Todos.Commands;

public class DeleteTodoCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}