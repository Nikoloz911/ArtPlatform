/// USER SERVICE
/// https://localhost:7086/


using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Contracts.DTO;
using UserService.Data;
using UserService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var factory = new ConnectionFactory() { HostName = "localhost" };
var connection = factory.CreateConnection(); 
var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "user_created",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, ea) =>
{
    using var scope = app.Services.CreateScope();
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
            ProfilePictureUrl = userEvent.ProfilePictureUrl
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
    }
};

channel.BasicConsume(
    queue: "user_created",
    autoAck: true,
    consumer: consumer
);

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
