namespace BlogApp.Application.Todos.Commands;

public class CreateTodoCommand : IRequest<TodoDto>
{
    public required string Title { get; set; }
    public string? Description { get; set; }
    public required string UserId { get; set; }
}

public class CreateTodoCommandValidator : AbstractValidator<CreateTodoCommand>
{
    public CreateTodoCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(messages.GetMessage("TitleRequired"))
            .MinimumLength(3).WithMessage(messages.GetMessage("TitleLength"))
            .MaximumLength(200).WithMessage(messages.GetMessage("TitleLength"))
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ0-9\\s\\-_.,!?()]+$").WithMessage(messages.GetMessage("TitleInvalid"));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage(messages.GetMessage("DescriptionMax"))
            .When(x => x.Description != null);

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage(messages.GetMessage("UserIdRequired"));
    }
}