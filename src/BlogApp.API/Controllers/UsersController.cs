using BlogApp.Application.Users.Commands;
using BlogApp.Application.Users.Queries;

namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ViewUsers")]
public class UsersController(
    IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Gets all users with pagination and search
    /// </summary>
    /// <param name="model">Pagination and search parameters</param>
    /// <returns>List of users</returns>
    /// <response code="200">Returns the list of users</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<IEnumerable<UserDto>>> GetAll([FromQuery] GetAllUsersQuery model)
    {
        return await mediator.Send(model);
    }

    /// <summary>
    ///     Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    /// <response code="200">Returns the user details</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<UserDto>> GetById(string id)
    {
        var query = new GetUserByIdQuery { Id = id };
        return await mediator.Send(query);
    }

    /// <summary>
    ///     Creates a new user
    /// </summary>
    /// <param name="model">User creation information</param>
    /// <returns>Created user details</returns>
    /// <response code="200">Returns the created user details</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<UserDto>> Create([FromBody] CreateUserCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<UserDto>(ModelState);

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Updates an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="model">User update information</param>
    /// <returns>Updated user details</returns>
    /// <response code="200">Returns the updated user details</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<UserDto>> Update(string id, [FromBody] UpdateUserCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<UserDto>(ModelState);

        if (id != model.Id)
            return ApiResponse<UserDto>.Failure("Invalid ID match");

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Deletes a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        var command = new DeleteUserCommand { Id = id };
        return await mediator.Send(command);
    }

    /// <summary>
    ///     Assigns a role to a user
    /// </summary>
    /// <param name="model">Role assignment information</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    [HttpPost("assign-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<string>> AssignRole([FromBody] AssignRoleCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<string>(ModelState);

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Removes a role from a user
    /// </summary>
    /// <param name="model">Role removal information</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    [HttpPost("remove-role")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<string>> RemoveRole([FromBody] RemoveRoleCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<string>(ModelState);

        return await mediator.Send(model);
    }
}