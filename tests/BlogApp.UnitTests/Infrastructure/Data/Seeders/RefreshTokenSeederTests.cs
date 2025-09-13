using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class RefreshTokenSeederTests : BaseTestClass
{
    public RefreshTokenSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoRefreshTokensExistAndUsersExist_ShouldCreateSampleRefreshTokens()
    {
        // Arrange
        var user1 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user1@example.com",
            Email = "user1@example.com",
            FirstName = "User",
            LastName = "One",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        var user2 = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user2@example.com",
            Email = "user2@example.com",
            FirstName = "User",
            LastName = "Two",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        _context!.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var seeder = new RefreshTokenSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var refreshTokens = await _context.RefreshToken.ToListAsync();
        refreshTokens.Should().HaveCount(3);

        var activeTokens = refreshTokens.Where(t => !t.IsRevoked).ToList();
        activeTokens.Should().HaveCount(2);

        var revokedToken = refreshTokens.FirstOrDefault(t => t.IsRevoked);
        revokedToken.Should().NotBeNull();
        revokedToken!.RevokedBy.Should().Be(user1.Id);
        revokedToken.RevokedReason.Should().Be("User requested token revocation");
    }

    [Fact]
    public async Task SeedAsync_WhenRefreshTokensAlreadyExist_ShouldNotCreateNewRefreshTokens()
    {
        // Arrange
        var existingRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = "existing-token",
            UserId = "user-id",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            CreatedById = "user-id"
        };

        _context!.RefreshToken.Add(existingRefreshToken);
        await _context.SaveChangesAsync();

        var seeder = new RefreshTokenSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var refreshTokens = await _context.RefreshToken.ToListAsync();
        refreshTokens.Should().HaveCount(1); // Should still be only 1 refresh token
        refreshTokens[0].Token.Should().Be("existing-token");
    }

    [Fact]
    public async Task SeedAsync_WhenNoUsersExist_ShouldNotCreateRefreshTokens()
    {
        // Arrange
        var seeder = new RefreshTokenSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var refreshTokens = await _context!.RefreshToken.ToListAsync();
        refreshTokens.Should().BeEmpty();
    }
}