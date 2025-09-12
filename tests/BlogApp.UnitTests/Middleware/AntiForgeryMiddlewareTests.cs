namespace BlogApp.UnitTests.Middleware;

public class AntiForgeryMiddlewareTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMessageService> _messageServiceMock;
    private readonly AntiForgeryMiddleware _middleware;
    private readonly Mock<RequestDelegate> _nextMock;

    public AntiForgeryMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _messageServiceMock = new Mock<IMessageService>();
        _configurationMock = new Mock<IConfiguration>();
        _middleware = new AntiForgeryMiddleware(_nextMock.Object);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    [InlineData("OPTIONS")]
    public async Task InvokeAsync_ShouldCallNext_WhenMethodIsNotStateChanging(string method)
    {
        // Arrange
        var context = CreateHttpContext(method);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_ShouldCallNext_WhenStateChangingMethodAndValidOrigin(string method)
    {
        // Arrange
        var context = CreateHttpContext(method);
        context.Request.Headers.Origin = "https://example.com";
        context.Request.Headers.Referer = "https://example.com/page";

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns("https://example.com");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_ShouldReturn403_WhenStateChangingMethodAndInvalidOrigin(string method)
    {
        // Arrange
        var context = CreateHttpContext(method);
        context.Request.Headers.Origin = "https://malicious.com";
        context.Request.Headers.Referer = "https://malicious.com/page";

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns("https://example.com");
        _messageServiceMock.Setup(e => e.GetMessage("CSRFInvalidOriginOrReferer"))
            .Returns("CSRF validation failed");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _nextMock.Verify(n => n(context), Times.Never);

        // Verify ApiResponse format
        var responseBody = GetResponseBody(context);
        responseBody.Should().Contain("CSRF validation failed");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("\"Error\":");
        responseBody.Should().Contain("CSRF_VALIDATION_FAILED");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task InvokeAsync_ShouldCallNext_WhenNoOriginHeaders(string method)
    {
        // Arrange
        var context = CreateHttpContext(method);
        _configurationMock.Setup(c => c["FrontendUrls"]).Returns("https://example.com");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    public async Task InvokeAsync_ShouldCallNext_WhenNoConfigurationAvailable(string method)
    {
        // Arrange
        var context = CreateHttpContext(method);
        context.Request.Headers.Origin = "https://malicious.com";
        context.Request.Headers.Referer = "https://malicious.com/page";

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns((string?)null);
        _configurationMock.Setup(c => c["APIUrl"]).Returns((string?)null);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("https://app.example.com,https://admin.example.com", "https://app.example.com", "https://app.example.com/login", true)]
    [InlineData("https://app.example.com,https://admin.example.com", "https://admin.example.com", "https://admin.example.com/dashboard", true)]
    [InlineData("https://app.example.com,https://admin.example.com", "https://malicious.com", "https://malicious.com/page", false)]
    [InlineData("https://example.com", "https://example.com", "https://different.com", false)]
    [InlineData("https://example.com", "https://different.com", "https://example.com", false)]
    public async Task InvokeAsync_ShouldValidateMultipleFrontendUrls(string frontendUrls, string origin, string referer, bool shouldAllow)
    {
        // Arrange
        var context = CreateHttpContext("POST");
        context.Request.Headers.Origin = origin;
        context.Request.Headers.Referer = referer;

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns(frontendUrls);
        _messageServiceMock.Setup(e => e.GetMessage("CSRFInvalidOriginOrReferer"))
            .Returns("CSRF validation failed");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        if (shouldAllow)
        {
            _nextMock.Verify(n => n(context), Times.Once);
            context.Response.StatusCode.Should().Be(200);
        }
        else
        {
            context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
            _nextMock.Verify(n => n(context), Times.Never);

            // Verify ApiResponse format
            var responseBody = GetResponseBody(context);
            responseBody.Should().Contain("CSRF validation failed");
            responseBody.Should().Contain("\"IsSuccess\":false");
            responseBody.Should().Contain("\"Error\":");
            responseBody.Should().Contain("correlationId");
            responseBody.Should().Contain("traceId");
        }
    }

    [Fact]
    public async Task InvokeAsync_ShouldUseApiUrl_WhenFrontendUrlsNotProvided()
    {
        // Arrange
        var context = CreateHttpContext("POST");
        context.Request.Headers.Origin = "https://api.example.com";
        context.Request.Headers.Referer = "https://api.example.com/swagger";

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns((string?)null);
        _configurationMock.Setup(c => c["APIUrl"]).Returns("https://api.example.com");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task InvokeAsync_ShouldTreatEmptyFrontendUrlsAsNotProvided(string emptyUrl)
    {
        // Arrange
        var context = CreateHttpContext("POST");
        context.Request.Headers.Origin = "https://api.example.com";
        context.Request.Headers.Referer = "https://api.example.com/swagger";

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns(emptyUrl);
        _configurationMock.Setup(c => c["APIUrl"]).Returns("https://api.example.com");

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    [Theory]
    [InlineData("https://example.com", "https://EXAMPLE.COM", "https://EXAMPLE.COM/page")]
    [InlineData("https://Example.Com", "https://example.com", "https://example.com/page")]
    public async Task InvokeAsync_ShouldBeCaseInsensitive(string allowedUrl, string origin, string referer)
    {
        // Arrange
        var context = CreateHttpContext("POST");
        context.Request.Headers.Origin = origin;
        context.Request.Headers.Referer = referer;

        _configurationMock.Setup(c => c["FrontendUrls"]).Returns(allowedUrl);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
    }

    private DefaultHttpContext CreateHttpContext(string method)
    {
        var context = new DefaultHttpContext();
        var services = new ServiceCollection();

        services.AddSingleton(_messageServiceMock.Object);
        services.AddSingleton(_configurationMock.Object);

        context.RequestServices = services.BuildServiceProvider();
        context.Request.Method = method;
        context.Response.Body = new MemoryStream();

        return context;
    }

    private static string GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return reader.ReadToEnd();
    }
}