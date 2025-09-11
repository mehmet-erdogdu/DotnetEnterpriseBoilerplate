namespace BlogApp.UnitTests.Application.Services;

public class MessageServiceTests : BaseServiceTest
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly MessageService _messageService;

    public MessageServiceTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _messageService = new MessageService(_mockHttpContextAccessor.Object);
    }

    [Fact]
    public void GetMessage_WithValidKey_ShouldReturnMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithValidKeyAndArgs_ShouldReturnFormattedMessage()
    {
        // Arrange
        var key = "TodoNotFound";
        var args = new object[] { Guid.NewGuid() };
        var expectedMessage = $"Todo with ID {args[0]} not found";

        // Act
        var result = _messageService.GetMessage(key, args);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithInvalidKey_ShouldReturnKey()
    {
        // Arrange
        var key = "NonExistentKey";

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(key);
    }

    [Fact]
    public void GetMessage_WithEnglishLanguageHeader_ShouldReturnEnglishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithTurkishLanguageHeader_ShouldReturnTurkishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "E-posta zorunludur"; // Turkish translation
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "tr-TR";
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithUnsupportedLanguage_ShouldReturnEnglishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "fr-FR"; // Unsupported language
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }

    [Fact]
    public void GetMessage_WithoutLanguageHeader_ShouldReturnEnglishMessage()
    {
        // Arrange
        var key = "EmailRequired";
        var expectedMessage = "Email is required";
        
        var httpContext = new DefaultHttpContext();
        // No Accept-Language header
        
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        // Act
        var result = _messageService.GetMessage(key);

        // Assert
        result.Should().Be(expectedMessage);
    }
}