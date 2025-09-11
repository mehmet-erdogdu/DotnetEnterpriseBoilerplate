namespace BlogApp.Application.Posts.Commands;

public class DeletePostCommand : IRequest<bool>
{
    public required Guid Id { get; set; }
}