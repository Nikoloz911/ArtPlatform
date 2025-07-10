/// SUBSCRIPTION SERVICE
/// https://localhost:7298

using CommonUtils.JWT;
using FluentValidation;
using SubscriptionService.Data;
using SubscriptionService.Helpers;
using SubscriptionService.RabbitMQ;

/// SUBSCRIPTION SERVICE 

var builder = WebApplication.CreateBuilder(args);

// JWT
builder.Services.AddSingleton<JwtConfigurator>();
var jwtConfigurator = new JwtConfigurator(builder.Configuration);
jwtConfigurator.ConfigureJwtAuthentication(builder.Services);
jwtConfigurator.ConfigureAuthorizationPolicies(builder.Services);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(SubscriptionMappingProfile));
//builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var rabbitMQConsumer = new SubscriptionRabbitMQ(app.Services);
rabbitMQConsumer.StartConsumer();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();