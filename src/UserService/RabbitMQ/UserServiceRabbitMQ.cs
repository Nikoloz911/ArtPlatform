using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserService.Data;
using UserService.Models;
using Contracts.DTO;

namespace UserService.RabbitMQ
{
    public class UserServiceRabbitMQ
    {
        private readonly IServiceProvider _serviceProvider;
        private IConnection _connection;
        private IModel _channel;

        public UserServiceRabbitMQ(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { HostName = "localhost" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "user_created", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "user_updated", durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void StartConsumer()
        {
            var createdConsumer = new EventingBasicConsumer(_channel);
            createdConsumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
                if (userEvent != null)
                {
                    var user = new User
                    {
                        Name = userEvent.Name,
                        Email = userEvent.Email,
                        Password = userEvent.Password,
                        Biography = userEvent.Biography,
                        ProfilePictureUrl = userEvent.ProfilePictureUrl,
                        Role = userEvent.Role,
                        IsVerified = userEvent.IsVerified,
                        VerificationCode = userEvent.VerificationCode,
                        VerificationCodeExpirity = userEvent.VerificationCodeExpirity
                    };

                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }
            };

            _channel.BasicConsume(queue: "user_created", autoAck: true, consumer: createdConsumer);

            var updatedConsumer = new EventingBasicConsumer(_channel);
            updatedConsumer.Received += async (model, ea) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();

                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var userUpdated = JsonSerializer.Deserialize<UserVerifiedEvent>(message);
                if (userUpdated != null)
                {
                    var user = context.Users.FirstOrDefault(u => u.Id == userUpdated.Id);
                    if (user != null)
                    {
                        user.IsVerified = userUpdated.IsVerified;
                        user.VerificationCode = userUpdated.VerificationCode;
                        user.VerificationCodeExpirity = userUpdated.VerificationCodeExpirity;

                        context.Users.Update(user);
                        await context.SaveChangesAsync();
                    }
                }
            };

            _channel.BasicConsume(queue: "user_updated", autoAck: true, consumer: updatedConsumer);
        }
    }
}
