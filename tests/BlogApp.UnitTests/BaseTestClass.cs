namespace BlogApp.UnitTests;

/// <summary>
///     Base test class with common setup and teardown
/// </summary>
public abstract class BaseTestClass : IDisposable
{
    protected ApplicationDbContext? _context;
    protected bool _disposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Creates a fresh in-memory database context for each test
    /// </summary>
    protected ApplicationDbContext CreateDbContext()
    {
        _context = TestHelper.CreateInMemoryDbContext();
        return _context;
    }

    /// <summary>
    ///     Seeds test data into the database
    /// </summary>
    protected static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        var user = TestHelper.TestData.CreateTestUser();
        var post = TestHelper.TestData.CreateTestPost();
        var todo = TestHelper.TestData.CreateTestTodo();
        var file = TestHelper.TestData.CreateTestFile();

        await context.Users.AddAsync(user);
        await context.Posts.AddAsync(post);
        await context.Todos.AddAsync(todo);
        await context.Files.AddAsync(file);

        await context.SaveChangesAsync();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) _context?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
///     Base class for controller tests with common controller setup
/// </summary>
public abstract class BaseControllerTest : BaseTestClass
{
    protected readonly Mock<ICurrentUserService> _mockCurrentUserService = TestHelper.MockSetups.CreateMockCurrentUserService();
    protected readonly Mock<IMediator> _mockMediator = new();
    protected readonly Mock<IMessageService> _mockMessageService = TestHelper.MockSetups.CreateMockMessageService();
    protected readonly Mock<IRabbitMqPublisher> _mockRabbitMqPublisher = TestHelper.MockSetups.CreateMockRabbitMqPublisher();

    /// <summary>
    ///     Sets up controller context with authenticated user
    /// </summary>
    protected static void SetupAuthenticatedUser(ControllerBase controller, string userId = "test-user-id")
    {
        controller.ControllerContext = TestHelper.CreateAuthenticatedControllerContext(userId);
    }
}

/// <summary>
///     Base class for application layer tests (command/query handlers)
/// </summary>
public abstract class BaseApplicationTest : BaseTestClass
{
    protected readonly Mock<ICacheInvalidationService> _mockCacheInvalidationService = new();
    protected readonly Mock<ILogger> _mockLogger = new();
    protected readonly Mock<IMessageService> _mockMessageService = TestHelper.MockSetups.CreateMockMessageService();
    protected readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
}

/// <summary>
///     Base class for service tests
/// </summary>
public abstract class BaseServiceTest : BaseTestClass
{
    protected readonly IConfiguration _configuration = TestHelper.CreateMockConfiguration();
    protected readonly Mock<ILogger> _mockLogger = new();
}