namespace BlogApp.UnitTests.Controllers;

public class FilesControllerTests : BaseControllerTest
{
    private readonly FilesController _controller;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    public FilesControllerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _controller = new FilesController(_mockMediator.Object, _mockCurrentUserService.Object, _mockUnitOfWork.Object);
    }

    #region GetPresignedUploadUrl Tests

    [Fact]
    public async Task GetPresignedUploadUrl_WithValidRequest_ShouldReturnPresignedUrl()
    {
        // Arrange
        var request = new PresignedUploadRequestDto
        {
            FileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            Description = "Test file"
        };

        var response = new PresignedUploadResponseDto
        {
            FileId = Guid.NewGuid(),
            UploadUrl = "https://s3.example.com/presigned-url",
            FilePath = "uploads/test-file.jpg",
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            FormFields = new Dictionary<string, string>
            {
                { "key", "uploads/test-file.jpg" },
                { "policy", "base64-policy" }
            }
        };

        var apiResponse = ApiResponse<PresignedUploadResponseDto>.Success(response);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        _mockUnitOfWork.Setup(x => x.SaveChangesAsync()).ReturnsAsync(true);

        // Act
        var result = await _controller.GetPresignedUploadUrl(request);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.UploadUrl.Should().Be("https://s3.example.com/presigned-url");
        result.Data.FormFields.Should().ContainKey("key");

        _mockMediator.Verify(x => x.Send(It.Is<GetPresignedUploadUrlCommand>(cmd =>
            cmd.Request.FileName == request.FileName &&
            cmd.Request.ContentType == request.ContentType &&
            cmd.Request.FileSize == request.FileSize &&
            cmd.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);

        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetPresignedUploadUrl_WithInvalidFileSize_ShouldReturnFailure()
    {
        // Arrange
        var request = new PresignedUploadRequestDto
        {
            FileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 0, // Invalid size
            Description = "Test file"
        };

        var apiResponse = ApiResponse<PresignedUploadResponseDto>.Failure("File size must be greater than 0");
        _mockMediator.Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetPresignedUploadUrl(request);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File size must be greater than 0");
    }

    [Fact]
    public async Task GetPresignedUploadUrl_WithDangerousFile_ShouldReturnFailure()
    {
        // Arrange
        var request = new PresignedUploadRequestDto
        {
            FileName = "malicious.exe",
            ContentType = "application/octet-stream",
            FileSize = 1024,
            Description = "Dangerous file"
        };

        var apiResponse = ApiResponse<PresignedUploadResponseDto>.Failure("File type not allowed");
        _mockMediator.Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetPresignedUploadUrl(request);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File type not allowed");
    }

    #endregion

    #region CompleteUpload Tests

    [Fact]
    public async Task CompleteUpload_WithValidData_ShouldReturnFileResponse()
    {
        // Arrange
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024,
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg"
        };

        var fileResponse = new FileUploadResponseDto
        {
            Id = completeDto.FileId,
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            FilePath = "uploads/test-file.jpg",
            Description = "Test file",
            CreatedAt = DateTime.UtcNow
        };

        var apiResponse = ApiResponse<FileUploadResponseDto>.Success(fileResponse);
        _mockMediator.Setup(x => x.Send(It.IsAny<CompleteUploadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.CompleteUpload(completeDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(completeDto.FileId);
        result.Data.FileSize.Should().Be(1024);

        _mockMediator.Verify(x => x.Send(It.Is<CompleteUploadCommand>(cmd =>
            cmd.CompleteDto.FileId == completeDto.FileId &&
            cmd.CompleteDto.ActualFileSize == completeDto.ActualFileSize &&
            cmd.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompleteUpload_WithNonExistingFile_ShouldReturnFailure()
    {
        // Arrange
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024
        };

        var apiResponse = ApiResponse<FileUploadResponseDto>.Failure("File not found");
        _mockMediator.Setup(x => x.Send(It.IsAny<CompleteUploadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.CompleteUpload(completeDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File not found");
    }

    [Fact]
    public async Task CompleteUpload_WithFileSizeMismatch_ShouldReturnFailure()
    {
        // Arrange
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 2048 // Different from expected size
        };

        var apiResponse = ApiResponse<FileUploadResponseDto>.Failure("File size mismatch");
        _mockMediator.Setup(x => x.Send(It.IsAny<CompleteUploadCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.CompleteUpload(completeDto);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File size mismatch");
    }

    #endregion

    #region GetFile Tests

    [Fact]
    public async Task GetFile_WithExistingFile_ShouldReturnFile()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var fileDto = TestHelper.TestData.CreateTestFileDto(id: fileId);

        var apiResponse = ApiResponse<FileDto>.Success(fileDto);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetFile(fileId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(fileId);

        _mockMediator.Verify(x => x.Send(It.Is<GetFileQuery>(q =>
            q.FileId == fileId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFile_WithNonExistingFile_ShouldReturnFailure()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var apiResponse = ApiResponse<FileDto>.Failure("File not found");
        _mockMediator.Setup(x => x.Send(It.IsAny<GetFileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetFile(fileId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File not found");
    }

    #endregion

    #region DownloadFile Tests

    [Fact]
    public async Task DownloadFile_WithExistingFile_ShouldReturnFileResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes("test file content"));

        var downloadResult = new DownloadFileResult
        {
            Success = true,
            FileStream = fileStream,
            FileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<DownloadFileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _controller.DownloadFile(fileId);

        // Assert
        result.Should().BeOfType<FileStreamResult>();
        var fileResult = result as FileStreamResult;
        fileResult!.FileDownloadName.Should().Be("test-file.jpg");
        fileResult.ContentType.Should().Be("image/jpeg");

        _mockMediator.Verify(x => x.Send(It.Is<DownloadFileQuery>(q =>
            q.FileId == fileId &&
            q.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DownloadFile_WithNonExistingFile_ShouldReturnNotFound()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var downloadResult = new DownloadFileResult
        {
            Success = false,
            ErrorMessage = "File not found"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<DownloadFileQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(downloadResult);

        // Act
        var result = await _controller.DownloadFile(fileId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().BeOfType<ApiResponse<object>>();
    }

    #endregion

    #region DeleteFile Tests

    [Fact]
    public async Task DeleteFile_WithExistingFileAndOwner_ShouldReturnSuccess()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var apiResponse = ApiResponse<bool>.Success(true);
        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.DeleteFile(fileId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockMediator.Verify(x => x.Send(It.Is<DeleteFileCommand>(cmd =>
            cmd.FileId == fileId &&
            cmd.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFile_WithNonExistingFile_ShouldReturnFailure()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var apiResponse = ApiResponse<bool>.Failure("File not found");
        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.DeleteFile(fileId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File not found");
    }

    [Fact]
    public async Task DeleteFile_WithUnauthorizedUser_ShouldReturnFailure()
    {
        // Arrange
        var fileId = Guid.NewGuid();

        var apiResponse = ApiResponse<bool>.Failure("Unauthorized access");
        _mockMediator.Setup(x => x.Send(It.IsAny<DeleteFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.DeleteFile(fileId);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Unauthorized access");
    }

    #endregion

    #region GetMyFiles Tests

    [Fact]
    public async Task GetMyFiles_WithUserFiles_ShouldReturnFilesList()
    {
        // Arrange
        var files = new List<FileDto>
        {
            TestHelper.TestData.CreateTestFileDto(),
            TestHelper.TestData.CreateTestFileDto()
        };

        var apiResponse = ApiResponse<IEnumerable<FileDto>>.Success(files);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserFilesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetMyFiles();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);

        _mockMediator.Verify(x => x.Send(It.Is<GetUserFilesQuery>(q =>
            q.UserId == "test-user-id"
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyFiles_WithNoFiles_ShouldReturnEmptyList()
    {
        // Arrange
        var files = new List<FileDto>();

        var apiResponse = ApiResponse<IEnumerable<FileDto>>.Success(files);
        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserFilesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetMyFiles();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyFiles_WithDatabaseError_ShouldReturnFailure()
    {
        // Arrange
        var apiResponse = ApiResponse<IEnumerable<FileDto>>.Failure("Database error occurred");
        _mockMediator.Setup(x => x.Send(It.IsAny<GetUserFilesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await _controller.GetMyFiles();

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Database error occurred");
    }

    #endregion

    #region Helper Methods

    private static PresignedUploadRequestDto CreateValidUploadRequest()
    {
        return new PresignedUploadRequestDto
        {
            FileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            Description = "Test file"
        };
    }

    private static CompleteUploadDto CreateValidCompleteUploadRequest(Guid? fileId = null)
    {
        return new CompleteUploadDto
        {
            FileId = fileId ?? Guid.NewGuid(),
            ActualFileSize = 1024,
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg"
        };
    }

    #endregion
}