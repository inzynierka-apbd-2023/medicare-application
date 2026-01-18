using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Microsoft.Extensions.Hosting;

public static class AuthExtensions
{
    public static IHostApplicationBuilder AddMedicareAuthentication(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"] ?? "MedicareApp";
        var audience = jwtSettings["Audience"] ?? "MedicareUsers";

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey is not configured in 'Jwt:SecretKey'.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role",
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue("accessToken", out var token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        return builder;
    }
}
