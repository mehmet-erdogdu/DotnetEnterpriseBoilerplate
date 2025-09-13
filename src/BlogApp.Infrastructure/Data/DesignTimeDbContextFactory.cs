using BlogApp.Infrastructure.Helpers;

namespace BlogApp.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var secrets = VaultHelper.LoadSecretsFromVaultAsync("BLOG_APP").GetAwaiter().GetResult();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(secrets!)
            .Build();

        optionsBuilder.UseNpgsql(configuration["DBConnection"], o => { o.MigrationsAssembly("BlogApp.Infrastructure"); });

        var httpContextAccessor = new HttpContextAccessor();
        ICurrentUserService currentUserService = new CurrentUserService(httpContextAccessor);

        var auditableInterceptor = new AuditableEntitySaveChangesInterceptor(currentUserService);
        var auditLoggingInterceptor = new AuditLoggingInterceptor(currentUserService);

        return new ApplicationDbContext(optionsBuilder.Options, auditableInterceptor, auditLoggingInterceptor);
    }
}