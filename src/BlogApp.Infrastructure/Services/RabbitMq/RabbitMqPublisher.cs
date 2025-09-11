using RabbitMQ.Client;

namespace BlogApp.Infrastructure.Services.RabbitMq;

public interface IRabbitMqPublisher
{
    Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default);
}

public class RabbitMqPublisher(IRabbitMqConnectionProvider connectionProvider) : IRabbitMqPublisher
{
    public Task PublishAsync<T>(string exchange, string routingKey, T message, CancellationToken cancellationToken = default)
    {
        var connection = connectionProvider.GetConnection();
        using var channel = connection.CreateModel();
        channel.ExchangeDeclare(exchange, ExchangeType.Topic, true);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
        channel.BasicPublish(exchange, routingKey, props, body);
        return Task.CompletedTask;
    }
}