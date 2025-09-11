namespace BlogApp.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private IAuditLogRepository? _auditLogRepository;
    private bool _disposed;
    private IPasswordHistoryRepository? _passwordHistoryRepository;
    private IPostRepository? _postRepository;
    private IRefreshTokenRepository? _refreshTokenRepository;
    private ITodoRepository? _todoRepository;
    private IDbContextTransaction? _transaction;

    public IPostRepository Posts => _postRepository ??= new PostRepository(context);
    public ITodoRepository Todos => _todoRepository ??= new TodoRepository(context);
    public IAuditLogRepository AuditLogs => _auditLogRepository ??= new AuditLogRepository(context);
    public IPasswordHistoryRepository PasswordHistory => _passwordHistoryRepository ??= new PasswordHistoryRepository(context);
    public IRefreshTokenRepository RefreshTokens => _refreshTokenRepository ??= new RefreshTokenRepository(context);

    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            if (_transaction != null) await _transaction.CommitAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackAsync()
    {
        try
        {
            if (_transaction != null) await _transaction.RollbackAsync();
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                context.Dispose();
            }

            _disposed = true;
        }
    }
}