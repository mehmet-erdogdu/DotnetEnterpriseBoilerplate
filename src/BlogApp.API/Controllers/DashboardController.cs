namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(
    IMediator mediator,
    ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("statistics")]
    public async Task<ApiResponse<DashboardStatisticsDto>> GetStatistics()
    {
        var postsCount = await mediator.Send(new GetPostsCountQuery(currentUserService.UserId));
        var todosCount = await mediator.Send(new GetTodosCountQuery(currentUserService.UserId));
        var filesCount = await mediator.Send(new GetFilesCountQuery(currentUserService.UserId));

        var statistics = new DashboardStatisticsDto
        {
            PostsCount = postsCount,
            TodosCount = todosCount,
            FilesCount = filesCount
        };

        return ApiResponse<DashboardStatisticsDto>.Success(statistics);
    }
}