using BlogApp.Application.DTOs;
using Xunit;

namespace BlogApp.UnitTests.Application.DTOs;

public class FileDtoTests
{
    [Fact]
    public void FileDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fileName = "test-file.txt";
        var originalFileName = "original-test-file.txt";
        var contentType = "text/plain";
        var fileSize = 1024L;
        var filePath = "/path/to/file/test-file.txt";
        var description = "Test file description";
        var uploadedById = "test-user-id";
        var createdAt = DateTime.UtcNow;

        // Act
        var fileDto = new FileDto
        {
            Id = id,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            Description = description,
            UploadedById = uploadedById,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(id, fileDto.Id);
        Assert.Equal(fileName, fileDto.FileName);
        Assert.Equal(originalFileName, fileDto.OriginalFileName);
        Assert.Equal(contentType, fileDto.ContentType);
        Assert.Equal(fileSize, fileDto.FileSize);
        Assert.Equal(filePath, fileDto.FilePath);
        Assert.Equal(description, fileDto.Description);
        Assert.Equal(uploadedById, fileDto.UploadedById);
        Assert.Equal(createdAt, fileDto.CreatedAt);
    }

    [Fact]
    public void FileDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var fileDto = new FileDto
        {
            Id = Guid.NewGuid(),
            FileName = "test-file.txt",
            OriginalFileName = "original-test-file.txt",
            ContentType = "text/plain",
            FileSize = 1024L,
            FilePath = "/path/to/file/test-file.txt",
            Description = null,
            UploadedById = null,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Null(fileDto.Description);
        Assert.Null(fileDto.UploadedById);
    }
}

public class UploadFileDtoTests
{
    [Fact]
    public void UploadFileDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var fileName = "test-file.txt";
        var contentType = "text/plain";
        var fileSize = 1024L;
        var description = "Test file description";

        // Act
        var uploadFileDto = new UploadFileDto
        {
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            Description = description
        };

        // Assert
        Assert.Equal(fileName, uploadFileDto.FileName);
        Assert.Equal(contentType, uploadFileDto.ContentType);
        Assert.Equal(fileSize, uploadFileDto.FileSize);
        Assert.Equal(description, uploadFileDto.Description);
    }

    [Fact]
    public void UploadFileDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var uploadFileDto = new UploadFileDto
        {
            FileName = "test-file.txt",
            ContentType = "text/plain",
            FileSize = 1024L,
            Description = null
        };

        // Assert
        Assert.Null(uploadFileDto.Description);
    }
}

public class FileUploadResponseDtoTests
{
    [Fact]
    public void FileUploadResponseDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fileName = "test-file.txt";
        var originalFileName = "original-test-file.txt";
        var contentType = "text/plain";
        var fileSize = 1024L;
        var filePath = "/path/to/file/test-file.txt";
        var description = "Test file description";
        var createdAt = DateTime.UtcNow;

        // Act
        var responseDto = new FileUploadResponseDto
        {
            Id = id,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            Description = description,
            CreatedAt = createdAt
        };

        // Assert
        Assert.Equal(id, responseDto.Id);
        Assert.Equal(fileName, responseDto.FileName);
        Assert.Equal(originalFileName, responseDto.OriginalFileName);
        Assert.Equal(contentType, responseDto.ContentType);
        Assert.Equal(fileSize, responseDto.FileSize);
        Assert.Equal(filePath, responseDto.FilePath);
        Assert.Equal(description, responseDto.Description);
        Assert.Equal(createdAt, responseDto.CreatedAt);
    }

    [Fact]
    public void FileUploadResponseDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var responseDto = new FileUploadResponseDto
        {
            Id = Guid.NewGuid(),
            FileName = "test-file.txt",
            OriginalFileName = "original-test-file.txt",
            ContentType = "text/plain",
            FileSize = 1024L,
            FilePath = "/path/to/file/test-file.txt",
            Description = null,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        Assert.Null(responseDto.Description);
    }
}

public class PresignedUploadRequestDtoTests
{
    [Fact]
    public void PresignedUploadRequestDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var fileName = "test-file.txt";
        var contentType = "text/plain";
        var fileSize = 1024L;
        var description = "Test file description";

        // Act
        var requestDto = new PresignedUploadRequestDto
        {
            FileName = fileName,
            ContentType = contentType,
            FileSize = fileSize,
            Description = description
        };

        // Assert
        Assert.Equal(fileName, requestDto.FileName);
        Assert.Equal(contentType, requestDto.ContentType);
        Assert.Equal(fileSize, requestDto.FileSize);
        Assert.Equal(description, requestDto.Description);
    }

    [Fact]
    public void PresignedUploadRequestDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var requestDto = new PresignedUploadRequestDto
        {
            FileName = "test-file.txt",
            ContentType = "text/plain",
            FileSize = 1024L,
            Description = null
        };

        // Assert
        Assert.Null(requestDto.Description);
    }
}

public class PresignedUploadResponseDtoTests
{
    [Fact]
    public void PresignedUploadResponseDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var uploadUrl = "https://example.com/upload";
        var filePath = "/path/to/file/test-file.txt";
        var expiresAt = DateTime.UtcNow.AddHours(1);
        var formFields = new Dictionary<string, string> { { "key", "value" } };

        // Act
        var responseDto = new PresignedUploadResponseDto
        {
            FileId = fileId,
            UploadUrl = uploadUrl,
            FilePath = filePath,
            ExpiresAt = expiresAt,
            FormFields = formFields
        };

        // Assert
        Assert.Equal(fileId, responseDto.FileId);
        Assert.Equal(uploadUrl, responseDto.UploadUrl);
        Assert.Equal(filePath, responseDto.FilePath);
        Assert.Equal(expiresAt, responseDto.ExpiresAt);
        Assert.Same(formFields, responseDto.FormFields);
    }

    [Fact]
    public void PresignedUploadResponseDto_Should_Have_Default_FormFields()
    {
        // Act
        var responseDto = new PresignedUploadResponseDto();

        // Assert
        Assert.NotNull(responseDto.FormFields);
        Assert.Empty(responseDto.FormFields);
    }
}

public class CompleteUploadDtoTests
{
    [Fact]
    public void CompleteUploadDto_Should_Set_Properties_Correctly()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var actualFileSize = 1024L;
        var originalFileName = "original-test-file.txt";
        var contentType = "text/plain";

        // Act
        var completeDto = new CompleteUploadDto
        {
            FileId = fileId,
            ActualFileSize = actualFileSize,
            OriginalFileName = originalFileName,
            ContentType = contentType
        };

        // Assert
        Assert.Equal(fileId, completeDto.FileId);
        Assert.Equal(actualFileSize, completeDto.ActualFileSize);
        Assert.Equal(originalFileName, completeDto.OriginalFileName);
        Assert.Equal(contentType, completeDto.ContentType);
    }

    [Fact]
    public void CompleteUploadDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Act
        var completeDto = new CompleteUploadDto
        {
            FileId = Guid.NewGuid(),
            ActualFileSize = 1024L,
            OriginalFileName = null,
            ContentType = null
        };

        // Assert
        Assert.Null(completeDto.OriginalFileName);
        Assert.Null(completeDto.ContentType);
    }
}