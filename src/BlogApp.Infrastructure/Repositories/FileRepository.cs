namespace BlogApp.Infrastructure.Repositories;

public class FileRepository(ApplicationDbContext context) : GenericRepository<FileEntity>(context), IFileRepository
{
    public async Task<FileEntity?> GetByFileNameAsync(string fileName)
    {
        return await _context.Set<FileEntity>()
            .FirstOrDefaultAsync(f => f.FileName == fileName);
    }

    public async Task<IEnumerable<FileEntity>> GetByUploadedByIdAsync(string uploadedById)
    {
        return await _context.Set<FileEntity>()
            .Where(f => f.UploadedById == uploadedById)
            .ToListAsync();
    }

    public async Task<bool> ExistsByFileNameAsync(string fileName)
    {
        return await _context.Set<FileEntity>()
            .AnyAsync(f => f.FileName == fileName);
    }

    public async Task<int> GetCountByUserIdAsync(string userId)
    {
        return await _context.Set<FileEntity>()
            .CountAsync(f => f.UploadedById == userId);
    }
}