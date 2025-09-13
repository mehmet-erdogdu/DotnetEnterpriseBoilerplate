using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class RoleSeederTests : BaseTestClass
{
    public RoleSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoRolesExist_ShouldCreateDefaultRoles()
    {
        // Arrange
        var seeder = new RoleSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var roles = await _context!.Roles.ToListAsync();
        roles.Should().HaveCount(3);

        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
        adminRole.Should().NotBeNull();
        adminRole!.NormalizedName.Should().Be("ADMIN");

        var userRole = roles.FirstOrDefault(r => r.Name == "User");
        userRole.Should().NotBeNull();
        userRole!.NormalizedName.Should().Be("USER");

        var moderatorRole = roles.FirstOrDefault(r => r.Name == "Moderator");
        moderatorRole.Should().NotBeNull();
        moderatorRole!.NormalizedName.Should().Be("MODERATOR");
    }

    [Fact]
    public async Task SeedAsync_WhenRolesAlreadyExist_ShouldNotCreateNewRoles()
    {
        // Arrange
        var existingRole = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "ExistingRole",
            NormalizedName = "EXISTINGROLE",
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        _context!.Roles.Add(existingRole);
        await _context.SaveChangesAsync();

        var seeder = new RoleSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var roles = await _context.Roles.ToListAsync();
        roles.Should().HaveCount(1); // Should still be only 1 role
        roles[0].Name.Should().Be("ExistingRole");
    }
}