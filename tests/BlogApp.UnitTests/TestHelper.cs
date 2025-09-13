using System.Linq.Expressions;
using Microsoft.Extensions.Options;

namespace BlogApp.UnitTests;

public static class TestHelper
{
    public static Dictionary<string, string?> GetMockSecrets()
    {
        return new Dictionary<string, string?>
        {
            ["AllowedHosts"] = "*",
            ["DBConnection"] = "Host=localhost;Port=5432;Database=blogapp_test;Username=test;Password=test123;SslMode=Disable;",
            ["RedisConnection"] = "localhost:6379,password=redis123",
            ["ElasticUrl"] = "http://localhost:9200",
            ["APIUrl"] = "https://localhost:7266",
            ["KnownProxies"] = "127.0.0.1",
            ["Firebase:ProjectId"] = "test-project-id",
            ["Firebase:ServiceAccountKeyPath"] = "test-service-account-key.json",
            ["RabbitMQ:HostName"] = "localhost",
            ["RabbitMQ:Port"] = "5672",
            ["RabbitMQ:UserName"] = "guest",
            ["RabbitMQ:Password"] = "guest",
            ["RabbitMQ:VirtualHost"] = "/",
            ["S3:Url"] = "http://localhost:9200",
            ["S3:AccessKeyId"] = "minioadmin",
            ["S3:SecretAccessKey"] = "minioadmin123",
            ["JWT:RefreshTokenExpirationDays"] = "7",
            ["JWT:Secret"] = new('x', 64),
            ["JWT:TokenExpirationMinutes"] = "30",
            ["JWT:ValidAudience"] = "test-aud",
            ["JWT:ValidIssuer"] = "test-iss",
            ["RateLimiting:AuthLimit"] = "10",
            ["RateLimiting:AuthWindowMinutes"] = "1",
            ["RateLimiting:GlobalLimit"] = "100",
            ["RateLimiting:QueueLimit"] = "0",
            ["RateLimiting:WindowMinutes"] = "1",
            ["Security:LockoutDurationMinutes"] = "5",
            ["Security:MaxLoginAttempts"] = "5",
            ["Security:PasswordHistoryCount"] = "5",
            ["Security:RequireHttps"] = "true",
            ["Serilog:GlobalLogLevel"] = "Information",
            ["Serilog:MinimumLevel:Default"] = "Information",
            ["Serilog:MinimumLevel:Microsoft"] = "Information",
            ["Serilog:MinimumLevel:Microsoft.AspNetCore"] = "Information",
            ["Serilog:MinimumLevel:Microsoft.EntityFrameworkCore"] = "Information"
        };
    }

    public static IConfiguration CreateMockConfiguration()
    {
        var secrets = GetMockSecrets();
        return new ConfigurationBuilder()
            .AddInMemoryCollection(secrets!)
            .Build();
    }

    /// <summary>
    ///     Creates a mock ApplicationDbContext for testing
    /// </summary>
    public static ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(x => x.UserId).Returns("test-user-id");

        var auditableInterceptor = new AuditableEntitySaveChangesInterceptor(currentUser.Object);
        var auditLoggingInterceptor = new AuditLoggingInterceptor(currentUser.Object);

