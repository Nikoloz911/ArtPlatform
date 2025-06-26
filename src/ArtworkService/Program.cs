/// ARTWORK SERVICE 

using ArtworkService.Data;
using ArtworkService.DTOs;
using ArtworkService.Helpers;
using ArtworkService.Validators;
using CommonUtils.JWT_Config;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(ArtworkMappingProfile));
builder.Services.AddScoped<IValidator<AddArtworkDTO>, AddArtworkValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateArtworkValidator>();

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