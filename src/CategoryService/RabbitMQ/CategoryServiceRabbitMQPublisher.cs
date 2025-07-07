using System.Text;
using System.Text.Json;
using Contracts.DTO;
using RabbitMQ.Client;

namespace CategoryService.RabbitMQ;

public class CategoryServiceRabbitMQPublisher
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public CategoryServiceRabbitMQPublisher()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare("category_created_by_id", false, false, false, null);
        _channel.QueueDeclare("category_updated_by_id", false, false, false, null);
        _channel.QueueDeclare("category_deleted_by_id", false, false, false, null);
    }

    public void PublishCategoryCreated(CategoryCreatedEvent createdEvent)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(createdEvent));
        _channel.BasicPublish("", "category_created_by_id", null, body);
    }

    public void PublishCategoryUpdated(CategoryUpdatedEvent updatedEvent)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(updatedEvent));
        _channel.BasicPublish("", "category_updated_by_id", null, body);
    }

    public void PublishCategoryDeleted(int id)
    {
        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new CategoryDeletedEvent { Id = id }));
        _channel.BasicPublish("", "category_deleted_by_id", null, body);
    }
}
