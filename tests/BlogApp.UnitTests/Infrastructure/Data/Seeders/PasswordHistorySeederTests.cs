using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class PasswordHistorySeederTests : BaseTestClass
{
    public PasswordHistorySeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoPasswordHistoriesExistAndUsersExist_ShouldCreateSamplePasswordHistories()
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

        var seeder = new PasswordHistorySeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var passwordHistories = await _context.PasswordHistory.ToListAsync();
        passwordHistories.Should().HaveCount(3);

        var user1Histories = passwordHistories.Where(p => p.UserId == user1.Id).ToList();
        user1Histories.Should().HaveCount(2);

        var user2Histories = passwordHistories.Where(p => p.UserId == user2.Id).ToList();
        user2Histories.Should().HaveCount(1);

        // Verify password hashes are set
        foreach (var history in passwordHistories) history.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SeedAsync_WhenPasswordHistoriesAlreadyExist_ShouldNotCreateNewPasswordHistories()
    {
        // Arrange
        var existingPasswordHistory = new PasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = "user-id",
            PasswordHash = "existing-hash",
            ChangedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedById = "user-id"
        };

        _context!.PasswordHistory.Add(existingPasswordHistory);
        await _context.SaveChangesAsync();

        var seeder = new PasswordHistorySeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var passwordHistories = await _context.PasswordHistory.ToListAsync();
        passwordHistories.Should().HaveCount(1); // Should still be only 1 password history
        passwordHistories[0].PasswordHash.Should().Be("existing-hash");
    }

    [Fact]
    public async Task SeedAsync_WhenNoUsersExist_ShouldNotCreatePasswordHistories()
    {
        // Arrange
        var seeder = new PasswordHistorySeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var passwordHistories = await _context!.PasswordHistory.ToListAsync();
        passwordHistories.Should().BeEmpty();
    }
}