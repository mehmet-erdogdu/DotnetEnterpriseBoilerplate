namespace BlogApp.Infrastructure.Data.Seeders;

public class UserRoleSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if user roles already exist
        if (await context.UserRoles.AnyAsync())
            return;

        // Get users and roles
        var users = await context.Users.ToListAsync();
        var roles = await context.Roles.ToListAsync();

        if (users.Count == 0 || roles.Count == 0)
            return;

        // Find admin user and admin role
        var adminUser = users.FirstOrDefault(u => u.Email == "admin@example.com");
        var adminRole = roles.FirstOrDefault(r => r.Name == "Admin");

        // Find regular user and user role
        var regularUser = users.FirstOrDefault(u => u.Email == "user@example.com");
        var userRole = roles.FirstOrDefault(r => r.Name == "User");

        var userRoles = new List<IdentityUserRole<string>>();

        // Assign admin role to admin user
        if (adminUser != null && adminRole != null)
            userRoles.Add(new IdentityUserRole<string>
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });

        // Assign user role to regular user
        if (regularUser != null && userRole != null)
            userRoles.Add(new IdentityUserRole<string>
            {
                UserId = regularUser.Id,
                RoleId = userRole.Id
            });

        // Assign user role to admin user as well (admins can do everything users can do)
        if (adminUser != null && userRole != null)
            userRoles.Add(new IdentityUserRole<string>
            {
                UserId = adminUser.Id,
                RoleId = userRole.Id
            });

        context.UserRoles.AddRange(userRoles);
        await context.SaveChangesAsync();
    }
}