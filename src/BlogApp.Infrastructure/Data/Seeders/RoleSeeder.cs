namespace BlogApp.Infrastructure.Data.Seeders;

public class RoleSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if roles already exist
        if (context.Roles.Any())
            return;

        var roles = new List<IdentityRole>
        {
            new()
            {
                Id = "68059c38-a5e9-4e04-9ea9-d0102e53aa9b",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Moderator",
                NormalizedName = "MODERATOR",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        };

        context.Roles.AddRange(roles);
        await context.SaveChangesAsync();
    }
}