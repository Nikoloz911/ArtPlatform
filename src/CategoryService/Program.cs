/// CATEGORY SERVICE 
/// https://localhost:7221
using CategoryService.Data;
using CategoryService.DTOs;
using CategoryService.Helpers;
using CategoryService.RabbitMQ;
using CategoryService.Validators;
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
builder.Services.AddAutoMapper(typeof(CategoryMappingProfile));
builder.Services.AddScoped<IValidator<AddCategoryDTO>, AddCategoryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCategoryValidator>();
builder.Services.AddSingleton<CategoryServiceRabbitMQPublisher>();
builder.Services.AddSingleton<CategoryServiceRabbitMQ>();

var app = builder.Build();

var rabbitMQConsumer = app.Services.GetRequiredService<CategoryServiceRabbitMQ>();
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