namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController(
    IMediator mediator,
    IMessageService messageService,
    IRabbitMqPublisher publisher) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IEnumerable<PostDto>>> GetAll([FromQuery] PaginationDto model)
    {
        var query = new GetAllPostsQuery
        {
            Page = model.Page,
            PageSize = model.PageSize,
            Search = model.Search
        };
        var result = await mediator.Send(query);
        return ApiResponse<IEnumerable<PostDto>>.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<PostDto>> GetById(Guid id)
    {
        var query = new GetPostByIdQuery { Id = id };
        var result = await mediator.Send(query);

        return result == null
            ? ApiResponse<PostDto>.Failure(messageService.GetMessage(ErrorMessageConstants.PostNotFound, id))
            : ApiResponse<PostDto>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResponse<PostDto>> Create([FromBody] CreatePostCommand command)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<PostDto>(ModelState);

        var result = await mediator.Send(command);

        // Publish event after successful creation
        await publisher.PublishAsync("blogapp.exchange", "post.created", new
        {
            id = result.Id, title = result.Title, createdAt = DateTime.UtcNow
        });

        return ApiResponse<PostDto>.Success(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse<string>> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return ApiResponse<string>.Failure(messageService.GetMessage("UnauthorizedAccess"));

        // Get the post to check ownership
        var postQuery = new GetPostByIdQuery { Id = id };
        var post = await mediator.Send(postQuery);

        if (post == null)
            return ApiResponse<string>.Failure(messageService.GetMessage(ErrorMessageConstants.PostNotFound, id));

        if (post.AuthorId != userId)
            return ApiResponse<string>.Failure(messageService.GetMessage("ForbiddenAccess"));

        var command = new DeletePostCommand { Id = id };
        var result = await mediator.Send(command);

        return result
            ? ApiResponse<string>.Success(messageService.GetMessage("Success"))
            : ApiResponse<string>.Failure(messageService.GetMessage(ErrorMessageConstants.PostNotFound, id));
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResponse<PostDto>> Update(Guid id, [FromBody] UpdatePostCommand command)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<PostDto>(ModelState);

        if (id != command.Id)
            return ApiResponse<PostDto>.Failure(messageService.GetMessage("InvalidIdMatch"));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get the post to check ownership
        var postQuery = new GetPostByIdQuery { Id = id };
        var post = await mediator.Send(postQuery);

        if (post == null)
            return ApiResponse<PostDto>.Failure(messageService.GetMessage(ErrorMessageConstants.PostNotFound, id));

        if (post.AuthorId != userId)
            return ApiResponse<PostDto>.Failure(messageService.GetMessage("ForbiddenAccess"));

        var result = await mediator.Send(command);
        return result == null
            ? ApiResponse<PostDto>.Failure(messageService.GetMessage(ErrorMessageConstants.PostNotFound, id))
            : ApiResponse<PostDto>.Success(result);
    }
}