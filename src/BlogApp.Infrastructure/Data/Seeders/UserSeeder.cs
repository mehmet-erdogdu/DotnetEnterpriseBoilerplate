namespace BlogApp.Infrastructure.Data.Seeders;

public class UserSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if users already exist
        if (context.Users.Any())
            return;

        var hasher = new PasswordHasher<ApplicationUser>();

        // Create admin user
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "admin@example.com",
            NormalizedUserName = "ADMIN@EXAMPLE.COM",
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

        // Create regular user
        var regularUser = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "user@example.com",
            NormalizedUserName = "USER@EXAMPLE.COM",
            Email = "user@example.com",
            NormalizedEmail = "USER@EXAMPLE.COM",
            FirstName = "Regular",
            LastName = "User",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = "system"
        };

        regularUser.PasswordHash = hasher.HashPassword(regularUser, "User123!");

        context.Users.AddRange(adminUser, regularUser);
        await context.SaveChangesAsync();
    }
}