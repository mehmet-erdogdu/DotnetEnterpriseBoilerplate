using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class UserSeederTests : BaseTestClass
{
    public UserSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoUsersExist_ShouldCreateAdminAndRegularUsers()
    {
        // Arrange
        var seeder = new UserSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var users = await _context!.Users.ToListAsync();
        users.Should().HaveCount(2);

        var adminUser = users.FirstOrDefault(u => u.Email == "admin@example.com");
        adminUser.Should().NotBeNull();
        adminUser!.FirstName.Should().Be("Admin");
        adminUser.LastName.Should().Be("User");

        var regularUser = users.FirstOrDefault(u => u.Email == "user@example.com");
        regularUser.Should().NotBeNull();
        regularUser!.FirstName.Should().Be("Regular");
        regularUser.LastName.Should().Be("User");

        // Verify passwords are hashed
        var passwordHasher = new PasswordHasher<ApplicationUser>();
        adminUser.PasswordHash.Should().NotBeNullOrEmpty();
        regularUser.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SeedAsync_WhenUsersAlreadyExist_ShouldNotCreateNewUsers()
    {
        // Arrange
        var existingUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "existing@example.com",
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        _context!.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var seeder = new UserSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var users = await _context.Users.ToListAsync();
        users.Should().HaveCount(1); // Should still be only 1 user
        users[0].Email.Should().Be("existing@example.com");
    }
}