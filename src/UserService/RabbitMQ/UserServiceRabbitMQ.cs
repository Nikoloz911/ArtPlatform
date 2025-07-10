using Contracts.DTO;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using UserService.Data;
using UserService.Models;
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
        _channel.QueueDeclare(queue: "user_updated_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueDeclare(queue: "user_deleted_by_id", durable: false, exclusive: false, autoDelete: false, arguments: null);
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
                    Id = userEvent.Id,
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

        // If you want to consume updates/deletes here, add consumers for these queues,
        // but usually UserService *publishes* them.

        // ... possible other consumers
    }

    public void PublishUserUpdated(UpdateUserEvent userUpdate)
    {
        var json = JsonSerializer.Serialize(userUpdate);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "", routingKey: "user_updated_by_id", basicProperties: null, body: body);
    }

    public void PublishUserDeleted(UserDeletedEvent userDeleted)
    {
        var json = JsonSerializer.Serialize(userDeleted);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish(exchange: "", routingKey: "user_deleted_by_id", basicProperties: null, body: body);
    }
}
