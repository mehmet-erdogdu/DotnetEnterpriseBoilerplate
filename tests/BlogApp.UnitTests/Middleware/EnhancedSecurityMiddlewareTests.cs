using System.Net;

namespace BlogApp.UnitTests.Middleware;

public class EnhancedSecurityMiddlewareTests : BaseTestClass
{
    private readonly DefaultHttpContext _httpContext;
    private readonly EnhancedSecurityMiddleware _middleware;
    private readonly Mock<ILogger<EnhancedSecurityMiddleware>> _mockLogger;
    private readonly Mock<IMessageService> _mockMessageService;
    private readonly Mock<RequestDelegate> _mockNext;
    private readonly ServiceCollection _services;

    public EnhancedSecurityMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<EnhancedSecurityMiddleware>>();
        _mockMessageService = new Mock<IMessageService>();
        _middleware = new EnhancedSecurityMiddleware(_mockNext.Object, _mockLogger.Object);
        _httpContext = new DefaultHttpContext();
        _services = new ServiceCollection();

        // Setup service provider
        _services.AddSingleton(_mockMessageService.Object);
        _httpContext.RequestServices = _services.BuildServiceProvider();
    }

    [Fact]
    public async Task InvokeAsync_WithNormalUserAgent_ShouldCallNext()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items.Should().ContainKey("ClientIP");
        _httpContext.Items.Should().ContainKey("UserAgent");
    }

    [Fact]
    public async Task InvokeAsync_WithSuspiciousUserAgent_ShouldBlockRequest()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "python-requests/2.28.1";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("AccessDenied"))
            .Returns("Access denied");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _httpContext.Response.ContentType.Should().Be("application/json");

        _httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("SUSPICIOUS_USER_AGENT");
        var details = (JsonElement)apiResponse.Error.Details!;
        details.TryGetProperty("userAgent", out _).Should().BeTrue();
    }

    [Theory]
    [InlineData("bot")]
    [InlineData("crawler")]
    [InlineData("spider")]
    [InlineData("scraper")]
    [InlineData("curl")]
    [InlineData("wget")]
    [InlineData("python")]
    [InlineData("java")]
    [InlineData("perl")]
    [InlineData("ruby")]
    [InlineData("php")]
    [InlineData("go-http-client")]
    [InlineData("okhttp")]
    [InlineData("apache-httpclient")]
    public async Task InvokeAsync_WithVariousSuspiciousUserAgents_ShouldBlockRequest(string suspiciousPattern)
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = $"MyApp/{suspiciousPattern}/1.0";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("AccessDenied"))
            .Returns("Access denied");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyUserAgent_ShouldBlockRequest()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("AccessDenied"))
            .Returns("Access denied");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WithNullUserAgent_ShouldBlockRequest()
    {
        // Arrange
        _httpContext.Request.Headers.Remove("User-Agent");
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("AccessDenied"))
            .Returns("Access denied");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Theory]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("192.168.1.1")]
    [InlineData("127.0.0.1")]
    public async Task InvokeAsync_WithSuspiciousIpAddress_ShouldBlockRequest(string suspiciousIp)
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse(suspiciousIp);
        _httpContext.Response.Body = new MemoryStream();

        _mockMessageService.Setup(x => x.GetMessage("AccessDenied"))
            .Returns("Access denied");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Never);
        _httpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        _httpContext.Response.ContentType.Should().Be("application/json");

        _httpContext.Response.Body.Position = 0;
        var responseBody = await new StreamReader(_httpContext.Response.Body).ReadToEndAsync();
        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(responseBody);

        apiResponse.Should().NotBeNull();
        apiResponse!.IsSuccess.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("SUSPICIOUS_IP_ADDRESS");
        var details = (JsonElement)apiResponse.Error.Details!;
        details.TryGetProperty("ipAddress", out _).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithXForwardedForHeader_ShouldUseForwardedIp()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        _httpContext.Request.Headers["X-Forwarded-For"] = "203.0.113.1, 70.41.3.18, 150.172.238.178";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items["ClientIP"].Should().Be("203.0.113.1");
    }

    [Fact]
    public async Task InvokeAsync_WithXRealIpHeader_ShouldUseRealIp()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        _httpContext.Request.Headers["X-Real-IP"] = "203.0.113.1";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.1");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items["ClientIP"].Should().Be("203.0.113.1");
    }

    [Fact]
    public async Task InvokeAsync_WithNullRemoteIp_ShouldUseUnknown()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        _httpContext.Connection.RemoteIpAddress = null;

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);
        _httpContext.Items["ClientIP"].Should().Be("Unknown");
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetSecurityContext()
    {
        // Arrange
        _httpContext.Request.Headers.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
        _httpContext.Connection.RemoteIpAddress = IPAddress.Parse("8.8.8.8");

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _httpContext.Items.Should().ContainKey("ClientIP");
        _httpContext.Items.Should().ContainKey("UserAgent");
        _httpContext.Items["ClientIP"].Should().Be("8.8.8.8");
        _httpContext.Items["UserAgent"].Should().Be("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
    }
}