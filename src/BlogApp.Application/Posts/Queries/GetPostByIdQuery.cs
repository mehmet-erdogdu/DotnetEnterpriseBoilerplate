namespace BlogApp.Application.Posts.Queries;

public class GetPostByIdQuery : IRequest<PostDto?>
{
    public Guid Id { get; set; }
}

public class GetPostByIdQueryValidator : AbstractValidator<GetPostByIdQuery>
{
    public GetPostByIdQueryValidator(IMessageService messages)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage(messages.GetMessage("IdRequired"));
    }
}