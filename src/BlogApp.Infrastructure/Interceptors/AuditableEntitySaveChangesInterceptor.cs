namespace BlogApp.Infrastructure.Interceptors;

public class AuditableEntitySaveChangesInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null) return;

        // Convert hard deletes to soft deletes
        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
                entry.Entity.DeletedById = currentUserService.UserId;
            }

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedById = currentUserService.UserId ?? string.Empty;
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedByIp = currentUserService.IpAddress;
                entry.Entity.CreatedByUserAgent = currentUserService.UserAgent;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedById = currentUserService.UserId;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedByIp = currentUserService.IpAddress;
                entry.Entity.UpdatedByUserAgent = currentUserService.UserAgent;
            }
        }
    }
}