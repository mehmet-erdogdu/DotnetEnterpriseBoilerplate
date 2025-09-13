using BlogApp.Worker.Services;
using RabbitMQ.Client;

namespace BlogApp.UnitTests.Worker.Services;

public class RabbitMqDemoConsumerServiceTests
{
    private readonly Mock<IModel> _mockChannel;
    private readonly Mock<IConnection> _mockConnection;
    private readonly Mock<IRabbitMqConnectionProvider> _mockConnectionProvider;
    private readonly Mock<ILogger<RabbitMqDemoConsumerService>> _mockLogger;
    private readonly RabbitMqDemoConsumerService _rabbitMqDemoConsumerService;

    public RabbitMqDemoConsumerServiceTests()
    {
        _mockConnectionProvider = new Mock<IRabbitMqConnectionProvider>();
        _mockLogger = new Mock<ILogger<RabbitMqDemoConsumerService>>();
        _mockConnection = new Mock<IConnection>();
        _mockChannel = new Mock<IModel>();

        _mockConnectionProvider.Setup(x => x.GetConnection()).Returns(_mockConnection.Object);
        _mockConnection.Setup(x => x.CreateModel()).Returns(_mockChannel.Object);

        // Setup channel methods
        var queueDeclareOk = new QueueDeclareOk("blogapp.posts.queue", 0, 0);
        _mockChannel.Setup(x => x.QueueDeclare("blogapp.posts.queue", true, false, false, null))
            .Returns(queueDeclareOk);

        _rabbitMqDemoConsumerService = new RabbitMqDemoConsumerService(
            _mockConnectionProvider.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task StartAsync_ShouldSetupRabbitMqResourcesCorrectly()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var task = _rabbitMqDemoConsumerService.StartAsync(cancellationToken);

        // Wait a bit for the execution to start
        await Task.Delay(50);

        // Assert
        _mockConnectionProvider.Verify(x => x.GetConnection(), Times.Once);
        _mockConnection.Verify(x => x.CreateModel(), Times.Once);
        _mockChannel.Verify(x => x.ExchangeDeclare("blogapp.exchange", ExchangeType.Topic, true, false, null), Times.Once);
        _mockChannel.Verify(x => x.QueueDeclare("blogapp.posts.queue", true, false, false, null), Times.Once);
        _mockChannel.Verify(x => x.QueueBind("blogapp.posts.queue", "blogapp.exchange", "post.*", null), Times.Once);

        // Verify that the task didn't fault
        Assert.NotEqual(TaskStatus.Faulted, task.Status);
    }

    [Fact]
    public async Task StartAsync_WhenCancelled_ShouldStopGracefully()
    {
        // Arrange
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act
        var task = _rabbitMqDemoConsumerService.StartAsync(cancellationToken);
        await Task.Delay(50); // Let it start

        // Cancel the token
        cancellationTokenSource.Cancel();

        // Wait a bit for graceful shutdown
        await Task.Delay(50);

        // Assert
        // The task should complete or be cancelled without faulting
        Assert.True(task.IsCompleted || task.IsCanceled);
        Assert.NotEqual(TaskStatus.Faulted, task.Status);
    }
}