namespace BlogApp.Infrastructure.Repositories;

public class AuditLogRepository(ApplicationDbContext context) : GenericRepository<AuditLog>(context), IAuditLogRepository
{
    public async Task<IEnumerable<AuditLog>> GetLogsForEntity(string tableName, Guid entityId)
    {
        return await context.AuditLogs
            .Where(log => log.TableName == tableName && log.EntityId == entityId)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByUser(string userId)
    {
        return await context.AuditLogs
            .Where(log => log.UserId == userId)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuditLog>> GetLogsByDateRange(DateTime startDate, DateTime endDate)
    {
        return await context.AuditLogs
            .Where(log => log.Timestamp >= startDate && log.Timestamp <= endDate)
            .OrderByDescending(log => log.Timestamp)
            .ToListAsync();
    }
}