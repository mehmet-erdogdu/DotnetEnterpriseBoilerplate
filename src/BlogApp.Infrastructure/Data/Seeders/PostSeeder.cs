namespace BlogApp.Infrastructure.Data.Seeders;

public class PostSeeder : ISeeder
{
    public async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if posts already exist
        if (await context.Posts.AnyAsync())
            return;

        // Get the first user as author
        var author = await context.Users.FirstOrDefaultAsync();
        if (author == null)
            return;

        var posts = new List<Post>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Welcome to Our Blog",
                Content = "This is the first post in our blog. We're excited to share our thoughts and ideas with you!",
                AuthorId = author.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = author.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "How to Build a Great Application",
                Content = "Building a great application requires careful planning, good architecture, and attention to detail. In this post, we'll explore some best practices.",
                AuthorId = author.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                CreatedById = author.Id
            },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Understanding Database Design",
                Content = "Database design is a crucial part of any application. A well-designed database can make your application fast, scalable, and maintainable.",
                AuthorId = author.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                CreatedById = author.Id
            }
        };

        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();
    }
}