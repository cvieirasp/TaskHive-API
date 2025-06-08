using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Resend;
using System;
using System.Text;
using TaskHive.Domain.Repositories;
using TaskHive.Domain.Services;
using TaskHive.Infrastructure.Repositories;
using TaskHive.Infrastructure.Services;

namespace TaskHive.Infrastructure.Configuration;

public static class InfrastructureServiceConfiguration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register Npgsql connection as scoped
        services.AddScoped(sp =>
        {
            //var url = configuration.GetConnectionString("LOCAL_CONNECTION")
            //    ?? throw new InvalidOperationException("Connection string 'LOCAL_CONNECTION' not found.");

            var url = configuration["DATABASE_URL"] ?? throw new InvalidOperationException("Database URL 'DATABASE_URL' not found.");

            // Convert Heroku-style format to Npgsql format.
            var uri = new Uri(url);
            var userInfo = uri.UserInfo.Split(':');

            var connectionString = $"Host={uri.Host};Port={uri.Port};Username={userInfo[0]};Password={userInfo[1]};Database={uri.AbsolutePath.TrimStart('/')};Ssl Mode=Require;Trust Server Certificate=true;";
            return new NpgsqlConnection(connectionString);
        });

        // Configure JWT authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Not map claim types.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!))
                };
            });

        // Register Resend
        services.AddOptions();
        services.AddHttpClient<ResendClient>();
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = configuration["Resend:ApiKey"]!;
        });
        services.AddTransient<IResend, ResendClient>();

        // Register repositories
        services.AddScoped<IProjectRepository, PostgresProjectRepository>();
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<IEmailVerificationRepository, PostgresEmailVerificationRepository>();

        // Register services
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();

        services.AddScoped<IEmailService>(sp => new ResendEmailService(
            sp.GetRequiredService<IResend>(),
            configuration["Email:From"]!,
            configuration["Email:BaseUrl"]!
        ));

        return services;
    }
} 