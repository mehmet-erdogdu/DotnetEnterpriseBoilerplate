namespace BlogApp.Infrastructure.Repositories;

public class PasswordHistoryRepository(ApplicationDbContext context) : GenericRepository<PasswordHistory>(context), IPasswordHistoryRepository
{
    public async Task<IEnumerable<PasswordHistory>> GetPasswordHistoryByUserIdAsync(string userId)
    {
        return await _context.Set<PasswordHistory>()
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.ChangedAt)
            .ToListAsync();
    }

    public async Task<bool> IsPasswordRecentlyUsedAsync(string userId, string passwordHash)
    {
        var passwordHistoryCount = int.Parse(
            Environment.GetEnvironmentVariable("Security__PasswordHistoryCount") ?? "5");

        var recentPasswords = await _context.Set<PasswordHistory>()
            .Where(ph => ph.UserId == userId)
            .OrderByDescending(ph => ph.ChangedAt)
            .Take(passwordHistoryCount)
            .Select(ph => ph.PasswordHash)
            .ToListAsync();

        return recentPasswords.Contains(passwordHash);
    }

    public async Task<int> GetPasswordHistoryCountAsync(string userId)
    {
        return await _context.Set<PasswordHistory>()
            .CountAsync(ph => ph.UserId == userId);
    }
}