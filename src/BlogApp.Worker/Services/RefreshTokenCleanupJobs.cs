namespace BlogApp.Worker.Services;

public class RefreshTokenCleanupJobs(
    IRefreshTokenService refreshTokenService,
    ILogger<RefreshTokenCleanupJobs> logger)
{
    [TickerFunction("RefreshTokenCleanup", "0 3 * * *")] // default 03:00
    public async Task Run()
    {
        await refreshTokenService.CleanupExpiredTokensAsync();
        logger.LogInformation("TickerQ cleanup executed at {Time}", DateTimeOffset.Now);
    }
}