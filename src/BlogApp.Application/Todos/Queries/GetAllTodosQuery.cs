namespace BlogApp.Application.Todos.Queries;

public class GetAllTodosQuery : IRequest<IEnumerable<TodoDto>>
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
}

public class GetAllTodosQueryValidator : AbstractValidator<GetAllTodosQuery>
{
    public GetAllTodosQueryValidator(IMessageService messages)
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage(messages.GetMessage("PageMin"))
            .When(x => x.Page.HasValue);

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage(messages.GetMessage("PageSizeMin"))
            .LessThanOrEqualTo(100).WithMessage(messages.GetMessage("PageSizeMax"))
            .When(x => x.PageSize.HasValue);
    }
}