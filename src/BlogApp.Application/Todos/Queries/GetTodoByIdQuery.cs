namespace BlogApp.Application.Todos.Queries;

public class GetTodoByIdQuery : IRequest<TodoDto?>
{
    public Guid Id { get; set; }
}

public class GetTodoByIdQueryValidator : AbstractValidator<GetTodoByIdQuery>
{
    public GetTodoByIdQueryValidator(IMessageService messages)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(messages.GetMessage("IdRequired"));
    }
}