/// USER SERVICE
/// https://localhost:7086

using CommonUtils.JWT;
using CommonUtils.JWT_Config;
using FluentValidation;
using UserService.Data;
using UserService.Helpers;
using UserService.RabbitMQ;
using UserService.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();
builder.Services.AddScoped<IJWTService, JWTService>();
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();

var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

builder.Services.ConfigureJwt(jwtKey, jwtIssuer, jwtAudience);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var rabbitMQConsumer = new UserServiceRabbitMQ(app.Services);
rabbitMQConsumer.StartConsumer();

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();