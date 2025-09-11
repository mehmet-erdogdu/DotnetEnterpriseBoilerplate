namespace BlogApp.UnitTests.Application.Files.Commands;

public class CompleteUploadCommandHandlerTests : BaseTestClass
{
    private readonly CompleteUploadCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFileService> _mockFileService;

    public CompleteUploadCommandHandlerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new CompleteUploadCommandHandler(
            _mockFileService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new CompleteUploadCommand
        {
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024
            },
            UserId = "test-user-id"
        };

        var expectedResponse = new FileUploadResponseDto
        {
            Id = command.CompleteDto.FileId,
            FileName = "test-file.jpg",
            OriginalFileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            FilePath = "/uploads/test-file.jpg",
            Description = "Test file",
            CreatedAt = DateTime.UtcNow
        };

        _mockFileService.Setup(x => x.CompleteUploadAsync(command.CompleteDto, command.UserId))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(expectedResponse.Id);
        result.Data.FileName.Should().Be(expectedResponse.FileName);

        _mockFileService.Verify(x => x.CompleteUploadAsync(command.CompleteDto, command.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new CompleteUploadCommand
        {
            CompleteDto = new CompleteUploadDto
            {
                FileId = Guid.NewGuid(),
                ActualFileSize = 1024
            },
            UserId = "test-user-id"
        };

        _mockFileService.Setup(x => x.CompleteUploadAsync(command.CompleteDto, command.UserId))
            .ThrowsAsync(new Exception("Upload completion failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("UploadCompletionFailed"))
            .Returns("Upload completion failed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Upload completion failed");
    }
}