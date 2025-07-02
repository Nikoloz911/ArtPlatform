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

            // Declare queues - these should match the consumer queues exactly
            _channel.QueueDeclare("artwork_created_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_updated_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_deleted_by_id", false, false, false, null);
        }

        public void PublishArtworkCreated(ArtworkCreatedEvent createdEvent)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(createdEvent));
                _channel.BasicPublish(exchange: "", routingKey: "artwork_created_by_id", basicProperties: null, body: body);
                Console.WriteLine($"Published ArtworkCreated event for ID: {createdEvent.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing ArtworkCreated event: {ex.Message}");
                throw;
            }
        }

        public void PublishArtworkUpdated(ArtworkUpdatedEvent updatedEvent)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(updatedEvent));
                _channel.BasicPublish(exchange: "", routingKey: "artwork_updated_by_id", basicProperties: null, body: body);
                Console.WriteLine($"Published ArtworkUpdated event for ID: {updatedEvent.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing ArtworkUpdated event: {ex.Message}");
                throw;
            }
        }

        public void PublishArtworkDeleted(ArtworkDeletedEvent deletedEvent)
        {
            try
            {
                var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(deletedEvent));
                _channel.BasicPublish(exchange: "", routingKey: "artwork_deleted_by_id", basicProperties: null, body: body);
                Console.WriteLine($"Published ArtworkDeleted event for ID: {deletedEvent.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error publishing ArtworkDeleted event: {ex.Message}");
                throw;
            }
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