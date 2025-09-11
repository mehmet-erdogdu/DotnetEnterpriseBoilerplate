namespace BlogApp.UnitTests.Infrastructure;

/// <summary>
/// Base class for infrastructure layer tests
/// </summary>
public abstract class BaseInfrastructureTest : BaseTestClass
{
    protected readonly Mock<ILogger> _mockLogger = new();
    protected readonly Mock<IDistributedCache> _mockDistributedCache = new();
    protected readonly Mock<IConfiguration> _mockConfiguration = new();
}