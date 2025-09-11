namespace BlogApp.UnitTests.Application.Files.Queries;

public class DownloadFileQueryHandlerTests : BaseApplicationTest
{
    private readonly DownloadFileQueryHandler _handler;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IFileRepository> _mockFileRepository;
    private readonly Mock<IMessageService> _mockMessageServiceLocal;

    public DownloadFileQueryHandlerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockFileRepository = new Mock<IFileRepository>();
        _mockMessageServiceLocal = TestHelper.MockSetups.CreateMockMessageService();

        _handler = new DownloadFileQueryHandler(
            _mockFileService.Object,
            _mockFileRepository.Object,
            _mockMessageServiceLocal.Object);
    }

    [Fact]
    public async Task Handle_WithValidFileAndPermission_ShouldReturnSuccessResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = userId
        };

        var file = TestHelper.TestData.CreateTestFile(id: fileId, uploadedById: userId);
        var fileStream = new MemoryStream();

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(file);

        _mockFileService.Setup(x => x.DownloadFileAsync(fileId))
            .ReturnsAsync(fileStream);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.FileStream.Should().BeSameAs(fileStream);
        result.FileName.Should().Be(file.OriginalFileName);
        result.ContentType.Should().Be(file.ContentType);
        result.FileSize.Should().Be(file.FileSize);
        result.ErrorMessage.Should().BeNull();

        _mockFileRepository.Verify(x => x.GetByIdAsync(fileId), Times.Once);
        _mockFileService.Verify(x => x.DownloadFileAsync(fileId), Times.Once);
        _mockMessageServiceLocal.Verify(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentFile_ShouldReturnFailureResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = userId
        };
        var errorMessage = "File not found";

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync((FileEntity?)null);

        _mockMessageServiceLocal.Setup(x => x.GetMessage("FileNotFound"))
            .Returns(errorMessage);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.FileStream.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        _mockFileRepository.Verify(x => x.GetByIdAsync(fileId), Times.Once);
        _mockFileService.Verify(x => x.DownloadFileAsync(It.IsAny<Guid>()), Times.Never);
        _mockMessageServiceLocal.Verify(x => x.GetMessage("FileNotFound"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFileAccessDenied_ShouldReturnFailureResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var ownerId = "other-user-id";
        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = userId
        };
        var errorMessage = "Access denied";

        var file = TestHelper.TestData.CreateTestFile(id: fileId, uploadedById: ownerId);

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(file);

        _mockMessageServiceLocal.Setup(x => x.GetMessage("FileAccessDenied"))
            .Returns(errorMessage);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.FileStream.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        _mockFileRepository.Verify(x => x.GetByIdAsync(fileId), Times.Once);
        _mockFileService.Verify(x => x.DownloadFileAsync(It.IsAny<Guid>()), Times.Never);
        _mockMessageServiceLocal.Verify(x => x.GetMessage("FileAccessDenied"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFileNotFoundException_ShouldReturnFailureResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = userId
        };
        var errorMessage = "File not found";

        var file = TestHelper.TestData.CreateTestFile(id: fileId, uploadedById: userId);

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(file);

        _mockFileService.Setup(x => x.DownloadFileAsync(fileId))
            .ThrowsAsync(new FileNotFoundException());

        _mockMessageServiceLocal.Setup(x => x.GetMessage("FileNotFound"))
            .Returns(errorMessage);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.FileStream.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        _mockFileRepository.Verify(x => x.GetByIdAsync(fileId), Times.Once);
        _mockFileService.Verify(x => x.DownloadFileAsync(fileId), Times.Once);
        _mockMessageServiceLocal.Verify(x => x.GetMessage("FileNotFound"), Times.Once);
    }

    [Fact]
    public async Task Handle_WithGeneralException_ShouldReturnFailureResult()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var userId = "test-user-id";
        var query = new DownloadFileQuery
        {
            FileId = fileId,
            UserId = userId
        };
        var errorMessage = "Download failed";

        var file = TestHelper.TestData.CreateTestFile(id: fileId, uploadedById: userId);

        _mockFileRepository.Setup(x => x.GetByIdAsync(fileId))
            .ReturnsAsync(file);

        _mockFileService.Setup(x => x.DownloadFileAsync(fileId))
            .ThrowsAsync(new Exception("Download error"));

        _mockMessageServiceLocal.Setup(x => x.GetMessage("FileDownloadFailed"))
            .Returns(errorMessage);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.FileStream.Should().BeNull();
        result.ErrorMessage.Should().Be(errorMessage);

        _mockFileRepository.Verify(x => x.GetByIdAsync(fileId), Times.Once);
        _mockFileService.Verify(x => x.DownloadFileAsync(fileId), Times.Once);
        _mockMessageServiceLocal.Verify(x => x.GetMessage("FileDownloadFailed"), Times.Once);
    }
}