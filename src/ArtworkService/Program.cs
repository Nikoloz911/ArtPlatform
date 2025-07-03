/// ARTWORK SERVICE 
/// https://localhost:7147

using ArtworkService.Data;
using ArtworkService.DTOs;
using ArtworkService.Helpers;
using ArtworkService.RabbitMQ;
using ArtworkService.Validators;
using CommonUtils.JWT;
using FluentValidation;

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
builder.Services.AddAutoMapper(typeof(ArtworkMappingProfile));
builder.Services.AddScoped<IValidator<AddArtworkDTO>, AddArtworkValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateArtworkValidator>();

var app = builder.Build();

var rabbitMQConsumer = new ArtworkServiceRabbitMQ();
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