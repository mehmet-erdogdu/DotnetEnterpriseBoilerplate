using ValidationResult = BlogApp.Application.DTOs.ValidationResult;

namespace BlogApp.Application.Services;

public interface IFileValidationService
{
    Task<ValidationResult> ValidatePresignedUploadRequestAsync(PresignedUploadRequestDto request, string userId);
    Task<ValidationResult> ValidateCompleteUploadRequestAsync(CompleteUploadDto request, string userId);
    Task<ValidationResult> ValidateFileContentAsync(Stream fileStream, string fileName, string contentType);
    bool IsValidFileName(string fileName);
    bool IsValidFileExtension(string fileName);
    bool IsValidMimeType(string mimeType);
    bool IsDangerousFile(string fileName, string mimeType);
    bool ContainsPathTraversal(string path);
    string SanitizeFileName(string fileName);
}