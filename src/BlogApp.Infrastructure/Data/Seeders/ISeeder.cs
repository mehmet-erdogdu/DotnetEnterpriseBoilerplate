namespace BlogApp.Infrastructure.Data.Seeders;

public interface ISeeder
{
    Task SeedAsync(ApplicationDbContext context);
}