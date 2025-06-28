/// USER SERVICE
/// https://localhost:7086

using CommonUtils.JWT;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using UserService.Data;
using UserService.Helpers;
using UserService.RabbitMQ;
using UserService.Validators;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAutoMapper(typeof(UserMappingProfile));
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserValidator>();

var jwtKey = builder.Configuration["JWT:Key"];
var jwtIssuer = builder.Configuration["JWT:Issuer"];
var jwtAudience = builder.Configuration["JWT:Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
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

builder.Services.AddScoped<IJWTService, JWTService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var rabbitMQConsumer = new UserServiceRabbitMQ(app.Services);
rabbitMQConsumer.StartConsumer();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();