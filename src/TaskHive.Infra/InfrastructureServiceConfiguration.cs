using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TaskHive.Domain.Repositories;
using TaskHive.Infrastructure.Repositories;

namespace TaskHive.Infrastructure;

public static class InfrastructureServiceConfiguration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Configure connection pooling
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            // Set minimum pool size
            MinPoolSize = 1,
            // Set maximum pool size (adjust based on your needs)
            MaxPoolSize = 100,
            // Set connection lifetime in seconds (default is 300)
            ConnectionIdleLifetime = 300,
            // Set connection timeout in seconds
            Timeout = 30,
            // Enable connection multiplexing
            Multiplexing = true
        };

        // Register the connection as scoped
        services.AddScoped<NpgsqlConnection>(sp => new NpgsqlConnection(builder.ToString()));

        // Register repositories
        services.AddScoped<IUserRepository, PostgresUserRepository>();
        services.AddScoped<IProjectRepository, PostgresProjectRepository>();

        return services;
    }
} 