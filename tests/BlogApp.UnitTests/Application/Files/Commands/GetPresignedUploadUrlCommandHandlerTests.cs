namespace BlogApp.UnitTests.Application.Files.Commands;

public class GetPresignedUploadUrlCommandHandlerTests : BaseTestClass
{
    private readonly GetPresignedUploadUrlCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFileService> _mockFileService;

    public GetPresignedUploadUrlCommandHandlerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new GetPresignedUploadUrlCommandHandler(
            _mockFileService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new GetPresignedUploadUrlCommand
        {
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024 * 1024, // 1MB
                Description = "Test file description"
            },
            UserId = "test-user-id"
        };

        var expectedResponse = new PresignedUploadResponseDto
        {
            UploadUrl = "https://s3.example.com/presigned-url",
            FormFields = new Dictionary<string, string>
            {
                { "key", "uploads/test-file.jpg" },
                { "policy", "base64-encoded-policy" }
            },
            FileId = Guid.NewGuid()
        };

        _mockFileService.Setup(x => x.GetPresignedUploadUrlAsync(command.Request, command.UserId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.UploadUrl.Should().Be(expectedResponse.UploadUrl);
        result.Data.FormFields.Should().BeEquivalentTo(expectedResponse.FormFields);
        result.Data.FileId.Should().Be(expectedResponse.FileId);

        _mockFileService.Verify(x => x.GetPresignedUploadUrlAsync(command.Request, command.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new GetPresignedUploadUrlCommand
        {
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024,
                Description = "Test file"
            },
            UserId = "test-user-id"
        };

        _mockFileService.Setup(x => x.GetPresignedUploadUrlAsync(command.Request, command.UserId))
            .ThrowsAsync(new Exception("S3 service error"));

        _mockErrorMessageService.Setup(x => x.GetMessage("PresignedUrlGenerationFailed"))
            .Returns("Presigned URL generation failed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Presigned URL generation failed");
    }

    [Fact]
    public async Task Handle_WithNullResponse_ShouldReturnSuccessWithNullData()
    {
        // Arrange
        var command = new GetPresignedUploadUrlCommand
        {
            Request = new PresignedUploadRequestDto
            {
                FileName = "test-file.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024,
                Description = "Test file"
            },
            UserId = "test-user-id"
        };

        _mockFileService.Setup(x => x.GetPresignedUploadUrlAsync(command.Request, command.UserId))
            .ReturnsAsync((PresignedUploadResponseDto)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Data.Should().BeNull();
    }
}