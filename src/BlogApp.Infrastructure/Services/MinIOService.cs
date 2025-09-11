namespace BlogApp.Infrastructure.Services;

public class MinIOService(
    IAmazonS3 s3Client,
    IFileRepository fileRepository,
    IFileValidationService fileValidationService,
    IMessageService messageService,
    IConfiguration configuration)
    : IFileService
{
    private readonly string _bucketName = configuration["S3:BucketName"] ?? "blogapp-files";

    public async Task<PresignedUploadResponseDto> GetPresignedUploadUrlAsync(PresignedUploadRequestDto request, string userId)
    {
        // Güvenlik validasyonu
        var validationResult = await fileValidationService.ValidatePresignedUploadRequestAsync(request, userId);
        if (!validationResult.IsValid)
            throw new InvalidOperationException($"{messageService.GetMessage("PresignedUrlGenerationFailed")}: {string.Join(", ", validationResult.Errors)}");

        var fileId = Guid.NewGuid();
        var sanitizedFileName = fileValidationService.SanitizeFileName(request.FileName);
        var fileName = $"{fileId}_{sanitizedFileName}";
        var filePath = $"uploads/{DateTime.UtcNow:yyyy/MM/dd}/{fileName}";
        var expiresAt = DateTime.UtcNow.AddMinutes(FileSecurityConstants.PresignedUrlExpiryMinutes);

        // Create presigned URL for PUT request (only sign essentials)
        var putRequest = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = filePath,
            Verb = HttpVerb.PUT,
            ContentType = request.ContentType,
            Expires = expiresAt
        };

        var uploadUrl = await s3Client.GetPreSignedURLAsync(putRequest);

        // Create temporary file entity (will be updated when upload is completed)
        var fileEntity = new FileEntity
        {
            Id = fileId,
            FileName = fileName,
            OriginalFileName = sanitizedFileName,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            FilePath = filePath,
            Description = request.Description,
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        await fileRepository.AddAsync(fileEntity);

        return new PresignedUploadResponseDto
        {
            FileId = fileId,
            UploadUrl = uploadUrl.Replace("https", "http"),
            FilePath = filePath,
            ExpiresAt = expiresAt,
            FormFields = new Dictionary<string, string>
            {
                ["Content-Type"] = request.ContentType
            }
        };
    }

    public async Task<FileUploadResponseDto> CompleteUploadAsync(CompleteUploadDto completeDto, string userId)
    {
        // Güvenlik validasyonu
        var validationResult = await fileValidationService.ValidateCompleteUploadRequestAsync(completeDto, userId);
        if (!validationResult.IsValid)
            throw new InvalidOperationException($"{messageService.GetMessage("UploadCompletionFailed")}: {string.Join(", ", validationResult.Errors)}");

        var file = await fileRepository.GetByIdAsync(completeDto.FileId);
        if (file == null)
            throw new FileNotFoundException(messageService.GetMessage("FileNotFound"));

        if (file.UploadedById != userId)
            throw new UnauthorizedAccessException(messageService.GetMessage("FileOwnershipRequired"));

        // S3'te dosyanın gerçekten var olup olmadığını kontrol et ve içerik validasyonu yap
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = file.FilePath
            };

            var response = await s3Client.GetObjectAsync(getRequest);


            // Dosya içeriği kontrolü (magic bytes)
            var contentValidation = await fileValidationService.ValidateFileContentAsync(
                response.ResponseStream,
                file.OriginalFileName,
                file.ContentType);

            if (!contentValidation.IsValid)
            {
                await response.ResponseStream.DisposeAsync();
                throw new InvalidOperationException($"{messageService.GetMessage("FileContentValidationFailed")}: {string.Join(", ", contentValidation.Errors)}");
            }

            // Response stream'i kapat
            await response.ResponseStream.DisposeAsync();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new FileNotFoundException(messageService.GetMessage("FileNotInS3"));
        }

        // Update file size with actual size
        file.FileSize = completeDto.ActualFileSize;
        fileRepository.Update(file);

        return new FileUploadResponseDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FilePath = file.FilePath,
            Description = file.Description,
            CreatedAt = file.CreatedAt
        };
    }

    public async Task<FileDto?> GetFileAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);
        if (file == null) return null;

        return new FileDto
        {
            Id = file.Id,
            FileName = file.FileName,
            OriginalFileName = file.OriginalFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FilePath = file.FilePath,
            Description = file.Description,
            UploadedById = file.UploadedById,
            CreatedAt = file.CreatedAt
        };
    }

    public async Task<Stream> DownloadFileAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);
        if (file == null)
            throw new FileNotFoundException(messageService.GetMessage("FileNotFound"));

        var getRequest = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = file.FilePath
        };

        var response = await s3Client.GetObjectAsync(getRequest);
        return response.ResponseStream;
    }

    public async Task<bool> DeleteFileAsync(Guid fileId, string userId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);
        if (file == null) return false;

        // Check if user owns the file or is admin
        if (file.UploadedById != userId)
            return false;

        // Delete from MinIO

        // Delete from database
        fileRepository.Remove(file);
        return true;
    }

    public async Task<IEnumerable<FileDto>> GetUserFilesAsync(string userId)
    {
        var files = await fileRepository.GetByUploadedByIdAsync(userId);
        return files.Select(f => new FileDto
        {
            Id = f.Id,
            FileName = f.FileName,
            OriginalFileName = f.OriginalFileName,
            ContentType = f.ContentType,
            FileSize = f.FileSize,
            FilePath = f.FilePath,
            Description = f.Description,
            UploadedById = f.UploadedById,
            CreatedAt = f.CreatedAt
        });
    }

    public async Task<string> GetFileUrlAsync(Guid fileId)
    {
        var file = await fileRepository.GetByIdAsync(fileId);
        if (file == null)
            throw new FileNotFoundException(messageService.GetMessage("FileNotFound"));

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = file.FilePath,
            Expires = DateTime.UtcNow.AddHours(1) // URL expires in 1 hour
        };

        return await s3Client.GetPreSignedURLAsync(request);
    }


    public static string GenerateSecurityHash(string userId, Guid fileId, string fileName)
    {
        var data = $"{userId}:{fileId}:{fileName}:{DateTime.UtcNow:yyyyMMdd}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }

    public static string GenerateSecurityHash(string userId, Guid fileId, string fileName, DateTime date)
    {
        var data = $"{userId}:{fileId}:{fileName}:{date:yyyyMMdd}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }
}