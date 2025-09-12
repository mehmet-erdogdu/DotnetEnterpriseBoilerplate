using System.Diagnostics.CodeAnalysis;

namespace BlogApp.Worker.Services;

[ExcludeFromCodeCoverage]
public class RabbitMqDemoConsumerService(IRabbitMqConnectionProvider connectionProvider, ILogger<RabbitMqDemoConsumerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var connection = connectionProvider.GetConnection();
        var channel = connection.CreateModel();
        channel.ExchangeDeclare("blogapp.exchange", ExchangeType.Topic, true);
        var queue = channel.QueueDeclare("blogapp.posts.queue", true, false, false);
        channel.QueueBind(queue.QueueName, "blogapp.exchange", "post.*");

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logger.LogInformation("RabbitMQ received: {Message}", message);
                // Simulate processing
                await Task.Delay(10, stoppingToken);
                channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing RabbitMQ message");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume(queue.QueueName, false, consumer);

        while (!stoppingToken.IsCancellationRequested) await Task.Delay(1000, stoppingToken);
    }
}