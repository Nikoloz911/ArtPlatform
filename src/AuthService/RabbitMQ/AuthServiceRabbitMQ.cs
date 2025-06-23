using RabbitMQ.Client;
using AuthService.Data;
using AuthService.Models;
using System.Text;
using System.Text.Json;
using Contracts.DTO;
using RabbitMQ.Client.Events;

namespace AuthService.RabbitMQ;
public class AuthServiceRabbitMQ
{
    private readonly IServiceProvider _serviceProvider;
    private IConnection _connection;
    private IModel _channel;

    public AuthServiceRabbitMQ(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(queue: "user_update_event", durable: false, exclusive: false, autoDelete: false, arguments: null);
    }

    public void StartConsumer()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DataContext>();

            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var updateEvent = JsonSerializer.Deserialize<UpdateUserEvent>(json);

            if (updateEvent != null)
            {
                var user = context.Users.FirstOrDefault(u => u.Email == updateEvent.Email);
                if (user != null)
                {
                    user.Name = updateEvent.Name;
                    user.Password = updateEvent.Password;
                    user.Biography = updateEvent.Biography;
                    user.ProfilePictureUrl = updateEvent.ProfilePictureUrl;

                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                }
            }
        };

        _channel.BasicConsume(queue: "user_update_event", autoAck: true, consumer: consumer);
    }
}

