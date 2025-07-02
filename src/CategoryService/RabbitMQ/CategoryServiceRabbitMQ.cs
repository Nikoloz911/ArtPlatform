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

            // Declare the same queues as the publisher
            _channel.QueueDeclare("artwork_created_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_updated_by_id", false, false, false, null);
            _channel.QueueDeclare("artwork_deleted_by_id", false, false, false, null);
        }

        public void StartConsumer()
        {
            StartCreateArtworkConsumer();
            StartUpdateArtworkConsumer();
            StartDeleteArtworkConsumer();
            Console.WriteLine("RabbitMQ consumers started for CategoryService");
        }

        private void StartCreateArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"Received artwork created message: {message}");

                    var createdEvent = JsonSerializer.Deserialize<ArtworkCreatedEvent>(message);
                    if (createdEvent == null)
                    {
                        Console.WriteLine("Failed to deserialize ArtworkCreatedEvent");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    // Check if category exists in CategoryService database
                    var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == createdEvent.CategoryId);
                    if (category == null)
                    {
                        // Create the category if it doesn't exist
                        category = new Category
                        {
                            Id = createdEvent.CategoryId,
                            CategoryName = createdEvent.CategoryName
                        };
                        context.Categories.Add(category);
                        Console.WriteLine($"Created new category '{createdEvent.CategoryName}' with ID {createdEvent.CategoryId}");
                    }

                    // Check if artwork already exists in CategoryService database
                    var existingArtwork = await context.Artwork.FirstOrDefaultAsync(a => a.Id == createdEvent.Id);
                    if (existingArtwork == null)
                    {
                        // Create the artwork in CategoryService database
                        var artwork = new Artwork
                        {
                            Id = createdEvent.Id,
                            Title = createdEvent.Title,
                            CategoryId = createdEvent.CategoryId,
                            CreationTime = createdEvent.CreationTime,
                            ImageAdress = createdEvent.ImageAdress
                        };

                        context.Artwork.Add(artwork);
                        await context.SaveChangesAsync();

                        Console.WriteLine($"Created artwork '{createdEvent.Title}' (ID: {createdEvent.Id}) in CategoryService database");
                    }
                    else
                    {
                        Console.WriteLine($"Artwork '{createdEvent.Title}' (ID: {createdEvent.Id}) already exists in CategoryService database");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing artwork created event: {ex.Message}");
                }
            };

            _channel.BasicConsume("artwork_created_by_id", autoAck: true, consumer);
        }

        private void StartUpdateArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"Received artwork updated message: {message}");

                    var updatedEvent = JsonSerializer.Deserialize<ArtworkUpdatedEvent>(message);
                    if (updatedEvent == null)
                    {
                        Console.WriteLine("Failed to deserialize ArtworkUpdatedEvent");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    // Find and update the artwork in CategoryService database
                    var artwork = await context.Artwork.FirstOrDefaultAsync(a => a.Id == updatedEvent.Id);
                    if (artwork != null)
                    {
                        artwork.Title = updatedEvent.Title;
                        artwork.CategoryId = updatedEvent.CategoryId;
                        artwork.ImageAdress = updatedEvent.ImageAdress;

                        await context.SaveChangesAsync();
                        Console.WriteLine($"Updated artwork '{updatedEvent.Title}' (ID: {updatedEvent.Id}) in CategoryService database");
                    }
                    else
                    {
                        Console.WriteLine($"Artwork with ID {updatedEvent.Id} not found in CategoryService database");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing artwork updated event: {ex.Message}");
                }
            };

            _channel.BasicConsume("artwork_updated_by_id", autoAck: true, consumer);
        }

        private void StartDeleteArtworkConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine($"Received artwork deleted message: {message}");

                    var deletedEvent = JsonSerializer.Deserialize<ArtworkDeletedEvent>(message);
                    if (deletedEvent == null)
                    {
                        Console.WriteLine("Failed to deserialize ArtworkDeletedEvent");
                        return;
                    }

                    using var scope = _serviceProvider.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                    // Find and delete the artwork from CategoryService database
                    var artwork = await context.Artwork.FirstOrDefaultAsync(a => a.Id == deletedEvent.Id);
                    if (artwork != null)
                    {
                        context.Artwork.Remove(artwork);
                        await context.SaveChangesAsync();
                        Console.WriteLine($"Deleted artwork '{deletedEvent.Title}' (ID: {deletedEvent.Id}) from CategoryService database");

                        // Check if category has any remaining artworks
                        var remainingArtworks = await context.Artwork.AnyAsync(a => a.CategoryId == deletedEvent.CategoryId);
                        if (!remainingArtworks)
                        {
                            Console.WriteLine($"Category {deletedEvent.CategoryId} has no remaining artworks");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Artwork with ID {deletedEvent.Id} not found in CategoryService database");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing artwork deleted event: {ex.Message}");
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