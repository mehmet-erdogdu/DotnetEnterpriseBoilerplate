namespace BlogApp.Infrastructure.Data.Seeders;

public class RoleClaimSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if role claims already exist
        if (context.RoleClaims.Any())
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
                    ClaimType = "Permission",
                    ClaimValue = "ViewUsers"
                },
                new IdentityRoleClaim<string>
                {
                    RoleId = adminRole.Id,
                    ClaimType = "Permission",
                    ClaimValue = "ViewFiles"
                },
                new IdentityRoleClaim<string>
                {
                    RoleId = adminRole.Id,
                    ClaimType = "Permission",
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
                    ClaimType = "Permission",
                    ClaimValue = "ViewFiles"
                }
            });

        context.RoleClaims.AddRange(roleClaims);
        await context.SaveChangesAsync();
    }
}