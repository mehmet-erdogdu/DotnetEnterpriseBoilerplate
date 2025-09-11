namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController(
    IMediator mediator,
    IMessageService messageService) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IEnumerable<TodoDto>>> GetAll([FromQuery] int? page, [FromQuery] int? pageSize)
    {
        var query = new GetAllTodosQuery
        {
            Page = page,
            PageSize = pageSize
        };
        var result = await mediator.Send(query);
        return ApiResponse<IEnumerable<TodoDto>>.Success(result);
    }

    [HttpGet("{id}")]
    public async Task<ApiResponse<TodoDto>> GetById(Guid id)
    {
        var query = new GetTodoByIdQuery { Id = id };
        var result = await mediator.Send(query);

        return result == null
            ? ApiResponse<TodoDto>.Failure(messageService.GetMessage(ErrorMessageConstants.TodoNotFound, id))
            : ApiResponse<TodoDto>.Success(result);
    }

    [HttpPost]
    public async Task<ApiResponse<TodoDto>> Create([FromBody] CreateTodoCommand command)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<TodoDto>(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        command.UserId = userId;
        var result = await mediator.Send(command);
        return ApiResponse<TodoDto>.Success(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ApiResponse<string>> Delete(Guid id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var todoQuery = new GetTodoByIdQuery { Id = id };
        var todo = await mediator.Send(todoQuery);

        if (todo == null)
            return ApiResponse<string>.Failure(messageService.GetMessage(ErrorMessageConstants.TodoNotFound, id));

        if (todo.UserId != userId)
            return ApiResponse<string>.Failure(messageService.GetMessage("ForbiddenAccess"));

        var command = new DeleteTodoCommand { Id = id };
        var result = await mediator.Send(command);

        return result
            ? ApiResponse<string>.Success(messageService.GetMessage("Success"))
            : ApiResponse<string>.Failure(messageService.GetMessage(ErrorMessageConstants.TodoNotFound, id));
    }

    [HttpPut("{id:guid}")]
    public async Task<ApiResponse<TodoDto>> Update(Guid id, [FromBody] UpdateTodoCommand command)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<TodoDto>(ModelState);

        if (id != command.Id)
            return ApiResponse<TodoDto>.Failure(messageService.GetMessage("InvalidIdMatch"));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var todoQuery = new GetTodoByIdQuery { Id = id };
        var todo = await mediator.Send(todoQuery);

        if (todo == null)
            return ApiResponse<TodoDto>.Failure(messageService.GetMessage(ErrorMessageConstants.TodoNotFound, id));

        if (todo.UserId != userId)
            return ApiResponse<TodoDto>.Failure(messageService.GetMessage("ForbiddenAccess"));

        var result = await mediator.Send(command);
        return result == null
            ? ApiResponse<TodoDto>.Failure(messageService.GetMessage(ErrorMessageConstants.TodoNotFound, id))
            : ApiResponse<TodoDto>.Success(result);
    }
}