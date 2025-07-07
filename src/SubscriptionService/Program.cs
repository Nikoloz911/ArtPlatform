
using CommonUtils.JWT;

/// SUBSCRIPTION SERVICE 

var builder = WebApplication.CreateBuilder(args);

// JWT
builder.Services.AddSingleton<JwtConfigurator>();
var jwtConfigurator = new JwtConfigurator(builder.Configuration);
jwtConfigurator.ConfigureJwtAuthentication(builder.Services);
jwtConfigurator.ConfigureAuthorizationPolicies(builder.Services);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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