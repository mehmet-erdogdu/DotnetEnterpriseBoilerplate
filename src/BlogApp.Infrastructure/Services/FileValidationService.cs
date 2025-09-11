namespace BlogApp.Infrastructure.Services;

public class FileValidationService(
    IFileRepository fileRepository,
    IDistributedCache cache,
    IMessageService messageService) : IFileValidationService
{
    private static readonly char[] WhitespaceChars = { ' ', '\t', '\n', '\r' };

    public async Task<ValidationResult> ValidatePresignedUploadRequestAsync(PresignedUploadRequestDto request, string userId)
    {
        var result = ValidationResult.Success();

        // Dosya adı validasyonu
        if (!IsValidFileName(request.FileName)) result = result.AddError(messageService.GetMessage("InvalidFileName"));

        // Dosya uzantısı validasyonu
        if (!IsValidFileExtension(request.FileName)) result = result.AddError(messageService.GetMessage("InvalidFileExtension"));

        // MIME türü validasyonu
        if (!IsValidMimeType(request.ContentType)) result = result.AddError(messageService.GetMessage("InvalidMimeType"));

        // Tehlikeli dosya kontrolü
        if (IsDangerousFile(request.FileName, request.ContentType)) result = result.AddError(messageService.GetMessage("DangerousFileDetected"));

        // Dosya boyutu validasyonu
        if (request.FileSize <= 0 || request.FileSize > FileSecurityConstants.MaxFileSize)
            result = result.AddError(string.Format(messageService.GetMessage("FileSizeInvalid"), FileSecurityConstants.MaxFileSize / (1024 * 1024)));

        // Rate limiting kontrolü
        var rateLimitResult = await CheckRateLimitAsync(userId);
        if (!rateLimitResult.IsValid) result = result.Combine(rateLimitResult);

        // Açıklama uzunluğu kontrolü
        if (!string.IsNullOrEmpty(request.Description) && request.Description.Length > 500) result = result.AddError(messageService.GetMessage("DescriptionTooLong"));

        return result;
    }

    public async Task<ValidationResult> ValidateCompleteUploadRequestAsync(CompleteUploadDto request, string userId)
    {
        var result = ValidationResult.Success();

        // File existence and ownership validation
        var fileValidation = await ValidateFileExistenceAndOwnershipAsync(request.FileId, userId);
        if (!fileValidation.IsValid)
            return fileValidation;

        var file = await fileRepository.GetByIdAsync(request.FileId);

        // File size validations
        result = result.Combine(ValidateFileSize(request.ActualFileSize));
        result = result.Combine(ValidateFileSizeConsistency(request.ActualFileSize, file!.FileSize));

        // Optional file name validation
        if (!string.IsNullOrEmpty(request.OriginalFileName)) result = result.Combine(ValidateFileName(request.OriginalFileName, request.ContentType ?? file.ContentType));

        // Optional content type validation
        if (!string.IsNullOrEmpty(request.ContentType)) result = result.Combine(ValidateMimeTypeOnly(request.ContentType));

        return result;
    }

    public async Task<ValidationResult> ValidateFileContentAsync(Stream fileStream, string fileName, string contentType)
    {
        var result = ValidationResult.Success();

        try
        {
            var canSeek = fileStream.CanSeek;
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            if (FileSecurityConstants.FileSignatures.TryGetValue(extension, out var expectedSignature))
            {
                var buffer = new byte[expectedSignature.Length];
                var bytesRead = await fileStream.ReadAsync(buffer);

                if (bytesRead == expectedSignature.Length)
                {
                    if (!buffer.SequenceEqual(expectedSignature))
                        result = result.AddError(messageService.GetMessage("FileContentMismatch"));
                }
                else
                {
                    result = result.AddError(messageService.GetMessage("FileTooSmallForValidation"));
                }

                // Stream'i başa sar (sadece seek destekliyse)
                if (canSeek)
                    fileStream.Position = 0;
            }
        }
        catch (Exception)
        {
            result = result.AddError(string.Format(messageService.GetMessage("ContentValidationError")));
        }

        return result;
    }

    public bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        if (fileName.Length < FileSecurityConstants.MinFileNameLength ||
            fileName.Length > FileSecurityConstants.MaxFileNameLength)
            return false;

        if (FileSecurityConstants.DangerousFileNameChars.Any(c => fileName.Contains(c)))
            return false;

        if (ContainsPathTraversal(fileName))
            return false;

        return true;
    }

    public bool IsValidFileExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension))
            return false;

        if (FileSecurityConstants.DangerousExtensions.Contains(extension))
            return false;

        return FileSecurityConstants.AllowedExtensions.Contains(extension);
    }

    public bool IsValidMimeType(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return false;

        if (FileSecurityConstants.DangerousMimeTypes.Contains(mimeType))
            return false;

        return FileSecurityConstants.AllowedMimeTypes.Contains(mimeType);
    }

    public bool IsDangerousFile(string fileName, string mimeType)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        // Tehlikeli uzantı kontrolü
        if (FileSecurityConstants.DangerousExtensions.Contains(extension))
            return true;

        // Tehlikeli MIME türü kontrolü
        if (FileSecurityConstants.DangerousMimeTypes.Contains(mimeType))
            return true;

        // Çift uzantı kontrolü (örn: file.jpg.exe)
        var parts = fileName.Split('.');
        if (parts.Length > 2)
        {
            var lastExtension = $".{parts[^1]}";
            var secondLastExtension = $".{parts[^2]}";

            if (FileSecurityConstants.AllowedExtensions.Contains(secondLastExtension) &&
                FileSecurityConstants.DangerousExtensions.Contains(lastExtension))
                return true;
        }

        return false;
    }

    public bool ContainsPathTraversal(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var normalizedPath = path.Replace('\\', '/').ToLowerInvariant();

        // Path traversal kontrolü
        if (normalizedPath.Contains('.') || normalizedPath.Contains('~'))
            return true;

        // Tehlikeli dizin isimleri kontrolü
        var pathParts = normalizedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return pathParts.Any(part => FileSecurityConstants.DangerousPathComponents.Contains(part));
    }

    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "unnamed_file";

        // Tehlikeli karakterleri kaldır
        var sb = new StringBuilder(fileName.Length);
        foreach (var c in fileName.Where(c => !FileSecurityConstants.DangerousFileNameChars.Contains(c)))
            sb.Append(c);

        var sanitized = sb.ToString();

        try
        {
            // Çoklu boşlukları tek boşluğa çevir
            sanitized = Regex.Replace(sanitized, @"\s+", " ",
                FileSecurityConstants.DefaultRegexOptions, FileSecurityConstants.RegexTimeout);
        }
        catch (RegexMatchTimeoutException)
        {
            // If regex times out, just trim spaces manually
            sanitized = string.Join(" ", sanitized.Split(WhitespaceChars,
                StringSplitOptions.RemoveEmptyEntries));
        }

        // Başındaki ve sonundaki boşlukları kaldır
        sanitized = sanitized.Trim();

        // Uzunluğu sınırla
        if (sanitized.Length > FileSecurityConstants.MaxFileNameLength)
        {
            var extension = Path.GetExtension(sanitized);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            var maxNameLength = FileSecurityConstants.MaxFileNameLength - extension.Length;

            sanitized = nameWithoutExtension[..maxNameLength] + extension;
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "unnamed_file" : sanitized;
    }

    private async Task<ValidationResult> ValidateFileExistenceAndOwnershipAsync(Guid fileId, string userId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);
        if (file == null) return ValidationResult.Failure(messageService.GetMessage("FileNotFound"));

        if (file.UploadedById != userId) return ValidationResult.Failure(messageService.GetMessage("FileOwnershipRequired"));

        return ValidationResult.Success();
    }

    private ValidationResult ValidateFileSize(long actualFileSize)
    {
        if (actualFileSize <= 0 || actualFileSize > FileSecurityConstants.MaxFileSize)
        {
            var errorMessage = string.Format(messageService.GetMessage("FileSizeInvalid"),
                FileSecurityConstants.MaxFileSize / (1024 * 1024));
            return ValidationResult.Failure(errorMessage);
        }

        return ValidationResult.Success();
    }

    private ValidationResult ValidateFileSizeConsistency(long actualSize, long expectedSize)
    {
        var minAllowedSize = expectedSize * 0.5;
        var maxAllowedSize = expectedSize * 2.0;

        if (actualSize < minAllowedSize || actualSize > maxAllowedSize)
        {
            var errorMessage = string.Format(messageService.GetMessage("FileSizeMismatch"),
                expectedSize, actualSize);
            return ValidationResult.Failure(errorMessage);
        }

        return ValidationResult.Success();
    }

    private ValidationResult ValidateFileName(string fileName, string contentType)
    {
        var result = ValidationResult.Success();

        if (!IsValidFileName(fileName))
            result = result.AddError(messageService.GetMessage("InvalidFileName"));

        if (!IsValidFileExtension(fileName))
            result = result.AddError(messageService.GetMessage("InvalidFileExtension"));

        if (IsDangerousFile(fileName, contentType))
            result = result.AddError(messageService.GetMessage("DangerousFileDetected"));

        return result;
    }

    private ValidationResult ValidateMimeTypeOnly(string contentType)
    {
        if (!IsValidMimeType(contentType)) return ValidationResult.Failure(messageService.GetMessage("InvalidMimeType"));
        return ValidationResult.Success();
    }

    private async Task<ValidationResult> CheckRateLimitAsync(string userId)
    {
        var hourKey = $"upload_rate_limit_hour_{userId}_{DateTime.UtcNow:yyyyMMddHH}";
        var dayKey = $"upload_rate_limit_day_{userId}_{DateTime.UtcNow:yyyyMMdd}";

        // Saatlik limit kontrolü
        var hourCount = await cache.GetStringAsync(hourKey);
        var hourCountInt = string.IsNullOrEmpty(hourCount) ? 0 : int.Parse(hourCount);

        if (hourCountInt >= FileSecurityConstants.MaxUploadRequestsPerHour)
            return ValidationResult.Failure(messageService.GetMessage("HourlyUploadLimitReached"));

        // Günlük limit kontrolü
        var dayCount = await cache.GetStringAsync(dayKey);
        var dayCountInt = string.IsNullOrEmpty(dayCount) ? 0 : int.Parse(dayCount);

        if (dayCountInt >= FileSecurityConstants.MaxUploadRequestsPerDay)
            return ValidationResult.Failure(messageService.GetMessage("DailyUploadLimitReached"));

        // Limitleri artır
        await cache.SetStringAsync(hourKey, (hourCountInt + 1).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
        await cache.SetStringAsync(dayKey, (dayCountInt + 1).ToString(),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1) });

        return ValidationResult.Success();
    }
}