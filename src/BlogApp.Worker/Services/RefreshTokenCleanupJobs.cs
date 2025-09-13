namespace BlogApp.Worker.Services;

[ExcludeFromCodeCoverage]
public class RefreshTokenCleanupJobs(
    IRefreshTokenService refreshTokenService,
    ILogger<RefreshTokenCleanupJobs> logger)
{
    public async Task Run()
    {
        await refreshTokenService.CleanupExpiredTokensAsync();
        logger.LogInformation("Hangfire cleanup executed at {Time}", DateTimeOffset.Now);
    }
}