using TickerQ.EntityFrameworkCore.Configurations;

namespace BlogApp.Infrastructure.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor,
    AuditLoggingInterceptor auditLoggingInterceptor)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Post> Posts { get; set; }
    public DbSet<Todo> Todos { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<FileEntity> Files { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditableEntitySaveChangesInterceptor);
        optionsBuilder.AddInterceptors(auditLoggingInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global query filters for soft delete
        builder.Model.GetEntityTypes()
            .Where(entityType => typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            .Select(entityType => entityType.ClrType)
            .ToList()
            .ForEach(entityType =>
            {
                var parameter = Expression.Parameter(entityType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var compare = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(compare, parameter);
                builder.Entity(entityType).HasQueryFilter(lambda);
            });

        // Apply TickerQ entity configurations explicitly
        builder.ApplyConfiguration(new TimeTickerConfigurations());
        builder.ApplyConfiguration(new CronTickerConfigurations());
        builder.ApplyConfiguration(new CronTickerOccurrenceConfigurations());

        builder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Post>()
            .HasOne(p => p.BannerImageFile)
            .WithMany()
            .HasForeignKey(p => p.BannerImageFileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Todo>()
            .HasOne(t => t.User)
            .WithMany(u => u.Todos)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FileEntity>()
            .HasOne(f => f.UploadedBy)
            .WithMany()
            .HasForeignKey(f => f.UploadedById)
            .OnDelete(DeleteBehavior.SetNull);
    }
}