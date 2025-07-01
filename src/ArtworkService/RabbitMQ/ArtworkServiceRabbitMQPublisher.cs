using Contracts.DTO;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace ArtworkService.RabbitMQ
{
    public class ArtworkServiceRabbitMQPublisher
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public ArtworkServiceRabbitMQPublisher()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("artwork_exchange", ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare("artwork_update_exchange", ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare("artwork_delete_exchange", ExchangeType.Fanout, durable: true);
        }

        public void PublishArtworkCreatedEvent(ArtworkCreatedEvent @event)
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "artwork_exchange", routingKey: "", basicProperties: null, body: body);
        }

        public void PublishArtworkUpdatedEvent(ArtworkUpdatedEvent @event)
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "artwork_update_exchange", routingKey: "", basicProperties: null, body: body);
        }

        public void PublishArtworkDeletedEvent(ArtworkDeletedEvent @event)
        {
            var message = JsonSerializer.Serialize(@event);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "artwork_delete_exchange", routingKey: "", basicProperties: null, body: body);
        }
    }
}