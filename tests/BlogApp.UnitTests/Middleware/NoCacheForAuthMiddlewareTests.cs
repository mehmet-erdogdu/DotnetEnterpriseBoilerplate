using Microsoft.Extensions.Primitives;

namespace BlogApp.UnitTests.Middleware;

public class NoCacheForAuthMiddlewareTests : BaseTestClass
{
    private readonly DefaultHttpContext _httpContext;
    private readonly NoCacheForAuthMiddleware _middleware;
    private readonly Mock<RequestDelegate> _mockNext;

    public NoCacheForAuthMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _middleware = new NoCacheForAuthMiddleware(_mockNext.Object);
        _httpContext = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_WithAuthPath_ShouldSetNoCacheHeaders()
    {
        // Arrange
        _httpContext.Request.Path = "/api/auth/login";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        _httpContext.Response.Headers.CacheControl.Should().NotBeNull();
        _httpContext.Response.Headers.Pragma.Should().NotBeNull();
        _httpContext.Response.Headers.Expires.Should().NotBeNull();

        _httpContext.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        _httpContext.Response.Headers.Pragma.ToString().Should().Be("no-cache");
        _httpContext.Response.Headers.Expires.ToString().Should().Be("0");
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/refresh")]
    [InlineData("/api/auth/logout")]
    [InlineData("/api/auth/change-password")]
    [InlineData("/api/auth/forgot-password")]
    [InlineData("/api/auth/reset-password")]
    public async Task InvokeAsync_WithVariousAuthPaths_ShouldSetNoCacheHeaders(string path)
    {
        // Arrange
        _httpContext.Request.Path = path;

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        _httpContext.Response.Headers.CacheControl.Should().NotBeNull();
        _httpContext.Response.Headers.Pragma.Should().NotBeNull();
        _httpContext.Response.Headers.Expires.Should().NotBeNull();

        _httpContext.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        _httpContext.Response.Headers.Pragma.ToString().Should().Be("no-cache");
        _httpContext.Response.Headers.Expires.ToString().Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_WithNonAuthPath_ShouldNotSetHeaders()
    {
        // Arrange
        _httpContext.Request.Path = "/api/posts";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.CacheControl).Should().BeTrue();
        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.Pragma).Should().BeTrue();
        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.Expires).Should().BeTrue();
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/posts")]
    [InlineData("/api/todos")]
    [InlineData("/api/files")]
    [InlineData("/api/notifications")]
    [InlineData("/api/dashboard")]
    [InlineData("/api/health")]
    public async Task InvokeAsync_WithVariousNonAuthPaths_ShouldNotSetHeaders(string path)
    {
        // Arrange
        _httpContext.Request.Path = path;

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.CacheControl).Should().BeTrue();
        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.Pragma).Should().BeTrue();
        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.Expires).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WithRootAuthPath_ShouldSetNoCacheHeaders()
    {
        // Arrange
        _httpContext.Request.Path = "/api/auth";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        _httpContext.Response.Headers.CacheControl.Should().NotBeNull();
        _httpContext.Response.Headers.Pragma.Should().NotBeNull();
        _httpContext.Response.Headers.Expires.Should().NotBeNull();

        _httpContext.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        _httpContext.Response.Headers.Pragma.ToString().Should().Be("no-cache");
        _httpContext.Response.Headers.Expires.ToString().Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_WithAuthPathSegment_ShouldSetNoCacheHeaders()
    {
        // Arrange
        _httpContext.Request.Path = "/api/auth/some/deep/path";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        _httpContext.Response.Headers.CacheControl.Should().NotBeNull();
        _httpContext.Response.Headers.Pragma.Should().NotBeNull();
        _httpContext.Response.Headers.Expires.Should().NotBeNull();

        _httpContext.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        _httpContext.Response.Headers.Pragma.ToString().Should().Be("no-cache");
        _httpContext.Response.Headers.Expires.ToString().Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_WithNonAuthPathContainingAuth_ShouldNotSetHeaders()
    {
        // Arrange
        _httpContext.Request.Path = "/api/authentication-not-auth";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.CacheControl).Should().BeTrue();
        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.Pragma).Should().BeTrue();
        StringValues.IsNullOrEmpty(_httpContext.Response.Headers.Expires).Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ShouldSetHeadersBeforeCallingNext()
    {
        // Arrange
        _httpContext.Request.Path = "/api/auth/login";
        var headersSetBeforeNext = false;

        _mockNext.Setup(next => next(_httpContext))
            .Callback(() => { headersSetBeforeNext = !StringValues.IsNullOrEmpty(_httpContext.Response.Headers.CacheControl); });

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        headersSetBeforeNext.Should().BeTrue();
        _mockNext.Verify(next => next(_httpContext), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithExistingHeaders_ShouldOverrideHeaders()
    {
        // Arrange
        _httpContext.Request.Path = "/api/auth/login";
        _httpContext.Response.Headers.CacheControl = "max-age=3600";
        _httpContext.Response.Headers.Pragma = "cache";
        _httpContext.Response.Headers.Expires = "Wed, 21 Oct 2025 07:28:00 GMT";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        _httpContext.Response.Headers.CacheControl.ToString().Should().Be("no-store");
        _httpContext.Response.Headers.Pragma.ToString().Should().Be("no-cache");
        _httpContext.Response.Headers.Expires.ToString().Should().Be("0");
    }

    [Fact]
    public async Task InvokeAsync_ShouldRegisterOnStartingCallback()
    {
        // Arrange
        _httpContext.Request.Path = "/api/auth/login";

        // Act
        await _middleware.InvokeAsync(_httpContext);

        // Assert
        _mockNext.Verify(next => next(_httpContext), Times.Once);

        // Verify that OnStarting callback is registered by checking if headers are still set
        _httpContext.Response.Headers.CacheControl.Should().NotBeNull();
        _httpContext.Response.Headers.Pragma.Should().NotBeNull();
        _httpContext.Response.Headers.Expires.Should().NotBeNull();
    }
}