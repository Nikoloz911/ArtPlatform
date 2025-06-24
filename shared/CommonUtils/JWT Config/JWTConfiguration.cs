using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CommonUtils.JWT_Config;
public static class JwtServiceExtensions
{
    public static IServiceCollection ConfigureJwt(this IServiceCollection services, string jwtKey, string issuer, string audience)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("OwnerOnly", policy => policy.RequireRole("Owner"));
            options.AddPolicy("ArtistOnly", policy => policy.RequireRole("Artist"));
            options.AddPolicy("CriticOnly", policy => policy.RequireRole("Critic"));

            // COMBINE POLICIES FOR MULTIPLE ROLES
            options.AddPolicy("AdminOrOwner", policy => policy.RequireRole("Admin", "Owner"));
            options.AddPolicy("AdminOrArtist", policy => policy.RequireRole("Admin", "Artist"));
            options.AddPolicy("ArtistOrCritic", policy => policy.RequireRole("Artist", "Critic"));
        });
        return services;
    }
}
