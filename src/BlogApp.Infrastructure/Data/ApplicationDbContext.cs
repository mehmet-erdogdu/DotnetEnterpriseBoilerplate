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
    public DbSet<RefreshToken> RefreshToken { get; set; }
    public DbSet<PasswordHistory> PasswordHistory { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(auditableEntitySaveChangesInterceptor);
        optionsBuilder.AddInterceptors(auditLoggingInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply entity configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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
    }
}