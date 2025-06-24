/// PORTFOLIO SERVICE 
/// https://localhost:7150

using CommonUtils.JWT_Config;
using FluentValidation;
using PortfolioService.Data;
using PortfolioService.Helpers;
using PortfolioService.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(PortfolioMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<AddPortfolioValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdatePortfolioValidator>();

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

app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();