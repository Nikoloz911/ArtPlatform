using Contracts.DTO;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SubscriptionService.Data;
using SubscriptionService.Models;
using System.Text;
using System.Text.Json;

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

            _channel.ExchangeDeclare("user_events", ExchangeType.Fanout);
            _channel.QueueDeclare("user_created_subscription", false, false, false, null);
            _channel.QueueBind("user_created_subscription", "user_events", "");
            _channel.QueueDeclare("user_updated_by_id", false, false, false, null);
            _channel.QueueDeclare("user_deleted_by_id", false, false, false, null);
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
                Console.WriteLine("[Subscription] Received user_created event");
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
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

            _channel.BasicConsume("user_created_subscription", true, consumer);
        }

        private void ConsumeUserUpdated()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
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

            _channel.BasicConsume("user_updated_by_id", true, consumer);
        }

        private void ConsumeUserDeleted()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var userDeleted = JsonSerializer.Deserialize<UserDeletedEvent>(message);
                if (userDeleted == null) return;

                var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userDeleted.Id);
                if (user == null) return;

                context.Users.Remove(user);
                await context.SaveChangesAsync();
            };

            _channel.BasicConsume("user_deleted_by_id", true, consumer);
        }
    }
}