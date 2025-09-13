using RabbitMQ.Client;

namespace BlogApp.Infrastructure.Services.RabbitMq;

public interface IRabbitMqConnectionProvider : IDisposable
{
    IConnection GetConnection();
}

public class RabbitMqConnectionProvider(IConfiguration configuration) : IRabbitMqConnectionProvider
{
    private readonly ConnectionFactory _connectionFactory = new()
    {
        HostName = configuration["RabbitMQ:HostName"]!,
        Port = int.Parse(configuration["RabbitMQ:Port"]!),
        UserName = configuration["RabbitMQ:UserName"]!,
        Password = configuration["RabbitMQ:Password"]!,
        VirtualHost = configuration["RabbitMQ:VirtualHost"]!,
        ClientProvidedName = "blogapp-connection"
    };

    private IConnection? _connection;
    private bool _disposed;

    public IConnection GetConnection()
    {
        if (_connection is { IsOpen: true })
            return _connection;
        _connection?.Dispose();
        _connection = _connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
        return _connection;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing) _connection?.Dispose();
            _disposed = true;
        }
    }
}