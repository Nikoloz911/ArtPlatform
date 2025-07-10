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
        _channel.ExchangeDeclare("user_events", ExchangeType.Fanout);
        _channel.QueueDeclare("user_created_user_service", false, false, false, null);
        _channel.QueueBind("user_created_user_service", "user_events", "");
        _channel.QueueDeclare("user_updated_by_id", false, false, false, null);
        _channel.QueueDeclare("user_deleted_by_id", false, false, false, null);
    }

    public void StartConsumer()
    {
        var createdConsumer = new EventingBasicConsumer(_channel);
        createdConsumer.Received += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var message = Encoding.UTF8.GetString(ea.Body.ToArray());
            var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(message);
            if (userEvent == null) return;

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
        };
        _channel.BasicConsume("user_created_user_service", true, createdConsumer);
    }

    public void PublishUserUpdated(UpdateUserEvent userUpdate)
    {
        var json = JsonSerializer.Serialize(userUpdate);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish("", "user_updated_by_id", null, body);
    }

    public void PublishUserDeleted(UserDeletedEvent userDeleted)
    {
        var json = JsonSerializer.Serialize(userDeleted);
        var body = Encoding.UTF8.GetBytes(json);
        _channel.BasicPublish("", "user_deleted_by_id", null, body);
    }
}