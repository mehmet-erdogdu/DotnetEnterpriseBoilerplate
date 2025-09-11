namespace BlogApp.UnitTests.Domain.Entities;

public class FileEntityTests : BaseTestClass
{
    [Fact]
    public void FileEntity_Properties_CanBeSetAndRetrieved()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fileName = "test-file.jpg";
        var originalFileName = "original-test-file.jpg";
        var contentType = "image/jpeg";
        var fileSize = 1024L;
        var filePath = "uploads/2024/01/01/test-file.jpg";
        var description = "Test file description";
        var uploadedById = "test-user-id";
        var createdAt = DateTime.UtcNow;
        var updatedAt = DateTime.UtcNow.AddMinutes(5);

        // Act
        var fileEntity = new FileEntity
        {
            Id = id,
            FileName = fileName,
            OriginalFileName = originalFileName,
            ContentType = contentType,
            FileSize = fileSize,
            FilePath = filePath,
            Description = description,
            UploadedById = uploadedById,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        fileEntity.Id.Should().Be(id);
        fileEntity.FileName.Should().Be(fileName);
        fileEntity.OriginalFileName.Should().Be(originalFileName);
        fileEntity.ContentType.Should().Be(contentType);
        fileEntity.FileSize.Should().Be(fileSize);
        fileEntity.FilePath.Should().Be(filePath);
        fileEntity.Description.Should().Be(description);
        fileEntity.UploadedById.Should().Be(uploadedById);
        fileEntity.CreatedAt.Should().Be(createdAt);
        fileEntity.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public void FileEntity_WithRequiredProperties_ShouldBeValid()
    {
        // Act
        var fileEntity = new FileEntity
        {
            FileName = "test-file.jpg",
            OriginalFileName = "original-test-file.jpg",
            ContentType = "image/jpeg",
            FilePath = "uploads/test-file.jpg"
        };

        // Assert
        fileEntity.FileName.Should().Be("test-file.jpg");
        fileEntity.OriginalFileName.Should().Be("original-test-file.jpg");
        fileEntity.ContentType.Should().Be("image/jpeg");
        fileEntity.FilePath.Should().Be("uploads/test-file.jpg");
        fileEntity.FileSize.Should().Be(0); // Default value
        fileEntity.Description.Should().BeNull(); // Default value
        fileEntity.UploadedById.Should().BeNull(); // Default value
    }

    [Fact]
    public void FileEntity_WithNullDescription_ShouldBeValid()
    {
        // Act
        var fileEntity = new FileEntity
        {
            FileName = "test-file.jpg",
            OriginalFileName = "original-test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024L,
            FilePath = "uploads/test-file.jpg",
            Description = null
        };

        // Assert
        fileEntity.Description.Should().BeNull();
    }

    [Fact]
    public void FileEntity_WithNullUploadedById_ShouldBeValid()
    {
        // Act
        var fileEntity = new FileEntity
        {
            FileName = "test-file.jpg",
            OriginalFileName = "original-test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024L,
            FilePath = "uploads/test-file.jpg",
            UploadedById = null
        };

        // Assert
        fileEntity.UploadedById.Should().BeNull();
    }
}