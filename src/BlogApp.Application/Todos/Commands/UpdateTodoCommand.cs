namespace BlogApp.Application.Todos.Commands;

public class UpdateTodoCommand : IRequest<TodoDto?>
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }
}

public class UpdateTodoCommandValidator : AbstractValidator<UpdateTodoCommand>
{
    public UpdateTodoCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(messages.GetMessage("IdRequired"));

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(messages.GetMessage("TitleRequired"))
            .MinimumLength(3).WithMessage(messages.GetMessage("TitleLength"))
            .MaximumLength(200).WithMessage(messages.GetMessage("TitleLength"))
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ0-9\\s\\-_.,!?()]+$").WithMessage(messages.GetMessage("TitleInvalid"));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage(messages.GetMessage("DescriptionMax"))
            .When(x => x.Description != null);
    }
}