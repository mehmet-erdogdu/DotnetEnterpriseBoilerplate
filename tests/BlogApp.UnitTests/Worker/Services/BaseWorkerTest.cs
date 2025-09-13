namespace BlogApp.UnitTests.Worker.Services;

public abstract class BaseWorkerTest
{
    protected readonly Mock<ILogger> MockLogger;

    protected BaseWorkerTest()
    {
        MockLogger = new Mock<ILogger>();
    }
}