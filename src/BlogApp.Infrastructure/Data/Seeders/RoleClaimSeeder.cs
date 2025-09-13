namespace BlogApp.Infrastructure.Data.Seeders;

public class RoleClaimSeeder : ISeeder
{
    private const string PermissionClaimType = "Permission";

    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if role claims already exist
        if (await context.RoleClaims.AnyAsync())
            return;

        // Get roles
        var roles = await context.Roles.ToListAsync();
        if (roles.Count == 0)
            return;

        var roleClaims = new List<IdentityRoleClaim<string>>();

        // Add claims to Admin role
        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");
        if (adminRole != null)
            roleClaims.AddRange(new[]
            {
                new IdentityRoleClaim<string>
                {
                    RoleId = adminRole.Id,
                    ClaimType = PermissionClaimType,
                    ClaimValue = "ViewUsers"
                },
                new IdentityRoleClaim<string>
                {
                    RoleId = adminRole.Id,
                    ClaimType = PermissionClaimType,
                    ClaimValue = "ViewFiles"
                },
                new IdentityRoleClaim<string>
                {
                    RoleId = adminRole.Id,
                    ClaimType = PermissionClaimType,
                    ClaimValue = "ViewRoles"
                }
            });

        // Add claims to User role
        var userRole = roles.FirstOrDefault(r => r.Name == "User");
        if (userRole != null)
            roleClaims.AddRange(new[]
            {
                new IdentityRoleClaim<string>
                {
                    RoleId = userRole.Id,
                    ClaimType = PermissionClaimType,
                    ClaimValue = "ViewFiles"
                }
            });

        context.RoleClaims.AddRange(roleClaims);
        await context.SaveChangesAsync();
    }
}