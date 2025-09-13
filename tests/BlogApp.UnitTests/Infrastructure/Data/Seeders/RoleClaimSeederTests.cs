using BlogApp.Infrastructure.Data.Seeders;

namespace BlogApp.UnitTests.Infrastructure.Data.Seeders;

public class RoleClaimSeederTests : BaseTestClass
{
    public RoleClaimSeederTests()
    {
        _context = CreateDbContext();
    }

    [Fact]
    public async Task SeedAsync_WhenNoRoleClaimsExistAndRolesExist_ShouldCreateRoleClaims()
    {
        // Arrange
        var adminRole = new IdentityRole
        {
            Id = "68059c38-a5e9-4e04-9ea9-d0102e53aa9b",
            Name = "Admin",
            NormalizedName = "ADMIN"
        };

        var userRole = new IdentityRole
        {
            Id = Guid.NewGuid().ToString(),
            Name = "User",
            NormalizedName = "USER"
        };

        _context!.Roles.AddRange(adminRole, userRole);
        await _context.SaveChangesAsync();

        var seeder = new RoleClaimSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var roleClaims = await _context.RoleClaims.ToListAsync();
        roleClaims.Should().HaveCount(4); // 3 claims for Admin role, 1 claim for User role

        var adminRoleClaims = roleClaims.Where(rc => rc.RoleId == adminRole.Id).ToList();
        adminRoleClaims.Should().HaveCount(3);
        adminRoleClaims.Should().Contain(rc => rc.ClaimType == "Permission" && rc.ClaimValue == "ViewUsers");
        adminRoleClaims.Should().Contain(rc => rc.ClaimType == "Permission" && rc.ClaimValue == "ViewFiles");
        adminRoleClaims.Should().Contain(rc => rc.ClaimType == "Permission" && rc.ClaimValue == "ViewRoles");

        var userRoleClaims = roleClaims.Where(rc => rc.RoleId == userRole.Id).ToList();
        userRoleClaims.Should().HaveCount(1);
        userRoleClaims[0].ClaimType.Should().Be("Permission");
        userRoleClaims[0].ClaimValue.Should().Be("ViewFiles");
    }

    [Fact]
    public async Task SeedAsync_WhenRoleClaimsAlreadyExist_ShouldNotCreateNewRoleClaims()
    {
        // Arrange
        var existingRoleClaim = new IdentityRoleClaim<string>
        {
            RoleId = "role-id",
            ClaimType = "ExistingClaimType",
            ClaimValue = "ExistingClaimValue"
        };

        _context!.RoleClaims.Add(existingRoleClaim);
        await _context.SaveChangesAsync();

        var seeder = new RoleClaimSeeder();

        // Act
        await seeder.SeedAsync(_context);

        // Assert
        var roleClaims = await _context.RoleClaims.ToListAsync();
        roleClaims.Should().HaveCount(1); // Should still be only 1 role claim
        roleClaims[0].ClaimType.Should().Be("ExistingClaimType");
    }

    [Fact]
    public async Task SeedAsync_WhenNoRolesExist_ShouldNotCreateRoleClaims()
    {
        // Arrange
        var seeder = new RoleClaimSeeder();

        // Act
        await seeder.SeedAsync(_context!);

        // Assert
        var roleClaims = await _context!.RoleClaims.ToListAsync();
        roleClaims.Should().BeEmpty();
    }
}