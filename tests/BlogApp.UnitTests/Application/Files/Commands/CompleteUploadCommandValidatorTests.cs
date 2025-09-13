using FluentValidation.TestHelper;

namespace BlogApp.UnitTests.Application.Files.Commands;

public class CompleteUploadCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly CompleteUploadCommandValidator _validator;

    public CompleteUploadCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new CompleteUploadCommandValidator(_mockMessageService.Object);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_UserId_Is_Null()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = null!,
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_FileId_Is_Empty()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.Empty,
                ActualFileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompleteDto.FileId)
            .WithErrorMessage("Error: FileIdRequired");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_FileId_Is_Valid()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompleteDto.FileId);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_ActualFileSize_Is_Zero()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 0
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompleteDto.ActualFileSize)
            .WithErrorMessage("Error: FileSizeMin");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_ActualFileSize_Is_Negative()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = -1
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompleteDto.ActualFileSize)
            .WithErrorMessage("Error: FileSizeMin");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_ActualFileSize_Exceeds_Max()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1048576L * 50 + 1 // Exceeds 50MB limit
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompleteDto.ActualFileSize)
            .WithErrorMessage("Error: FileSizeMax");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_ActualFileSize_Is_Valid()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1048576L * 10 // 10MB, within limit
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompleteDto.ActualFileSize);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_OriginalFileName_Exceeds_Max_Length()
    {
        // Arrange
        var longFileName = new string('a', 256); // 256 characters, exceeds 255 limit
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024,
                OriginalFileName = longFileName
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompleteDto.OriginalFileName)
            .WithErrorMessage("Error: FileNameMax");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_OriginalFileName_Is_Valid_Length()
    {
        // Arrange
        var validFileName = new string('a', 255); // 255 characters, within limit
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024,
                OriginalFileName = validFileName
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompleteDto.OriginalFileName);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_OriginalFileName_Is_Null()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024,
                OriginalFileName = null
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompleteDto.OriginalFileName);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Have_Error_When_ContentType_Exceeds_Max_Length()
    {
        // Arrange
        var longContentType = new string('a', 256); // 256 characters, exceeds 255 limit
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024,
                ContentType = longContentType
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompleteDto.ContentType)
            .WithErrorMessage("Error: ContentTypeMax");
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_ContentType_Is_Valid_Length()
    {
        // Arrange
        var validContentType = new string('a', 255); // 255 characters, within limit
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024,
                ContentType = validContentType
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompleteDto.ContentType);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_ContentType_Is_Null()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024,
                ContentType = null
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompleteDto.ContentType);
    }

    [Fact]
    public void CompleteUploadCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new CompleteUploadCommand
        {
            UserId = "valid-user-id",
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1048576L * 10, // 10MB
                OriginalFileName = "test-file.txt",
                ContentType = "text/plain"
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}