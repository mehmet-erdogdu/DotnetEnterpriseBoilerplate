namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ViewRoles")]
public class RolesController(
    IMediator mediator) : ControllerBase
{
    /// <summary>
    ///     Gets all roles
    /// </summary>
    /// <returns>List of roles</returns>
    /// <response code="200">Returns the list of roles</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<IEnumerable<RoleDto>>> GetAll()
    {
        var query = new GetAllRolesQuery();
        return await mediator.Send(query);
    }

    /// <summary>
    ///     Gets a role by ID
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Role details</returns>
    /// <response code="200">Returns the role details</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<RoleDto>> GetById(string id)
    {
        var query = new GetRoleByIdQuery { Id = id };
        return await mediator.Send(query);
    }

    /// <summary>
    ///     Creates a new role
    /// </summary>
    /// <param name="model">Role creation information</param>
    /// <returns>Created role details</returns>
    /// <response code="200">Returns the created role details</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<RoleDto>> Create([FromBody] CreateRoleCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<RoleDto>(ModelState);

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Updates an existing role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <param name="model">Role update information</param>
    /// <returns>Updated role details</returns>
    /// <response code="200">Returns the updated role details</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<RoleDto>> Update(string id, [FromBody] UpdateRoleCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<RoleDto>(ModelState);

        if (id != model.Id)
            return ApiResponse<RoleDto>.Failure("Invalid ID match");

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Deletes a role
    /// </summary>
    /// <param name="id">Role ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<string>> Delete(string id)
    {
        var command = new DeleteRoleCommand { Id = id };
        return await mediator.Send(command);
    }

    /// <summary>
    ///     Gets all claims for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <returns>List of role claims</returns>
    /// <response code="200">Returns the list of role claims</response>
    [HttpGet("{roleId}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<IEnumerable<RoleClaimDto>>> GetRoleClaims(string roleId)
    {
        var query = new GetRoleClaimsQuery { RoleId = roleId };
        return await mediator.Send(query);
    }

    /// <summary>
    ///     Gets a specific claim for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="claimId">Claim ID</param>
    /// <returns>Role claim details</returns>
    /// <response code="200">Returns the role claim details</response>
    [HttpGet("{roleId}/claims/{claimId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<RoleClaimDto>> GetRoleClaimById(string roleId, int claimId)
    {
        var query = new GetRoleClaimByIdQuery { RoleId = roleId, Id = claimId };
        return await mediator.Send(query);
    }

    /// <summary>
    ///     Adds a claim to a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="model">Role claim information</param>
    /// <returns>Created role claim details</returns>
    /// <response code="200">Returns the created role claim details</response>
    [HttpPost("{roleId}/claims")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<RoleClaimDto>> AddRoleClaim(string roleId, [FromBody] AddRoleClaimCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<RoleClaimDto>(ModelState);

        if (roleId != model.RoleId)
            return ApiResponse<RoleClaimDto>.Failure("Invalid Role ID match");

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Updates a claim for a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="claimId">Claim ID</param>
    /// <param name="model">Role claim update information</param>
    /// <returns>Updated role claim details</returns>
    /// <response code="200">Returns the updated role claim details</response>
    [HttpPut("{roleId}/claims/{claimId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<RoleClaimDto>> UpdateRoleClaim(string roleId, int claimId, [FromBody] UpdateRoleClaimCommand model)
    {
        if (!ModelState.IsValid)
            return this.CreateValidationErrorResponse<RoleClaimDto>(ModelState);

        if (roleId != model.RoleId)
            return ApiResponse<RoleClaimDto>.Failure("Invalid Role ID match");

        if (claimId != model.Id)
            return ApiResponse<RoleClaimDto>.Failure("Invalid Claim ID match");

        return await mediator.Send(model);
    }

    /// <summary>
    ///     Deletes a claim from a role
    /// </summary>
    /// <param name="roleId">Role ID</param>
    /// <param name="claimId">Claim ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Returns success message</response>
    [HttpDelete("{roleId}/claims/{claimId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ApiResponse<string>> DeleteRoleClaim(string roleId, int claimId)
    {
        var command = new DeleteRoleClaimCommand { RoleId = roleId, Id = claimId };
        return await mediator.Send(command);
    }
}