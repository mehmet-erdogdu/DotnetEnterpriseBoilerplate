using Amazon.S3;
using Amazon.S3.Model;
using ValidationResult = BlogApp.Application.DTOs.ValidationResult;

namespace BlogApp.UnitTests.Infrastructure.Services;

public class MinIOServiceTests : BaseInfrastructureTest
{
    private readonly MinIOService _minIOService;
    private new readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IFileValidationService> _mockFileValidationService;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<IAmazonS3> _mockS3Client;

    public MinIOServiceTests()
    {
        _mockS3Client = new Mock<IAmazonS3>();
        _mockFileRepository = new Mock<IFileRepository>();
        _mockFileValidationService = new Mock<IFileValidationService>();
        _mockMessageService = new Mock<IMessageService>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup configuration
        _mockConfiguration.Setup(c => c["S3:BucketName"]).Returns("test-bucket");

        _minIOService = new MinIOService(
            _mockS3Client.Object,
            _mockFileRepository.Object,
            _mockFileValidationService.Object,
            _mockMessageService.Object,
            _mockConfiguration.Object);
    }

    [Fact]
    public async Task GetPresignedUploadUrlAsync_WithValidRequest_ReturnsPresignedUrl()
    {
        // Arrange
        var request = new PresignedUploadRequestDto
        {
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024 * 1024,
            Description = "Test file"
        };
        var userId = "test-user-id";

        var validationResult = ValidationResult.Success();
        _mockFileValidationService.Setup(x => x.ValidatePresignedUploadRequestAsync(request, userId))
            .ReturnsAsync(validationResult);

        _mockFileValidationService.Setup(x => x.SanitizeFileName(request.FileName))
            .Returns("test.jpg");

        _mockS3Client.Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync("http://test-presigned-url.com");

        _mockFileRepository.Setup(x => x.AddAsync(It.IsAny<FileEntity>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _minIOService.GetPresignedUploadUrlAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.UploadUrl.Should().NotBeNullOrEmpty();
        result.FileId.Should().NotBeEmpty();
        _mockFileRepository.Verify(x => x.AddAsync(It.IsAny<FileEntity>()), Times.Once);
    }

    [Fact]
    public async Task GetPresignedUploadUrlAsync_WithInvalidRequest_ThrowsException()
    {
        // Arrange
        var request = new PresignedUploadRequestDto
        {
            FileName = "test.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024 * 1024,
            Description = "Test file"
        };
        var userId = "test-user-id";

        var validationResult = ValidationResult.Failure("Invalid request");
        _mockFileValidationService.Setup(x => x.ValidatePresignedUploadRequestAsync(request, userId))
            .ReturnsAsync(validationResult);

        _mockMessageService.Setup(x => x.GetMessage("PresignedUrlGenerationFailed"))
            .Returns("Presigned URL generation failed");

        // Act
        Func<Task> act = async () => await _minIOService.GetPresignedUploadUrlAsync(request, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Presigned URL generation failed*");
    }

    [Fact]
    public async Task CompleteUploadAsync_WithValidRequest_ReturnsFileUploadResponse()
    {
        // Arrange
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024 * 1024
        };
        var userId = "test-user-id";

        var validationResult = ValidationResult.Success();
        _mockFileValidationService.Setup(x => x.ValidateCompleteUploadRequestAsync(completeDto, userId))
            .ReturnsAsync(validationResult);

        var fileEntity = new FileEntity
        {
            Id = completeDto.FileId,
            FileName = "test.jpg",
            OriginalFileName = "test.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024 * 1024,
            FilePath = "uploads/test.jpg",
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(completeDto.FileId))
            .ReturnsAsync(fileEntity);

        var getObjectResponse = new GetObjectResponse
        {
            ResponseStream = new MemoryStream()
        };

        _mockS3Client.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
            .ReturnsAsync(getObjectResponse);

        var contentValidationResult = ValidationResult.Success();
        _mockFileValidationService.Setup(x => x.ValidateFileContentAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(contentValidationResult);

        // Act
        var result = await _minIOService.CompleteUploadAsync(completeDto, userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(completeDto.FileId);
        _mockFileRepository.Verify(x => x.Update(It.IsAny<FileEntity>()), Times.Once);
    }

    [Fact]
    public async Task CompleteUploadAsync_WithInvalidValidation_ThrowsException()
    {
        // Arrange
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024 * 1024
        };
        var userId = "test-user-id";

        var validationResult = ValidationResult.Failure("Invalid request");
        _mockFileValidationService.Setup(x => x.ValidateCompleteUploadRequestAsync(completeDto, userId))
            .ReturnsAsync(validationResult);

        _mockMessageService.Setup(x => x.GetMessage("UploadCompletionFailed"))
            .Returns("Upload completion failed");

        // Act
        Func<Task> act = async () => await _minIOService.CompleteUploadAsync(completeDto, userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Upload completion failed*");
    }

    [Fact]
    public async Task CompleteUploadAsync_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024 * 1024
        };
        var userId = "test-user-id";

        var validationResult = ValidationResult.Success();
        _mockFileValidationService.Setup(x => x.ValidateCompleteUploadRequestAsync(completeDto, userId))
            .ReturnsAsync(validationResult);

        _mockFileRepository.Setup(x => x.GetByIdAsync(completeDto.FileId))
            .ReturnsAsync((FileEntity?)null);

        _mockMessageService.Setup(x => x.GetMessage("FileNotFound"))
            .Returns("File not found");

        // Act
        Func<Task> act = async () => await _minIOService.CompleteUploadAsync(completeDto, userId);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("File not found");
    }

    [Fact]
    public async Task GetFileAsync_WithExistingFile_ReturnsFileDto()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var fileEntity = new FileEntity
        {
            Id = fileId,
            FileName = "test.jpg",
            OriginalFileName = "test.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024 * 1024,
            FilePath = "uploads/test.jpg",
            UploadedById = "test-user-id",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(fileEntity);

        // Act
        var result = await _minIOService.GetFileAsync(fileId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(fileId);
        result.FileName.Should().Be(fileEntity.FileName);
    }

    [Fact]
    public async Task GetFileAsync_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync((FileEntity?)null);

        // Act
        var result = await _minIOService.GetFileAsync(fileId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_WithValidFile_ReturnsTrue()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var fileEntity = new FileEntity
        {
            Id = fileId,
            FileName = "test.jpg",
            OriginalFileName = "test.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024 * 1024,
            FilePath = "uploads/test.jpg",
            UploadedById = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(fileEntity);

        // Act
        var result = await _minIOService.DeleteFileAsync(fileId, userId);

        // Assert
        result.Should().BeTrue();
        _mockFileRepository.Verify(x => x.Remove(fileEntity), Times.Once);
    }

    [Fact]
    public async Task DeleteFileAsync_WithWrongUser_ReturnsFalse()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var fileEntity = new FileEntity
        {
            Id = fileId,
            FileName = "test.jpg",
            OriginalFileName = "test.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024 * 1024,
            FilePath = "uploads/test.jpg",
            UploadedById = "other-user-id",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(fileEntity);

        // Act
        var result = await _minIOService.DeleteFileAsync(fileId, userId);

        // Assert
        result.Should().BeFalse();
        _mockFileRepository.Verify(x => x.Remove(It.IsAny<FileEntity>()), Times.Never);
    }

    [Fact]
    public async Task GetUserFilesAsync_WithUserId_ReturnsFileDtos()
    {
        // Arrange
        var userId = "test-user-id";
        var fileEntities = new List<FileEntity>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FileName = "test1.jpg",
                OriginalFileName = "test1.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024 * 1024,
                FilePath = "uploads/test1.jpg",
                UploadedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FileName = "test2.jpg",
                OriginalFileName = "test2.jpg",
                ContentType = "image/jpeg",
                FileSize = 2 * 1024 * 1024,
                FilePath = "uploads/test2.jpg",
                UploadedById = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockFileRepository.Setup(x => x.GetByUploadedByIdAsync(userId))
            .ReturnsAsync(fileEntities);

        // Act
        var result = await _minIOService.GetUserFilesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }
}