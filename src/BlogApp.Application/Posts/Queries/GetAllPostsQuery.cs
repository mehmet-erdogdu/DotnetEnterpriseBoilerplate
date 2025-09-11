namespace BlogApp.Application.Posts.Queries;

public class GetAllPostsQuery : IRequest<IEnumerable<PostDto>>
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public string? Search { get; set; }
}

public class GetAllPostsQueryValidator : AbstractValidator<GetAllPostsQuery>
{
    public GetAllPostsQueryValidator(IMessageService messages)
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