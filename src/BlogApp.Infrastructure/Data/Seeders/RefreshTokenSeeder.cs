namespace BlogApp.Infrastructure.Data.Seeders;

public class RefreshTokenSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if refresh tokens already exist
        if (await context.RefreshToken.AnyAsync())
            return;

        // Get users for refresh tokens
        var users = await context.Users.Take(2).ToListAsync();
        if (users.Count == 0)
            return;

        var refreshTokens = new List<RefreshToken>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Token = "sample-refresh-token-1",
                UserId = users[0].Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedById = users[0].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Token = "sample-refresh-token-2",
                UserId = users[1].Id,
                ExpiresAt = DateTime.UtcNow.AddDays(5),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                CreatedById = users[1].Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Token = "revoked-refresh-token-3",
                UserId = users[0].Id,
                ExpiresAt = DateTime.UtcNow.AddDays(3),
                IsRevoked = true,
                RevokedAt = DateTime.UtcNow.AddHours(-12),
                RevokedBy = users[0].Id,
                RevokedReason = "User requested token revocation",
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedById = users[0].Id
            }
        };

        context.RefreshToken.AddRange(refreshTokens);
        await context.SaveChangesAsync();
    }
}