using FluentValidation.TestHelper;
using BlogApp.Application.Files.Commands;

namespace BlogApp.UnitTests.Application.Files.Commands;

public class GetPresignedUploadUrlCommandValidatorTests
{
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly GetPresignedUploadUrlCommandValidator _validator;

    public GetPresignedUploadUrlCommandValidatorTests()
    {
        _mockMessageService = new Mock<IMessageService>();
        _mockMessageService.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns((string key, object[] args) => $"Error: {key}");

        _validator = new GetPresignedUploadUrlCommandValidator(_mockMessageService.Object);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_UserId_Is_Empty()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_UserId_Is_Null()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = null!,
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("Error: UserIdRequired");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_UserId_Is_Valid()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_FileName_Is_Empty()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "",
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.FileName)
            .WithErrorMessage("Error: FileNameRequired");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_FileName_Is_Null()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = null!,
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.FileName)
            .WithErrorMessage("Error: FileNameRequired");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_FileName_Exceeds_Max_Length()
    {
        // Arrange
        var longFileName = new string('a', 256); // 256 characters, exceeds 255 limit
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = longFileName,
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.FileName)
            .WithErrorMessage("Error: FileNameMax");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_FileName_Is_Valid()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.FileName);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_ContentType_Is_Empty()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.ContentType)
            .WithErrorMessage("Error: ContentTypeRequired");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_ContentType_Is_Null()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = null!,
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.ContentType)
            .WithErrorMessage("Error: ContentTypeRequired");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_ContentType_Exceeds_Max_Length()
    {
        // Arrange
        var longContentType = new string('a', 256); // 256 characters, exceeds 255 limit
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = longContentType,
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.ContentType)
            .WithErrorMessage("Error: ContentTypeMax");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_ContentType_Is_Valid()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.ContentType);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_FileSize_Is_Zero()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 0
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.FileSize)
            .WithErrorMessage("Error: FileSizeMin");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_FileSize_Is_Negative()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = -1
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.FileSize)
            .WithErrorMessage("Error: FileSizeMin");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_FileSize_Exceeds_Max()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1048576L * 50 + 1 // Exceeds 50MB limit
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.FileSize)
            .WithErrorMessage("Error: FileSizeMax");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_FileSize_Is_Valid()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1048576L * 10 // 10MB, within limit
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.FileSize);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Have_Error_When_Description_Exceeds_Max_Length()
    {
        // Arrange
        var longDescription = new string('a', 1001); // 1001 characters, exceeds 1000 limit
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024,
                Description = longDescription
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Request.Description)
            .WithErrorMessage("Error: DescriptionMax");
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_Description_Is_Valid_Length()
    {
        // Arrange
        var validDescription = new string('a', 1000); // 1000 characters, within limit
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024,
                Description = validDescription
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_Description_Is_Null()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1024,
                Description = null
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Request.Description);
    }

    [Fact]
    public void GetPresignedUploadUrlCommandValidator_Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var model = new GetPresignedUploadUrlCommand
        {
            UserId = "valid-user-id",
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.txt",
                ContentType = "text/plain",
                FileSize = 1048576L * 10, // 10MB
                Description = "A test file for upload"
            }
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}