namespace BlogApp.UnitTests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private readonly DefaultHttpContext _context;
    private readonly GlobalExceptionMiddleware _middleware;
    private readonly Mock<ILogger<GlobalExceptionMiddleware>> _mockLogger;
    private readonly Mock<RequestDelegate> _mockNext;

    public GlobalExceptionMiddlewareTests()
    {
        _mockNext = new Mock<RequestDelegate>();
        _mockLogger = new Mock<ILogger<GlobalExceptionMiddleware>>();
        _middleware = new GlobalExceptionMiddleware(_mockNext.Object, _mockLogger.Object);
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream();
    }

    [Fact]
    public async Task InvokeAsync_WithNoException_ShouldCallNext()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        _mockNext.Setup(x => x(_context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _mockNext.Verify(x => x(_context), Times.Once);
        _context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WithUnauthorizedAccessException_ShouldReturnUnauthorized()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        mockMessageService.Setup(x => x.GetMessage("UnauthorizedAccess"))
            .Returns("Unauthorized access");

        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        _mockNext.Setup(x => x(_context))
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be(401);
        _context.Response.ContentType.Should().Be("application/json");

        var responseBody = GetResponseBody();
        responseBody.Should().Contain("Unauthorized access");
        responseBody.Should().Contain("UNAUTHORIZED");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("\"Error\":");
        responseBody.Should().Contain("\"Data\":null");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");
    }

    [Fact]
    public async Task InvokeAsync_WithArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        _mockNext.Setup(x => x(_context))
            .ThrowsAsync(new ArgumentException("Invalid argument"));

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be(400);
        _context.Response.ContentType.Should().Be("application/json");

        var responseBody = GetResponseBody();
        responseBody.Should().Contain("Invalid argument");
        responseBody.Should().Contain("INVALID_ARGUMENT");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("\"Error\":");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");
    }

    [Fact]
    public async Task InvokeAsync_WithKeyNotFoundException_ShouldReturnNotFound()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        mockMessageService.Setup(x => x.GetMessage("ResourceNotFound"))
            .Returns("Resource not found");

        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        _mockNext.Setup(x => x(_context))
            .ThrowsAsync(new KeyNotFoundException("Key not found"));

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be(404);
        _context.Response.ContentType.Should().Be("application/json");

        var responseBody = GetResponseBody();
        responseBody.Should().Contain("Resource not found");
        responseBody.Should().Contain("NOT_FOUND");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("\"Error\":");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");
    }

    [Fact]
    public async Task InvokeAsync_WithValidationException_ShouldReturnBadRequestWithValidationDetails()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        mockMessageService.Setup(x => x.GetMessage("ValidationError"))
            .Returns("Validation error");
        mockMessageService.Setup(x => x.GetMessage("Required"))
            .Returns("Field is required");

        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        var validationFailures = new[]
        {
            new ValidationFailure("Name", "Required"),
            new ValidationFailure("Email", "Invalid format")
        };
        var validationException = new ValidationException(validationFailures);

        _mockNext.Setup(x => x(_context))
            .ThrowsAsync(validationException);

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be(400);
        _context.Response.ContentType.Should().Be("application/json");

        var responseBody = GetResponseBody();
        responseBody.Should().Contain("Validation error");
        responseBody.Should().Contain("VALIDATION_ERROR");
        responseBody.Should().Contain("Field is required");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("\"Error\":");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");
    }

    [Fact]
    public async Task InvokeAsync_WithGenericException_ShouldReturnInternalServerError()
    {
        // Arrange
        var mockMessageService = new Mock<IMessageService>();
        mockMessageService.Setup(x => x.GetMessage("UnexpectedErrorOccurred"))
            .Returns("An unexpected error occurred");

        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        _mockNext.Setup(x => x(_context))
            .ThrowsAsync(new Exception("Generic error"));

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be(500);
        _context.Response.ContentType.Should().Be("application/json");

        var responseBody = GetResponseBody();
        responseBody.Should().Contain("An unexpected error occurred");
        responseBody.Should().Contain("INTERNAL_SERVER_ERROR");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("\"Error\":");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");
    }

    [Fact]
    public async Task InvokeAsync_WithGenericExceptionInDevelopment_ShouldIncludeExceptionDetails()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        var mockMessageService = new Mock<IMessageService>();
        mockMessageService.Setup(x => x.GetMessage("UnexpectedErrorOccurred"))
            .Returns("An unexpected error occurred");

        _context.RequestServices = CreateServiceProvider(mockMessageService.Object);

        _mockNext.Setup(x => x(_context))
            .ThrowsAsync(new Exception("Generic error"));

        // Act
        await _middleware.InvokeAsync(_context);

        // Assert
        _context.Response.StatusCode.Should().Be(500);

        var responseBody = GetResponseBody();
        responseBody.Should().Contain("Generic error");
        responseBody.Should().Contain("\"Details\":");
        responseBody.Should().Contain("\"IsSuccess\":false");
        responseBody.Should().Contain("correlationId");
        responseBody.Should().Contain("traceId");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    private static ServiceProvider CreateServiceProvider(IMessageService errorMessageService)
    {
        var services = new ServiceCollection();
        services.AddSingleton(errorMessageService);
        return services.BuildServiceProvider();
    }

    private string GetResponseBody()
    {
        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(_context.Response.Body);
        return reader.ReadToEnd();
    }
}