/// CATEGORY SERVICE 
/// https://localhost:7221

using CategoryService.Data;
using CategoryService.DTOs;
using CategoryService.Helpers;
using CategoryService.Validators;
using CommonUtils.JWT_Config;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(CategoryMappingProfile));
builder.Services.AddScoped<IValidator<AddCategoryDTO>, AddCategoryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCategoryValidator>();


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