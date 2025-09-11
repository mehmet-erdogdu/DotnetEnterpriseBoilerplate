namespace BlogApp.UnitTests.Application.Files.Queries;

public class GetFileQueryHandlerTests : BaseTestClass
{
    private readonly GetFileQueryHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFileService> _mockFileService;

    public GetFileQueryHandlerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new GetFileQueryHandler(
            _mockFileService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithExistingFile_ShouldReturnSuccessResponse()
    {
        // Arrange
        var query = new GetFileQuery
        {
            FileId = Guid.NewGuid()
        };

        var expectedFile = new FileDto
        {
            Id = query.FileId,
            FileName = "test-file.jpg",
            ContentType = "image/jpeg",
            FileSize = 1024,
            Description = "Test file",
            CreatedAt = DateTime.UtcNow,
            UploadedById = "test-user-id"
        };

        _mockFileService.Setup(x => x.GetFileAsync(query.FileId))
            .ReturnsAsync(expectedFile);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(expectedFile.Id);
        result.Data.FileName.Should().Be(expectedFile.FileName);
        result.Data.ContentType.Should().Be(expectedFile.ContentType);
        result.Data.FileSize.Should().Be(expectedFile.FileSize);

        _mockFileService.Verify(x => x.GetFileAsync(query.FileId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ShouldReturnFailureResponse()
    {
        // Arrange
        var query = new GetFileQuery
        {
            FileId = Guid.NewGuid()
        };

        _mockFileService.Setup(x => x.GetFileAsync(query.FileId))
            .ReturnsAsync((FileDto?)null);

        _mockErrorMessageService.Setup(x => x.GetMessage("FileNotFound"))
            .Returns("File not found");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File not found");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var query = new GetFileQuery
        {
            FileId = Guid.NewGuid()
        };

        _mockFileService.Setup(x => x.GetFileAsync(query.FileId))
            .ThrowsAsync(new Exception("Database connection failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("FileListFailed"))
            .Returns("File operation failed");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File operation failed");
    }
}