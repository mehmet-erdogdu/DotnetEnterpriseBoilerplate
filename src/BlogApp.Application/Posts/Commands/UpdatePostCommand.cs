namespace BlogApp.Application.Posts.Commands;

public class UpdatePostCommand : IRequest<PostDto?>
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
}

public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator(IMessageService messages)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(messages.GetMessage("IdRequired"));

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(messages.GetMessage("TitleRequired"))
            .MinimumLength(3).WithMessage(messages.GetMessage("TitleLength"))
            .MaximumLength(200).WithMessage(messages.GetMessage("TitleLength"))
            .Matches("^[a-zA-ZğüşıöçĞÜŞİÖÇ0-9\\s\\-_.,!?()]+$").WithMessage(messages.GetMessage("TitleInvalid"));

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage(messages.GetMessage("ContentRequired"))
            .MinimumLength(10).WithMessage(messages.GetMessage("ContentLength"))
            .MaximumLength(10000).WithMessage(messages.GetMessage("ContentLength"));
    }
}