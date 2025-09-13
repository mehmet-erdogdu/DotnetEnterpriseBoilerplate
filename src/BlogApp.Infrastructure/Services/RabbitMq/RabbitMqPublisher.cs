using RabbitMQ.Client;

namespace BlogApp.Infrastructure.Services.RabbitMq;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default);
}

public class RabbitMqPublisher(IRabbitMqConnectionProvider connectionProvider) : IRabbitMqPublisher
{
    public async Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var connection = connectionProvider.GetConnection();
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, true, cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        await channel.BasicPublishAsync(exchange, routingKey, body, cancellationToken: cancellationToken);
    }
}