/// CATEGORY SERVICE 
/// https://localhost:7221

using CategoryService.Data;
using CategoryService.DTOs;
using CategoryService.Helpers;
using CategoryService.Validators;
using CommonUtils.JWT;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(CategoryMappingProfile));
builder.Services.AddScoped<IValidator<AddCategoryDTO>, AddCategoryValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateCategoryValidator>();
builder.Services.AddScoped<IJWTService, JWTService>();

var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"));
    options.AddPolicy("OwnerOnly", policy => policy.RequireRole("OWNER"));
    options.AddPolicy("ArtistOnly", policy => policy.RequireRole("ARTIST"));
    options.AddPolicy("CriticOnly", policy => policy.RequireRole("CRITIC"));
    options.AddPolicy("AdminOrOwner", policy => policy.RequireRole("ADMIN", "OWNER"));
    options.AddPolicy("AdminOrArtist", policy => policy.RequireRole("ADMIN", "ARTIST"));
    options.AddPolicy("ArtistOrCritic", policy => policy.RequireRole("ARTIST", "CRITIC"));
    options.AddPolicy("OwnerOrAdmin", policy => policy.RequireRole("OWNER", "ADMIN"));
});

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