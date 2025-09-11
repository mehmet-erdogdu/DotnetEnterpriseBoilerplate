namespace BlogApp.Infrastructure.Services;

public class PasswordService(IPasswordHistoryRepository passwordHistoryRepository, IUnitOfWork unitOfWork) : IPasswordService
{
    public async Task TrackPasswordChangeAsync(string userId, string passwordHash)
    {
        var passwordHistory = new PasswordHistory
        {
            UserId = userId,
            PasswordHash = passwordHash,
            ChangedAt = DateTime.UtcNow
        };

        await passwordHistoryRepository.AddAsync(passwordHistory);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> IsPasswordRecentlyUsedAsync(string userId, string passwordHash)
    {
        return await passwordHistoryRepository.IsPasswordRecentlyUsedAsync(userId, passwordHash);
    }

    public async Task<int> GetPasswordHistoryCountAsync(string userId)
    {
        return await passwordHistoryRepository.GetPasswordHistoryCountAsync(userId);
    }
}

public interface IPasswordService
{
    Task TrackPasswordChangeAsync(string userId, string passwordHash);
    Task<bool> IsPasswordRecentlyUsedAsync(string userId, string passwordHash);
    Task<int> GetPasswordHistoryCountAsync(string userId);
}