        return new ApplicationDbContext(options, auditableInterceptor, auditLoggingInterceptor);
    }

    /// <summary>
    ///     Creates a controller context with authenticated user
    /// </summary>
    public static ControllerContext CreateAuthenticatedControllerContext(string userId = "test-user-id", string email = "test@example.com")
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, "Test User")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };

        return new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    /// <summary>
    ///     Creates test data for entities
    /// </summary>
    public static class TestData
    {
        public static ApplicationUser CreateTestUser(string? id = null, string? email = null)
        {
            return new ApplicationUser
            {
                Id = id ?? "test-user-id",
                Email = email ?? "test@example.com",
                UserName = email ?? "test@example.com",
                FirstName = "Test",
                LastName = "Test",
                EmailConfirmed = true
            };
        }

        public static Post CreateTestPost(string? authorId = null, Guid? id = null)
        {
            return new Post
            {
                Id = id ?? Guid.NewGuid(),
                Title = "Test Post Title",
                Content = "This is test post content",
                AuthorId = authorId ?? "test-user-id",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Todo CreateTestTodo(string? userId = null, Guid? id = null)
        {
            return new Todo
            {
                Id = id ?? Guid.NewGuid(),
                Title = "Test Todo",
                Description = "Test todo description",
                UserId = userId ?? "test-user-id",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static FileEntity CreateTestFile(string? uploadedById = null, Guid? id = null)
        {
            return new FileEntity
            {
                Id = id ?? Guid.NewGuid(),
                FileName = "test-file.jpg",
                OriginalFileName = "original-test-file.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024,
                FilePath = "uploads/2024/01/01/test-file.jpg",
                Description = "Test file description",
                UploadedById = uploadedById ?? "test-user-id",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static PostDto CreateTestPostDto(string? authorId = null, Guid? id = null)
        {
            return new PostDto
            {
                Id = id ?? Guid.NewGuid(),
                Title = "Test Post Title",
                Content = "This is test post content",
                AuthorId = authorId ?? "test-user-id",
                AuthorName = "Test User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static TodoDto CreateTestTodoDto(string? userId = null, Guid? id = null)
        {
            return new TodoDto
            {
                Id = id ?? Guid.NewGuid(),
                Title = "Test Todo",
                Description = "Test todo description",
                UserId = userId ?? "test-user-id",
                UserName = "Test User",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static FileDto CreateTestFileDto(string? uploadedById = null, Guid? id = null)
        {
            return new FileDto
            {
                Id = id ?? Guid.NewGuid(),
                FileName = "test-file.jpg",
                OriginalFileName = "original-test-file.jpg",
                ContentType = "image/jpeg",
                FileSize = 1024,
                FilePath = "uploads/2024/01/01/test-file.jpg",
                Description = "Test file description",
                UploadedById = uploadedById ?? "test-user-id",
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    ///     Common mock setups
    /// </summary>
    public static class MockSetups
    {
        public static Mock<ILogger<T>> CreateMockLogger<T>() where T : class
        {
            return new Mock<ILogger<T>>();
        }

        public static Mock<ICurrentUserService> CreateMockCurrentUserService(string? userId = null)
        {
            var mock = new Mock<ICurrentUserService>();
            mock.Setup(x => x.UserId).Returns(userId ?? "test-user-id");
            return mock;
        }

        public static Mock<IMessageService> CreateMockMessageService()
        {
            var mock = new Mock<IMessageService>();
            mock.Setup(x => x.GetMessage(It.IsAny<string>(), It.IsAny<object[]>()))
                .Returns((string key, object[] args) => $"Error: {key}");
            return mock;
        }

        public static Mock<ICacheService> CreateMockCacheService()
        {
            var mock = new Mock<ICacheService>();
            mock.Setup(x => x.GetAsync<string>(It.IsAny<string>()))
                .ReturnsAsync((string?)null);
            mock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);
            mock.Setup(x => x.RemoveAsync(It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            return mock;
        }

        public static Mock<IRabbitMqPublisher> CreateMockRabbitMqPublisher()
        {
            var mock = new Mock<IRabbitMqPublisher>();
            mock.Setup(x => x.PublishAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return mock;
        }

        public static Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();

            var mock = new Mock<UserManager<ApplicationUser>>(
                store.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors.Object,
                serviceProvider.Object,
                logger.Object);

            return mock;
        }

        public static Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            // Create a simple mock by using the base constructor with minimal parameters
            var store = new Mock<IRoleStore<IdentityRole>>();
            store.Setup(x => x.Dispose()).Callback(() => { });

            var roleValidators = new List<IRoleValidator<IdentityRole>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new IdentityErrorDescriber();
            var logger = new Mock<ILogger<RoleManager<IdentityRole>>>();

            // Create the RoleManager with the correct parameters that match .NET 9 constructor
            // Using the constructor that takes all required parameters
            var roleManager = new RoleManager<IdentityRole>(
                store.Object,
                roleValidators,
                keyNormalizer.Object,
                errors,
                logger.Object);

            // Create a mock of the concrete RoleManager instance
            var mock = new Mock<RoleManager<IdentityRole>>(MockBehavior.Default,
                store.Object,
                roleValidators,
                keyNormalizer.Object,
                errors,
                logger.Object);

            return mock;
        }
    }

    /// <summary>
    ///     Assertion helpers
    /// </summary>
    public static class AssertHelpers
    {
        public static void AssertApiResponseSuccess<T>(ApiResponse<T> response)
        {
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeTrue();
            response.Error.Should().BeNull();
            response.Data.Should().NotBeNull();
        }

        public static void AssertApiResponseFailure<T>(ApiResponse<T> response, string? expectedError = null)
        {
            response.Should().NotBeNull();
            response.IsSuccess.Should().BeFalse();
            response.Error.Should().NotBeNull();
            response.Data.Should().Be(default(T));

            if (!string.IsNullOrEmpty(expectedError)) response.Error!.Message.Should().Contain(expectedError);
        }

        public static void AssertOkResult<T>(ActionResult<T> result)
        {
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<OkObjectResult>();
        }

        public static void AssertCreatedResult<T>(ActionResult<T> result)
        {
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<CreatedAtActionResult>();
        }

        public static void AssertNotFoundResult<T>(ActionResult<T> result)
        {
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<NotFoundObjectResult>();
        }

        public static void AssertBadRequestResult<T>(ActionResult<T> result)
        {
            result.Should().NotBeNull();
            result.Result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}

/// <summary>
///     Async enumerable implementation for testing
/// </summary>
public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    {
    }

    public TestAsyncEnumerable(Expression expression) : base(expression)
    {
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

/// <summary>
///     Async enumerator implementation for testing
/// </summary>
public class TestAsyncEnumerator<T>(IEnumerator<T> inner) : IAsyncEnumerator<T>
{
    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(inner.MoveNext());
    }

    public T Current => inner.Current;

    public ValueTask DisposeAsync()
    {
        inner.Dispose();
        GC.SuppressFinalize(this);
        return default;
    }
}

/// <summary>
///     Async query provider implementation for testing
/// </summary>
public class TestAsyncQueryProvider<T>(IQueryProvider inner) : IQueryProvider
{
    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<T>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return inner.Execute<TResult>(expression);
    }
}