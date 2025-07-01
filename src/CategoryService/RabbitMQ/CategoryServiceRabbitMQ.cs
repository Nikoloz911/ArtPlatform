using System.Text;
using System.Text.Json;
using Contracts.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CategoryService.Data;
using CategoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoryService.RabbitMQ
{
    public class CategoryServiceRabbitMQ
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public CategoryServiceRabbitMQ()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchanges
            _channel.ExchangeDeclare("artwork_exchange", ExchangeType.Fanout, durable: true);
            _channel.ExchangeDeclare("artwork_update_exchange", ExchangeType.Fanout, durable: true);

            // Declare and bind queues to the exchanges
            _channel.QueueDeclare(queue: "category_artwork_created_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: "category_artwork_created_queue", exchange: "artwork_exchange", routingKey: "");

            _channel.QueueDeclare(queue: "category_artwork_updated_queue", durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue: "category_artwork_updated_queue", exchange: "artwork_update_exchange", routingKey: "");
        }

        public void StartConsuming()
        {
            var createdConsumer = new EventingBasicConsumer(_channel);
            createdConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var createdEvent = JsonSerializer.Deserialize<ArtworkCreatedEvent>(message);
                if (createdEvent == null) return;

                using var context = new DataContext();
                var exists = await context.Artwork.AnyAsync(a => a.Id == createdEvent.Id);
                if (exists) return;

                var artwork = new Artwork
                {
                    Id = createdEvent.Id,
                    Title = createdEvent.Title,
                    CategoryId = createdEvent.CategoryId,
                    CategoryName = createdEvent.CategoryName,
                    CreationTime = createdEvent.CreationTime,
                    ImageAdress = createdEvent.ImageAdress
                };

                context.Artwork.Add(artwork);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume(queue: "category_artwork_created_queue", autoAck: true, consumer: createdConsumer);

            var updatedConsumer = new EventingBasicConsumer(_channel);
            updatedConsumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var updatedEvent = JsonSerializer.Deserialize<ArtworkUpdatedEvent>(message);
                if (updatedEvent == null) return;

                using var context = new DataContext();
                var artwork = await context.Artwork.FirstOrDefaultAsync(a => a.Id == updatedEvent.Id);
                if (artwork == null) return;

                artwork.Title = updatedEvent.Title;
                artwork.CategoryId = updatedEvent.CategoryId;
                artwork.ImageAdress = updatedEvent.ImageAdress;

                context.Artwork.Update(artwork);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume(queue: "category_artwork_updated_queue", autoAck: true, consumer: updatedConsumer);
        }
    }
}