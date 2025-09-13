namespace BlogApp.UnitTests.Application.Services;

public class FileValidationServiceAdditionalTests
{
    private readonly FileValidationService _fileValidationService;
    private readonly Mock<IDistributedCache> _mockCache;
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IMessageService> _mockMessageService;

    public FileValidationServiceAdditionalTests()
    {
        _mockFileRepository = new Mock<IFileRepository>();
        _mockCache = new Mock<IDistributedCache>();
        _mockMessageService = new Mock<IMessageService>();

        _fileValidationService = new FileValidationService(
            _mockFileRepository.Object,
            _mockCache.Object,
            _mockMessageService.Object);
    }

    #region Additional FileExtension Validation Tests

    [Fact]
    public void IsValidFileExtension_WithNoExtension_ReturnsFalse()
    {
        // Arrange
        var fileName = "test_file"; // No extension

        // Act
        var result = _fileValidationService.IsValidFileExtension(fileName);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Additional MimeType Validation Tests

    [Fact]
    public void IsValidMimeType_WithEmptyMimeType_ReturnsFalse()
    {
        // Arrange
        var mimeType = ""; // Empty MIME type

        // Act
        var result = _fileValidationService.IsValidMimeType(mimeType);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Additional DangerousFile Tests

    [Fact]
    public void IsDangerousFile_WithDoubleExtension_ReturnsTrue()
    {
        // Arrange
        var fileName = "document.pdf.exe"; // Double extension
        var mimeType = "application/octet-stream";

        // Act
        var result = _fileValidationService.IsDangerousFile(fileName, mimeType);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region ValidateCompleteUploadRequestAsync Tests

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithNonExistentFile_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024
        };
        var userId = "test-user-id";

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync((FileEntity?)null);

        _mockMessageService.Setup(x => x.GetMessage("FileNotFound"))
            .Returns("File not found");

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("File not found");
    }

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithWrongFileOwner_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024
        };
        var userId = "test-user-id";
        var fileOwner = "different-user-id";

