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

            _channel.QueueDeclare(
                queue: "user_created",
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
        }

        public void StartConsumer()
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
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

            _channel.BasicConsume(
                queue: "user_created",
                autoAck: true,
                consumer: consumer
            );
        }
    }
}
