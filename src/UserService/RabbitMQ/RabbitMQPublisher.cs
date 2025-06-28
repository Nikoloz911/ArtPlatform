using Contracts.DTO;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace UserService.RabbitMQ;
public class RabbitMQPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQPublisher()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "user_updated_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "user_deleted_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void PublishUserUpdated(UpdateUserEvent userUpdate)
    {
        var json = JsonSerializer.Serialize(userUpdate);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "", routingKey: "user_updated_by_id", basicProperties: null, body: body);
    }

    public void PublishUserDeleted(UserDeletedEvent userDeleted)
    {
        var json = JsonSerializer.Serialize(userDeleted);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "", routingKey: "user_deleted_by_id", basicProperties: null, body: body);
    }
}