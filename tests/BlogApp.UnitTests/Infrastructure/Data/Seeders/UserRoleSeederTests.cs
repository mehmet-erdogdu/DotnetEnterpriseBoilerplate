using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class UserRoleSeederTests : BaseTestClass
{
    public UserRoleSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoUserRolesExistAndUsersAndRolesExist_ShouldCreateUserRoles()
    {
        // Arrange
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "admin@example.com",
            Email = "admin@example.com",
            FirstName = "Admin",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        var regularUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user@example.com",
            Email = "user@example.com",
            FirstName = "Regular",
            LastName = "User",
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        var adminRole = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Admin",
            NormalizedName = "ADMIN"
        };

        var userRole = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User",
            NormalizedName = "USER"
        };

        _context!.Users.AddRange(adminUser, regularUser);
        _context.Roles.AddRange(adminRole, userRole);
        await _context.SaveChangesAsync();

        var seeder = new UserRoleSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var userRoles = await _context.UserRoles.ToListAsync();
        userRoles.Should().HaveCount(3); // Admin gets both Admin and User roles, Regular user gets User role

        var adminUserRoles = userRoles.Where(ur => ur.UserId == adminUser.Id).ToList();
        adminUserRoles.Should().HaveCount(2);

        var regularUserRoles = userRoles.Where(ur => ur.UserId == regularUser.Id).ToList();
        regularUserRoles.Should().HaveCount(1);
        regularUserRoles[0].RoleId.Should().Be(userRole.Id);
    }

    [Fact]
    public async Task SeedAsync_WhenUserRolesAlreadyExist_ShouldNotCreateNewUserRoles()
    {
        // Arrange
        var existingUserRole = new IdentityUserRole<string>
        {
            UserId = "user-id",
            RoleId = "role-id"
        };

        _context!.UserRoles.Add(existingUserRole);
        await _context.SaveChangesAsync();

        var seeder = new UserRoleSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var userRoles = await _context.UserRoles.ToListAsync();
        userRoles.Should().HaveCount(1); // Should still be only 1 user role
        userRoles[0].UserId.Should().Be("user-id");
    }

    [Fact]
    public async Task SeedAsync_WhenNoUsersOrRolesExist_ShouldNotCreateUserRoles()
    {
        // Arrange
        var seeder = new UserRoleSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var userRoles = await _context!.UserRoles.ToListAsync();
        userRoles.Should().BeEmpty();
    }
}