using System.Diagnostics.CodeAnalysis;

namespace BlogApp.Worker.Services;

[ExcludeFromCodeCoverage]
public class RabbitMqDemoConsumerService(IRabbitMqConnectionProvider connectionProvider, ILogger<RabbitMqDemoConsumerService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var connection = connectionProvider.GetConnection();
        // Use CreateChannelAsync instead of CreateModel in v7
        var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.ExchangeDeclareAsync("blogapp.exchange", ExchangeType.Topic, true, cancellationToken: stoppingToken);
        var queue = await channel.QueueDeclareAsync("blogapp.posts.queue", true, false, false, cancellationToken: stoppingToken);
        await channel.QueueBindAsync(queue.QueueName, "blogapp.exchange", "post.*", cancellationToken: stoppingToken);

        // Use AsyncEventingBasicConsumer with the correct event handler
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logger.LogWarning("RabbitMQ received: {Message}", message);
                // Simulate processing
                await Task.Delay(10, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing RabbitMQ message");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(queue.QueueName, false, consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested) await Task.Delay(1000, stoppingToken);
    }
}