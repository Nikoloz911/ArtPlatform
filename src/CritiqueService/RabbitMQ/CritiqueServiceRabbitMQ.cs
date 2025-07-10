using System.Text;
using System.Text.Json;
using Contracts.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using CritiqueService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace CritiqueService.RabbitMQ
{
    public class CritiqueServiceRabbitMQ : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public CritiqueServiceRabbitMQ(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare("artwork_created_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_updated_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_deleted_by_id", false, false, false, null);

            StartCreateArtworkConsumer();
            StartUpdateArtworkConsumer();
            StartDeleteArtworkConsumer();

            return Task.CompletedTask;
        }

        private void StartCreateArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var createdEvent = JsonSerializer.Deserialize<ArtworkCreatedEvent>(message);
                if (createdEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var exists = await context.Artworks.AnyAsync(a => a.Id == createdEvent.Id);
                if (!exists)
                {
                    context.Artworks.Add(new Models.Artwork
                    {
                        Title = createdEvent.Title,
                        ImageAdress = createdEvent.ImageAdress,
                        CategoryId = createdEvent.CategoryId,
                        CreationTime = createdEvent.CreationTime,
                        CategoryName = createdEvent.CategoryName
                    });

                    await context.SaveChangesAsync();
                }
            };

            _channel.BasicConsume("artwork_created_by_id", true, consumer);
        }

        private void StartUpdateArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var updatedEvent = JsonSerializer.Deserialize<ArtworkUpdatedEvent>(message);
                if (updatedEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var artwork = await context.Artworks.FirstOrDefaultAsync(a => a.Id == updatedEvent.Id);
                if (artwork != null)
                {
                    artwork.Title = updatedEvent.Title;
                    artwork.ImageAdress = updatedEvent.ImageAdress;
                    await context.SaveChangesAsync();
                }
            };

            _channel.BasicConsume("artwork_updated_by_id", true, consumer);
        }

        private void StartDeleteArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var deletedEvent = JsonSerializer.Deserialize<ArtworkDeletedEvent>(message);
                if (deletedEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var artwork = await context.Artworks.FirstOrDefaultAsync(a => a.Id == deletedEvent.Id);
                if (artwork != null)
                {
                    context.Artworks.Remove(artwork);
                    await context.SaveChangesAsync();
                }
            };

            _channel.BasicConsume("artwork_deleted_by_id", true, consumer);
        }

        public override void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
