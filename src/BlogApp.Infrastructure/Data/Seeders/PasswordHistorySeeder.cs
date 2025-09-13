namespace BlogApp.Infrastructure.Data.Seeders;

public class PasswordHistorySeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if password histories already exist
        if (context.PasswordHistory.Any())
            return;

        // Get users for password history
        var users = await context.Users.Take(2).ToListAsync();
        if (users.Count == 0)
            return;

        var hasher = new PasswordHasher<ApplicationUser>();

        var passwordHistories = new List<PasswordHistory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = users[0].Id,
                PasswordHash = hasher.HashPassword(users[0], "OldPassword123!"),
                ChangedAt = DateTime.UtcNow.AddDays(-30),
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                CreatedById = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = users[0].Id,
                PasswordHash = hasher.HashPassword(users[0], "OlderPassword456@"),
                ChangedAt = DateTime.UtcNow.AddDays(-60),
                CreatedAt = DateTime.UtcNow.AddDays(-60),
                CreatedById = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                UserId = users[1].Id,
                PasswordHash = hasher.HashPassword(users[1], "UserOldPassword789#"),
                ChangedAt = DateTime.UtcNow.AddDays(-45),
                CreatedAt = DateTime.UtcNow.AddDays(-45),
                CreatedById = users[1].Id
            }
        };

        context.PasswordHistory.AddRange(passwordHistories);
        await context.SaveChangesAsync();
    }
}