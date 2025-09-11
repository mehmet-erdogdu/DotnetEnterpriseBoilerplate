namespace BlogApp.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IPostRepository Posts { get; }
    ITodoRepository Todos { get; }
    IAuditLogRepository AuditLogs { get; }
    IPasswordHistoryRepository PasswordHistory { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<bool> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}