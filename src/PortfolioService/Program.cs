/// PORTFOLIO SERVICE 
/// https://localhost:7150

using CommonUtils.JWT;
using FluentValidation;
using PortfolioService.Data;
using PortfolioService.Helpers;
using PortfolioService.Validators;

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
builder.Services.AddAutoMapper(typeof(PortfolioMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<AddPortfolioValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdatePortfolioValidator>();

var app = builder.Build();

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