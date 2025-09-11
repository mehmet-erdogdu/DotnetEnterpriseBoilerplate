namespace BlogApp.Application.Services;

public interface IFileService
{
    Task<FileDto?> GetFileAsync(Guid fileId);
    Task<Stream> DownloadFileAsync(Guid fileId);
    Task<bool> DeleteFileAsync(Guid fileId, string userId);
    Task<IEnumerable<FileDto>> GetUserFilesAsync(string userId);
    Task<string> GetFileUrlAsync(Guid fileId);
    Task<PresignedUploadResponseDto> GetPresignedUploadUrlAsync(PresignedUploadRequestDto request, string userId);
    Task<FileUploadResponseDto> CompleteUploadAsync(CompleteUploadDto completeDto, string userId);
}