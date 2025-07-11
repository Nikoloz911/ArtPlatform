/// AUTH SERVICE
/// https://localhost:7046

using FluentValidation;
using AuthService.Data;
using AuthService.Helper;
using AuthService.Validators;
using AuthService.RabbitMQ;
using CommonUtils.JWT;

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
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();
builder.Services.AddScoped<IJWTService, JWTService>();

var app = builder.Build();

var rabbitMQConsumer = new AuthServiceRabbitMQ(app.Services);
rabbitMQConsumer.StartConsumer();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();