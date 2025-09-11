namespace BlogApp.UnitTests.Application.Files.Commands;

public class DeleteFileCommandHandlerTests : BaseTestClass
{
    private readonly DeleteFileCommandHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFileService> _mockFileService;

    public DeleteFileCommandHandlerTests()
    {
        _mockFileService = new Mock<IFileService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new DeleteFileCommandHandler(
            _mockFileService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var command = new DeleteFileCommand
        {
            FileId = Guid.NewGuid(),
            UserId = "test-user-id"
        };

        _mockFileService.Setup(x => x.DeleteFileAsync(command.FileId, command.UserId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().BeTrue();

        _mockFileService.Verify(x => x.DeleteFileAsync(command.FileId, command.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new DeleteFileCommand
        {
            FileId = Guid.NewGuid(),
            UserId = "unauthorized-user-id"
        };

        _mockFileService.Setup(x => x.DeleteFileAsync(command.FileId, command.UserId))
            .ReturnsAsync(false);

        _mockErrorMessageService.Setup(x => x.GetMessage("FileOperationUnauthorized"))
            .Returns("File operation unauthorized");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File operation unauthorized");
    }

    [Fact]
    public async Task Handle_WhenServiceThrowsException_ShouldReturnFailureResponse()
    {
        // Arrange
        var command = new DeleteFileCommand
        {
            FileId = Guid.NewGuid(),
            UserId = "test-user-id"
        };

        _mockFileService.Setup(x => x.DeleteFileAsync(command.FileId, command.UserId))
            .ThrowsAsync(new Exception("Delete operation failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("FileDeleteFailed"))
            .Returns("File delete failed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "File delete failed");
    }
}