        var file = new FileEntity
        {
            Id = request.FileId,
            UploadedById = fileOwner,
            FileSize = request.ActualFileSize,
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FilePath = "uploads/test-file.jpg"
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync(file);

        _mockMessageService.Setup(x => x.GetMessage("FileOwnershipRequired"))
            .Returns("File ownership required");

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("File ownership required");
    }

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithInvalidFileSize_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = -1 // Invalid size
        };
        var userId = "test-user-id";

        var file = new FileEntity
        {
            Id = request.FileId,
            UploadedById = userId,
            FileSize = 1024,
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FilePath = "uploads/test-file.jpg"
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync(file);

        _mockMessageService.Setup(x => x.GetMessage("FileSizeInvalid"))
            .Returns("File size must be between 0 and {0} MB");
        _mockMessageService.Setup(x => x.GetMessage("FileSizeMismatch"))
            .Returns("File size mismatch: expected {0}, actual {1}");

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("File size must be between 0 and 100 MB");
    }

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithFileSizeMismatch_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 5000 // Much larger than expected
        };
        var userId = "test-user-id";

        var file = new FileEntity
        {
            Id = request.FileId,
            UploadedById = userId,
            FileSize = 1024, // Expected size
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FilePath = "uploads/test-file.jpg"
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync(file);

        _mockMessageService.Setup(x => x.GetMessage("FileSizeMismatch", It.IsAny<object[]>()))
            .Returns("File size mismatch: expected 1024, got 5000");

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("File size mismatch: expected 1024, got 5000");
    }

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithInvalidOriginalFileName_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024,
            OriginalFileName = " " // Invalid whitespace name
        };
        var userId = "test-user-id";

        var file = new FileEntity
        {
            Id = request.FileId,
            UploadedById = userId,
            FileSize = request.ActualFileSize,
            ContentType = "image/jpeg",
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            FilePath = "uploads/test-file.jpg"
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync(file);

        _mockMessageService.Setup(x => x.GetMessage("InvalidFileName"))
            .Returns("Invalid file name");

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid file name");
    }

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithInvalidContentType_ReturnsFailure()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024,
            ContentType = "application/x-msdownload" // Invalid content type
        };
        var userId = "test-user-id";

        var file = new FileEntity
        {
            Id = request.FileId,
            UploadedById = userId,
            FileSize = request.ActualFileSize,
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "application/x-msdownload",
            FilePath = "uploads/test-file.exe"
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync(file);

        _mockMessageService.Setup(x => x.GetMessage("InvalidMimeType"))
            .Returns("Invalid MIME type");

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Invalid MIME type");
    }

    [Fact]
    public async Task ValidateCompleteUploadRequestAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var request = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024
        };
        var userId = "test-user-id";

        var file = new FileEntity
        {
            Id = request.FileId,
            UploadedById = userId,
            FileSize = request.ActualFileSize,
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FilePath = "uploads/test-file.jpg"
        };

        _mockFileRepository.Setup(x => x.GetByIdAsync(request.FileId))
            .ReturnsAsync(file);

        // Act
        var result = await _fileValidationService.ValidateCompleteUploadRequestAsync(request, userId);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region ValidateFileContentAsync Tests

    [Fact]
    public async Task ValidateFileContentAsync_WithFileTooSmallForValidation_ReturnsFailure()
    {
        // Arrange
        var content = new byte[] { 0xFF }; // Too small for JPEG validation
        var stream = new MemoryStream(content);
        var fileName = "test_file.jpg";
        var contentType = "image/jpeg";

        _mockMessageService.Setup(x => x.GetMessage("FileTooSmallForValidation"))
            .Returns("File too small for validation");

        // Act
        var result = await _fileValidationService.ValidateFileContentAsync(stream, fileName, contentType);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("File too small for validation");
    }

    [Fact]
    public async Task ValidateFileContentAsync_WithExceptionDuringValidation_ReturnsFailure()
    {
        // Arrange
        var stream = new Mock<Stream>();
        stream.Setup(x => x.CanSeek).Returns(true);
        stream.Setup(x => x.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("Test exception"));

        var fileName = "test_file.jpg";
        var contentType = "image/jpeg";

        _mockMessageService.Setup(x => x.GetMessage("ContentValidationError"))
            .Returns("Content validation error: {0}");
        _mockMessageService.Setup(x => x.GetMessage("FileTooSmallForValidation"))
            .Returns("Content validation error: {0}");

        // Act
        var result = await _fileValidationService.ValidateFileContentAsync(stream.Object, fileName, contentType);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Additional FileName Validation Tests

    [Fact]
    public void IsValidFileName_WithFileNameTooLong_ReturnsFalse()
    {
        // Arrange
        var fileName = new string('a', 300); // Exceeds max length

        // Act
        var result = _fileValidationService.IsValidFileName(fileName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFileName_WithDangerousChars_ReturnsFalse()
    {
        // Arrange
        var fileName = "test<file>.jpg"; // Contains dangerous chars

        // Act
        var result = _fileValidationService.IsValidFileName(fileName);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Additional PathTraversal Tests

    [Fact]
    public void ContainsPathTraversal_WithTilde_ReturnsTrue()
    {
        // Arrange
        var path = "~/test_file";

        // Act
        var result = _fileValidationService.ContainsPathTraversal(path);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ContainsPathTraversal_WithDangerousPathComponent_ReturnsTrue()
    {
        // Arrange
        var path = "etc/passwd";

        // Act
        var result = _fileValidationService.ContainsPathTraversal(path);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Additional SanitizeFileName Tests

    [Fact]
    public void SanitizeFileName_WithOnlyWhitespace_ReturnsDefaultName()
    {
        // Arrange
        var fileName = "   "; // Only whitespace

        // Act
        var result = _fileValidationService.SanitizeFileName(fileName);

        // Assert
        result.Should().Be("unnamed_file");
    }

    [Fact]
    public void SanitizeFileName_WithVeryLongName_TruncatesCorrectly()
    {
        // Arrange
        var longName = new string('a', 300);
        var fileName = $"{longName}.jpg";

        // Act
        var result = _fileValidationService.SanitizeFileName(fileName);

        // Assert
        result.Length.Should().BeLessThanOrEqualTo(255);
        result.Should().EndWith(".jpg");
    }

    #endregion
}