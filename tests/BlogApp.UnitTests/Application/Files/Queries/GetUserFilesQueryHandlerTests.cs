namespace BlogApp.UnitTests.Application.Files.Queries;

public class GetUserFilesQueryHandlerTests : BaseTestClass
{
    private readonly GetUserFilesQueryHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFileService> _mockFileService;

    public GetUserFilesQueryHandlerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new GetUserFilesQueryHandler(
            _mockFileService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var query = new GetUserFilesQuery
        {
            UserId = "test-user-id"
        };

        var expectedFiles = new List<FileDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                FileName = "file1.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024,
                UploadedById = query.UserId,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                FileName = "file2.pdf",
                ContentType = "application/pdf",
                FileSize = 2048,
                UploadedById = query.UserId,
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            }
        };

        _mockFileService.Setup(x => x.GetUserFilesAsync(query.UserId))
            .ReturnsAsync(expectedFiles);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data.Should().BeEquivalentTo(expectedFiles);

        _mockFileService.Verify(x => x.GetUserFilesAsync(query.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserWithoutFiles_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetUserFilesQuery
        {
            UserId = "user-without-files"
        };

        var emptyFiles = new List<FileDto>();

        _mockFileService.Setup(x => x.GetUserFilesAsync(query.UserId))
            .ReturnsAsync(emptyFiles);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var query = new GetUserFilesQuery
        {
            UserId = "test-user-id"
        };

        _mockFileService.Setup(x => x.GetUserFilesAsync(query.UserId))
            .ThrowsAsync(new Exception("Database connection failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("FileListFailed"))
            .Returns("Failed to retrieve file list");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Failed to retrieve file list");
    }
}