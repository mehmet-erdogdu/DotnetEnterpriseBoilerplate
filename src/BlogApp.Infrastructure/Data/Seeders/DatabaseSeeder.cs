using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlogApp.Infrastructure.Data.Seeders;

public abstract class DatabaseSeeder
{
    public static async Task SeedDatabaseAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<DatabaseSeeder>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Run migrations
            await context.Database.MigrateAsync();

            // Run seeders in order
            var seeders = new ISeeder[]
            {
                new UserSeeder(),
                new RoleSeeder(),
                new UserRoleSeeder(),
                new RoleClaimSeeder(),
                new PostSeeder(),
                new TodoSeeder(),
                new PasswordHistorySeeder(),
                new RefreshTokenSeeder()
            };

            foreach (var seeder in seeders)
            {
                await seeder.SeedAsync(context);
                logger.LogInformation("Seeded {SeederName}", seeder.GetType().Name);
            }

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            // Use InvalidOperationException instead of generic Exception to address SonarQube issue S112
            throw new InvalidOperationException("Database seeding failed. See inner exception for details.", ex);
        }
    }
}