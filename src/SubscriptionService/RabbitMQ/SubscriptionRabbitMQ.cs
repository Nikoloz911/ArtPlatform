using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using SubscriptionService.Data;
using SubscriptionService.Models;
using Contracts.DTO;
using Microsoft.EntityFrameworkCore;

namespace SubscriptionService.RabbitMQ
{
    public class SubscriptionRabbitMQ
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public SubscriptionRabbitMQ(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "user_created", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "user_updated_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "user_deleted_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void StartConsumer()
        {
            ConsumeUserCreated();
            ConsumeUserUpdated();
            ConsumeUserDeleted();
        }

        private void ConsumeUserCreated()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userCreated = JsonSerializer.Deserialize<UserCreatedEvent>(message);
                if (userCreated == null) return;

                var user = new User
                {
                    Name = userCreated.Name,
                    Email = userCreated.Email,
                    Password = userCreated.Password,
                    Biography = userCreated.Biography,
                    ProfilePictureUrl = userCreated.ProfilePictureUrl,
                    Role = userCreated.Role,
                    IsVerified = userCreated.IsVerified,
                    VerificationCode = userCreated.VerificationCode,
                    VerificationCodeExpirity = userCreated.VerificationCodeExpirity
                };

                context.Users.Add(user);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume(queue: "user_created", autoAck: true, consumer: consumer);
        }

        private void ConsumeUserUpdated()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
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

            _channel.BasicConsume(queue: "user_updated_by_id", autoAck: true, consumer: consumer);
        }

        private void ConsumeUserDeleted()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
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

            _channel.BasicConsume(queue: "user_deleted_by_id", autoAck: true, consumer: consumer);
        }
    }
}
