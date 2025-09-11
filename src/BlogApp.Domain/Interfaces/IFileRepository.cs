namespace BlogApp.Domain.Interfaces;

public interface IFileRepository : IGenericRepository<FileEntity>
{
    Task<FileEntity?> GetByFileNameAsync(string fileName);
    Task<IEnumerable<FileEntity>> GetByUploadedByIdAsync(string uploadedById);
    Task<bool> ExistsByFileNameAsync(string fileName);
    Task<int> GetCountByUserIdAsync(string userId);
}