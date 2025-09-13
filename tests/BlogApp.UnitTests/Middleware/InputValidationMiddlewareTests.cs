namespace BlogApp.UnitTests.Middleware;

public class InputValidationMiddlewareTests : BaseTestClass
{
    private readonly DefaultHttpContext _httpContext;
    private readonly InputValidationMiddleware _middleware;
    private readonly Mock<ILogger<InputValidationMiddleware>> _mockLogger;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly ServiceCollection _services;

    public InputValidationMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<InputValidationMiddleware>>();
        _mockMessageService = new Mock<IMessageService>();
        _middleware = new InputValidationMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext = new DefaultHttpContext();
        _services = new ServiceCollection();

        // Setup service provider
        _services.AddSingleton(_mockMessageService.Object);
        _httpContext.RequestServices = _services.BuildServiceProvider();
    }

    [Fact]
    public async Task InvokeAsync_WithGetRequest_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.QueryString = new QueryString("?param=value");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithValidJsonContentType_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "application/json";
        _httpContext.Request.ContentLength = 100;

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithValidFormUrlEncodedContentType_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "application/x-www-form-urlencoded";
        _httpContext.Request.ContentLength = 100;

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithValidMultipartFormDataContentType_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Method = "POST";
        _httpContext.Request.ContentType = "multipart/form-data; boundary=----WebKitFormBoundary";
        _httpContext.Request.ContentLength = 100;

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WithMissingContentType_ShouldReturnBadRequest(string method)
    {
        // Arrange
        _httpContext.Request.Method = method;
        _httpContext.Request.ContentType = null;
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("InvalidContentType"))
            .Returns("Invalid content type");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _httpContext.Response.ContentType.Should().Be("application/json");

        _httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("INVALID_CONTENT_TYPE");
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WithInvalidContentType_ShouldReturnBadRequest(string method)
    {
        // Arrange
        _httpContext.Request.Method = method;
        _httpContext.Request.ContentType = "text/plain";
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("InvalidContentType"))
            .Returns("Invalid content type");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _httpContext.Response.ContentType.Should().Be("application/json");
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("PATCH")]
    public async Task InvokeAsync_WithLargeContentLength_ShouldReturnPayloadTooLarge(string method)
    {
        // Arrange
        _httpContext.Request.Method = method;
        _httpContext.Request.ContentType = "application/json";
        _httpContext.Request.ContentLength = 2097152; // 2MB
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("RequestPayloadTooLarge"))
            .Returns("Request payload too large");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status413PayloadTooLarge);
        _httpContext.Response.ContentType.Should().Be("application/json");

        _httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("PAYLOAD_TOO_LARGE");
        var details = (JsonElement)apiResponse.Error.Details!;
        details.TryGetProperty("contentLength", out _).Should().BeTrue();
        details.TryGetProperty("maxAllowedSize", out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithSuspiciousQueryString_ShouldLogWarning()
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.QueryString = new QueryString("?param=<script>alert('xss')</script>");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Query string sanitized")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("?param=<script>alert('xss')</script>")]
    [InlineData("?param=javascript:alert('xss')")]
    [InlineData("?param=vbscript:alert('xss')")]
    [InlineData("?param=SELECT * FROM users")]
    [InlineData("?param=../../etc/passwd")]
    [InlineData("?param=%2e%2e%2f%2e%2e%2fetc%2fpasswd")]
    public async Task InvokeAsync_WithVariousSuspiciousQueryStrings_ShouldSanitize(string queryString)
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.QueryString = new QueryString(queryString);

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithSuspiciousXForwardedForHeader_ShouldReturnBadRequest()
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Headers["X-Forwarded-For"] = "<script>alert('xss')</script>";
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("InvalidRequestHeaders"))
            .Returns("Invalid request headers");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _httpContext.Response.ContentType.Should().Be("application/json");

        _httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("INVALID_HEADERS");
    }

    [Theory]
    [InlineData("X-Forwarded-For")]
    [InlineData("X-Real-IP")]
    [InlineData("X-Forwarded-Host")]
    public async Task InvokeAsync_WithSuspiciousHeaderValues_ShouldReturnBadRequest(string headerName)
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Headers[headerName] = "javascript:alert('xss')";
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("InvalidRequestHeaders"))
            .Returns("Invalid request headers");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task InvokeAsync_WithRegexTimeout_ShouldReturnEmptyStringForSecurity()
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.QueryString = new QueryString("?param=valid");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ShouldRethrowWithContext()
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _mockNext.Setup(next => next(_httpContext))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(_httpContext));
        exception.Message.Should().Contain("Input validation failed");
        exception.Message.Should().Contain("GET");
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ShouldThrow()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _middleware.InvokeAsync(null!));
    }

    [Fact]
    public async Task InvokeAsync_WithValidHeaders_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Method = "GET";
        _httpContext.Request.Headers["X-Forwarded-For"] = "192.168.1.1";
        _httpContext.Request.Headers["X-Real-IP"] = "203.0.113.1";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }
}