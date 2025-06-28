using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using AuthService.Data;
using AuthService.Models;
using Contracts.DTO;
using Microsoft.EntityFrameworkCore;

namespace AuthService.RabbitMQ
{
    public class AuthServiceRabbitMQ
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public AuthServiceRabbitMQ(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "user_updated_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "user_deleted_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }
        public void StartConsumer()
        {
            var updatedConsumer = new EventingBasicConsumer(_channel);

            updatedConsumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userUpdated = JsonSerializer.Deserialize<UpdateUserEvent>(message);
                if (userUpdated == null) return;

                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userUpdated.Id);
                if (user == null) return;

                user.Name = userUpdated.Name;
                user.Password = userUpdated.Password;
                user.Biography = userUpdated.Biography;
                user.ProfilePictureUrl = userUpdated.ProfilePictureUrl;

                context.Users.Update(user);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume(queue: "user_updated_by_id", autoAck: true, consumer: updatedConsumer);

            var deletedConsumer = new EventingBasicConsumer(_channel);

            deletedConsumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userDeleted = JsonSerializer.Deserialize<UserDeletedEvent>(message);
                if (userDeleted == null) return;

                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDeleted.Id);
                if (user == null) return;

                context.Users.Remove(user);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume(queue: "user_deleted_by_id", autoAck: true, consumer: deletedConsumer);
        }
    }
}