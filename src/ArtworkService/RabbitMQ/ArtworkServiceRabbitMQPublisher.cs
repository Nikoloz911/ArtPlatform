using Contracts.DTO;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ArtworkService.RabbitMQ
{
    public class ArtworkServiceRabbitMQPublisher : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private bool _disposed = false;

        public ArtworkServiceRabbitMQPublisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare("artwork_created_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_updated_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_deleted_by_id", false, false, false, null);
        }

        public void PublishArtworkCreated(ArtworkCreatedEvent createdEvent)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(createdEvent));
            _channel.BasicPublish(exchange: "", routingKey: "artwork_created_by_id", basicProperties: null, body: body);
        }

        public void PublishArtworkUpdated(ArtworkUpdatedEvent updatedEvent)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(updatedEvent));
            _channel.BasicPublish(exchange: "", routingKey: "artwork_updated_by_id", basicProperties: null, body: body);
        }

        public void PublishArtworkDeleted(ArtworkDeletedEvent deletedEvent)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deletedEvent));
            _channel.BasicPublish(exchange: "", routingKey: "artwork_deleted_by_id", basicProperties: null, body: body);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
                _disposed = true;
            }
        }
    }
}