using System.Reflection;

namespace BlogApp.UnitTests.Middleware;

public class RequestLoggingMiddlewareTests
{
    private static readonly string[] SensitiveFields = { "password" };

    private readonly Mock<ILogger<RequestLoggingMiddleware>> _loggerMock;
    private readonly RequestLoggingMiddleware _middleware;
    private readonly Mock<RequestDelegate> _nextMock;

    public RequestLoggingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestLoggingMiddleware>>();
        _nextMock = new Mock<RequestDelegate>();
        _middleware = new RequestLoggingMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("/api/posts", "{\"title\":\"Test\",\"password\":\"secret123\",\"content\":\"Hello\"}",
        "{\"title\":\"Test\",\"password\": \"***\",\"content\":\"Hello\"}")]
    [InlineData("/api/users", "{\"username\":\"test\",\"token\":\"abc123\",\"email\":\"test@test.com\"}",
        "{\"username\":\"test\",\"token\": \"***\",\"email\":\"test@test.com\"}")]
    [InlineData("/api/files", "{\"filename\":\"test.txt\",\"secret\":\"mysecret\",\"size\":1024}",
        "{\"filename\":\"test.txt\",\"secret\": \"***\",\"size\":1024}")]
    [InlineData("/api/auth/login", "{\"username\":\"test\",\"password\":\"secret\"}",
        "***REDACTED***")] // Auth endpoints should be completely redacted
    public void SanitizeRequestBody_ShouldRedactSensitiveFields(string path, string input, string expected)
    {
        // Arrange
        var context = CreateHttpContext(path, "POST", input);

        // Act & Assert - Using reflection to test the private method
        var method = typeof(RequestLoggingMiddleware).GetMethod("SanitizeRequestBody",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method?.Invoke(null, new object[] { input, new PathString(path) });

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("/api/posts", "{\"id\":123,\"token\":\"jwt123\",\"title\":\"Test\"}",
        "{\"id\":123,\"token\": \"***\",\"title\":\"Test\"}")]
    [InlineData("/api/users", "{\"name\":\"John\",\"key\":\"apikey123\",\"role\":\"admin\"}",
        "{\"name\":\"John\",\"key\": \"***\",\"role\":\"admin\"}")]
    [InlineData("/api/auth/refresh", "{\"refreshToken\":\"refresh123\",\"userId\":1}",
        "***REDACTED***")] // Auth endpoints should be completely redacted
    public void SanitizeResponseBody_ShouldRedactSensitiveFields(string path, string input, string expected)
    {
        // Arrange
        var context = CreateHttpContext(path, "GET");

        // Act & Assert - Using reflection to test the private method
        var method = typeof(RequestLoggingMiddleware).GetMethod("SanitizeResponseBody",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method?.Invoke(null, new object[] { input, new PathString(path) });

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("password=secret&username=test", "password=***&username=test")]
    [InlineData("token=abc123&id=1&key=mykey", "token=***&id=1&key=***")]
    [InlineData("normal=value&other=data", "normal=value&other=data")]
    [InlineData("", "")]
    public void SanitizeQueryString_ShouldRedactSensitiveParameters(string input, string expected)
    {
        // Act & Assert - Using reflection to test the private method
        var method = typeof(RequestLoggingMiddleware).GetMethod("SanitizeQueryString",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method?.Invoke(null, new object[] { input });

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("/api/auth/login", false)]
    [InlineData("/api/auth/register", false)]
    [InlineData("/api/posts", true)]
    [InlineData("/api/users", true)]
    public void ShouldLogRequestBody_ShouldReturnCorrectValue(string path, bool expected)
    {
        // Arrange
        var request = new Mock<HttpRequest>();
        request.Setup(r => r.Path).Returns(new PathString(path));

        // Act & Assert - Using reflection to test the private method
        var method = typeof(RequestLoggingMiddleware).GetMethod("ShouldLogRequestBody",
            BindingFlags.NonPublic | BindingFlags.Static);

        var result = method?.Invoke(null, new object[] { request.Object });

        result.Should().Be(expected);
    }

    [Fact]
    public void SanitizeJsonBody_ShouldHandleNullAndEmptyInputs()
    {
        // Act & Assert - Using reflection to test the private method
        var method = typeof(RequestLoggingMiddleware).GetMethod("SanitizeJsonBody",
            BindingFlags.NonPublic | BindingFlags.Static);

        var nullResult = method?.Invoke(null, new object[] { null!, new PathString("/api/test"), SensitiveFields });
        var emptyResult = method?.Invoke(null, new object[] { "", new PathString("/api/test"), SensitiveFields });

        nullResult.Should().BeNull();
        emptyResult.Should().Be("");
    }

    [Fact]
    public async Task InvokeAsync_ShouldLogRequest_WhenProcessingRequest()
    {
        // Arrange
        var context = CreateHttpContext("/api/test", "GET");

        _nextMock.Setup(n => n(It.IsAny<HttpContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _nextMock.Verify(n => n(context), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("HTTP GET /api/test")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private static DefaultHttpContext CreateHttpContext(string path, string method, string? body = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Request.Method = method;

        if (!string.IsNullOrEmpty(body))
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            context.Request.Body = new MemoryStream(bytes);
            context.Request.ContentLength = bytes.Length;
        }

        context.Response.Body = new MemoryStream();

        return context;
    }
}