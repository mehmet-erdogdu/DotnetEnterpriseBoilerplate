namespace BlogApp.UnitTests.Middleware;

public class CorrelationIdMiddlewareTests : BaseTestClass
{
    private new readonly HttpContext _context;
    private readonly CorrelationIdMiddleware _middleware;
    private readonly Mock<RequestDelegate> _mockNext;

    public CorrelationIdMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _middleware = new CorrelationIdMiddleware(_mockNext.Object);
        _context = new DefaultHttpContext();
    }

    [Fact]
    public async Task InvokeAsync_WithoutCorrelationId_ShouldGenerateNewOne()
    {
        // Arrange
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        _context.Response.Headers["X-Correlation-ID"].ToString().Should().NotBeNullOrEmpty();
        _mockNext.Verify(x => x(_context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WithExistingCorrelationId_ShouldUseExistingOne()
    {
        // Arrange
        var existingCorrelationId = Guid.NewGuid().ToString();
        _context.Request.Headers["X-Correlation-ID"] = existingCorrelationId;
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.Headers["X-Correlation-ID"].ToString().Should().Be(existingCorrelationId);
        _mockNext.Verify(x => x(_context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _mockNext.Verify(x => x(_context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenNextThrowsException_ShouldStillSetCorrelationId()
    {
        // Arrange
        var exception = new Exception("Test exception");
        _mockNext.Setup(x => x(_context)).ThrowsAsync(exception);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _middleware.InvokeAsync(_context));

        _context.Response.Headers.Should().ContainKey("X-Correlation-ID");
        _context.Response.Headers["X-Correlation-ID"].ToString().Should().NotBeNullOrEmpty();
    }
}