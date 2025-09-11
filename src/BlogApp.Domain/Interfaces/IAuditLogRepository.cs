namespace BlogApp.Domain.Interfaces;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<IEnumerable<AuditLog>> GetLogsForEntity(string tableName, Guid entityId);
    Task<IEnumerable<AuditLog>> GetLogsByUser(string userId);
    Task<IEnumerable<AuditLog>> GetLogsByDateRange(DateTime startDate, DateTime endDate);
}