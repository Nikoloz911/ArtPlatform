using System.Text;
using System.Text.Json;
using ArtworkService.Data;
using Contracts.DTO;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ArtworkService.RabbitMQ;

public class ArtworkServiceRabbitMQ
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public ArtworkServiceRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare("category_created_by_id", false, false, false, null);
        _channel.QueueDeclare("category_updated_by_id", false, false, false, null);
        _channel.QueueDeclare("category_deleted_by_id", false, false, false, null);
    }

    public void StartConsumer()
    {
        StartDeleteCategoryConsumer();
        StartUpdateCategoryConsumer();
        StartCreateCategoryConsumer();
    }

    private void StartDeleteCategoryConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var deletedEvent = JsonSerializer.Deserialize<CategoryDeletedEvent>(message);
            if (deletedEvent == null) return;

            using var context = new DataContext();
            var artworks = context.Artwork.Where(a => a.CategoryId == deletedEvent.Id);
            if (await artworks.AnyAsync())
            {
                context.Artwork.RemoveRange(artworks);
                await context.SaveChangesAsync();
            }

            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == deletedEvent.Id);
            if (category != null)
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
            }
        };

        _channel.BasicConsume("category_deleted_by_id", true, consumer);
    }

    private void StartUpdateCategoryConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var updatedEvent = JsonSerializer.Deserialize<CategoryUpdatedEvent>(message);
            if (updatedEvent == null) return;

            using var context = new DataContext();
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Id == updatedEvent.Id);
            if (category == null) return;

            category.CategoryName = updatedEvent.CategoryName;
            category.Description = updatedEvent.Description;
            category.ImageURL = updatedEvent.ImageURL;

            context.Categories.Update(category);
            await context.SaveChangesAsync();
        };

        _channel.BasicConsume("category_updated_by_id", true, consumer);
    }

    private void StartCreateCategoryConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var createdEvent = JsonSerializer.Deserialize<CategoryCreatedEvent>(message);
            if (createdEvent == null) return;

            using var context = new DataContext();
            var alreadyExists = await context.Categories.AnyAsync(c => c.Id == createdEvent.Id);
            if (alreadyExists) return;

            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories ON");

            context.Categories.Add(new Models.Category
            {
                CategoryName = createdEvent.CategoryName,
                Description = createdEvent.Description,
                ImageURL = createdEvent.ImageURL,
                CreationDate = createdEvent.CreationDate 
            });

            await context.SaveChangesAsync();

            await context.Database.ExecuteSqlRawAsync("SET IDENTITY_INSERT Categories OFF");
        };

        _channel.BasicConsume("category_created_by_id", true, consumer);
    }
}