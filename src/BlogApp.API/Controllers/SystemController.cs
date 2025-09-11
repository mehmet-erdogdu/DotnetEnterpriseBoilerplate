namespace BlogApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemController(
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("secret/configuration/value")]
    public ApiResponse<Dictionary<string, string?>> Get()
    {
        var value = configuration.GetSection("React").GetChildren()
            .ToDictionary(x => x.Key, x => x.Value);
        return ApiResponse<Dictionary<string, string?>>.Success(value);
    }
}