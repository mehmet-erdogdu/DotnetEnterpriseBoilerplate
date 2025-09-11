namespace BlogApp.Domain.Interfaces;

public interface IPasswordHistoryRepository : IGenericRepository<PasswordHistory>
{
    Task<IEnumerable<PasswordHistory>> GetPasswordHistoryByUserIdAsync(string userId);
    Task<bool> IsPasswordRecentlyUsedAsync(string userId, string passwordHash);
    Task<int> GetPasswordHistoryCountAsync(string userId);
}