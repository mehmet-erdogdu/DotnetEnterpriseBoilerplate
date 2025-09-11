namespace BlogApp.UnitTests.Application.FirebaseNotifications.Queries;

public class GetDeviceTokensQueryHandlerTests : BaseTestClass
{
    private readonly GetDeviceTokensQueryHandler _handler;
    private readonly Mock<IMessageService> _mockErrorMessageService;
    private readonly Mock<IFirebaseNotificationService> _mockFirebaseNotificationService;

    public GetDeviceTokensQueryHandlerTests()
    {
        _mockFirebaseNotificationService = new Mock<IFirebaseNotificationService>();
        _mockErrorMessageService = new Mock<IMessageService>();

        _handler = new GetDeviceTokensQueryHandler(
            _mockFirebaseNotificationService.Object,
            _mockErrorMessageService.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnSuccessResponse()
    {
        // Arrange
        var query = new GetDeviceTokensQuery
        {
            UserId = "test-user-id"
        };

        var expectedTokens = new List<string>
        {
            "device-token-1",
            "device-token-2",
            "device-token-3"
        };

        _mockFirebaseNotificationService.Setup(x => x.GetUserDeviceTokensAsync(query.UserId))
            .ReturnsAsync(expectedTokens);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseSuccess(result);
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(3);
        result.Data.Should().BeEquivalentTo(expectedTokens);

        _mockFirebaseNotificationService.Verify(x => x.GetUserDeviceTokensAsync(query.UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUserWithoutTokens_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetDeviceTokensQuery
        {
            UserId = "user-without-tokens"
        };

        var emptyTokens = new List<string>();

        _mockFirebaseNotificationService.Setup(x => x.GetUserDeviceTokensAsync(query.UserId))
            .ReturnsAsync(emptyTokens);

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
        var query = new GetDeviceTokensQuery
        {
            UserId = "test-user-id"
        };

        _mockFirebaseNotificationService.Setup(x => x.GetUserDeviceTokensAsync(query.UserId))
            .ThrowsAsync(new Exception("Database connection failed"));

        _mockErrorMessageService.Setup(x => x.GetMessage("InternalServerError"))
            .Returns("Internal server error");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        TestHelper.AssertHelpers.AssertApiResponseFailure(result, "Internal server error");
    }
}