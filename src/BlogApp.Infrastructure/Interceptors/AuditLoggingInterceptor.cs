namespace BlogApp.Infrastructure.Interceptors;

public class AuditLoggingInterceptor(ICurrentUserService currentUserService) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            var auditLogs = CreateAuditLogs(eventData.Context);
            eventData.Context.Set<AuditLog>().AddRange(auditLogs);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditLog> CreateAuditLogs(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var changes = context.ChangeTracker.Entries()
            .Where(e => e.State is EntityState.Modified or EntityState.Added or EntityState.Deleted)
            .ToList();

        foreach (var entry in changes)
        {
            // Skip AuditLog entities to prevent infinite loop
            if (entry.Entity is AuditLog)
                continue;

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                EntityId = GetEntityId(entry),
                Action = ResolveAction(entry),
                UserId = currentUserService.UserId ?? "System",
                UserIp = currentUserService.IpAddress,
                UserAgent = currentUserService.UserAgent,
                Timestamp = DateTime.UtcNow
            };

            if (entry.State == EntityState.Added)
            {
                auditLog.NewValues = SerializeValues(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else if (entry.State == EntityState.Modified)
            {
                var changedProperties = entry.Properties
                    .Where(p => p.IsModified && !IsIgnoredProperty(p.Metadata.Name))
                    .ToList();

                if (changedProperties.Count > 0)
                {
                    auditLog.ChangedColumns = JsonSerializer.Serialize(changedProperties.Select(p => p.Metadata.Name));
                    auditLog.OldValues = SerializeValues(changedProperties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                    auditLog.NewValues = SerializeValues(changedProperties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
                }
            }
            else if (entry.State == EntityState.Deleted)
            {
                auditLog.OldValues = SerializeValues(entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
            }

            auditLogs.Add(auditLog);
        }

        return auditLogs;
    }

    private static string ResolveAction(EntityEntry entry)
    {
        if (entry.State == EntityState.Deleted) return "Deleted";

        // Detect soft delete (IsDeleted turned true) and restore (IsDeleted turned false)
        if (entry.State == EntityState.Modified && entry.Entity is ISoftDeletable)
        {
            var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(ISoftDeletable.IsDeleted));
            if (isDeletedProp != null && isDeletedProp.IsModified)
            {
                var newVal = isDeletedProp.CurrentValue as bool? ?? false;
                var oldVal = isDeletedProp.OriginalValue as bool? ?? false;
                if (!oldVal && newVal) return "Deleted"; // soft delete
                if (oldVal && !newVal) return "Restored";
            }
        }

        return entry.State.ToString();
    }

    private static Guid GetEntityId(EntityEntry entry)
    {
        var idProperty = entry.Properties.FirstOrDefault(p =>
            p.Metadata.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

        return idProperty?.CurrentValue is Guid guid ? guid : Guid.Empty;
    }

    private static string? SerializeValues(Dictionary<string, object?> values)
    {
        return values.Count > 0 ? JsonSerializer.Serialize(values) : null;
    }

    private static bool IsIgnoredProperty(string propertyName)
    {
        // Ignore audit fields from the base AuditableEntity
        var ignoredProperties = new[]
        {
            "UpdatedAt",
            "UpdatedById",
            "UpdatedByIp",
            "UpdatedByUserAgent"
        };

        return ignoredProperties.Contains(propertyName);
    }
}