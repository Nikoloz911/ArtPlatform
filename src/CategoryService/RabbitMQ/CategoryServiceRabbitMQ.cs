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
    public class CategoryServiceRabbitMQ : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposed = false;

        public CategoryServiceRabbitMQ(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare("artwork_created_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_updated_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_deleted_by_id", false, false, false, null);
        }

        public void StartConsumer()
        {
            StartCreateArtworkConsumer();
            StartUpdateArtworkConsumer();
            StartDeleteArtworkConsumer();
        }

        private void StartCreateArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var createdEvent = JsonSerializer.Deserialize<ArtworkCreatedEvent>(message);
                if (createdEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == createdEvent.CategoryId);
                if (category == null)
                {
                    category = new Category
                    {
                        CategoryName = createdEvent.CategoryName
                    };
                    context.Categories.Add(category);
                    await context.SaveChangesAsync();
                }

                var artwork = new Artwork
                {
                    Title = createdEvent.Title,
                    ImageAdress = createdEvent.ImageAdress,
                    CategoryName = createdEvent.CategoryName,
                    CategoryId = category.Id,
                    CreationTime = createdEvent.CreationTime
                };

                context.Artwork.Add(artwork);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume("artwork_created_by_id", autoAck: true, consumer);
        }

        private void StartUpdateArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var updatedEvent = JsonSerializer.Deserialize<ArtworkUpdatedEvent>(message);
                if (updatedEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var artwork = await context.Artwork.FirstOrDefaultAsync(a => a.Id == updatedEvent.Id);
                if (artwork != null)
                {
                    artwork.Title = updatedEvent.Title;
                    artwork.CategoryId = updatedEvent.CategoryId;
                    artwork.ImageAdress = updatedEvent.ImageAdress;

                    var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == updatedEvent.CategoryId);
                    if (category != null)
                    {
                        artwork.CategoryName = category.CategoryName;
                    }

                    await context.SaveChangesAsync();
                }
            };

            _channel.BasicConsume("artwork_updated_by_id", autoAck: true, consumer);
        }

        private void StartDeleteArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var deletedEvent = JsonSerializer.Deserialize<ArtworkDeletedEvent>(message);
                if (deletedEvent == null) return;

                using var scope = _serviceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var artwork = await context.Artwork.FirstOrDefaultAsync(a => a.Id == deletedEvent.Id);
                if (artwork != null)
                {
                    context.Artwork.Remove(artwork);
                    await context.SaveChangesAsync();
                }
            };

            _channel.BasicConsume("artwork_deleted_by_id", autoAck: true, consumer);
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