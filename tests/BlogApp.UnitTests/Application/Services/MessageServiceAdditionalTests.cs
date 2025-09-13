namespace BlogApp.UnitTests.Application.Services;

public class MessageServiceAdditionalTests
{
    private readonly MessageService _messageService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public MessageServiceAdditionalTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _messageService = new MessageService(_mockHttpContextAccessor.Object);
    }

    [Fact]
    public void GetMessage_WithNullHttpContext_ShouldReturnEnglishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithEmptyLanguageHeader_ShouldReturnEnglishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.AcceptLanguage = ""; // Empty header

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithWhitespaceLanguageHeader_ShouldReturnEnglishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.AcceptLanguage = "   "; // Whitespace only

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithMultipleLanguagePreferences_ShouldReturnMessageForFirstPreference()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required"; // English message

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.AcceptLanguage = "fr-FR,en-US;q=0.9,tr-TR;q=0.8"; // Multiple preferences

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithTurkishLanguageAsFirstPreference_ShouldReturnTurkishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "E-posta zorunludur"; // Turkish translation

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.AcceptLanguage = "tr-TR,en-US;q=0.9,fr-FR;q=0.8"; // Turkish first

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithFormattedMessageAndArgs_ShouldReturnFormattedMessage()
    {
        // Arrange
        var key = "UserNotFound";
        var userId = "test-user-id";
        var args = new object[] { userId };
        var expectedMessage = "User not found";

        // Act
        var result = _messageService.GetMessage(key, args);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithFormattedMessageAndMultipleArgs_ShouldReturnFormattedMessage()
    {
        // Arrange
        var key = "FileSizeInvalid";
        var minSize = 1;
        var maxSize = 100;
        var args = new object[] { minSize, maxSize };
        var expectedMessage = "File size must be between 0 and 1 MB";

        // Act
        var result = _messageService.GetMessage(key, args);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithNonExistentResourceFile_ShouldHandleGracefully()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.AcceptLanguage = "de-DE"; // German - not supported

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }
}