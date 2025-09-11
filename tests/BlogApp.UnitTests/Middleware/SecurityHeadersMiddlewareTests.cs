namespace BlogApp.UnitTests.Middleware;

public class SecurityHeadersMiddlewareTests
{
    private readonly DefaultHttpContext _context;
    private readonly SecurityHeadersMiddleware _middleware;
    private readonly Mock<RequestDelegate> _mockNext;

    public SecurityHeadersMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _middleware = new SecurityHeadersMiddleware(_mockNext.Object);
        _context = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddSecurityHeaders()
    {
        // Arrange
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers.Should().ContainKey("X-Content-Type-Options");
        _context.Response.Headers.XContentTypeOptions.ToString().Should().Be("nosniff");

        _context.Response.Headers.Should().ContainKey("X-Frame-Options");
        _context.Response.Headers.XFrameOptions.ToString().Should().Be("DENY");

        _context.Response.Headers.Should().ContainKey("X-XSS-Protection");
        _context.Response.Headers.XXSSProtection.ToString().Should().Be("1; mode=block");

        _context.Response.Headers.Should().ContainKey("Referrer-Policy");
        _context.Response.Headers["Referrer-Policy"].ToString().Should().Be("strict-origin-when-cross-origin");

        _context.Response.Headers.Should().ContainKey("Permissions-Policy");
        _context.Response.Headers["Permissions-Policy"].ToString().Should().Be("geolocation=(), microphone=(), camera=(), payment=(), usb=()");

        _mockNext.Verify(x => x(_context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_InDevelopment_ShouldUseDevelopmentCSP()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = _context.Response.Headers.ContentSecurityPolicy.ToString();
        csp.Should().Contain("'unsafe-inline'");
        csp.Should().Contain("'unsafe-eval'");
        csp.Should().Contain("ws:");
        csp.Should().Contain("wss:");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task InvokeAsync_InProduction_ShouldUseProductionCSP()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers.Should().ContainKey("Content-Security-Policy");
        var csp = _context.Response.Headers.ContentSecurityPolicy.ToString();
        csp.Should().NotContain("'unsafe-inline'");
        csp.Should().NotContain("'unsafe-eval'");
        csp.Should().Contain("upgrade-insecure-requests");
        csp.Should().Contain("require-trusted-types-for 'script'");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public async Task InvokeAsync_ShouldAddHstsHeader()
    {
        // Arrange
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers.Should().ContainKey("Strict-Transport-Security");
        _context.Response.Headers.StrictTransportSecurity.ToString().Should().Be("max-age=31536000; includeSubDomains; preload");
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNext()
    {
        // Arrange
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _mockNext.Verify(x => x(_context), Times.Once);
    }
}